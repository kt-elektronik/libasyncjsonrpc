// MIT License
// Copyright (c) 2023 Dirk Kaar <dirk.kaar@samsongroup.com>
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Concurrent;

namespace AsyncRPCCore
{
    /// <summary>
    /// The ClientMuxer class contains a universal implementation of an RPC client that can send unconfirmed messages,
    /// request / reply remote procedure calls, and can receive unsolicited unconfirmed notification events. Its design
    /// is based off the JSON-RPC specification, but in all probability it can be used as base class in other
    /// similar designs. In particular, the encoding (marshalling) of the messages is not a concern of this class.
    /// Also the transport medium is only assumed to be a general stream. In cases where a datagram protocol is
    /// used, some design changes will have to be undertaken. In this version, the IUnmarshalMessageForId interface
    /// is responsible for detecting PDUs in the data stream.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to establish related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public abstract class ClientMuxer<IdType> : Muxer<IdType>, IClientMuxer<IdType> where IdType : struct
    {
        public ClientMuxer(int maxConcurrency = 5)
        {
            RpcConcurrencySema = new SemaphoreSlim(maxConcurrency);
        }

        protected SemaphoreSlim RpcConcurrencySema { get; }
        protected ConcurrentDictionary<IdType, TaskCompletionSource<IMuxerMessage<IdType>>> PendingRpcs { get; } = new();

        /// <summary>
        /// This function supports both request / reply messagging and unconfirmed messages. The Muxer class
        /// relies on message Ids of IdType to identify replies to requests there were sent by it. CallAsync
        /// expects the message parameter to be fully populated, that is, the Id must be in the MuxerMessage's
        /// Id property, and the same Id must be encoded in the raw message. Cancellation supports a
        /// roundtrip timeout for requests. If a reply is normally received, the CallAsync will return the
        /// remote server's reply that matched to this Id.
        /// </summary>
        /// <param name="message">The message to transmit. For request/reply messaging, the Id property must be
        /// set to the same value that is encoded in the RawMessage. For unconfirmed messages, the Id property
        /// must be null.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates a pending async call.
        /// If supported by the Stream instance, it can cancel request / reply messaging waiting
        /// for a reply after sending.
        /// </param>
        /// <returns>For unconfirmed messages, returns null. Also returns null if cancelled.</returns>
        public async Task<IMuxerMessage<IdType>?> CallAsync(IMuxerMessage<IdType> message, CancellationToken cancellation = default)
        {
            var replyTCS = new TaskCompletionSource<IMuxerMessage<IdType>>();
            if (message.IsTwoWayMessage)
            {
                lock (PendingRpcs)
                {
                    if (!PendingRpcs.TryAdd((IdType)message.Id!, replyTCS)) throw new InvalidOperationException("trying to add a message with an id that is in use");
                }
                await RpcConcurrencySema.WaitAsync(cancellation).ConfigureAwait(false);
            }
            await TxStream.WriteAsync(message.RawMessage, cancellation).ConfigureAwait(false);
            if (!message.IsTwoWayMessage) return null;
            try
            {
                return await replyTCS.Task.WaitAsync(cancellation).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                lock (PendingRpcs)
                {
                    if (PendingRpcs.TryRemove((IdType)message.Id!, out _))
                    {
                        replyTCS.SetCanceled(cancellation);
                        RpcConcurrencySema.Release();
                    }
                }
                return null;
            }
        }

        protected sealed override void OnRxTwoWayMessage(IMuxerMessage<IdType> message, CancellationToken cancellation = default)
        {
            lock (PendingRpcs)
            {
                if (PendingRpcs.TryRemove((IdType)message.Id!, out var replyTCS))
                {
                    replyTCS.SetResult(message);
                    RpcConcurrencySema.Release();
                }
            }
        }

        /// <summary>
        /// This is the message scheduler for replies and unsolicited unconfirmed notification
        /// messages. As an async implementation, it can be run in fire and forget mode, while
        /// still supporting cancellation through an optional cancellation token.
        /// </summary>
        /// <param name="cancellation">An optional cancellation token that allows stopping
        /// the message scheduler.</param>
        new public async Task RunRxAsync(CancellationToken cancellation = default)
        {
            PendingRpcs.Clear();

            try
            {
                await base.RunRxAsync(cancellation);
            }
            catch (OperationCanceledException)
            {
                int pendingCnt = 0;
                // If cancelled, consider all in-flight RPCs stale.
                lock (PendingRpcs)
                {
                    foreach (var replyTCS in PendingRpcs)
                    {
                        replyTCS.Value.SetCanceled(cancellation);
                        ++pendingCnt;
                    }
                    PendingRpcs.Clear();
                    RpcConcurrencySema.Release(pendingCnt);
                }
                throw;
            }
        }
    }
}

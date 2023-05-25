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

namespace AsyncRPCCore
{
    /// <summary>
    /// The Muxer class contains a universal implementation of an RPC client that can both send
    /// and receive request / reply RPC messages, as well as unsolicited unconfirmed notification events.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to establish related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public abstract class Muxer<IdType> : IMuxer<IdType> where IdType : struct
    {
        /// <summary>
        /// This must be set when a Muxer object is constructed.
        /// </summary>
        public required Stream RxStream { get; init; }
        public required Stream TxStream { get; init; }

        /// <summary>
        /// This must be set when a Muxer object is constructed.
        /// </summary>
        public required IUnmarshalMessageForId<IdType> UnmarshalMessageId { get; init; }

        /// <summary>
        /// Unsolicited unconfirmed notifications and messages
        /// are handled by this protected method.
        /// </summary>
        protected abstract void OnRxOneWayMessage(IMuxerMessage<IdType> message, CancellationToken cancellation = default);

        /// <summary>
        /// Incoming two-way request messages (server role) or response messages (client role)
        /// are handled by this protected method.
        /// </summary>
        protected abstract void OnRxTwoWayMessage(IMuxerMessage<IdType> message, CancellationToken cancellation = default);

        /// <summary>
        /// This is the message scheduler for replies and unsolicited unconfirmed notification
        /// messages. As an async implementation, it can be run in fire and forget mode, while
        /// still supporting cancellation through an optional cancellation token.
        /// </summary>
        /// <param name="cancellation">An optional cancellation token that allows stopping
        /// the message scheduler.</param>
        public async Task RunRxAsync(CancellationToken cancellation = default)
        {
            await foreach (var message in UnmarshalMessageId.ToMuxerMessagesAsync(RxStream, cancellation).WithCancellation(cancellation).ConfigureAwait(false))
            {
                if (message.IsTwoWayMessage)
                {
                    OnRxTwoWayMessage(message, cancellation);
                }
                else
                {
                    OnRxOneWayMessage(message, cancellation);
                }
            }
        }
    }
}

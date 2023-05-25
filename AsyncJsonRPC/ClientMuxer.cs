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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// The ClientMuxer class contains a JSON-RPC client implementation that can send notifications,
    /// request / reply remote procedure calls, and can receive unsolicited notification events.
    /// In its current incarnation, to a degree, it is specific to the KT-Elektronik UIOSim's
    /// JSON-RPC over Serial-To-USB API. More specifically, message bounds are not detected by
    /// counting curly braces, but are simply marked by CR/LF. The UIOSim never sends formatted
    /// JSON, but each message is a single line.
    /// </summary>
    public abstract class ClientMuxer : AsyncRPCCore.ClientMuxer<uint>, IClientMuxer
    {
        public ClientMuxer(int maxConcurrency = 5) : base(maxConcurrency)
        {
        }

        /// <summary>
        /// Each in-flight request/reply RPC in JSON-RPC needs a temporarily unique integer ID. A MsgIdSource
        /// instance assigned to this property provides such IDs.
        /// </summary>
        IUniqueMsgIdSource<uint> MsgIdSource { get; } = new MsgIdSource();

        /// <summary>
        /// This function wraps the base class CallAsync() specifically for JSON-RPC request/reply
        /// calls.
        /// </summary>
        /// <param name="message">The message to transmit. The requisite message Id for JSON-RPC
        /// remote procedure calls is set internally by this function.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates a pending async call.
        /// If supported by the Stream instance, it can cancel request / reply messaging waiting
        /// for a reply after sending.
        /// </param>
        /// <returns>For unconfirmed messages, returns null. Also returns null if cancelled.</returns>
        public async Task<(Response?, ErrorResponse?)> CallAsync<Response>(Datagram message, CancellationToken cancellation = default) where Response : class
        {
            uint msgId = MsgIdSource.Fetch();
            try
            {
                message = message with { Id = msgId };
                if (await CallAsync(new MuxerMessage(msgId, message.GetBytes()), cancellation).ConfigureAwait(false) is not MuxerMessage muxerReply) return (null, null);
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var msgDocument = muxerReply.MsgDocument;
                    if (muxerReply.IsErrorMessage)
                    {
                        var error = msgDocument?.RootElement.Deserialize<ErrorResponse>(options);
                        return (null, error);
                    }
                    else
                    {
                        var response = msgDocument?.RootElement.Deserialize<Response>(options);
                        return (response, null);
                    }
                }
                catch
                {
                    return (null, null);
                }
            }
            finally
            {
                MsgIdSource.Release(msgId);
            }
        }

        public IEnumerable<Task<(Response?, ErrorResponse?)>> CallAsync<Response>(IEnumerable<Datagram> messages,
            CancellationToken cancellation = default) where Response : class
        {
            foreach (var message in messages)
            {
                yield return CallAsync<Response>(message, cancellation);
            }
        }

        /// <summary>
        /// This function wraps the base class CallAsync() specifically for JSON-RPC notifications to server.
        /// </summary>
        /// <param name="message">The notification message to transmit.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates an async call awaiting the
        /// output stream.
        /// </param>
        public async Task NotifyAsync(Datagram message, CancellationToken cancellation = default)
        {
            // make double sure
            message = message with { Id = null };
            _ = await CallAsync(new MuxerMessage(message.Id, message.GetBytes()), cancellation).ConfigureAwait(false);
        }

        public async Task NotifyAsync(IEnumerable<Datagram> messages, CancellationToken cancellation = default)
        {
            foreach (var message in messages)
            {
                await NotifyAsync(message, cancellation).ConfigureAwait(false);
            }
        }
    }
}

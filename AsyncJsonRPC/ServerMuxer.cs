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

using AsyncRPCCore;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace AsyncJsonRPC
{
    /// <summary>
    /// The ServerMuxer class contains a JSON-RPC server implementation that can send notifications,
    /// receive request / reply remote procedure calls, as well as receive unsolicited notification events.
    /// In its current incarnation, to a degree, it is specific to the KT-Elektronik UIOSim's
    /// JSON-RPC over Serial-To-USB API. More specifically, message bounds are not detected by
    /// counting curly braces, but are simply marked by CR/LF. The UIOSim never sends formatted
    /// JSON, but each message is a single line.
    /// </summary>
    public abstract class ServerMuxer : ServerMuxer<uint>, IServerMuxer
    {
        public async Task NotifyAsync(Datagram message, CancellationToken cancellation = default)
        {
            // make double sure
            message = message with { Id = null };
            await base.NotifyAsync(message.GetBytes(), cancellation).ConfigureAwait(false);
        }

        public async Task NotifyAsync(IEnumerable<Datagram> messages, CancellationToken cancellation = default)
        {
            foreach (var message in messages)
            {
                await NotifyAsync(message, cancellation).ConfigureAwait(false);
            }
        }

        protected abstract Task<Datagram> OnRxRPCAsync(string method, JsonDocument message, CancellationToken cancellation = default);

        protected sealed override async void OnRxTwoWayMessage(IMuxerMessage<uint> message, CancellationToken cancellation = default)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var msgDocument = (message as MuxerMessage)?.MsgDocument;
            if (msgDocument is not null)
            {
                var hasMethod = msgDocument.RootElement.TryGetProperty("method", out var methodProperty);
                var method = methodProperty.GetString();
                hasMethod &= method is not null;
                var hasId = msgDocument.RootElement.TryGetProperty("id", out var msgIdProperty);
                hasId &= msgIdProperty.TryGetUInt32(out var msgId);

                if (hasMethod && hasId)
                {
                    var response = await OnRxRPCAsync(method!, msgDocument!, cancellation);
                    response = response with { Id = msgId };
                    try
                    {
                        await TxStream.WriteAsync(response.GetBytes(), cancellation).ConfigureAwait(false);
                    }
                    catch (IOException)
                    {
                        // inability to provided a reply implies inability to provide an error message,
                        // let connection management handle the underlying cause
                    }
                }
            }
        }
    }
}

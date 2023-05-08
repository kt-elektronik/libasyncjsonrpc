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
            try
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
                        await TxStream.WriteAsync(response.GetBytes(), cancellation).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

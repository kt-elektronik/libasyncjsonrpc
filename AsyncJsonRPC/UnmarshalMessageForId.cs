using AsyncRPCCore;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AsyncJsonRPC
{
    /// <summary>
    /// The ClientMuxer class delegates the splitting into JSON-RPC datagrams
    /// and the unmarshalling of message IDs from the received data stream
    /// to this implementation.
    /// </summary>
    public class UnmarshalMessageForId : IUnmarshalMessageForId
    {
        readonly byte CR = (byte)'\r';
        readonly byte LF = (byte)'\n';
        readonly byte NUL = (byte)'\0';

        /// <summary>
        /// Extract the JSON-RPC message ID from a raw message datagram that
        /// is a JSON-RPC response.
        /// </summary>
        /// <param name="rawMessage">A datagram that is a JSON-RPC response.</param>
        /// <returns>A MuxerMessage object that contains the rawMessage, plus the extracted
        /// JSON-RPC response ID, or null.</returns>
        public IMuxerMessage<uint>? ToMuxerMessage(byte[] rawMessage)
        {
            JsonDocument? msgDocument;
            try
            {
                msgDocument = JsonDocument.Parse(rawMessage);
            }
            catch
            {
                msgDocument = null;
            }
            if (msgDocument is null) return null;

            JsonElement id = msgDocument.RootElement.GetProperty("id");
            return new MuxerMessage(
                Id: id.ValueKind == JsonValueKind.Number ? id.GetUInt32() : null,
                RawMessage: rawMessage, MsgDocument: msgDocument)
            { IsErrorMessage = msgDocument.RootElement.TryGetProperty("error", out var _) };
        }

        /// <summary>
        /// Split up the raw stream into separate JSON-RPC datagram, viz.
        /// replies and notifications from a server.
        /// </summary>
        /// <param name="rawStream">A stream that supports async reading</param>
        /// <param name="cancellationToken">Optional, can be used to abort this async
        /// enumerable.</param>
        /// <returns>The next detected JSON-RPC datagram.</returns>
        public async IAsyncEnumerable<IMuxerMessage<uint>> ToMuxerMessagesAsync(
            Stream rawStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var rawMessage = new Queue<byte>(256);
            for (; ; )
            {
                using var ms = new MemoryStream();
                try
                {
                    var buffer = new byte[256];
                    var bytesRead = await rawStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        yield break;
                    }
                    ms.Write(buffer, 0, bytesRead);
                }
                catch (IOException)
                {
                    yield break;
                }
                // skip CRs and NULs
                foreach (var octet in ms.ToArray().SkipWhile((o) => o == CR || o == NUL))
                {
                    if (octet == LF)
                    {
                        if (rawMessage.Count != 0)
                        {
                            // LF is the message separator
                            var message = ToMuxerMessage(rawMessage.ToArray());
                            rawMessage.Clear();
                            if (message is not null)
                            {
                                yield return message;
                            }
                        }
                    }
                    else
                    {
                        rawMessage.Enqueue(octet);
                    }
                }
            }
        }
    }
}

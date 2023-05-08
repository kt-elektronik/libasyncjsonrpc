using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// This is the JSON-RPC data transfer object class for the common error response.
    /// </summary>
    /// <param name="Code">
    /// -32700	Parse error Invalid JSON was received by the server.<br/>
    ///     An error occurred on the server while parsing the JSON text.<br/>
    /// -32600	Invalid Request The JSON sent is not a valid Request object.<br/>
    /// -32601	Method not found    The method does not exist / is not available.<br/>
    /// -32602	Invalid params  Invalid method parameter(s).<br/>
    /// -32603	Internal error  Internal JSON-RPC error.<br/>
    /// -32000 to -32099	Server error    Reserved for implementation-defined server-errors.
    /// </param>
    public sealed record class ErrorResponse(short Code, string Message) : Datagram
    {
        public sealed record class ErrorField(short Code, string Message);

        [JsonIgnore]
        public short Code
        {
            get => Error.Code;
        }

        [JsonIgnore]
        public string Message
        {
            get => Error.Message;
        }

        [JsonInclude]
        [JsonPropertyName("error")]
        public ErrorField Error { get; private set; } = new ErrorField(Code, Message);

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

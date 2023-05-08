using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// This is the JSON-RPC data transfer object class for requests and notifications.
    /// It is not intended for verbatim use, but instead to be used in derived types that encapsule
    /// specific semantic notifications, and requests. For reply messages, please use the Datagram
    /// superclass.
    /// </summary>
    public abstract record class BasicMessage(string Method) : Datagram
    {
        [JsonPropertyName("method")]
        public string Method { get; } = Method;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

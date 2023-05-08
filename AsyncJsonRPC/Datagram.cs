using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// This is the JSON-RPC data transfer object class, common to notifications and request/reply RPCs.
    /// It is not intended for verbatim use, but instead to be used in derived types that encapsule
    /// specific semantic replies. For request and notification messages, please use the BasicMessage subclass.
    /// </summary>
    public record class Datagram([property: JsonPropertyName("id")] uint? Id = null)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public byte[] GetBytes()
        {
            return Encoding.ASCII.GetBytes($"\n{ToString()}\n");
        }

        protected static T JListGetter<T>(IList list, int index)
        {
            if (list[index]!.GetType() == typeof(T))
            {
                return (T)list[index]!;
            }
            var element = (JsonElement)list[index]!;
            if (element.ValueKind == JsonValueKind.String && Enum.TryParse(typeof(T), element.GetString(), out object? value))
            {
                return (T)value;
            }
            return element.Deserialize<T>()!;
        }

        protected static object JListSetter<T>(T value)
        {
            return (typeof(T).IsEnum ? value!.ToString() : value)!;
        }
    }
}

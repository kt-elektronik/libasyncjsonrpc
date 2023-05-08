using System.Text.Json;

namespace AsyncJsonRPC
{
    public record class MuxerMessage(uint? Id, byte[] RawMessage, JsonDocument? MsgDocument = default) : AsyncRPCCore.MuxerMessage<uint>(Id, RawMessage)
    {
    }
}

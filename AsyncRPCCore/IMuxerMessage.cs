namespace AsyncRPCCore
{
    public interface IMuxerMessage<IdType> where IdType : struct
    {
        IdType? Id { get; }
        bool IsErrorMessage { get; }
        bool IsTwoWayMessage { get; }
        byte[] RawMessage { get; }
    }
}

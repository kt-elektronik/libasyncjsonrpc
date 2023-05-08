namespace AsyncRPCCore
{
    /// <summary>
    /// This is the data transfer object class for AsyncRPCCore.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to link related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public record class MuxerMessage<IdType>(IdType? Id, byte[] RawMessage) : IMuxerMessage<IdType> where IdType : struct
    {
        /// <summary>
        /// If it is a oneway message or notification, this property is false.
        /// In case of RPC request messages, this property yields true.
        /// </summary>
        public bool IsTwoWayMessage => Id is not null;

        /// <summary>
        /// If it is an error response message, this property is true.
        /// In case of RPC request or oneway messages, this property yields false.
        /// </summary>
        public bool IsErrorMessage { get; init; } = false;
    }
}

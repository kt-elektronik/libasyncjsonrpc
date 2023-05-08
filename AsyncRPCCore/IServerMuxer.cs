namespace AsyncRPCCore
{
    /// <summary>
    /// This defines a universal interface of an RPC client that can send unconfirmed messages,
    /// request / reply remote procedure calls, and can receive unsolicited unconfirmed notification events. Its design
    /// is based off the JSON-RPC specification, but in all probability it can be used in other
    /// similar designs. In particular, the encoding (marshalling) of the messages is not a concern of this interface.
    /// Also the transport medium is only assumed to be a general stream. In cases where a datagram protocol is
    /// used, some design changes will have to be undertaken. In this version, the IUnmarshalMessageForId interface
    /// is responsible for detecting PDUs in the data stream.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to establish related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public interface IServerMuxer<IdType> : IMuxer<IdType> where IdType : struct
    {
        /// <summary>
        /// This function supports unconfirmed notification messages.
        /// </summary>
        /// <param name="rawMessage">The notification message to transmit.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates an async call awaiting the
        /// output stream.
        /// </param>
        Task NotifyAsync(byte[] rawMessage, CancellationToken cancellation = default);
    }
}
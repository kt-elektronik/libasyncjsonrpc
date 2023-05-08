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
    public interface IMuxer<IdType> where IdType : struct
    {
        /// <summary>
        ///  The stream on which the muxer receives messages
        /// </summary>
        Stream RxStream { get; init; }
        /// <summary>
        /// The stream on which the Muxer transmits messages
        /// </summary>
        Stream TxStream { get; init; }
        IUnmarshalMessageForId<IdType> UnmarshalMessageId { get; init; }

        /// <summary>
        /// This is the message scheduler for replies and unsolicited unconfirmed notification
        /// messages. As an async function, it can be run in fire and forget mode, while
        /// still supporting cancellation through an optional cancellation token.
        /// </summary>
        /// <param name="cancellation">An optional cancellation token that allows stopping
        /// the message scheduler.</param>
        Task RunRxAsync(CancellationToken cancellation = default);
    }
}
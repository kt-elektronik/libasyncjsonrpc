namespace AsyncRPCCore
{
    /// <summary>
    /// The ServerMuxer class contains a universal implementation of an RPC server that can receive unconfirmed messages,
    /// request / reply remote procedure calls, and can send unsolicited unconfirmed notification events. Its design
    /// is based off the JSON-RPC specification, but in all probability it can be used as base class in other
    /// similar designs. In particular, the encoding (marshalling) of the messages is not a concern of this class.
    /// Also the transport medium is only assumed to be a general stream. In cases where a datagram protocol is
    /// used, some design changes will have to be undertaken. In this version, the IUnmarshalMessageForId interface
    /// is responsible for detecting PDUs in the data stream.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to establish related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public abstract class ServerMuxer<IdType> : Muxer<IdType>, IServerMuxer<IdType> where IdType : struct
    {
        public async Task NotifyAsync(byte[] rawMessage, CancellationToken cancellation = default)
        {
            await TxStream.WriteAsync(rawMessage, cancellation).ConfigureAwait(false);
        }
    }
}
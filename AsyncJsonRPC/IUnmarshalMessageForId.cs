namespace AsyncJsonRPC
{
    /// <summary>
    /// The ClientMuxer class offloads parsing of the received data stream
    /// to an enumeration of MuxerMessages to implementations of this
    /// interface. In addition to that, ClientMuxer must be able to access
    /// message Ids in replies to request messages, this must also be
    /// performed by any implementation.
    /// </summary>
    public interface IUnmarshalMessageForId : AsyncRPCCore.IUnmarshalMessageForId<uint>
    {
    }
}

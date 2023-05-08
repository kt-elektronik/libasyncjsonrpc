namespace AsyncJsonRPC
{
    /// <summary>
    /// The IServerMuxer interface defines a JSON-RPC server implementation that can send notifications,
    /// receive request / reply remote procedure calls, as well as receive unsolicited notification events.
    /// </summary>
    public interface IServerMuxer : AsyncRPCCore.IServerMuxer<uint>
    {
        /// <summary>
        /// This function wraps the base interface NotifyAsync() specifically for JSON-RPC notifications to client.
        /// </summary>
        /// <param name="message">The notification message to transmit.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates an async call awaiting the
        /// output stream.
        /// </param>
        Task NotifyAsync(Datagram message, CancellationToken cancellation = default);
        Task NotifyAsync(IEnumerable<Datagram> messages, CancellationToken cancellation = default);
    }
}
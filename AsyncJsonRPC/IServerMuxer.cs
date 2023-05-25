// MIT License
// Copyright (c) 2023 Dirk Kaar <dirk.kaar@samsongroup.com>
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

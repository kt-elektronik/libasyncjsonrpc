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

namespace AsyncRPCCore
{
    /// <summary>
    /// The ClientMuxer class offloads parsing of the received data stream
    /// to an enumeration of MuxerMessages to implementations of this
    /// interface. In addition to that, ClientMuxer must be able to access
    /// message Ids in replies to request messages, this must also be
    /// performed by any implementation.
    /// </summary>
    /// <typeparam name="IdType">This is the type of the field contained in request and reply messages that
    /// is used to establish related messages. Without it, in batched or interleaved calling, it might
    /// be impossible to associate the right reply to the correct request in flight.</typeparam>
    public interface IUnmarshalMessageForId<IdType> where IdType : struct
    {
        /// <summary>
        /// Extract the message ID from a raw message datagram.
        /// </summary>
        /// <param name="rawMessage">A datagram as returned by ToMuxerMessagesAsync.</param>
        /// <returns>A MuxerMessage object that contains the rawMessage, plus the extracted
        /// message ID, if any, or null.</returns>
        public IMuxerMessage<IdType>? ToMuxerMessage(byte[] rawMessage);

        /// <summary>
        /// Split up the raw stream into separate datagrams, viz.
        /// replies and notifications from a server.
        /// </summary>
        /// <param name="rawStream">A stream that supports async reading</param>
        /// <param name="cancellationToken">Optional, can be used to abort this async
        /// enumerable.</param>
        /// <returns>The next detected datagram.</returns>
        public IAsyncEnumerable<IMuxerMessage<IdType>> ToMuxerMessagesAsync(
            Stream rawStream, CancellationToken cancellationToken = default);

    }
}

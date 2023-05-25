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
    public interface IClientMuxer<IdType> : IMuxer<IdType> where IdType : struct
    {
        /// <summary>
        /// This function supports both request / reply messagging and unconfirmed messages. The IClientMuxer interface
        /// relies on message Ids of IdType to identify replies to requests there were sent by it. CallAsync
        /// expects the message parameter to be fully populated, that is, the Id must be in the MuxerMessage's
        /// Id property, and the same Id must be encoded in the raw message. Cancellation supports a
        /// roundtrip timeout for requests. If a reply is normally received, the CallAsync will return the
        /// remote server's reply that matched to this Id.
        /// </summary>
        /// <param name="message">The message to transmit. For request/reply messaging, the Id property must be
        /// set to the same value that is encoded in the RawMessage. For unconfirmed messages, the Id property
        /// must be null.
        /// </param>
        /// <param name="cancellation">This cancellation token terminates a pending async call.
        /// If supported by the Stream instance, it can cancel request / reply messaging waiting
        /// for a reply after sending.
        /// </param>
        /// <returns>For unconfirmed messages, returns null. Also returns null if cancelled.</returns>
        Task<IMuxerMessage<IdType>?> CallAsync(IMuxerMessage<IdType> message, CancellationToken cancellation = default);
    }
}

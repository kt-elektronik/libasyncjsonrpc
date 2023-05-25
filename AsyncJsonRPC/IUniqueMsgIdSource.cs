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
    /// An interface for temporarily unique identifers, like message sequence
    /// numbers, UUIDs, or JSON-RPC Id field values.
    /// </summary>
    /// <typeparam name="IdType"></typeparam>
    internal interface IUniqueMsgIdSource<IdType> where IdType : struct
    {
        /// <summary>
        /// Acquire a unique message identifier. The same identifier may have
        /// been issued before, and may be issued again later, but not before
        /// this one is released by a call to the Release() method.
        /// </summary>
        /// <returns>A temporarily unique Id.</returns>
        IdType Fetch();
        /// <summary>
        /// Release is only valid to call for an Id previously aquired from a
        /// call of the Fetch() method, that has not yet been released in the
        /// meantime.
        /// </summary>
        /// <param name="id">A message identifier from a previous call to Fetch().</param>
        void Release(IdType id);
    }
}

﻿// MIT License
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

using System.Numerics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestProject1")]
namespace AsyncJsonRPC
{
    /// <summary>
    /// An implementation of IUniqueMsgIdSource.
    /// Creates temporarily unique JSON-RPC Id field values.
    /// </summary>
    /// <typeparam name="IdType"></typeparam>
    internal class MsgIdSource : IUniqueMsgIdSource<uint>
    {
        const uint BITCNT_UINT = 8 * sizeof(uint);

        List<uint> Stack { get; } = new List<uint>();

        /// <summary>
        /// Acquire a unique message identifier. The same identifier may have
        /// been issued before, and may be issued again later, but not before
        /// this one is released by a call to the Release() method.
        /// </summary>
        /// <returns>A temporarily unique Id.</returns>
        public uint Fetch()
        {
            int index = 0;
            uint? id;
            lock (Stack)
            {
                foreach (var item in Stack)
                {
                    // Least Significant Zero bit
                    int lsz = BitOperations.TrailingZeroCount(~item);
                    if (lsz < BITCNT_UINT)
                    {
                        id = (uint?)(index * BITCNT_UINT + lsz + 1);
                        Stack[index] |= 1U << lsz;
                        return (uint)id;
                    }
                    ++index;
                }
                Stack.Add(1);
            }
            id = (uint?)(index * BITCNT_UINT + 1);
            return (uint)id;
        }

        /// <summary>
        /// Release is only valid to call for an Id previously acquired from a
        /// call of the Fetch() method, that has not yet been released in the
        /// meantime.
        /// </summary>
        /// <param name="id">A message identifier from a previous call to Fetch().</param>
        public void Release(uint id)
        {
            int index = (int)((id - 1) / BITCNT_UINT);
            byte bit = (byte)((id - 1) - index * BITCNT_UINT);
            lock (Stack)
            {
                var item = Stack[index] & ~((uint)1 << bit);
                if (item == 0)
                {
                    var end = index + 1;
                    while (end < Stack.Count)
                    {
                        if (Stack[end] != 0) break;
                        ++end;
                    }
                    if (Stack.Count == end)
                    {
                        while (index > 0 && Stack[index - 1] == 0) --index;
                        Stack.RemoveRange(index, end - index);
                        return;
                    }
                }
                Stack[index] = item;
            }
        }
    }
}

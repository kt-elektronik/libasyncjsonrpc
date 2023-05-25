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

using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// This is the JSON-RPC data transfer object class, common to notifications and request/reply RPCs.
    /// It is not intended for verbatim use, but instead to be used in derived types that encapsule
    /// specific semantic replies. For request and notification messages, please use the BasicMessage subclass.
    /// </summary>
    public record class Datagram([property: JsonPropertyName("id")] uint? Id = null)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public byte[] GetBytes()
        {
            return Encoding.ASCII.GetBytes($"\n{ToString()}\n");
        }

        protected static T JListGetter<T>(IList list, int index)
        {
            if (list[index]!.GetType() == typeof(T))
            {
                return (T)list[index]!;
            }
            var element = (JsonElement)list[index]!;
            if (element.ValueKind == JsonValueKind.String && Enum.TryParse(typeof(T), element.GetString(), out object? value))
            {
                return (T)value;
            }
            return element.Deserialize<T>()!;
        }

        protected static object JListSetter<T>(T value)
        {
            return (typeof(T).IsEnum ? value!.ToString() : value)!;
        }
    }
}

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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncJsonRPC
{
    /// <summary>
    /// This is the JSON-RPC data transfer object class for the common error response.
    /// </summary>
    /// <param name="Code">
    /// -32700	Parse error Invalid JSON was received by the server.<br/>
    ///     An error occurred on the server while parsing the JSON text.<br/>
    /// -32600	Invalid Request The JSON sent is not a valid Request object.<br/>
    /// -32601	Method not found    The method does not exist / is not available.<br/>
    /// -32602	Invalid params  Invalid method parameter(s).<br/>
    /// -32603	Internal error  Internal JSON-RPC error.<br/>
    /// -32000 to -32099	Server error    Reserved for implementation-defined server-errors.
    /// </param>
    public sealed record class ErrorResponse(short Code, string Message) : Datagram
    {
        public sealed record class ErrorField([property: JsonPropertyName("code")] short Code, [property: JsonPropertyName("message")] string Message);

        [JsonIgnore]
        public short Code
        {
            get => Error.Code;
        }

        [JsonIgnore]
        public string Message
        {
            get => Error.Message;
        }

        [JsonInclude]
        [JsonPropertyName("error")]
        public ErrorField Error { get; private set; } = new ErrorField(Code, Message);

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

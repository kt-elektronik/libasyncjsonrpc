﻿using AsyncJsonRPC;
using AsyncJsonRPCExample;
using AsyncJsonRPCExample.Notifications;
using AsyncRPCCore;
using System.IO.Pipes;

var clientTxStream = new AnonymousPipeServerStream(PipeDirection.Out);
var serverRxStream = new AnonymousPipeClientStream(PipeDirection.In, clientTxStream.GetClientHandleAsString());
var serverTxStream = new AnonymousPipeServerStream(PipeDirection.Out);
var clientRxStream = new AnonymousPipeClientStream(PipeDirection.In, serverTxStream.GetClientHandleAsString());
var client = new AsyncJsonRPCExample.ClientMuxer() { RxStream = clientRxStream, TxStream = clientTxStream, UnmarshalMessageId = new UnmarshalMessageForId() };
var server = new AsyncJsonRPCExample.ServerMuxer() { RxStream = serverRxStream, TxStream = serverTxStream, UnmarshalMessageId = new UnmarshalMessageForId() };

_ = server.RunRxAsync().ConfigureAwait(false);

var finished = new SemaphoreSlim(0);
var finishedTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100));
finishedTimer.Elapsed += (sender, e) => finished.Release();

var fibonacciGenerator = new EventGenerator<FibonacciNotification>();
client.FibonacciEvent += fibonacciGenerator.OnEvent;
_ = Task.Run(async () =>
{
    await foreach (var e in fibonacciGenerator.OnEventAsync())
    {
        finishedTimer.Stop();
        finishedTimer.Start();
        Console.WriteLine($"Got FibonacciNotification.Value = {e.Value}");
    }
    client.FibonacciEvent -= fibonacciGenerator.OnEvent;
});

_ = client.RunRxAsync().ConfigureAwait(false);

var (response, error) = await client.CallAsync(new SubscribeFibonacci(93));
if (error is not null)
{
    Console.WriteLine("RPC SubscribeFibonacci(10) has failed.");
    Environment.Exit(1);
}
Console.WriteLine($"RPC SubscribeFibonacci(10) Response = {response!.Depth}");
finishedTimer.Start();

await finished.WaitAsync();

namespace AsyncJsonRPCExample
{
    using AsyncJsonRPC;
    using System.Collections;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed record class SubscribeFibonacci(int Depth) : AsyncJsonRPC.BasicMessage(MethodName)
    {
        public const string MethodName = "AsyncJsonRPCExample.SubscribeFibonacci";

        [JsonIgnore]
        public int Depth
        {
            get => JListGetter<int>(Params, 0);
        }

        [JsonInclude]
        [JsonPropertyName("params")]
        public ArrayList Params { get; private set; } = new ArrayList() { JListSetter(Depth) };
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public static class SubscribeFibonacciExtension
    {
        public static async Task<(SubscribeFibonacciResponse?, ErrorResponse?)> CallAsync(
            this ClientMuxer muxer, SubscribeFibonacci message, CancellationToken cancellation = default)
        {
            return await muxer.CallAsync<SubscribeFibonacciResponse>(message, cancellation);
        }

        public static IEnumerable<Task<(SubscribeFibonacciResponse?, ErrorResponse?)>> CallAsync(this ClientMuxer muxer,
            IEnumerable<SubscribeFibonacci> messages, CancellationToken cancellation = default)
        {
            foreach (var result in muxer.CallAsync<SubscribeFibonacciResponse>(messages, cancellation))
            {
                yield return result;
            }
        }
    }

    public sealed class ServerMuxer : AsyncJsonRPC.ServerMuxer
    {
        public ServerMuxer() { }

        protected override Task<Datagram> OnRxRPCAsync(string method, JsonDocument message, CancellationToken cancellation = default)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            Datagram? response = null;
            switch (method)
            {
                case SubscribeFibonacci.MethodName:
                    var subFibonacci = message.RootElement.Deserialize<SubscribeFibonacci>(options)!;
                    if (subFibonacci.Depth < 94)
                    {
                        response = new SubscribeFibonacciResponse(subFibonacci.Depth);
                        // fire and forget
                        var muxer = this;
                        _ = Task.Run(async () =>
                        {
                            async IAsyncEnumerable<ulong> Fib()
                            {
                                ulong a = 0;
                                ulong b = 1;
                                for (var j = 0; j <= subFibonacci.Depth; ++j)
                                {
                                    yield return a;
                                    var c = a + b;
                                    b = a;
                                    a = c;
                                    await Task.Delay(10, cancellation);
                                }
                                yield break;
                            };
                            await foreach (var f in Fib()) { await NotifyAsync(new Notifications.FibonacciNotification(f), cancellation); }
                        }, cancellation);
                    }
                    break;
            }
            return Task.FromResult(response ?? new ErrorResponse(-32601, String.Empty));
        }

        protected override void OnRxOneWayMessage(IMuxerMessage<uint> message, CancellationToken cancellation = default) { }

    }

    public sealed record class SubscribeFibonacciResponse(int Depth) : AsyncJsonRPC.Datagram
    {
        [JsonIgnore]
        public int Depth
        {
            get => JListGetter<int>(Results, 0);
        }

        [JsonInclude]
        [JsonPropertyName("result")]
        public int[] Results { get; private set; } = new int[] { Depth };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    namespace Notifications
    {
        public sealed record class FibonacciNotification(ulong Value) :
                AsyncJsonRPC.BasicMessage(MethodName)
        {
            public const string MethodName = "AsyncJsonRPCExample.Notifications.Fibonacci";

            [JsonIgnore]
            public ulong Value
            {
                get => JListGetter<ulong>(Params, 0);
            }

            [JsonInclude]
            [JsonPropertyName("params")]
            public ArrayList Params { get; private set; } = new ArrayList() { JListSetter(Value) };

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }
    }

    public class ClientMuxer : AsyncJsonRPC.ClientMuxer, IClientMuxer
    {
        public ClientMuxer(int maxConcurrency = 5) : base(maxConcurrency) { }

        protected override void OnRxOneWayMessage(IMuxerMessage<uint> message, CancellationToken cancellation = default)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var msgDocument = (message as MuxerMessage)?.MsgDocument;
                if (msgDocument is not null)
                {
                    var hasMethod = msgDocument.RootElement.TryGetProperty("method", out var methodProperty);
                    var method = methodProperty.GetString();
                    hasMethod &= method is not null;

                    if (hasMethod)
                    {
                        switch (method)
                        {
                            case "AsyncJsonRPCExample.Notifications.Fibonacci":
                                FibonacciEvent?.Invoke(this,
                                    msgDocument?.RootElement.Deserialize<Notifications.FibonacciNotification>(options)!);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch
            {
                // silently discard
            }
        }

        public event EventHandler<AsyncJsonRPCExample.Notifications.FibonacciNotification>? FibonacciEvent;
    }
}

﻿namespace AsyncRPCCore
{
    public sealed record class EventGenerator<T>(CancellationToken Token = default)
    {
        private TaskCompletionSource<T> tcs = new();
        private readonly SemaphoreSlim next = new(1, 1);

        public async void OnEvent(object? _, T e)
        {
            await next.WaitAsync(Token);
            tcs.SetResult(e);
        }
        public async IAsyncEnumerable<T> OnEventAsync()
        {
            while (!Token.IsCancellationRequested)
            {
                var value = await tcs.Task;
                tcs = new TaskCompletionSource<T> { };
                next.Release();
                yield return value;
            }
        }
    }
}
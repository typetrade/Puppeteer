// <copyright file="TaskQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Puppeteer.Helpers
{
    public sealed class TaskQueue : IDisposable, IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly AsyncLocal<bool> _held = new();
        private int _disposed;

        public TaskQueue() => _semaphore = new SemaphoreSlim(1);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            if (!_held.Value)
            {
                _semaphore.Wait();
            }

            _semaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            if (!_held.Value)
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
            }

            _semaphore.Dispose();
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _held.Value = true;
                return await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                TryRelease(_semaphore);
                _held.Value = false;
            }
        }

        public async Task Enqueue(Func<Task> taskGenerator)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _held.Value = true;
                await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                TryRelease(_semaphore);
                _held.Value = false;
            }
        }

        private void TryRelease(SemaphoreSlim semaphore)
        {
            try
            {
                semaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // If semaphore has already been disposed, then Release() will fail
                // but we can safely ignore it
            }
        }
    }
}

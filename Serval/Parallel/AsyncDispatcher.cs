using System;
using System.Threading;

namespace Serval.Parallel {
    public class AsyncDispatcher : IDispatcher {
        private readonly SemaphoreSlim semaphore;

        public AsyncDispatcher(int parallelism = 1) {
            if(parallelism < 1)
                throw new ArgumentException(nameof(parallelism) + " must be a positive integer.");
            semaphore = new SemaphoreSlim(parallelism);
        }

        public async void Invoke(Action action) {
            if(action == null)
                throw new ArgumentNullException(nameof(action));
            await semaphore.WaitAsync();
            try {
                action();
            } finally {
                semaphore.Release();
            }
        }
    }
}

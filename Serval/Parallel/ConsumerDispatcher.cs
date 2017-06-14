using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Serval.Parallel {
    public sealed class ConsumerDispatcher : IDispatcher {
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public ConsumerDispatcher(int parallelism = 1) {
            if(parallelism < 1)
                throw new ArgumentException(nameof(parallelism) + " must be a positive integer.");
            for(int i = 0; i < parallelism; i++)
                Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
        }

        private void Consume() {
            while(!_cancellation.Token.IsCancellationRequested)
                _queue.Take().Invoke();
        }

        public void Invoke(Action action) {
            _queue.Add(action ?? throw new ArgumentNullException(nameof(action)));
        }
    }
}

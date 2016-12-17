using System;
using System.Threading;
using System.Collections.Concurrent;

namespace Serval.Connections.Pooling {
    public abstract class GrowingBlockingCollection<T> : BlockingCollection<T> {
        private object glock = new object();

        public TimeSpan Timeout {
            get;
        }

        public int Increment {
            get;
        }

        public GrowingBlockingCollection(TimeSpan timeout = default(TimeSpan), int increment = 0) : base() {
            Timeout = timeout;
            Increment = increment;
        }

        public GrowingBlockingCollection(int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 0) : base(boundedCapacity) {
            Timeout = timeout;
            Increment = increment;
        }

        public GrowingBlockingCollection(IProducerConsumerCollection<T> collection, TimeSpan timeout = default(TimeSpan), int increment = 0) : base(collection) {
            Timeout = timeout;
            Increment = increment;
        }

        public GrowingBlockingCollection(IProducerConsumerCollection<T> collection, int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 0) : base(collection, boundedCapacity) {
            Timeout = timeout;
            Increment = increment;
        }

        public T TakeOrGrow() {
            T item;
            while(true) {
                if(Increment <= 0) {
                    return Take();
                }else{
                    if(TryTake(out item, Timeout)) {
                        return item;
                    } else {
                        if(Monitor.TryEnter(glock)) {
                            item = Grow();
                            Monitor.Exit(glock);
                            return item;
                        }
                    }
                }
            }
        }

        protected abstract T Grow();
    }
}


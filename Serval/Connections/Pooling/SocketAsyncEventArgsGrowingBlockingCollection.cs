using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Serval.Connections.Pooling {
    public class SocketAsyncEventArgsGrowingBlockingCollection : GrowingBlockingCollection<SocketAsyncEventArgs> {
        private EventHandler<SocketAsyncEventArgs> handler;

        public SocketAsyncEventArgsGrowingBlockingCollection(EventHandler<SocketAsyncEventArgs> handler, TimeSpan timeout = default(TimeSpan), int increment = 10, int initial = 50) : base(timeout, increment) {
            this.handler = handler;
            Grow(initial);
        }

        public SocketAsyncEventArgsGrowingBlockingCollection(EventHandler<SocketAsyncEventArgs> handler, int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 10, int initial = 50) : base(boundedCapacity, timeout, increment) {
            this.handler = handler;
            Grow(initial);
        }

        public SocketAsyncEventArgsGrowingBlockingCollection(EventHandler<SocketAsyncEventArgs> handler, IProducerConsumerCollection<SocketAsyncEventArgs> collection, TimeSpan timeout = default(TimeSpan), int increment = 10, int initial = 50) : base(collection, timeout, increment) {
            this.handler = handler;
            Grow(initial);
        }

        public SocketAsyncEventArgsGrowingBlockingCollection(EventHandler<SocketAsyncEventArgs> handler, IProducerConsumerCollection<SocketAsyncEventArgs> collection, int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 10, int initial = 50) : base(collection, boundedCapacity, timeout, increment) {
            this.handler = handler;
            Grow(initial);
        }

        protected override SocketAsyncEventArgs Grow() {
            Grow(Increment - 1);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += handler;
            args.SetBuffer(Utils.NO_BUFFER, 0, 0);
            return args;
        }

        private void Grow(int amount) {
            for(int i = 0; i < amount; i++) {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += handler;
                args.SetBuffer(Utils.NO_BUFFER, 0, 0);
                Add(args);
            }
        }
    }
}


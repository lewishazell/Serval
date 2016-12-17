using System;
using System.Collections.Concurrent;

namespace Serval.Connections.Pooling {
    public class ByteArrayGrowingBlockingCollection : GrowingBlockingCollection<byte[]> {
        public int BufferSize {
            get;
        }

        public ByteArrayGrowingBlockingCollection(TimeSpan timeout = default(TimeSpan), int increment = 0, int bufferSize = 1024, int initial = 1024) : base(timeout, increment) {
            BufferSize = bufferSize;
            Grow(initial);
        }

        public ByteArrayGrowingBlockingCollection(int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 0, int bufferSize = 1024, int initial = 1024) : base(boundedCapacity, timeout, increment) {
            BufferSize = bufferSize;
            Grow(initial);
        }

        public ByteArrayGrowingBlockingCollection(IProducerConsumerCollection<byte[]> collection, TimeSpan timeout = default(TimeSpan), int increment = 0, int bufferSize = 1024, int initial = 1024) : base(collection, timeout, increment) {
            BufferSize = bufferSize;
            Grow(initial);
        }

        public ByteArrayGrowingBlockingCollection(IProducerConsumerCollection<byte[]> collection, int boundedCapacity, TimeSpan timeout = default(TimeSpan), int increment = 0, int bufferSize = 1024, int initial = 1024) : base(collection, boundedCapacity, timeout, increment) {
            BufferSize = bufferSize;
            Grow(initial);
        }

        protected override byte[] Grow() {
            Grow(Increment - 1);
            return new byte[BufferSize];
        }

        private void Grow(int amount) {
            for(int i = 0; i < amount; i++)
                Add(new byte[BufferSize]);
        }
    }
}


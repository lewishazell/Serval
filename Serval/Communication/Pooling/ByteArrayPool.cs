using System;

namespace Serval.Communication.Pooling {
    public sealed class ByteArrayPool : FixedBlockingPool<byte[]> {
        public int BufferSize {
            get;
        }

        public ByteArrayPool(int size, int bufferSize) : base() {
            if(size < 0)
                throw new ArgumentException("Argument " + nameof(size) + " must not be negative.");
            if(bufferSize < 0)
                throw new ArgumentException("Argument " + nameof(bufferSize) + " must not be negative.");
            BufferSize = bufferSize;
            byte[][] generated = Generate(size);
            foreach(byte[] bytes in generated)
                Pool.Add(bytes);
        }

        public new void Return(byte[] item) {
            Array.Clear(item, 0, item.Length);
            base.Return(item);
        }

        protected override byte[][] Generate(int amount) {
            byte[][] bytes = new byte[amount][];
            for(int i = 0; i < bytes.Length; i++) {
                bytes[i] = new byte[BufferSize];
            }
            return bytes;
        }
    }
}


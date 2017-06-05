using System;

namespace Serval.Communication.Pooling {
    internal class ByteArrayPool : FixedBlockingPool<byte[]> {
        internal static readonly byte[] NO_BUFFER = new byte[0];

        internal int BufferSize {
            get;
        }

        internal ByteArrayPool(int size, int bufferSize) : base() {
            BufferSize = bufferSize;
            byte[][] generated = Generate(size);
            foreach(byte[] bytes in generated)
                Pool.Add(bytes);
        }

        internal new void Return(byte[] item) {
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


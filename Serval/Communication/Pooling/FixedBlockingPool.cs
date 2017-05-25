using System;
using System.Threading;
using System.Collections.Concurrent;

namespace Serval.Communication.Pooling {
    internal abstract class FixedBlockingPool<T> : IPool<T> {
        protected BlockingCollection<T> Pool {
            get;
        } = new BlockingCollection<T>();

        protected FixedBlockingPool() { }

        protected FixedBlockingPool(int size) {
            T[] items = Generate(size);
            foreach(T item in items)
                Pool.Add(item);
        }

        public T Retreive() {
            return Pool.Take();
        }

        public void Return(T item) {
            Pool.Add(item);
        }

        protected abstract T[] Generate(int amount);
    }
}


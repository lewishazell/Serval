using System;
using System.Collections.Concurrent;

namespace Serval {
    abstract class Pool<T1, T2> {
        protected BlockingCollection<T1> Items {
            get;
        } = new BlockingCollection<T1>();
        private BlockingCollection<T2> Consumers {
            get;
        } = new BlockingCollection<T2>();

        abstract void Grow(int amount);

        abstract void Allocate(T2 cosumer);

        abstract void Return(T1 item);
    }
}


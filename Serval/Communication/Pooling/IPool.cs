using System;

namespace Serval.Communication.Pooling {
    public interface IPool<T> {
        T Retreive();

        void Return(T item);
    }
}


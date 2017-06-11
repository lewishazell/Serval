using System;

namespace Serval.Parallel {
    public interface IDispatcher {
        void Invoke(Action action);
    }
}

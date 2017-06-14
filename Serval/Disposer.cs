using System;

namespace Serval {
    internal struct Disposer : IDisposable {
        private readonly IDisposable _disposable;

        public Disposer(IDisposable disposable) {
            _disposable = disposable;
        }

        public void Dispose() {
            _disposable.Dispose();
        }
    }
}

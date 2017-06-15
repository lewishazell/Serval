using System;

using Serval.Parallel;

namespace Serval.Transceive.Dispatching {
    public abstract class DispatchingTransmitter<TRecipient, TForward> : ForwardingTransmitter<TRecipient, TForward, TForward> {
        private readonly IDispatcher _dispatcher;

        public DispatchingTransmitter(ITransmitter<TRecipient, TForward> child, IDispatcher dispatcher) : base(child) {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public override void SendAsync(TRecipient connection, TForward obj) {
            _dispatcher.Invoke(() => Forward(connection, obj));
        }
    }
}

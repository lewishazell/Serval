using System;
using Serval.Communication.Tcp;
using Serval.Transceive.Tcp;
using Serval.Parallel;

namespace Serval.Transceive.Dispatching.Tcp {
    public sealed class TcpDispatchingTransmitter<TForward> : DispatchingTransmitter<Connection, TForward>, ITcpTransmitter<TForward> {
        private readonly IDispatcher _dispatcher;
        private readonly ITcpTransmitter<TForward> _child;

        public TcpDispatchingTransmitter(ITcpTransmitter<TForward> child, IDispatcher dispatcher) : base(child, dispatcher) {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public void DisconnectAsync(Connection connection) {
            _dispatcher.Invoke(() => _child.DisconnectAsync(connection));
        }
    }
}

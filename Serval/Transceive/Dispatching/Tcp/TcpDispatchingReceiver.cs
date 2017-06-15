using System;
using Serval.Parallel;
using Serval.Communication.Tcp;
using Serval.Transceive.Tcp;

namespace Serval.Transceive.Dispatching.Tcp {
    public sealed class TcpDispatchingReceiver<TForward> : DispatchingReceiver<Connection, TForward>, ITcpReceiver<TForward> {
        private readonly IDispatcher _dispatcher;
        private readonly ITcpReceiver<TForward> _child;

        public TcpDispatchingReceiver(ITcpReceiver<TForward> child, IDispatcher dispatcher) : base(child, dispatcher)  {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public void Connected(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => _child.Connected(connection));
        }

        public void Disconnected(Connection connection, IDisposable disposer) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => _child.Disconnected(connection, disposer));
        }
    }
}

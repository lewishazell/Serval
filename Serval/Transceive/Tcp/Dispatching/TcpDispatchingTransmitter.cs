using System;
using Serval.Communication.Tcp;
using Serval.Parallel;

namespace Serval.Transceive.Tcp.Dispatching {
    public sealed class TcpDispatchingTransmitter<TForward> : TcpForwardingTransmitter<TForward, TForward> {
        private readonly IDispatcher _dispatcher;

        public TcpDispatchingTransmitter(ITcpTransmitter<TForward> child, IDispatcher dispatcher) : base(child) {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public override void DisconnectAsync(Connection connection) {
            _dispatcher.Invoke(() => base.DisconnectAsync(connection));
        }

        public override void SendAsync(Connection connection, TForward obj) {
            _dispatcher.Invoke(() => Forward(connection, obj));
        }
    }
}

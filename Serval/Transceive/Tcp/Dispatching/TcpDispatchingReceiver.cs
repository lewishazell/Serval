using System;
using Serval.Parallel;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp.Dispatching {
    public sealed class TcpDispatchingReceiver<TForward> : TcpForwardingReceiver<TForward, TForward> {
        private readonly IDispatcher _dispatcher;

        public TcpDispatchingReceiver(ITcpReceiver<TForward> child, IDispatcher dispatcher) : base(child)  {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public override void Connected(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => base.Connected(connection));
        }

        public override void Received(Connection connection, TForward obj) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => Forward(connection, obj));
        }

        public override void Sent(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => base.Sent(connection));
        }

        public override void Disconnected(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _dispatcher.Invoke(() => base.Disconnected(connection));
        }

        public override void Caught(Connection connection, Exception exception) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            if(exception == null)
                throw new ArgumentNullException(nameof(exception));
            _dispatcher.Invoke(() => base.Caught(connection, exception));
        }
    }
}

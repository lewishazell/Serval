using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp {
    public abstract class TcpForwardingReceiver<TInput, TOutput> : ForwardingReceiver<Connection, TInput, TOutput>, ITcpReceiver<TInput> {
        private readonly ITcpReceiver<TOutput> _child;

        public TcpForwardingReceiver(ITcpReceiver<TOutput> child) : base(child) {
        }

        public virtual void Connected(Connection connection) {
            _child.Connected(connection ?? throw new ArgumentNullException(nameof(connection)));
        }

        public virtual void Disconnected(Connection connection, IDisposable disposer) {
            _child.Disconnected(connection ?? throw new ArgumentNullException(nameof(connection)), disposer);
        }
    }
}

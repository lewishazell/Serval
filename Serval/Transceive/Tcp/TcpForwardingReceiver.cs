using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp {
    public abstract class TcpForwardingReceiver<TInput, TOutput> : ITcpReceiver<TInput> {
        private ITcpReceiver<TOutput> Child { get;  }

        public TcpForwardingReceiver(ITcpReceiver<TOutput> child) {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public virtual void Connected(Connection connection) {
            Child.Connected(connection ?? throw new ArgumentNullException(nameof(connection)));
        }

        public abstract void Received(Connection connection, TInput obj);

        protected void Forward(Connection connection, TOutput obj) {
            Child.Received(connection ?? throw new ArgumentNullException(nameof(connection)), obj);
        }

        public virtual void Sent(Connection connection) {
            Child.Sent(connection ?? throw new ArgumentNullException(nameof(connection)));
        }

        public virtual void Disconnected(Connection connection, IDisposable disposer) {
            Child.Disconnected(connection ?? throw new ArgumentNullException(nameof(connection)), disposer);
        }

        public virtual void Caught(Connection connection, Exception exception) {
            Child.Caught(connection ?? throw new ArgumentNullException(nameof(connection)), exception ?? throw new ArgumentNullException(nameof(exception)));
        }
    }
}

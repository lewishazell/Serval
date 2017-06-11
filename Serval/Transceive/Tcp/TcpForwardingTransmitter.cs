using System;
using Serval.Communication.Tcp;
using Serval.Channels.Tcp;

namespace Serval.Transceive.Tcp {
    public abstract class TcpForwardingTransmitter<TInput, TOutput> : ITcpTransmitter<TInput> {
        private ITcpTransmitter<TOutput> Child { get; }

        public TcpForwardingTransmitter(ITcpTransmitter<TOutput> child) {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public abstract void SendAsync(Connection connection, TInput obj);

        protected void Forward(Connection connection, TOutput obj) {
            Child.SendAsync(connection ?? throw new ArgumentNullException(nameof(connection)), obj);
        }

        public virtual void DisconnectAsync(Connection connection) {
            Child.DisconnectAsync(connection ?? throw new ArgumentNullException(nameof(connection)));
        }
    }
}

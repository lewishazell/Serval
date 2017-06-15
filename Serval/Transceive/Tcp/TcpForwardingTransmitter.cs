using System;
using Serval.Communication.Tcp;
using Serval.Channels.Tcp;

namespace Serval.Transceive.Tcp {
    public abstract class TcpForwardingTransmitter<TInput, TOutput> : ForwardingTransmitter<Connection, TInput, TOutput>, ITcpTransmitter<TInput> {
        private readonly ITcpTransmitter<TOutput> _child;

        public TcpForwardingTransmitter(ITcpTransmitter<TOutput> child) : base(child) {
        }

        public virtual void DisconnectAsync(Connection connection) {
            _child.DisconnectAsync(connection ?? throw new ArgumentNullException(nameof(connection)));
        }
    }
}

using System;

using Serval.Communication.Tcp;
using Serval.Transceive.Tcp;

namespace Serval.Transceive.Encoding.Tcp {
    public sealed class TcpDecodingReceiver : DecodingReceiver<Connection>, ITcpReceiver<byte[]> {
        private ITcpReceiver<string> _child { get; }

        public TcpDecodingReceiver(ITcpReceiver<string> child, System.Text.Encoding encoding) : base(child, encoding) {
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public void Connected(Connection connection) {
            _child.Connected(connection ?? throw new ArgumentNullException(nameof(connection)));
        }

        public void Disconnected(Connection connection, IDisposable disposer) {
            _child.Disconnected(connection ?? throw new ArgumentNullException(nameof(connection)), disposer ?? throw new ArgumentNullException(nameof(disposer)));
        }
    }
}

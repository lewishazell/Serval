using System;

using Serval.Transceive.Tcp;
using Serval.Communication.Tcp;
using System.Text;

namespace Serval.Transceive.Encoding.Tcp {
    public sealed class TcpEncodingTransmitter : EncodingTransmitter<Connection>, ITcpTransmitter<string> {
        private ITcpTransmitter<byte[]> Child { get; }

        public TcpEncodingTransmitter(ITcpTransmitter<byte[]> child, System.Text.Encoding encoding) : base(child, encoding) {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public void DisconnectAsync(Connection connection) {
            Child.DisconnectAsync(connection);
        }
    }
}

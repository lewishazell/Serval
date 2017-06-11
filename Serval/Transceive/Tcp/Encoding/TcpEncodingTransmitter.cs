using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp.Encoding {
    public sealed class TcpEncodingTransmitter : TcpForwardingTransmitter<string, byte[]> {
        private System.Text.Encoding Encoding {
            get;
        }

        public TcpEncodingTransmitter(ITcpTransmitter<byte[]> child, System.Text.Encoding encoding) : base(child) {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public override void SendAsync(Connection connection, string message) {
            Forward(connection ?? throw new ArgumentNullException(nameof(connection)), Encoding.GetBytes(message ?? throw new ArgumentNullException(nameof(message))));
        }
    }
}

using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp.Encoding {
    public class TcpDecodingReceiver : TcpForwardingReceiver<byte[], string> {
        private System.Text.Encoding Encoding {
            get;
        }

        public TcpDecodingReceiver(ITcpReceiver<string> child, System.Text.Encoding encoding) : base(child) {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public override void Received(Connection connection, byte[] data) {
            Forward(connection ?? throw new ArgumentException(nameof(connection)), Encoding.GetString(data ?? throw new ArgumentNullException(nameof(data))));
        }
    }
}

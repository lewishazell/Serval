using System.Net;

using Serval.Transceive.Udp;

namespace Serval.Transceive.Encoding.Udp {
    public sealed class UdpEncodingTransmitter : EncodingTransmitter<EndPoint>, IUdpTransmitter<string> {
        public UdpEncodingTransmitter(IUdpTransmitter<byte[]> child, System.Text.Encoding encoding) : base(child, encoding) {
        }
    }
}

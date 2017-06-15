using System.Net;
using Serval.Transceive.Udp;

namespace Serval.Transceive.Encoding.Udp {
    public sealed class UdpDecodingReceiver : DecodingReceiver<EndPoint> {
        public UdpDecodingReceiver(IUdpReceiver<string> child, System.Text.Encoding encoding) : base(child, encoding) {
        }
    }
}

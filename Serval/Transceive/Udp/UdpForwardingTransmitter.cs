using System.Net;

namespace Serval.Transceive.Udp {
    public abstract class UdpForwardingTransmitter<TInput, TOutput> : ForwardingTransmitter<EndPoint, TInput, TOutput>, IUdpTransmitter<TInput> {
        public UdpForwardingTransmitter(IUdpTransmitter<TOutput> child) : base(child) {
        }
    }
}

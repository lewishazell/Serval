using System.Net;

namespace Serval.Transceive.Udp {
    public abstract class UdpForwardingReceiver<TInput, TOutput> : ForwardingReceiver<EndPoint, TInput, TOutput>, IUdpReceiver<TInput> {
        public UdpForwardingReceiver(IUdpReceiver<TOutput> child) : base(child) {
        }
    }
}

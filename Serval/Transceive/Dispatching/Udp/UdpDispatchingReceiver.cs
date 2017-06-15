using System.Net;
using Serval.Parallel;
using Serval.Transceive.Udp;

namespace Serval.Transceive.Dispatching.Udp {
    public sealed class UdpDispatchingReceiver<TForward> : DispatchingReceiver<EndPoint, TForward>, IUdpReceiver<TForward> {
        public UdpDispatchingReceiver(IUdpReceiver<TForward> child, IDispatcher dispatcher) : base(child, dispatcher) {
        }
    }
}

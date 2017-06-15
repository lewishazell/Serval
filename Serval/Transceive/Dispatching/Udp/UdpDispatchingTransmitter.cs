using System.Net;
using Serval.Parallel;
using Serval.Transceive.Udp;

namespace Serval.Transceive.Dispatching.Udp {
    public sealed class UdpDispatchingTransmitter<TForward> : DispatchingTransmitter<EndPoint, TForward>, IUdpTransmitter<TForward> {
        public UdpDispatchingTransmitter(IUdpTransmitter<TForward> child, IDispatcher dispatcher) : base(child, dispatcher) {
        }
    }
}

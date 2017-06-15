using System.Net;

namespace Serval.Transceive.Udp {
    public interface IUdpReceiver<TInput> : IReceiver<EndPoint, TInput> {
    }
}

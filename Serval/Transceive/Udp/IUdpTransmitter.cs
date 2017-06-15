using System.Net;

namespace Serval.Transceive.Udp {
    public interface IUdpTransmitter<TInput> : ITransmitter<EndPoint, TInput> {
    }
}

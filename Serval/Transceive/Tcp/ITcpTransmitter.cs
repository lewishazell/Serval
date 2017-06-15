using Serval.Communication.Tcp;
using Serval.Channels.Tcp;

namespace Serval.Transceive.Tcp {
    public interface ITcpTransmitter<TInput> : ITransmitter<Connection, TInput> {
        void DisconnectAsync(Connection connection);
    }
}

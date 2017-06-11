using Serval.Communication.Tcp;
using Serval.Channels.Tcp;

namespace Serval.Transceive.Tcp {
    public interface ITcpTransmitter<TInput> {
        void SendAsync(Connection connection, TInput obj);

        void DisconnectAsync(Connection connection);
    }
}

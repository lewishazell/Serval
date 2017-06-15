using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp {
    public interface ITcpReceiver<TInput> : IReceiver<Connection, TInput> {
        void Connected(Connection connection);

        void Disconnected(Connection connection, IDisposable disposer);
    }
}

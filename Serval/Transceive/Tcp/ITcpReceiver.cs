using System;
using Serval.Communication.Tcp;

namespace Serval.Transceive.Tcp {
    public interface ITcpReceiver<TInput> {
        void Connected(Connection connection);

        void Received(Connection connection, TInput obj);

        void Sent(Connection connection);

        void Disconnected(Connection connection, IDisposable disposer);

        void Caught(Connection connection, Exception expception);
    }
}

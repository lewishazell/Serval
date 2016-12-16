using System;

namespace Serval.Channels {
    interface IChannel {
        void Connected();

        void Received();

        void Send();

        void Disconnected();
    }
}


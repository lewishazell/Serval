using System;
using System.Net;
using System.Collections.Immutable;

namespace Serval.Connections {
    public interface IConnection {
        EndPoint Address {
            get;
        }

        void Send(ImmutableArray<byte> data);

        void Disconnect();
    }
}


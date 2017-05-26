using System;
using System.Net;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Serval.Communication.Tcp {
    public interface IConnection {
        EndPoint EndPoint {
            get;
        }

        Task<Connection> SendAsync(ImmutableArray<byte> data);

        Task<Tuple<Connection, ImmutableArray<byte>>> ReceiveAsync();

        void Disconnect();
    }
}


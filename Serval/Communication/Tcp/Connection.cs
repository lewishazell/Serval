using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Serval.Communication.Tcp {
    using SendTuple = Tuple<Connection, ImmutableArray<byte>, Action>;

    public sealed class Connection : IConnection {
        internal Socket Socket {
            get;
        }

        public EndPoint EndPoint {
            get;
        }

        internal Connection(Socket socket) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            EndPoint = socket.RemoteEndPoint;
            Socket = socket;
        }
    }
}


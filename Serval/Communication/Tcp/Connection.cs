using System;
using System.Net;
using System.Net.Sockets;

namespace Serval.Communication.Tcp {
    public sealed class Connection {
        internal Socket Socket {
            get;
        }

        public EndPoint EndPoint {
            get;
        }

        internal Connection(Socket socket) {
            EndPoint = socket.RemoteEndPoint;
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }
    }
}


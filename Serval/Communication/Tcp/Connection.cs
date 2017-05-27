using System;
using System.Net;
using System.Net.Sockets;

namespace Serval.Communication.Tcp {
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


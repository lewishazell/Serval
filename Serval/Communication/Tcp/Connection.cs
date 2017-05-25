using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Immutable;

namespace Serval.Communication.Tcp {
    public sealed class Connection {
        private Channels.Tcp.TcpChannelHandler Handler {
            get;
        }

        internal TcpCommunicator Communicator {
            get;
        }

        internal Socket Socket {
            get;
        }

        public EndPoint EndPoint {
            get;
        }

        internal Connection(Channels.Tcp.TcpChannelHandler handler, TcpCommunicator communicator, Socket socket) {
            if(handler == null)
                throw new ArgumentNullException("handler");
            if(communicator == null)
                throw new ArgumentNullException("communicator");
            if(socket == null)
                throw new ArgumentNullException("socket");
            Handler = handler;
            Communicator = communicator;
            EndPoint = socket.RemoteEndPoint;
            Socket = socket;
        }

        public void Send(ImmutableArray<byte> data) {
            if(data == null)
                throw new ArgumentNullException("data");
            Handler.Send(this, data);
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }
    }
}


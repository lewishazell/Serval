using System;
using System.Net;
using System.Net.Sockets;

namespace Serval.Connections {
    public sealed class Connection {
        public EndPoint EndPoint {
            get;
        }

        Socket Socket {
            get;
        }

        public Channels.Channel Channel {
            get;
        }

        internal Connection(Socket socket, Channels.Channel channel) {
            EndPoint = socket.RemoteEndPoint;
            Socket = socket;
            Channel = channel;
        }

        public void Disconnect() {

        }
    }
}


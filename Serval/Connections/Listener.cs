using System;
using System.Net;
using System.Net.Sockets;

namespace Serval.Connections {
    sealed class Listener : IListener {
        public readonly Socket listener;

        public Server Server {
            get;
        }

        public IPEndPoint EndPoint {
            get;
        }

        public Listener(Server server, IPEndPoint endpoint, int backlog = 100) {
            Server = server;
            EndPoint = endpoint;
            listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endpoint);
            listener.Listen(backlog);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += Heard;
            Listen(args);
        }

        private void Heard(object sender, SocketAsyncEventArgs args) {
            Server.Handler(sender, args);
            Listen(args);
        }

        private void Listen(SocketAsyncEventArgs args) {
            args.AcceptSocket = null;
            if(!listener.AcceptAsync(args))
                Heard(listener, args);
        }
    }
}


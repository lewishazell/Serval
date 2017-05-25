using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Immutable;

using Serval.Communication.Tcp;

namespace Serval.Channels.Tcp {
    public sealed class TcpChannel : Channel {
        private TcpCommunicator Communicator {
            get;
        }

        internal TcpChannelHandler Handler {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, TcpChannelHandler handler, int buffers, int bufferSize, int eventArgs) : base(endpoint, buffers, bufferSize, eventArgs) {
            if(handler == null)
                throw new ArgumentNullException("handler");
            Communicator = new TcpCommunicator(this);
            Communicator.Bind(endpoint, 100);
            Handler = handler;
        }

        internal void Accept(Connection connected) {
            if(connected == null)
                throw new ArgumentNullException("connected");
            Handler.OnAccept(connected);
        }

        internal void Send(Connection recipient, byte[] data) {

        }

        internal void Receive(Connection sender, byte[] data) {
            Handler.OnReceived(sender, data);
        }

        internal void Disconnect(Connection disconnected) {

        }

        internal void Close(Connection connection) {

        }
    }
}


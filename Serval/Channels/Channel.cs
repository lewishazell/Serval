using System;
using System.Net;
using System.Collections.Immutable;

using Serval.Connections;

namespace Serval.Channels {
    public abstract class Channel : IChannel {
        public IServer Server {
            get;
        }

        public abstract int InitialBuffers {
            get;
        }

        public abstract int BufferSize {
            get;
        }

        public abstract int BufferIncrement {
            get;
        }

        public abstract TimeSpan BufferTimeout {
            get;
        }

        public abstract int InitialEventArgs {
            get;
        }

        public abstract int EventArgsIncrement {
            get;
        }

        public abstract TimeSpan EventArgsTimeout {
            get;
        }

        public IPEndPoint EndPoint {
            get;
        }

        public Channel(IPEndPoint endpoint) {
            EndPoint = endpoint;
            Server = (IServer) new Connections.Server(this);
        }

        internal void Accept(Connections.Connection connected) {
            Accepted(connected);
        }

        internal void Send(Connections.Connection recipient, ImmutableArray<byte> data) {

        }

        internal void Receive(Connections.Connection sender, ImmutableArray<byte> data) {
            Received(sender, data);
        }

        internal void Disconnect(Connections.Connection disconnected) {

        }

        internal void Close(Connections.Connection connection) {

        }

        protected abstract void Accepted(Connections.Connection connected);

        protected abstract void Received(Connections.Connection sender, ImmutableArray<byte> data);
    }
}


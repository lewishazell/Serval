using System;
using System.Net;
using System.Net.Sockets;

using Serval.Communication.Tcp;
using Serval.Transceive.Tcp;
using Serval.Communication.Pooling;

namespace Serval.Channels.Tcp {    
    public class TcpChannel : TcpCommunicator, IChannel, ITcpTransmitter<byte[]> {
        private readonly ITcpReceiver<byte[]> _child;

        public EndPoint EndPoint {
            get;
        }

        public TcpChannel(EndPoint endpoint, IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, int listenBacklog, AddressFamily addressFamily, ITcpReceiver<byte[]> child) : base(buffers, arguments, addressFamily) {
            if(listenBacklog < 0)
                throw new ArgumentException("Argument " + nameof(listenBacklog) + " must not be negative.");
            EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _child = child ?? throw new ArgumentNullException(nameof(child));
            Bind(endpoint, listenBacklog);
        }

        public TcpChannel(EndPoint endpoint, int buffers, int bufferSize, int eventArgs, int listenBacklog, AddressFamily addressFamily, ITcpReceiver<byte[]> child) : this(endpoint, new ByteArrayPool(buffers, bufferSize), new SocketAsyncEventArgsPool(eventArgs), listenBacklog, addressFamily, child) {
        }

        protected override void Connected(Socket socket) {
            Connection connection = new Connection(socket ?? throw new ArgumentNullException(nameof(socket)));
            _child.Connected(connection);
            Receive(socket, connection);
        }

        protected override void Received(object token, byte[] data) {
            _child.Received((Connection) token ?? throw new ArgumentNullException(nameof(token)), data ?? throw new ArgumentNullException(nameof(data)));
        }

        protected override void Sent(object token) {
            _child.Sent((Connection) token ?? throw new ArgumentNullException(nameof(token)));
        }

        protected override void Disconnected(object token) {
            Connection connection = (Connection) token ?? throw new ArgumentNullException(nameof(token));
            _child.Disconnected(connection, new Disposer(connection.Socket));
        }

        protected override void Caught(object token, Exception exception) {
            _child.Caught((Connection) token ?? throw new ArgumentNullException(nameof(token)), exception ?? throw new ArgumentNullException(nameof(exception)));
        }

        public void SendAsync(Connection connection, byte[] data) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            Send(connection.Socket, data ?? throw new ArgumentNullException(nameof(data)), connection);
        }

        public void DisconnectAsync(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            Disconnect(connection.Socket);
        }
    }
}


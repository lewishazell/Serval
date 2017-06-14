using System;
using System.Net;
using System.Net.Sockets;

using Serval.Communication.Tcp;
using Serval.Transceive.Tcp;
using Serval.Communication.Pooling;

namespace Serval.Channels.Tcp {    
    public class TcpChannel : TcpCommunicator, IChannel, ITcpTransmitter<byte[]> {
        public IPEndPoint EndPoint {
            get;
        }

        private ITcpReceiver<byte[]> Child {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, int listenBacklog, AddressFamily addressFamily, ITcpReceiver<byte[]> child) : base(buffers, arguments, addressFamily) {
            if(listenBacklog < 0)
                throw new ArgumentException("Argument " + nameof(listenBacklog) + " must not be negative.");
            EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Child = child ?? throw new ArgumentNullException(nameof(child));
            Bind(endpoint, listenBacklog);
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs, int listenBacklog, AddressFamily addressFamily, ITcpReceiver<byte[]> child) : this(endpoint, new ByteArrayPool(buffers, bufferSize), new SocketAsyncEventArgsPool(eventArgs), listenBacklog, addressFamily, child) {
        }

        protected override void Connected(Socket socket) {
            Connection connection = new Connection(socket ?? throw new ArgumentNullException(nameof(socket)));
            Child.Connected(connection);
            Read(socket, connection);
        }

        protected override void Received(object token, byte[] data) {
            Child.Received((Connection) token ?? throw new ArgumentNullException(nameof(token)), data ?? throw new ArgumentNullException(nameof(data)));
        }

        protected override void Sent(object token) {
            Child.Sent((Connection) token ?? throw new ArgumentNullException(nameof(token)));
        }

        protected override void Disconnected(object token) {
            Connection connection = (Connection) token ?? throw new ArgumentNullException(nameof(token));
            Child.Disconnected(connection, new Disposer(connection.Socket));
        }

        protected override void Caught(object token, Exception exception) {
            Child.Caught((Connection) token ?? throw new ArgumentNullException(nameof(token)), exception ?? throw new ArgumentNullException(nameof(exception)));
        }

        public void SendAsync(Connection connection, byte[] data) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            SendAsync(connection.Socket, data ?? throw new ArgumentNullException(nameof(data)), connection);
        }

        public void DisconnectAsync(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            DisconnectAsync(connection.Socket);
        }
    }
}


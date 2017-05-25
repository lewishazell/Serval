using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Serval.Channels.Tcp;

namespace Serval.Communication.Tcp {
    public class TcpCommunicator : Communicator {
        private TcpChannel Channel {
            get;
        }

        public TcpCommunicator(TcpChannel channel) : base(channel, channel.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
            if(channel == null)
                throw new ArgumentNullException("channel");
            Channel = channel;
        }

        internal void Bind(IPEndPoint ep, int listenBacklog) {
            if(ep == null)
                throw new ArgumentNullException("ep");
            Socket.Bind(ep);
            Socket.Listen(listenBacklog);
            SocketAsyncEventArgs args = Arguments.Retreive();
            args.Completed += Heard;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        private void Heard(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException("sender");
            if(args == null)
                throw new ArgumentNullException("args");
            Socket accepted = args.AcceptSocket;
            SocketAsyncEventArgs receive = Arguments.Retreive();
            Connection connection = new Connection(Channel.Handler, this, accepted);
            receive.UserToken = connection;
            receive.Completed += Received;
            receive.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
            // "Read" nothing from the socket - basically, wait for activity.
            Task.Run(() => {
                Channel.Accept(connection);
                if(!accepted.ReceiveAsync(receive))
                    Received(accepted, receive);
            });
            args.AcceptSocket = null;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        private void Received(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException("sender");
            if(args == null)
                throw new ArgumentNullException("args");
            Connection connection = (Connection) args.UserToken;
            Socket socket = (Socket) sender;
            if(args.Buffer == Pooling.ByteArrayPool.NO_BUFFER) {
                byte[] buffer = Buffers.Retreive();
                args.SetBuffer(buffer, 0, buffer.Length);
                if(!socket.ReceiveAsync(args))
                    Received(sender, args);
            }else if(args.BytesTransferred == 0 || !socket.Connected) { // Disconnected
                Buffers.Return(args.Buffer);
                args.Completed -= Received;
                args.UserToken = null;
                args.AcceptSocket = null;
                args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
            } else {
                byte[] received = new byte[args.BytesTransferred];
                Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred); // Copy the received bytes, only for outputting purposes.
                if(socket.Available == 0) {
                    Buffers.Return(args.Buffer);
                    args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                }
                Channel.Receive(connection, received);
                if(!socket.ReceiveAsync(args))
                    Received(sender, args);
            }
        }

        public void Send(byte[] data) {
            if(data == null)
                throw new ArgumentNullException("data");
            Send(Socket, data);
        }

        internal void Send(Socket socket, byte[] data) {
            if(socket == null)
                throw new ArgumentNullException("socket");
            if(data == null)
                throw new ArgumentNullException("data");
            SocketAsyncEventArgs args = Arguments.Retreive();
            args.Completed += Sent;
            byte[] buffer = Buffers.Retreive();
            args.SetBuffer(buffer, 0, buffer.Length);
            Array.Copy(data, args.Buffer, Math.Min(data.Length, args.Buffer.Length));
            if(!socket.SendAsync(args))
                Sent(socket, args);
        }

        private void Sent(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException("sender");
            if(args == null)
                throw new ArgumentNullException("args");
            Buffers.Return(args.Buffer);
            args.Completed -= Sent;
            args.AcceptSocket = null;
            args.UserToken = null;
            args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
            Arguments.Return(args);
        }
    }
}


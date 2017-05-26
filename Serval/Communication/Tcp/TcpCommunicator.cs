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
                throw new ArgumentNullException(nameof(channel));
            Channel = channel;
        }

        internal void Bind(IPEndPoint ep, int listenBacklog) {
            if(ep == null)
                throw new ArgumentNullException(nameof(ep));
            Socket.Bind(ep);
            Socket.Listen(listenBacklog);
            Arguments.RetrieveAsync().ContinueWith(t => {
                SocketAsyncEventArgs args = t.Result;
                args.Completed += Heard;
                if(!Socket.AcceptAsync(args))
                    Heard(Socket, args);
            });
        }

        private void Heard(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Socket accepted = args.AcceptSocket;
            Arguments.RetrieveAsync().ContinueWith(t => {
                SocketAsyncEventArgs receive = t.Result;
                Connection connection = new Connection(accepted);
                receive.UserToken = connection;
                receive.Completed += Received;
                receive.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                // "Read" nothing from the socket - basically, wait for activity.
                Task.Run(() => {
                    Channel.Connected(connection);
                    if(!accepted.ReceiveAsync(receive))
                        Received(accepted, receive);
                });
                args.AcceptSocket = null;
                if(!Socket.AcceptAsync(args))
                    Heard(Socket, args);
            });
        }

        private void Received(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Connection connection = (Connection) args.UserToken;
            Socket socket = (Socket) sender;
            if(args.Buffer == Pooling.AsyncByteArrayPool.NO_BUFFER) {
                Buffers.RetrieveAsync().ContinueWith(task => {
                    byte[] buffer = task.Result;
                    args.SetBuffer(buffer, 0, buffer.Length);
                    if(!socket.ReceiveAsync(args))
                        Received(sender, args);
                });
            }else if(args.BytesTransferred == 0 || !socket.Connected) { // Disconnected
                Buffers.Return(args.Buffer);
                args.Completed -= Received;
                args.UserToken = null;
                args.AcceptSocket = null;
                args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
            } else {
                byte[] received = new byte[args.BytesTransferred];
                Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred); // Copy the received bytes, only for outputting purposes.
                if(socket.Available == 0) {
                    Buffers.Return(args.Buffer);
                    args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                }
                Channel.Receive(connection, received);
                if(!socket.ReceiveAsync(args))
                    Received(sender, args);
            }
        }

        public void Send(byte[] data, Action callback = null) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            Send(Socket, data, callback);
        }

        internal void Send(Socket socket, byte[] data, Action callback) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            Arguments.RetrieveAsync().ContinueWith(t1 => {
                SocketAsyncEventArgs args = t1.Result;
                args.Completed += Sent;
                args.UserToken = callback;
                Buffers.RetrieveAsync().ContinueWith(t2 => {
                    byte[] buffer = t2.Result;
                    args.SetBuffer(buffer, 0, buffer.Length);
                    Array.Copy(data, args.Buffer, Math.Min(data.Length, args.Buffer.Length));
                    if(!socket.SendAsync(args))
                        Sent(socket, args);
                });
            });
        }

        private void Sent(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Buffers.Return(args.Buffer);
            args.Completed -= Sent;
            args.AcceptSocket = null;
            Action callback = args.UserToken as Action;
            args.UserToken = null;
            args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
            Arguments.Return(args);
            callback?.Invoke();
        }
    }
}


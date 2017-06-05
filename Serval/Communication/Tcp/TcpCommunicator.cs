using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Serval.Communication.Tcp {
    public class TcpCommunicator : Communicator {
        private readonly BufferBlock<Socket> _accepted = new BufferBlock<Socket>();
        private readonly BufferBlock<Tuple<object, byte[]>> _received = new BufferBlock<Tuple<object, byte[]>>();
        private readonly BufferBlock<object> _disconnected = new BufferBlock<object>();
        
        public TcpCommunicator(int buffers, int bufferSize, int eventArgs, AddressFamily family) : base(buffers, bufferSize, eventArgs, family, SocketType.Stream, ProtocolType.Tcp) {
        }

        internal void Bind(IPEndPoint ep, int listenBacklog) {
            if(ep == null)
                throw new ArgumentNullException(nameof(ep));
            Socket.Bind(ep);
            Socket.Listen(listenBacklog);
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Heard;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        private void Heard(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Socket socket = args.AcceptSocket;
            _accepted.Post(socket);
            args.AcceptSocket = null;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        internal Task<Socket> AcceptedAsync() {
            return _accepted.ReceiveAsync();
        }

        internal void Read(Socket socket, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(token == null)
                throw new ArgumentNullException(nameof(token));
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.UserToken = token;
            args.Completed += Received;
            // "Read" nothing from the socket - basically, wait for activity.
            args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
            if(!socket.ReceiveAsync(args))
                Received(socket, args);
        }

        private void Received(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            object token = args.UserToken;
            Socket socket = (Socket) sender;
            if(args.Buffer == Pooling.ByteArrayPool.NO_BUFFER) {
                byte[] buffer = Buffers.Retrieve();
                args.SetBuffer(buffer, 0, buffer.Length);
                if(!socket.ReceiveAsync(args))
                    Received(sender, args);
            }else {
                if(!socket.Connected || args.BytesTransferred == 0) {
                    args.Completed -= Received;
                    args.UserToken = null;
                    args.AcceptSocket = null;
                    args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                    Buffers.Return(args.Buffer);
                    Arguments.Return(args);
                    _disconnected.Post(token);
                } else {
                    byte[] received = new byte[args.BytesTransferred];
                    Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred); // Copy the received bytes, only for outputting purposes.
                    if(socket.Available == 0) {
                        args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                        Buffers.Return(args.Buffer);
                    }
                    _received.Post(new Tuple<object, byte[]>(token, received));
                    if(!socket.ReceiveAsync(args))
                        Received(socket, args);
                }
            }
        }

        internal Task<Tuple<object, byte[]>> ReceivedAsync() {
            return _received.ReceiveAsync();
        }

        internal Task<object> DisconnectedAsync() {
            return _disconnected.ReceiveAsync();
        }

        internal Task<object> Send(Socket socket, byte[] data, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Sent;
            args.UserToken = new Tuple<TaskCompletionSource<object>, object>(tcs, token);
            byte[] buffer = Buffers.Retrieve();
            int length = Math.Min(data.Length, buffer.Length);
            args.SetBuffer(buffer, 0, length);
            Array.Copy(data, args.Buffer, length);
            try {
                if(!socket.SendAsync(args))
                    Sent(socket, args);
            }catch(Exception ex) {
                args.Completed -= Sent;
                args.UserToken = null;
                Buffers.Return(args.Buffer);
                args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        private void Sent(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Tuple<TaskCompletionSource<object>, object> tuple =
                (Tuple<TaskCompletionSource<object>, object>) args.UserToken;
            TaskCompletionSource<object> tcs = tuple.Item1;
            object token = tuple.Item2;
            try {
                Buffers.Return(args.Buffer);
                args.Completed -= Sent;
                args.AcceptSocket = null;
                args.UserToken = null;
                args.SetBuffer(Pooling.ByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
                tcs.SetResult(token);
            } catch(Exception ex) {
                tcs.SetException(ex);
            }
        }

        internal void Disconnect(Socket socket) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            socket.Shutdown(SocketShutdown.Both);
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Disconnected;
            if(!socket.DisconnectAsync(args))
                Disconnected(socket, args);
        }

        private void Disconnected(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            args.Completed -= Disconnected;
            args.AcceptSocket = null;
            args.UserToken = null;
            Arguments.Return(args);
        }
    }
}


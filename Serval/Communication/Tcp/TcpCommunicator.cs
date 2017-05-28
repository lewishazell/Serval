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

        internal async void Bind(IPEndPoint ep, int listenBacklog) {
            if(ep == null)
                throw new ArgumentNullException(nameof(ep));
            Socket.Bind(ep);
            Socket.Listen(listenBacklog);
            SocketAsyncEventArgs args = await Arguments.RetrieveAsync();
            args.Completed += Heard;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        private async void Heard(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Socket socket = args.AcceptSocket;
            Task.Run(() => _accepted.Post(socket));
            args.AcceptSocket = null;
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        internal Task<Socket> AcceptedAsync() {
            return _accepted.ReceiveAsync();
        }

        internal async void Read(Socket socket, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(token == null)
                throw new ArgumentNullException(nameof(token));
            SocketAsyncEventArgs args = await Arguments.RetrieveAsync();
            args.UserToken = token;
            args.Completed += Received;
            // "Read" nothing from the socket - basically, wait for activity.
            args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
            if(!socket.ReceiveAsync(args))
                Received(socket, args);
        }

        private async void Received(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            object token = args.UserToken;
            Socket socket = (Socket) sender;
            if(args.Buffer == Pooling.AsyncByteArrayPool.NO_BUFFER) {
                byte[] buffer = await Buffers.RetrieveAsync();
                args.SetBuffer(buffer, 0, buffer.Length);
                if(!socket.ReceiveAsync(args))
                    Received(sender, args);
            }else {
                if(!socket.Connected || args.BytesTransferred == 0) {
                    args.Completed -= Received;
                    args.UserToken = null;
                    args.AcceptSocket = null;
                    args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                    Buffers.Return(args.Buffer);
                    Arguments.Return(args);
                    Task.Run(() => _disconnected.Post(token));
                } else {
                    byte[] received = new byte[args.BytesTransferred];
                    Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred); // Copy the received bytes, only for outputting purposes.
                    if(socket.Available == 0) {
                        args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                        Buffers.Return(args.Buffer);
                    }
                    Task.Run(() => {
                        _received.Post(new Tuple<object, byte[]>(token, received));
                        if(!socket.ReceiveAsync(args))
                            Received(socket, args);
                    });
                }
            }
        }

        internal Task<Tuple<object, byte[]>> ReceivedAsync() {
            return _received.ReceiveAsync();
        }

        internal Task<object> DisconnectedAsync() {
            return _disconnected.ReceiveAsync();
        }

        internal async Task<object> Send(Socket socket, byte[] data, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            SocketAsyncEventArgs args = await Arguments.RetrieveAsync();
            args.Completed += Sent;
            args.UserToken = new Tuple<TaskCompletionSource<object>, object>(tcs, token);
            byte[] buffer = await Buffers.RetrieveAsync();
            args.SetBuffer(buffer, 0, buffer.Length);
            Array.Copy(data, args.Buffer, Math.Min(data.Length, args.Buffer.Length));
            Array.Clear(args.Buffer, data.Length, args.Buffer.Length - data.Length);
            try {
                if(!socket.SendAsync(args))
                    Sent(socket, args);
            }catch(Exception ex) {
                args.Completed -= Sent;
                args.UserToken = null;
                Buffers.Return(args.Buffer);
                args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
                Task.Run(() => tcs.SetException(ex));
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
                args.SetBuffer(Pooling.AsyncByteArrayPool.NO_BUFFER, 0, 0);
                Arguments.Return(args);
                Task.Run(() => tcs.SetResult(token));
            } catch(Exception ex) {
                Task.Run(() => tcs.SetException(ex));
            }
        }

        internal async void Disconnect(Socket socket) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            socket.Shutdown(SocketShutdown.Both);
            SocketAsyncEventArgs args = await Arguments.RetrieveAsync();
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


﻿using System;
using System.Net;
using System.Net.Sockets;

using Serval.Communication.Pooling;

namespace Serval.Communication.Tcp {
    public abstract class TcpCommunicator : Communicator {
        private static readonly byte[] NO_BUFFER = new byte[0];

        public TcpCommunicator(IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, AddressFamily addressFamily) : base(buffers, arguments, addressFamily, SocketType.Stream, ProtocolType.Tcp) {
        }

        protected void Bind(IPEndPoint ep, int listenBacklog) {
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
            args.AcceptSocket = null;
            Connected(socket);
            if(!Socket.AcceptAsync(args))
                Heard(Socket, args);
        }

        protected abstract void Connected(Socket socket);

        protected void Read(Socket socket, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(token == null)
                throw new ArgumentNullException(nameof(token));
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.UserToken = token;
            args.Completed += Received;
            args.SetBuffer(NO_BUFFER, 0, 0);
            Read(socket, args);
        }

        private void Read(Socket socket, SocketAsyncEventArgs args) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
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
            if(args.Buffer == NO_BUFFER) {
                byte[] buffer = Buffers.Retrieve();
                args.SetBuffer(buffer, 0, buffer.Length);
                Read(socket, args);
            }else {
                if(!socket.Connected || args.BytesTransferred == 0) {
                    args.Completed -= Received;
                    args.UserToken = null;
                    args.AcceptSocket = null;
                    Buffers.Return(args.Buffer);
                    args.SetBuffer(NO_BUFFER, 0, 0);
                    Arguments.Return(args);
                    Disconnected(token);
                } else {
                    byte[] received = new byte[args.BytesTransferred];
                    Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred);
                    Received(token, received);
                    if(socket.Available == 0) {
                        Buffers.Return(args.Buffer);
                        args.SetBuffer(NO_BUFFER, 0, 0);
                    }
                    Read(socket, args);
                }
            }
        }

        protected abstract void Received(object token, byte[] data);

        protected abstract void Disconnected(object token);

        protected void SendAsync(Socket socket, byte[] data, object token) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Sent;
            args.UserToken = token;
            byte[] buffer = Buffers.Retrieve();
            try {
                int length = Math.Min(data.Length, buffer.Length);
                args.SetBuffer(buffer, 0, length);
                Array.Copy(data, args.Buffer, length);
                if(!socket.SendAsync(args))
                    Sent(socket, args);
            }catch(Exception ex) {
                args.Completed -= Sent;
                args.UserToken = null;
                Buffers.Return(args.Buffer);
                args.SetBuffer(NO_BUFFER, 0, 0);
                Arguments.Return(args);
                Caught(token, ex);
            }
        }

        private void Sent(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            object token = args.UserToken;
            try {
                args.Completed -= Sent;
                args.AcceptSocket = null;
                args.UserToken = null;
                Buffers.Return(args.Buffer);
                args.SetBuffer(NO_BUFFER, 0, 0);
                Arguments.Return(args);
                Sent(token);
            } catch(Exception ex) {
                Caught(token, ex);
            }
        }

        protected abstract void Sent(object token);

        protected abstract void Caught(object token, Exception ex);

        internal void DisconnectAsync(Socket socket) {
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


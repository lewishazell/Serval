using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Immutable;

namespace Serval.Connections {
    public class Server : IServer {
        private readonly Pooling.ByteArrayGrowingBlockingCollection buffers;
        private readonly Pooling.SocketAsyncEventArgsGrowingBlockingCollection saea;
        private readonly Listener listener;
        private readonly Channels.Channel channel;

        public Server(Channels.Channel channel) {
            this.listener = new Listener(this, channel.EndPoint);
            this.channel = channel;
            buffers = new Serval.Connections.Pooling.ByteArrayGrowingBlockingCollection(channel.BufferTimeout, channel.BufferIncrement, channel.BufferSize, channel.InitialBuffers);
            saea = new Pooling.SocketAsyncEventArgsGrowingBlockingCollection(this.Handler, channel.EventArgsTimeout, channel.EventArgsIncrement, channel.InitialEventArgs);
        }

        internal void Handler(object sender, SocketAsyncEventArgs args) {
            Socket socket = (Socket) sender;
            switch(args.LastOperation) {
                case SocketAsyncOperation.Accept:
                    Heard(socket, args);
                    break;
                case SocketAsyncOperation.Receive:
                    Received(socket, args);
                    break;
            }
        }

        private void Heard(object sender, SocketAsyncEventArgs args) {
            Socket accepted = args.AcceptSocket;
            SocketAsyncEventArgs receive = saea.TakeOrGrow();
            Connection connection = new Connection(accepted, channel);
            receive.UserToken = new Token {
                Connection = connection
            };
            receive.SetBuffer(Utils.NO_BUFFER, 0, 0); // Empty buffer, waiting for activity.
            // "Read" nothing from the socket - basically, wait for activity.
            channel.Accept(connection);
            if(!accepted.ReceiveAsync(receive))
                Received(accepted, receive);
        }

        private void Received(Socket sender, SocketAsyncEventArgs args) {
            Token token = (Token) args.UserToken;
            if(args.Buffer == Utils.NO_BUFFER) {
                args.SetBuffer(buffers.TakeOrGrow(), 0, buffers.BufferSize);
                if(!sender.ReceiveAsync(args))
                    Received(sender, args);
            }else if(args.BytesTransferred == 0 || !sender.Connected) { // Disconnected
                buffers.Add(args.Buffer);
                args.SetBuffer(Utils.NO_BUFFER, 0, 0);
                saea.Add(args);
            } else {
                byte[] received = new byte[args.BytesTransferred < buffers.BufferSize ? args.BytesTransferred : buffers.BufferSize];
                Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred); // Copy the received bytes, only for outputting purposes.
                if(args.BytesTransferred < buffers.BufferSize) {
                    buffers.Add(args.Buffer);
                    args.SetBuffer(Utils.NO_BUFFER, 0, 0);
                }
                channel.Receive(token.Connection, received.ToImmutableArray());
                if(!sender.ReceiveAsync(args))
                    Received(sender, args);
            }
        }
    }
}


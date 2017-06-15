using System;
using System.Net;
using System.Net.Sockets;

using Serval.Communication.Pooling;

namespace Serval.Communication.Udp {
    public abstract class UdpCommunicator : Communicator {
        private readonly EndPoint _listen = new IPEndPoint(IPAddress.Any, 0);

        public UdpCommunicator(IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, AddressFamily addressFamily) : base(buffers, arguments, addressFamily, SocketType.Dgram, ProtocolType.Udp) {
        }

        protected void Bind(EndPoint ep) {
            if(ep == null)
                throw new ArgumentNullException(nameof(ep));
            Socket.Bind(ep);
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Received;
            byte[] buffer = Buffers.Retrieve();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.RemoteEndPoint = _listen;
            if(!Socket.ReceiveFromAsync(args))
                Received(Socket, args);
        }

        protected abstract void Received(EndPoint endpoint, byte[] data);

        private void Received(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            Socket socket = (Socket) sender;
            EndPoint endpoint = args.RemoteEndPoint;
            byte[] received = new byte[args.BytesTransferred];
            Array.Copy(args.Buffer, 0, received, 0, args.BytesTransferred);
            Received(endpoint, received);
            args.RemoteEndPoint = _listen;
            if(!Socket.ReceiveFromAsync(args))
                Received(Socket, args);
        }

        protected void Send(EndPoint endpoint, byte[] data) {
            if(endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            SocketAsyncEventArgs args = Arguments.Retrieve();
            args.Completed += Sent;
            args.RemoteEndPoint = endpoint;
            byte[] buffer = Buffers.Retrieve();
            int length = Math.Min(data.Length, buffer.Length);
            args.SetBuffer(buffer, 0, length);
            Array.Copy(data, args.Buffer, length);
            try {
                if(!Socket.SendToAsync(args))
                    Sent(Socket, args);
            } catch(Exception ex) {
                args.Completed -= Sent;
                args.RemoteEndPoint = null;
                Buffers.Return(buffer);
                args.SetBuffer(NO_BUFFER, 0, 0);
                Arguments.Return(args);
                Caught(endpoint, ex);
            }
        }

        private void Sent(object sender, SocketAsyncEventArgs args) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(args == null)
                throw new ArgumentNullException(nameof(args));
            EndPoint endpoint = args.RemoteEndPoint;
            args.Completed -= Sent;
            args.RemoteEndPoint = null;
            Buffers.Return(args.Buffer);
            args.SetBuffer(NO_BUFFER, 0, 0);
            Arguments.Return(args);
            try {
                Sent(endpoint);
            }catch(Exception ex) {
                Caught(endpoint, ex);
            }
        }

        protected abstract void Sent(EndPoint endpoint);

        protected abstract void Caught(EndPoint endpoint, Exception ex);
    }
}

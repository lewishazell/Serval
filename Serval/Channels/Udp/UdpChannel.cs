using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serval.Communication.Udp;
using Serval.Transceive.Udp;
using System.Net;
using Serval.Communication.Pooling;
using System.Net.Sockets;

namespace Serval.Channels.Udp {
    public class UdpChannel : UdpCommunicator, IChannel, IUdpTransmitter<byte[]> {
        private readonly IUdpReceiver<byte[]> _child;

        public EndPoint EndPoint {
            get;
        }

        public UdpChannel(EndPoint endpoint, IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, AddressFamily addressFamily, IUdpReceiver<byte[]> child) : base(buffers, arguments, addressFamily) {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Bind(endpoint);
        }

        public UdpChannel(EndPoint endpoint, int buffers, int bufferSize, int eventArgs, AddressFamily addressFamily, IUdpReceiver<byte[]> child) : this(endpoint, new ByteArrayPool(buffers, bufferSize), new SocketAsyncEventArgsPool(eventArgs), addressFamily, child) {
        }

        public void SendAsync(EndPoint recipient, byte[] obj) {
            Send(recipient, obj);
        }

        protected override void Received(EndPoint endpoint, byte[] data) {
            _child.Received(endpoint, data);
        }

        protected override void Sent(EndPoint endpoint) {
            _child.Sent(endpoint);
        }

        protected override void Caught(EndPoint endpoint, Exception ex) {
            _child.Caught(endpoint, ex);
        }
    }
}

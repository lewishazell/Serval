using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Serval.Communication.Tcp {
    public sealed class Connection {
        internal BufferBlock<byte[]> Receives {
            get;
        } = new BufferBlock<byte[]>();

        internal BufferBlock<Tuple<Connection, ImmutableArray<byte>>> Sends {
            get;
        } = new BufferBlock<Tuple<Connection, ImmutableArray<byte>>>();

        internal Socket Socket {
            get;
        }

        public EndPoint EndPoint {
            get;
        }

        internal Connection(Socket socket) {
            if(socket == null)
                throw new ArgumentNullException(nameof(socket));
            EndPoint = socket.RemoteEndPoint;
            Socket = socket;
        }

        public void Send(ImmutableArray<byte> data) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            Sends.Post(new Tuple<Connection, ImmutableArray<byte>>(this, data));
        }

        public async Task<byte[]> ReceiveAsync() {
            return await Receives.ReceiveAsync();
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }
    }
}


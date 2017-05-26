using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Serval.Communication.Tcp {
    using SendTuple = Tuple<Connection, ImmutableArray<byte>, Action>;

    public sealed class Connection : IConnection {
        private BufferBlock<Tuple<Connection, ImmutableArray<byte>>> Receives {
            get;
        } = new BufferBlock<Tuple<Connection, ImmutableArray<byte>>>();

        private BufferBlock<SendTuple> Sends {
            get;
        } = new BufferBlock<SendTuple>();

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

        public Task<Connection> SendAsync(ImmutableArray<byte> data) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            TaskCompletionSource<Connection> tcs = new TaskCompletionSource<Connection>();
            Sends.Post(new SendTuple(this, data, () => tcs.SetResult(this)));
            return tcs.Task;
        }

        internal async Task<SendTuple> GetSendAsync() {
            return await Sends.ReceiveAsync();
        }

        internal void PostReceive(ImmutableArray<byte> data) {
            Receives.Post(new Tuple<Connection, ImmutableArray<byte>>(this, data));
        }
        
        public async Task<Tuple<Connection, ImmutableArray<byte>>> ReceiveAsync() {
            return await Receives.ReceiveAsync();
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }
    }
}


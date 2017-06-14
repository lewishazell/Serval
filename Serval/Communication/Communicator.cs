using System.Net.Sockets;

using Serval.Communication.Pooling;

namespace Serval.Communication {
    public abstract class Communicator {
        protected IPool<byte[]> Buffers {
            get;
        }

        protected IPool<SocketAsyncEventArgs> Arguments {
            get;
        }

        protected Socket Socket {
            get;
        }

        public Communicator(IPool<byte[]> buffers, IPool<SocketAsyncEventArgs> arguments, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            Buffers = buffers;
            Arguments = arguments;
            Socket = new Socket(addressFamily, socketType, protocolType);
        }
    }
}


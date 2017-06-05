using System;
using System.Net.Sockets;

using Serval.Channels;

namespace Serval.Communication {
    public abstract class Communicator {
        protected Pooling.IPool<byte[]> Buffers {
            get;
        }

        protected Pooling.IPool<SocketAsyncEventArgs> Arguments {
            get;
        }

        protected Socket Socket {
            get;
        }

        public Communicator(int buffers, int bufferSize, int eventArgs, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            Buffers = new Pooling.ByteArrayPool(buffers, bufferSize);
            Arguments =  new Pooling.SocketAsyncEventArgsPool(eventArgs);
            Socket = new Socket(addressFamily, socketType, protocolType);
        }
    }
}


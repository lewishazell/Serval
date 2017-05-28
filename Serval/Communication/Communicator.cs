using System;
using System.Net.Sockets;

using Serval.Channels;

namespace Serval.Communication {
    public abstract class Communicator {
        protected Pooling.IAsyncPool<byte[]> Buffers {
            get;
        }

        protected Pooling.IAsyncPool<SocketAsyncEventArgs> Arguments {
            get;
        }

        protected Socket Socket {
            get;
        }

        public Communicator(int buffers, int bufferSize, int eventArgs, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            Buffers = new Pooling.AsyncByteArrayPool(buffers, bufferSize);
            Arguments =  new Pooling.AsyncSocketAsyncEventArgsPool(eventArgs);
            Socket = new Socket(addressFamily, socketType, protocolType);
        }
    }
}


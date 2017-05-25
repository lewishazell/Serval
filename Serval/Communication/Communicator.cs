using System;
using System.Net.Sockets;

using Serval.Channels;

namespace Serval.Communication {
    public abstract class Communicator {
        private Channel Channel {
            get;
        }

        protected Pooling.IPool<byte[]> Buffers {
            get;
        }

        protected Pooling.IPool<SocketAsyncEventArgs> Arguments {
            get;
        }

        protected Socket Socket {
            get;
        }

        public Communicator(Channel channel, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            if(channel == null)
                throw new ArgumentNullException("channel");
            Channel = channel;
            Buffers = new Pooling.ByteArrayPool(Channel.Buffers, Channel.BufferSize);
            Arguments =  new Pooling.SocketAsyncEventArgsPool(Channel.EventArgs);
            Socket = new Socket(addressFamily, socketType, protocolType);
        }
    }
}


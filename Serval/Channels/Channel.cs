using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Immutable;

using Serval.Communication;

namespace Serval.Channels {
    public abstract class Channel {
        public IPEndPoint EndPoint {
            get;
        }

        public int Buffers {
            get;
        }

        public int BufferSize {
            get;
        }

        public int EventArgs {
            get;
        }

        public Channel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs) {
            if(endpoint == null)
                throw new ArgumentNullException("endpoint");
            EndPoint = endpoint;
            Buffers = buffers;
            BufferSize = bufferSize;
            EventArgs = eventArgs;
        }
    }
}


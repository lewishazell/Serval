using System;
using System.Net;
using System.Collections.Immutable;
using Serval.Connections;

namespace Serval.Channels {
    public interface IChannel {
        IServer Server {
            get;
        }

        int InitialBuffers {
            get;
        }

        int BufferSize {
            get;
        }

        int BufferIncrement {
            get;
        }

        TimeSpan BufferTimeout {
            get;
        }

        int InitialEventArgs {
            get;
        }

        int EventArgsIncrement {
            get;
        }

        TimeSpan EventArgsTimeout {
            get;
        }

        IPEndPoint EndPoint {
            get;
        }
    }
}


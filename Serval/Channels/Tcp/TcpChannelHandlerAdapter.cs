using System;

namespace Serval.Channels.Tcp {
    class TcpChannelHandlerAdapter : TcpChannelHandler {
        private TcpChannelHandler Handler {
            get;
        }

        internal TcpChannelHandlerAdapter(TcpChannelHandler handler) {
            Handler = handler;
        }

        internal Accept
    }
}


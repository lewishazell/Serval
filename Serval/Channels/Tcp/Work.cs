using System;
using System.Collections.Immutable;

using Serval.Communication.Tcp;
namespace Serval.Channels.Tcp {
    internal class Work {
        internal volatile Connection Connection;

        internal volatile Operation Operation;

        internal volatile byte[] Bytes;
    }
}


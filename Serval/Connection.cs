using System;
using System.Net;

namespace Serval {
    public sealed class Connection {
        public IPAddress Address {
            get;
        }

        Connection(IPAddress address) {
            Address = address;
        }
    }
}


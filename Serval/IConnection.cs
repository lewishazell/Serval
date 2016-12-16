using System;
using System.Net;

namespace Serval {
    public interface IConnection {
        IPAddress Address {
            get;
        }
    }
}


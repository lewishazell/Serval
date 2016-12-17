using System;
using System.Net;
namespace Serval {
    interface IListener {
        IPEndPoint EndPoint {
            get;
        }
    }
}


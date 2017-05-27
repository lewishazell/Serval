using System.Net;

namespace Serval.Communication.Tcp {
    public interface IConnection {
        EndPoint EndPoint {
            get;
        }
    }
}


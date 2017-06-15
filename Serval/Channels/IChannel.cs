using System.Net;

namespace Serval.Channels {
    public interface IChannel {
        EndPoint EndPoint { get;  }
    }
}

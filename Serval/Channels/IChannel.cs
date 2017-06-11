using System.Net;

namespace Serval.Channels {
    public interface IChannel {
        IPEndPoint EndPoint { get;  }
    }
}

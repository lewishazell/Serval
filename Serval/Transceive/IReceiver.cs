using System;

namespace Serval.Transceive {
    public interface IReceiver<TSender, TInput> {
        void Received(TSender sender, TInput obj);

        void Sent(TSender recipient);

        void Caught(TSender sender, Exception exception);
    }
}

using System;

using Serval.Parallel;

namespace Serval.Transceive.Dispatching {
    public abstract class DispatchingReceiver<TSender, TForward> : ForwardingReceiver<TSender, TForward, TForward> {
        private readonly IDispatcher _dispatcher;

        public DispatchingReceiver(IReceiver<TSender, TForward> child, IDispatcher dispatcher) : base(child) {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public override void Received(TSender sender, TForward obj) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            _dispatcher.Invoke(() => Forward(sender, obj));
        }

        public override void Sent(TSender sender) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            _dispatcher.Invoke(() => base.Sent(sender));
        }

        public override void Caught(TSender sender, Exception exception) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(exception == null)
                throw new ArgumentNullException(nameof(exception));
            _dispatcher.Invoke(() => base.Caught(sender, exception));
        }
    }
}

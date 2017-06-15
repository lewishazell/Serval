using System;

namespace Serval.Transceive {
    public abstract class ForwardingReceiver<TSender, TInput, TOutput> : IReceiver<TSender, TInput> {
        private readonly IReceiver<TSender, TOutput> _child;

        public ForwardingReceiver(IReceiver<TSender, TOutput> child) {
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public virtual void Caught(TSender sender, Exception exception) {
            _child.Caught(sender, exception);
        }

        public abstract void Received(TSender sender, TInput obj);

        protected void Forward(TSender sender, TOutput obj) {
            _child.Received(sender, obj);
        }

        public virtual void Sent(TSender sender) {
            _child.Sent(sender);
        }
    }
}

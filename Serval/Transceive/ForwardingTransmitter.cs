using System;

namespace Serval.Transceive {
    public abstract class ForwardingTransmitter<TRecipient, TInput, TOutput> : ITransmitter<TRecipient, TInput> {
        private readonly ITransmitter<TRecipient, TOutput> _child;

        public ForwardingTransmitter(ITransmitter<TRecipient, TOutput> child) {
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public abstract void SendAsync(TRecipient recipient, TInput obj);

        protected void Forward(TRecipient recipient, TOutput obj) {
            if(recipient == null)
                throw new ArgumentNullException(nameof(recipient));
            _child.SendAsync(recipient, obj);
        }
    }
}

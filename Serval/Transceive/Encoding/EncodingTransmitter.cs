using System;

namespace Serval.Transceive.Encoding {
    public abstract class EncodingTransmitter<TSender> : ForwardingTransmitter<TSender, string, byte[]> {
        private System.Text.Encoding Encoding {
            get;
        }

        public EncodingTransmitter(ITransmitter<TSender, byte[]> child, System.Text.Encoding encoding) : base(child) {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public override void SendAsync(TSender sender, string message) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            Forward(sender, Encoding.GetBytes(message ?? throw new ArgumentNullException(nameof(message))));
        }
    }
}

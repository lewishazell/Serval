using System;

namespace Serval.Transceive.Encoding {
    public abstract class DecodingReceiver<TSender> : ForwardingReceiver<TSender, byte[], string> {
        private System.Text.Encoding Encoding {
            get;
        }

        public DecodingReceiver(IReceiver<TSender, string> child, System.Text.Encoding encoding) : base(child) {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public override void Received(TSender sender, byte[] data) {
            if(sender == null)
                throw new ArgumentException(nameof(sender));
            Forward(sender, Encoding.GetString(data ?? throw new ArgumentNullException(nameof(data))));
        }
    }
}

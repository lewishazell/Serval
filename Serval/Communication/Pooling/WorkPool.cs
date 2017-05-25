using System;
using System.Collections.Immutable;

namespace Serval.Communication.Pooling {
    internal class WorkPool : FixedBlockingPool<Channels.Tcp.Work> {
        internal WorkPool(int size) : base(size) {
        }

        internal new void Return(Channels.Tcp.Work item) {
            item.Bytes = null;
            item.Operation = Operation.None;
            item.Connection = null;
            base.Return(item);
        }

        protected override Channels.Tcp.Work[] Generate(int amount) {
            Channels.Tcp.Work[] pairs = new Serval.Channels.Tcp.Work[amount];
            for(int i = 0; i < amount; i++)
                pairs[i] = new Channels.Tcp.Work();
            return pairs;
        }
    }
}


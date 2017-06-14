using System;
using System.Net.Sockets;

namespace Serval.Communication.Pooling {
    public sealed class SocketAsyncEventArgsPool : FixedBlockingPool<SocketAsyncEventArgs> {
        public SocketAsyncEventArgsPool(int size) : base(size) {
            if(size < 0)
                throw new ArgumentException("Argument " + nameof(size) + " must not be negative.");
        }

        protected override SocketAsyncEventArgs[] Generate(int amount) {
            SocketAsyncEventArgs[] args = new SocketAsyncEventArgs[amount];
            for(int i = 0; i < amount; i++)
                args[i] = new SocketAsyncEventArgs();
            return args;
        }
    }
}


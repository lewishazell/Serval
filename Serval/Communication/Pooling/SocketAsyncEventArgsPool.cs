using System.Net.Sockets;

namespace Serval.Communication.Pooling {
    internal class SocketAsyncEventArgsPool : FixedBlockingPool<SocketAsyncEventArgs> {
        internal SocketAsyncEventArgsPool(int size) : base(size) { }

        protected override SocketAsyncEventArgs[] Generate(int amount) {
            SocketAsyncEventArgs[] args = new SocketAsyncEventArgs[amount];
            for(int i = 0; i < amount; i++)
                args[i] = new SocketAsyncEventArgs();
            return args;
        }
    }
}


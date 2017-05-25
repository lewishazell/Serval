using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Concurrent;

using Serval.Communication.Tcp;
using Serval.Communication.Pooling;

namespace Serval.Channels.Tcp {
    public abstract class TcpChannelHandler {
        private WorkPool pool;
        BlockingCollection<Work> queue = new BlockingCollection<Work>();

        public TcpChannelHandler(int backlog) {
            pool = new Communication.Pooling.WorkPool(backlog);
            Task.Run(() => {
                Work work;
                while(true) {
                    work = queue.Take();
                    switch(work.Operation) {
                        case Operation.Accept:
                            Accept(work.Connection);
                            break;

                        case Operation.Receive:
                            Received(work.Connection, work.Bytes.ToImmutableArray());
                            break;

                        case Operation.Send:
                            work.Connection.Communicator.Send(work.Connection.Socket, work.Bytes);
                            break;

                        case Operation.Disconnect:
                            break;
                    }
                    pool.Return(work);
                }
            });
        }

        internal void OnAccept(Connection connection) {
            Work work = pool.Retreive();
            work.Connection = connection;
            work.Operation = Operation.Accept;
            queue.Add(work);
        }

        internal void OnReceived(Connection connection, byte[] data) {
            Work work = pool.Retreive();
            work.Connection = connection;
            work.Operation = Operation.Receive;
            work.Bytes = data;
            queue.Add(work);
        }

        protected abstract void Accept(Connection connection);

        protected abstract void Received(Connection connection, ImmutableArray<byte> data);

        internal void Send(Connection connection, ImmutableArray<byte> data) {
            if(connection == null)
                throw new ArgumentNullException("connection");
            if(data == null)
                throw new ArgumentNullException("data");
            Work work = pool.Retreive();
            work.Connection = connection;
            work.Bytes = new byte[data.Length];
            data.CopyTo(work.Bytes);
            work.Operation = Operation.Send;
            queue.Add(work);
        }
    }
}


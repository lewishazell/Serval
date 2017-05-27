using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Serval.Communication.Tcp;

namespace Serval.Channels.Tcp {
    using SendTuple = Tuple<Connection, ImmutableArray<byte>, Action>;
    
    public sealed class TcpChannel : Channel {
        private readonly BufferBlock<Connection> _accepts = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _connected = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnects = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnected = new BufferBlock<Connection>();
        
        private TcpCommunicator Communicator {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs) : base(endpoint, buffers, bufferSize, eventArgs) {
            Communicator = new TcpCommunicator(this);
            Communicator.Bind(endpoint, 100);
            Dictionary<Connection, Task<SendTuple>> sends = new Dictionary<Connection, Task<SendTuple>>();
            Task.Run(() => Run(sends, _accepts.ReceiveAsync(), _disconnects.ReceiveAsync()));
        }

        private void Run(Dictionary<Connection, Task<SendTuple>> sends, Task<Connection> connected, Task<Connection> disconnected) {
            Task<Task<SendTuple>> monitor =
                sends.Count != 0 ? Task.WhenAny(sends.Values) : null;
            Task.WhenAny(monitor != null ? new Task[] {connected, monitor, disconnected} : new Task[] {connected, disconnected}).ContinueWith(t => {
                if(connected == t.Result) {
                    Connection connection = connected.Result;
                    sends.Add(connection, connection.GetSendAsync());
                    connected = _accepts.ReceiveAsync();
                    if(monitor != null) monitor.Dispose();
                    Task.Run(() => Run(sends, connected, disconnected));
                    _connected.Post(connection);
                } else if(monitor != null && monitor == t.Result) {
                    Task<SendTuple> send = monitor.Result;
                    Connection connection = send.Result.Item1;
                    ImmutableArray<byte> data = send.Result.Item2;
                    Action callback = send.Result.Item3;
                    sends.Remove(connection);
                    sends.Add(connection, connection.GetSendAsync());
                    Task.Run(() => Run(sends, connected, disconnected));
                    Communicator.Send(connection.Socket, data.ToArray(), callback);
                } else if(disconnected == t.Result) {
                    Connection connection = disconnected.Result;
                    Task<SendTuple> send = sends[connection];
                    sends.Remove(connection);
                    disconnected = _disconnects.ReceiveAsync();
                    Task.Run(() => Run(sends, connected, disconnected));
                    _disconnected.Post(connection);
                }
            });
        }

        internal void Accepted(Connection connected) {
            if(connected == null)
                throw new ArgumentNullException(nameof(connected));
            _accepts.Post(connected);
        }
        
        public async Task<Connection> ConnectedAsync() {
            return await _connected.ReceiveAsync();
        }

        public async Task<Connection> DisconnectedAsync() {
            return await _disconnected.ReceiveAsync();
        }

        internal void Receive(Connection sender, byte[] data) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            Task.Run(() => {
                sender.PostReceive(data.ToImmutableArray());
            });
        }

        internal void Disconnect(Connection disconnected) {
            if(disconnected == null)
                throw new ArgumentNullException(nameof(disconnected));
            _disconnects.Post(disconnected);
        }

        internal void Close(Connection connection) {

        }
    }
}


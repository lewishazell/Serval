using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Serval.Communication.Tcp;

namespace Serval.Channels.Tcp {
    using SendTuple = Tuple<Connection, byte[], Action<bool>>;
    using ReceiveTuple = Tuple<Connection, byte[]>;
    
    public sealed class TcpChannel : Channel {
        private readonly BufferBlock<Connection> _accepts = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _connected = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnects = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnected = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnecting = new BufferBlock<Connection>();
        private readonly BufferBlock<ReceiveTuple> _received = new BufferBlock<ReceiveTuple>();
        private readonly BufferBlock<SendTuple> _sending = new BufferBlock<SendTuple>();
        
        private TcpCommunicator Communicator {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs, int listenBacklog) : base(endpoint, buffers, bufferSize, eventArgs) {
            Communicator = new TcpCommunicator(this);
            Communicator.Bind(endpoint, listenBacklog);
            Dictionary<Connection, Task<SendTuple>> sends = new Dictionary<Connection, Task<SendTuple>>();
            Run();
        }

        private async void Run() {
            Task<Connection> connected = _accepts.ReceiveAsync();
            Task<Connection> disconnected = _disconnects.ReceiveAsync();
            Task<SendTuple> sending = _sending.ReceiveAsync();
            Task<Connection> disconnecting = _disconnecting.ReceiveAsync();
            Task[] tasks = new Task[] { connected, sending, disconnected, disconnecting };
            while(true) {
                Task t = await Task.WhenAny(tasks);
                if(t == connected) {
                    Connection connection = connected.Result;
                    tasks[0] = connected = _accepts.ReceiveAsync();
                    Task.Run(() => _connected.Post(connection));
                }else if(t == sending) {
                    Connection connection = sending.Result.Item1;
                    byte[] data = sending.Result.Item2;
                    Action<bool> callback = sending.Result.Item3;
                    tasks[1] = sending = _sending.ReceiveAsync();
                    Communicator.Send(connection.Socket, data.ToArray(), callback);
                }else if(t == disconnected) {
                    Connection connection = disconnected.Result;
                    connection.Socket.Close();
                    tasks[2] = disconnected = _disconnects.ReceiveAsync();
                    Task.Run(() => _disconnected.Post(connection));
                }else if(t == disconnecting) {
                    Connection connection = disconnecting.Result;
                    Communicator.Disconnect(connection.Socket);
                    tasks[3] = disconnecting = _disconnecting.ReceiveAsync();
                }
            }
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
                _received.Post(new Tuple<Connection, byte[]>(sender, data));
            });
        }
        
        public async Task<Tuple<Connection, byte[]>> ReceiveAsync() {
            return await _received.ReceiveAsync();
        }
        
        public Task<Tuple<Connection, bool>> SendAsync(Connection connection, byte[] data) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            TaskCompletionSource<Tuple<Connection, bool>> tcs = new TaskCompletionSource<Tuple<Connection, bool>>();
            _sending.Post(new SendTuple(connection, data, successful => { 
                Task.Run(() => {
                    tcs.SetResult(new Tuple<Connection, bool>(connection, successful));
                }); 
            }));
            return tcs.Task;
        }

        internal void Disconnected(Connection disconnected) {
            if(disconnected == null)
                throw new ArgumentNullException(nameof(disconnected));
            _disconnects.Post(disconnected);
        }

        public void DisconnectAsync(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _disconnecting.Post(connection);
        }
    }
}


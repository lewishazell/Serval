using System;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using Serval.Communication.Tcp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Serval.Channels.Tcp {
    using SendTuple = Tuple<Connection, byte[], TaskCompletionSource<Connection>>;
    using ReceiveTuple = Tuple<Connection, byte[]>;
    
    public sealed class TcpChannel : Channel {
        private readonly BufferBlock<Connection> _connected = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnected = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _disconnecting = new BufferBlock<Connection>();
        private readonly BufferBlock<SendTuple> _sending = new BufferBlock<SendTuple>();
        
        private TcpCommunicator Communicator {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs, int listenBacklog) : base(endpoint, buffers, bufferSize, eventArgs) {
            Communicator = new TcpCommunicator(buffers, bufferSize, eventArgs, AddressFamily.InterNetwork);
            Communicator.Bind(endpoint, listenBacklog);
            Dictionary<Connection, Task<SendTuple>> sends = new Dictionary<Connection, Task<SendTuple>>();
            Run();
        }

        private async void Run() {
            Task<Socket> connected = Communicator.AcceptedAsync();
            Task<object> disconnected = Communicator.DisconnectedAsync();
            Task<SendTuple> sending = _sending.ReceiveAsync();
            Task<Connection> disconnecting = _disconnecting.ReceiveAsync();
            Task[] tasks = new Task[] {connected, sending, disconnected, disconnecting};
            while(true) {
                Task t = await Task.WhenAny(tasks);
                if(t == connected) {
                    Socket socket = connected.Result;
                    Connection connection = new Connection(socket);
                    tasks[0] = connected = Communicator.AcceptedAsync();
                    _connected.Post(connection);
                    Communicator.Read(socket, connection);
                } else if(t == sending) {
                    Connection connection = sending.Result.Item1;
                    byte[] data = sending.Result.Item2;
                    TaskCompletionSource<Connection> tcs = sending.Result.Item3;
                    tasks[1] = sending = _sending.ReceiveAsync();
                    try {
                        Communicator.Send(connection.Socket, data.ToArray(), connection)
                            .ContinueWith(sent => { tcs.SetResult((Connection) sent.Result); });
                    } catch(Exception ex) {
                        tcs.SetException(ex);
                    }
                } else if(t == disconnected) {
                    Connection connection = (Connection) disconnected.Result;
                    connection.Socket.Close();
                    tasks[2] = disconnected = Communicator.DisconnectedAsync();
                    _disconnected.Post(connection);
                } else if(t == disconnecting) {
                    Connection connection = disconnecting.Result;
                    Communicator.Disconnect(connection.Socket);
                    tasks[3] = disconnecting = _disconnecting.ReceiveAsync();
                }
            }
        }
        
        public async Task<Connection> ConnectedAsync() {
            return await _connected.ReceiveAsync();
        }

        public async Task<Connection> DisconnectedAsync() {
            return await _disconnected.ReceiveAsync();
        }
        
        public async Task<ReceiveTuple> ReceiveAsync() {
            Tuple<object, byte[]> tuple = await Communicator.ReceivedAsync();
            return new ReceiveTuple((Connection) tuple.Item1, tuple.Item2);
        }
        
        public Task<Connection> SendAsync(Connection connection, byte[] data) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            TaskCompletionSource<Connection> tcs = new TaskCompletionSource<Connection>();
            _sending.Post(new SendTuple(connection, data, tcs));
            return tcs.Task;
        }

        public void DisconnectAsync(Connection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            _disconnecting.Post(connection);
        }
    }
}


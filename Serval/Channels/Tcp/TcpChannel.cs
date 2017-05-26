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
    public sealed class TcpChannel : Channel {
        private BufferBlock<Connection> Connects { get; } = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _connected = new BufferBlock<Connection>();
        
        private TcpCommunicator Communicator {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs) : base(endpoint, buffers, bufferSize, eventArgs) {
            Communicator = new TcpCommunicator(this);
            Communicator.Bind(endpoint, 100);
            Run();
        }

        private void Run() {
            Task.Run(() => {
                List<Task<Tuple<Connection, ImmutableArray<byte>>>> sends = new List<Task<Tuple<Connection, ImmutableArray<byte>>>>();
                Task<Connection> connected = _connected.ReceiveAsync();
                Task<Task<Tuple<Connection, ImmutableArray<byte>>>> monitor = null;
                while(true) {
                    Task task = Task.WhenAny(monitor != null ? new Task[] { connected, monitor } : new Task[] { connected }).Result;
                    if(connected == task) {
                        sends.Add(connected.Result.Sends.ReceiveAsync());
                        Connects.Post(connected.Result);
                        connected = _connected.ReceiveAsync();
                    } else if(monitor != null && monitor == task) {
                        Task<Tuple<Connection, ImmutableArray<byte>>> send = monitor.Result;
                        sends.Remove(send);
                        Communicator.Send(send.Result.Item1.Socket, send.Result.Item2.ToArray());
                        sends.Add(send.Result.Item1.Sends.ReceiveAsync());
                    }
                    if(monitor != null) monitor.Dispose();
                    if(sends.Count != 0) monitor = Task.WhenAny(sends);
                }
            });
        }

        internal void Connected(Connection connected) {
            if(connected == null)
                throw new ArgumentNullException(nameof(connected));
            _connected.Post(connected);
        }
        
        public async Task<Connection> ConnectedAsync() {
            return await Connects.ReceiveAsync();
        }

        internal void Receive(Connection sender, byte[] data) {
            if(sender == null)
                throw new ArgumentNullException(nameof(sender));
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            sender.Receives.Post(data);
        }

        internal void Disconnect(Connection disconnected) {

        }

        internal void Close(Connection connection) {

        }
    }
}


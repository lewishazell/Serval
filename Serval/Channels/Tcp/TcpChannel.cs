﻿using System;
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
        private BufferBlock<Connection> Connects { get; } = new BufferBlock<Connection>();
        private readonly BufferBlock<Connection> _connected = new BufferBlock<Connection>();
        
        private TcpCommunicator Communicator {
            get;
        }

        public TcpChannel(IPEndPoint endpoint, int buffers, int bufferSize, int eventArgs) : base(endpoint, buffers, bufferSize, eventArgs) {
            Communicator = new TcpCommunicator(this);
            Communicator.Bind(endpoint, 100);
            List<Task<SendTuple>> sends = new List<Task<SendTuple>>();
            Task<Connection> connected = _connected.ReceiveAsync();
            Task.Run(() => Run(sends, connected));
        }

        private void Run(List<Task<SendTuple>> sends, Task<Connection> connected) {
            Task<Task<SendTuple>> monitor =
                sends.Count != 0 ? Task.WhenAny(sends) : null;
            Task.WhenAny(monitor != null ? new Task[] {connected, monitor} : new Task[] {connected}).ContinueWith(t => {
                if(connected == t.Result) {
                    Connection connection = connected.Result;
                    sends.Add(connection.GetSendAsync());
                    connected = _connected.ReceiveAsync();
                    if(monitor != null) monitor.Dispose();
                    Task.Run(() => Run(sends, connected));
                    Connects.Post(connection);
                } else if(monitor != null && monitor == t.Result) {
                    Task<SendTuple> send = monitor.Result;
                    Connection connection = send.Result.Item1;
                    ImmutableArray<byte> data = send.Result.Item2;
                    Action callback = send.Result.Item3;
                    sends.Remove(send);
                    sends.Add(connection.GetSendAsync());
                    Task.Run(() => Run(sends, connected));
                    Communicator.Send(connection.Socket, data.ToArray(), callback);
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
            Task.Run(() => {
                sender.PostReceive(data.ToImmutableArray());
            });
        }

        internal void Disconnect(Connection disconnected) {

        }

        internal void Close(Connection connection) {

        }
    }
}


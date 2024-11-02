﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace LanCloud.Servers.Wjp
{
    public class WjpProxy : IDisposable
    {
        public WjpProxy(IWjpProxyConfig config)
        {
            Config = config;
            Thread = new Thread(new ThreadStart(Start));
            Thread.Start();
        }
        private IWjpProxyConfig Config { get; }
        private Thread Thread { get; }

        ConcurrentQueue<WjpProxyQueueItem> Queue { get; } = new ConcurrentQueue<WjpProxyQueueItem>();
        AutoResetEvent Enqueued { get; } = new AutoResetEvent(false);
        public bool Stop { get; set; }
        public bool Connected { get; private set; }
        public event EventHandler<EventArgs> StateChanged;

        private void Start()
        {
            while (!Stop)
            {
                using (var client = new TcpClient(Config.Hostname, Config.Port))
                using (var stream = client.GetStream())
                using (var reader = new BinaryReader(stream))
                using (var writer = new BinaryWriter(stream))
                {
                    while (client.Connected)
                    {
                        if (!Connected)
                        {
                            Connected = true;
                            StateChanged?.Invoke(this, null);
                        }
                        if (Enqueued.WaitOne(1000))
                        {
                            while (Queue.TryDequeue(out WjpProxyQueueItem requestItem))
                            {
                                writer.Write(requestItem.Request.MessageType);
                                writer.Write(requestItem.Request.Json);
                                writer.Write(Convert.ToInt32(requestItem.Request.Data?.Length ?? -1));
                                if (requestItem.Request.Data != null)
                                {
                                    writer.Write(requestItem.Request.Data);
                                }
                                var response = reader.ReadString();
                                var datalength = reader.ReadInt32();
                                byte[] data = null;
                                if (datalength >= 0)
                                {
                                    data = reader.ReadBytes(datalength);
                                }
                                requestItem.Response = new WjpResponse(response, data);
                                requestItem.Done.Set();
                            }
                        }
                    }
                }
                if (Connected)
                {
                    Connected = false;
                    StateChanged?.Invoke(this, null);
                }

                Thread.Sleep(5000);
            }
        }

        public WjpResponse SendRequest(WjpRequest request)
        {
            var queueItem = new WjpProxyQueueItem(request);
            Queue.Enqueue(queueItem);
            Enqueued.Set();
            if (!queueItem.Done.WaitOne(10000))
                throw new Exception("Timeout occured");
            return queueItem.Response;
        }
        public void Dispose()
        {
            Stop = true;
            if (Thread.CurrentThread != Thread) // Kan dat?
                Thread.Join();
        }

    }
}
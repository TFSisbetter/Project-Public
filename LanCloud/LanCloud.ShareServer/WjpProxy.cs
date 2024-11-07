﻿using LanCloud.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace LanCloud.Servers.Wjp
{
    public class WjpProxy : IDisposable
    {
        private string _Status { get; set; }
        public string Status
        {
            get => _Status;
            set
            {
                _Status = value;
                Application.StatusChanged();
            }
        }

        public WjpProxy(IWjpProxyConfig config, IWjpApplication application, ILogger logger)
        {
            Config = config;
            Application = application;
            Logger = logger;

            Thread = new Thread(new ThreadStart(Start));
            Thread.Start();
        }
        private IWjpProxyConfig Config { get; }
        public IWjpApplication Application { get; }
        public ILogger Logger { get; }
        private Thread Thread { get; }

        ConcurrentQueue<WjpProxyQueueItem> Queue { get; } = new ConcurrentQueue<WjpProxyQueueItem>();
        AutoResetEvent Enqueued { get; } = new AutoResetEvent(false);
        public bool Stop { get; set; }
        public bool Connected { get; private set; }
        public event EventHandler<EventArgs> StateChanged;

        public string HostName => Config.HostName;
        public int Port => Config.Port;

        public long ResponseTime { get; private set; }

        private void Start()
        {
            while (!Stop)
            {
                try
                {
                    Status = Logger.Info($"Trying to connect to {Config.HostName}:{Config.Port}");
                    using (var client = new TcpClient(Config.HostName, Config.Port))
                    using (var stream = client.GetStream())
                    using (var reader = new BinaryReader(stream))
                    using (var writer = new BinaryWriter(stream))
                    {
                        while (client.Connected && !Stop)
                        {
                            if (!Connected)
                            {
                                Connected = true;
                                new Thread(new ThreadStart(SendStateChanged)).Start();
                                Status = Logger.Info($"Connected to {Config.HostName}:{Config.Port}");
                            }

                            Stopwatch sw = Stopwatch.StartNew();
                            writer.Write(-1);
                            if (reader.ReadInt32() != -1)
                            {
                                break;
                            }
                            ResponseTime = sw.ElapsedMilliseconds;

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
                }
                catch (SocketException ex)
                {
                    Status = Logger.Error(ex);
                }
                catch (EndOfStreamException ex)
                {
                    Status =Logger.Error(ex);
                }

                if (Connected)
                {
                    Connected = false;
                    new Thread(new ThreadStart(SendStateChanged)).Start();
                    Status = Logger.Info($"Not connected to {Config.HostName}:{Config.Port}");
                }
                Status = Logger.Info($"Waiting to retry to {Config.HostName}:{Config.Port}");
                Thread.Sleep(10000);
            }
        }

        private void SendStateChanged()
        {
            StateChanged?.Invoke(this, null);
        }

        public WjpResponse SendRequest(WjpRequest request)
        {
            var queueItem = new WjpProxyQueueItem(request);
            Queue.Enqueue(queueItem);
            Enqueued.Set();
            if (!queueItem.Done.WaitOne(100000))
                throw new Exception("Timeout occured");
            return queueItem.Response;
        }

        public void Dispose()
        {
            Stop = true;
            if (Connected)
            {
                if (Thread.CurrentThread != Thread)
                {
                    Thread.Join();
                }
            }
            else
            {
                // It is waiting on the connection to start, just kill the thread
                Thread.Abort();
            }
            Status = Logger.Info("Disposed");
        }
    }
}

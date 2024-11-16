﻿using LanCloud.Domain.Application;
using LanCloud.Models;
using LanCloud.Shared.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace LanCloud.Domain.IO.Reader
{
    public class FileStripeReader : IDisposable
    {
        public FileStripeReader(ReconstructBuffer fileBitsBuffer, FileStripe fileBit, ILogger logger)
        {
            if (fileBit == null)
            {
                Exception = new Exception("FileBit not found");
                return;
            }

            FileBitsBuffer = fileBitsBuffer;
            FileBit = fileBit;
            Logger = logger;

            Buffer = new DoubleBuffer(Application.FileBitBufferSize);

            Stopwatch = Stopwatch.StartNew();
            Stream = FileBit.OpenRead();

            Thread = new Thread(new ThreadStart(Start));
            Thread.Start();
        }

        public ReconstructBuffer FileBitsBuffer { get; }
        public FileStripe FileBit { get; }
        public ILogger Logger { get; }
        public DoubleBuffer Buffer { get; }
        public Stopwatch Stopwatch { get; }
        public FileStream Stream { get; }
        private Thread Thread { get; }
        private AutoResetEvent StartNext { get; } = new AutoResetEvent(true);
        private AutoResetEvent BufferIsWritten { get; } = new AutoResetEvent(false);
        private bool EndOfFile { get; set; } = false;
        public long BufferReads { get; private set; }
        public bool KillSwitch { get; private set; }
        private Queue<long> Speeds { get; } = new Queue<long>(16);
        public Exception Exception { get; private set; }

        public LocalApplication Application => FileBitsBuffer.Application;

        public long Speed { get; private set; }
        private void AddSpeed(long speed)
        {
            while (Speeds.Count > 16)
            {
                Speeds.Dequeue();
            }
            Speeds.Enqueue(speed);
            Speed = Speeds.Sum() / Speeds.Count;
        }

        public byte[] Indexes => FileBit.Indexes;

        private void Start()
        {
            try
            {
                while (!KillSwitch)
                {
                    if (StartNext.WaitOne(100))
                    {
                        if (!EndOfFile)
                        {
                            // Do the read
                            Buffer.WriteBufferPosition = Stream.Read(Buffer.WriteBuffer, 0, Buffer.WriteBuffer.Length);

                            if (Buffer.WriteBufferPosition <= 0)
                            {
                                EndOfFile = true;
                            }
                            else
                            {
                                BufferReads++;

                                var time = Stopwatch.ElapsedTicks;
                                Stopwatch.Restart();
                                var speed = Buffer.WriteBufferPosition * Stopwatch.Frequency / time;
                                AddSpeed(speed);
                            }
                        }
                        else
                        {
                            Buffer.WriteBufferPosition = 0;
                        }

                        BufferIsWritten.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Exception = ex;
            }
        }
        public void FlipAndSeekTo(long bufferIndex)
        {
            BufferIsWritten.WaitOne();

            Buffer.Flip();

            var seekPosition = bufferIndex * Buffer.WriteBuffer.Length;
            var newPosition = Stream.Seek(seekPosition, SeekOrigin.Begin);
            var newIndex = newPosition / Buffer.WriteBuffer.Length;
            var newIndexRemainder = newPosition % Buffer.WriteBuffer.Length;
            if (newIndexRemainder > 0) throw new Exception("Not able to seek here");

            BufferReads = newIndex;

            StartNext.Set();
        }

        public void FlipBuffer(int bufferCounter)
        {
            BufferIsWritten.WaitOne();
            
            Buffer.Flip();

            StartNext.Set();
        }

        public void Dispose()
        {
            KillSwitch = true;
            if (Thread.CurrentThread != Thread)
                Thread?.Join();
            Stream?.Dispose();
        }
    }
}
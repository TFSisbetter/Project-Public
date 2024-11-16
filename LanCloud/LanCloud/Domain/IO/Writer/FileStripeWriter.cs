﻿using LanCloud.Domain.Share;
using LanCloud.Models;
using LanCloud.Shared.Log;
using System;
using System.Linq;
using System.Threading;

namespace LanCloud.Domain.IO.Writer
{
    public class FileStripeWriter
    {
        public FileStripeWriter(DoubleBuffer buffer, FileRefWriter fileRefWriter, LocalShareStripe localSharePart, ILogger logger)
        {
            Buffer = buffer;
            FileRefWriter = fileRefWriter;
            LocalShareBit = localSharePart;
            Logger = logger;
            
            FileBit = LocalShareBit.LocalShare.FileBits.CreateTempFileBit(FileRefWriter.PathInfo.Extention, localSharePart.Indexes);
            
            Thread = new Thread(new ThreadStart(Start));
            Thread.Start();

            Logger.Info($"Opened {FileBit.Info.Name} as output for parts: {string.Join(" xor ", Indexes.Select(a => $"#{a}"))}");
        }

        public FileRefWriter FileRefWriter { get; }
        public LocalShareStripe LocalShareBit { get; }
        public ILogger Logger { get; }

        public DoubleBuffer Buffer { get; }
        public FileStripe FileBit { get; }

        public Thread Thread { get; }

        public AutoResetEvent WritingIsDone { get; } = new AutoResetEvent(true);
        public AutoResetEvent StartNext { get; } = new AutoResetEvent(false);
        public int Position { get; private set; } = 0;
        private bool KillSwitch { get; set; } = false;

        public byte[] Indexes => LocalShareBit.Indexes;

        private void Start()
        {
            using (var stream = FileBit.OpenWrite())
            {
                while (!KillSwitch)
                {
                    if (StartNext.WaitOne(100))
                    {
                        if (!KillSwitch && Buffer.ReadBufferPosition > 0)
                        {
                            var data = Buffer.ReadBuffer;
                            var datalength = Buffer.ReadBufferPosition;

                            stream.Write(data, 0, datalength);
                            Position += datalength;
                        }

                        WritingIsDone.Set();
                    }
                }
            }
        }

        public FileStripe Stop(long length, string hash)
        {
            if (Thread.CurrentThread == Thread) throw new Exception("Cannot wait for own thread");

            KillSwitch = true;
            StartNext.Set();
            Thread.Join();

            FileBit.Update(length, hash);
            LocalShareBit.LocalShare.FileBits.AddFileBit(FileBit);
            return FileBit;
        }

    }
}
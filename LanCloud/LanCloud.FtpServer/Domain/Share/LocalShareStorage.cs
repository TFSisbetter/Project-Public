﻿using LanCloud.Configs;
using LanCloud.Services;
using LanCloud.Shared.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LanCloud.Domain.Share
{
    public class LocalShareStorage //: IDisposable
    {
        public LocalShareStorage(LocalShareConfig config, ILogger logger)
        {
            Config = config;
            Logger = logger;

            HashService = new HashService();
            Root = new DirectoryInfo(Config.FullName);
            if (!Root.Exists) Root.Create();
            _FileBits = Root
                .GetFiles("*.filebit")
                .Select(file => new FileBit(file))
                .ToList();

            Logger.Info($"Loaded");
        }

        private LocalShareConfig Config { get; }
        private ILogger Logger { get; }
        private HashService HashService { get; }
        public DirectoryInfo Root { get; }
        private List<FileBit> _FileBits { get; }

        //public Thread Thread { get; }
        //private bool KillSwitch { get; set; }

        public FileBit[] FileBits
        {
            get
            {
                lock (_FileBits)
                {
                    return _FileBits.ToArray();
                }
            }
        }
        public FileBit AddFileBit(string path, long part, byte[] data, long originalSize)
        {
            var extention = path.Split('.').Last();
            string hash = HashService.CalculateHash(data);

            var duplicate = FileBits.FirstOrDefault(a =>
                a.Information.Extention == extention &&
                a.Information.Part == part &&
                a.Information.OriginalSize == originalSize &&
                a.Information.Hash == hash);
            if (duplicate != null)
            {
                var paths = duplicate.Information.Paths.ToList();
                paths.Add(path);
                duplicate.Information.Paths = paths.ToArray();
                duplicate.SaveInformation();
            }

            var information = new FileBitInformation()
            {
                Paths = new string[] { path },
                Part = part,
                Size = data.Length,
                Extention = extention,
                OriginalSize = originalSize,
                Hash = hash
            };

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var jsonFullname = Path.Combine(Root.FullName, $"{guid}.json");
            var jsonFileInfo = new FileInfo(jsonFullname);


            //var jsonFile

            var bit = new FileBit(jsonFileInfo, information, data);
            lock (_FileBits)
            {
                _FileBits.Add(bit);
            }
            return bit;
        }


        //public void Dispose()
        //{
        //    KillSwitch = true;
        //    if (Thread != Thread.CurrentThread)
        //    {
        //        Thread.Join();
        //    }
        //}
    }
}
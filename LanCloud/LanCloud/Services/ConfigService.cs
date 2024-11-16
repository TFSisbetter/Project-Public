﻿using LanCloud.Models.Configs;
using LanCloud.Shared.Log;
using Newtonsoft.Json;
using System.IO;

namespace LanCloud.Services
{
    public class ConfigService
    {
        public ConfigService(string currentDirectory, ILogger logger)
        {
            Logger = logger;
            Fullname = Path.Combine(currentDirectory, "LanCloud.json");
        }

        public string Fullname { get; }
        public ILogger Logger { get; }

        public ApplicationConfig Load()
        {
            if (!File.Exists(Fullname))
            {
                Logger.Info("No config found, creating dummy config");
                var config = new ApplicationConfig()
                {
                    HostName = "WJPC2",
                    RefDirectoryName = "E:\\Test\\Ref",
                    FileBitBufferSize = 1024 * 4,
                    FtpBufferSize = 1024 * 4,
                    WjpBufferSize = 1024 * 16,
                    Servers = new RemoteApplicationConfig[]
                    {
                        new RemoteApplicationConfig()
                        {
                            HostName = "192.168.178.67",
                            Port = 8080,
                            IsThisComputer = true
                        },
                        new RemoteApplicationConfig()
                        {
                            HostName = "192.168.178.32",
                            Port = 8080,
                            IsThisComputer = false
                        }
                    },
                    Shares = new LocalShareConfig[]
                    {
                        new LocalShareConfig()
                        {
                            DirectoryName = "E:\\Test\\0",
                            Parts = new LocalShareBitConfig[]
                            {
                                new LocalShareBitConfig(0)
                            }
                        },
                        new LocalShareConfig()
                        {
                            DirectoryName = "E:\\Test\\1",
                            Parts = new LocalShareBitConfig[]
                            {
                                new LocalShareBitConfig(1)
                            }
                        },
                        new LocalShareConfig()
                        {
                            DirectoryName = "E:\\Test\\P",
                            Parts = new LocalShareBitConfig[]
                            {
                                new LocalShareBitConfig(new int[] { 0, 1 })
                            }
                        }
                    },

                };
                Save(config);
                return config;
            }

            Logger.Info("Config found, reading config settings");
            using (var reader = new StreamReader(Fullname))
            {
                var json = reader.ReadToEnd();
                var config = JsonConvert.DeserializeObject<ApplicationConfig>(json);

                if ((config.Servers == null || config.Servers.Length == 0) &&
                    (config.Shares == null || config.Shares.Length == 0))
                    throw new System.Exception("Nothing is configured, please setup LanCloud.config file.");

                return config;
            }
        }

        public void Save(ApplicationConfig config)
        {
            Logger.Info("Saving the config");

            if (File.Exists(Fullname))
            {
                File.Delete(Fullname);
            }

            var json = JsonConvert.SerializeObject(config);
            using (var writer = new StreamWriter(Fullname))
            {
                writer.Write(json);
            }
        }
    }
}
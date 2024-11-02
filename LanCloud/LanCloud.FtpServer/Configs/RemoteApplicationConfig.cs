﻿using LanCloud.Servers.Wjp;

namespace LanCloud.Configs
{
    public class RemoteApplicationConfig : IWjpProxyConfig
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool IsThisComputer { get; set; }
    }
}
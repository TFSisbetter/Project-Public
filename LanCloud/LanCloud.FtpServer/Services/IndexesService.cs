﻿using System.Linq;

namespace LanCloud.Services
{
    public static class IndexesService
    {
        public static bool Matches(this byte[] thisOne, byte[] compareTo)
        {
            return thisOne.Length == compareTo.Length &&
                thisOne.All(a => compareTo.Any(b => a == b));
        }
    }
}
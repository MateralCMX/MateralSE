namespace VRage.Compression
{
    using System;

    public enum DeflateOptionEnum : byte
    {
        Normal = 0,
        Maximum = 2,
        Fast = 4,
        SuperFast = 6,
        None = 0xff
    }
}


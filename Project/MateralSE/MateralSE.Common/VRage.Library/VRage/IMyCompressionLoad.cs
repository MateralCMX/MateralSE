namespace VRage
{
    using System;

    public interface IMyCompressionLoad
    {
        bool EndOfFile();
        byte GetByte();
        int GetBytes(int bytes, byte[] output);
        int GetInt32();
    }
}


namespace System.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class IOExceptionExtensions
    {
        public static bool IsFileLocked(this IOException e)
        {
            int num = Marshal.GetHRForException(e) & 0xffff;
            return ((num == 0x20) || (num == 0x21));
        }
    }
}


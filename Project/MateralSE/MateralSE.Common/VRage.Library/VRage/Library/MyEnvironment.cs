namespace VRage.Library
{
    using System;

    public static class MyEnvironment
    {
        public static bool Is64BitProcess =>
            Environment.Is64BitProcess;

        public static string NewLine =>
            Environment.NewLine;

        public static int ProcessorCount =>
            Environment.ProcessorCount;

        public static int TickCount =>
            Environment.TickCount;

        public static long WorkingSetForMyLog =>
            Environment.WorkingSet;
    }
}


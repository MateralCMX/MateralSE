namespace VRage.Scripting
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Library.Extensions;
    using VRage.Library.Threading;
    using VRage.Scripting.CompilerMethods;
    using VRage.Utils;

    public static class MyModWatchdog
    {
        private static int ModIdAllocator = 0;
        public static ModInfoT[] ModInfo;
        private static SpinLockRef ModInfoLock = new SpinLockRef();
        [ThreadStatic]
        private static RuntimeInfoT RuntimeInfo;

        static MyModWatchdog()
        {
            Warnings = new ConcurrentDictionary<long, MyTuple<string, MyStringId>>();
        }

        public static int AllocateModId(string modName)
        {
            if (modName == null)
            {
                modName = "No Name";
            }
            using (ModInfoLock.Acquire())
            {
                Warnings.Clear();
                ModIdAllocator++;
                int modIdAllocator = ModIdAllocator;
                MyArrayHelpers.InitOrReserve<ModInfoT>(ref ModInfo, modIdAllocator + 1, 0x400, 1.5f);
                ModInfo[modIdAllocator] = new ModInfoT(modName);
                ModPerfCounter.InitModInfo(modIdAllocator);
                return modIdAllocator;
            }
        }

        public static void Init(Thread updateThread)
        {
            ModPerfCounter.InitializeUpdateThread(updateThread);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ModMethodEnter(int modId)
        {
            int* numPtr1 = (int*) ref RuntimeInfo.CallStackDepth;
            int num = numPtr1[0];
            numPtr1[0] = num + 1;
            if (num == 0)
            {
                RuntimeInfo.RootModId = modId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ModMethodExit()
        {
            int* numPtr1 = (int*) ref RuntimeInfo.CallStackDepth;
            int num = numPtr1[0];
            numPtr1[0] = num - 1;
            int num2 = num;
        }

        public static bool ReportIncorrectBehaviour(MyStringId message)
        {
            int rootModId = RuntimeInfo.RootModId;
            int num2 = message.Id | rootModId;
            if (Warnings.ContainsKey((long) num2))
            {
                return false;
            }
            string friendlyName = ModInfo[rootModId].FriendlyName;
            if (Warnings.TryAdd((long) num2, MyTuple.Create<string, MyStringId>(friendlyName, message)))
            {
                return false;
            }
            string format = string.Format(MyTexts.GetString(message), friendlyName);
            MyLog.Default.Log(MyLogSeverity.Error, format, Array.Empty<object>());
            return true;
        }

        public static ConcurrentDictionary<long, MyTuple<string, MyStringId>> Warnings
        {
            [CompilerGenerated]
            get => 
                <Warnings>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Warnings>k__BackingField = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ModInfoT
        {
            public readonly string FriendlyName;
            public ModInfoT(string name)
            {
                this.FriendlyName = name;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RuntimeInfoT
        {
            public int RootModId;
            public int CallStackDepth;
        }
    }
}


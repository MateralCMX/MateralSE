namespace VRage.Scripting.CompilerMethods
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Library.Extensions;
    using VRage.Scripting;

    public static class ModPerfCounter
    {
        private static Thread MainThread;
        private static int[] MainThreadCallStackDepth;

        private static string BlockName(int modId, string memberName, int line)
        {
            string friendlyName = MyModWatchdog.ModInfo[modId].FriendlyName;
            if (friendlyName.Length > 5)
            {
                friendlyName = friendlyName.Substring(0, 5);
            }
            return $"{friendlyName}_{memberName}:{line}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EnterMethod(int modId)
        {
            MyModWatchdog.ModMethodEnter(modId);
            if (IsMainThread)
            {
                int* numPtr1 = (int*) ref MainThreadCallStackDepth[modId];
                int num = numPtr1[0];
                numPtr1[0] = num + 1;
                if (num == 0)
                {
                    MySimpleProfiler.Begin(MyModWatchdog.ModInfo[modId].FriendlyName, MySimpleProfiler.ProfilingBlockType.MOD, "EnterMethod");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnterMethod_Profile(int modId, [CallerMemberName] string memberName = "", [CallerLineNumber] int line = 0)
        {
            EnterMethod(modId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ExitMethod(int modId)
        {
            MyModWatchdog.ModMethodExit();
            if (IsMainThread)
            {
                int* numPtr1 = (int*) ref MainThreadCallStackDepth[modId];
                int num = numPtr1[0] - 1;
                numPtr1[0] = num;
                if (num == 0)
                {
                    MySimpleProfiler.EndNoMemberPairingCheck();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExitMethod_Profile(int modId, [CallerMemberName] string memberName = "", [CallerLineNumber] int line = 0)
        {
            ExitMethod(modId);
        }

        internal static void InitializeUpdateThread(Thread updateThread)
        {
            MainThread = updateThread;
        }

        public static void InitModInfo(int modId)
        {
            MyArrayHelpers.InitOrReserve<int>(ref MainThreadCallStackDepth, modId + 1, 0x400, 1.5f);
            MainThreadCallStackDepth[modId] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReenterYieldMethod(int modId)
        {
            EnterMethod(modId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReenterYieldMethod_Profile(int modId, [CallerMemberName] string memberName = "", [CallerLineNumber] int line = 0)
        {
            ReenterYieldMethod(modId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T YieldGuard<T>(int modId, T value)
        {
            ExitMethod(modId);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T YieldGuard_Profile<T>(int modId, T value, [CallerMemberName] string memberName = "", [CallerLineNumber] int line = 0) => 
            YieldGuard<T>(modId, value);

        private static bool IsMainThread =>
            ReferenceEquals(Thread.CurrentThread, MainThread);
    }
}


namespace VRage
{
    using System;
    using System.Diagnostics;

    public static class MyCompilationSymbols
    {
        public const bool ProfileFromStart = false;
        public const bool PerformanceProfiling = false;
        public const bool ProfileManagedAllocations = false;
        public const bool TracingEnabled = false;
        public const bool HavokDeepProfiling = false;
        public const string PerformanceProfilingSymbol = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";
        public const bool ProfileRenderMessages = false;
        public const bool EnableSharpDxObjectTracking = false;
        public static bool EnableNetworkPacketTracking = false;
        public static bool EnableNetworkClientUpdateTracking = false;
        public static bool EnableNetworkPositionTracking = false;
        public static bool EnableNetworkServerIncomingPacketTracking = false;
        public static bool EnableNetworkServerOutgoingPacketTracking = false;
        public const bool DX11Debug = false;
        public static bool DX11DebugOutput = true;
        public static bool DX11LogOutput = true;
        public const bool AftermathEnabled = true;
        public const bool DX11DebugOutputEnableInfo = false;
        public const bool CreateRefenceDevice = false;
        public const bool DX11ForceStereo = false;
        public const bool EnableNsightDebugging = false;
        public const bool EnableNsightShaderDebugging = false;
        public const bool EnableNsightShaderPreprocessor = false;
        public const bool LogRenderGIDs = false;
        public const bool ReinterpretFormatsStoredInFiles = true;
        public const string DX11DebugSymbol = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";

        [Conditional("DEBUG")]
        private static void SetDebug(ref bool debugging)
        {
            debugging = true;
        }

        public static bool IsDebugBuild =>
            false;
    }
}


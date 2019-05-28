namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ClientReadyDataMsg
    {
        public bool UsePlayoutDelayBufferForCharacter;
        public bool UsePlayoutDelayBufferForJetpack;
        public bool UsePlayoutDelayBufferForGrids;
    }
}


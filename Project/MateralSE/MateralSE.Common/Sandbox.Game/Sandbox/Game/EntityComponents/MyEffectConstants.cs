namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct MyEffectConstants
    {
        public static float HealthTick;
        public static float HealthInterval;
        public static float MinOxygenLevelForHealthRegeneration;
        public static float GenericHeal;
        public static float MedRoomHeal;
        static MyEffectConstants()
        {
            HealthTick = 0.4166667f;
            HealthInterval = 1f;
            MinOxygenLevelForHealthRegeneration = 0.75f;
            GenericHeal = -0.075f;
            MedRoomHeal = 5f * GenericHeal;
        }
    }
}


namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyToolHitCondition
    {
        public string[] EntityType;
        public string Animation;
        public float AnimationTimeScale;
        public string StatsAction;
        public string StatsActionIfHit;
        public string StatsModifier;
        public string StatsModifierIfHit;
        public string Component;
    }
}


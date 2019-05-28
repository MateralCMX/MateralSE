namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct CubeBlockEffect
    {
        public string Name;
        public string Origin;
        public float Delay;
        public bool Loop;
        public float SpawnTimeMin;
        public float SpawnTimeMax;
        public float Duration;
        public CubeBlockEffect(string Name, string Origin, float Delay, bool Loop, float SpawnTimeMin, float SpawnTimeMax, float Duration)
        {
            this.Name = Name;
            this.Origin = Origin;
            this.Delay = Delay;
            this.Loop = Loop;
            this.SpawnTimeMin = SpawnTimeMin;
            this.SpawnTimeMax = SpawnTimeMax;
            this.Duration = Duration;
        }

        public CubeBlockEffect(MyObjectBuilder_CubeBlockDefinition.CubeBlockEffect Effect)
        {
            this.Name = Effect.Name;
            this.Origin = Effect.Origin;
            this.Delay = Effect.Delay;
            this.Loop = Effect.Loop;
            this.SpawnTimeMin = Effect.SpawnTimeMin;
            this.SpawnTimeMax = Effect.SpawnTimeMax;
            this.Duration = Effect.Duration;
        }
    }
}


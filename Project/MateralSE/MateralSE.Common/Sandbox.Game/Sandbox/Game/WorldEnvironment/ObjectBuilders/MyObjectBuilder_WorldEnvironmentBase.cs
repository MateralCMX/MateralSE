namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using VRage.Game;

    public abstract class MyObjectBuilder_WorldEnvironmentBase : MyObjectBuilder_DefinitionBase
    {
        public double SectorSize = 64.0;
        public double ItemsPerSqMeter = 0.0017;
        public int MaxSyncLod = 1;

        protected MyObjectBuilder_WorldEnvironmentBase()
        {
        }
    }
}


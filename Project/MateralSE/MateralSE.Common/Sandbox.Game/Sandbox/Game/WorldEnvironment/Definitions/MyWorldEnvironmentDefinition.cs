namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WorldEnvironmentBase), (Type) null)]
    public abstract class MyWorldEnvironmentDefinition : MyDefinitionBase
    {
        public int SyncLod;
        public MyRuntimeEnvironmentItemInfo[] Items;
        public double SectorSize;
        public double ItemDensity;

        protected MyWorldEnvironmentDefinition()
        {
        }

        public MyEnvironmentSector CreateSector() => 
            ((MyEnvironmentSector) Activator.CreateInstance(this.SectorType));

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WorldEnvironmentBase base2 = (MyObjectBuilder_WorldEnvironmentBase) builder;
            this.SectorSize = base2.SectorSize;
            this.ItemDensity = base2.ItemsPerSqMeter;
            this.SyncLod = base2.MaxSyncLod;
        }

        public abstract Type SectorType { get; }
    }
}


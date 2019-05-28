namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;

    public abstract class MyEnvironmentModuleBase : IMyEnvironmentModule
    {
        protected MyLogicalEnvironmentSectorBase Sector;

        protected MyEnvironmentModuleBase()
        {
        }

        public abstract void Close();
        public virtual void DebugDraw()
        {
        }

        public abstract MyObjectBuilder_EnvironmentModuleBase GetObjectBuilder();
        public abstract void HandleSyncEvent(int logicalItem, object data, bool fromClient);
        public virtual void Init(MyLogicalEnvironmentSectorBase sector, MyObjectBuilder_Base ob)
        {
            this.Sector = sector;
        }

        public abstract void OnItemEnable(int item, bool enable);
        public virtual void ProcessItems(Dictionary<short, MyLodEnvironmentItemSet> items, int changedLodMin, int changedLodMax)
        {
            using (MyEnvironmentModelUpdateBatch batch = new MyEnvironmentModelUpdateBatch(this.Sector))
            {
                foreach (KeyValuePair<short, MyLodEnvironmentItemSet> pair in items)
                {
                    MyRuntimeEnvironmentItemInfo info;
                    this.Sector.GetItemDefinition((ushort) pair.Key, out info);
                    MyDefinitionId subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalModelCollectionDefinition), info.Subtype);
                    MyPhysicalModelCollectionDefinition definition = MyDefinitionManager.Static.GetDefinition<MyPhysicalModelCollectionDefinition>(subtypeId);
                    if (definition != null)
                    {
                        foreach (int num in pair.Value.Items)
                        {
                            float sample = MyHashRandomUtils.UniformFloatFromSeed(num);
                            MyDefinitionId modelDef = definition.Items.Sample(sample);
                            batch.Add(modelDef, num);
                        }
                    }
                }
            }
        }
    }
}


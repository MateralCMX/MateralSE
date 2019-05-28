namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyGridPickupComponent : MySessionComponentBase
    {
        public static MyGridPickupComponent Static;
        private Dictionary<MyDefinitionId, MyDefinitionId> m_blockVariationToBaseBlock;
        private Dictionary<MyDefinitionId, MyFixedPoint> m_blockMaxStackSizes;

        public MyGridPickupComponent()
        {
            Static = this;
            this.m_blockVariationToBaseBlock = new Dictionary<MyDefinitionId, MyDefinitionId>(MyDefinitionId.Comparer);
            this.m_blockMaxStackSizes = new Dictionary<MyDefinitionId, MyFixedPoint>(MyDefinitionId.Comparer);
        }

        public MyDefinitionId GetBaseBlock(MyDefinitionId id)
        {
            MyDefinitionId id2;
            return (!this.m_blockVariationToBaseBlock.TryGetValue(id, out id2) ? id : id2);
        }

        public MyFixedPoint GetMaxStackSize(MyDefinitionId id)
        {
            MyFixedPoint point;
            return (!this.m_blockMaxStackSizes.TryGetValue(id, out point) ? 1 : point);
        }

        public override void LoadData()
        {
            base.LoadData();
            this.m_blockVariationToBaseBlock = new Dictionary<MyDefinitionId, MyDefinitionId>(MyDefinitionId.Comparer);
            foreach (MyCubeBlockDefinition definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyCubeBlockDefinition>())
            {
                if (definition.BlockStages != null)
                {
                    foreach (MyDefinitionId id in definition.BlockStages)
                    {
                        this.m_blockVariationToBaseBlock[id] = definition.Id;
                    }
                }
            }
            this.m_blockMaxStackSizes = new Dictionary<MyDefinitionId, MyFixedPoint>(MyDefinitionId.Comparer);
            foreach (MyCubeBlockStackSizeDefinition definition2 in MyDefinitionManager.Static.GetDefinitions<MyCubeBlockStackSizeDefinition>())
            {
                if (definition2.BlockMaxStackSizes != null)
                {
                    foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> pair in definition2.BlockMaxStackSizes)
                    {
                        this.m_blockMaxStackSizes[pair.Key] = pair.Value;
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_blockVariationToBaseBlock = null;
            this.m_blockMaxStackSizes = null;
        }

        public override Type[] Dependencies =>
            base.Dependencies;

        public override bool IsRequiredByGame =>
            MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID;
    }
}


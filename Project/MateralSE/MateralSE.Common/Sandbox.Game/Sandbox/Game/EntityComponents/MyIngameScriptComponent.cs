namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MyProgrammableBlock), false, new string[] {  })]
    public class MyIngameScriptComponent : MyGameLogicComponent
    {
        private MyProgrammableBlock m_block;
        private UpdateType m_nextUpdate;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            this.m_block = (MyProgrammableBlock) base.Entity;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            this.NextUpdate |= UpdateType.Update1;
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation();
            this.NextUpdate |= UpdateType.Update10;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation();
            this.NextUpdate |= UpdateType.Update100;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            UpdateType nextUpdate = this.m_nextUpdate;
            this.m_nextUpdate = UpdateType.None;
            if (nextUpdate != UpdateType.None)
            {
                this.m_block.Run(string.Empty, nextUpdate);
            }
        }

        public UpdateType NextUpdate
        {
            get => 
                this.m_nextUpdate;
            set
            {
                if (value != UpdateType.None)
                {
                    this.m_nextUpdate |= value;
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                else
                {
                    this.m_nextUpdate = value;
                    base.NeedsUpdate = MyEntityUpdateEnum.NONE;
                }
            }
        }
    }
}


namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;

    public class MyMemoryEnvironmentModule : IMyEnvironmentModule
    {
        private MyLogicalEnvironmentSectorBase m_sector;
        private readonly HashSet<int> m_disabledItems = new HashSet<int>();

        public void Close()
        {
        }

        public void DebugDraw()
        {
        }

        public MyObjectBuilder_EnvironmentModuleBase GetObjectBuilder()
        {
            if (this.m_disabledItems.Count <= 0)
            {
                return null;
            }
            MyObjectBuilder_DummyEnvironmentModule module1 = new MyObjectBuilder_DummyEnvironmentModule();
            module1.DisabledItems = this.m_disabledItems;
            return module1;
        }

        public void HandleSyncEvent(int logicalItem, object data, bool fromClient)
        {
        }

        public void Init(MyLogicalEnvironmentSectorBase sector, MyObjectBuilder_Base ob)
        {
            if (ob != null)
            {
                this.m_disabledItems.UnionWith(((MyObjectBuilder_DummyEnvironmentModule) ob).DisabledItems);
            }
            this.m_sector = sector;
        }

        public void OnItemEnable(int itemId, bool enabled)
        {
            if (enabled)
            {
                this.m_disabledItems.Remove(itemId);
            }
            else
            {
                this.m_disabledItems.Add(itemId);
            }
            this.m_sector.InvalidateItem(itemId);
        }

        public void ProcessItems(Dictionary<short, MyLodEnvironmentItemSet> items, int changedLodMin, int changedLodMax)
        {
            foreach (int num in this.m_disabledItems)
            {
                this.m_sector.InvalidateItem(num);
            }
        }

        public bool NeedToSave =>
            (this.m_disabledItems.Count > 0);
    }
}


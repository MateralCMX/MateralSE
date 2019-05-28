namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;

    public interface IMyEnvironmentModule
    {
        void Close();
        void DebugDraw();
        MyObjectBuilder_EnvironmentModuleBase GetObjectBuilder();
        void HandleSyncEvent(int logicalItem, object data, bool fromClient);
        void Init(MyLogicalEnvironmentSectorBase sector, MyObjectBuilder_Base ob);
        void OnItemEnable(int item, bool enable);
        void ProcessItems(Dictionary<short, MyLodEnvironmentItemSet> items, int changedLodMin, int changedLodMax);
    }
}


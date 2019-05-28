namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Collections.Generic;

    public interface IMyEnvironmentModuleProxy
    {
        void Close();
        void CommitLodChange(int lodBefore, int lodAfter);
        void CommitPhysicsChange(bool enabled);
        void DebugDraw();
        void HandleSyncEvent(int item, object data, bool fromClient);
        void Init(MyEnvironmentSector sector, List<int> items);
        void OnItemChange(int index, short newModel);
        void OnItemChangeBatch(List<int> items, int offset, short newModel);
    }
}


namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0x3e7, typeof(MyObjectBuilder_SessionComponentAssetModifiers), (Type) null)]
    public class MySessionComponentAssetModifiers : MySessionComponentBase
    {
        private List<MyAssetModifierComponent> m_componentListForLazyUpdates = new List<MyAssetModifierComponent>();

        public void RegisterComponentForLazyUpdate(MyAssetModifierComponent comp)
        {
            this.m_componentListForLazyUpdates.Add(comp);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            int index = 0;
            while (index < this.m_componentListForLazyUpdates.Count)
            {
                bool flag = true;
                if (((this.m_componentListForLazyUpdates[index].Entity != null) && !this.m_componentListForLazyUpdates[index].Entity.Closed) && !this.m_componentListForLazyUpdates[index].Entity.MarkedForClose)
                {
                    flag = this.m_componentListForLazyUpdates[index].LazyUpdate();
                }
                if (flag)
                {
                    this.m_componentListForLazyUpdates.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }
    }
}


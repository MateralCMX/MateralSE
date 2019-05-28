namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;
    using VRageMath;

    public class MyRadioReceiver : MyDataReceiver
    {
        protected override void GetBroadcastersInMyRange(ref HashSet<MyDataBroadcaster> broadcastersInRange)
        {
            base.m_tmpBroadcasters.Clear();
            MyRadioBroadcasters.GetAllBroadcastersInSphere(new BoundingSphereD(base.Entity.PositionComp.GetPosition(), 0.5), base.m_tmpBroadcasters);
            foreach (MyDataBroadcaster broadcaster in base.m_tmpBroadcasters)
            {
                broadcastersInRange.Add(broadcaster);
            }
            MyAntennaSystem.Static.GetEntityBroadcasters(base.Entity as VRage.Game.Entity.MyEntity, ref broadcastersInRange, 0L);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            base.Enabled = true;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            base.Enabled = false;
        }
    }
}


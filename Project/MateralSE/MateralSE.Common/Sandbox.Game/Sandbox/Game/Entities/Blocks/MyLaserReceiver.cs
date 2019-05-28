namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;

    public class MyLaserReceiver : MyDataReceiver
    {
        protected override void GetBroadcastersInMyRange(ref HashSet<MyDataBroadcaster> broadcastersInRange)
        {
            foreach (MyLaserBroadcaster broadcaster in MyAntennaSystem.Static.LaserAntennas.Values)
            {
                if (!ReferenceEquals(broadcaster, base.Broadcaster))
                {
                    if (broadcaster.RealAntenna != null)
                    {
                        if (!broadcaster.RealAntenna.Enabled)
                        {
                            continue;
                        }
                        if (!broadcaster.RealAntenna.IsFunctional)
                        {
                            continue;
                        }
                        if (broadcaster.RealAntenna.ResourceSink.SuppliedRatioByType(MyResourceDistributorComponent.ElectricityId) <= 0.99f)
                        {
                            continue;
                        }
                    }
                    long? successfullyContacting = broadcaster.SuccessfullyContacting;
                    long antennaEntityId = base.Broadcaster.AntennaEntityId;
                    if ((successfullyContacting.GetValueOrDefault() == antennaEntityId) & (successfullyContacting != null))
                    {
                        broadcastersInRange.Add(broadcaster);
                    }
                }
            }
            MyAntennaSystem.Static.GetEntityBroadcasters(base.Entity as MyEntity, ref broadcastersInRange, 0L);
        }
    }
}


namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyEnvironmentalEntityCacher : MySessionComponentBase
    {
        private const long EntityPreserveTime = 0x3e8L;
        private HashSet<long> m_index;
        private MyBinaryStructHeap<long, EntityReference> m_entities;

        public MyEntity GetEntity(long entityId) => 
            (!this.m_index.Remove(entityId) ? null : this.m_entities.Remove(entityId).Entity);

        public void QueueEntity(MyEntity entity)
        {
            EntityReference reference = new EntityReference {
                Entity = entity
            };
            this.m_entities.Insert(reference, Time() + 0x3e8L);
            this.m_index.Add(entity.EntityId);
            if (base.UpdateOrder == MyUpdateOrder.NoUpdate)
            {
                base.SetUpdateOrder(MyUpdateOrder.AfterSimulation);
            }
        }

        private static long Time() => 
            (MySession.Static.ElapsedGameTime.Ticks / 0x2710L);

        public override void UpdateAfterSimulation()
        {
            long num = Time();
            while ((this.m_entities.Count > 0) && (this.m_entities.MinKey() < num))
            {
                this.m_index.Remove(this.m_entities.RemoveMin().Entity.EntityId);
            }
            if (this.m_entities.Count == 0)
            {
                base.SetUpdateOrder(MyUpdateOrder.NoUpdate);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EntityReference
        {
            public MyEntity Entity;
        }
    }
}


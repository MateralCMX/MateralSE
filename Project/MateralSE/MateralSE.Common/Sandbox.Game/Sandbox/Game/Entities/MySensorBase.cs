namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    internal class MySensorBase : MyEntity
    {
        private Stack<DetectedEntityInfo> m_unusedInfos = new Stack<DetectedEntityInfo>();
        private Dictionary<MyEntity, DetectedEntityInfo> m_detectedEntities = new Dictionary<MyEntity, DetectedEntityInfo>(new InstanceComparer<MyEntity>());
        private List<MyEntity> m_deleteList = new List<MyEntity>();
        private Action<MyPositionComponentBase> m_entityPositionChanged;
        private Action<MyEntity> m_entityClosed;
        [CompilerGenerated]
        private SensorFilterHandler Filter;
        [CompilerGenerated]
        private EntitySensorHandler EntityEntered;
        [CompilerGenerated]
        private EntitySensorHandler EntityMoved;
        [CompilerGenerated]
        private EntitySensorHandler EntityLeft;

        public event EntitySensorHandler EntityEntered
        {
            [CompilerGenerated] add
            {
                EntitySensorHandler entityEntered = this.EntityEntered;
                while (true)
                {
                    EntitySensorHandler a = entityEntered;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Combine(a, value);
                    entityEntered = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityEntered, handler3, a);
                    if (ReferenceEquals(entityEntered, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EntitySensorHandler entityEntered = this.EntityEntered;
                while (true)
                {
                    EntitySensorHandler source = entityEntered;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Remove(source, value);
                    entityEntered = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityEntered, handler3, source);
                    if (ReferenceEquals(entityEntered, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EntitySensorHandler EntityLeft
        {
            [CompilerGenerated] add
            {
                EntitySensorHandler entityLeft = this.EntityLeft;
                while (true)
                {
                    EntitySensorHandler a = entityLeft;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Combine(a, value);
                    entityLeft = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityLeft, handler3, a);
                    if (ReferenceEquals(entityLeft, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EntitySensorHandler entityLeft = this.EntityLeft;
                while (true)
                {
                    EntitySensorHandler source = entityLeft;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Remove(source, value);
                    entityLeft = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityLeft, handler3, source);
                    if (ReferenceEquals(entityLeft, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EntitySensorHandler EntityMoved
        {
            [CompilerGenerated] add
            {
                EntitySensorHandler entityMoved = this.EntityMoved;
                while (true)
                {
                    EntitySensorHandler a = entityMoved;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Combine(a, value);
                    entityMoved = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityMoved, handler3, a);
                    if (ReferenceEquals(entityMoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EntitySensorHandler entityMoved = this.EntityMoved;
                while (true)
                {
                    EntitySensorHandler source = entityMoved;
                    EntitySensorHandler handler3 = (EntitySensorHandler) Delegate.Remove(source, value);
                    entityMoved = Interlocked.CompareExchange<EntitySensorHandler>(ref this.EntityMoved, handler3, source);
                    if (ReferenceEquals(entityMoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event SensorFilterHandler Filter
        {
            [CompilerGenerated] add
            {
                SensorFilterHandler filter = this.Filter;
                while (true)
                {
                    SensorFilterHandler a = filter;
                    SensorFilterHandler handler3 = (SensorFilterHandler) Delegate.Combine(a, value);
                    filter = Interlocked.CompareExchange<SensorFilterHandler>(ref this.Filter, handler3, a);
                    if (ReferenceEquals(filter, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                SensorFilterHandler filter = this.Filter;
                while (true)
                {
                    SensorFilterHandler source = filter;
                    SensorFilterHandler handler3 = (SensorFilterHandler) Delegate.Remove(source, value);
                    filter = Interlocked.CompareExchange<SensorFilterHandler>(ref this.Filter, handler3, source);
                    if (ReferenceEquals(filter, source))
                    {
                        return;
                    }
                }
            }
        }

        public MySensorBase()
        {
            base.Save = false;
            this.m_entityPositionChanged = new Action<MyPositionComponentBase>(this.entity_OnPositionChanged);
            this.m_entityClosed = new Action<MyEntity>(this.entity_OnClose);
        }

        public bool AnyEntityWithState(EventType type) => 
            this.m_detectedEntities.Any<KeyValuePair<MyEntity, DetectedEntityInfo>>(s => (s.Value.EventType == type));

        private void entity_OnClose(MyEntity obj)
        {
            DetectedEntityInfo info;
            if (this.m_detectedEntities.TryGetValue(obj, out info))
            {
                info.EventType = EventType.Delete;
            }
        }

        private void entity_OnPositionChanged(MyPositionComponentBase entity)
        {
            DetectedEntityInfo info;
            if (this.m_detectedEntities.TryGetValue(entity.Container.Entity as MyEntity, out info))
            {
                info.Moved = true;
            }
        }

        protected bool FilterEntity(MyEntity entity)
        {
            SensorFilterHandler filter = this.Filter;
            if (filter == null)
            {
                return false;
            }
            bool processEntity = true;
            filter(this, entity, ref processEntity);
            return !processEntity;
        }

        public MyEntity GetClosestEntity(Vector3 position)
        {
            MyEntity key = null;
            double maxValue = double.MaxValue;
            foreach (KeyValuePair<MyEntity, DetectedEntityInfo> pair in this.m_detectedEntities)
            {
                double num2 = (position - pair.Key.PositionComp.GetPosition()).LengthSquared();
                if (num2 < maxValue)
                {
                    maxValue = num2;
                    key = pair.Key;
                }
            }
            return key;
        }

        private DetectedEntityInfo GetInfo() => 
            ((this.m_unusedInfos.Count != 0) ? this.m_unusedInfos.Pop() : new DetectedEntityInfo());

        public bool HasAnyMoved() => 
            this.m_detectedEntities.Any<KeyValuePair<MyEntity, DetectedEntityInfo>>(s => s.Value.Moved);

        private void raise_EntityEntered(MyEntity entity)
        {
            EntitySensorHandler entityEntered = this.EntityEntered;
            if (entityEntered != null)
            {
                entityEntered(this, entity);
            }
        }

        private void raise_EntityLeft(MyEntity entity)
        {
            EntitySensorHandler entityLeft = this.EntityLeft;
            if (entityLeft != null)
            {
                entityLeft(this, entity);
            }
        }

        private void raise_EntityMoved(MyEntity entity)
        {
            EntitySensorHandler entityMoved = this.EntityMoved;
            if (entityMoved != null)
            {
                entityMoved(this, entity);
            }
        }

        public void RaiseAllMove()
        {
            EntitySensorHandler entityMoved = this.EntityMoved;
            if (entityMoved != null)
            {
                foreach (KeyValuePair<MyEntity, DetectedEntityInfo> pair in this.m_detectedEntities)
                {
                    entityMoved(this, pair.Key);
                }
            }
        }

        protected void TrackEntity(MyEntity entity)
        {
            if (!this.FilterEntity(entity))
            {
                DetectedEntityInfo info;
                if (this.m_detectedEntities.TryGetValue(entity, out info))
                {
                    if (info.EventType == EventType.Delete)
                    {
                        info.EventType = EventType.None;
                    }
                }
                else
                {
                    entity.PositionComp.OnPositionChanged += this.m_entityPositionChanged;
                    entity.OnClose += this.m_entityClosed;
                    info = this.GetInfo();
                    info.Moved = false;
                    info.EventType = EventType.Add;
                    this.m_detectedEntities[entity] = info;
                }
            }
        }

        private void UntrackEntity(MyEntity entity)
        {
            entity.PositionComp.OnPositionChanged -= this.m_entityPositionChanged;
            entity.OnClose -= this.m_entityClosed;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            foreach (KeyValuePair<MyEntity, DetectedEntityInfo> pair in this.m_detectedEntities)
            {
                if (pair.Value.EventType == EventType.Delete)
                {
                    this.UntrackEntity(pair.Key);
                    this.raise_EntityLeft(pair.Key);
                    this.m_deleteList.Add(pair.Key);
                    this.m_unusedInfos.Push(pair.Value);
                    continue;
                }
                if (pair.Value.EventType == EventType.Add)
                {
                    this.raise_EntityEntered(pair.Key);
                }
                else if (pair.Value.Moved)
                {
                    this.raise_EntityMoved(pair.Key);
                }
                pair.Value.Moved = false;
                pair.Value.EventType = EventType.Delete;
            }
            foreach (MyEntity entity in this.m_deleteList)
            {
                this.m_detectedEntities.Remove(entity);
            }
            this.m_deleteList.Clear();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySensorBase.<>c <>9 = new MySensorBase.<>c();
            public static Func<KeyValuePair<MyEntity, MySensorBase.DetectedEntityInfo>, bool> <>9__25_0;

            internal bool <HasAnyMoved>b__25_0(KeyValuePair<MyEntity, MySensorBase.DetectedEntityInfo> s) => 
                s.Value.Moved;
        }

        private class DetectedEntityInfo
        {
            public bool Moved;
            public Sandbox.Game.Entities.MySensorBase.EventType EventType;
        }

        public enum EventType : byte
        {
            None = 0,
            Add = 1,
            Delete = 2
        }
    }
}


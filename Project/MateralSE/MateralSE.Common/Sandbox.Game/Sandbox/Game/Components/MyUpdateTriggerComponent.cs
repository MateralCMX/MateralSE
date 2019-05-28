namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;

    [MyComponentBuilder(typeof(MyObjectBuilder_UpdateTrigger), true)]
    public class MyUpdateTriggerComponent : MyTriggerComponent
    {
        private int m_size;
        private Dictionary<MyEntity, MyEntityUpdateEnum> m_needsUpdate;

        public MyUpdateTriggerComponent()
        {
            this.m_size = 100;
            this.m_needsUpdate = new Dictionary<MyEntity, MyEntityUpdateEnum>();
        }

        public MyUpdateTriggerComponent(int triggerSize)
        {
            this.m_size = 100;
            this.m_needsUpdate = new Dictionary<MyEntity, MyEntityUpdateEnum>();
            this.m_size = triggerSize;
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_UpdateTrigger trigger = builder as MyObjectBuilder_UpdateTrigger;
            this.m_size = trigger.Size;
        }

        private void DisableRecursively(MyEntity entity)
        {
            this.Enabled = false;
            this.m_needsUpdate[entity] = entity.NeedsUpdate;
            entity.NeedsUpdate = MyEntityUpdateEnum.NONE;
            entity.Render.Visible = false;
            if (entity.Hierarchy != null)
            {
                foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
                {
                    this.DisableRecursively((MyEntity) base2.Entity);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (((base.Entity != null) && !base.Entity.MarkedForClose) && (base.QueryResult.Count != 0))
            {
                this.EnableRecursively((MyEntity) base.Entity);
                this.m_needsUpdate.Clear();
            }
            this.m_needsUpdate.Clear();
        }

        private void EnableRecursively(MyEntity entity)
        {
            this.Enabled = true;
            if (this.m_needsUpdate.ContainsKey(entity))
            {
                entity.NeedsUpdate = this.m_needsUpdate[entity];
            }
            entity.Render.Visible = true;
            if (entity.Hierarchy != null)
            {
                foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
                {
                    this.EnableRecursively((MyEntity) base2.Entity);
                }
            }
        }

        private void grid_OnBlockOwnershipChanged(MyCubeGrid obj)
        {
            bool flag = false;
            foreach (long num in obj.BigOwners)
            {
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(num);
                if ((playerFaction != null) && !playerFaction.IsEveryoneNpc())
                {
                    flag = true;
                    break;
                }
            }
            foreach (long num2 in obj.SmallOwners)
            {
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(num2);
                if ((playerFaction != null) && !playerFaction.IsEveryoneNpc())
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                obj.Components.Remove<MyUpdateTriggerComponent>();
                obj.OnBlockOwnershipChanged -= new Action<MyCubeGrid>(this.grid_OnBlockOwnershipChanged);
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            MyCubeGrid entity = base.Entity as MyCubeGrid;
            if (entity != null)
            {
                entity.OnBlockOwnershipChanged += new Action<MyCubeGrid>(this.grid_OnBlockOwnershipChanged);
            }
        }

        protected override bool QueryEvaluator(MyEntity entity) => 
            ((entity.Physics != null) && (!entity.Physics.IsStatic && (!(entity is MyFloatingObject) && (!(entity is MyDebrisBase) && !ReferenceEquals(entity, base.Entity.GetTopMostParent(null))))));

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_UpdateTrigger trigger1 = base.Serialize(copy) as MyObjectBuilder_UpdateTrigger;
            trigger1.Size = this.m_size;
            return trigger1;
        }

        protected override void UpdateInternal()
        {
            if (base.Entity.Physics != null)
            {
                base.m_AABB = base.Entity.PositionComp.WorldAABB.Inflate((double) (this.m_size / 2));
                bool flag = this.m_needsUpdate.Count != 0;
                int index = base.QueryResult.Count - 1;
                while (true)
                {
                    if (index >= 0)
                    {
                        MyEntity entity = base.QueryResult[index];
                        if ((entity.Closed || !entity.PositionComp.WorldAABB.Intersects(base.m_AABB)) || (entity is MyMeteor))
                        {
                            base.QueryResult.RemoveAtFast<MyEntity>(index);
                            index--;
                            continue;
                        }
                    }
                    base.DoQuery = base.QueryResult.Count == 0;
                    base.UpdateInternal();
                    if (base.QueryResult.Count == 0)
                    {
                        if (!flag)
                        {
                            this.DisableRecursively((MyEntity) base.Entity);
                            return;
                        }
                    }
                    else if (flag)
                    {
                        this.EnableRecursively((MyEntity) base.Entity);
                        this.m_needsUpdate.Clear();
                    }
                    return;
                }
            }
        }

        public int Size
        {
            get => 
                this.m_size;
            set
            {
                this.m_size = value;
                if (base.Entity != null)
                {
                    base.m_AABB.Inflate((double) (value / 2));
                }
            }
        }

        public override string ComponentTypeDebugString =>
            "Pirate update trigger";
    }
}


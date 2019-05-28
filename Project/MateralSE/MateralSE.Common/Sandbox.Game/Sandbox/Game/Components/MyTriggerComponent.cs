namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;
    using VRageRender;

    [MyComponentBuilder(typeof(MyObjectBuilder_TriggerBase), true)]
    public class MyTriggerComponent : MyEntityComponentBase
    {
        private static uint m_triggerCounter;
        private const uint PRIME = 0x1f;
        private readonly uint m_updateOffset;
        private readonly List<MyEntity> m_queryResult;
        protected TriggerType m_triggerType;
        protected BoundingBoxD m_AABB;
        protected BoundingSphereD m_boundingSphere;
        public Vector3D DefaultTranslation;

        public MyTriggerComponent()
        {
            this.m_queryResult = new List<MyEntity>();
            this.DefaultTranslation = Vector3D.Zero;
            this.m_triggerType = TriggerType.AABB;
            this.UpdateFrequency = 300;
            m_triggerCounter++;
            this.m_updateOffset = (uint) ((m_triggerCounter * 0x1f) % this.UpdateFrequency);
            this.DoQuery = true;
        }

        public MyTriggerComponent(TriggerType type, uint updateFrequency = 300)
        {
            this.m_queryResult = new List<MyEntity>();
            this.DefaultTranslation = Vector3D.Zero;
            this.m_triggerType = type;
            this.UpdateFrequency = updateFrequency;
            m_triggerCounter++;
            this.m_updateOffset = (uint) ((m_triggerCounter * 0x1f) % this.UpdateFrequency);
            this.DoQuery = true;
        }

        public bool Contains(Vector3D point)
        {
            TriggerType triggerType = this.m_triggerType;
            return ((triggerType == TriggerType.AABB) ? (this.m_AABB.Contains(point) == ContainmentType.Contains) : ((triggerType == TriggerType.Sphere) ? (this.m_boundingSphere.Contains(point) == ContainmentType.Contains) : false));
        }

        public virtual void DebugDraw()
        {
            Color red = Color.Red;
            if (this.CustomDebugColor != null)
            {
                red = this.CustomDebugColor.Value;
            }
            if (this.m_triggerType == TriggerType.AABB)
            {
                MyRenderProxy.DebugDrawAABB(this.m_AABB, (this.m_queryResult.Count == 0) ? red : Color.Green, 1f, 1f, false, false, false);
            }
            else
            {
                MyRenderProxy.DebugDrawSphere(this.m_boundingSphere.Center, (float) this.m_boundingSphere.Radius, (this.m_queryResult.Count == 0) ? red : Color.Green, 1f, false, false, true, false);
            }
            if (base.Entity.Parent != null)
            {
                MyRenderProxy.DebugDrawLine3D(this.Center, base.Entity.Parent.PositionComp.GetPosition(), Color.Yellow, Color.Green, false, false);
            }
            foreach (MyEntity local1 in this.m_queryResult)
            {
                MyRenderProxy.DebugDrawAABB(local1.PositionComp.WorldAABB, Color.Yellow, 1f, 1f, false, false, false);
                MatrixD worldMatrix = base.Entity.WorldMatrix;
                MyRenderProxy.DebugDrawLine3D(local1.WorldMatrix.Translation, worldMatrix.Translation, Color.Yellow, Color.Green, false, false);
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_TriggerBase base2 = builder as MyObjectBuilder_TriggerBase;
            if (base2 != null)
            {
                this.m_AABB = (BoundingBoxD) base2.AABB;
                this.m_boundingSphere = (BoundingSphereD) base2.BoundingSphere;
                this.m_triggerType = (base2.Type == -1) ? TriggerType.AABB : ((TriggerType) base2.Type);
                this.DefaultTranslation = (Vector3D) base2.Offset;
            }
        }

        public virtual void Dispose()
        {
            this.m_queryResult.Clear();
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            base.Entity.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.OnEntityPositionCompPositionChanged);
            base.Entity.NeedsWorldMatrix = true;
            if (base.Entity.InScene)
            {
                MySessionComponentTriggerSystem.Static.AddTrigger(this);
            }
        }

        public override void OnAddedToScene()
        {
            MySessionComponentTriggerSystem.Static.AddTrigger(this);
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            MySessionComponentTriggerSystem.RemoveTrigger((MyEntity) base.Entity, this);
            base.Entity.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.OnEntityPositionCompPositionChanged);
            this.Dispose();
        }

        private void OnEntityPositionCompPositionChanged(MyPositionComponentBase myPositionComponentBase)
        {
            TriggerType triggerType = this.m_triggerType;
            if (triggerType == TriggerType.AABB)
            {
                Vector3D vctTranlsation = (base.Entity.PositionComp.GetPosition() - this.m_AABB.Matrix.Translation) + this.DefaultTranslation;
                this.m_AABB.Translate(vctTranlsation);
            }
            else
            {
                if (triggerType != TriggerType.Sphere)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.m_boundingSphere.Center = base.Entity.PositionComp.GetPosition() + this.DefaultTranslation;
            }
        }

        protected virtual bool QueryEvaluator(MyEntity entity) => 
            true;

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_TriggerBase base2 = base.Serialize(false) as MyObjectBuilder_TriggerBase;
            if (base2 != null)
            {
                base2.AABB = this.m_AABB;
                base2.BoundingSphere = this.m_boundingSphere;
                base2.Type = (int) this.m_triggerType;
                base2.Offset = this.DefaultTranslation;
            }
            return base2;
        }

        public void Update()
        {
            if ((((long) MySession.Static.GameplayFrameCounter) % ((ulong) this.UpdateFrequency)) == this.m_updateOffset)
            {
                this.UpdateInternal();
            }
        }

        protected virtual void UpdateInternal()
        {
            if (this.DoQuery)
            {
                this.m_queryResult.Clear();
                TriggerType triggerType = this.m_triggerType;
                if (triggerType == TriggerType.AABB)
                {
                    MyGamePruningStructure.GetTopMostEntitiesInBox(ref this.m_AABB, this.m_queryResult, MyEntityQueryType.Both);
                }
                else
                {
                    if (triggerType != TriggerType.Sphere)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref this.m_boundingSphere, this.m_queryResult, MyEntityQueryType.Both);
                }
                int index = 0;
                while (index < this.m_queryResult.Count)
                {
                    MyEntity entity = this.m_queryResult[index];
                    if (!this.QueryEvaluator(entity))
                    {
                        this.m_queryResult.RemoveAtFast<MyEntity>(index);
                        continue;
                    }
                    triggerType = this.m_triggerType;
                    if (triggerType == TriggerType.AABB)
                    {
                        if (!this.m_AABB.Intersects(this.m_queryResult[index].PositionComp.WorldAABB))
                        {
                            this.m_queryResult.RemoveAtFast<MyEntity>(index);
                            continue;
                        }
                        index++;
                        continue;
                    }
                    if (triggerType != TriggerType.Sphere)
                    {
                        index++;
                        continue;
                    }
                    if (!this.m_boundingSphere.Intersects(this.m_queryResult[index].PositionComp.WorldAABB))
                    {
                        this.m_queryResult.RemoveAtFast<MyEntity>(index);
                        continue;
                    }
                    index++;
                }
            }
        }

        protected bool DoQuery { get; set; }

        protected List<MyEntity> QueryResult =>
            this.m_queryResult;

        public uint UpdateFrequency { get; set; }

        public virtual bool Enabled { get; protected set; }

        public override string ComponentTypeDebugString =>
            "Trigger";

        public Color? CustomDebugColor { get; set; }

        public Vector3D Center
        {
            get
            {
                TriggerType triggerType = this.m_triggerType;
                return ((triggerType == TriggerType.AABB) ? this.m_AABB.Center : ((triggerType == TriggerType.Sphere) ? this.m_boundingSphere.Center : Vector3D.Zero));
            }
        }

        public enum TriggerType
        {
            AABB,
            Sphere
        }
    }
}


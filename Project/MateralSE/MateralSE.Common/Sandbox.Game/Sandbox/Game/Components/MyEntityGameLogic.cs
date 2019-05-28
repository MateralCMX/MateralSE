namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyEntityGameLogic : MyGameLogicComponent
    {
        [CompilerGenerated]
        private Action<MyEntity> OnMarkForClose;
        [CompilerGenerated]
        private Action<MyEntity> OnClose;
        [CompilerGenerated]
        private Action<MyEntity> OnClosing;
        protected MyEntity m_entity;

        public event Action<MyEntity> OnClose
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onClose = this.OnClose;
                while (true)
                {
                    Action<MyEntity> a = onClose;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onClose = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnClose, action3, a);
                    if (ReferenceEquals(onClose, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onClose = this.OnClose;
                while (true)
                {
                    Action<MyEntity> source = onClose;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onClose = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnClose, action3, source);
                    if (ReferenceEquals(onClose, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEntity> OnClosing
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onClosing = this.OnClosing;
                while (true)
                {
                    Action<MyEntity> a = onClosing;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onClosing = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnClosing, action3, a);
                    if (ReferenceEquals(onClosing, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onClosing = this.OnClosing;
                while (true)
                {
                    Action<MyEntity> source = onClosing;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onClosing = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnClosing, action3, source);
                    if (ReferenceEquals(onClosing, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEntity> OnMarkForClose
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onMarkForClose = this.OnMarkForClose;
                while (true)
                {
                    Action<MyEntity> a = onMarkForClose;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onMarkForClose = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnMarkForClose, action3, a);
                    if (ReferenceEquals(onMarkForClose, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onMarkForClose = this.OnMarkForClose;
                while (true)
                {
                    Action<MyEntity> source = onMarkForClose;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onMarkForClose = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnMarkForClose, action3, source);
                    if (ReferenceEquals(onMarkForClose, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyEntityGameLogic()
        {
            this.GameLogic = new MyNullGameLogicComponent();
        }

        private void AllocateEntityID()
        {
            if ((base.Container.Entity.EntityId == 0) && !MyEntityIdentifier.AllocationSuspended)
            {
                base.Container.Entity.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
        }

        protected void CallAndClearOnClose()
        {
            if (this.OnClose != null)
            {
                this.OnClose(this.m_entity);
            }
            this.OnClose = null;
        }

        protected void CallAndClearOnClosing()
        {
            if (this.OnClosing != null)
            {
                this.OnClosing(this.m_entity);
            }
            this.OnClosing = null;
        }

        public override void Close()
        {
            ((IMyGameLogicComponent) this.GameLogic).Close();
            MyHierarchyComponent<MyEntity> hierarchy = this.m_entity.Hierarchy;
            while (true)
            {
                if (hierarchy != null)
                {
                    ListReader<MyHierarchyComponentBase> children = hierarchy.Children;
                    if (children.Count > 0)
                    {
                        MyHierarchyComponentBase childHierarchy = hierarchy.Children[hierarchy.Children.Count - 1];
                        childHierarchy.Container.Entity.Close();
                        hierarchy.RemoveByJN(childHierarchy);
                        continue;
                    }
                }
                this.CallAndClearOnClosing();
                MyEntities.RemoveName(this.m_entity);
                MyEntities.RemoveFromClosedEntities(this.m_entity);
                if (this.m_entity.Physics != null)
                {
                    this.m_entity.Physics.Close();
                    this.m_entity.Physics = null;
                    this.m_entity.RaisePhysicsChanged();
                }
                MyEntities.UnregisterForUpdate(this.m_entity, true);
                MyEntities.UnregisterForDraw(this.m_entity);
                if ((hierarchy == null) || (hierarchy.Parent == null))
                {
                    MyEntities.Remove(this.m_entity);
                }
                else
                {
                    this.m_entity.Parent.Hierarchy.RemoveByJN(hierarchy);
                    if (this.m_entity.Parent.InScene)
                    {
                        this.m_entity.OnRemovedFromScene(this.m_entity);
                    }
                    MyEntities.RaiseEntityRemove(this.m_entity);
                }
                if (this.m_entity.EntityId != 0)
                {
                    MyEntityIdentifier.RemoveEntity(this.m_entity.EntityId);
                }
                this.CallAndClearOnClose();
                base.Closed = true;
                return;
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyPositionAndOrientation orientation = new MyPositionAndOrientation {
                Position = base.Container.Entity.PositionComp.GetPosition(),
                Up = (SerializableVector3) base.Container.Entity.WorldMatrix.Up,
                Forward = (SerializableVector3) base.Container.Entity.WorldMatrix.Forward
            };
            MyObjectBuilder_EntityBase base1 = MyEntityFactory.CreateObjectBuilder(base.Container.Entity as MyEntity);
            base1.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
            base1.EntityId = base.Container.Entity.EntityId;
            base1.Name = base.Container.Entity.Name;
            base1.PersistentFlags = base.Container.Entity.Render.PersistentFlags;
            return base1;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (objectBuilder != null)
            {
                if (objectBuilder.PositionAndOrientation != null)
                {
                    MyPositionAndOrientation orientation = objectBuilder.PositionAndOrientation.Value;
                    MatrixD worldMatrix = MatrixD.CreateWorld((Vector3D) orientation.Position, (Vector3) orientation.Forward, (Vector3) orientation.Up);
                    base.Container.Entity.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                }
                if (objectBuilder.EntityId != 0)
                {
                    base.Container.Entity.EntityId = objectBuilder.EntityId;
                }
                base.Container.Entity.Name = objectBuilder.Name;
                base.Container.Entity.Render.PersistentFlags = objectBuilder.PersistentFlags;
            }
            this.AllocateEntityID();
            base.Container.Entity.InScene = false;
            MyEntities.SetEntityName(this.m_entity, false);
            if (this.m_entity.SyncFlag)
            {
                this.m_entity.CreateSync();
            }
            this.GameLogic.Init(objectBuilder);
        }

        public void Init(StringBuilder displayName, string model, MyEntity parentObject, float? scale, string modelCollision = null)
        {
            base.Container.Entity.DisplayName = displayName?.ToString();
            this.m_entity.RefreshModels(model, modelCollision);
            if (parentObject != null)
            {
                parentObject.Hierarchy.AddChild(base.Container.Entity, false, false);
            }
            base.Container.Entity.PositionComp.Scale = scale;
            this.AllocateEntityID();
        }

        public override void MarkForClose()
        {
            base.MarkedForClose = true;
            MyEntities.Close(this.m_entity);
            this.GameLogic.MarkForClose();
            Action<MyEntity> onMarkForClose = this.OnMarkForClose;
            if (onMarkForClose != null)
            {
                onMarkForClose(this.m_entity);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_entity = base.Container.Entity as MyEntity;
        }

        public override void UpdateAfterSimulation()
        {
            this.GameLogic.UpdateAfterSimulation();
        }

        public override void UpdateAfterSimulation10()
        {
            this.GameLogic.UpdateAfterSimulation10();
        }

        public override void UpdateAfterSimulation100()
        {
            this.GameLogic.UpdateAfterSimulation100();
        }

        public override void UpdateBeforeSimulation()
        {
            this.GameLogic.UpdateBeforeSimulation();
        }

        public override void UpdateBeforeSimulation10()
        {
            this.GameLogic.UpdateBeforeSimulation10();
        }

        public override void UpdateBeforeSimulation100()
        {
            this.GameLogic.UpdateBeforeSimulation100();
        }

        public override void UpdateOnceBeforeFrame()
        {
            this.GameLogic.UpdateOnceBeforeFrame();
        }

        public override void UpdatingStopped()
        {
        }

        public MyGameLogicComponent GameLogic { get; set; }
    }
}


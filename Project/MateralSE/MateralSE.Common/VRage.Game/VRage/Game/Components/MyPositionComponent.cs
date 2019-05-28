namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using VRage.ModAPI;
    using VRageMath;

    public class MyPositionComponent : MyPositionComponentBase
    {
        public Action<object> WorldPositionChanged;
        private MySyncComponentBase m_syncObject;
        private MyPhysicsComponentBase m_physics;
        private MyHierarchyComponentBase m_hierarchy;
        public static bool SynchronizationEnabled = true;
        private List<MyEntity> entities = new List<MyEntity>();
        private Dictionary<string, int> types = new Dictionary<string, int>();

        private void container_ComponentAdded(Type type, MyEntityComponentBase comp)
        {
            if (type == typeof(MySyncComponentBase))
            {
                this.m_syncObject = comp as MySyncComponentBase;
            }
            else if (type == typeof(MyPhysicsComponentBase))
            {
                this.m_physics = comp as MyPhysicsComponentBase;
            }
            else if (type == typeof(MyHierarchyComponentBase))
            {
                this.m_hierarchy = comp as MyHierarchyComponentBase;
            }
        }

        private void container_ComponentRemoved(Type type, MyEntityComponentBase comp)
        {
            if (type == typeof(MySyncComponentBase))
            {
                this.m_syncObject = null;
            }
            else if (type == typeof(MyPhysicsComponentBase))
            {
                this.m_physics = null;
            }
            else if (type == typeof(MyHierarchyComponentBase))
            {
                this.m_hierarchy = null;
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_syncObject = base.Container.Get<MySyncComponentBase>();
            this.m_physics = base.Container.Get<MyPhysicsComponentBase>();
            this.m_hierarchy = base.Container.Get<MyHierarchyComponentBase>();
            base.Container.ComponentAdded += new Action<Type, MyEntityComponentBase>(this.container_ComponentAdded);
            base.Container.ComponentRemoved += new Action<Type, MyEntityComponentBase>(this.container_ComponentRemoved);
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            base.Container.ComponentAdded -= new Action<Type, MyEntityComponentBase>(this.container_ComponentAdded);
            base.Container.ComponentRemoved -= new Action<Type, MyEntityComponentBase>(this.container_ComponentRemoved);
        }

        protected override void OnWorldPositionChanged(object source, bool updateChildren, bool forceUpdateAllChildren)
        {
            base.Container.Entity.UpdateGamePruningStructure();
            if ((updateChildren && (this.m_hierarchy != null)) && (this.m_hierarchy.Children.Count > 0))
            {
                this.UpdateChildren(source, forceUpdateAllChildren);
            }
            base.m_worldVolumeDirty = true;
            base.m_worldAABBDirty = true;
            base.m_normalizedInvMatrixDirty = true;
            base.m_invScaledMatrixDirty = true;
            if (((this.m_physics != null) && !ReferenceEquals(this.m_physics, source)) && this.m_physics.Enabled)
            {
                this.m_physics.OnWorldPositionChanged(source);
            }
            base.RaiseOnPositionChanged(this);
            this.WorldPositionChanged.InvokeIfNotNull<object>(source);
            if ((base.Container.Entity.Render != null) && ((base.Entity.Flags & EntityFlags.InvalidateOnMove) != 0))
            {
                base.Container.Entity.Render.InvalidateRenderObjects();
            }
        }

        protected virtual void UpdateChildren(object source, bool forceUpdateAllChildren)
        {
            MatrixD worldMatrix = base.WorldMatrix;
            using (List<MyHierarchyComponentBase>.Enumerator enumerator = (forceUpdateAllChildren ? this.m_hierarchy.Children : this.m_hierarchy.ChildrenNeedingWorldMatrix).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Container.Entity.PositionComp.UpdateWorldMatrix(ref worldMatrix, source, true, forceUpdateAllChildren);
                }
            }
        }

        public override BoundingBox LocalAABB
        {
            get => 
                base.m_localAABB;
            set
            {
                base.LocalAABB = value;
                base.Container.Entity.UpdateGamePruningStructure();
            }
        }

        protected override bool ShouldSync =>
            (SynchronizationEnabled && ((base.Container.Get<MySyncComponentBase>() != null) && (this.m_syncObject != null)));
    }
}


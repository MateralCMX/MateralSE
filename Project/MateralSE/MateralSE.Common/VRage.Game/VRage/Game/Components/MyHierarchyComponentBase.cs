namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_HierarchyComponentBase), true)]
    public class MyHierarchyComponentBase : MyEntityComponentBase
    {
        private readonly List<MyHierarchyComponentBase> m_children = new List<MyHierarchyComponentBase>();
        private readonly List<MyHierarchyComponentBase> m_childrenNeedingWorldMatrix = new List<MyHierarchyComponentBase>();
        private readonly List<MyEntity> m_deserializedEntities = new List<MyEntity>();
        [CompilerGenerated]
        private Action<IMyEntity> OnChildRemoved;
        [CompilerGenerated]
        private Action<MyHierarchyComponentBase, MyHierarchyComponentBase> OnParentChanged;
        public long ChildId;
        private MyEntityComponentContainer m_parentContainer;
        private MyHierarchyComponentBase m_parent;

        public event Action<IMyEntity> OnChildRemoved
        {
            [CompilerGenerated] add
            {
                Action<IMyEntity> onChildRemoved = this.OnChildRemoved;
                while (true)
                {
                    Action<IMyEntity> a = onChildRemoved;
                    Action<IMyEntity> action3 = (Action<IMyEntity>) Delegate.Combine(a, value);
                    onChildRemoved = Interlocked.CompareExchange<Action<IMyEntity>>(ref this.OnChildRemoved, action3, a);
                    if (ReferenceEquals(onChildRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyEntity> onChildRemoved = this.OnChildRemoved;
                while (true)
                {
                    Action<IMyEntity> source = onChildRemoved;
                    Action<IMyEntity> action3 = (Action<IMyEntity>) Delegate.Remove(source, value);
                    onChildRemoved = Interlocked.CompareExchange<Action<IMyEntity>>(ref this.OnChildRemoved, action3, source);
                    if (ReferenceEquals(onChildRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyHierarchyComponentBase, MyHierarchyComponentBase> OnParentChanged
        {
            [CompilerGenerated] add
            {
                Action<MyHierarchyComponentBase, MyHierarchyComponentBase> onParentChanged = this.OnParentChanged;
                while (true)
                {
                    Action<MyHierarchyComponentBase, MyHierarchyComponentBase> a = onParentChanged;
                    Action<MyHierarchyComponentBase, MyHierarchyComponentBase> action3 = (Action<MyHierarchyComponentBase, MyHierarchyComponentBase>) Delegate.Combine(a, value);
                    onParentChanged = Interlocked.CompareExchange<Action<MyHierarchyComponentBase, MyHierarchyComponentBase>>(ref this.OnParentChanged, action3, a);
                    if (ReferenceEquals(onParentChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyHierarchyComponentBase, MyHierarchyComponentBase> onParentChanged = this.OnParentChanged;
                while (true)
                {
                    Action<MyHierarchyComponentBase, MyHierarchyComponentBase> source = onParentChanged;
                    Action<MyHierarchyComponentBase, MyHierarchyComponentBase> action3 = (Action<MyHierarchyComponentBase, MyHierarchyComponentBase>) Delegate.Remove(source, value);
                    onParentChanged = Interlocked.CompareExchange<Action<MyHierarchyComponentBase, MyHierarchyComponentBase>>(ref this.OnParentChanged, action3, source);
                    if (ReferenceEquals(onParentChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void AddChild(IMyEntity child, bool preserveWorldPos = false, bool insertIntoSceneIfNeeded = true)
        {
            MyHierarchyComponentBase item = child.Components.Get<MyHierarchyComponentBase>();
            if (!this.m_children.Contains(item))
            {
                MatrixD worldMatrix = child.WorldMatrix;
                item.Parent = this;
                this.m_children.Add(item);
                if (child.NeedsWorldMatrix)
                {
                    this.m_childrenNeedingWorldMatrix.Add(item);
                }
                if (preserveWorldPos)
                {
                    child.PositionComp.SetWorldMatrix(worldMatrix, base.Entity, true, true, true, false, false, true);
                }
                else
                {
                    MyPositionComponentBase base3 = base.Container.Get<MyPositionComponentBase>();
                    child.Components.Get<MyPositionComponentBase>().UpdateWorldMatrix(ref base3.WorldMatrix, null, true, false);
                }
                if ((base.Container.Entity.InScene && !child.InScene) & insertIntoSceneIfNeeded)
                {
                    child.OnAddedToScene(base.Container.Entity);
                }
            }
        }

        public void AddChildWithMatrix(IMyEntity child, ref Matrix childLocalMatrix, bool insertIntoSceneIfNeeded = true)
        {
            MyHierarchyComponentBase item = child.Components.Get<MyHierarchyComponentBase>();
            item.Parent = this;
            this.m_children.Add(item);
            child.WorldMatrix = childLocalMatrix * base.Container.Get<MyPositionComponentBase>().WorldMatrix;
            if (child.NeedsWorldMatrix)
            {
                this.m_childrenNeedingWorldMatrix.Add(item);
            }
            if ((base.Container.Entity.InScene && !child.InScene) & insertIntoSceneIfNeeded)
            {
                child.OnAddedToScene(this);
            }
        }

        private void Container_ComponentAdded(Type arg1, MyEntityComponentBase arg2)
        {
            if (typeof(MyHierarchyComponentBase).IsAssignableFrom(arg1))
            {
                this.m_parent = arg2 as MyHierarchyComponentBase;
            }
        }

        private void Container_ComponentRemoved(Type arg1, MyEntityComponentBase arg2)
        {
            if (ReferenceEquals(arg2, this.m_parent))
            {
                this.m_parent = null;
            }
        }

        public void Delete()
        {
            for (int i = this.m_children.Count - 1; i >= 0; i--)
            {
                this.m_children[i].Container.Entity.Delete();
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_HierarchyComponentBase base2 = builder as MyObjectBuilder_HierarchyComponentBase;
            if (base2 != null)
            {
                this.m_deserializedEntities.Clear();
                foreach (MyObjectBuilder_EntityBase base3 in base2.Children)
                {
                    if (MyEntityIdentifier.ExistsById(base3.EntityId))
                    {
                        continue;
                    }
                    MyEntity item = MyEntity.MyEntitiesCreateFromObjectBuilderExtCallback(base3, true);
                    if (item != null)
                    {
                        this.m_deserializedEntities.Add(item);
                    }
                }
                foreach (MyEntity entity2 in this.m_deserializedEntities)
                {
                    this.AddChild(entity2, true, false);
                }
            }
        }

        public void GetChildrenRecursive(HashSet<IMyEntity> result)
        {
            int num = 0;
            while (true)
            {
                ListReader<MyHierarchyComponentBase> children = this.Children;
                if (num >= children.Count)
                {
                    return;
                }
                MyHierarchyComponentBase base2 = this.Children[num];
                result.Add(base2.Container.Entity);
                base2.GetChildrenRecursive(result);
                num++;
            }
        }

        public MyHierarchyComponentBase GetTopMostParent(Type type = null)
        {
            MyHierarchyComponentBase parent = this;
            while ((parent.Parent != null) && ((type == null) || !parent.Container.Contains(type)))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
        }

        public override void OnBeforeRemovedFromContainer()
        {
            if ((this.m_parentContainer != null) && !this.m_parentContainer.Entity.MarkedForClose)
            {
                this.m_parentContainer.ComponentAdded -= new Action<Type, MyEntityComponentBase>(this.Container_ComponentAdded);
                this.m_parentContainer.ComponentRemoved -= new Action<Type, MyEntityComponentBase>(this.Container_ComponentRemoved);
            }
            this.m_parent = null;
            this.m_parentContainer = null;
            base.OnBeforeRemovedFromContainer();
        }

        public void RemoveByJN(MyHierarchyComponentBase childHierarchy)
        {
            this.m_children.Remove(childHierarchy);
            this.m_childrenNeedingWorldMatrix.Remove(childHierarchy);
        }

        public void RemoveChild(IMyEntity child, bool preserveWorldPos = false)
        {
            MyHierarchyComponentBase item = child.Components.Get<MyHierarchyComponentBase>();
            MatrixD worldMatrix = new MatrixD();
            if (preserveWorldPos)
            {
                worldMatrix = child.WorldMatrix;
            }
            if (child.InScene)
            {
                child.OnRemovedFromScene(this);
            }
            this.m_children.Remove(item);
            this.m_childrenNeedingWorldMatrix.Remove(item);
            if (preserveWorldPos)
            {
                child.WorldMatrix = worldMatrix;
            }
            item.Parent = null;
            this.OnChildRemoved.InvokeIfNotNull<IMyEntity>(child);
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            if (this.Children.Count == 0)
            {
                return null;
            }
            MyObjectBuilder_HierarchyComponentBase base2 = new MyObjectBuilder_HierarchyComponentBase();
            foreach (MyHierarchyComponentBase base3 in this.Children)
            {
                if (base3.Entity.Save)
                {
                    base2.Children.Add(base3.Entity.GetObjectBuilder(copy));
                }
            }
            return ((base2.Children.Count > 0) ? base2 : null);
        }

        internal void UpdateNeedsWorldMatrix()
        {
            if (base.Entity.Parent != null)
            {
                if (!base.Entity.NeedsWorldMatrix || !base.Entity.Parent.Hierarchy.m_children.Contains(this))
                {
                    base.Entity.Parent.Hierarchy.m_childrenNeedingWorldMatrix.Remove(this);
                }
                else if (!base.Entity.Parent.Hierarchy.m_childrenNeedingWorldMatrix.Contains(this))
                {
                    base.Entity.Parent.Hierarchy.m_childrenNeedingWorldMatrix.Add(this);
                }
            }
        }

        public ListReader<MyHierarchyComponentBase> Children =>
            this.m_children;

        public ListReader<MyHierarchyComponentBase> ChildrenNeedingWorldMatrix =>
            this.m_childrenNeedingWorldMatrix;

        public MyHierarchyComponentBase Parent
        {
            get => 
                this.m_parent;
            set
            {
                MyHierarchyComponentBase parent = this.m_parent;
                if (this.m_parentContainer != null)
                {
                    this.m_parentContainer.ComponentAdded -= new Action<Type, MyEntityComponentBase>(this.Container_ComponentAdded);
                    this.m_parentContainer.ComponentRemoved -= new Action<Type, MyEntityComponentBase>(this.Container_ComponentRemoved);
                    this.m_parentContainer = null;
                }
                this.m_parent = value;
                if (this.m_parent != null)
                {
                    this.m_parentContainer = this.m_parent.Container;
                    this.m_parentContainer.ComponentAdded += new Action<Type, MyEntityComponentBase>(this.Container_ComponentAdded);
                    this.m_parentContainer.ComponentRemoved += new Action<Type, MyEntityComponentBase>(this.Container_ComponentRemoved);
                }
                this.OnParentChanged.InvokeIfNotNull<MyHierarchyComponentBase, MyHierarchyComponentBase>(parent, this.m_parent);
            }
        }

        public override string ComponentTypeDebugString =>
            "Hierarchy";
    }
}


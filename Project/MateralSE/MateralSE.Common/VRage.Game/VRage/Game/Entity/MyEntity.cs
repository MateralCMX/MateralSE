namespace VRage.Game.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.Game.Networking;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    [MyEntityType(typeof(MyObjectBuilder_EntityBase), true)]
    public class MyEntity : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        protected readonly VRage.Sync.Sync<ContactPointData, SyncDirection.FromServer> m_contactPoint;
        public MyDefinitionId? DefinitionId;
        public string Name;
        public bool DebugAsyncLoading;
        private List<MyEntity> m_tmpOnPhysicsChanged = new List<MyEntity>();
        protected List<MyHudEntityParams> m_hudParams = new List<MyHudEntityParams>();
        private string m_displayNameText;
        private MyPositionComponentBase m_position;
        public bool m_positionResetFromServer;
        private MyRenderComponentBase m_render;
        private List<MyDebugRenderComponentBase> m_debugRenderers = new List<MyDebugRenderComponentBase>();
        protected MyModel m_modelCollision;
        public int GamePruningProxyId = -1;
        public int TopMostPruningProxyId = -1;
        public bool StaticForPruningStructure;
        public int TargetPruningProxyId = -1;
        private bool m_raisePhysicsCalled;
        private long m_pins;
        private MyGameLogicComponent m_gameLogic;
        private long m_entityId;
        private MySyncComponentBase m_syncObject;
        private MyModStorageComponentBase m_storage;
        private bool m_isPreview;
        private bool m_isreadyForReplication;
        public Dictionary<IMyReplicable, Action> ReadyForReplicationAction = new Dictionary<IMyReplicable, Action>();
        private MyHierarchyComponent<MyEntity> m_hierarchy;
        private MyPhysicsComponentBase m_physics;
        private string m_displayName;
        [CompilerGenerated]
        private Action<MyEntity> OnMarkForClose;
        [CompilerGenerated]
        private Action<MyEntity> OnClose;
        [CompilerGenerated]
        private Action<MyEntity> OnClosing;
        [CompilerGenerated]
        private Action<MyEntity> OnPhysicsChanged;
        [CompilerGenerated]
        private Action<MyPhysicsComponentBase, MyPhysicsComponentBase> OnPhysicsComponentChanged;
        [CompilerGenerated]
        private Action<MyEntity> AddedToScene;
        public static Action<MyEntity> AddToGamePruningStructureExtCallBack;
        public static Action<MyEntity> RemoveFromGamePruningStructureExtCallBack;
        public static Action<MyEntity> UpdateGamePruningStructureExtCallBack;
        public static MyEntityFactoryCreateObjectBuilderDelegate MyEntityFactoryCreateObjectBuilderExtCallback;
        public static CreateDefaultSyncEntityDelegate CreateDefaultSyncEntityExtCallback;
        public static Action<MyEntity> MyWeldingGroupsAddNodeExtCallback;
        public static Action<MyEntity> MyWeldingGroupsRemoveNodeExtCallback;
        public static Action<MyEntity, List<MyEntity>> MyWeldingGroupsGetGroupNodesExtCallback;
        public static MyWeldingGroupsGroupExistsDelegate MyWeldingGroupsGroupExistsExtCallback;
        public static Action<MyEntity> MyProceduralWorldGeneratorTrackEntityExtCallback;
        public static Action<MyEntity> CreateStandardRenderComponentsExtCallback;
        public static Action<MyComponentContainer, MyObjectBuilderType, MyStringHash, MyObjectBuilder_ComponentContainer> InitComponentsExtCallback;
        public static Func<MyObjectBuilder_EntityBase, bool, MyEntity> MyEntitiesCreateFromObjectBuilderExtCallback;

        public event Action<MyEntity> AddedToScene
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> addedToScene = this.AddedToScene;
                while (true)
                {
                    Action<MyEntity> a = addedToScene;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    addedToScene = Interlocked.CompareExchange<Action<MyEntity>>(ref this.AddedToScene, action3, a);
                    if (ReferenceEquals(addedToScene, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> addedToScene = this.AddedToScene;
                while (true)
                {
                    Action<MyEntity> source = addedToScene;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    addedToScene = Interlocked.CompareExchange<Action<MyEntity>>(ref this.AddedToScene, action3, source);
                    if (ReferenceEquals(addedToScene, source))
                    {
                        return;
                    }
                }
            }
        }

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

        public event Action<MyEntity> OnPhysicsChanged
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onPhysicsChanged = this.OnPhysicsChanged;
                while (true)
                {
                    Action<MyEntity> a = onPhysicsChanged;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onPhysicsChanged = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnPhysicsChanged, action3, a);
                    if (ReferenceEquals(onPhysicsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onPhysicsChanged = this.OnPhysicsChanged;
                while (true)
                {
                    Action<MyEntity> source = onPhysicsChanged;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onPhysicsChanged = Interlocked.CompareExchange<Action<MyEntity>>(ref this.OnPhysicsChanged, action3, source);
                    if (ReferenceEquals(onPhysicsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPhysicsComponentBase, MyPhysicsComponentBase> OnPhysicsComponentChanged
        {
            [CompilerGenerated] add
            {
                Action<MyPhysicsComponentBase, MyPhysicsComponentBase> onPhysicsComponentChanged = this.OnPhysicsComponentChanged;
                while (true)
                {
                    Action<MyPhysicsComponentBase, MyPhysicsComponentBase> a = onPhysicsComponentChanged;
                    Action<MyPhysicsComponentBase, MyPhysicsComponentBase> action3 = (Action<MyPhysicsComponentBase, MyPhysicsComponentBase>) Delegate.Combine(a, value);
                    onPhysicsComponentChanged = Interlocked.CompareExchange<Action<MyPhysicsComponentBase, MyPhysicsComponentBase>>(ref this.OnPhysicsComponentChanged, action3, a);
                    if (ReferenceEquals(onPhysicsComponentChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPhysicsComponentBase, MyPhysicsComponentBase> onPhysicsComponentChanged = this.OnPhysicsComponentChanged;
                while (true)
                {
                    Action<MyPhysicsComponentBase, MyPhysicsComponentBase> source = onPhysicsComponentChanged;
                    Action<MyPhysicsComponentBase, MyPhysicsComponentBase> action3 = (Action<MyPhysicsComponentBase, MyPhysicsComponentBase>) Delegate.Remove(source, value);
                    onPhysicsComponentChanged = Interlocked.CompareExchange<Action<MyPhysicsComponentBase, MyPhysicsComponentBase>>(ref this.OnPhysicsComponentChanged, action3, source);
                    if (ReferenceEquals(onPhysicsComponentChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<VRage.ModAPI.IMyEntity> VRage.ModAPI.IMyEntity.OnClose
        {
            add
            {
                this.OnClose += this.GetDelegate(value);
            }
            remove
            {
                this.OnClose -= this.GetDelegate(value);
            }
        }

        event Action<VRage.ModAPI.IMyEntity> VRage.ModAPI.IMyEntity.OnClosing
        {
            add
            {
                this.OnClosing += this.GetDelegate(value);
            }
            remove
            {
                this.OnClosing -= this.GetDelegate(value);
            }
        }

        event Action<VRage.ModAPI.IMyEntity> VRage.ModAPI.IMyEntity.OnMarkForClose
        {
            add
            {
                this.OnMarkForClose += this.GetDelegate(value);
            }
            remove
            {
                this.OnMarkForClose -= this.GetDelegate(value);
            }
        }

        event Action<VRage.ModAPI.IMyEntity> VRage.ModAPI.IMyEntity.OnPhysicsChanged
        {
            add
            {
                this.OnPhysicsChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OnPhysicsChanged -= this.GetDelegate(value);
            }
        }

        public MyEntity()
        {
            this.Components = new MyEntityComponentContainer(this);
            this.Components.ComponentAdded += new Action<Type, MyEntityComponentBase>(this.Components_ComponentAdded);
            this.Components.ComponentRemoved += new Action<Type, MyEntityComponentBase>(this.Components_ComponentRemoved);
            this.Flags = EntityFlags.Default;
            this.InitComponents();
        }

        public void AddDebugRenderComponent(MyDebugRenderComponentBase render)
        {
            this.m_debugRenderers.Add(render);
        }

        public void AddToGamePruningStructure()
        {
            if (this.UsePrunning())
            {
                AddToGamePruningStructureExtCallBack(this);
            }
        }

        public virtual void AfterPaste()
        {
        }

        private void AllocateEntityID()
        {
            if ((this.EntityId == 0) && !MyEntityIdentifier.AllocationSuspended)
            {
                this.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
        }

        public virtual void ApplyLastControls()
        {
        }

        protected virtual void BeforeDelete()
        {
        }

        public virtual void BeforePaste()
        {
        }

        public virtual void BeforeSave()
        {
        }

        private void CallAndClearOnClose()
        {
            this.OnClose.InvokeIfNotNull<MyEntity>(this);
            this.OnClose = null;
        }

        private void CallAndClearOnClosing()
        {
            this.OnClosing.InvokeIfNotNull<MyEntity>(this);
            this.OnClosing = null;
        }

        protected virtual bool CanBeAddedToRender() => 
            true;

        protected virtual void ClampToWorld()
        {
            BoundingBoxD xd1;
            Vector3D position = this.PositionComp.GetPosition();
            float num = 10f;
            if (MyAPIGatewayShortcuts.GetWorldBoundaries != null)
            {
                xd1 = MyAPIGatewayShortcuts.GetWorldBoundaries();
            }
            else
            {
                xd1 = new BoundingBoxD();
            }
            BoundingBoxD xd = xd1;
            if (((xd.Max.X > xd.Min.X) && (xd.Max.Y > xd.Min.Y)) && (xd.Max.Z > xd.Min.Z))
            {
                if (position.X > xd.Max.X)
                {
                    position.X = xd.Max.X - num;
                }
                else if (position.X < xd.Min.X)
                {
                    position.X = xd.Min.X + num;
                }
                if (position.Y > xd.Max.Y)
                {
                    position.Y = xd.Max.Y - num;
                }
                else if (position.Y < xd.Min.Y)
                {
                    position.Y = xd.Min.Y + num;
                }
                if (position.Z > xd.Max.Z)
                {
                    position.Z = xd.Max.Z - num;
                }
                else if (position.Z < xd.Min.Z)
                {
                    position.Z = xd.Min.Z + num;
                }
                this.PositionComp.SetPosition(position, null, false, true);
            }
        }

        public void ClearDebugRenderComponents()
        {
            this.m_debugRenderers.Clear();
        }

        public void Close()
        {
            if (!this.MarkedForClose)
            {
                this.MarkedForClose = true;
                this.Closing();
                MyEntitiesInterface.Close(this);
                this.GameLogic.MarkForClose();
                this.OnMarkForClose.InvokeIfNotNull<MyEntity>(this);
            }
        }

        protected virtual void Closing()
        {
        }

        private void Components_ComponentAdded(Type t, MyEntityComponentBase c)
        {
            if (typeof(MyPhysicsComponentBase).IsAssignableFrom(t))
            {
                this.m_physics = c as MyPhysicsComponentBase;
            }
            else if (typeof(MySyncComponentBase).IsAssignableFrom(t))
            {
                this.m_syncObject = c as MySyncComponentBase;
            }
            else if (typeof(MyGameLogicComponent).IsAssignableFrom(t))
            {
                this.m_gameLogic = c as MyGameLogicComponent;
            }
            else if (typeof(MyPositionComponentBase).IsAssignableFrom(t))
            {
                this.m_position = c as MyPositionComponentBase;
                if (this.m_position == null)
                {
                    this.PositionComp = new MyNullPositionComponent();
                }
            }
            else if (typeof(MyHierarchyComponentBase).IsAssignableFrom(t))
            {
                this.m_hierarchy = c as MyHierarchyComponent<MyEntity>;
            }
            else if (typeof(MyRenderComponentBase).IsAssignableFrom(t))
            {
                this.m_render = c as MyRenderComponentBase;
                if (this.m_render == null)
                {
                    this.Render = new MyNullRenderComponent();
                }
            }
            else if (typeof(MyInventoryBase).IsAssignableFrom(t))
            {
                this.OnInventoryComponentAdded(c as MyInventoryBase);
            }
            else if (typeof(MyModStorageComponentBase).IsAssignableFrom(t))
            {
                this.m_storage = c as MyModStorageComponentBase;
            }
        }

        private void Components_ComponentRemoved(Type t, MyEntityComponentBase c)
        {
            if (typeof(MyPhysicsComponentBase).IsAssignableFrom(t))
            {
                this.m_physics = null;
            }
            else if (typeof(MySyncComponentBase).IsAssignableFrom(t))
            {
                this.m_syncObject = null;
            }
            else if (typeof(MyGameLogicComponent).IsAssignableFrom(t))
            {
                this.m_gameLogic = null;
            }
            else if (typeof(MyPositionComponentBase).IsAssignableFrom(t))
            {
                this.PositionComp = new MyNullPositionComponent();
            }
            else if (typeof(MyHierarchyComponentBase).IsAssignableFrom(t))
            {
                this.m_hierarchy = null;
            }
            else if (typeof(MyRenderComponentBase).IsAssignableFrom(t))
            {
                this.Render = new MyNullRenderComponent();
            }
            else if (typeof(MyInventoryBase).IsAssignableFrom(t))
            {
                this.OnInventoryComponentRemoved(c as MyInventoryBase);
            }
            else if (typeof(MyModStorageComponentBase).IsAssignableFrom(t))
            {
                this.m_storage = null;
            }
        }

        public bool ContainsDebugRenderComponent(Type render)
        {
            using (List<MyDebugRenderComponentBase>.Enumerator enumerator = this.m_debugRenderers.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.GetType() == render)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CreateSync()
        {
            this.SyncObject = this.OnCreateSync();
        }

        public void DebugDraw()
        {
            if (this.Hierarchy != null)
            {
                using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Hierarchy.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Container.Entity.DebugDraw();
                    }
                }
            }
            using (List<MyDebugRenderComponentBase>.Enumerator enumerator2 = this.m_debugRenderers.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.DebugDraw();
                }
            }
        }

        public void DebugDrawInvalidTriangles()
        {
            using (List<MyDebugRenderComponentBase>.Enumerator enumerator = this.m_debugRenderers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDrawInvalidTriangles();
                }
            }
        }

        public virtual void DebugDrawPhysics()
        {
            using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Hierarchy.Children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    (enumerator.Current.Container.Entity as MyEntity).DebugDrawPhysics();
                }
            }
            if ((this.m_physics != null) && (this.GetDistanceBetweenCameraAndBoundingSphere() <= 200.0))
            {
                this.m_physics.DebugDraw();
            }
        }

        public void Delete()
        {
            if (!this.Closed)
            {
                this.Render.RemoveRenderObjects();
                this.Close();
                this.BeforeDelete();
                if (this.GameLogic != null)
                {
                    ((IMyGameLogicComponent) this.GameLogic).Close();
                }
                MyHierarchyComponent<MyEntity> hierarchy = this.Hierarchy;
                if (hierarchy != null)
                {
                    hierarchy.Delete();
                }
                this.CallAndClearOnClosing();
                MyEntitiesInterface.RemoveName(this);
                MyEntitiesInterface.RemoveFromClosedEntities(this);
                if (this.m_physics != null)
                {
                    this.m_physics.Close();
                    this.Physics = null;
                    this.RaisePhysicsChanged();
                }
                MyEntitiesInterface.UnregisterUpdate(this, true);
                MyEntitiesInterface.UnregisterDraw(this);
                MyEntity parent = this.Parent;
                if (parent == null)
                {
                    MyEntitiesInterface.Remove(this);
                }
                else
                {
                    parent.Hierarchy.RemoveByJN(hierarchy);
                    if (parent.InScene)
                    {
                        this.OnRemovedFromScene(this);
                        MyEntitiesInterface.RaiseEntityRemove(this);
                    }
                }
                if ((this.EntityId != 0) && ReferenceEquals(MyEntityIdentifier.GetEntityById(this.EntityId, true), this))
                {
                    MyEntityIdentifier.RemoveEntity(this.EntityId);
                }
                this.CallAndClearOnClose();
                this.ClearDebugRenderComponents();
                this.Components.Clear();
                this.Closed = true;
            }
        }

        public virtual void DeserializeControls(BitStream stream, bool outOfOrder)
        {
            stream.ReadBool();
        }

        public virtual bool DoOverlapSphereTest(float sphereRadius, Vector3D spherePos) => 
            false;

        public virtual MyEntity GetBaseEntity() => 
            this;

        private Action<MyEntity> GetDelegate(Action<VRage.ModAPI.IMyEntity> value) => 
            ((Action<MyEntity>) Delegate.CreateDelegate(typeof(Action<MyEntity>), value.Target, value.Method));

        public double GetDistanceBetweenCameraAndBoundingSphere()
        {
            Vector3D position = MyAPIGatewayShortcuts.GetMainCamera().Position;
            BoundingSphereD worldVolume = this.PositionComp.WorldVolume;
            return MyUtils.GetSmallestDistanceToSphereAlwaysPositive(ref position, ref worldVolume);
        }

        public double GetDistanceBetweenCameraAndPosition() => 
            Vector3D.Distance(MyAPIGatewayShortcuts.GetMainCamera().Position, this.PositionComp.GetPosition());

        public double GetDistanceBetweenPlayerPositionAndBoundingSphere()
        {
            Vector3D from = MyAPIGatewayShortcuts.GetLocalPlayerPosition();
            BoundingSphereD worldVolume = this.PositionComp.WorldVolume;
            return MyUtils.GetSmallestDistanceToSphereAlwaysPositive(ref from, ref worldVolume);
        }

        public virtual string GetFriendlyName() => 
            string.Empty;

        public virtual List<MyHudEntityParams> GetHudParams(bool allowBlink) => 
            this.m_hudParams;

        internal virtual bool GetIntersectionsWithLine(ref LineD line, List<MyIntersectionResultLineTriangleEx> result, IntersectionFlags flags = 3)
        {
            MyModel model = this.Model;
            if (model != null)
            {
                model.GetTrianglePruningStructure().GetTrianglesIntersectingLine(this, ref line, flags, result);
            }
            return (result.Count > 0);
        }

        public virtual bool GetIntersectionWithAABB(ref BoundingBoxD aabb)
        {
            MyModel model = this.Model;
            return ((model != null) && model.GetTrianglePruningStructure().GetIntersectionWithAABB(this, ref aabb));
        }

        public virtual bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? t, IntersectionFlags flags = 3)
        {
            bool flag = false;
            t = 0;
            MyModel model = this.Model;
            if (model != null)
            {
                MyIntersectionResultLineTriangleEx? nullable = model.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, flags);
                if (nullable != null)
                {
                    t = new MyIntersectionResultLineTriangleEx?(nullable.Value);
                    flag = true;
                }
            }
            return flag;
        }

        public virtual bool GetIntersectionWithLine(ref LineD line, out Vector3D? v, bool useCollisionModel = true, IntersectionFlags flags = 3)
        {
            v = 0;
            MyModel modelCollision = this.Model;
            if (useCollisionModel)
            {
                modelCollision = this.ModelCollision;
            }
            if (modelCollision != null)
            {
                MyIntersectionResultLineTriangleEx? nullable = modelCollision.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, flags);
                if (nullable != null)
                {
                    v = new Vector3D?(nullable.Value.IntersectionPointInWorldSpace);
                    return true;
                }
            }
            return false;
        }

        public virtual unsafe Vector3D? GetIntersectionWithLineAndBoundingSphere(ref LineD line, float boundingSphereRadiusMultiplier)
        {
            if (this.Render.GetModel() != null)
            {
                BoundingSphereD worldVolume = this.PositionComp.WorldVolume;
                double* numPtr1 = (double*) ref worldVolume.Radius;
                numPtr1[0] *= boundingSphereRadiusMultiplier;
                if (MyUtils.IsLineIntersectingBoundingSphere(ref line, ref worldVolume))
                {
                    return new Vector3D?(worldVolume.Center);
                }
            }
            return null;
        }

        public virtual bool GetIntersectionWithSphere(ref BoundingSphereD sphere)
        {
            MyModel model = this.Model;
            return ((model != null) && model.GetTrianglePruningStructure().GetIntersectionWithSphere(this, ref sphere));
        }

        public MyInventoryBase GetInventoryBase()
        {
            MyInventoryBase component = null;
            this.Components.TryGet<MyInventoryBase>(out component);
            return component;
        }

        public virtual MyInventoryBase GetInventoryBase(int index)
        {
            MyInventoryBase component = null;
            return (this.Components.TryGet<MyInventoryBase>(out component) ? component.IterateInventory(index, 0) : null);
        }

        public double GetLargestDistanceBetweenCameraAndBoundingSphere()
        {
            Vector3D position = MyAPIGatewayShortcuts.GetMainCamera().Position;
            BoundingSphereD worldVolume = this.PositionComp.WorldVolume;
            return MyUtils.GetLargestDistanceToSphere(ref position, ref worldVolume);
        }

        public virtual MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_EntityBase base2 = MyEntityFactoryCreateObjectBuilderExtCallback(this);
            if (base2 != null)
            {
                MyPositionAndOrientation orientation = new MyPositionAndOrientation {
                    Position = this.PositionComp.GetPosition(),
                    Up = (SerializableVector3) this.WorldMatrix.Up,
                    Forward = (SerializableVector3) this.WorldMatrix.Forward
                };
                base2.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                base2.EntityId = this.EntityId;
                base2.Name = this.Name;
                base2.PersistentFlags = this.Render.PersistentFlags;
                base2.ComponentContainer = this.Components.Serialize(copy);
                if (this.DefinitionId != null)
                {
                    base2.SubtypeName = this.DefinitionId.Value.SubtypeName;
                }
            }
            return base2;
        }

        public double GetSmallestDistanceBetweenCameraAndBoundingSphere()
        {
            Vector3D position = MyAPIGatewayShortcuts.GetMainCamera().Position;
            BoundingSphereD worldVolume = this.PositionComp.WorldVolume;
            return MyUtils.GetSmallestDistanceToSphereAlwaysPositive(ref position, ref worldVolume);
        }

        public MyEntitySubpart GetSubpart(string name) => 
            this.Subparts[name];

        public MyEntity GetTopMostParent(Type type = null)
        {
            MyEntity parent = this;
            while ((parent.Parent != null) && ((type == null) || !parent.GetType().IsSubclassOf(type)))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        public void GetTrianglesIntersectingSphere(ref BoundingSphere sphere, Vector3? referenceNormalVector, float? maxAngle, List<MyTriangle_Vertex_Normals> retTriangles, int maxNeighbourTriangles)
        {
            MyModel model = this.Model;
            if (model != null)
            {
                model.GetTrianglePruningStructure().GetTrianglesIntersectingSphere(ref sphere, referenceNormalVector, maxAngle, retTriangles, maxNeighbourTriangles);
            }
        }

        public virtual MatrixD GetViewMatrix() => 
            this.PositionComp.WorldMatrixNormalizedInv;

        public virtual void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            this.MarkedForClose = false;
            this.Closed = false;
            this.Render.PersistentFlags = MyPersistentEntityFlags2.CastShadows;
            if (objectBuilder == null)
            {
                this.AllocateEntityID();
            }
            else
            {
                if (objectBuilder.EntityId != 0)
                {
                    this.EntityId = objectBuilder.EntityId;
                }
                else
                {
                    this.AllocateEntityID();
                }
                this.DefinitionId = new MyDefinitionId?(objectBuilder.GetId());
                if (objectBuilder.EntityDefinitionId != null)
                {
                    this.DefinitionId = new MyDefinitionId?(objectBuilder.EntityDefinitionId.Value);
                }
                if (objectBuilder.PositionAndOrientation != null)
                {
                    MyPositionAndOrientation orientation = objectBuilder.PositionAndOrientation.Value;
                    if (!orientation.Position.x.IsValid())
                    {
                        orientation.Position.x = 0.0;
                    }
                    if (!orientation.Position.y.IsValid())
                    {
                        orientation.Position.y = 0.0;
                    }
                    if (!orientation.Position.z.IsValid())
                    {
                        orientation.Position.z = 0.0;
                    }
                    MatrixD worldMatrix = MatrixD.CreateWorld((Vector3D) orientation.Position, (Vector3) orientation.Forward, (Vector3) orientation.Up);
                    if (!worldMatrix.IsValid())
                    {
                        worldMatrix = MatrixD.Identity;
                    }
                    this.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, true);
                    this.ClampToWorld();
                }
                this.Name = objectBuilder.Name;
                this.Render.PersistentFlags = objectBuilder.PersistentFlags & ~MyPersistentEntityFlags2.InScene;
                InitComponentsExtCallback(this.Components, this.DefinitionId.Value.TypeId, this.DefinitionId.Value.SubtypeId, objectBuilder.ComponentContainer);
            }
            MyEntitiesInterface.SetEntityName(this, false);
            if (this.SyncFlag)
            {
                this.CreateSync();
            }
            this.GameLogic.Init(objectBuilder);
        }

        public virtual void Init(StringBuilder displayName, string model, MyEntity parentObject, float? scale, string modelCollision = null)
        {
            this.MarkedForClose = false;
            this.Closed = false;
            this.Render.PersistentFlags = MyPersistentEntityFlags2.CastShadows;
            this.DisplayName = displayName?.ToString();
            this.RefreshModels(model, modelCollision);
            if (parentObject != null)
            {
                parentObject.Hierarchy.AddChild(this, false, false);
            }
            if (this.PositionComp.Scale == null)
            {
                this.PositionComp.Scale = scale;
            }
            this.AllocateEntityID();
        }

        public virtual void InitComponents()
        {
            if (this.Hierarchy == null)
            {
                this.Hierarchy = new MyHierarchyComponent<MyEntity>();
            }
            if (this.GameLogic == null)
            {
                this.GameLogic = new MyNullGameLogicComponent();
            }
            if (this.PositionComp == null)
            {
                this.PositionComp = new MyPositionComponent();
            }
            this.PositionComp.LocalMatrix = Matrix.Identity;
            if (this.Render == null)
            {
                CreateStandardRenderComponentsExtCallback(this);
            }
        }

        protected virtual MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data) => 
            new MyEntitySubpart();

        public virtual void OnAddedToScene(object source)
        {
            if (!this.IsPreview)
            {
                this.SetReadyForReplication();
            }
            this.InScene = true;
            if (this.NeedsUpdate != MyEntityUpdateEnum.NONE)
            {
                MyEntitiesInterface.RegisterUpdate(this);
            }
            if (this.GameLogic != null)
            {
                ((IMyGameLogicComponent) this.GameLogic).RegisterForUpdate();
            }
            if (this.Render.NeedsDraw)
            {
                MyEntitiesInterface.RegisterDraw(this);
            }
            if (this.m_physics != null)
            {
                this.m_physics.Activate();
            }
            this.AddToGamePruningStructure();
            this.Components.OnAddedToScene();
            if (this.Hierarchy != null)
            {
                foreach (MyHierarchyComponentBase base2 in this.Hierarchy.Children)
                {
                    if (!base2.Container.Entity.InScene)
                    {
                        base2.Container.Entity.OnAddedToScene(source);
                    }
                }
            }
            if ((this.Flags & EntityFlags.UpdateRender) > 0)
            {
                this.Render.UpdateRenderObject(true, false);
            }
            MyProceduralWorldGeneratorTrackEntityExtCallback(this);
            this.AddedToScene.InvokeIfNotNull<MyEntity>(this);
        }

        protected virtual MySyncComponentBase OnCreateSync() => 
            CreateDefaultSyncEntityExtCallback(this);

        protected virtual void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
        }

        protected virtual void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
        }

        public virtual void OnRemovedFromScene(object source)
        {
            this.InScene = false;
            if (this.Hierarchy != null)
            {
                using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Hierarchy.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Container.Entity.OnRemovedFromScene(source);
                    }
                }
            }
            this.Components.OnRemovedFromScene();
            MyEntitiesInterface.UnregisterUpdate(this, false);
            MyEntitiesInterface.UnregisterDraw(this);
            if (this.GameLogic != null)
            {
                ((IMyGameLogicComponent) this.GameLogic).UnregisterForUpdate();
            }
            if ((this.m_physics != null) && this.m_physics.Enabled)
            {
                this.m_physics.Deactivate();
            }
            if (this.Parent != null)
            {
                this.Render.FadeOut = this.Parent.Render.FadeOut;
            }
            this.Render.RemoveRenderObjects();
            RemoveFromGamePruningStructureExtCallBack(this);
        }

        public EntityPin Pin() => 
            new EntityPin(this);

        public virtual void PrepareForDraw()
        {
            using (List<MyDebugRenderComponentBase>.Enumerator enumerator = this.m_debugRenderers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.PrepareForDraw();
                }
            }
        }

        public void RaisePhysicsChanged()
        {
            if (!this.m_raisePhysicsCalled)
            {
                this.m_raisePhysicsCalled = true;
                if (!this.InScene)
                {
                    Action<MyEntity> onPhysicsChanged = this.OnPhysicsChanged;
                    if (onPhysicsChanged != null)
                    {
                        onPhysicsChanged(this);
                    }
                }
                else
                {
                    MyWeldingGroupsGetGroupNodesExtCallback(this, this.m_tmpOnPhysicsChanged);
                    foreach (MyEntity entity in this.m_tmpOnPhysicsChanged)
                    {
                        Action<MyEntity> onPhysicsChanged = entity.OnPhysicsChanged;
                        if (onPhysicsChanged != null)
                        {
                            onPhysicsChanged(entity);
                        }
                    }
                    this.m_tmpOnPhysicsChanged.Clear();
                }
                this.m_raisePhysicsCalled = false;
            }
        }

        public virtual unsafe void RefreshModels(string model, string modelCollision)
        {
            float valueOrDefault = this.PositionComp.Scale.GetValueOrDefault(1f);
            if (model != null)
            {
                this.Render.ModelStorage = MyModels.GetModelOnlyData(model);
                MyModel model2 = this.Render.GetModel();
                this.PositionComp.LocalVolumeOffset = (model2 == null) ? Vector3.Zero : (model2.BoundingSphere.Center * valueOrDefault);
            }
            if (modelCollision != null)
            {
                this.m_modelCollision = MyModels.GetModelOnlyData(modelCollision);
            }
            if (this.Render.ModelStorage != null)
            {
                BoundingBox boundingBox = this.Render.GetModel().BoundingBox;
                BoundingBox* boxPtr1 = (BoundingBox*) ref boundingBox;
                boxPtr1->Min = boundingBox.Min * valueOrDefault;
                BoundingBox* boxPtr2 = (BoundingBox*) ref boundingBox;
                boxPtr2->Max = boundingBox.Max * valueOrDefault;
                this.PositionComp.LocalAABB = boundingBox;
                bool allocationSuspended = MyEntityIdentifier.AllocationSuspended;
                try
                {
                    MyEntityIdentifier.AllocationSuspended = false;
                    if (this.Subparts == null)
                    {
                        this.Subparts = new Dictionary<string, MyEntitySubpart>();
                    }
                    else
                    {
                        foreach (KeyValuePair<string, MyEntitySubpart> pair in this.Subparts)
                        {
                            this.Hierarchy.RemoveChild(pair.Value, false);
                            pair.Value.Close();
                        }
                        this.Subparts.Clear();
                    }
                    MyEntitySubpart.Data outData = new MyEntitySubpart.Data();
                    foreach (KeyValuePair<string, MyModelDummy> pair2 in this.Render.GetModel().Dummies)
                    {
                        if (MyEntitySubpart.GetSubpartFromDummy(model, pair2.Key, pair2.Value, ref outData))
                        {
                            MyEntitySubpart subpart = this.InstantiateSubpart(pair2.Value, ref outData);
                            subpart.Render.EnableColorMaskHsv = this.Render.EnableColorMaskHsv;
                            subpart.Render.ColorMaskHsv = this.Render.ColorMaskHsv;
                            subpart.Render.TextureChanges = this.Render.TextureChanges;
                            MyModel modelOnlyData = MyModels.GetModelOnlyData(outData.File);
                            if ((modelOnlyData != null) && (this.Model != null))
                            {
                                modelOnlyData.Rescale(this.Model.ScaleFactor);
                            }
                            subpart.Init(null, outData.File, this, this.PositionComp.Scale, null);
                            subpart.Render.NeedsDrawFromParent = false;
                            subpart.Render.PersistentFlags = this.Render.PersistentFlags & ~MyPersistentEntityFlags2.InScene;
                            subpart.PositionComp.LocalMatrix = outData.InitialTransform;
                            this.Subparts[outData.Name] = subpart;
                            if (this.InScene)
                            {
                                subpart.OnAddedToScene(this);
                            }
                        }
                    }
                }
                finally
                {
                    MyEntityIdentifier.AllocationSuspended = allocationSuspended;
                }
            }
            else
            {
                float num2 = 0.5f;
                this.PositionComp.LocalAABB = new BoundingBox(new Vector3(-num2), new Vector3(num2));
            }
        }

        public void RemoveDebugRenderComponent(Type t)
        {
            int count = this.m_debugRenderers.Count;
            while (count > 0)
            {
                count--;
                if (this.m_debugRenderers[count].GetType() == t)
                {
                    this.m_debugRenderers.RemoveAt(count);
                }
            }
        }

        public void RemoveDebugRenderComponent(MyDebugRenderComponentBase render)
        {
            this.m_debugRenderers.Remove(render);
        }

        public void RemoveFromGamePruningStructure()
        {
            if (this.UsePrunning())
            {
                RemoveFromGamePruningStructureExtCallBack(this);
            }
        }

        public virtual void ResetControls()
        {
        }

        public virtual void SerializeControls(BitStream stream)
        {
            stream.WriteBool(false);
        }

        public void SetEmissiveParts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            UpdateNamedEmissiveParts(this.Render.RenderObjectIDs[0], emissiveName, emissivePartColor, emissivity);
        }

        public void SetEmissivePartsForSubparts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            if (this.Subparts != null)
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in this.Subparts)
                {
                    pair.Value.SetEmissiveParts(emissiveName, emissivePartColor, emissivity);
                }
            }
        }

        public void SetFadeOut(bool state)
        {
            this.Render.FadeOut = state;
            if (this.Hierarchy != null)
            {
                using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Hierarchy.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Container.Entity.Render.FadeOut = state;
                    }
                }
            }
        }

        private void SetReadyForReplication()
        {
            this.IsReadyForReplication = true;
            if (this.Hierarchy != null)
            {
                using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Hierarchy.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ((MyEntity) enumerator.Current.Entity).SetReadyForReplication();
                    }
                }
            }
        }

        public virtual void Simulate()
        {
        }

        public virtual void Teleport(MatrixD worldMatrix, object source = null, bool ignoreAssert = false)
        {
            if (!this.Closed && (this.Hierarchy != null))
            {
                HashSet<VRage.ModAPI.IMyEntity> result = new HashSet<VRage.ModAPI.IMyEntity>();
                HashSet<VRage.ModAPI.IMyEntity> set2 = new HashSet<VRage.ModAPI.IMyEntity>();
                result.Add(this);
                this.Hierarchy.GetChildrenRecursive(result);
                foreach (VRage.ModAPI.IMyEntity entity in result)
                {
                    if (entity.Physics != null)
                    {
                        if (entity.Physics.Enabled)
                        {
                            entity.Physics.Enabled = false;
                            continue;
                        }
                        set2.Add(entity);
                    }
                }
                this.PositionComp.SetWorldMatrix(worldMatrix, source, false, true, true, true, false, ignoreAssert);
                foreach (VRage.ModAPI.IMyEntity entity2 in result.Reverse<VRage.ModAPI.IMyEntity>())
                {
                    if (entity2.Physics == null)
                    {
                        continue;
                    }
                    if (!set2.Contains(entity2))
                    {
                        entity2.Physics.Enabled = true;
                    }
                }
            }
        }

        public override string ToString() => 
            (base.GetType().Name + " {" + this.EntityId.ToString("X8") + "}");

        public bool TryGetSubpart(string name, out MyEntitySubpart subpart) => 
            this.Subparts.TryGetValue(name, out subpart);

        public void Unpin()
        {
            Interlocked.Decrement(ref this.m_pins);
        }

        public virtual void UpdateAfterSimulation()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateAfterSimulation(true);
        }

        public virtual void UpdateAfterSimulation10()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateAfterSimulation10(true);
        }

        public virtual void UpdateAfterSimulation100()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateAfterSimulation100(true);
        }

        public virtual void UpdateBeforeSimulation()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateBeforeSimulation(true);
        }

        public virtual void UpdateBeforeSimulation10()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateBeforeSimulation10(true);
        }

        public virtual void UpdateBeforeSimulation100()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateBeforeSimulation100(true);
        }

        public void UpdateGamePruningStructure()
        {
            if (this.UsePrunning())
            {
                UpdateGamePruningStructureExtCallBack(this);
            }
        }

        protected static void UpdateNamedEmissiveParts(uint renderObjectId, string emissiveName, Color emissivePartColor, float emissivity)
        {
            if (renderObjectId != uint.MaxValue)
            {
                MyRenderProxy.UpdateColorEmissivity(renderObjectId, 0, emissiveName, emissivePartColor, emissivity);
            }
        }

        public virtual void UpdateOnceBeforeFrame()
        {
            ((IMyGameLogicComponent) this.m_gameLogic).UpdateOnceBeforeFrame(true);
        }

        public void UpdateSoundContactPoint(long entityId, Vector3 localPosition, Vector3 normal, Vector3 separatingVelocity, float separatingSpeed)
        {
            ContactPointData newValue = new ContactPointData {
                EntityId = entityId,
                LocalPosition = localPosition,
                Normal = normal,
                ContactPointType = ContactPointData.ContactPointDataTypes.AnySound,
                SeparatingVelocity = separatingVelocity,
                SeparatingSpeed = separatingSpeed
            };
            this.m_contactPoint.SetLocalValue(newValue);
        }

        public virtual void UpdatingStopped()
        {
        }

        private bool UsePrunning()
        {
            EntityFlags flags = (this.Parent == null) ? EntityFlags.IsNotGamePrunningStructureObject : EntityFlags.IsGamePrunningStructureObject;
            return (this.InScene && ((this.Flags & flags) == 0));
        }

        VRage.Game.ModAPI.Ingame.IMyInventory VRage.Game.ModAPI.Ingame.IMyEntity.GetInventory() => 
            (this.GetInventoryBase() as VRage.Game.ModAPI.Ingame.IMyInventory);

        VRage.Game.ModAPI.Ingame.IMyInventory VRage.Game.ModAPI.Ingame.IMyEntity.GetInventory(int index) => 
            (this.GetInventoryBase(index) as VRage.Game.ModAPI.Ingame.IMyInventory);

        Vector3D VRage.Game.ModAPI.Ingame.IMyEntity.GetPosition() => 
            this.PositionComp.GetPosition();

        void VRage.ModAPI.IMyEntity.Close()
        {
            this.Close();
        }

        void VRage.ModAPI.IMyEntity.Delete()
        {
            this.Delete();
        }

        bool VRage.ModAPI.IMyEntity.DoOverlapSphereTest(float sphereRadius, Vector3D spherePos) => 
            this.DoOverlapSphereTest(sphereRadius, spherePos);

        void VRage.ModAPI.IMyEntity.EnableColorMaskForSubparts(bool value)
        {
            if (this.Subparts != null)
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in this.Subparts)
                {
                    pair.Value.Render.EnableColorMaskHsv = value;
                }
            }
        }

        void VRage.ModAPI.IMyEntity.GetChildren(List<VRage.ModAPI.IMyEntity> children, Func<VRage.ModAPI.IMyEntity, bool> collect)
        {
            foreach (VRage.ModAPI.IMyEntity entity in children)
            {
                if ((collect == null) || collect(entity))
                {
                    children.Add(entity);
                }
            }
        }

        Vector3 VRage.ModAPI.IMyEntity.GetDiffuseColor() => 
            ((Vector3) this.Render.GetDiffuseColor());

        float VRage.ModAPI.IMyEntity.GetDistanceBetweenCameraAndBoundingSphere() => 
            ((float) this.GetDistanceBetweenCameraAndBoundingSphere());

        float VRage.ModAPI.IMyEntity.GetDistanceBetweenCameraAndPosition() => 
            ((float) this.GetDistanceBetweenCameraAndPosition());

        string VRage.ModAPI.IMyEntity.GetFriendlyName() => 
            this.GetFriendlyName();

        Vector3D? VRage.ModAPI.IMyEntity.GetIntersectionWithLineAndBoundingSphere(ref LineD line, float boundingSphereRadiusMultiplier) => 
            this.GetIntersectionWithLineAndBoundingSphere(ref line, boundingSphereRadiusMultiplier);

        bool VRage.ModAPI.IMyEntity.GetIntersectionWithSphere(ref BoundingSphereD sphere) => 
            this.GetIntersectionWithSphere(ref sphere);

        VRage.Game.ModAPI.IMyInventory VRage.ModAPI.IMyEntity.GetInventory() => 
            (this.GetInventoryBase() as VRage.Game.ModAPI.IMyInventory);

        VRage.Game.ModAPI.IMyInventory VRage.ModAPI.IMyEntity.GetInventory(int index) => 
            (this.GetInventoryBase(index) as VRage.Game.ModAPI.IMyInventory);

        float VRage.ModAPI.IMyEntity.GetLargestDistanceBetweenCameraAndBoundingSphere() => 
            ((float) this.GetLargestDistanceBetweenCameraAndBoundingSphere());

        MyObjectBuilder_EntityBase VRage.ModAPI.IMyEntity.GetObjectBuilder(bool copy) => 
            this.GetObjectBuilder(copy);

        float VRage.ModAPI.IMyEntity.GetSmallestDistanceBetweenCameraAndBoundingSphere() => 
            ((float) this.GetSmallestDistanceBetweenCameraAndBoundingSphere());

        VRage.ModAPI.IMyEntity VRage.ModAPI.IMyEntity.GetTopMostParent(Type type) => 
            this.GetTopMostParent(type);

        void VRage.ModAPI.IMyEntity.GetTrianglesIntersectingSphere(ref BoundingSphere sphere, Vector3? referenceNormalVector, float? maxAngle, List<MyTriangle_Vertex_Normals> retTriangles, int maxNeighbourTriangles)
        {
            this.GetTrianglesIntersectingSphere(ref sphere, referenceNormalVector, maxAngle, retTriangles, maxNeighbourTriangles);
        }

        MatrixD VRage.ModAPI.IMyEntity.GetViewMatrix() => 
            this.GetViewMatrix();

        MatrixD VRage.ModAPI.IMyEntity.GetWorldMatrixNormalizedInv() => 
            this.PositionComp.WorldMatrixNormalizedInv;

        bool VRage.ModAPI.IMyEntity.IsVisible() => 
            this.Render.IsVisible();

        void VRage.ModAPI.IMyEntity.SetColorMaskForSubparts(Vector3 colorMaskHsv)
        {
            if (this.Subparts != null)
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in this.Subparts)
                {
                    pair.Value.Render.ColorMaskHsv = colorMaskHsv;
                }
            }
        }

        void VRage.ModAPI.IMyEntity.SetEmissiveParts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            this.SetEmissiveParts(emissiveName, emissivePartColor, emissivity);
        }

        void VRage.ModAPI.IMyEntity.SetEmissivePartsForSubparts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            this.SetEmissivePartsForSubparts(emissiveName, emissivePartColor, emissivity);
        }

        void VRage.ModAPI.IMyEntity.SetLocalMatrix(Matrix localMatrix, object source)
        {
            this.PositionComp.SetLocalMatrix(ref localMatrix, source, true);
        }

        void VRage.ModAPI.IMyEntity.SetPosition(Vector3D pos)
        {
            this.PositionComp.SetPosition(pos, null, false, true);
        }

        void VRage.ModAPI.IMyEntity.SetTextureChangesForSubparts(Dictionary<string, MyTextureChange> textureChanges)
        {
            if (this.Subparts != null)
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in this.Subparts)
                {
                    pair.Value.Render.TextureChanges = textureChanges;
                }
            }
        }

        void VRage.ModAPI.IMyEntity.SetWorldMatrix(MatrixD worldMatrix, object source)
        {
            this.PositionComp.SetWorldMatrix(worldMatrix, source, false, true, true, false, false, false);
        }

        public MyEntityComponentContainer Components { get; private set; }

        public MyPositionComponentBase PositionComp
        {
            get => 
                this.m_position;
            set => 
                this.Components.Add<MyPositionComponentBase>(value);
        }

        public MyRenderComponentBase Render
        {
            get => 
                this.m_render;
            set => 
                this.Components.Add<MyRenderComponentBase>(value);
        }

        public MyGameLogicComponent GameLogic
        {
            get => 
                this.m_gameLogic;
            set => 
                this.Components.Add<MyGameLogicComponent>(value);
        }

        public long EntityId
        {
            get => 
                this.m_entityId;
            set
            {
                if (this.m_entityId == 0)
                {
                    if (value != 0)
                    {
                        this.m_entityId = value;
                        MyEntityIdentifier.AddEntityWithId(this);
                    }
                }
                else
                {
                    long entityId = this.m_entityId;
                    if (value == 0)
                    {
                        this.m_entityId = 0L;
                        MyEntityIdentifier.RemoveEntity(entityId);
                    }
                    else
                    {
                        this.m_entityId = value;
                        MyEntityIdentifier.SwapRegisteredEntityId(this, entityId, this.m_entityId);
                    }
                }
            }
        }

        public MySyncComponentBase SyncObject
        {
            get => 
                this.m_syncObject;
            protected set => 
                this.Components.Add<MySyncComponentBase>(value);
        }

        public MyModStorageComponentBase Storage
        {
            get => 
                this.m_storage;
            set => 
                this.Components.Add<MyModStorageComponentBase>(value);
        }

        public bool Closed { get; protected set; }

        public bool MarkedForClose { get; protected set; }

        public virtual float MaxGlassDistSq
        {
            get
            {
                IMyCamera camera = MyAPIGatewayShortcuts.GetMainCamera?.Invoke();
                return ((camera == null) ? 4000000f : ((0.01f * camera.FarPlaneDistance) * camera.FarPlaneDistance));
            }
        }

        public bool Save
        {
            get => 
                ((this.Flags & EntityFlags.Save) != 0);
            set
            {
                if (value)
                {
                    this.Flags |= EntityFlags.Save;
                }
                else
                {
                    this.Flags &= ~EntityFlags.Save;
                }
            }
        }

        public bool IsPreview
        {
            get => 
                this.m_isPreview;
            set => 
                (this.m_isPreview = value);
        }

        public bool IsReadyForReplication
        {
            get => 
                this.m_isreadyForReplication;
            private set
            {
                this.m_isreadyForReplication = value;
                if (this.m_isreadyForReplication && (this.ReadyForReplicationAction.Count > 0))
                {
                    using (Dictionary<IMyReplicable, Action>.ValueCollection.Enumerator enumerator = this.ReadyForReplicationAction.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current();
                        }
                    }
                    this.ReadyForReplicationAction.Clear();
                }
            }
        }

        public MyEntityUpdateEnum NeedsUpdate
        {
            get
            {
                MyEntityUpdateEnum nONE = MyEntityUpdateEnum.NONE;
                if ((this.Flags & EntityFlags.NeedsUpdate) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_FRAME;
                }
                if ((this.Flags & EntityFlags.NeedsUpdate10) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                }
                if ((this.Flags & EntityFlags.NeedsUpdate100) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                }
                if ((this.Flags & EntityFlags.NeedsUpdateBeforeNextFrame) != 0)
                {
                    nONE |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                if ((this.Flags & EntityFlags.NeedsSimulate) != 0)
                {
                    nONE |= MyEntityUpdateEnum.SIMULATE;
                }
                return nONE;
            }
            set
            {
                if (value != this.NeedsUpdate)
                {
                    if (this.InScene)
                    {
                        MyEntitiesInterface.UnregisterUpdate(this, false);
                    }
                    this.Flags &= ~EntityFlags.NeedsUpdateBeforeNextFrame;
                    this.Flags &= ~EntityFlags.NeedsUpdate;
                    this.Flags &= ~EntityFlags.NeedsUpdate10;
                    this.Flags &= ~EntityFlags.NeedsUpdate100;
                    this.Flags &= ~EntityFlags.NeedsSimulate;
                    if ((value & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
                    {
                        this.Flags |= EntityFlags.NeedsUpdateBeforeNextFrame;
                    }
                    if ((value & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE)
                    {
                        this.Flags |= EntityFlags.NeedsUpdate;
                    }
                    if ((value & MyEntityUpdateEnum.EACH_10TH_FRAME) != MyEntityUpdateEnum.NONE)
                    {
                        this.Flags |= EntityFlags.NeedsUpdate10;
                    }
                    if ((value & MyEntityUpdateEnum.EACH_100TH_FRAME) != MyEntityUpdateEnum.NONE)
                    {
                        this.Flags |= EntityFlags.NeedsUpdate100;
                    }
                    if ((value & MyEntityUpdateEnum.SIMULATE) != MyEntityUpdateEnum.NONE)
                    {
                        this.Flags |= EntityFlags.NeedsSimulate;
                    }
                    if (this.InScene)
                    {
                        MyEntitiesInterface.RegisterUpdate(this);
                    }
                }
            }
        }

        public MatrixD WorldMatrix
        {
            get => 
                ((this.PositionComp != null) ? this.PositionComp.WorldMatrix : MatrixD.Zero);
            set
            {
                if (this.PositionComp != null)
                {
                    this.PositionComp.SetWorldMatrix(value, null, false, true, true, false, false, false);
                }
            }
        }

        public MyEntity Parent
        {
            get
            {
                if ((this.m_hierarchy == null) || (this.m_hierarchy.Parent == null))
                {
                    return null;
                }
                return (this.m_hierarchy.Parent.Container.Entity as MyEntity);
            }
            private set => 
                (this.m_hierarchy.Parent = value.Components.Get<MyHierarchyComponentBase>());
        }

        public MyHierarchyComponent<MyEntity> Hierarchy
        {
            get => 
                this.m_hierarchy;
            set => 
                this.Components.Add<MyHierarchyComponentBase>(value);
        }

        MyHierarchyComponentBase VRage.ModAPI.IMyEntity.Hierarchy
        {
            get => 
                this.m_hierarchy;
            set
            {
                if (value is MyHierarchyComponent<MyEntity>)
                {
                    this.Components.Add<MyHierarchyComponentBase>(value);
                }
            }
        }

        MyPhysicsComponentBase VRage.ModAPI.IMyEntity.Physics
        {
            get => 
                this.m_physics;
            set => 
                this.Components.Add<MyPhysicsComponentBase>(value);
        }

        public MyPhysicsComponentBase Physics
        {
            get => 
                this.m_physics;
            set
            {
                MyPhysicsComponentBase physics = this.m_physics;
                this.Components.Add<MyPhysicsComponentBase>(value);
                this.OnPhysicsComponentChanged.InvokeIfNotNull<MyPhysicsComponentBase, MyPhysicsComponentBase>(physics, value);
            }
        }

        public bool InvalidateOnMove
        {
            get => 
                ((this.Flags & EntityFlags.InvalidateOnMove) != 0);
            set
            {
                if (value)
                {
                    this.Flags |= EntityFlags.InvalidateOnMove;
                }
                else
                {
                    this.Flags &= ~EntityFlags.InvalidateOnMove;
                }
            }
        }

        public bool SyncFlag
        {
            get => 
                ((this.Flags & EntityFlags.Sync) != 0);
            set => 
                (this.Flags = value ? (this.Flags | EntityFlags.Sync) : (this.Flags & ~EntityFlags.Sync));
        }

        public bool NeedsWorldMatrix
        {
            get => 
                ((this.Flags & EntityFlags.NeedsWorldMatrix) != 0);
            set
            {
                this.Flags = value ? (this.Flags | EntityFlags.NeedsWorldMatrix) : (this.Flags & ~EntityFlags.NeedsWorldMatrix);
                if (this.Hierarchy != null)
                {
                    this.Hierarchy.UpdateNeedsWorldMatrix();
                }
            }
        }

        public bool InScene
        {
            get => 
                ((this.Render != null) && ((this.Render.PersistentFlags & MyPersistentEntityFlags2.InScene) > MyPersistentEntityFlags2.None));
            set
            {
                if (this.Render != null)
                {
                    if (value)
                    {
                        MyRenderComponentBase render = this.Render;
                        render.PersistentFlags |= MyPersistentEntityFlags2.InScene;
                    }
                    else
                    {
                        MyRenderComponentBase render = this.Render;
                        render.PersistentFlags &= ~MyPersistentEntityFlags2.InScene;
                    }
                }
            }
        }

        public virtual bool IsVolumetric =>
            false;

        public virtual Vector3D LocationForHudMarker =>
            ((this.PositionComp != null) ? this.PositionComp.GetPosition() : Vector3D.Zero);

        public MyModel Model =>
            this.Render.GetModel();

        public MyModel ModelCollision =>
            ((this.m_modelCollision == null) ? this.Render.GetModel() : this.m_modelCollision);

        public string DisplayName
        {
            get => 
                this.m_displayName;
            set => 
                (this.m_displayName = value);
        }

        public string DebugName
        {
            get
            {
                string str = this.m_displayName ?? this.Name;
                if (str == null)
                {
                    str = "";
                }
                string[] textArray1 = new string[] { str, " (", base.GetType().Name, ", ", this.EntityId.ToString(), ")" };
                return string.Concat(textArray1);
            }
        }

        public Dictionary<string, MyEntitySubpart> Subparts { get; private set; }

        public virtual bool IsCCDForProjectiles =>
            false;

        public bool Pinned =>
            (Interlocked.Read(ref this.m_pins) > 0L);

        public int InventoryCount
        {
            get
            {
                MyInventoryBase component = null;
                return (!this.Components.TryGet<MyInventoryBase>(out component) ? 0 : component.GetInventoryCount());
            }
        }

        public bool HasInventory =>
            (this.InventoryCount > 0);

        public virtual string DisplayNameText { get; set; }

        public MySnapshotFlags LastSnapshotFlags { get; set; }

        public EntityFlags Flags { get; set; }

        VRage.ModAPI.IMyEntity VRage.ModAPI.IMyEntity.Parent =>
            this.Parent;

        string VRage.ModAPI.IMyEntity.Name
        {
            get => 
                this.Name;
            set => 
                (this.Name = value);
        }

        bool VRage.ModAPI.IMyEntity.DebugAsyncLoading =>
            this.DebugAsyncLoading;

        string VRage.ModAPI.IMyEntity.DisplayName
        {
            get => 
                this.DisplayName;
            set => 
                (this.DisplayName = value);
        }

        bool VRage.ModAPI.IMyEntity.MarkedForClose =>
            this.MarkedForClose;

        bool VRage.ModAPI.IMyEntity.Closed =>
            this.Closed;

        IMyModel VRage.ModAPI.IMyEntity.Model =>
            this.Model;

        MyEntityComponentBase VRage.ModAPI.IMyEntity.GameLogic
        {
            get => 
                this.GameLogic;
            set => 
                (this.GameLogic = (MyGameLogicComponent) value);
        }

        MyEntityUpdateEnum VRage.ModAPI.IMyEntity.NeedsUpdate
        {
            get => 
                this.NeedsUpdate;
            set => 
                (this.NeedsUpdate = value);
        }

        bool VRage.ModAPI.IMyEntity.NearFlag
        {
            get => 
                this.Render.NearFlag;
            set => 
                (this.Render.NearFlag = value);
        }

        bool VRage.ModAPI.IMyEntity.CastShadows
        {
            get => 
                this.Render.CastShadows;
            set => 
                (this.Render.CastShadows = value);
        }

        bool VRage.ModAPI.IMyEntity.FastCastShadowResolve
        {
            get => 
                this.Render.FastCastShadowResolve;
            set => 
                (this.Render.FastCastShadowResolve = value);
        }

        bool VRage.ModAPI.IMyEntity.NeedsResolveCastShadow
        {
            get => 
                this.Render.NeedsResolveCastShadow;
            set => 
                (this.Render.NeedsResolveCastShadow = value);
        }

        float VRage.ModAPI.IMyEntity.MaxGlassDistSq =>
            this.MaxGlassDistSq;

        bool VRage.ModAPI.IMyEntity.NeedsDraw
        {
            get => 
                this.Render.NeedsDraw;
            set => 
                (this.Render.NeedsDraw = value);
        }

        bool VRage.ModAPI.IMyEntity.NeedsDrawFromParent
        {
            get => 
                this.Render.NeedsDrawFromParent;
            set => 
                (this.Render.NeedsDrawFromParent = value);
        }

        bool VRage.ModAPI.IMyEntity.Transparent
        {
            get => 
                !(this.Render.Transparency == 0f);
            set => 
                (this.Render.Transparency = value ? 0.25f : 0f);
        }

        bool VRage.ModAPI.IMyEntity.ShadowBoxLod
        {
            get => 
                this.Render.ShadowBoxLod;
            set => 
                (this.Render.ShadowBoxLod = value);
        }

        bool VRage.ModAPI.IMyEntity.SkipIfTooSmall
        {
            get => 
                this.Render.SkipIfTooSmall;
            set => 
                (this.Render.SkipIfTooSmall = value);
        }

        MyModStorageComponentBase VRage.ModAPI.IMyEntity.Storage
        {
            get => 
                this.Storage;
            set => 
                (this.Storage = value);
        }

        bool VRage.ModAPI.IMyEntity.Visible
        {
            get => 
                this.Render.Visible;
            set => 
                (this.Render.Visible = value);
        }

        bool VRage.ModAPI.IMyEntity.Save
        {
            get => 
                this.Save;
            set => 
                (this.Save = value);
        }

        MyPersistentEntityFlags2 VRage.ModAPI.IMyEntity.PersistentFlags
        {
            get => 
                this.Render.PersistentFlags;
            set => 
                (this.Render.PersistentFlags = value);
        }

        bool VRage.ModAPI.IMyEntity.InScene
        {
            get => 
                this.InScene;
            set => 
                (this.InScene = value);
        }

        bool VRage.ModAPI.IMyEntity.InvalidateOnMove =>
            this.InvalidateOnMove;

        bool VRage.ModAPI.IMyEntity.IsCCDForProjectiles =>
            this.IsCCDForProjectiles;

        bool VRage.ModAPI.IMyEntity.IsVolumetric =>
            this.IsVolumetric;

        BoundingBox VRage.ModAPI.IMyEntity.LocalAABB
        {
            get => 
                this.PositionComp.LocalAABB;
            set => 
                (this.PositionComp.LocalAABB = value);
        }

        BoundingBox VRage.ModAPI.IMyEntity.LocalAABBHr =>
            this.PositionComp.LocalAABB;

        Matrix VRage.ModAPI.IMyEntity.LocalMatrix
        {
            get => 
                this.PositionComp.LocalMatrix;
            set => 
                (this.PositionComp.LocalMatrix = value);
        }

        BoundingSphere VRage.ModAPI.IMyEntity.LocalVolume
        {
            get => 
                this.PositionComp.LocalVolume;
            set => 
                (this.PositionComp.LocalVolume = value);
        }

        Vector3 VRage.ModAPI.IMyEntity.LocalVolumeOffset
        {
            get => 
                this.PositionComp.LocalVolumeOffset;
            set => 
                (this.PositionComp.LocalVolumeOffset = value);
        }

        Vector3D VRage.ModAPI.IMyEntity.LocationForHudMarker =>
            this.LocationForHudMarker;

        bool VRage.ModAPI.IMyEntity.Synchronized
        {
            get => 
                this.IsPreview;
            set => 
                (this.IsPreview = value);
        }

        MatrixD VRage.ModAPI.IMyEntity.WorldMatrix
        {
            get => 
                this.PositionComp.WorldMatrix;
            set => 
                (this.PositionComp.WorldMatrix = value);
        }

        MatrixD VRage.ModAPI.IMyEntity.WorldMatrixInvScaled =>
            this.PositionComp.WorldMatrixInvScaled;

        MatrixD VRage.ModAPI.IMyEntity.WorldMatrixNormalizedInv =>
            this.PositionComp.WorldMatrixNormalizedInv;

        bool VRage.Game.ModAPI.Ingame.IMyEntity.HasInventory =>
            this.HasInventory;

        int VRage.Game.ModAPI.Ingame.IMyEntity.InventoryCount =>
            this.InventoryCount;

        string VRage.Game.ModAPI.Ingame.IMyEntity.DisplayName =>
            this.DisplayName;

        string VRage.Game.ModAPI.Ingame.IMyEntity.Name =>
            this.Name;

        BoundingBoxD VRage.Game.ModAPI.Ingame.IMyEntity.WorldAABB =>
            this.PositionComp.WorldAABB;

        BoundingBoxD VRage.Game.ModAPI.Ingame.IMyEntity.WorldAABBHr =>
            this.PositionComp.WorldAABB;

        MatrixD VRage.Game.ModAPI.Ingame.IMyEntity.WorldMatrix =>
            this.PositionComp.WorldMatrix;

        BoundingSphereD VRage.Game.ModAPI.Ingame.IMyEntity.WorldVolume =>
            this.PositionComp.WorldVolume;

        BoundingSphereD VRage.Game.ModAPI.Ingame.IMyEntity.WorldVolumeHr =>
            this.PositionComp.WorldVolume;

        [StructLayout(LayoutKind.Sequential)]
        public struct ContactPointData
        {
            public long EntityId;
            public Vector3 LocalPosition;
            public Vector3 Normal;
            public ContactPointDataTypes ContactPointType;
            public Vector3 SeparatingVelocity;
            public float SeparatingSpeed;
            public float Impulse;
            [Flags]
            public enum ContactPointDataTypes
            {
                None = 0,
                Sounds = 1,
                Particle_PlanetCrash = 2,
                Particle_Collision = 4,
                Particle_GridCollision = 8,
                Particle_Dust = 0x10,
                AnySound = 1,
                AnyParticle = 30
            }
        }

        public delegate MySyncComponentBase CreateDefaultSyncEntityDelegate(MyEntity thisEntity);

        [StructLayout(LayoutKind.Sequential)]
        public struct EntityPin : IDisposable
        {
            private MyEntity m_entity;
            public EntityPin(MyEntity entity)
            {
                this.m_entity = entity;
                Interlocked.Increment(ref entity.m_pins);
            }

            public void Dispose()
            {
                this.m_entity.Unpin();
            }
        }

        public delegate MyObjectBuilder_EntityBase MyEntityFactoryCreateObjectBuilderDelegate(MyEntity entity);

        public delegate bool MyWeldingGroupsGroupExistsDelegate(MyEntity entity);
    }
}


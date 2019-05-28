namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEntityType(typeof(MyObjectBuilder_SafeZone), true)]
    public class MySafeZone : MyEntity, IMyEventProxy, IMyEventOwner
    {
        public float Radius;
        protected MyConcurrentHashSet<long> m_containedEntities = new MyConcurrentHashSet<long>();
        private List<MyBillboard> m_persistentBillboards = new List<MyBillboard>();
        private static object m_drawLock = new object();
        public List<MyFaction> Factions = new List<MyFaction>();
        public List<long> Players = new List<long>();
        public HashSet<long> Entities = new HashSet<long>();
        private List<long> m_entitiesToSend = new List<long>();
        private List<long> m_entitiesToAdd = new List<long>();
        private MyHudNotification m_safezoneEnteredNotification = new MyHudNotification(MyCommonTexts.SafeZone_Entered, 0x7d0, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        private MyHudNotification m_safezoneLeftNotification = new MyHudNotification(MyCommonTexts.SafeZone_Left, 0x7d0, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        private Vector3 m_size;

        public MySafeZone()
        {
            base.SyncFlag = true;
        }

        public void AddContainedToList()
        {
            using (ConcurrentEnumerator<SpinLockRef.Token, long, HashSet<long>.Enumerator> enumerator = this.m_containedEntities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyEntity entity;
                    MyIDModule module;
                    if (!MyEntities.TryGetEntityById(enumerator.Current, out entity, false))
                    {
                        continue;
                    }
                    IMyComponentOwner<MyIDModule> owner = entity as IMyComponentOwner<MyIDModule>;
                    if ((owner == null) || !owner.GetComponent(out module))
                    {
                        if (!this.Entities.Contains(entity.EntityId))
                        {
                            this.Entities.Add(entity.EntityId);
                        }
                        continue;
                    }
                    if (!this.Players.Contains(module.Owner))
                    {
                        this.Players.Add(module.Owner);
                    }
                }
            }
        }

        private void ClearBillboards()
        {
            using (List<MyBillboard>.Enumerator enumerator = this.m_persistentBillboards.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyRenderProxy.RemovePersistentBillboard(enumerator.Current);
                }
            }
            this.m_persistentBillboards.Clear();
        }

        protected override void Closing()
        {
            MySessionComponentSafeZones.RemoveSafeZone(this);
            this.ClearBillboards();
            base.Closing();
        }

        private HkBvShape CreateFieldShape()
        {
            HkPhantomCallbackShape shape = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_Enter), new HkPhantomHandler(this.phantom_Leave));
            return new HkBvShape(this.GetHkShape(), (HkShape) shape, HkReferencePolicy.TakeOwnership);
        }

        private void entity_OnClose(MyEntity obj)
        {
            if (base.PositionComp != null)
            {
                base.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
            }
            if (this.RemoveEntityInternal(obj, true))
            {
                this.SendRemovedEntity(obj.EntityId, true);
            }
        }

        protected HkShape GetHkShape() => 
            ((this.Shape != MySafeZoneShape.Sphere) ? ((HkShape) new HkBoxShape(this.Size / 2f)) : ((HkShape) new HkSphereShape(this.Radius)));

        public override bool GetIntersectionWithLine(ref LineD line, out Vector3D? v, bool useCollisionModel = true, IntersectionFlags flags = 3)
        {
            v = 0;
            RayD ray = new RayD(line.From, line.Direction);
            if (this.Shape == MySafeZoneShape.Sphere)
            {
                double num;
                double num2;
                if (base.PositionComp.WorldVolume.IntersectRaySphere(ray, out num, out num2))
                {
                    v = new Vector3D?(line.From + (line.Direction * num));
                    return true;
                }
            }
            else
            {
                double? nullable = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix).Intersects(ref ray);
                if (nullable != null)
                {
                    v = new Vector3D?(line.From + (line.Direction * nullable.Value));
                    return true;
                }
            }
            return false;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_SafeZone objectBuilder = (MyObjectBuilder_SafeZone) base.GetObjectBuilder(copy);
            objectBuilder.Radius = this.Radius;
            objectBuilder.Size = this.Size;
            objectBuilder.Shape = this.Shape;
            objectBuilder.Enabled = this.Enabled;
            objectBuilder.AccessTypePlayers = this.AccessTypePlayers;
            objectBuilder.AccessTypeFactions = this.AccessTypeFactions;
            objectBuilder.AccessTypeGrids = this.AccessTypeGrids;
            objectBuilder.AccessTypeFloatingObjects = this.AccessTypeFloatingObjects;
            objectBuilder.AllowedActions = this.AllowedActions;
            objectBuilder.DisplayName = base.DisplayName;
            objectBuilder.Factions = this.Factions.ConvertAll<long>(x => x.FactionId).ToArray();
            objectBuilder.Players = this.Players.ToArray();
            objectBuilder.Entities = this.Entities.ToArray<long>();
            if (Sync.IsServer && (this.m_containedEntities.Count > 0))
            {
                objectBuilder.ContainedEntities = this.m_containedEntities.ToArray<long>();
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            base.Render = new MyNullRenderComponent();
            base.Save = true;
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            MyObjectBuilder_SafeZone ob = (MyObjectBuilder_SafeZone) objectBuilder;
            this.InitInternal(ob, false);
            MySessionComponentSafeZones.AddSafeZone(this);
            if (base.PositionComp != null)
            {
                base.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
            }
        }

        internal void InitInternal(MyObjectBuilder_SafeZone ob, bool insertEntities = true)
        {
            this.Radius = ob.Radius;
            bool flag = this.Enabled != ob.Enabled;
            this.Enabled = ob.Enabled;
            this.AccessTypePlayers = ob.AccessTypePlayers;
            this.AccessTypeFactions = ob.AccessTypeFactions;
            this.AccessTypeGrids = ob.AccessTypeGrids;
            this.AccessTypeFloatingObjects = ob.AccessTypeFloatingObjects;
            this.AllowedActions = ob.AllowedActions;
            bool flag2 = this.Size != ob.Size;
            this.Size = ob.Size;
            bool flag3 = this.Shape != ob.Shape;
            this.Shape = ob.Shape;
            base.DisplayName = ob.DisplayName;
            bool flag4 = base.PositionComp.WorldMatrix != ob.PositionAndOrientation.Value.GetMatrix();
            base.PositionComp.WorldMatrix = ob.PositionAndOrientation.Value.GetMatrix();
            if (ob.Factions != null)
            {
                this.Factions = (from x in ob.Factions.ToList<long>().ConvertAll<MyFaction>(x => (MyFaction) MySession.Static.Factions.TryGetFactionById(x))
                    where x != null
                    select x).ToList<MyFaction>();
            }
            if (ob.Players != null)
            {
                this.Players = ob.Players.ToList<long>();
            }
            if (ob.Entities != null)
            {
                this.Entities = new HashSet<long>(ob.Entities);
            }
            if (((!(this.Radius == ob.Radius) | flag3) | flag2) | flag4)
            {
                this.RecreatePhysics(insertEntities);
                flag = false;
            }
            if (flag & insertEntities)
            {
                this.InsertContainingEntities();
                this.RecreateBillboards();
            }
            if (!Sync.IsServer && (ob.ContainedEntities != null))
            {
                this.m_entitiesToAdd.AddArray<long>(ob.ContainedEntities);
            }
        }

        private void InsertContainingEntities()
        {
            if (Sync.IsServer)
            {
                List<MyEntity> topMostEntitiesInSphere = null;
                if (this.Shape == MySafeZoneShape.Sphere)
                {
                    BoundingSphereD boundingSphere = new BoundingSphereD(base.PositionComp.WorldMatrix.Translation, (double) this.Radius);
                    topMostEntitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref boundingSphere);
                }
                else
                {
                    MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                    topMostEntitiesInSphere = MyEntities.GetEntitiesInOBB(ref obb);
                }
                foreach (MyEntity entity in topMostEntitiesInSphere)
                {
                    MyOrientedBoundingBoxD xd3 = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                    MyOrientedBoundingBoxD other = new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB, entity.PositionComp.WorldMatrix);
                    if ((xd3.Contains(ref other) == ContainmentType.Contains) && this.InsertEntityInternal(entity, false))
                    {
                        this.m_entitiesToSend.Add(entity.EntityId);
                    }
                }
                this.SendInsertedEntities(this.m_entitiesToSend);
                topMostEntitiesInSphere.Clear();
                this.m_entitiesToSend.Clear();
            }
        }

        [Event(null, 0x2a4), Reliable, BroadcastExcept]
        private void InsertEntities_Implementation(List<long> list)
        {
            foreach (long num in list)
            {
                this.InsertEntity_Implementation(num, false);
            }
        }

        internal void InsertEntity(MyEntity entity)
        {
            if (this.Shape == MySafeZoneShape.Box)
            {
                MyOrientedBoundingBoxD xd = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                MyOrientedBoundingBoxD other = new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB, entity.PositionComp.WorldMatrix);
                if (xd.Contains(ref other) != ContainmentType.Contains)
                {
                    return;
                }
            }
            else
            {
                BoundingSphereD sphere = new BoundingSphereD(base.PositionComp.WorldMatrix.Translation, (double) this.Radius);
                MyOrientedBoundingBoxD xd3 = new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB, entity.PositionComp.WorldMatrix);
                if (xd3.Contains(ref sphere) != ContainmentType.Contains)
                {
                    return;
                }
            }
            if (this.InsertEntityInternal(entity, false))
            {
                this.SendInsertedEntity(entity.EntityId, false);
            }
        }

        [Event(null, 0x295), Reliable, BroadcastExcept]
        private void InsertEntity_Implementation(long entityId, bool addedOrRemoved)
        {
            if (!this.m_containedEntities.Contains(entityId))
            {
                MyEntity entity;
                this.m_containedEntities.Add(entityId);
                if (MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    this.InsertEntityLocal(entity, addedOrRemoved);
                }
            }
        }

        private bool InsertEntityInternal(MyEntity entity, bool addedOrRemoved)
        {
            if (entity != null)
            {
                MyEntity topEntity = entity.GetTopMostParent(null);
                if (topEntity.Physics == null)
                {
                    return false;
                }
                if (topEntity is MySafeZone)
                {
                    return false;
                }
                if (topEntity.Physics.ShapeChangeInProgress)
                {
                    return false;
                }
                if (!this.m_containedEntities.Contains(topEntity.EntityId))
                {
                    this.m_containedEntities.Add(topEntity.EntityId);
                    this.InsertEntityLocal(entity, addedOrRemoved);
                    MySandboxGame.Static.Invoke(delegate {
                        if (((topEntity.Physics != null) && topEntity.Physics.HasRigidBody) && !topEntity.Physics.IsStatic)
                        {
                            ((MyPhysicsBody) topEntity.Physics).RigidBody.Activate();
                        }
                    }, "MyGravityGeneratorBase/Activate physics");
                    return true;
                }
            }
            return false;
        }

        private void InsertEntityLocal(MyEntity topEntity, bool addedOrRemoved)
        {
            if ((this.Enabled && ((MySession.Static.ControlledEntity != null) && ReferenceEquals(((MyEntity) MySession.Static.ControlledEntity).GetTopMostParent(null), topEntity))) && !addedOrRemoved)
            {
                if (!this.IsSafe((MyEntity) MySession.Static.ControlledEntity))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                }
                else
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                    MyHud.Notifications.Add(this.m_safezoneEnteredNotification);
                }
            }
        }

        public bool IsActionAllowed(MyEntity entity, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            if (!this.Enabled)
            {
                return true;
            }
            if (entity != null)
            {
                MyEntity entity2;
                if (!this.m_containedEntities.Contains(entity.EntityId))
                {
                    return true;
                }
                if (((sourceEntityId == 0) || !MyEntities.TryGetEntityById(sourceEntityId, out entity2, false)) || this.IsSafe(entity2.GetTopMostParent(null)))
                {
                    return this.AllowedActions.HasFlag(action);
                }
            }
            return false;
        }

        public bool IsActionAllowed(BoundingBoxD aabb, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            MyEntity entity;
            return (!this.Enabled || (this.IsOutside(aabb) || ((((sourceEntityId == 0) || !MyEntities.TryGetEntityById(sourceEntityId, out entity, false)) || this.IsSafe(entity.GetTopMostParent(null))) && this.AllowedActions.HasFlag(action))));
        }

        public bool IsActionAllowed(Vector3D point, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            MyEntity entity;
            if (!this.Enabled)
            {
                return true;
            }
            bool flag = false;
            if (this.Shape == MySafeZoneShape.Sphere)
            {
                flag = base.PositionComp.WorldVolume.Contains(point) != ContainmentType.Contains;
            }
            else
            {
                MyOrientedBoundingBoxD xd = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                flag = !xd.Contains(ref point);
            }
            return (flag || ((((sourceEntityId == 0) || !MyEntities.TryGetEntityById(sourceEntityId, out entity, false)) || this.IsSafe(entity.GetTopMostParent(null))) && this.AllowedActions.HasFlag(action)));
        }

        private bool IsOutside(MyEntity entity)
        {
            bool flag = false;
            MyOrientedBoundingBoxD other = new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB, entity.PositionComp.WorldMatrix);
            if (this.Shape == MySafeZoneShape.Sphere)
            {
                BoundingSphereD worldVolume = base.PositionComp.WorldVolume;
                flag = !other.Intersects(ref worldVolume);
            }
            else
            {
                MyOrientedBoundingBoxD xd2 = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                flag = !xd2.Intersects(ref other);
            }
            return flag;
        }

        private bool IsOutside(BoundingBoxD aabb)
        {
            bool flag = false;
            if (this.Shape == MySafeZoneShape.Sphere)
            {
                flag = !base.PositionComp.WorldVolume.Intersects(aabb);
            }
            else
            {
                MyOrientedBoundingBoxD xd = new MyOrientedBoundingBoxD(base.PositionComp.LocalAABB, base.PositionComp.WorldMatrix);
                flag = !xd.Intersects(ref aabb);
            }
            return flag;
        }

        private bool IsSafe(MyEntity entity)
        {
            MyIDModule module;
            MyInventoryBagEntity entity2 = entity as MyInventoryBagEntity;
            if ((entity is MyFloatingObject) || (entity2 != null))
            {
                return (!this.Entities.Contains(entity.EntityId) ? (this.AccessTypeFloatingObjects != MySafeZoneAccess.Whitelist) : (this.AccessTypeFloatingObjects == MySafeZoneAccess.Whitelist));
            }
            MyEntity topMostParent = entity.GetTopMostParent(null);
            IMyComponentOwner<MyIDModule> owner = topMostParent as IMyComponentOwner<MyIDModule>;
            if ((owner != null) && owner.GetComponent(out module))
            {
                if (this.AccessTypePlayers == MySafeZoneAccess.Whitelist)
                {
                    if (this.Players.Contains(module.Owner))
                    {
                        return true;
                    }
                }
                else if (this.Players.Contains(module.Owner))
                {
                    return false;
                }
                MyFaction item = MySession.Static.Factions.TryGetPlayerFaction(module.Owner) as MyFaction;
                if (item != null)
                {
                    if (this.AccessTypeFactions == MySafeZoneAccess.Whitelist)
                    {
                        if (this.Factions.Contains(item))
                        {
                            return true;
                        }
                    }
                    else if (this.Factions.Contains(item))
                    {
                        return false;
                    }
                }
                return (this.AccessTypePlayers == MySafeZoneAccess.Blacklist);
            }
            MyCubeGrid grid = topMostParent as MyCubeGrid;
            if (grid == null)
            {
                return (!(entity is MyAmmoBase) || this.AllowedActions.HasFlag(MySafeZoneAction.Shooting));
            }
            if (this.AccessTypeGrids == MySafeZoneAccess.Whitelist)
            {
                if (this.Entities.Contains(topMostParent.EntityId))
                {
                    return true;
                }
            }
            else if (this.Entities.Contains(topMostParent.EntityId))
            {
                return false;
            }
            if (grid.BigOwners.Count > 0)
            {
                using (List<long>.Enumerator enumerator = grid.BigOwners.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        long current = enumerator.Current;
                        MyFaction item = MySession.Static.Factions.TryGetPlayerFaction(current) as MyFaction;
                        if (item != null)
                        {
                            return (!this.Factions.Contains(item) ? (this.AccessTypeFactions != MySafeZoneAccess.Whitelist) : (this.AccessTypeFactions == MySafeZoneAccess.Whitelist));
                        }
                    }
                }
            }
            return (this.AccessTypeGrids == MySafeZoneAccess.Blacklist);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void phantom_Enter(HkPhantomCallbackShape sender, HkRigidBody body)
        {
            MyEntity entity = body.GetEntity(0) as MyEntity;
            bool addedOrRemoved = MySessionComponentSafeZones.IsRecentlyAddedOrRemoved(entity);
            if (this.InsertEntityInternal(entity, addedOrRemoved))
            {
                this.SendInsertedEntity(entity.EntityId, addedOrRemoved);
            }
        }

        private void phantom_Leave(HkPhantomCallbackShape sender, HkRigidBody body)
        {
            IMyEntity entity = body.GetEntity(0);
            if (entity != null)
            {
                MyEntity topEntity = entity.GetTopMostParent(null) as MyEntity;
                if (((topEntity.Physics != null) && !topEntity.Physics.ShapeChangeInProgress) && ReferenceEquals(topEntity, entity))
                {
                    <>c__DisplayClass71_2 class_3;
                    bool addedOrRemoved = MySessionComponentSafeZones.IsRecentlyAddedOrRemoved(topEntity) || !entity.InScene;
                    Vector3D position1 = body.Position;
                    Quaternion rotation1 = Quaternion.CreateFromRotationMatrix(body.GetRigidBodyMatrix());
                    Vector3D position2 = base.PositionComp.GetPosition();
                    Quaternion rotation2 = Quaternion.CreateFromRotationMatrix(base.PositionComp.GetOrientation());
                    MySandboxGame.Static.Invoke(delegate {
                        if (this.Physics != null)
                        {
                            bool flag1;
                            if ((entity.Physics == null) || (entity.Physics.RigidBody == null))
                            {
                                flag1 = true;
                            }
                            else
                            {
                                flag1 = !MyPhysics.IsPenetratingShapeShape(entity.Physics.RigidBody.GetShape(), ref position1, ref rotation1, this.Physics.RigidBody.GetShape(), ref position2, ref rotation2);
                            }
                            if (flag1)
                            {
                                if (this.RemoveEntityInternal(topEntity, addedOrRemoved))
                                {
                                    this.SendRemovedEntity(topEntity.EntityId, addedOrRemoved);
                                }
                                topEntity.OnClose -= new Action<MyEntity>(class_3.entity_OnClose);
                            }
                        }
                    }, "Phantom leave");
                }
            }
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
            this.RecreateBillboards();
        }

        public void RecreateBillboards()
        {
            if (!Sync.IsDedicated)
            {
                this.ClearBillboards();
                Color color = this.Enabled ? Color.CadetBlue : Color.Gray;
                color.A = 100;
                if (this.Shape == MySafeZoneShape.Sphere)
                {
                    this.RecreateSphere(color);
                }
                else
                {
                    this.RecreateBox(color);
                }
            }
        }

        private void RecreateBox(Color color)
        {
            object drawLock = m_drawLock;
            lock (drawLock)
            {
                BoundingBoxD localAABB = base.PositionComp.LocalAABB;
                MyStringId? faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref base.WorldMatrix, ref localAABB, ref color, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 10, 0.002f, faceMaterial, new MyStringId?(MyStringId.GetOrCompute("Square")), false, -1, MyBillboard.BlendTypeEnum.Standard, 100f, this.m_persistentBillboards);
            }
        }

        public void RecreatePhysics(bool insertEntities = true)
        {
            if (base.Physics != null)
            {
                base.Physics.Close();
                base.Physics = null;
            }
            if (this.Shape == MySafeZoneShape.Sphere)
            {
                base.PositionComp.LocalVolume = new BoundingSphere(Vector3.Zero, this.Radius);
            }
            else
            {
                base.PositionComp.LocalAABB = new BoundingBox(-this.Size / 2f, this.Size / 2f);
            }
            this.m_containedEntities.Clear();
            if (Sync.IsServer)
            {
                HkBvShape shape = this.CreateFieldShape();
                base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_KINEMATIC);
                base.Physics.IsPhantom = true;
                HkMassProperties? massProperties = null;
                ((MyPhysicsBody) base.Physics).CreateFromCollisionObject((HkShape) shape, base.PositionComp.LocalVolume.Center, base.WorldMatrix, massProperties, 15);
                shape.Base.RemoveReference();
                base.Physics.Enabled = true;
                if (insertEntities)
                {
                    this.InsertContainingEntities();
                }
            }
            if (!Sync.IsDedicated)
            {
                this.RecreateBillboards();
            }
        }

        private void RecreateSphere(Color color)
        {
            object drawLock = m_drawLock;
            lock (drawLock)
            {
                MyStringId? faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentSphere(ref base.WorldMatrix, this.Radius, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 0x24, faceMaterial, new MyStringId?(MyStringId.GetOrCompute("Square")), 0.2f, -1, this.m_persistentBillboards, MyBillboard.BlendTypeEnum.Standard, 100f);
            }
        }

        [Event(null, 0x2ad), Reliable, BroadcastExcept]
        private void RemoveEntity_Implementation(long entityId, bool addedOrRemoved)
        {
            if (this.m_containedEntities.Contains(entityId))
            {
                MyEntity entity;
                this.m_containedEntities.Remove(entityId);
                if (MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    this.RemoveEntityLocal(entity, addedOrRemoved);
                }
            }
        }

        internal bool RemoveEntityInternal(MyEntity entity, bool addedOrRemoved)
        {
            bool flag1 = this.m_containedEntities.Remove(entity.EntityId);
            if (flag1)
            {
                this.RemoveEntityLocal(entity, addedOrRemoved);
            }
            return flag1;
        }

        private void RemoveEntityLocal(MyEntity entity, bool addedOrRemoved)
        {
            if (((this.Enabled && ((MySession.Static != null) && ((MySession.Static.ControlledEntity != null) && (ReferenceEquals(((MyEntity) MySession.Static.ControlledEntity).GetTopMostParent(null), entity) && this.IsSafe(entity))))) && !addedOrRemoved) && (!(entity is MyCharacter) || !((entity as MyCharacter).IsUsing is MyCockpit)))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                MyHud.Notifications.Add(this.m_safezoneLeftNotification);
            }
        }

        private void SendInsertedEntities(List<long> list)
        {
            if (base.IsReadyForReplication)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MySafeZone, List<long>>(this, x => new Action<List<long>>(x.InsertEntities_Implementation), list, targetEndpoint);
            }
        }

        private void SendInsertedEntity(long entityId, bool addedOrRemoved)
        {
            if (base.IsReadyForReplication)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MySafeZone, long, bool>(this, x => new Action<long, bool>(x.InsertEntity_Implementation), entityId, addedOrRemoved, targetEndpoint);
            }
        }

        private void SendRemovedEntity(long entityId, bool addedOrRemoved)
        {
            if (base.IsReadyForReplication)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MySafeZone, long, bool>(this, x => new Action<long, bool>(x.RemoveEntity_Implementation), entityId, addedOrRemoved, targetEndpoint);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (Sync.IsServer && this.Enabled)
            {
                using (ConcurrentEnumerator<SpinLockRef.Token, long, HashSet<long>.Enumerator> enumerator = this.m_containedEntities.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityById(enumerator.Current, out entity, false))
                        {
                            continue;
                        }
                        if (!entity.Physics.IsKinematic && (!entity.Physics.IsStatic && !this.IsSafe(entity)))
                        {
                            MyAmmoBase base2 = entity as MyAmmoBase;
                            if (base2 != null)
                            {
                                base2.MarkForDestroy();
                                continue;
                            }
                            Vector3D up = entity.PositionComp.GetPosition() - base.PositionComp.GetPosition();
                            if (up.LengthSquared() > 0.10000000149011612)
                            {
                                up.Normalize();
                            }
                            else
                            {
                                up = Vector3.Up;
                            }
                            Vector3D vectord2 = (up * entity.Physics.Mass) * 1000.0;
                            Vector3D? position = null;
                            Vector3? torque = null;
                            float? maxSpeed = null;
                            entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, new Vector3?((Vector3) vectord2), position, torque, maxSpeed, true, false);
                        }
                    }
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (Sync.IsServer)
            {
                this.InsertContainingEntities();
            }
            else
            {
                this.m_containedEntities.Clear();
                foreach (long num in this.m_entitiesToAdd)
                {
                    this.InsertEntity_Implementation(num, false);
                }
                this.m_entitiesToAdd.Clear();
            }
            if (!Sync.IsDedicated)
            {
                this.RecreateBillboards();
            }
        }

        public bool Enabled { get; set; }

        public MySafeZoneAccess AccessTypePlayers { get; set; }

        public MySafeZoneAccess AccessTypeFactions { get; set; }

        public MySafeZoneAccess AccessTypeGrids { get; set; }

        public MySafeZoneAccess AccessTypeFloatingObjects { get; set; }

        public MySafeZoneAction AllowedActions { get; set; }

        public MySafeZoneShape Shape { get; set; }

        public Vector3 Size
        {
            get => 
                this.m_size;
            set
            {
                if (this.m_size != value)
                {
                    this.m_size = value;
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySafeZone.<>c <>9 = new MySafeZone.<>c();
            public static Converter<long, MyFaction> <>9__51_0;
            public static Func<MyFaction, bool> <>9__51_1;
            public static Converter<MyFaction, long> <>9__52_0;
            public static Func<MySafeZone, Action<long, bool>> <>9__72_0;
            public static Func<MySafeZone, Action<List<long>>> <>9__73_0;
            public static Func<MySafeZone, Action<long, bool>> <>9__74_0;

            internal long <GetObjectBuilder>b__52_0(MyFaction x) => 
                x.FactionId;

            internal MyFaction <InitInternal>b__51_0(long x) => 
                ((MyFaction) MySession.Static.Factions.TryGetFactionById(x));

            internal bool <InitInternal>b__51_1(MyFaction x) => 
                (x != null);

            internal Action<List<long>> <SendInsertedEntities>b__73_0(MySafeZone x) => 
                new Action<List<long>>(x.InsertEntities_Implementation);

            internal Action<long, bool> <SendInsertedEntity>b__72_0(MySafeZone x) => 
                new Action<long, bool>(x.InsertEntity_Implementation);

            internal Action<long, bool> <SendRemovedEntity>b__74_0(MySafeZone x) => 
                new Action<long, bool>(x.RemoveEntity_Implementation);
        }
    }
}


namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEntityType(typeof(MyObjectBuilder_Missile), true)]
    public sealed class MyMissile : MyAmmoBase, IMyEventProxy, IMyEventOwner, IMyDestroyableObject
    {
        private MyMissileAmmoDefinition m_missileAmmoDefinition;
        private float m_maxTrajectory;
        private MyParticleEffect m_smokeEffect;
        private MyExplosionTypeEnum m_explosionType;
        private MyEntity m_collidedEntity;
        private Vector3D? m_collisionPoint;
        private Vector3 m_collisionNormal;
        private long m_owner;
        private readonly float m_smokeEffectOffsetMultiplier = 0.4f;
        private Vector3 m_linearVelocity;
        private MyWeaponPropertiesWrapper m_weaponProperties;
        private long m_launcherId;
        public static bool DEBUG_DRAW_MISSILE_TRAJECTORY;
        internal int m_pruningProxyId = -1;
        private readonly MyEntity3DSoundEmitter m_soundEmitter;
        private bool m_removed;

        public MyMissile()
        {
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
            if (MySession.Static.Settings.RealisticSound && MyFakes.ENABLE_NEW_SOUNDS)
            {
                Func<bool> entity = () => (MySession.Static.ControlledEntity != null) && ((MySession.Static.ControlledEntity.Entity is MyCharacter) && ReferenceEquals(MySession.Static.ControlledEntity.Entity, this.m_collidedEntity));
                this.m_soundEmitter.EmitterMethods[1].Add(entity);
                this.m_soundEmitter.EmitterMethods[0].Add(entity);
            }
            base.Flags |= EntityFlags.IsNotGamePrunningStructureObject;
            if (Sync.IsDedicated)
            {
                base.Flags &= ~EntityFlags.UpdateRender;
                base.InvalidateOnMove = false;
            }
        }

        protected override void Closing()
        {
            base.Closing();
            this.Done();
        }

        private void DoDamage(float damage, MyStringHash damageType, bool sync, long attackerId)
        {
            if (sync)
            {
                if (Sync.IsServer)
                {
                    MySyncDamage.DoDamageSynced(this, damage, damageType, attackerId);
                }
            }
            else
            {
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseDestroyed(this, new MyDamageInformation(false, damage, damageType, attackerId));
                }
                this.MarkForExplosion();
            }
        }

        private void Done()
        {
            if (this.m_collidedEntity != null)
            {
                this.m_collidedEntity.Unpin();
                this.m_collidedEntity = null;
            }
        }

        private unsafe void ExecuteExplosion()
        {
            if (!Sync.IsServer)
            {
                this.Return();
            }
            else
            {
                this.PlaceDecal();
                float missileExplosionRadius = this.m_missileAmmoDefinition.MissileExplosionRadius;
                BoundingSphereD ed = new BoundingSphereD(base.PositionComp.GetPosition(), (double) missileExplosionRadius);
                MyEntity character = null;
                MyIdentity identity = Sync.Players.TryGetIdentity(this.m_owner);
                if (identity != null)
                {
                    character = identity.Character;
                }
                MyExplosionInfo explosionInfo = new MyExplosionInfo {
                    PlayerDamage = 0f,
                    Damage = this.m_missileAmmoDefinition.MissileExplosionDamage,
                    ExplosionType = this.m_explosionType,
                    ExplosionSphere = ed,
                    LifespanMiliseconds = 700,
                    HitEntity = this.m_collidedEntity,
                    ParticleScale = 1f,
                    OwnerEntity = character,
                    Direction = new Vector3?(Vector3.Normalize(base.PositionComp.GetPosition() - base.m_origin)),
                    VoxelExplosionCenter = ed.Center + ((missileExplosionRadius * base.WorldMatrix.Forward) * 0.25),
                    ExplosionFlags = MyExplosionFlags.CREATE_PARTICLE_DEBRIS | MyExplosionFlags.APPLY_DEFORMATION | MyExplosionFlags.CREATE_SHRAPNELS | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.AFFECT_VOXELS,
                    VoxelCutoutScale = 0.3f,
                    PlaySound = true,
                    ApplyForceAndDamage = true,
                    OriginEntity = base.m_originEntity,
                    KeepAffectedBlocks = true
                };
                if ((this.m_collidedEntity != null) && (this.m_collidedEntity.Physics != null))
                {
                    explosionInfo.Velocity = this.m_collidedEntity.Physics.LinearVelocity;
                }
                if (!base.m_markedToDestroy)
                {
                    MyExplosionFlags* flagsPtr1 = (MyExplosionFlags*) ref explosionInfo.ExplosionFlags;
                    *((int*) flagsPtr1) |= 0x20;
                }
                MyExplosions.AddExplosion(ref explosionInfo, true);
                if (((this.m_collidedEntity != null) && (!(this.m_collidedEntity is MyAmmoBase) && (this.m_collidedEntity.Physics != null))) && !this.m_collidedEntity.Physics.IsStatic)
                {
                    Vector3? torque = null;
                    float? maxSpeed = null;
                    this.m_collidedEntity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?((Vector3) (100f * base.Physics.LinearVelocity)), this.m_collisionPoint, torque, maxSpeed, true, false);
                }
                this.Return();
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_Missile objectBuilder = (MyObjectBuilder_Missile) base.GetObjectBuilder(copy);
            objectBuilder.LinearVelocity = base.Physics.LinearVelocity;
            objectBuilder.AmmoMagazineId = (SerializableDefinitionId) this.m_weaponProperties.AmmoMagazineId;
            objectBuilder.WeaponDefinitionId = (SerializableDefinitionId) this.m_weaponProperties.WeaponDefinitionId;
            objectBuilder.Owner = this.m_owner;
            objectBuilder.OriginEntity = base.m_originEntity;
            objectBuilder.LauncherId = this.m_launcherId;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyObjectBuilder_Missile missile = (MyObjectBuilder_Missile) objectBuilder;
            base.Init(objectBuilder);
            this.m_weaponProperties = new MyWeaponPropertiesWrapper(missile.WeaponDefinitionId);
            this.m_weaponProperties.ChangeAmmoMagazine(missile.AmmoMagazineId);
            this.m_missileAmmoDefinition = this.m_weaponProperties.GetCurrentAmmoDefinitionAs<MyMissileAmmoDefinition>();
            base.Init(this.m_weaponProperties, this.m_missileAmmoDefinition.MissileModelName, false, true, true, Sync.IsServer);
            this.UseDamageSystem = true;
            this.m_maxTrajectory = this.m_missileAmmoDefinition.MaxTrajectory;
            base.SyncFlag = true;
            this.m_collisionPoint = null;
            this.m_owner = missile.Owner;
            base.m_originEntity = missile.OriginEntity;
            this.m_linearVelocity = missile.LinearVelocity;
            this.m_launcherId = missile.LauncherId;
            base.OnPhysicsChanged += new Action<MyEntity>(this.OnMissilePhysicsChanged);
        }

        public override void MarkForDestroy()
        {
            this.Return();
        }

        private void MarkForExplosion()
        {
            if (base.m_markedToDestroy)
            {
                this.Return();
            }
            else
            {
                base.m_shouldExplode = true;
            }
            if (Sync.IsServer && !this.m_removed)
            {
                IMyMissileGunObject entityById = MyEntities.GetEntityById(this.m_launcherId, false) as IMyMissileGunObject;
                if (entityById != null)
                {
                    entityById.RemoveMissile(base.EntityId);
                }
                this.m_removed = true;
            }
        }

        public override unsafe void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.m_shouldExplode = false;
            base.Start(base.PositionComp.GetPosition(), this.m_linearVelocity, base.WorldMatrix.Forward);
            if (base.m_physicsEnabled)
            {
                base.Physics.RigidBody.MaxLinearVelocity = this.m_missileAmmoDefinition.DesiredSpeed;
                base.Physics.RigidBody.Layer = 8;
                base.Physics.CanUpdateAccelerations = false;
            }
            this.m_explosionType = MyExplosionTypeEnum.MISSILE_EXPLOSION;
            if (!Sync.IsDedicated)
            {
                MySoundPair shootSound = base.m_weaponDefinition.WeaponAmmoDatas[1].ShootSound;
                if (shootSound != null)
                {
                    bool? nullable = null;
                    this.m_soundEmitter.PlaySingleSound(shootSound, true, false, false, nullable);
                }
                MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation -= worldMatrix.Forward * this.m_smokeEffectOffsetMultiplier;
                Vector3D translation = worldMatrix.Translation;
                MyParticlesManager.TryCreateParticleEffect("Smoke_Missile", ref MatrixD.Identity, ref translation, base.Render.GetRenderObjectID(), out this.m_smokeEffect);
                IMyMissileGunObject entityById = MyEntities.GetEntityById(this.m_launcherId, false) as IMyMissileGunObject;
                if (entityById != null)
                {
                    entityById.MissileShootEffect();
                }
            }
        }

        protected override void OnContactStart(ref MyPhysics.MyContactPointEvent value)
        {
            if (!base.MarkedForClose && (this.m_collidedEntity == null))
            {
                MyEntity otherEntity = value.ContactPointEvent.GetOtherEntity(this) as MyEntity;
                if (otherEntity != null)
                {
                    otherEntity.Pin();
                    this.m_collidedEntity = otherEntity;
                    this.m_collisionPoint = new Vector3D?(value.Position);
                    this.m_collisionNormal = value.Normal;
                    if (!Sync.IsServer)
                    {
                        this.PlaceDecal();
                    }
                    else
                    {
                        this.MarkForExplosion();
                    }
                }
            }
        }

        private void OnMissilePhysicsChanged(MyEntity entity)
        {
            if ((base.Physics != null) && (base.Physics.RigidBody != null))
            {
                base.Physics.RigidBody.CallbackLimit = 1;
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            if (this.m_smokeEffect != null)
            {
                this.m_smokeEffect.Stop(false);
                this.m_smokeEffect = null;
            }
            this.m_soundEmitter.StopSound(true, true);
        }

        private void PlaceDecal()
        {
            if ((this.m_collidedEntity != null) && (this.m_collisionPoint != null))
            {
                MyHitInfo hitInfo = new MyHitInfo {
                    Position = this.m_collisionPoint.Value,
                    Normal = this.m_collisionNormal
                };
                MyStringHash source = new MyStringHash();
                MyDecals.HandleAddDecal(this.m_collidedEntity, hitInfo, this.m_missileAmmoDefinition.PhysicalMaterial, source, null, -1f);
            }
        }

        public static MyObjectBuilder_Missile PrepareBuilder(MyWeaponPropertiesWrapper weaponProperties, Vector3D position, Vector3D initialVelocity, Vector3D direction, long owner, long originEntity, long launcherId)
        {
            MyObjectBuilder_Missile local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Missile>();
            local1.LinearVelocity = (Vector3) initialVelocity;
            local1.AmmoMagazineId = (SerializableDefinitionId) weaponProperties.AmmoMagazineId;
            local1.WeaponDefinitionId = (SerializableDefinitionId) weaponProperties.WeaponDefinitionId;
            local1.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            Vector3D to = position + (direction * 4.0);
            if (MyPhysics.CastRay(position, to, 0) == null)
            {
                position = to;
            }
            MyObjectBuilder_Missile local2 = local1;
            local2.PositionAndOrientation = new MyPositionAndOrientation(position, (Vector3) direction, (Vector3) Vector3D.CalculatePerpendicularVector(direction));
            local2.Owner = owner;
            local2.OriginEntity = originEntity;
            local2.LauncherId = launcherId;
            local2.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            return local2;
        }

        private void Return()
        {
            this.Done();
            MyMissiles.Return(this);
        }

        public override void UpdateBeforeSimulation()
        {
            if (base.m_shouldExplode)
            {
                this.ExecuteExplosion();
            }
            else
            {
                base.UpdateBeforeSimulation();
                Vector3D position = base.PositionComp.GetPosition();
                if (base.m_physicsEnabled)
                {
                    this.m_linearVelocity = base.Physics.LinearVelocity;
                    base.Physics.AngularVelocity = Vector3.Zero;
                }
                if (this.m_missileAmmoDefinition.MissileSkipAcceleration)
                {
                    this.m_linearVelocity = (Vector3) (base.WorldMatrix.Forward * this.m_missileAmmoDefinition.DesiredSpeed);
                }
                else
                {
                    this.m_linearVelocity += (base.PositionComp.WorldMatrix.Forward * this.m_missileAmmoDefinition.MissileAcceleration) * 0.01666666753590107;
                }
                if (base.m_physicsEnabled)
                {
                    base.Physics.LinearVelocity = this.m_linearVelocity;
                }
                else
                {
                    Vector3.ClampToSphere(ref this.m_linearVelocity, this.m_missileAmmoDefinition.DesiredSpeed);
                    base.PositionComp.SetPosition(base.PositionComp.GetPosition() + (this.m_linearVelocity * 0.01666667f), null, false, true);
                }
                if (Vector3.DistanceSquared((Vector3) base.PositionComp.GetPosition(), (Vector3) base.m_origin) >= (this.m_maxTrajectory * this.m_maxTrajectory))
                {
                    this.MarkForExplosion();
                }
                if (DEBUG_DRAW_MISSILE_TRAJECTORY)
                {
                    MyRenderProxy.DebugDrawLine3D(position, base.PositionComp.GetPosition(), Color.AliceBlue, Color.AliceBlue, true, false);
                }
                MyMissiles.OnMissileMoved(this, ref this.m_linearVelocity);
            }
        }

        public void UpdateData(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyObjectBuilder_Missile missile = (MyObjectBuilder_Missile) objectBuilder;
            if (objectBuilder.PositionAndOrientation != null)
            {
                MyPositionAndOrientation orientation = objectBuilder.PositionAndOrientation.Value;
                MatrixD worldMatrix = MatrixD.CreateWorld((Vector3D) orientation.Position, (Vector3) orientation.Forward, (Vector3) orientation.Up);
                base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, true);
            }
            base.EntityId = missile.EntityId;
            this.m_owner = missile.Owner;
            base.m_originEntity = missile.OriginEntity;
            this.m_linearVelocity = missile.LinearVelocity;
            this.m_launcherId = missile.LauncherId;
            this.m_collisionPoint = null;
            base.m_markedToDestroy = false;
            this.m_removed = false;
        }

        bool IMyDestroyableObject.DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId)
        {
            this.DoDamage(damage, damageType, sync, attackerId);
            return true;
        }

        void IMyDestroyableObject.OnDestroy()
        {
        }

        public SerializableDefinitionId AmmoMagazineId =>
            ((SerializableDefinitionId) this.m_weaponProperties.AmmoMagazineId);

        public SerializableDefinitionId WeaponDefinitionId =>
            ((SerializableDefinitionId) this.m_weaponProperties.WeaponDefinitionId);

        private bool UseDamageSystem { get; set; }

        public long Owner =>
            this.m_owner;

        float IMyDestroyableObject.Integrity =>
            1f;

        bool IMyDestroyableObject.UseDamageSystem =>
            this.UseDamageSystem;
    }
}


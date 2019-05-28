namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_Meteor), true)]
    public class MyMeteor : VRage.Game.Entity.MyEntity, IMyDestroyableObject, IMyDecalProxy, IMyMeteor, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEventProxy, IMyEventOwner
    {
        private static readonly int MAX_TRAJECTORY_LENGTH = 0x2710;
        private static readonly int SPEED = 90;
        private MyMeteorGameLogic m_logic;
        private bool m_hasModifiableDamage;

        public MyMeteor()
        {
            base.Components.ComponentAdded += new Action<System.Type, MyEntityComponentBase>(this.Components_ComponentAdded);
            this.GameLogic = new MyMeteorGameLogic();
            base.Render = new MyRenderComponentDebrisVoxel();
        }

        private void Components_ComponentAdded(System.Type arg1, MyComponentBase arg2)
        {
            if (arg1 == typeof(MyGameLogicComponent))
            {
                this.m_logic = arg2 as MyMeteorGameLogic;
            }
        }

        public bool DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId)
        {
            this.GameLogic.DoDamage(damage, damageType, sync, hitInfo, attackerId);
            return true;
        }

        private static string GetMaterialName()
        {
            string minedOre = "Stone";
            bool flag = false;
            MyVoxelMaterialDefinition definition = null;
            foreach (MyVoxelMaterialDefinition definition2 in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (definition2.MinedOre == minedOre)
                {
                    flag = true;
                    break;
                }
                definition = definition2;
            }
            if (!flag && (definition != null))
            {
                minedOre = definition.MinedOre;
            }
            return minedOre;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            this.GameLogic.GetObjectBuilder(false);

        public void OnDestroy()
        {
            this.GameLogic.OnDestroy();
        }

        private static MyObjectBuilder_Meteor PrepareBuilder(ref MyPhysicalInventoryItem item)
        {
            MyObjectBuilder_Meteor local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Meteor>();
            local1.Item = item.GetObjectBuilder();
            local1.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            return local1;
        }

        private static void SetSpawnSettings(VRage.Game.Entity.MyEntity meteorEntity, Vector3D position, Vector3 speed)
        {
            Vector3 forward = -MySector.DirectionToSunNormalized;
            Vector3 vector2 = MyUtils.GetRandomVector3Normalized();
            while (forward == vector2)
            {
                vector2 = MyUtils.GetRandomVector3Normalized();
            }
            meteorEntity.WorldMatrix = MatrixD.CreateWorld(position, forward, Vector3.Cross(Vector3.Cross(forward, vector2), forward));
            meteorEntity.Physics.RigidBody.MaxLinearVelocity = 500f;
            meteorEntity.Physics.LinearVelocity = speed;
            meteorEntity.Physics.AngularVelocity = MyUtils.GetRandomVector3Normalized() * MyUtils.GetRandomFloat(1.5f, 3f);
        }

        public static VRage.Game.Entity.MyEntity Spawn(ref MyPhysicalInventoryItem item, Vector3D position, Vector3 speed)
        {
            Vector3D? relativeOffset = null;
            return Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderParallel(PrepareBuilder(ref item), true, delegate (VRage.Game.Entity.MyEntity x) {
                SetSpawnSettings(x, position, speed);
            }, null, null, relativeOffset, false, false);
        }

        public static VRage.Game.Entity.MyEntity SpawnRandom(Vector3D position, Vector3 direction)
        {
            string materialName = GetMaterialName();
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem(500 * ((MyFixedPoint) MyUtils.GetRandomFloat(1f, 3f)), MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(materialName), 1f);
            return Spawn(ref item, position, direction * SPEED);
        }

        void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
        }

        public MyMeteorGameLogic GameLogic
        {
            get => 
                this.m_logic;
            set => 
                (base.GameLogic = value);
        }

        public override bool IsCCDForProjectiles =>
            true;

        public float Integrity =>
            this.GameLogic.Integrity;

        public bool UseDamageSystem =>
            this.m_hasModifiableDamage;

        public class MyMeteorGameLogic : MyEntityGameLogic
        {
            private const int VISIBLE_RANGE_MAX_DISTANCE_SQUARED = 0x895440;
            public MyPhysicalInventoryItem Item;
            private StringBuilder m_textCache;
            private float m_integrity = 100f;
            private string[] m_particleEffectNames = new string[2];
            private MyParticleEffect m_dustEffect;
            private int m_timeCreated;
            private Vector3 m_particleVectorForward = Vector3.Zero;
            private Vector3 m_particleVectorUp = Vector3.Zero;
            private MeteorStatus m_meteorStatus = MeteorStatus.InSpace;
            private MyEntity3DSoundEmitter m_soundEmitter = new MyEntity3DSoundEmitter(null, false, 1f);
            private bool m_closeAfterSimulation;
            private MySoundPair m_meteorFly = new MySoundPair("MeteorFly", true);
            private MySoundPair m_meteorExplosion = new MySoundPair("MeteorExplosion", true);

            public override void Close()
            {
                if (this.m_dustEffect != null)
                {
                    this.m_dustEffect.Stop(true);
                    this.m_dustEffect = null;
                }
                base.Close();
            }

            private void CloseMeteorInternal()
            {
                if (this.Entity.Physics != null)
                {
                    this.Entity.Physics.Enabled = false;
                    this.Entity.Physics.Deactivate();
                }
                this.MarkForClose();
            }

            private void CreateCrater(MyPhysics.MyContactPointEvent value, MyVoxelBase voxel)
            {
                BoundingSphereD ed;
                Vector3 vector2;
                MyVoxelMaterialDefinition voxelMaterial;
                if (Math.Abs(Vector3.Normalize(-this.Entity.WorldMatrix.Forward).Dot(value.ContactPointEvent.ContactPoint.Normal)) < 0.1)
                {
                    MyParticleEffect effect;
                    if (this.InParticleVisibleRange && MyParticlesManager.TryCreateParticleEffect("Meteorit_Smoke1AfterHit", this.Entity.WorldMatrix, out effect))
                    {
                        effect.UserScale = ((float) this.Entity.PositionComp.WorldVolume.Radius) * 2f;
                    }
                    this.m_particleVectorUp = Vector3.Zero;
                    this.m_closeAfterSimulation = Sync.IsServer;
                    return;
                }
                if (!Sync.IsServer)
                {
                    goto TR_0004;
                }
                else
                {
                    float radius = this.Entity.PositionComp.Scale.Value * 5f;
                    ed = new BoundingSphere((Vector3) value.Position, radius);
                    if (value.ContactPointEvent.SeparatingVelocity < 0f)
                    {
                        vector2 = Vector3.Normalize(this.Entity.Physics.LinearVelocity);
                    }
                    else
                    {
                        vector2 = Vector3.Normalize(Vector3.Reflect(this.Entity.Physics.LinearVelocity, value.ContactPointEvent.ContactPoint.Normal));
                    }
                    voxelMaterial = this.VoxelMaterial;
                    int num2 = MyDefinitionManager.Static.GetVoxelMaterialDefinitions().Count<MyVoxelMaterialDefinition>() * 2;
                    while ((!voxelMaterial.IsRare || (!voxelMaterial.SpawnsFromMeteorites || (voxelMaterial.MinVersion > MySession.Static.Settings.VoxelGeneratorVersion))) || (voxelMaterial.MaxVersion < MySession.Static.Settings.VoxelGeneratorVersion))
                    {
                        if (--num2 < 0)
                        {
                            voxelMaterial = this.VoxelMaterial;
                            break;
                        }
                        voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinitions().ElementAt<MyVoxelMaterialDefinition>(MyUtils.GetRandomInt(MyDefinitionManager.Static.GetVoxelMaterialDefinitions().Count<MyVoxelMaterialDefinition>() - 1));
                    }
                }
                voxel.CreateVoxelMeteorCrater(ed.Center, (float) ed.Radius, -vector2, voxelMaterial);
                MyVoxelGenerator.MakeCrater(voxel, ed, -vector2, voxelMaterial);
            TR_0004:
                this.m_soundEmitter.Entity = voxel;
                this.m_soundEmitter.SetPosition(new Vector3D?(this.Entity.PositionComp.GetPosition()));
                this.m_closeAfterSimulation = Sync.IsServer;
            }

            private unsafe void DestroyGrid(ref MyPhysics.MyContactPointEvent value, MyCubeGrid grid)
            {
                HkBreakOffPointInfo* infoPtr1;
                HkBreakOffPointInfo* infoPtr2;
                HkRigidBody bodyA;
                MyGridContactInfo info = new MyGridContactInfo(ref value.ContactPointEvent, grid) {
                    EnableDeformation = false,
                    EnableParticles = false
                };
                HkBreakOffPointInfo info3 = new HkBreakOffPointInfo {
                    ContactPoint = value.ContactPointEvent.ContactPoint,
                    ContactPosition = info.ContactPosition,
                    ContactPointProperties = value.ContactPointEvent.ContactProperties,
                    IsContact = true,
                    BreakingImpulse = grid.Physics.Shape.BreakImpulse
                };
                if (value.ContactPointEvent.Base.BodyA != grid.Physics.RigidBody)
                {
                    bodyA = value.ContactPointEvent.Base.BodyA;
                }
                else
                {
                    bodyA = value.ContactPointEvent.Base.BodyB;
                }
                infoPtr1->CollidingBody = bodyA;
                infoPtr1 = (HkBreakOffPointInfo*) ref info3;
                infoPtr2->ContactPointDirection = (value.ContactPointEvent.Base.BodyB == grid.Physics.RigidBody) ? ((float) (-1)) : ((float) 1);
                infoPtr2 = (HkBreakOffPointInfo*) ref info3;
                HkBreakOffPointInfo pt = info3;
                this.m_soundEmitter.Entity = grid;
                this.m_soundEmitter.SetPosition(new Vector3D?(this.Entity.PositionComp.GetPosition()));
                grid.Physics.PerformMeteoritDeformation(ref pt, value.ContactPointEvent.SeparatingVelocity);
                this.m_closeAfterSimulation = Sync.IsServer;
            }

            private void DestroyMeteor()
            {
                MyParticleEffect effect;
                if (this.InParticleVisibleRange && MyParticlesManager.TryCreateParticleEffect("Meteorit_Smoke1AfterHit", this.GetParticlePosition(), out effect))
                {
                    effect.UserScale = MyUtils.GetRandomFloat(0.8f, 1.2f);
                }
                if (this.m_dustEffect != null)
                {
                    this.m_dustEffect.StopEmitting(10f);
                    this.m_dustEffect.StopLights();
                    this.m_dustEffect = null;
                }
                this.PlayExplosionSound();
            }

            public void DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId)
            {
                if (sync)
                {
                    if (Sync.IsServer)
                    {
                        MySyncDamage.DoDamageSynced(this.Entity, damage, damageType, attackerId);
                    }
                }
                else
                {
                    MyDamageInformation info = new MyDamageInformation(false, damage, damageType, attackerId);
                    if (this.Entity != null)
                    {
                        if (this.Entity.UseDamageSystem)
                        {
                            MyDamageSystem.Static.RaiseBeforeDamageApplied(this.Entity, ref info);
                        }
                        this.m_integrity -= info.Amount;
                        if (this.Entity.UseDamageSystem)
                        {
                            MyDamageSystem.Static.RaiseAfterDamageApplied(this.Entity, info);
                        }
                        if ((this.m_integrity <= 0f) && Sync.IsServer)
                        {
                            this.m_closeAfterSimulation = Sync.IsServer;
                            if (this.Entity.UseDamageSystem)
                            {
                                MyDamageSystem.Static.RaiseDestroyed(this.Entity, info);
                            }
                        }
                    }
                }
            }

            public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
            {
                MyObjectBuilder_Meteor objectBuilder = (MyObjectBuilder_Meteor) base.GetObjectBuilder(copy);
                if ((this.Entity == null) || (this.Entity.Physics == null))
                {
                    objectBuilder.LinearVelocity = Vector3.One * 10f;
                    objectBuilder.AngularVelocity = Vector3.Zero;
                }
                else
                {
                    objectBuilder.LinearVelocity = this.Entity.Physics.LinearVelocity;
                    objectBuilder.AngularVelocity = this.Entity.Physics.AngularVelocity;
                }
                if (base.GameLogic != null)
                {
                    objectBuilder.Item = this.Item.GetObjectBuilder();
                    objectBuilder.Integrity = this.Integrity;
                }
                return objectBuilder;
            }

            private MatrixD GetParticlePosition()
            {
                if (this.m_particleVectorUp != Vector3.Zero)
                {
                    return MatrixD.CreateWorld(this.Entity.WorldMatrix.Translation, this.m_particleVectorForward, this.m_particleVectorUp);
                }
                return this.Entity.WorldMatrix;
            }

            protected virtual HkShape GetPhysicsShape(HkMassProperties massProperties, float mass, float scale)
            {
                Vector3 halfExtents = (this.Entity.Render.GetModel().BoundingBox.Max - this.Entity.Render.GetModel().BoundingBox.Min) / 2f;
                massProperties = (this.VoxelMaterial == null) ? HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(halfExtents, mass) : HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(this.Entity.Render.GetModel().BoundingSphere.Radius * scale, mass);
                return MyDebris.Static.GetDebrisShape(this.Entity.Render.GetModel(), HkShapeType.ConvexVertices, scale);
            }

            public override void Init(MyObjectBuilder_EntityBase objectBuilder)
            {
                this.Entity.SyncFlag = true;
                base.Init(objectBuilder);
                MyObjectBuilder_Meteor meteor = (MyObjectBuilder_Meteor) objectBuilder;
                this.Item = new MyPhysicalInventoryItem(meteor.Item);
                this.m_particleEffectNames[0] = "Meteory_Fire_Atmosphere";
                this.m_particleEffectNames[1] = "Meteory_Fire_Space";
                this.InitInternal();
                this.Entity.Physics.LinearVelocity = meteor.LinearVelocity;
                this.Entity.Physics.AngularVelocity = meteor.AngularVelocity;
                this.m_integrity = meteor.Integrity;
            }

            private void InitInternal()
            {
                MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(this.Item.Content);
                MyObjectBuilder_Ore content = this.Item.Content as MyObjectBuilder_Ore;
                string model = physicalItemDefinition.Model;
                float num = 1f;
                this.VoxelMaterial = null;
                if (content != null)
                {
                    foreach (MyVoxelMaterialDefinition definition2 in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
                    {
                        if (definition2.MinedOre == content.SubtypeName)
                        {
                            this.VoxelMaterial = definition2;
                            model = MyDebris.GetAmountBasedDebrisVoxel((float) this.Item.Amount);
                            num = (float) Math.Pow((double) ((((float) this.Item.Amount) * physicalItemDefinition.Volume) / MyDebris.VoxelDebrisModelVolume), 0.33300000429153442);
                            break;
                        }
                    }
                }
                if (num < 0.15f)
                {
                    num = 0.15f;
                }
                MyRenderComponentDebrisVoxel render = this.Entity.Render as MyRenderComponentDebrisVoxel;
                render.VoxelMaterialIndex = this.VoxelMaterial.Index;
                render.TexCoordOffset = 5f;
                render.TexCoordScale = 8f;
                float? scale = null;
                this.Entity.Init(new StringBuilder("Meteor"), model, null, scale, null);
                this.Entity.PositionComp.Scale = new float?(num);
                HkMassProperties properties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(this.Entity.PositionComp.LocalVolume.Radius, ((float) (4.1887903296220665 * Math.Pow((double) this.Entity.PositionComp.LocalVolume.Radius, 3.0))) * 3.7f);
                HkSphereShape shape = new HkSphereShape(this.Entity.PositionComp.LocalVolume.Radius);
                if (this.Entity.Physics != null)
                {
                    this.Entity.Physics.Close();
                }
                this.Entity.Physics = new MyPhysicsBody(this.Entity, RigidBodyFlag.RBF_BULLET);
                this.Entity.Physics.ReportAllContacts = true;
                this.Entity.GetPhysicsBody().CreateFromCollisionObject((HkShape) shape, Vector3.Zero, MatrixD.Identity, new HkMassProperties?(properties), 15);
                this.Entity.Physics.Enabled = true;
                this.Entity.Physics.RigidBody.ContactPointCallbackEnabled = true;
                shape.Base.RemoveReference();
                this.Entity.Physics.PlayCollisionCueEnabled = true;
                this.m_timeCreated = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                base.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
                this.StartLoopSound();
            }

            public override void MarkForClose()
            {
                this.DestroyMeteor();
                base.MarkForClose();
            }

            public override void OnAddedToContainer()
            {
                base.OnAddedToContainer();
                this.m_soundEmitter.Entity = base.Container.Entity as VRage.Game.Entity.MyEntity;
            }

            public override void OnAddedToScene()
            {
                base.OnAddedToScene();
                this.Entity.GetPhysicsBody().ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.RigidBody_ContactPointCallback);
            }

            public void OnDestroy()
            {
            }

            private void PlayExplosionSound()
            {
                this.m_soundEmitter.SetVelocity(new Vector3?(Vector3.Zero));
                this.m_soundEmitter.SetPosition(new Vector3D?(this.Entity.PositionComp.GetPosition()));
                bool? nullable = null;
                this.m_soundEmitter.PlaySingleSound(this.m_meteorExplosion, true, false, false, nullable);
            }

            private void RigidBody_ContactPointCallback(ref MyPhysics.MyContactPointEvent value)
            {
                if ((!base.MarkedForClose && this.Entity.Physics.Enabled) && !this.m_closeAfterSimulation)
                {
                    VRage.ModAPI.IMyEntity otherEntity = value.ContactPointEvent.GetOtherEntity(this.Entity);
                    if (Sync.IsServer)
                    {
                        if (otherEntity is MyCubeGrid)
                        {
                            MyCubeGrid grid = otherEntity as MyCubeGrid;
                            if (grid.BlocksDestructionEnabled)
                            {
                                this.DestroyGrid(ref value, grid);
                            }
                        }
                        else if (otherEntity is MyCharacter)
                        {
                            (otherEntity as MyCharacter).DoDamage(50f * this.Entity.PositionComp.Scale.Value, MyDamageType.Environment, true, this.Entity.EntityId);
                        }
                        else if (otherEntity is MyFloatingObject)
                        {
                            (otherEntity as MyFloatingObject).DoDamage(100f * this.Entity.PositionComp.Scale.Value, MyDamageType.Deformation, true, this.Entity.EntityId);
                        }
                        else if (otherEntity is MyMeteor)
                        {
                            this.m_closeAfterSimulation = true;
                            (otherEntity.GameLogic as MyMeteor.MyMeteorGameLogic).m_closeAfterSimulation = true;
                        }
                        this.m_closeAfterSimulation = true;
                    }
                    if (otherEntity is MyVoxelBase)
                    {
                        this.CreateCrater(value, otherEntity as MyVoxelBase);
                    }
                }
            }

            private void StartLoopSound()
            {
                bool? nullable = null;
                this.m_soundEmitter.PlaySingleSound(this.m_meteorFly, false, false, false, nullable);
            }

            private void StopLoopSound()
            {
                this.m_soundEmitter.StopSound(true, true);
            }

            public override void UpdateAfterSimulation()
            {
                if (this.m_closeAfterSimulation)
                {
                    this.CloseMeteorInternal();
                    this.m_closeAfterSimulation = false;
                }
                base.UpdateAfterSimulation();
            }

            public override void UpdateBeforeSimulation()
            {
                base.UpdateBeforeSimulation();
                if (this.m_dustEffect != null)
                {
                    this.UpdateParticlePosition();
                }
            }

            public override void UpdateBeforeSimulation100()
            {
                base.UpdateBeforeSimulation100();
                if (this.m_particleVectorUp == Vector3.Zero)
                {
                    this.m_particleVectorUp = !(this.Entity.Physics.LinearVelocity != Vector3.Zero) ? Vector3.Up : -Vector3.Normalize(this.Entity.Physics.LinearVelocity);
                    this.m_particleVectorUp.CalculatePerpendicularVector(out this.m_particleVectorForward);
                }
                Vector3D position = this.Entity.PositionComp.GetPosition();
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                if (((closestPlanet == null) || !closestPlanet.HasAtmosphere) || (closestPlanet.GetAirDensity(position) <= 0.5f))
                {
                    this.m_meteorStatus = MeteorStatus.InSpace;
                }
                else
                {
                    this.m_meteorStatus = MeteorStatus.InAtmosphere;
                }
                if ((this.m_meteorStatus != this.m_meteorStatus) && (this.m_dustEffect != null))
                {
                    this.m_dustEffect.Stop(true);
                    this.m_dustEffect = null;
                }
                if ((this.m_dustEffect != null) && !this.InParticleVisibleRange)
                {
                    this.m_dustEffect.Stop(true);
                    this.m_dustEffect = null;
                }
                if (((this.m_dustEffect == null) && this.InParticleVisibleRange) && MyParticlesManager.TryCreateParticleEffect(this.m_particleEffectNames[(int) this.m_meteorStatus], MatrixD.CreateWorld(this.Entity.WorldMatrix.Translation, this.m_particleVectorForward, this.m_particleVectorUp), out this.m_dustEffect))
                {
                    this.UpdateParticlePosition();
                    this.m_dustEffect.UserScale = this.Entity.PositionComp.Scale.Value;
                }
                this.m_soundEmitter.Update();
                if (Sync.IsServer && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_timeCreated) > (Math.Min((float) (MyMeteor.MAX_TRAJECTORY_LENGTH / MyMeteor.SPEED), ((float) MyMeteor.MAX_TRAJECTORY_LENGTH) / this.Entity.Physics.LinearVelocity.Length()) * 1000f)))
                {
                    this.CloseMeteorInternal();
                }
            }

            private void UpdateParticlePosition()
            {
                if (!(this.m_particleVectorUp != Vector3.Zero))
                {
                    this.m_dustEffect.Enabled = false;
                }
                else
                {
                    this.m_dustEffect.Enabled = true;
                    this.m_dustEffect.WorldMatrix = this.GetParticlePosition();
                }
            }

            internal MyMeteor Entity =>
                ((base.Container != null) ? (base.Container.Entity as MyMeteor) : null);

            public MyVoxelMaterialDefinition VoxelMaterial { get; set; }

            private bool InParticleVisibleRange
            {
                get
                {
                    MyMeteor entity = this.Entity;
                    if (entity != null)
                    {
                        return ((MySector.MainCamera.Position - entity.WorldMatrix.Translation).LengthSquared() < 9000000.0);
                    }
                    string msg = "Error: MyMeteor.Container should not be null!";
                    MyLog.Default.WriteLine(msg);
                    return false;
                }
            }

            public float Integrity =>
                this.m_integrity;

            private enum MeteorStatus
            {
                InAtmosphere,
                InSpace
            }
        }
    }
}


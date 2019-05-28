namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public sealed class MyDrillBase
    {
        public MyInventory OutputInventory;
        public float VoxelHarvestRatio = 0.009f;
        private VRage.Game.Entity.MyEntity m_drillEntity;
        private MyFixedPoint m_inventoryCollectionRatio;
        private MyDrillSensorBase m_sensor;
        public MyStringHash m_drillMaterial = MyStringHash.GetOrCompute("HandDrill");
        public MySoundPair m_idleSoundLoop = new MySoundPair("ToolPlayDrillIdle", true);
        private MyStringHash m_metalMaterial = MyStringHash.GetOrCompute("Metal");
        private MyStringHash m_rockMaterial = MyStringHash.GetOrCompute("Rock");
        private int m_lastContactTime;
        private int m_lastItemId;
        private string m_currentDustEffectName = "";
        public MyParticleEffect DustParticles;
        private MySlimBlock m_target;
        private string m_dustEffectName;
        private string m_dustEffectStonesName;
        private string m_sparksEffectName;
        private bool m_particleEffectsEnabled = true;
        private float m_animationMaxSpeedRatio;
        private float m_animationLastUpdateTime;
        private readonly float m_animationSlowdownTimeInSeconds;
        private float m_floatingObjectSpawnOffset;
        private float m_floatingObjectSpawnRadius;
        private bool m_previousDust;
        private bool m_previousSparks;
        private VRage.Game.Entity.MyEntity m_drilledEntity;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private bool m_initialHeatup = true;
        private MyDrillCutOut m_cutOut;
        private readonly float m_drillCameraMeanShakeIntensity = 0.85f;
        public static float DRILL_MAX_SHAKE = 2f;
        private bool force2DSound;
        private Action<float, string, string> m_onOreCollected;
        private string m_drilledVoxelMaterial;
        private bool m_drilledVoxelMaterialValid;
        public MyParticleEffect SparkEffect;
        private readonly List<MyPhysics.HitInfo> m_castList = new List<MyPhysics.HitInfo>();

        public MyDrillBase(VRage.Game.Entity.MyEntity drillEntity, string dustEffectName, string dustEffectStonesName, string sparksEffectName, MyDrillSensorBase sensor, MyDrillCutOut cutOut, float animationSlowdownTimeInSeconds, float floatingObjectSpawnOffset, float floatingObjectSpawnRadius, float inventoryCollectionRatio = 0f, Action<float, string, string> onOreCollected = null)
        {
            this.m_drillEntity = drillEntity;
            this.m_sensor = sensor;
            this.m_cutOut = cutOut;
            this.m_dustEffectName = dustEffectName;
            this.m_dustEffectStonesName = dustEffectStonesName;
            this.m_sparksEffectName = sparksEffectName;
            this.m_animationSlowdownTimeInSeconds = animationSlowdownTimeInSeconds;
            this.m_floatingObjectSpawnOffset = floatingObjectSpawnOffset;
            this.m_floatingObjectSpawnRadius = floatingObjectSpawnRadius;
            this.m_inventoryCollectionRatio = (MyFixedPoint) inventoryCollectionRatio;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this.m_drillEntity, true, 1f);
            this.m_onOreCollected = onOreCollected;
        }

        private void CheckParticles(bool newDust, bool newSparks)
        {
            if (this.m_previousDust != newDust)
            {
                if (this.m_previousDust)
                {
                    this.StopDustParticles();
                }
                this.m_previousDust = newDust;
            }
            if (this.m_previousSparks != newSparks)
            {
                if (this.m_previousSparks)
                {
                    this.StopSparkParticles();
                }
                this.m_previousSparks = newSparks;
            }
        }

        public void Close()
        {
            this.IsDrilling = false;
            this.StopDustParticles();
            this.StopSparkParticles();
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
        }

        private Vector3 ComputeDebrisDirection()
        {
            Vector3D vectord = this.m_sensor.Center - this.m_sensor.FrontPoint;
            vectord.Normalize();
            return (Vector3) vectord;
        }

        private void CreateParticles(Vector3D position, bool createDust, bool createSparks, bool createStones, MatrixD parent, uint parentId, MyStringHash materialName)
        {
            if (this.m_particleEffectsEnabled && !Sync.IsDedicated)
            {
                if (createDust)
                {
                    string str = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, MyStringHash.GetOrCompute("ShipDrill"), materialName);
                    if ((!this.m_previousDust || (this.DustParticles == null)) || !this.m_currentDustEffectName.Equals(str))
                    {
                        this.CurrentDustEffectName = !string.IsNullOrEmpty(str) ? str : (createStones ? this.m_dustEffectStonesName : this.m_dustEffectName);
                        if (((this.DustParticles == null) || (this.DustParticles.GetName() != this.m_currentDustEffectName)) && (this.DustParticles != null))
                        {
                            this.DustParticles.Stop(false);
                            this.DustParticles = null;
                        }
                        if (this.DustParticles == null)
                        {
                            MyParticlesManager.TryCreateParticleEffect(this.m_currentDustEffectName, ref parent, ref position, parentId, out this.DustParticles);
                        }
                    }
                }
                if (createSparks && (!this.m_previousSparks || (this.SparkEffect == null)))
                {
                    if (this.SparkEffect != null)
                    {
                        this.SparkEffect.Stop(false);
                    }
                    MyParticlesManager.TryCreateParticleEffect(this.m_sparksEffectName, ref parent, ref position, parentId, out this.SparkEffect);
                }
            }
        }

        public void DebugDraw()
        {
            this.m_sensor.DebugDraw();
            MyRenderProxy.DebugDrawSphere(this.m_cutOut.Sphere.Center, (float) this.m_cutOut.Sphere.Radius, Color.Red, 0.6f, true, false, true, false);
        }

        public bool Drill(bool collectOre = true, bool performCutout = true, bool assignDamagedMaterial = false, float speedMultiplier = 1f)
        {
            bool flag = false;
            bool newDust = false;
            bool newSparks = false;
            MySoundPair objA = null;
            if (((this.m_drillEntity.Parent != null) && (this.m_drillEntity.Parent.Physics != null)) && !this.m_drillEntity.Parent.Physics.Enabled)
            {
                return false;
            }
            this.DrilledEntity = null;
            this.CollectingOre = false;
            MyStringHash nullOrEmpty = MyStringHash.NullOrEmpty;
            MyStringHash hash2 = MyStringHash.NullOrEmpty;
            float maxValue = float.MaxValue;
            bool flag4 = false;
            foreach (KeyValuePair<long, MyDrillSensorBase.DetectionInfo> pair2 in this.m_sensor.CachedEntitiesInRange)
            {
                flag = false;
                VRage.Game.Entity.MyEntity entity = pair2.Value.Entity;
                if (!entity.MarkedForClose)
                {
                    if (entity is MyCubeGrid)
                    {
                        if (this.DrillGrid(pair2.Value, performCutout, ref nullOrEmpty))
                        {
                            flag = flag4 = newSparks = true;
                        }
                    }
                    else if (entity is MyVoxelBase)
                    {
                        if (this.DrillVoxel(pair2.Value, collectOre, performCutout, assignDamagedMaterial, ref nullOrEmpty))
                        {
                            flag = newDust = true;
                        }
                    }
                    else if (entity is MyFloatingObject)
                    {
                        flag = this.DrillFloatingObject(pair2.Value);
                    }
                    else if (entity is MyCharacter)
                    {
                        flag = this.DrillCharacter(pair2.Value, out nullOrEmpty);
                    }
                    else if (entity is MyEnvironmentSector)
                    {
                        flag = this.DrillEnvironmentSector(pair2.Value, speedMultiplier, out nullOrEmpty);
                    }
                    if (flag)
                    {
                        float num2 = Vector3.DistanceSquared((Vector3) pair2.Value.DetectionPoint, (Vector3) this.Sensor.Center);
                        if ((nullOrEmpty != MyStringHash.NullOrEmpty) && (num2 < maxValue))
                        {
                            hash2 = nullOrEmpty;
                            maxValue = num2;
                        }
                    }
                }
            }
            if (hash2 != MyStringHash.NullOrEmpty)
            {
                objA = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Start, this.m_drillMaterial, hash2);
                if ((objA == null) || ReferenceEquals(objA, MySoundPair.Empty))
                {
                    hash2 = !flag4 ? this.m_rockMaterial : this.m_metalMaterial;
                }
                objA = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Start, this.m_drillMaterial, hash2);
            }
            if ((objA == null) || ReferenceEquals(objA, MySoundPair.Empty))
            {
                this.StartIdleSound(this.m_idleSoundLoop, this.Force2DSound);
            }
            else
            {
                this.StartLoopSound(objA, this.Force2DSound);
            }
            if (!this.IsDrilling)
            {
                this.IsDrilling = true;
                this.m_animationLastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
            this.CheckParticles(newDust, newSparks);
            return flag;
        }

        private unsafe bool DrillCharacter(MyDrillSensorBase.DetectionInfo entry, out MyStringHash targetMaterial)
        {
            BoundingSphereD sphere = this.m_cutOut.Sphere;
            double* numPtr1 = (double*) ref sphere.Radius;
            numPtr1[0] *= 0.800000011920929;
            MyCharacter entity = entry.Entity as MyCharacter;
            targetMaterial = MyStringHash.GetOrCompute(entity.Definition.PhysicalMaterial);
            if (entity.GetIntersectionWithSphere(ref sphere))
            {
                this.DrilledEntity = entity;
                this.DrilledEntityPoint = entry.DetectionPoint;
                if (((this.m_drillEntity is MyHandDrill) && (ReferenceEquals((this.m_drillEntity as MyHandDrill).Owner, MySession.Static.LocalCharacter) && !ReferenceEquals(entity, MySession.Static.LocalCharacter))) && !entity.IsDead)
                {
                    MySession @static = MySession.Static;
                    @static.TotalDamageDealt += (uint) 20;
                }
                if (Sync.IsServer)
                {
                    entity.DoDamage(20f, MyDamageType.Drill, true, (this.m_drillEntity != null) ? this.m_drillEntity.EntityId : 0L);
                }
                this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                return true;
            }
            BoundingSphereD ed2 = new BoundingSphereD(entity.PositionComp.WorldMatrix.Translation + (entity.WorldMatrix.Up * 1.25), 0.60000002384185791);
            if (!ed2.Intersects(sphere))
            {
                return false;
            }
            this.DrilledEntity = entity;
            this.DrilledEntityPoint = entry.DetectionPoint;
            if (((this.m_drillEntity is MyHandDrill) && (ReferenceEquals((this.m_drillEntity as MyHandDrill).Owner, MySession.Static.LocalCharacter) && !ReferenceEquals(entity, MySession.Static.LocalCharacter))) && !entity.IsDead)
            {
                MySession @static = MySession.Static;
                @static.TotalDamageDealt += (uint) 20;
            }
            if (Sync.IsServer)
            {
                entity.DoDamage(20f, MyDamageType.Drill, true, (this.m_drillEntity != null) ? this.m_drillEntity.EntityId : 0L);
            }
            this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            return true;
        }

        private bool DrillEnvironmentSector(MyDrillSensorBase.DetectionInfo entry, float speedMultiplier, out MyStringHash targetMaterial)
        {
            targetMaterial = MyStringHash.GetOrCompute("Wood");
            this.DrilledEntity = entry.Entity;
            this.DrilledEntityPoint = entry.DetectionPoint;
            if (Sync.IsServer)
            {
                if (this.m_lastItemId != entry.ItemId)
                {
                    this.m_lastItemId = entry.ItemId;
                    this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
                if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastContactTime) > (1500f * speedMultiplier))
                {
                    Vector3D hitnormal = this.m_drillEntity.WorldMatrix.Forward + this.m_drillEntity.WorldMatrix.Right;
                    hitnormal.Normalize();
                    float num = 100f;
                    (entry.Entity as MyEnvironmentSector).GetModule<MyBreakableEnvironmentProxy>().BreakAt(entry.ItemId, entry.DetectionPoint, hitnormal, (double) ((10f * 10f) * num));
                    this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    this.m_lastItemId = 0;
                }
            }
            return true;
        }

        private unsafe bool DrillFloatingObject(MyDrillSensorBase.DetectionInfo entry)
        {
            MyFloatingObject obj2 = entry.Entity as MyFloatingObject;
            BoundingSphereD sphere = this.m_cutOut.Sphere;
            double* numPtr1 = (double*) ref sphere.Radius;
            numPtr1[0] *= 1.3300000429153442;
            if (!obj2.GetIntersectionWithSphere(ref sphere))
            {
                return false;
            }
            this.DrilledEntity = obj2;
            this.DrilledEntityPoint = entry.DetectionPoint;
            if (Sync.IsServer)
            {
                if (obj2.Item.Content.TypeId != typeof(MyObjectBuilder_Ore))
                {
                    obj2.DoDamage(70f, MyDamageType.Drill, true, (this.m_drillEntity != null) ? this.m_drillEntity.EntityId : 0L);
                }
                else
                {
                    VRage.Game.Entity.MyEntity drillEntity;
                    if ((this.m_drillEntity == null) || !this.m_drillEntity.HasInventory)
                    {
                        drillEntity = null;
                    }
                    else
                    {
                        drillEntity = this.m_drillEntity;
                    }
                    VRage.Game.Entity.MyEntity thisEntity = drillEntity;
                    if (thisEntity == null)
                    {
                        MyHandDrill drillEntity = this.m_drillEntity as MyHandDrill;
                        if (drillEntity != null)
                        {
                            thisEntity = drillEntity.Owner;
                        }
                    }
                    if (thisEntity != null)
                    {
                        thisEntity.GetInventory(0).TakeFloatingObject(obj2);
                    }
                }
            }
            this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            return true;
        }

        private bool DrillGrid(MyDrillSensorBase.DetectionInfo entry, bool performCutout, ref MyStringHash targetMaterial)
        {
            bool flag = false;
            MyCubeGrid entity = entry.Entity as MyCubeGrid;
            if ((entity.Physics != null) && entity.Physics.Enabled)
            {
                flag = this.TryDrillBlocks(entity, entry.DetectionPoint, !Sync.IsServer || !performCutout, out targetMaterial);
            }
            if (flag)
            {
                this.DrilledEntity = entity;
                this.DrilledEntityPoint = entry.DetectionPoint;
                Vector3D position = Vector3D.Transform(this.ParticleOffset, this.m_drillEntity.WorldMatrix);
                MatrixD worldMatrix = this.m_drillEntity.WorldMatrix;
                MyFunctionalBlock drillEntity = this.m_drillEntity as MyFunctionalBlock;
                if (drillEntity != null)
                {
                    worldMatrix.Translation = position;
                    worldMatrix = MatrixD.Multiply(worldMatrix, drillEntity.CubeGrid.PositionComp.WorldMatrixInvScaled);
                }
                this.CreateParticles(position, false, true, false, worldMatrix, this.m_drillEntity.Render.ParentIDs[0], targetMaterial);
            }
            return flag;
        }

        private bool DrillVoxel(MyDrillSensorBase.DetectionInfo entry, bool collectOre, bool performCutout, bool assignDamagedMaterial, ref MyStringHash targetMaterial)
        {
            MyVoxelBase entity = entry.Entity as MyVoxelBase;
            Vector3D detectionPoint = entry.DetectionPoint;
            bool flag = false;
            if (!Sync.IsDedicated)
            {
                MyVoxelMaterialDefinition material = null;
                Vector3D from = this.m_cutOut.Sphere.Center - this.m_drillEntity.WorldMatrix.Forward;
                MyPhysics.CastRay(from, (this.m_cutOut.Sphere.Center - this.m_drillEntity.WorldMatrix.Forward) + (this.m_drillEntity.WorldMatrix.Forward * (this.m_cutOut.Sphere.Radius + 1.0)), this.m_castList, 0x1c);
                bool flag2 = false;
                foreach (MyPhysics.HitInfo info in this.m_castList)
                {
                    if (info.HkHitInfo.GetHitEntity() is MyVoxelBase)
                    {
                        detectionPoint = info.Position;
                        if (entity.GetMaterialAt(ref detectionPoint) == null)
                        {
                            material = MyDefinitionManager.Static.GetVoxelMaterialDefinitions().FirstOrDefault<MyVoxelMaterialDefinition>();
                        }
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2 && this.m_drilledVoxelMaterialValid)
                {
                    material = MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.m_drilledVoxelMaterial);
                }
                if (material != null)
                {
                    this.CollectingOre = collectOre;
                    this.DrilledEntity = entity;
                    this.DrilledEntityPoint = detectionPoint;
                    targetMaterial = material.MaterialTypeNameHash;
                    this.SpawnVoxelParticles(material);
                    flag = true;
                }
            }
            if (Sync.IsServer & performCutout)
            {
                this.TryDrillVoxels(entity, detectionPoint, collectOre, assignDamagedMaterial);
            }
            return flag;
        }

        private void OnDrilledEntityClose(VRage.Game.Entity.MyEntity entity)
        {
            this.DrilledEntity = null;
        }

        private void OnDrillResults(Dictionary<MyVoxelMaterialDefinition, int> materials, Vector3D hitPosition, bool collectOre)
        {
            int num = 0;
            this.m_drilledVoxelMaterial = "";
            this.m_drilledVoxelMaterialValid = true;
            foreach (KeyValuePair<MyVoxelMaterialDefinition, int> pair in materials)
            {
                int removedAmount = pair.Value;
                if (collectOre && !this.TryHarvestOreMaterial(pair.Key, (Vector3) hitPosition, removedAmount, false))
                {
                    removedAmount = 0;
                }
                if (removedAmount > num)
                {
                    num = removedAmount;
                    this.m_drilledVoxelMaterial = (pair.Key.DamagedMaterial != MyStringHash.NullOrEmpty) ? pair.Key.DamagedMaterial.ToString() : pair.Key.Id.SubtypeName;
                }
            }
        }

        public void PerformCameraShake(float multiplier = 1f)
        {
            if (MySector.MainCamera != null)
            {
                float num = MathHelper.Clamp(((float) (-Math.Log(MyRandom.Instance.NextDouble()) * this.m_drillCameraMeanShakeIntensity)) * DRILL_MAX_SHAKE, 0f, DRILL_MAX_SHAKE);
                MySector.MainCamera.CameraShake.AddShake(num * multiplier);
            }
        }

        private void SpawnOrePieces(MyFixedPoint amountItems, MyFixedPoint maxAmountPerDrop, Vector3 hitPosition, MyObjectBuilder_PhysicalObject oreObjBuilder, MyVoxelMaterialDefinition voxelMaterial)
        {
            if (Sync.IsServer)
            {
                Vector3 forward = Vector3.Normalize(this.m_sensor.FrontPoint - this.m_sensor.Center);
                BoundingSphere sphere = new BoundingSphere(hitPosition - (forward * this.m_floatingObjectSpawnRadius), this.m_floatingObjectSpawnRadius);
                while (amountItems > 0)
                {
                    float randomFloat = MyRandom.Instance.GetRandomFloat(((float) maxAmountPerDrop) / 10f, (float) maxAmountPerDrop);
                    MyFixedPoint amount = (MyFixedPoint) MathHelper.Min((float) amountItems, randomFloat);
                    amountItems -= amount;
                    MyPhysicalInventoryItem item = new MyPhysicalInventoryItem(amount, oreObjBuilder, 1f);
                    if (MyFakes.ENABLE_DRILL_ROCKS)
                    {
                        Action<VRage.Game.Entity.MyEntity> <>9__0;
                        Action<VRage.Game.Entity.MyEntity> onDone = <>9__0;
                        if (<>9__0 == null)
                        {
                            Action<VRage.Game.Entity.MyEntity> local1 = <>9__0;
                            onDone = <>9__0 = delegate (VRage.Game.Entity.MyEntity entity) {
                                entity.Physics.LinearVelocity = MyUtils.GetRandomVector3HemisphereNormalized(forward) * MyUtils.GetRandomFloat(1.5f, 4f);
                                entity.Physics.AngularVelocity = MyUtils.GetRandomVector3Normalized() * MyUtils.GetRandomFloat(4f, 8f);
                            };
                        }
                        MyFloatingObjects.Spawn(item, sphere, null, voxelMaterial, onDone);
                    }
                }
            }
        }

        private void SpawnVoxelParticles(MyVoxelMaterialDefinition material)
        {
            Vector3D position = Vector3D.Transform(this.ParticleOffset, this.m_drillEntity.WorldMatrix);
            MatrixD worldMatrix = this.m_drillEntity.WorldMatrix;
            MyFunctionalBlock drillEntity = this.m_drillEntity as MyFunctionalBlock;
            if (drillEntity != null)
            {
                worldMatrix.Translation = position;
                worldMatrix = MatrixD.Multiply(worldMatrix, drillEntity.CubeGrid.PositionComp.WorldMatrixInvScaled);
            }
            this.CreateParticles(position, true, false, true, worldMatrix, this.m_drillEntity.Render.ParentIDs[0], material.MaterialTypeNameHash);
        }

        private void StartIdleSound(MySoundPair cuePair, bool force2D)
        {
            if (this.m_soundEmitter != null)
            {
                bool? nullable;
                if (!this.m_soundEmitter.IsPlaying)
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySound(cuePair, false, false, force2D, false, false, nullable);
                }
                else if (!this.m_soundEmitter.SoundPair.Equals(cuePair))
                {
                    this.m_soundEmitter.StopSound(false, true);
                    nullable = null;
                    this.m_soundEmitter.PlaySound(cuePair, false, true, force2D, false, false, nullable);
                }
            }
        }

        private void StartLoopSound(MySoundPair cueEnum, bool force2D)
        {
            if (this.m_soundEmitter != null)
            {
                bool? nullable;
                if (!this.m_soundEmitter.IsPlaying)
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySound(cueEnum, false, false, force2D, false, false, nullable);
                }
                else if (!this.m_soundEmitter.SoundPair.Equals(cueEnum))
                {
                    if (this.m_soundEmitter.SoundPair.Equals(this.m_idleSoundLoop))
                    {
                        this.m_soundEmitter.StopSound(true, true);
                        nullable = null;
                        this.m_soundEmitter.PlaySound(cueEnum, false, false, force2D, false, false, nullable);
                    }
                    else
                    {
                        this.m_soundEmitter.StopSound(false, true);
                        nullable = null;
                        this.m_soundEmitter.PlaySound(cueEnum, false, true, force2D, false, false, nullable);
                    }
                }
            }
        }

        public void StopDrill()
        {
            this.m_drilledVoxelMaterial = "";
            this.m_drilledVoxelMaterialValid = false;
            this.IsDrilling = false;
            this.m_initialHeatup = true;
            this.StopDustParticles();
            this.StopSparkParticles();
            this.StopLoopSound();
        }

        private void StopDustParticles()
        {
            if (this.DustParticles != null)
            {
                this.DustParticles.Stop(false);
                this.DustParticles = null;
            }
        }

        public void StopLoopSound()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(false, true);
            }
        }

        public void StopSparkParticles()
        {
            if (this.SparkEffect != null)
            {
                this.SparkEffect.Stop(false);
                this.SparkEffect = null;
            }
        }

        private bool TryDrillBlocks(MyCubeGrid grid, Vector3D worldPoint, bool onlyCheck, out MyStringHash blockMaterial)
        {
            MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3D vectord = Vector3D.Transform(this.m_sensor.Center, worldMatrixNormalizedInv);
            Vector3D vectord2 = Vector3D.Transform(this.m_sensor.FrontPoint, worldMatrixNormalizedInv);
            Vector3D vectord3 = Vector3D.Transform(worldPoint, worldMatrixNormalizedInv);
            Vector3I pos = Vector3I.Round(vectord / ((double) grid.GridSize));
            MySlimBlock cubeBlock = grid.GetCubeBlock(pos);
            if (cubeBlock == null)
            {
                Vector3I vectori2 = Vector3I.Round(vectord2 / ((double) grid.GridSize));
                cubeBlock = grid.GetCubeBlock(vectori2);
            }
            blockMaterial = (cubeBlock == null) ? MyStringHash.NullOrEmpty : (!(cubeBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeId == MyStringHash.NullOrEmpty) ? cubeBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeId : this.m_metalMaterial);
            bool flag = false;
            if ((!onlyCheck && ((cubeBlock != null) && (cubeBlock != null))) && cubeBlock.CubeGrid.BlocksDestructionEnabled)
            {
                float damage = MyFakes.DRILL_DAMAGE;
                MyHitInfo? hitInfo = null;
                cubeBlock.DoDamage(damage, MyDamageType.Drill, Sync.IsServer, hitInfo, (this.m_drillEntity != null) ? this.m_drillEntity.EntityId : 0L);
                Vector3 localNormal = Vector3.Normalize(vectord2 - vectord);
                if (cubeBlock.BlockDefinition.BlockTopology == MyBlockTopology.Cube)
                {
                    float deformationOffset = MyFakes.DEFORMATION_DRILL_OFFSET_RATIO * damage;
                    flag = grid.Physics.ApplyDeformation(deformationOffset, MathHelper.Clamp((float) (0.011904f * damage), (float) (grid.GridSize * 0.75f), (float) (grid.GridSize * 1.3f)), MathHelper.Clamp((float) (0.008928f * damage), (float) (grid.GridSize * 0.9f), (float) (grid.GridSize * 1.3f)), (Vector3) vectord3, localNormal, MyDamageType.Drill, 0f, 0f, (this.m_drillEntity != null) ? this.m_drillEntity.EntityId : 0L);
                }
            }
            this.m_target = flag ? null : cubeBlock;
            bool flag2 = false;
            if (cubeBlock != null)
            {
                if (flag)
                {
                    BoundingSphereD sphere = this.m_cutOut.Sphere;
                    BoundingBoxD bb = BoundingBoxD.CreateFromSphere(sphere);
                    MyDebris.Static.CreateExplosionDebris(ref sphere, cubeBlock.CubeGrid, ref bb, 0.3f, true);
                }
                flag2 = true;
            }
            return flag2;
        }

        private void TryDrillVoxels(MyVoxelBase voxels, Vector3D hitPosition, bool collectOre, bool applyDamagedMaterial)
        {
            if (voxels.GetOrePriority() != -1)
            {
                MyShapeSphere sphere1 = new MyShapeSphere();
                sphere1.Center = this.m_cutOut.Sphere.Center;
                sphere1.Radius = (float) this.m_cutOut.Sphere.Radius;
                MyShapeSphere shape = sphere1;
                if (!collectOre)
                {
                    shape.Radius *= 3f;
                }
                MyVoxelBase.OnCutOutResults results = (x, y, z) => this.OnDrillResults(z, hitPosition, collectOre);
                voxels.CutOutShapeWithPropertiesAsync(results, shape, Sync.IsServer, false, applyDamagedMaterial, false, true);
            }
        }

        private bool TryHarvestOreMaterial(MyVoxelMaterialDefinition material, Vector3 hitPosition, int removedAmount, bool onlyCheck)
        {
            if (string.IsNullOrEmpty(material.MinedOre))
            {
                return false;
            }
            if (!onlyCheck)
            {
                MyObjectBuilder_Ore objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                objectBuilder.MaterialTypeName = new MyStringHash?(material.Id.SubtypeId);
                float num = (((((float) removedAmount) / 255f) * 1f) * this.VoxelHarvestRatio) * material.MinedOreRatio;
                if (!MySession.Static.AmountMined.ContainsKey(material.MinedOre))
                {
                    MySession.Static.AmountMined[material.MinedOre] = 0;
                }
                Dictionary<string, MyFixedPoint> amountMined = MySession.Static.AmountMined;
                string minedOre = material.MinedOre;
                amountMined[minedOre] += (MyFixedPoint) num;
                MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(objectBuilder);
                MyFixedPoint amountItems = (MyFixedPoint) (num / physicalItemDefinition.Volume);
                MyFixedPoint maxAmountPerDrop = (MyFixedPoint) (0.15f / physicalItemDefinition.Volume);
                if (this.OutputInventory == null)
                {
                    this.SpawnOrePieces(amountItems, maxAmountPerDrop, hitPosition, objectBuilder, material);
                }
                else
                {
                    MyFixedPoint b = amountItems * (1 - this.m_inventoryCollectionRatio);
                    b = MyFixedPoint.Min((maxAmountPerDrop * 10) - ((MyFixedPoint) 0.001), b);
                    MyFixedPoint amount = (amountItems * this.m_inventoryCollectionRatio) - b;
                    this.OutputInventory.AddItems(amount, objectBuilder);
                    this.SpawnOrePieces(b, maxAmountPerDrop, hitPosition, objectBuilder, material);
                    if (this.m_onOreCollected != null)
                    {
                        this.m_onOreCollected((float) b, objectBuilder.TypeId.ToString(), objectBuilder.SubtypeId.ToString());
                    }
                }
            }
            return true;
        }

        public void UpdateAfterSimulation()
        {
            if (!this.IsDrilling && (this.m_animationMaxSpeedRatio > float.Epsilon))
            {
                float num = (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_animationLastUpdateTime) / 1000f;
                this.m_animationMaxSpeedRatio -= num / this.m_animationSlowdownTimeInSeconds;
                if (this.m_animationMaxSpeedRatio < float.Epsilon)
                {
                    this.m_animationMaxSpeedRatio = 0f;
                }
            }
            if (this.IsDrilling)
            {
                float num2 = (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_animationLastUpdateTime) / 1000f;
                this.m_animationMaxSpeedRatio += (2f * num2) / this.m_animationSlowdownTimeInSeconds;
                if (this.m_animationMaxSpeedRatio > 1f)
                {
                    this.m_animationMaxSpeedRatio = 1f;
                }
                if (this.m_sensor.CachedEntitiesInRange.Count == 0)
                {
                    this.DrilledEntity = null;
                    this.CheckParticles(false, false);
                }
            }
            this.m_animationLastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public void UpdatePosition(MatrixD worldMatrix)
        {
            this.m_sensor.OnWorldPositionChanged(ref worldMatrix);
            this.m_cutOut.UpdatePosition(ref worldMatrix);
        }

        public void UpdateSoundEmitter(Vector3 velocity)
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.SetVelocity(new Vector3?(velocity));
                this.m_soundEmitter.Update();
            }
        }

        public HashSet<VRage.Game.Entity.MyEntity> IgnoredEntities =>
            this.m_sensor.IgnoredEntities;

        public string CurrentDustEffectName
        {
            get => 
                this.m_currentDustEffectName;
            set => 
                (this.m_currentDustEffectName = value);
        }

        public MySoundPair CurrentLoopCueEnum { get; set; }

        public Vector3D ParticleOffset { get; set; }

        public bool IsDrilling { get; private set; }

        public VRage.Game.Entity.MyEntity DrilledEntity
        {
            get => 
                this.m_drilledEntity;
            private set
            {
                if (this.m_drilledEntity != null)
                {
                    this.m_drilledEntity.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.OnDrilledEntityClose);
                }
                this.m_drilledEntity = value;
                if (this.m_drilledEntity != null)
                {
                    this.m_drilledEntity.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.OnDrilledEntityClose);
                }
            }
        }

        public bool CollectingOre { get; protected set; }

        public Vector3D DrilledEntityPoint { get; private set; }

        public float AnimationMaxSpeedRatio =>
            this.m_animationMaxSpeedRatio;

        public MyDrillSensorBase Sensor =>
            this.m_sensor;

        public MyDrillCutOut CutOut =>
            this.m_cutOut;

        public bool Force2DSound
        {
            get => 
                this.force2DSound;
            set
            {
                bool flag = (this.m_soundEmitter != null) && this.m_soundEmitter.IsPlaying;
                MySoundPair soundPair = this.m_soundEmitter.SoundPair;
                this.force2DSound = value;
                if ((((value != this.force2DSound) & flag) && (soundPair != null)) && (this.m_soundEmitter != null))
                {
                    bool? nullable = null;
                    this.m_soundEmitter.PlaySound(soundPair, true, true, this.Force2DSound, false, false, nullable);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Sounds
        {
            public MySoundPair IdleLoop;
            public MySoundPair MetalLoop;
            public MySoundPair RockLoop;
        }
    }
}


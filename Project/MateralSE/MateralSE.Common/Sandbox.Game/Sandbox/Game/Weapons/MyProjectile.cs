namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.EnvironmentItems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    internal class MyProjectile
    {
        private const int CHECK_INTERSECTION_INTERVAL = 5;
        private MyProjectileStateEnum m_state;
        private Vector3D m_origin;
        private Vector3D m_velocity_Projectile;
        private Vector3D m_velocity_Combined;
        private Vector3D m_directionNormalized;
        private float m_speed;
        private float m_maxTrajectory;
        private Vector3D m_position;
        private VRage.Game.Entity.MyEntity[] m_ignoredEntities;
        private VRage.Game.Entity.MyEntity m_weapon;
        private MyCharacterHitInfo m_charHitInfo;
        private MyCubeGrid.MyCubeGridHitInfo m_cubeGridHitInfo;
        public float LengthMultiplier = 1f;
        private MyProjectileAmmoDefinition m_projectileAmmoDefinition;
        private MyStringId m_projectileTrailMaterialId;
        public VRage.Game.Entity.MyEntity OwnerEntity;
        public VRage.Game.Entity.MyEntity OwnerEntityAbsolute;
        private int m_checkIntersectionIndex;
        private static int checkIntersectionCounter = 0;
        private bool m_positionChecked;
        private static List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>> m_entityRaycastResult = null;
        private static List<MyPhysics.HitInfo> m_raycastResult = new List<MyPhysics.HitInfo>(0x10);
        private const float m_impulseMultiplier = 0.5f;
        private static MyStringHash m_hashBolt = MyStringHash.GetOrCompute("Bolt");
        private static MyStringId ID_PROJECTILE_TRAIL_LINE = MyStringId.GetOrCompute("ProjectileTrailLine");
        public static readonly MyTimedItemCache CollisionSoundsTimedCache = new MyTimedItemCache(60);
        public static readonly MyTimedItemCache CollisionParticlesTimedCache = new MyTimedItemCache(200);
        public static double CollisionSoundSpaceMapping = 0.039999999105930328;
        public static double CollisionParticlesSpaceMapping = 0.800000011920929;
        private static readonly MyTimedItemCache m_prefetchedVoxelRaysTimedCache = new MyTimedItemCache(0xfa0);
        private const double m_prefetchedVoxelRaysSourceMapping = 0.5;
        private const double m_prefetchedVoxelRaysDirectionMapping = 50.0;
        public static bool DEBUG_DRAW_PROJECTILE_TRAJECTORY = false;

        private void ApllyDeformationCubeGrid(Vector3D hitPosition, MyCubeGrid grid)
        {
            MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3D vectord2 = Vector3D.TransformNormal(this.m_directionNormalized, worldMatrixNormalizedInv);
            float deformationOffset = MyFakes.DEFORMATION_PROJECTILE_OFFSET_RATIO * this.m_projectileAmmoDefinition.ProjectileMassDamage;
            grid.Physics.ApplyDeformation(deformationOffset, MathHelper.Clamp((float) (0.011904f * this.m_projectileAmmoDefinition.ProjectileMassDamage), (float) (grid.GridSize * 0.75f), (float) (grid.GridSize * 1.3f)), MathHelper.Clamp((float) (0.008928f * this.m_projectileAmmoDefinition.ProjectileMassDamage), (float) (grid.GridSize * 0.9f), (float) (grid.GridSize * 1.3f)), (Vector3) Vector3D.Transform(hitPosition, worldMatrixNormalizedInv), (Vector3) vectord2, MyDamageType.Bullet, 0f, 0f, 0L);
        }

        public static void ApplyProjectileForce(IMyEntity entity, Vector3D intersectionPosition, Vector3 normalizedDirection, bool isPlayerShip, float impulse)
        {
            if (((entity.Physics != null) && entity.Physics.Enabled) && !entity.Physics.IsStatic)
            {
                if (entity is MyCharacter)
                {
                    impulse *= 100f;
                }
                float? maxSpeed = null;
                entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(normalizedDirection * impulse), new Vector3D?(intersectionPosition), new Vector3?(Vector3.Zero), maxSpeed, true, false);
            }
        }

        public void Close()
        {
            this.OwnerEntity = null;
            this.m_ignoredEntities = null;
            this.m_weapon = null;
        }

        private static void CreateBasicHitParticles(string effectName, ref Vector3D hitPoint, ref Vector3 normal, ref Vector3D direction, IMyEntity physObject, VRage.Game.Entity.MyEntity weapon, float scale, VRage.Game.Entity.MyEntity ownerEntity = null)
        {
            MyParticleEffect effect;
            MyUtilRandomVector3ByDeviatingVector vector1 = new MyUtilRandomVector3ByDeviatingVector((Vector3) Vector3D.Reflect(direction, normal));
            MatrixD xd = MatrixD.CreateFromDir(normal);
            if (MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.CreateWorld(hitPoint, xd.Forward, xd.Up), out effect))
            {
                effect.UserScale = scale;
            }
        }

        private void CreateDecal(MyStringHash materialType)
        {
        }

        private void DoDamage(float damage, MyHitInfo hitInfo, object customdata, IMyEntity damagedEntity)
        {
            VRage.Game.Entity.MyEntity controlledEntity = (VRage.Game.Entity.MyEntity) MySession.Static.ControlledEntity;
            if (((this.OwnerEntityAbsolute != null) && this.OwnerEntityAbsolute.Equals(MySession.Static.ControlledEntity)) && ((damagedEntity is IMyDestroyableObject) || (damagedEntity is MyCubeGrid)))
            {
                MySession @static = MySession.Static;
                @static.TotalDamageDealt += (uint) damage;
            }
            if (!Sync.IsServer)
            {
                MyCharacter character = damagedEntity as MyCharacter;
                if (character != null)
                {
                    character.DoDamage(damage, MyDamageType.Bullet, false, (this.m_weapon != null) ? this.m_weapon.EntityId : 0L);
                }
            }
            else if (this.m_projectileAmmoDefinition.PhysicalMaterial == m_hashBolt)
            {
                IMyDestroyableObject obj2 = damagedEntity as IMyDestroyableObject;
                if ((obj2 != null) && (damagedEntity is MyCharacter))
                {
                    obj2.DoDamage(damage, MyDamageType.Bolt, true, new MyHitInfo?(hitInfo), (this.m_weapon != null) ? this.GetSubpartOwner(this.m_weapon).EntityId : 0L);
                }
            }
            else
            {
                MyCubeGrid cubeGrid = damagedEntity as MyCubeGrid;
                MyCubeBlock fatBlock = damagedEntity as MyCubeBlock;
                MySlimBlock slimBlock = null;
                if (fatBlock != null)
                {
                    cubeGrid = fatBlock.CubeGrid;
                    slimBlock = fatBlock.SlimBlock;
                }
                else if (cubeGrid != null)
                {
                    slimBlock = cubeGrid.GetTargetedBlock(hitInfo.Position - (0.001f * hitInfo.Normal));
                    if (slimBlock != null)
                    {
                        fatBlock = slimBlock.FatBlock;
                    }
                }
                if (cubeGrid != null)
                {
                    if (((cubeGrid.Physics != null) && cubeGrid.Physics.Enabled) && (cubeGrid.BlocksDestructionEnabled || MyFakes.ENABLE_VR_FORCE_BLOCK_DESTRUCTIBLE))
                    {
                        bool flag = false;
                        if ((slimBlock != null) && (cubeGrid.BlocksDestructionEnabled || slimBlock.ForceBlockDestructible))
                        {
                            slimBlock.DoDamage(damage, MyDamageType.Bullet, true, new MyHitInfo?(hitInfo), (this.m_weapon != null) ? this.GetSubpartOwner(this.m_weapon).EntityId : 0L);
                            if (fatBlock == null)
                            {
                                flag = true;
                            }
                        }
                        if (cubeGrid.BlocksDestructionEnabled & flag)
                        {
                            this.ApllyDeformationCubeGrid(hitInfo.Position, cubeGrid);
                        }
                    }
                }
                else if (!(damagedEntity is MyEntitySubpart))
                {
                    IMyDestroyableObject obj3 = damagedEntity as IMyDestroyableObject;
                    if (obj3 != null)
                    {
                        obj3.DoDamage(damage, MyDamageType.Bullet, true, new MyHitInfo?(hitInfo), (this.m_weapon != null) ? this.GetSubpartOwner(this.m_weapon).EntityId : 0L);
                    }
                }
                else if ((damagedEntity.Parent != null) && (damagedEntity.Parent.Parent is MyCubeGrid))
                {
                    hitInfo.Position = damagedEntity.Parent.WorldAABB.Center;
                    this.DoDamage(damage, hitInfo, customdata, damagedEntity.Parent.Parent);
                }
            }
        }

        public void Draw()
        {
            if (this.m_state != MyProjectileStateEnum.KILLED)
            {
                double num = Vector3D.Distance(this.m_position, this.m_origin);
                if ((num > 0.0) && this.m_positionChecked)
                {
                    Vector3D vectord = this.m_position - ((this.m_directionNormalized * 120.0) * 0.01666666753590107);
                    Vector3D vectord2 = Vector3D.Normalize(this.m_position - vectord);
                    double num2 = (this.LengthMultiplier * this.m_projectileAmmoDefinition.ProjectileTrailScale) * (MyParticlesManager.Paused ? ((double) 0.6f) : ((double) MyUtils.GetRandomFloat(0.6f, 0.8f)));
                    if (num < num2)
                    {
                        num2 = num;
                    }
                    if ((this.m_state == MyProjectileStateEnum.ACTIVE) || ((num * num) >= ((this.m_velocity_Combined.LengthSquared() * 0.01666666753590107) * 5.0)))
                    {
                        vectord = this.m_position - (num2 * vectord2);
                    }
                    else
                    {
                        vectord = this.m_position - ((((num - num2) * MyUtils.GetRandomFloat(0f, 1f)) + num2) * vectord2);
                    }
                    if (Vector3D.DistanceSquared(vectord, this.m_origin) >= 4.0)
                    {
                        float num3 = MyParticlesManager.Paused ? 1f : MyUtils.GetRandomFloat(1f, 2f);
                        float thickness = ((MyParticlesManager.Paused ? 0.2f : MyUtils.GetRandomFloat(0.2f, 0.3f)) * this.m_projectileAmmoDefinition.ProjectileTrailScale) * MathHelper.Lerp(0.2f, 0.8f, MySector.MainCamera.Zoom.GetZoomLevel());
                        float num5 = 1f;
                        float num6 = 10f;
                        if (num2 > 0.0)
                        {
                            if (this.m_projectileAmmoDefinition.ProjectileTrailMaterial != null)
                            {
                                MyTransparentGeometry.AddLineBillboard(this.m_projectileTrailMaterialId, new Vector4(this.m_projectileAmmoDefinition.ProjectileTrailColor * num6, 1f), vectord, (Vector3) vectord2, (float) num2, thickness, MyBillboard.BlendTypeEnum.Standard, -1, 1f, null);
                            }
                            else
                            {
                                MyTransparentGeometry.AddLineBillboard(ID_PROJECTILE_TRAIL_LINE, new Vector4((this.m_projectileAmmoDefinition.ProjectileTrailColor * num3) * num6, 1f) * num5, vectord, (Vector3) vectord2, (float) num2, thickness, MyBillboard.BlendTypeEnum.Standard, -1, 1f, null);
                            }
                        }
                    }
                }
            }
        }

        private void GetHitEntityAndPosition(LineD line, out IMyEntity entity, out MyHitInfo hitInfoRet, out object customdata)
        {
            entity = null;
            hitInfoRet = new MyHitInfo();
            customdata = null;
            using (MyUtils.ReuseCollection<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>(ref m_entityRaycastResult))
            {
                MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref line, m_entityRaycastResult, MyEntityQueryType.Both);
                using (List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>.Enumerator enumerator = m_entityRaycastResult.GetEnumerator())
                {
                    while (true)
                    {
                        Vector3D? nullable;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MySafeZone element = enumerator.Current.Element as MySafeZone;
                        if ((element != null) && (element.Enabled && (!element.AllowedActions.HasFlag(MySafeZoneAction.Shooting) && (element.GetIntersectionWithLine(ref line, out nullable, true, IntersectionFlags.ALL_TRIANGLES) && (nullable != null)))))
                        {
                            hitInfoRet.Position = nullable.Value;
                            hitInfoRet.Normal = (Vector3) -line.Direction;
                            entity = element;
                            return;
                        }
                    }
                }
            }
            int num = 0;
            using (MyUtils.ReuseCollection<MyPhysics.HitInfo>(ref m_raycastResult))
            {
                MyPhysics.CastRay(line.From, line.To, m_raycastResult, 15);
                do
                {
                    if (num < m_raycastResult.Count)
                    {
                        MyPhysics.HitInfo info = m_raycastResult[num];
                        entity = info.HkHitInfo.GetHitEntity() as VRage.Game.Entity.MyEntity;
                        hitInfoRet.Position = info.Position;
                        hitInfoRet.Normal = info.HkHitInfo.Normal;
                        hitInfoRet.ShapeKey = info.HkHitInfo.GetShapeKey(0);
                    }
                    if (this.IsIgnoredEntity(entity))
                    {
                        entity = null;
                    }
                    if ((entity is MyCharacter) && !Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        if (!(entity as MyCharacter).GetIntersectionWithLine(ref line, ref this.m_charHitInfo, IntersectionFlags.ALL_TRIANGLES))
                        {
                            entity = null;
                        }
                        else
                        {
                            hitInfoRet.Position = this.m_charHitInfo.Triangle.IntersectionPointInWorldSpace;
                            hitInfoRet.Normal = this.m_charHitInfo.Triangle.NormalInWorldSpace;
                            customdata = this.m_charHitInfo;
                        }
                    }
                    else
                    {
                        MyIntersectionResultLineTriangleEx? nullable2;
                        MyCubeGrid grid = entity as MyCubeGrid;
                        if (grid != null)
                        {
                            if (grid.GetIntersectionWithLine(ref line, ref this.m_cubeGridHitInfo, IntersectionFlags.ALL_TRIANGLES))
                            {
                                hitInfoRet.Position = this.m_cubeGridHitInfo.Triangle.IntersectionPointInWorldSpace;
                                hitInfoRet.Normal = this.m_cubeGridHitInfo.Triangle.NormalInWorldSpace;
                                if (Vector3.Dot(hitInfoRet.Normal, (Vector3) line.Direction) > 0f)
                                {
                                    hitInfoRet.Normal = -hitInfoRet.Normal;
                                }
                            }
                            MyHitInfo info2 = new MyHitInfo {
                                Position = hitInfoRet.Position,
                                Normal = hitInfoRet.Normal
                            };
                            if (this.m_cubeGridHitInfo != null)
                            {
                                MyCube userObject = this.m_cubeGridHitInfo.Triangle.UserObject as MyCube;
                                if (((userObject != null) && (userObject.CubeBlock.FatBlock != null)) && (userObject.CubeBlock.FatBlock.Physics == null))
                                {
                                    entity = userObject.CubeBlock.FatBlock;
                                }
                            }
                        }
                        MyVoxelBase base2 = entity as MyVoxelBase;
                        if ((base2 != null) && base2.GetIntersectionWithLine(ref line, out nullable2, IntersectionFlags.DIRECT_TRIANGLES))
                        {
                            hitInfoRet.Position = nullable2.Value.IntersectionPointInWorldSpace;
                            hitInfoRet.Normal = nullable2.Value.NormalInWorldSpace;
                            hitInfoRet.ShapeKey = 0;
                        }
                    }
                }
                while ((entity == null) && (++num < m_raycastResult.Count));
            }
        }

        private VRage.Game.Entity.MyEntity GetSubpartOwner(VRage.Game.Entity.MyEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (!(entity is MyEntitySubpart))
            {
                return entity;
            }
            VRage.Game.Entity.MyEntity parent = entity;
            while ((parent is MyEntitySubpart) && (parent != null))
            {
                parent = parent.Parent;
            }
            return ((parent != null) ? parent : entity);
        }

        private static void GetSurfaceAndMaterial(IMyEntity entity, ref LineD line, ref Vector3D hitPosition, uint shapeKey, out MySurfaceImpactEnum surfaceImpact, out MyStringHash materialType)
        {
            MyVoxelBase self = entity as MyVoxelBase;
            if (self != null)
            {
                materialType = VRage.Game.MyMaterialType.ROCK;
                surfaceImpact = MySurfaceImpactEnum.DESTRUCTIBLE;
                MyVoxelMaterialDefinition materialAt = self.GetMaterialAt(ref hitPosition);
                if (materialAt != null)
                {
                    materialType = materialAt.MaterialTypeNameHash;
                }
            }
            else if (entity is MyCharacter)
            {
                surfaceImpact = MySurfaceImpactEnum.CHARACTER;
                materialType = VRage.Game.MyMaterialType.CHARACTER;
                if ((entity as MyCharacter).Definition.PhysicalMaterial != null)
                {
                    materialType = MyStringHash.GetOrCompute((entity as MyCharacter).Definition.PhysicalMaterial);
                }
            }
            else if (entity is MyFloatingObject)
            {
                MyStringHash rOCK;
                MyStringHash hash2;
                MyFloatingObject obj2 = entity as MyFloatingObject;
                if (obj2.VoxelMaterial != null)
                {
                    rOCK = VRage.Game.MyMaterialType.ROCK;
                }
                else if ((obj2.ItemDefinition == null) || (obj2.ItemDefinition.PhysicalMaterial == MyStringHash.NullOrEmpty))
                {
                    rOCK = VRage.Game.MyMaterialType.METAL;
                }
                else
                {
                    rOCK = obj2.ItemDefinition.PhysicalMaterial;
                }
                materialType = hash2;
                surfaceImpact = MySurfaceImpactEnum.METAL;
            }
            else if (entity is Sandbox.Game.WorldEnvironment.MyEnvironmentSector)
            {
                surfaceImpact = MySurfaceImpactEnum.METAL;
                materialType = VRage.Game.MyMaterialType.METAL;
                Sandbox.Game.WorldEnvironment.MyEnvironmentSector sector = entity as Sandbox.Game.WorldEnvironment.MyEnvironmentSector;
                int itemFromShapeKey = sector.GetItemFromShapeKey(shapeKey);
                if (((itemFromShapeKey >= 0) && ((sector.DataView != null) && (sector.DataView.Items != null))) && (sector.DataView.Items.Count > itemFromShapeKey))
                {
                    Sandbox.Game.WorldEnvironment.ItemInfo info = sector.DataView.Items[itemFromShapeKey];
                    MyRuntimeEnvironmentItemInfo info2 = null;
                    if (sector.EnvironmentDefinition.Items.TryGetValue<MyRuntimeEnvironmentItemInfo>(info.DefinitionIndex, out info2) && info2.Type.Name.Equals("Tree"))
                    {
                        surfaceImpact = MySurfaceImpactEnum.DESTRUCTIBLE;
                        materialType = VRage.Game.MyMaterialType.WOOD;
                    }
                }
            }
            else if (entity is MyTrees)
            {
                surfaceImpact = MySurfaceImpactEnum.DESTRUCTIBLE;
                materialType = VRage.Game.MyMaterialType.WOOD;
            }
            else if (entity is IMyHandheldGunObject<MyGunBase>)
            {
                surfaceImpact = MySurfaceImpactEnum.METAL;
                materialType = VRage.Game.MyMaterialType.METAL;
                MyGunBase gunBase = (entity as IMyHandheldGunObject<MyGunBase>).GunBase;
                if (((gunBase != null) && (gunBase.WeaponProperties != null)) && (gunBase.WeaponProperties.WeaponDefinition != null))
                {
                    materialType = gunBase.WeaponProperties.WeaponDefinition.PhysicalMaterial;
                }
            }
            else
            {
                surfaceImpact = MySurfaceImpactEnum.METAL;
                materialType = VRage.Game.MyMaterialType.METAL;
                MyCubeGrid cubeGrid = entity as MyCubeGrid;
                MyCubeBlock fatBlock = entity as MyCubeBlock;
                MySlimBlock slimBlock = null;
                if (fatBlock != null)
                {
                    cubeGrid = fatBlock.CubeGrid;
                    slimBlock = fatBlock.SlimBlock;
                }
                else if (cubeGrid != null)
                {
                    slimBlock = cubeGrid.GetTargetedBlock(hitPosition);
                    if (slimBlock != null)
                    {
                        fatBlock = slimBlock.FatBlock;
                    }
                }
                if ((cubeGrid != null) && (slimBlock != null))
                {
                    if ((slimBlock.BlockDefinition.PhysicalMaterial != null) && !slimBlock.BlockDefinition.PhysicalMaterial.Id.TypeId.IsNull)
                    {
                        materialType = MyStringHash.GetOrCompute(slimBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeName);
                    }
                    else if (fatBlock != null)
                    {
                        MyIntersectionResultLineTriangleEx? t = null;
                        fatBlock.GetIntersectionWithLine(ref line, out t, IntersectionFlags.ALL_TRIANGLES);
                        if ((t != null) && (fatBlock.ModelCollision.GetDrawTechnique(t.Value.Triangle.TriangleIndex) == MyMeshDrawTechnique.GLASS))
                        {
                            materialType = MyStringHash.GetOrCompute("Glass");
                        }
                    }
                }
            }
        }

        private bool IsIgnoredEntity(IMyEntity entity)
        {
            if (this.m_ignoredEntities != null)
            {
                foreach (VRage.Game.Entity.MyEntity entity2 in this.m_ignoredEntities)
                {
                    if (ReferenceEquals(entity, entity2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void PlayHitSound(MyStringHash materialType, IMyEntity entity, Vector3D position, MyStringHash thisType)
        {
            bool flag = CollisionSoundsTimedCache.IsPlaceUsed(position, CollisionSoundSpaceMapping, MySandboxGame.TotalSimulationTimeInMilliseconds, true);
            if (!(this.OwnerEntity is MyWarhead) && !flag)
            {
                MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                if (emitter != null)
                {
                    VRage.Game.Entity.MyEntity weapon = this.m_weapon;
                    MySoundPair objA = null;
                    objA = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Hit, thisType, materialType);
                    if ((objA == null) || ReferenceEquals(objA, MySoundPair.Empty))
                    {
                        objA = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Start, thisType, materialType);
                    }
                    if (objA.SoundId.IsNull && (entity is MyVoxelBase))
                    {
                        materialType = VRage.Game.MyMaterialType.ROCK;
                        objA = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Start, thisType, materialType);
                    }
                    if ((objA != null) && !ReferenceEquals(objA, MySoundPair.Empty))
                    {
                        emitter.Entity = (VRage.Game.Entity.MyEntity) entity;
                        emitter.SetPosition(new Vector3D?(this.m_position));
                        emitter.SetVelocity(new Vector3?(Vector3.Zero));
                        if (((MySession.Static != null) && MyFakes.ENABLE_NEW_SOUNDS) && MySession.Static.Settings.RealisticSound)
                        {
                            Func<bool> canHear = () => (MySession.Static.ControlledEntity != null) && ReferenceEquals(MySession.Static.ControlledEntity.Entity, entity);
                            emitter.StoppedPlaying += e => e.EmitterMethods[0].Remove(canHear, true);
                            emitter.EmitterMethods[0].Add(canHear);
                        }
                        bool? nullable = null;
                        emitter.PlaySound(objA, false, false, false, false, false, nullable);
                    }
                }
            }
        }

        private void PrefetchVoxelPhysicsIfNeeded()
        {
            LineD ray = new LineD(this.m_origin, this.m_origin + (this.m_directionNormalized * this.m_maxTrajectory), (double) this.m_maxTrajectory);
            LineD ed2 = new LineD(new Vector3D(Math.Floor(ray.From.X) * 0.5, Math.Floor(ray.From.Y) * 0.5, Math.Floor(ray.From.Z) * 0.5), new Vector3D(Math.Floor((double) (this.m_directionNormalized.X * 50.0)), Math.Floor((double) (this.m_directionNormalized.Y * 50.0)), Math.Floor((double) (this.m_directionNormalized.Z * 50.0))));
            if (!m_prefetchedVoxelRaysTimedCache.IsItemPresent(ed2.GetHash(), MySandboxGame.TotalSimulationTimeInMilliseconds, true))
            {
                using (MyUtils.ReuseCollection<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>(ref m_entityRaycastResult))
                {
                    MyGamePruningStructure.GetAllEntitiesInRay(ref ray, m_entityRaycastResult, MyEntityQueryType.Static);
                    using (List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>.Enumerator enumerator = m_entityRaycastResult.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyVoxelPhysics element = enumerator.Current.Element as MyVoxelPhysics;
                            if (element != null)
                            {
                                element.PrefetchShapeOnRay(ref ray);
                            }
                        }
                    }
                }
            }
        }

        public void Start(MyProjectileAmmoDefinition ammoDefinition, MyWeaponDefinition weaponDefinition, VRage.Game.Entity.MyEntity[] ignoreEntities, Vector3D origin, Vector3 initialVelocity, Vector3 directionNormalized, VRage.Game.Entity.MyEntity weapon)
        {
            this.m_projectileAmmoDefinition = ammoDefinition;
            this.m_state = MyProjectileStateEnum.ACTIVE;
            this.m_ignoredEntities = ignoreEntities;
            this.m_origin = origin + (0.1 * directionNormalized);
            this.m_position = this.m_origin;
            this.m_weapon = weapon;
            if (ammoDefinition.ProjectileTrailMaterial != null)
            {
                this.m_projectileTrailMaterialId = MyStringId.GetOrCompute(ammoDefinition.ProjectileTrailMaterial);
            }
            this.LengthMultiplier = (ammoDefinition.ProjectileTrailProbability < MyUtils.GetRandomFloat(0f, 1f)) ? 0f : 40f;
            this.m_directionNormalized = directionNormalized;
            this.m_speed = ammoDefinition.DesiredSpeed * ((ammoDefinition.SpeedVar > 0f) ? MyUtils.GetRandomFloat(1f - ammoDefinition.SpeedVar, 1f + ammoDefinition.SpeedVar) : 1f);
            this.m_velocity_Projectile = this.m_directionNormalized * this.m_speed;
            this.m_velocity_Combined = initialVelocity + this.m_velocity_Projectile;
            this.m_maxTrajectory = ammoDefinition.MaxTrajectory;
            bool useRandomizedRange = true;
            if (weaponDefinition != null)
            {
                this.m_maxTrajectory *= weaponDefinition.RangeMultiplier;
                useRandomizedRange = weaponDefinition.UseRandomizedRange;
            }
            if (useRandomizedRange)
            {
                this.m_maxTrajectory *= MyUtils.GetRandomFloat(0.8f, 1f);
            }
            this.m_checkIntersectionIndex = checkIntersectionCounter % 5;
            checkIntersectionCounter += 3;
            this.m_positionChecked = false;
            this.PrefetchVoxelPhysicsIfNeeded();
        }

        private void StopEffect()
        {
        }

        public unsafe bool Update()
        {
            if (this.m_state != MyProjectileStateEnum.KILLED)
            {
                LineD ed;
                IMyEntity entity;
                MyHitInfo info;
                object obj2;
                MySurfaceImpactEnum enum2;
                MyStringHash hash;
                Vector3D position = this.m_position;
                this.m_position += (this.m_velocity_Combined * 0.01666666753590107) * MyFakes.SIMULATION_SPEED;
                if (DEBUG_DRAW_PROJECTILE_TRAJECTORY)
                {
                    MyRenderProxy.DebugDrawLine3D(position, this.m_position, Color.AliceBlue, Color.AliceBlue, true, false);
                }
                Vector3 vector1 = (Vector3) (this.m_position - this.m_origin);
                if (Vector3.Dot(vector1, vector1) >= (this.m_maxTrajectory * this.m_maxTrajectory))
                {
                    this.StopEffect();
                    this.m_state = MyProjectileStateEnum.KILLED;
                    return false;
                }
                int num3 = this.m_checkIntersectionIndex + 1;
                this.m_checkIntersectionIndex = num3;
                this.m_checkIntersectionIndex = num3 % 5;
                if ((this.m_checkIntersectionIndex != 0) && this.m_positionChecked)
                {
                    return true;
                }
                Vector3D to = position + (5.0 * ((this.m_velocity_Projectile * 0.01666666753590107) * MyFakes.SIMULATION_SPEED));
                LineD* edPtr1 = (LineD*) new LineD(this.m_positionChecked ? position : this.m_origin, to);
                this.m_positionChecked = true;
                edPtr1 = (LineD*) ref ed;
                this.GetHitEntityAndPosition(ed, out entity, out info, out obj2);
                if (entity == null)
                {
                    return true;
                }
                if (this.IsIgnoredEntity(entity))
                {
                    return true;
                }
                bool flag = false;
                MyCharacter character = entity as MyCharacter;
                if (character != null)
                {
                    IStoppableAttackingTool currentWeapon = character.CurrentWeapon as IStoppableAttackingTool;
                    if (currentWeapon != null)
                    {
                        currentWeapon.StopShooting(this.OwnerEntity);
                    }
                    if (obj2 != null)
                    {
                        flag = (obj2 as MyCharacterHitInfo).HitHead && this.m_projectileAmmoDefinition.HeadShot;
                    }
                }
                this.m_position = info.Position;
                this.m_position += ed.Direction * 0.01;
                float damageMultiplier = 1f;
                if (this.m_weapon is IMyHandheldGunObject<MyGunBase>)
                {
                    MyGunBase gunBase = (this.m_weapon as IMyHandheldGunObject<MyGunBase>).GunBase;
                    if (((gunBase != null) && (gunBase.WeaponProperties != null)) && (gunBase.WeaponProperties.WeaponDefinition != null))
                    {
                        damageMultiplier = gunBase.WeaponProperties.WeaponDefinition.DamageMultiplier;
                    }
                }
                GetSurfaceAndMaterial(entity, ref ed, ref this.m_position, info.ShapeKey, out enum2, out hash);
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.PlayHitSound(hash, entity, info.Position, this.m_projectileAmmoDefinition.PhysicalMaterial);
                }
                info.Velocity = this.m_velocity_Combined;
                float damage = ((entity is IMyCharacter) ? (flag ? this.m_projectileAmmoDefinition.ProjectileHeadShotDamage : this.m_projectileAmmoDefinition.ProjectileHealthDamage) : this.m_projectileAmmoDefinition.ProjectileMassDamage) * damageMultiplier;
                if (!MySessionComponentSafeZones.IsActionAllowed(info.Position, MySafeZoneAction.Damage, 0L))
                {
                    damage = 0f;
                }
                if (damage > 0f)
                {
                    this.DoDamage(damage, info, obj2, entity);
                }
                MyDecals.HandleAddDecal(entity, info, hash, this.m_projectileAmmoDefinition.PhysicalMaterial, obj2 as MyCharacterHitInfo, damage);
                this.CreateDecal(hash);
                if (!Sandbox.Engine.Platform.Game.IsDedicated && !CollisionParticlesTimedCache.IsPlaceUsed(info.Position, CollisionParticlesSpaceMapping, MySandboxGame.TotalSimulationTimeInMilliseconds + MyRandom.Instance.Next(0, CollisionParticlesTimedCache.EventTimeoutMs / 2), true))
                {
                    IMyEntity parent = entity;
                    if ((entity is MyCubeBlock) && (entity.Parent != null))
                    {
                        parent = entity.Parent;
                    }
                    if (!MyMaterialPropertiesHelper.Static.TryCreateCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Hit, info.Position, info.Normal, this.m_projectileAmmoDefinition.PhysicalMaterial, hash, parent) && (enum2 != MySurfaceImpactEnum.CHARACTER))
                    {
                        CreateBasicHitParticles(this.m_projectileAmmoDefinition.ProjectileOnHitEffectName, ref info.Position, ref info.Normal, ref ed.Direction, entity, this.m_weapon, 1f, this.OwnerEntity);
                    }
                }
                if ((damage > 0f) && ((this.m_weapon == null) || !ReferenceEquals(entity.GetTopMostParent(null), this.m_weapon.GetTopMostParent(null))))
                {
                    ApplyProjectileForce(entity, info.Position, (Vector3) this.m_directionNormalized, false, this.m_projectileAmmoDefinition.ProjectileHitImpulse * 0.5f);
                }
                this.StopEffect();
                this.m_state = MyProjectileStateEnum.KILLED;
            }
            return false;
        }

        private enum MyProjectileStateEnum : byte
        {
            ACTIVE = 0,
            KILLED = 1
        }
    }
}


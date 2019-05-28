namespace Sandbox.Game
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyExplosion
    {
        private static readonly MyProjectileAmmoDefinition SHRAPNEL_DATA = new MyProjectileAmmoDefinition();
        private BoundingSphereD m_explosionSphere;
        private MyLight m_light;
        public int ElapsedMiliseconds;
        private Vector3 m_velocity;
        private MyParticleEffect m_explosionEffect;
        private HashSet<MySlimBlock> m_explodedBlocksInner = new HashSet<MySlimBlock>();
        private HashSet<MySlimBlock> m_explodedBlocksExact = new HashSet<MySlimBlock>();
        private HashSet<MySlimBlock> m_explodedBlocksOuter = new HashSet<MySlimBlock>();
        private MyGridExplosion m_gridExplosion = new MyGridExplosion();
        private MyExplosionInfo m_explosionInfo;
        private bool m_explosionTriggered;
        private MyGridExplosion m_damageInfo;
        private Vector2[] m_explosionForceSlices = new Vector2[] { new Vector2(0.8f, 1f), new Vector2(1f, 0.5f), new Vector2(1.2f, 0.2f) };
        private HashSet<VRage.Game.Entity.MyEntity> m_pushedEntities = new HashSet<VRage.Game.Entity.MyEntity>();
        public static bool DEBUG_EXPLOSIONS = false;
        private static readonly HashSet<MyVoxelBase> m_rootVoxelsToCutTmp = new HashSet<MyVoxelBase>();
        private static readonly List<MyVoxelBase> m_overlappingVoxelsTmp = new List<MyVoxelBase>();
        private static MySoundPair m_explPlayer = new MySoundPair("WepExplOnPlay", false);
        private static MySoundPair m_smMissileShip = new MySoundPair("WepSmallMissileExplShip", false);
        private static MySoundPair m_smMissileExpl = new MySoundPair("WepSmallMissileExpl", false);
        private static MySoundPair m_lrgWarheadExpl = new MySoundPair("WepLrgWarheadExpl", false);
        private static MySoundPair m_missileExpl = new MySoundPair("WepMissileExplosion", false);
        public static MySoundPair SmallWarheadExpl = new MySoundPair("WepSmallWarheadExpl", false);
        public static MySoundPair SmallPoofSound = new MySoundPair("PoofExplosionCat1", false);
        public static MySoundPair LargePoofSound = new MySoundPair("PoofExplosionCat3", false);

        static MyExplosion()
        {
            SHRAPNEL_DATA.DesiredSpeed = 100f;
            SHRAPNEL_DATA.SpeedVar = 0f;
            SHRAPNEL_DATA.MaxTrajectory = 1000f;
            SHRAPNEL_DATA.ProjectileHitImpulse = 10f;
            SHRAPNEL_DATA.ProjectileMassDamage = 10f;
            SHRAPNEL_DATA.ProjectileHealthDamage = 10f;
            SHRAPNEL_DATA.ProjectileTrailColor = MyProjectilesConstants.GetProjectileTrailColorByType(MyAmmoType.HighSpeed);
            SHRAPNEL_DATA.AmmoType = MyAmmoType.HighSpeed;
            SHRAPNEL_DATA.ProjectileTrailScale = 0.1f;
            SHRAPNEL_DATA.ProjectileOnHitEffectName = "Hit_BasicAmmoSmall";
        }

        private void ApplyExplosionOnEntities(ref MyExplosionInfo m_explosionInfo, List<VRage.Game.Entity.MyEntity> entities)
        {
            foreach (VRage.Game.Entity.MyEntity entity in entities)
            {
                if (!(entity is IMyDestroyableObject))
                {
                    continue;
                }
                float damage = !(entity is MyCharacter) ? m_explosionInfo.Damage : m_explosionInfo.PlayerDamage;
                if (damage != 0f)
                {
                    MyHitInfo? hitInfo = null;
                    (entity as IMyDestroyableObject).DoDamage(damage, MyDamageType.Explosion, true, hitInfo, (m_explosionInfo.OwnerEntity != null) ? m_explosionInfo.OwnerEntity.EntityId : 0L);
                }
            }
        }

        private void ApplyExplosionOnVoxel(ref MyExplosionInfo explosionInfo)
        {
            if (((explosionInfo.AffectVoxels && MySession.Static.EnableVoxelDestruction) && MySession.Static.HighSimulationQuality) && (explosionInfo.Damage > 0f))
            {
                MySession.Static.VoxelMaps.GetAllOverlappingWithSphere(ref this.m_explosionSphere, m_overlappingVoxelsTmp);
                int num = m_overlappingVoxelsTmp.Count - 1;
                while (true)
                {
                    if (num < 0)
                    {
                        m_overlappingVoxelsTmp.Clear();
                        foreach (MyVoxelBase base2 in m_rootVoxelsToCutTmp)
                        {
                            bool createDebris = true;
                            CutOutVoxelMap(((float) this.m_explosionSphere.Radius) * explosionInfo.VoxelCutoutScale, explosionInfo.VoxelExplosionCenter, base2, createDebris, false);
                            base2.RequestVoxelCutoutSphere(explosionInfo.VoxelExplosionCenter, ((float) this.m_explosionSphere.Radius) * explosionInfo.VoxelCutoutScale, createDebris, false);
                        }
                        m_rootVoxelsToCutTmp.Clear();
                        break;
                    }
                    m_rootVoxelsToCutTmp.Add(m_overlappingVoxelsTmp[num].RootVoxel);
                    num--;
                }
            }
        }

        public void ApplyVolumetricDamageToGrid()
        {
            if (this.m_damageInfo != null)
            {
                this.ApplyVolumetricDamageToGrid(this.m_damageInfo, (this.m_explosionInfo.OwnerEntity != null) ? this.m_explosionInfo.OwnerEntity.EntityId : 0L);
            }
            this.m_damageInfo = null;
        }

        private void ApplyVolumetricDamageToGrid(MyGridExplosion damageInfo, long attackerId)
        {
            Dictionary<MySlimBlock, float> damagedBlocks = damageInfo.DamagedBlocks;
            HashSet<MySlimBlock> affectedCubeBlocks = damageInfo.AffectedCubeBlocks;
            HashSet<MyCubeGrid> affectedCubeGrids = damageInfo.AffectedCubeGrids;
            if (MyDebugDrawSettings.DEBUG_DRAW_VOLUMETRIC_EXPLOSION_COLORING)
            {
                foreach (MySlimBlock block in affectedCubeBlocks)
                {
                    block.CubeGrid.ChangeColor(block, new Vector3(0.66f, 1f, 1f));
                }
                foreach (KeyValuePair<MySlimBlock, float> pair in damagedBlocks)
                {
                    float num = 1f - (pair.Value / damageInfo.Damage);
                    pair.Key.CubeGrid.ChangeColor(pair.Key, new Vector3(num / 3f, 1f, 0.5f));
                }
            }
            else
            {
                bool hasAnyBeforeHandler = MyDamageSystem.Static.HasAnyBeforeHandler;
                foreach (KeyValuePair<MySlimBlock, float> pair2 in damagedBlocks)
                {
                    MySlimBlock key = pair2.Key;
                    if (!key.CubeGrid.MarkedForClose && (((key.FatBlock == null) || !key.FatBlock.MarkedForClose) && (!key.IsDestroyed && key.CubeGrid.BlocksDestructionEnabled)))
                    {
                        float amount = pair2.Value;
                        if (hasAnyBeforeHandler && key.UseDamageSystem)
                        {
                            MyDamageInformation info = new MyDamageInformation(false, amount, MyDamageType.Explosion, attackerId);
                            MyDamageSystem.Static.RaiseBeforeDamageApplied(key, ref info);
                            if (info.Amount <= 0f)
                            {
                                continue;
                            }
                            amount = info.Amount;
                        }
                        if (affectedCubeBlocks.Contains(pair2.Key) && !this.m_explosionInfo.KeepAffectedBlocks)
                        {
                            key.CubeGrid.RemoveDestroyedBlock(key, 0L);
                        }
                        else
                        {
                            if (((key.FatBlock == null) && ((key.Integrity / key.DeformationRatio) < amount)) || ReferenceEquals(key.FatBlock, this.m_explosionInfo.HitEntity))
                            {
                                key.CubeGrid.RemoveDestroyedBlock(key, 0L);
                            }
                            else
                            {
                                if (key.FatBlock != null)
                                {
                                    amount *= 7f;
                                }
                                MyHitInfo? hitInfo = null;
                                key.DoDamage(amount, MyDamageType.Explosion, true, hitInfo, 0L);
                                if (!key.IsDestroyed)
                                {
                                    hitInfo = null;
                                    key.CubeGrid.ApplyDestructionDeformation(key, 1f, hitInfo, 0L);
                                }
                            }
                            foreach (MySlimBlock block3 in key.Neighbours)
                            {
                                block3.CubeGrid.Physics.AddDirtyBlock(block3);
                            }
                            key.CubeGrid.Physics.AddDirtyBlock(key);
                        }
                    }
                }
            }
        }

        private unsafe bool ApplyVolumetricExplosion(ref MyExplosionInfo m_explosionInfo, List<VRage.Game.Entity.MyEntity> entities)
        {
            bool gridWasHit = false;
            BoundingSphereD explosionSphere = this.m_explosionSphere;
            double* numPtr1 = (double*) ref explosionSphere.Radius;
            numPtr1[0] *= 1.25;
            MyGridExplosion explosionDamageInfo = this.ApplyVolumetricExplosionOnGrid(ref m_explosionInfo, ref explosionSphere, entities);
            if ((m_explosionInfo.ExplosionFlags & MyExplosionFlags.APPLY_DEFORMATION) == MyExplosionFlags.APPLY_DEFORMATION)
            {
                this.m_damageInfo = explosionDamageInfo;
                this.m_damageInfo.ComputeDamagedBlocks();
                gridWasHit = this.m_damageInfo.GridWasHit;
            }
            if (m_explosionInfo.HitEntity is MyWarhead)
            {
                MySlimBlock slimBlock = (m_explosionInfo.HitEntity as MyWarhead).SlimBlock;
                if (!slimBlock.CubeGrid.BlocksDestructionEnabled)
                {
                    slimBlock.CubeGrid.RemoveDestroyedBlock(slimBlock, 0L);
                    foreach (MySlimBlock block2 in slimBlock.Neighbours)
                    {
                        block2.CubeGrid.Physics.AddDirtyBlock(block2);
                    }
                    slimBlock.CubeGrid.Physics.AddDirtyBlock(slimBlock);
                }
            }
            this.ApplyVolumetricExplosionOnEntities(ref m_explosionInfo, entities, explosionDamageInfo);
            entities.Clear();
            return gridWasHit;
        }

        private unsafe void ApplyVolumetricExplosionOnEntities(ref MyExplosionInfo m_explosionInfo, List<VRage.Game.Entity.MyEntity> entities, MyGridExplosion explosionDamageInfo)
        {
            float radius = ((float) explosionDamageInfo.Sphere.Radius) * 2f;
            float num2 = 1f / radius;
            float num3 = 1f / ((float) explosionDamageInfo.Sphere.Radius);
            HkSphereShape? nullable = null;
            foreach (VRage.Game.Entity.MyEntity entity in entities)
            {
                BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                float num4 = (float) worldAABB.Distance(this.m_explosionSphere.Center);
                float num5 = num4 * num2;
                if (num5 <= 1f)
                {
                    float num6 = 1f - (num5 * num5);
                    MyAmmoBase base2 = entity as MyAmmoBase;
                    if (base2 != null)
                    {
                        base2.MarkForDestroy();
                    }
                    else if (((entity.Physics != null) && (entity.Physics.Enabled && !entity.Physics.IsStatic)) && m_explosionInfo.ApplyForceAndDamage)
                    {
                        Vector3 vector;
                        m_explosionInfo.StrengthImpulse = 100f * ((float) this.m_explosionSphere.Radius);
                        m_explosionInfo.StrengthAngularImpulse = 50000f;
                        m_explosionInfo.HitEntity = m_explosionInfo.HitEntity?.GetBaseEntity();
                        if (((m_explosionInfo.Direction != null) && (m_explosionInfo.HitEntity != null)) && ReferenceEquals(m_explosionInfo.HitEntity.GetTopMostParent(null), entity))
                        {
                            vector = m_explosionInfo.Direction.Value;
                        }
                        else
                        {
                            Vector3 zero = Vector3.Zero;
                            MyCubeGrid grid = entity as MyCubeGrid;
                            if (grid != null)
                            {
                                if (nullable == null)
                                {
                                    nullable = new HkSphereShape(radius);
                                }
                                HkRigidBody rigidBody = grid.Physics.RigidBody;
                                Matrix rigidBodyMatrix = rigidBody.GetRigidBodyMatrix();
                                Vector3 translation = rigidBodyMatrix.Translation;
                                Quaternion quaternion = Quaternion.CreateFromRotationMatrix(rigidBodyMatrix);
                                Vector3 vector4 = (Vector3) grid.Physics.WorldToCluster(this.m_explosionSphere.Center);
                                using (ClearToken<HkShapeCollision> token = MyPhysics.GetPenetrationsShapeShape(nullable.Value, ref vector4, ref Quaternion.Identity, rigidBody.GetShape(), ref translation, ref quaternion))
                                {
                                    float gridSize = grid.GridSize;
                                    MyGridShape shape = grid.Physics.Shape;
                                    int num13 = Math.Min(token.List.Count, 100);
                                    BoundingSphere sphere = new BoundingSphere((Vector3) (Vector3D.Transform(this.m_explosionSphere.Center, grid.PositionComp.WorldMatrixNormalizedInv) / ((double) gridSize)), ((float) this.m_explosionSphere.Radius) / gridSize);
                                    BoundingBoxI xi = BoundingBoxI.CreateFromSphere(sphere);
                                    xi.Inflate(1);
                                    int num14 = 0;
                                    Vector3 vector5 = Vector3.Zero;
                                    int num15 = 0;
                                    while (true)
                                    {
                                        if (num15 >= num13)
                                        {
                                            if (vector5 != Vector3.Zero)
                                            {
                                                zero = (Vector3) (grid.GridIntegerToWorld(vector5 / ((float) num14)) - this.m_explosionSphere.Center);
                                            }
                                            break;
                                        }
                                        HkShapeCollision collision = token.List[num15];
                                        if ((collision.ShapeKeyCount != 0) && (collision.ShapeKeyCount <= 1))
                                        {
                                            Vector3I vectori;
                                            Vector3I vectori2;
                                            shape.GetShapeBounds(collision.GetShapeKey(0), out vectori, out vectori2);
                                            if (!(vectori != vectori2))
                                            {
                                                num14++;
                                                vector5 += vectori;
                                            }
                                            else
                                            {
                                                MySlimBlock cubeBlock = grid.GetCubeBlock(vectori);
                                                if (cubeBlock != null)
                                                {
                                                    if (cubeBlock.FatBlock != null)
                                                    {
                                                        num14++;
                                                        vector5 += new Vector3((Vector3I) (vectori2 + vectori)) / 2f;
                                                    }
                                                    else
                                                    {
                                                        Vector3I* vectoriPtr1 = (Vector3I*) ref vectori;
                                                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr1, ref xi.Min, ref xi.Max, out vectori);
                                                        Vector3I* vectoriPtr2 = (Vector3I*) ref vectori2;
                                                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr2, ref xi.Min, ref xi.Max, out vectori2);
                                                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
                                                        while (iterator.IsValid())
                                                        {
                                                            Vector3I current = iterator.Current;
                                                            if (sphere.Contains((Vector3) current) == ContainmentType.Contains)
                                                            {
                                                                num14++;
                                                                vector5 += current;
                                                            }
                                                            iterator.MoveNext();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        num15++;
                                    }
                                }
                            }
                            if (zero == Vector3.Zero)
                            {
                                zero = (Vector3) (entity.PositionComp.WorldAABB.Center - this.m_explosionSphere.Center);
                            }
                            zero.Normalize();
                            vector = zero;
                        }
                        bool flag = !(entity is MyCubeGrid) || MyExplosions.ShouldUseMassScaleForEntity(entity);
                        float num10 = (num6 / (flag ? 50f : 1f)) * m_explosionInfo.StrengthImpulse;
                        float mass = entity.Physics.Mass;
                        if (flag)
                        {
                            num10 *= mass * MathHelper.Lerp((float) 0.1f, (float) 1f, (float) (1f - MyMath.FastTanH(mass / 1000000f)));
                        }
                        else
                        {
                            num10 = Math.Min(num10, mass);
                        }
                        Vector3? torque = null;
                        float? maxSpeed = null;
                        entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(vector * num10), new Vector3D?(this.m_explosionSphere.Center), torque, maxSpeed, true, false);
                    }
                    if (((entity is IMyDestroyableObject) || (base2 != null)) || m_explosionInfo.ApplyForceAndDamage)
                    {
                        MyCharacter character = entity as MyCharacter;
                        if (character != null)
                        {
                            MyCockpit isUsing = character.IsUsing as MyCockpit;
                            if (isUsing != null)
                            {
                                if (explosionDamageInfo.DamagedBlocks.ContainsKey(isUsing.SlimBlock))
                                {
                                    character.DoDamage(explosionDamageInfo.DamageRemaining[isUsing.SlimBlock].DamageRemaining, MyDamageType.Explosion, true, (m_explosionInfo.OwnerEntity != null) ? m_explosionInfo.OwnerEntity.EntityId : 0L);
                                }
                                continue;
                            }
                        }
                        if (!(entity is MyCubeGrid))
                        {
                            IMyDestroyableObject obj2 = entity as IMyDestroyableObject;
                            if (obj2 != null)
                            {
                                float num7 = num4 * num3;
                                if (num7 <= 1f)
                                {
                                    float num8 = 1f - (num7 * num7);
                                    MyHitInfo? hitInfo = null;
                                    obj2.DoDamage(explosionDamageInfo.Damage * num8, MyDamageType.Explosion, true, hitInfo, (m_explosionInfo.OwnerEntity != null) ? m_explosionInfo.OwnerEntity.EntityId : 0L);
                                }
                            }
                        }
                    }
                }
            }
            if (nullable != null)
            {
                nullable.Value.Base.RemoveReference();
            }
        }

        private MyGridExplosion ApplyVolumetricExplosionOnGrid(ref MyExplosionInfo explosionInfo, ref BoundingSphereD sphere, List<VRage.Game.Entity.MyEntity> entities)
        {
            this.m_gridExplosion.Init(explosionInfo.ExplosionSphere, explosionInfo.Damage);
            MyCubeGrid node = null;
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group objB = null;
            if (!MySession.Static.Settings.EnableTurretsFriendlyFire && (explosionInfo.OriginEntity != 0))
            {
                VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(explosionInfo.OriginEntity, false);
                if (entityById != null)
                {
                    node = entityById.GetTopMostParent(null) as MyCubeGrid;
                    if (node != null)
                    {
                        objB = MyCubeGridGroups.Static.Logical.GetGroup(node);
                    }
                }
            }
            foreach (VRage.Game.Entity.MyEntity entity2 in entities)
            {
                if (ReferenceEquals(entity2, explosionInfo.ExcludedEntity))
                {
                    continue;
                }
                MyCubeGrid objA = entity2 as MyCubeGrid;
                if ((objA != null) && (objA.CreatePhysics && (!ReferenceEquals(objA, node) && ((objB == null) || !ReferenceEquals(MyCubeGridGroups.Static.Logical.GetGroup(objA), objB)))))
                {
                    this.m_gridExplosion.AffectedCubeGrids.Add(objA);
                    float detectionBlockHalfSize = (objA.GridSize / 2f) / 1.25f;
                    MatrixD worldMatrixInvScaled = objA.PositionComp.WorldMatrixInvScaled;
                    BoundingSphereD ed = new BoundingSphereD(sphere.Center, (double) ((float) Math.Max((double) 0.10000000149011612, (double) (sphere.Radius - objA.GridSize))));
                    BoundingSphereD ed2 = new BoundingSphereD(sphere.Center, sphere.Radius);
                    BoundingSphereD ed3 = new BoundingSphereD(sphere.Center, sphere.Radius + ((objA.GridSize * 0.5f) * ((float) Math.Sqrt(3.0))));
                    objA.GetBlocksInsideSpheres(ref ed, ref ed2, ref ed3, this.m_explodedBlocksInner, this.m_explodedBlocksExact, this.m_explodedBlocksOuter, false, detectionBlockHalfSize, ref worldMatrixInvScaled);
                    this.m_explodedBlocksInner.UnionWith(this.m_explodedBlocksExact);
                    this.m_gridExplosion.AffectedCubeBlocks.UnionWith(this.m_explodedBlocksInner);
                    foreach (MySlimBlock block in this.m_explodedBlocksOuter)
                    {
                        objA.Physics.AddDirtyBlock(block);
                    }
                    this.m_explodedBlocksInner.Clear();
                    this.m_explodedBlocksExact.Clear();
                    this.m_explodedBlocksOuter.Clear();
                }
            }
            return this.m_gridExplosion;
        }

        private MatrixD CalculateEffectMatrix(BoundingSphereD explosionSphere)
        {
            Vector3D vectord = MySector.MainCamera.Position - explosionSphere.Center;
            vectord = !MyUtils.IsZero(vectord, 1E-05f) ? MyUtils.Normalize(vectord) : MySector.MainCamera.ForwardVector;
            return MatrixD.CreateTranslation(this.m_explosionSphere.Center + (vectord * 0.89999997615814209));
        }

        public void Clear()
        {
            this.m_gridExplosion.DamagedBlocks.Clear();
            this.m_gridExplosion.AffectedCubeGrids.Clear();
            this.m_gridExplosion.AffectedCubeBlocks.Clear();
        }

        public void Close()
        {
            if (this.m_light != null)
            {
                MyLights.RemoveLight(this.m_light);
                this.m_light = null;
            }
        }

        private void CreateParticleEffectInternal()
        {
            string customEffect;
            switch (this.m_explosionInfo.ExplosionType)
            {
                case MyExplosionTypeEnum.MISSILE_EXPLOSION:
                    customEffect = "Explosion_Missile";
                    break;

                case MyExplosionTypeEnum.BOMB_EXPLOSION:
                    customEffect = "Dummy";
                    break;

                case MyExplosionTypeEnum.AMMO_EXPLOSION:
                    customEffect = "Dummy";
                    break;

                case MyExplosionTypeEnum.GRID_DEFORMATION:
                    customEffect = "Dummy";
                    break;

                case MyExplosionTypeEnum.GRID_DESTRUCTION:
                    customEffect = "Grid_Destruction";
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_02:
                    customEffect = "Explosion_Warhead_02";
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_15:
                    customEffect = "Explosion_Warhead_15";
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_30:
                    customEffect = "Explosion_Warhead_30";
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_50:
                    customEffect = "Explosion_Warhead_50";
                    break;

                case MyExplosionTypeEnum.CUSTOM:
                    customEffect = this.m_explosionInfo.CustomEffect;
                    break;

                default:
                    throw new NotImplementedException();
            }
            this.GenerateExplosionParticles(customEffect, this.m_explosionSphere, this.m_explosionInfo.ParticleScale);
        }

        public static void CutOutVoxelMap(float radius, Vector3D center, MyVoxelBase voxelMap, bool createDebris, bool damage = false)
        {
            MyShapeSphere sphere1 = new MyShapeSphere();
            sphere1.Center = center;
            sphere1.Radius = radius;
            MyShapeSphere sphereShape = sphere1;
            MyVoxelBase.OnCutOutResults results = null;
            if (createDebris)
            {
                results = (x, y, z) => OnCutOutVoxelMap(x, y, sphereShape, voxelMap);
            }
            voxelMap.CutOutShapeWithPropertiesAsync(results, sphereShape, false, false, damage, false, true);
        }

        public void DebugDraw()
        {
            if (DEBUG_EXPLOSIONS)
            {
                MyLight light = this.m_light;
            }
        }

        private void GenerateExplosionParticles(string newParticlesName, BoundingSphereD explosionSphere, float particleScale)
        {
            if (MyParticlesManager.TryCreateParticleEffect(newParticlesName, this.CalculateEffectMatrix(explosionSphere), out this.m_explosionEffect))
            {
                this.m_explosionEffect.OnDelete += delegate (object <p0>, System.EventArgs <p1>) {
                    this.m_explosionInfo.LifespanMiliseconds = 0;
                    this.m_explosionEffect = null;
                };
                this.m_explosionEffect.UserScale = particleScale;
            }
        }

        private MySoundPair GetCueByExplosionType(MyExplosionTypeEnum explosionType)
        {
            MySoundPair smMissileShip = null;
            switch (explosionType)
            {
                case MyExplosionTypeEnum.MISSILE_EXPLOSION:
                {
                    bool flag = false;
                    if (this.m_explosionInfo.HitEntity is MyCubeGrid)
                    {
                        foreach (MySlimBlock block in (this.m_explosionInfo.HitEntity as MyCubeGrid).GetBlocks())
                        {
                            if ((block.FatBlock is MyCockpit) && ReferenceEquals((block.FatBlock as MyCockpit).Pilot, MySession.Static.ControlledEntity))
                            {
                                smMissileShip = m_smMissileShip;
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        smMissileShip = m_smMissileExpl;
                    }
                    break;
                }
                case MyExplosionTypeEnum.BOMB_EXPLOSION:
                    smMissileShip = m_lrgWarheadExpl;
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_02:
                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_15:
                    smMissileShip = SmallWarheadExpl;
                    break;

                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_30:
                case MyExplosionTypeEnum.WARHEAD_EXPLOSION_50:
                    smMissileShip = m_lrgWarheadExpl;
                    break;

                case MyExplosionTypeEnum.CUSTOM:
                    smMissileShip = this.m_explosionInfo.CustomSound;
                    break;

                default:
                    smMissileShip = m_missileExpl;
                    break;
            }
            return smMissileShip;
        }

        private Vector4 GetSmutDecalRandomColor() => 
            new Vector4(MyUtils.GetRandomFloat(0.2f, 0.3f), MyUtils.GetRandomFloat(0.2f, 0.3f), MyUtils.GetRandomFloat(0.2f, 0.3f), 1f);

        private static void OnCutOutVoxelMap(float voxelsCountInPercent, MyVoxelMaterialDefinition voxelMaterial, MyShapeSphere sphereShape, MyVoxelBase voxelMap)
        {
            if ((voxelsCountInPercent > 0f) && (voxelMaterial != null))
            {
                MyParticleEffect effect;
                BoundingSphereD explosionSphere = new BoundingSphereD(sphereShape.Center, (double) sphereShape.Radius);
                if (MyRenderConstants.RenderQualityProfile.ExplosionDebrisCountMultiplier > 0f)
                {
                    if (voxelMaterial.DamagedMaterial != MyStringHash.NullOrEmpty)
                    {
                        voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(voxelMaterial.DamagedMaterial.ToString());
                    }
                    MyDebris.Static.CreateExplosionDebris(ref explosionSphere, voxelsCountInPercent, voxelMaterial, voxelMap);
                }
                if (MyParticlesManager.TryCreateParticleEffect("MaterialExplosion_Destructible", MatrixD.CreateTranslation(explosionSphere.Center), out effect))
                {
                    effect.UserRadiusMultiplier = (float) explosionSphere.Radius;
                    effect.UserScale = 0.2f;
                }
            }
        }

        private void PerformCameraShake(float intensityWeight)
        {
            if (MySector.MainCamera != null)
            {
                float num = MySector.MainCamera.CameraShake.MaxShake * intensityWeight;
                Vector3D vectord = MySector.MainCamera.Position - this.m_explosionSphere.Center;
                double num2 = 1.0 / vectord.LengthSquared();
                float num3 = (float) ((this.m_explosionSphere.Radius * this.m_explosionSphere.Radius) * num2);
                if (num3 > 1E-05f)
                {
                    MySector.MainCamera.CameraShake.AddShake(num * num3);
                    MySector.MainCamera.CameraSpring.AddCurrentCameraControllerVelocity((Vector3) ((num * vectord) * num2));
                }
            }
        }

        private void PlaySound()
        {
            MySoundPair cueByExplosionType = this.GetCueByExplosionType(this.m_explosionInfo.ExplosionType);
            if ((cueByExplosionType != null) && !ReferenceEquals(cueByExplosionType, MySoundPair.Empty))
            {
                MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                if (emitter != null)
                {
                    emitter.SetPosition(new Vector3D?(this.m_explosionSphere.Center));
                    emitter.Entity = this.m_explosionInfo.HitEntity;
                    bool? nullable = null;
                    emitter.PlaySound(cueByExplosionType, false, false, false, false, false, nullable);
                }
            }
            if (ReferenceEquals(this.m_explosionInfo.HitEntity, MySession.Static.ControlledEntity))
            {
                MyAudio.Static.PlaySound(m_explPlayer.SoundId, null, MySoundDimensions.D2, false, false);
            }
        }

        private unsafe void RemoveDestroyedObjects()
        {
            if (this.m_explosionInfo.Damage > 0f)
            {
                this.ApplyExplosionOnVoxel(ref this.m_explosionInfo);
                BoundingSphereD explosionSphere = this.m_explosionSphere;
                double* numPtr1 = (double*) ref explosionSphere.Radius;
                numPtr1[0] *= 2.0;
                List<VRage.Game.Entity.MyEntity> topMostEntitiesInSphere = Sandbox.Game.Entities.MyEntities.GetTopMostEntitiesInSphere(ref explosionSphere);
                this.ApplyVolumetricExplosion(ref this.m_explosionInfo, topMostEntitiesInSphere);
            }
            if ((0 != 0) && this.m_explosionInfo.CreateShrapnels)
            {
                for (int i = 0; i < 10; i++)
                {
                    VRage.Game.Entity.MyEntity[] entityArray2;
                    if (this.m_explosionInfo.HitEntity is MyWarhead)
                    {
                        entityArray2 = new VRage.Game.Entity.MyEntity[] { this.m_explosionInfo.HitEntity };
                    }
                    else
                    {
                        entityArray2 = null;
                    }
                    MyProjectiles.AddShrapnel(SHRAPNEL_DATA, entityArray2, (Vector3) this.m_explosionSphere.Center, Vector3.Zero, MyUtils.GetRandomVector3Normalized(), false, 1f, 1f, null, null, 1f);
                }
            }
        }

        public void Start(MyExplosionInfo explosionInfo)
        {
            this.m_explosionInfo = explosionInfo;
            this.ElapsedMiliseconds = 0;
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_explosionInfo.CreateDebris = false;
                this.m_explosionInfo.PlaySound = false;
                this.m_explosionInfo.CreateParticleDebris = false;
                this.m_explosionInfo.CreateParticleEffect = false;
                this.m_explosionInfo.CreateDecals = false;
            }
        }

        private void StartInternal()
        {
            this.m_velocity = this.m_explosionInfo.Velocity;
            this.m_explosionSphere = this.m_explosionInfo.ExplosionSphere;
            if (this.m_explosionInfo.PlaySound)
            {
                this.PlaySound();
            }
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.m_light.Start(this.m_explosionSphere.Center, MyExplosionsConstants.EXPLOSION_LIGHT_COLOR, Math.Min((float) (((float) this.m_explosionSphere.Radius) * 8f), (float) 120f), "MyExplosion");
                this.m_light.Intensity = 2f;
                this.m_light.Range = Math.Min((float) (((float) this.m_explosionSphere.Radius) * 3f), (float) 120f);
            }
            if (this.m_explosionInfo.CreateParticleEffect)
            {
                this.CreateParticleEffectInternal();
            }
            if (this.m_explosionInfo.CreateDebris && (this.m_explosionInfo.HitEntity != null))
            {
                BoundingBoxD bb = BoundingBoxD.CreateFromSphere(new BoundingSphereD(this.m_explosionSphere.Center, this.m_explosionSphere.Radius * 0.699999988079071));
                MyDebris.Static.CreateExplosionDebris(ref this.m_explosionSphere, this.m_explosionInfo.HitEntity, ref bb, 0.5f, true);
            }
            if (this.m_explosionInfo.CreateParticleDebris)
            {
                this.GenerateExplosionParticles("Explosion_Debris", this.m_explosionSphere, 1f);
            }
            this.m_explosionTriggered = false;
        }

        public unsafe bool Update()
        {
            if (this.ElapsedMiliseconds == 0)
            {
                this.StartInternal();
            }
            this.ElapsedMiliseconds += 0x10;
            if (this.ElapsedMiliseconds < MyExplosionsConstants.CAMERA_SHAKE_TIME_MS)
            {
                this.PerformCameraShake(1f - (((float) this.ElapsedMiliseconds) / MyExplosionsConstants.CAMERA_SHAKE_TIME_MS));
            }
            if (this.ElapsedMiliseconds > this.m_explosionInfo.ObjectsRemoveDelayInMiliseconds)
            {
                if (Sync.IsServer)
                {
                    this.RemoveDestroyedObjects();
                }
                this.m_explosionInfo.ObjectsRemoveDelayInMiliseconds = 0x7fffffff;
                this.m_explosionTriggered = true;
            }
            if (this.m_light != null)
            {
                float num = 1f - (((float) this.ElapsedMiliseconds) / ((float) this.m_explosionInfo.LifespanMiliseconds));
                this.m_light.Intensity = 2f * num;
            }
            if (this.m_explosionEffect != null)
            {
                Vector3D* vectordPtr1 = (Vector3D*) ref this.m_explosionSphere.Center;
                vectordPtr1[0] += this.m_velocity * 0.01666667f;
                this.m_explosionEffect.WorldMatrix = this.CalculateEffectMatrix(this.m_explosionSphere);
            }
            else if ((this.ElapsedMiliseconds >= this.m_explosionInfo.LifespanMiliseconds) && this.m_explosionTriggered)
            {
                if (DEBUG_EXPLOSIONS)
                {
                    return true;
                }
                this.Close();
                return false;
            }
            return ((this.m_explosionEffect == null) || !this.m_explosionEffect.IsStopped);
        }
    }
}


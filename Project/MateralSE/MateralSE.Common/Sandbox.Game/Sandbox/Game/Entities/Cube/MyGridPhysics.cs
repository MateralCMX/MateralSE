namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Entities.EnvironmentItems;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridPhysics : MyPhysicsBody
    {
        private List<HkdBreakableBodyInfo> m_newBodies;
        private List<HkdShapeInstanceInfo> m_children;
        private static List<HkdShapeInstanceInfo> m_tmpChildren_RemoveShapes = new List<HkdShapeInstanceInfo>();
        private static List<HkdShapeInstanceInfo> m_tmpChildren_CompoundIds = new List<HkdShapeInstanceInfo>();
        private static List<string> m_tmpShapeNames = new List<string>();
        private static HashSet<MySlimBlock> m_tmpBlocksToDelete = new HashSet<MySlimBlock>();
        private static HashSet<MySlimBlock> m_tmpBlocksUpdateDamage = new HashSet<MySlimBlock>();
        private static HashSet<ushort> m_tmpCompoundIds = new HashSet<ushort>();
        private static List<MyDefinitionId> m_tmpDefinitions = new List<MyDefinitionId>();
        private bool m_recreateBody;
        private Vector3 m_oldLinVel;
        private Vector3 m_oldAngVel;
        private List<HkdBreakableBody> m_newBreakableBodies;
        private List<MyFracturedBlock.Info> m_fractureBlocksCache;
        private Dictionary<Vector3I, List<HkdShapeInstanceInfo>> m_fracturedBlocksShapes;
        private List<MyFractureComponentBase.Info> m_fractureBlockComponentsCache;
        private Dictionary<MySlimBlock, List<HkdShapeInstanceInfo>> m_fracturedSlimBlocksShapes;
        private List<HkdShapeInstanceInfo> m_childList;
        private static readonly float LargeGridDeformationRatio = 1f;
        private static readonly float SmallGridDeformationRatio = 2.5f;
        private static readonly int MaxEffectsPerFrame = 3;
        public static readonly float LargeShipMaxAngularVelocityLimit = MathHelper.ToRadians((float) 18000f);
        public static readonly float SmallShipMaxAngularVelocityLimit = MathHelper.ToRadians((float) 36000f);
        private const float SPEED_OF_LIGHT_IN_VACUUM = 2.997924E+08f;
        public const float MAX_SHIP_SPEED = 1.498962E+08f;
        public int DisableGravity;
        private static readonly int SparksEffectDelayPerContactMs = 0x3e8;
        public const int COLLISION_SPARK_LIMIT_COUNT = 3;
        public const int COLLISION_SPARK_LIMIT_TIME = 20;
        private List<ushort> m_tmpContactId;
        private MyConcurrentDictionary<ushort, int> m_lastContacts;
        private MyCubeGrid m_grid;
        private MyGridShape m_shape;
        private List<ExplosionInfo> m_explosions;
        private const int MAX_NUM_CONTACTS_PER_FRAME = 10;
        private const int MAX_NUM_CONTACTS_PER_FRAME_SIMPLE_GRID = 1;
        private readonly MyDirtyBlocksInfo m_dirtyCubesInfo;
        [ThreadStatic]
        private static List<Vector3I> m_tmpCubeList;
        private bool m_isClientPredicted;
        private ulong m_isClientPredictedLastFrameCheck;
        private bool m_isServer;
        public float DeformationRatio;
        private Vector3 m_cachedGravity;
        public const float MAX = 100f;
        public const int PLANET_CRASH_MIN_BLOCK_COUNT = 5;
        public const int ACCUMULATION_TIME = 30;
        public const int CRASH_LIMIT = 90;
        private MyParticleEffect m_planetCrash_Effect;
        private Vector3D? m_planetCrash_CenterPoint;
        private Vector3? m_planetCrash_Normal;
        private bool m_planetCrash_IsStarted;
        private int m_planetCrash_TimeBetweenPoints;
        private int m_planetCrash_CrashAccumulation;
        private float m_planetCrash_ScaleCurrent;
        private float m_planetCrash_ScaleGoal;
        private float m_planetCrash_generationMultiplier;
        private HashSet<MySlimBlock> m_blocksInContact;
        private List<MyPhysics.HitInfo> m_hitList;
        private const float BREAK_OFFSET_MULTIPLIER = 0.7f;
        private static readonly MyConcurrentPool<HashSet<Vector3I>> m_dirtyCubesPool;
        private static readonly MyConcurrentPool<Dictionary<MySlimBlock, float>> m_damagedCubesPool;
        private MyConcurrentQueue<GridEffect> m_gridEffects;
        private MyConcurrentQueue<GridCollisionhit> m_gridCollisionEffects;
        private List<CollisionParticleEffect> m_collisionParticles;
        private static HashSet<Vector3I> m_dirtyBlocksSmallCache;
        private static HashSet<Vector3I> m_dirtyBlocksLargeCache;
        private int m_debrisPerFrame;
        private const int MaxDebrisPerFrame = 3;
        private static float IMP;
        private static float ACCEL;
        private static float RATIO;
        public static float PREDICTION_IMPULSE_SCALE;
        private const int TOIs_THRESHOLD = 1;
        private const int FRAMES_TO_REMEMBER_TOI_IMPACT = 20;
        private int m_lastTOIFrame;
        private HkCollidableQualityType m_savedQuality;
        private bool m_appliedSlowdownThisFrame;
        private int m_removeBlocksCallbackScheduled;
        private ulong m_frameCollided;
        private ulong m_frameFirstImpact;
        private float m_impactDot;
        private readonly List<Vector3D> m_contactPosCache;
        private readonly ConcurrentDictionary<MySlimBlock, byte> m_removedCubes;
        private readonly Dictionary<VRage.Game.Entity.MyEntity, bool> m_predictedContactEntities;
        private static readonly List<VRage.Game.Entity.MyEntity> m_predictedContactEntitiesToRemove;

        static MyGridPhysics()
        {
            m_dirtyCubesPool = new MyConcurrentPool<HashSet<Vector3I>>(10, x => x.Clear(), 0x2710, null);
            m_damagedCubesPool = new MyConcurrentPool<Dictionary<MySlimBlock, float>>(10, x => x.Clear(), 0x2710, null);
            m_dirtyBlocksSmallCache = new HashSet<Vector3I>();
            m_dirtyBlocksLargeCache = new HashSet<Vector3I>();
            IMP = 1E-07f;
            ACCEL = 0.1f;
            RATIO = 0.2f;
            PREDICTION_IMPULSE_SCALE = 0.005f;
            m_predictedContactEntitiesToRemove = new List<VRage.Game.Entity.MyEntity>();
        }

        public MyGridPhysics(MyCubeGrid grid, MyGridShape shape = null, bool staticPhysics = false) : base(grid, GetFlags(grid))
        {
            this.m_newBodies = new List<HkdBreakableBodyInfo>();
            this.m_children = new List<HkdShapeInstanceInfo>();
            this.m_newBreakableBodies = new List<HkdBreakableBody>();
            this.m_fractureBlocksCache = new List<MyFracturedBlock.Info>();
            this.m_fracturedBlocksShapes = new Dictionary<Vector3I, List<HkdShapeInstanceInfo>>();
            this.m_fractureBlockComponentsCache = new List<MyFractureComponentBase.Info>();
            this.m_fracturedSlimBlocksShapes = new Dictionary<MySlimBlock, List<HkdShapeInstanceInfo>>();
            this.m_childList = new List<HkdShapeInstanceInfo>();
            this.m_tmpContactId = new List<ushort>();
            this.m_lastContacts = new MyConcurrentDictionary<ushort, int>(0, null);
            this.m_explosions = new List<ExplosionInfo>();
            this.m_dirtyCubesInfo = new MyDirtyBlocksInfo();
            this.m_planetCrash_ScaleCurrent = 1f;
            this.m_planetCrash_ScaleGoal = 1f;
            this.m_blocksInContact = new HashSet<MySlimBlock>();
            this.m_hitList = new List<MyPhysics.HitInfo>();
            this.m_gridEffects = new MyConcurrentQueue<GridEffect>();
            this.m_gridCollisionEffects = new MyConcurrentQueue<GridCollisionhit>();
            this.m_collisionParticles = new List<CollisionParticleEffect>();
            this.m_savedQuality = HkCollidableQualityType.Invalid;
            this.m_contactPosCache = new List<Vector3D>();
            this.m_removedCubes = new ConcurrentDictionary<MySlimBlock, byte>();
            this.m_predictedContactEntities = new Dictionary<VRage.Game.Entity.MyEntity, bool>();
            this.m_grid = grid;
            this.m_shape = shape;
            this.DeformationRatio = (this.m_grid.GridSizeEnum == MyCubeSize.Large) ? LargeGridDeformationRatio : SmallGridDeformationRatio;
            base.MaterialType = VRage.Game.MyMaterialType.METAL;
            if (staticPhysics)
            {
                base.Flags = RigidBodyFlag.RBF_KINEMATIC;
            }
            this.CreateBody();
            if (MyFakes.ENABLE_PHYSICS_HIGH_FRICTION)
            {
                this.Friction = MyFakes.PHYSICS_HIGH_FRICTION;
            }
            this.m_isServer = Sync.IsServer;
        }

        public override void Activate(object world, ulong clusterObjectID)
        {
            if (MyPerGameSettings.Destruction && this.IsStatic)
            {
                this.Shape.FindConnectionsToWorld();
            }
            base.Activate(world, clusterObjectID);
            this.MarkBreakable((HkWorld) world);
        }

        public override void ActivateBatch(object world, ulong clusterObjectID)
        {
            if (MyPerGameSettings.Destruction && this.IsStatic)
            {
                this.Shape.FindConnectionsToWorld();
            }
            base.ActivateBatch(world, clusterObjectID);
            this.MarkBreakable((HkWorld) world);
        }

        protected override void ActivateCollision()
        {
            if (base.m_world != null)
            {
                this.HavokCollisionSystemID = base.m_world.GetCollisionFilter().GetNewSystemGroup();
            }
        }

        public unsafe void AddBlock(MySlimBlock block)
        {
            Vector3I vectori;
            vectori.X = block.Min.X;
            while (vectori.X <= block.Max.X)
            {
                vectori.Y = block.Min.Y;
                while (true)
                {
                    if (vectori.Y > block.Max.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = block.Min.Z;
                    while (true)
                    {
                        if (vectori.Z > block.Max.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        this.m_dirtyCubesInfo.DirtyBlocks.Add(vectori);
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
        }

        private void AddCollisionEffect(Vector3D position, Vector3 normal, float separatingSpeed, float impulse)
        {
            if (MyFakes.ENABLE_COLLISION_EFFECTS && (this.m_gridEffects.Count < MaxEffectsPerFrame))
            {
                GridEffect instance = new GridEffect {
                    Type = GridEffectType.Collision,
                    Position = position,
                    Normal = normal,
                    Scale = 1f,
                    SeparatingSpeed = separatingSpeed,
                    Impulse = impulse
                };
                this.m_gridEffects.Enqueue(instance);
                MySandboxGame.Static.Invoke(() => this.m_grid.MarkForUpdate(), "AddCollisionEffect");
            }
        }

        private void AddDestructionEffect(Vector3D position, Vector3 direction)
        {
            if (MyFakes.ENABLE_DESTRUCTION_EFFECTS && (this.m_gridEffects.Count < MaxEffectsPerFrame))
            {
                GridEffect instance = new GridEffect {
                    Type = GridEffectType.Destruction,
                    Position = position,
                    Normal = direction,
                    Scale = 1f
                };
                this.m_gridEffects.Enqueue(instance);
                MySandboxGame.Static.Invoke(() => this.m_grid.MarkForUpdate(), "AddDestructionEffect");
            }
        }

        public void AddDirtyArea(Vector3I min, Vector3I max)
        {
            BoundingBoxI entity = new BoundingBoxI {
                Min = min,
                Max = max
            };
            this.m_dirtyCubesInfo.DirtyParts.Add(entity);
            this.m_grid.MarkForUpdate();
        }

        public void AddDirtyBlock(MySlimBlock block)
        {
            BoundingBoxI entity = new BoundingBoxI {
                Min = block.Min,
                Max = block.Max
            };
            this.m_dirtyCubesInfo.DirtyParts.Add(entity);
            this.m_grid.MarkForUpdate();
        }

        private void AddDustEffect(Vector3D position, float scale)
        {
            if (this.m_gridEffects.Count < MaxEffectsPerFrame)
            {
                GridEffect instance = new GridEffect {
                    Type = GridEffectType.Dust,
                    Position = position,
                    Normal = Vector3.Forward,
                    Scale = scale
                };
                this.m_gridEffects.Enqueue(instance);
                MySandboxGame.Static.Invoke(() => this.m_grid.MarkForUpdate(), "AddDustEffect");
            }
        }

        private void AddFaces(MySlimBlock a, Vector3I ab)
        {
            if (!a.DisconnectFaces.Contains(ab * Vector3I.UnitX))
            {
                a.DisconnectFaces.Add(ab * Vector3I.UnitX);
            }
            if (!a.DisconnectFaces.Contains(ab * Vector3I.UnitY))
            {
                a.DisconnectFaces.Add(ab * Vector3I.UnitY);
            }
            if (!a.DisconnectFaces.Contains(ab * Vector3I.UnitZ))
            {
                a.DisconnectFaces.Add(ab * Vector3I.UnitZ);
            }
        }

        private void AddGridCollisionEffect(Vector3D relativePosition, Vector3 normal, Vector3 relativeVelocity, float separatingSpeed, float impulse)
        {
            if (MyFakes.ENABLE_COLLISION_EFFECTS && (this.m_gridEffects.Count < MaxEffectsPerFrame))
            {
                GridCollisionhit instance = new GridCollisionhit {
                    RelativePosition = relativePosition,
                    Normal = normal,
                    RelativeVelocity = relativeVelocity,
                    SeparatingSpeed = separatingSpeed,
                    Impulse = impulse
                };
                this.m_gridCollisionEffects.Enqueue(instance);
                MySandboxGame.Static.Invoke(() => this.m_grid.MarkForUpdate(), "AddGridCollisionEffect");
            }
        }

        public bool AnyPredictedContactEntities()
        {
            BoundingBoxD box = base.Entity.PositionComp.WorldAABB.Inflate((double) 5.0);
            foreach (KeyValuePair<VRage.Game.Entity.MyEntity, bool> pair in this.m_predictedContactEntities)
            {
                if (!pair.Key.MarkedForClose)
                {
                    if (pair.Value)
                    {
                        continue;
                    }
                    if (pair.Key.PositionComp.WorldAABB.Intersects(ref box))
                    {
                        continue;
                    }
                }
                m_predictedContactEntitiesToRemove.Add(pair.Key);
            }
            foreach (VRage.Game.Entity.MyEntity entity in m_predictedContactEntitiesToRemove)
            {
                this.m_predictedContactEntities.Remove(entity);
            }
            m_predictedContactEntitiesToRemove.Clear();
            return (this.m_predictedContactEntities.Count > 0);
        }

        public bool ApplyDeformation(float deformationOffset, float softAreaPlanar, float softAreaVertical, Vector3 localPos, Vector3 localNormal, MyStringHash damageType, float offsetThreshold = 0f, float lowerRatioLimit = 0f, long attackerId = 0L)
        {
            int num;
            return this.ApplyDeformation(deformationOffset, softAreaPlanar, softAreaVertical, localPos, localNormal, damageType, out num, offsetThreshold, lowerRatioLimit, attackerId);
        }

        public unsafe bool ApplyDeformation(float deformationOffset, float softAreaPlanar, float softAreaVertical, Vector3 localPos, Vector3 localNormal, MyStringHash damageType, out int blocksDestroyedByThisCp, float offsetThreshold = 0f, float lowerRatioLimit = 0f, long attackerId = 0L)
        {
            float num7;
            BoundingBoxI xi2;
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori3;
            blocksDestroyedByThisCp = 0;
            bool flag = false;
            if (!this.m_grid.BlocksDestructionEnabled)
            {
                return flag;
            }
            float num = this.m_grid.GridSize * 0.7f;
            float num2 = localNormal.AbsMax() * deformationOffset;
            bool isServer = Sync.IsServer;
            float softAreaPlanarInv = 1f / softAreaPlanar;
            float softAreaVerticalInv = 1f / softAreaVertical;
            Vector3I gridPos = Vector3I.Round((localPos + (this.m_grid.GridSize / 2f)) / this.m_grid.GridSize);
            Vector3D axis = localNormal;
            Vector3 up = (Vector3) MyUtils.GetRandomPerpendicularVector(ref axis);
            Vector3 vector = Vector3.Cross(up, localNormal);
            MyDamageInformation info = new MyDamageInformation(true, 1f, MyDamageType.Deformation, attackerId);
            if ((1f - (num / num2)) <= 0f)
            {
                goto TR_0004;
            }
            else
            {
                float num5 = softAreaVertical;
                float num6 = softAreaPlanar - ((num * softAreaPlanar) / num2);
                Vector3 vector2 = up * num6;
                Vector3 vector3 = vector * num6;
                BoundingBoxI xi = BoundingBoxI.CreateInvalid();
                xi.Include(Vector3I.Round((localPos + (localNormal * num5)) / this.m_grid.GridSize));
                xi.Include(Vector3I.Round(((localPos - vector2) - vector3) / this.m_grid.GridSize));
                xi.Include(Vector3I.Round(((localPos + vector2) - vector3) / this.m_grid.GridSize));
                xi.Include(Vector3I.Round(((localPos - vector2) + vector3) / this.m_grid.GridSize));
                xi.Include(Vector3I.Round(((localPos + vector2) + vector3) / this.m_grid.GridSize));
                num7 = 1f;
                xi2 = BoundingBoxI.CreateInvalid();
                vectori2 = Vector3I.Max(xi.Min, this.m_grid.Min);
                vectori3 = Vector3I.Min(xi.Max, this.m_grid.Max);
                vectori.X = vectori2.X;
            }
            goto TR_0027;
        TR_0004:
            if ((blocksDestroyedByThisCp == 0) && MySession.Static.HighSimulationQuality)
            {
                Parallel.Start(delegate {
                    this.DeformBones(deformationOffset, gridPos, softAreaPlanar, softAreaVertical, localNormal, localPos, damageType, offsetThreshold, lowerRatioLimit, attackerId, up);
                }, Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Deformations, "DeformBones"));
            }
            return flag;
        TR_0027:
            while (true)
            {
                if (vectori.X <= vectori3.X)
                {
                    vectori.Y = vectori2.Y;
                }
                else
                {
                    if (blocksDestroyedByThisCp <= 0)
                    {
                        num7 = Math.Max(num7, 0.2f);
                        softAreaPlanar *= num7;
                        softAreaVertical *= num7;
                    }
                    else if (isServer)
                    {
                        xi2.Inflate(1);
                        this.m_dirtyCubesInfo.DirtyParts.Add(xi2);
                        this.ScheduleRemoveBlocksCallbacks();
                    }
                    goto TR_0004;
                }
                break;
            }
            while (true)
            {
                if (vectori.Y > vectori3.Y)
                {
                    int* numPtr3 = (int*) ref vectori.X;
                    numPtr3[0]++;
                    break;
                }
                vectori.Z = vectori2.Z;
                while (true)
                {
                    while (true)
                    {
                        if (vectori.Z <= vectori3.Z)
                        {
                            float num8 = 1f;
                            if (vectori != gridPos)
                            {
                                Vector3 closestCorner = this.m_grid.GetClosestCorner(vectori, localPos);
                                num8 = CalculateSoften(softAreaPlanarInv, softAreaVerticalInv, ref localNormal, closestCorner - localPos);
                                if (num8 == 0f)
                                {
                                    break;
                                }
                            }
                            float num9 = num2 * num8;
                            if (num9 > num)
                            {
                                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
                                if (cubeBlock != null)
                                {
                                    if (!isServer)
                                    {
                                        num7 = Math.Min(num7, cubeBlock.DeformationRatio);
                                        if ((Math.Max(lowerRatioLimit, cubeBlock.DeformationRatio) * num9) > num)
                                        {
                                            flag = true;
                                            blocksDestroyedByThisCp++;
                                        }
                                    }
                                    else
                                    {
                                        if (cubeBlock.UseDamageSystem)
                                        {
                                            info.Amount = 1f;
                                            MyDamageSystem.Static.RaiseBeforeDamageApplied(cubeBlock, ref info);
                                            if (info.Amount == 0f)
                                            {
                                                break;
                                            }
                                        }
                                        num7 = Math.Min(num7, cubeBlock.DeformationRatio);
                                        if ((Math.Max(lowerRatioLimit, cubeBlock.DeformationRatio) * num9) > num)
                                        {
                                            flag = true;
                                            if (this.m_removedCubes.TryAdd(cubeBlock, 0))
                                            {
                                                bool flag1 = MyFakes.DEFORMATION_LOGGING;
                                                blocksDestroyedByThisCp++;
                                                xi2.Include(ref cubeBlock.Min);
                                                xi2.Include(ref cubeBlock.Max);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            continue;
                        }
                        break;
                    }
                    int* numPtr1 = (int*) ref vectori.Z;
                    numPtr1[0]++;
                }
            }
            goto TR_0027;
        }

        private void BreakableBody_AfterControllerOperation(HkdBreakableBody b)
        {
            if (this.m_recreateBody)
            {
                b.BreakableShape.SetStrenghtRecursively(MyDestructionConstants.STRENGTH, 0.7f);
            }
        }

        private void BreakableBody_BeforeControllerOperation(HkdBreakableBody b)
        {
            if (this.m_recreateBody)
            {
                b.BreakableShape.SetStrenghtRecursively(float.MaxValue, 0.7f);
            }
        }

        private unsafe bool BreakAtPoint(ref HkBreakOffPointInfo pt, ref HkArrayUInt32 brokenKeysOut)
        {
            pt.ContactPosition = this.ClusterToWorld(pt.ContactPoint.Position);
            IMyEntity entity = pt.CollidingBody.GetEntity(0);
            if ((entity.Physics != null) && (entity.Physics.RigidBody != null))
            {
                float num = this.CalculateSeparatingVelocity(this.RigidBody, pt.CollidingBody, ref pt.ContactPoint).Length();
                float separatingVelocity = num;
                bool flag1 = MyFakes.DEFORMATION_LOGGING;
                if (((num * Math.Min(this.IsStatic ? entity.Physics.Mass : this.Mass, entity.Physics.IsStatic ? this.Mass : entity.Physics.Mass)) <= 21000f) || (separatingVelocity <= 0f))
                {
                    return false;
                }
                float* singlePtr1 = (float*) ref pt.ContactPointDirection;
                singlePtr1[0] *= -1f;
                this.PerformDeformationOnGroup((VRage.Game.Entity.MyEntity) base.Entity, (VRage.Game.Entity.MyEntity) entity, ref pt, separatingVelocity);
                this.PerformDeformationOnGroup((VRage.Game.Entity.MyEntity) entity, (VRage.Game.Entity.MyEntity) base.Entity, ref pt, separatingVelocity);
            }
            return false;
        }

        private unsafe HkBreakOffLogicResult BreakLogicHandler(HkRigidBody otherBody, uint shapeKey, float* maxImpulse)
        {
            if (maxImpulse[0] == 0f)
            {
                maxImpulse[0] = this.Shape.BreakImpulse;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(this.m_grid, MySafeZoneAction.Damage, 0L) || (!MySession.Static.Settings.EnableVoxelDestruction && (otherBody.GetEntity(0) is MyVoxelBase)))
            {
                return HkBreakOffLogicResult.DoNotBreakOff;
            }
            HkBreakOffLogicResult useLimit = HkBreakOffLogicResult.UseLimit;
            if (!Sync.IsServer)
            {
                useLimit = HkBreakOffLogicResult.DoNotBreakOff;
            }
            else if (((this.RigidBody == null) || base.Entity.MarkedForClose) || (otherBody == null))
            {
                useLimit = HkBreakOffLogicResult.DoNotBreakOff;
            }
            else
            {
                IMyEntity entity = otherBody.GetEntity(0);
                if (entity == null)
                {
                    return HkBreakOffLogicResult.DoNotBreakOff;
                }
                if (((entity is Sandbox.Game.WorldEnvironment.MyEnvironmentSector) || (entity is MyFloatingObject)) || (entity is MyDebrisBase))
                {
                    useLimit = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (entity is MyCharacter)
                {
                    useLimit = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (ReferenceEquals(entity.GetTopMostParent(null), base.Entity))
                {
                    useLimit = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else
                {
                    MyCubeGrid nodeB = entity as MyCubeGrid;
                    if ((!MySession.Static.Settings.EnableSubgridDamage && (nodeB != null)) && MyCubeGridGroups.Static.Physical.HasSameGroup(this.m_grid, nodeB))
                    {
                        useLimit = HkBreakOffLogicResult.DoNotBreakOff;
                    }
                    else if ((base.Entity is MyCubeGrid) || (nodeB != null))
                    {
                        useLimit = HkBreakOffLogicResult.UseLimit;
                    }
                }
                if (base.WeldInfo.Children.Count > 0)
                {
                    base.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(this.RigidBody, this.Shape.BreakImpulse);
                }
            }
            bool flag1 = MyFakes.DEFORMATION_LOGGING;
            return useLimit;
        }

        private bool BreakPartsHandler(ref HkBreakOffPoints breakOffPoints, ref HkArrayUInt32 brokenKeysOut)
        {
            bool flag = false;
            if (!MySessionComponentSafeZones.IsActionAllowed(this.m_grid, MySafeZoneAction.Damage, 0L))
            {
                return flag;
            }
            for (int i = 0; i < breakOffPoints.Count; i++)
            {
                HkBreakOffPointInfo pt = breakOffPoints[i];
                flag |= this.BreakAtPoint(ref pt, ref brokenKeysOut);
            }
            return false;
        }

        private Vector3 CalculateSeparatingVelocity(HkRigidBody bodyA, HkRigidBody bodyB, ref HkContactPoint cp) => 
            this.CalculateSeparatingVelocity(bodyA, bodyB, cp.Position);

        private Vector3 CalculateSeparatingVelocity(HkRigidBody bodyA, HkRigidBody bodyB, Vector3 position)
        {
            Vector3 zero = Vector3.Zero;
            if (!bodyA.IsFixed)
            {
                Vector3 vector3 = position - bodyA.CenterOfMassWorld;
                zero = Vector3.Cross(bodyA.AngularVelocity, vector3);
                zero.Add(bodyA.LinearVelocity);
            }
            Vector3 vector2 = Vector3.Zero;
            if (!bodyB.IsFixed)
            {
                Vector3 vector4 = position - bodyB.CenterOfMassWorld;
                vector2 = Vector3.Cross(bodyB.AngularVelocity, vector4);
                vector2.Add(bodyB.LinearVelocity);
            }
            return (zero - vector2);
        }

        private Vector3 CalculateSeparatingVelocity(MyCubeGrid first, MyCubeGrid second, Vector3 position)
        {
            Vector3D vectord = this.ClusterToWorld(position);
            Vector3 zero = Vector3.Zero;
            if (!first.IsStatic && (first.Physics != null))
            {
                Vector3 vector3 = (Vector3) (vectord - first.Physics.CenterOfMassWorld);
                zero = Vector3.Cross(first.Physics.AngularVelocity, vector3);
                zero.Add(first.Physics.LinearVelocity);
            }
            Vector3 vector2 = Vector3.Zero;
            if (!second.IsStatic && (second.Physics != null))
            {
                Vector3 vector4 = (Vector3) (vectord - second.Physics.CenterOfMassWorld);
                vector2 = Vector3.Cross(second.Physics.AngularVelocity, vector4);
                vector2.Add(second.Physics.LinearVelocity);
            }
            return (zero - vector2);
        }

        private static float CalculateSoften(float softAreaPlanarInv, float softAreaVerticalInv, ref Vector3 normal, Vector3 contactToTarget)
        {
            float num;
            Vector3.Dot(ref normal, ref contactToTarget, out num);
            if (num < 0f)
            {
                num = -num;
            }
            float num2 = 1f - (num * softAreaVerticalInv);
            if (num2 <= 0f)
            {
                return 0f;
            }
            float num3 = contactToTarget.LengthSquared() - (num * num);
            if (num3 <= 0f)
            {
                return num2;
            }
            float num5 = 1f - (((float) Math.Sqrt((double) num3)) * softAreaPlanarInv);
            return ((num5 > 0f) ? (num2 * num5) : 0f);
        }

        public bool CheckLastDestroyedBlockFracturePieces()
        {
            if (Sync.IsServer && ((this.m_grid.BlocksCount == 1) && !this.m_grid.IsStatic))
            {
                MySlimBlock block = this.m_grid.GetBlocks().First<MySlimBlock>();
                if (block.FatBlock != null)
                {
                    MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock == null)
                    {
                        MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
                        if (fractureComponent != null)
                        {
                            bool flag3 = this.m_grid.EnableGenerators(false, false);
                            MyDestructionHelper.CreateFracturePiece(fractureComponent, true);
                            this.m_grid.RemoveBlock(block, true);
                            this.m_grid.EnableGenerators(flag3, false);
                        }
                        return (fractureComponent != null);
                    }
                    bool enable = this.m_grid.EnableGenerators(false, false);
                    bool flag2 = true;
                    List<MySlimBlock> list = new List<MySlimBlock>(fatBlock.GetBlocks());
                    foreach (MySlimBlock block3 in list)
                    {
                        flag2 = flag2 && block3.FatBlock.Components.Has<MyFractureComponentBase>();
                    }
                    if (flag2)
                    {
                        foreach (MySlimBlock block4 in list)
                        {
                            MyFractureComponentCubeBlock fractureComponent = block4.GetFractureComponent();
                            ushort? blockId = fatBlock.GetBlockId(block4);
                            if (fractureComponent != null)
                            {
                                MyDestructionHelper.CreateFracturePiece(fractureComponent, true);
                            }
                            this.m_grid.RemoveBlockWithId(block4.Position, new ushort?(blockId.Value), true);
                        }
                    }
                    this.m_grid.EnableGenerators(enable, false);
                    return flag2;
                }
            }
            return false;
        }

        public void ClearFractureBlockComponents()
        {
            this.m_fractureBlockComponentsCache.Clear();
        }

        public override void Close()
        {
            base.Close();
            if (this.m_planetCrash_Effect != null)
            {
                this.m_planetCrash_Effect.Stop(false);
            }
            foreach (CollisionParticleEffect effect in this.m_collisionParticles)
            {
                effect.RemainingTime = -1;
                this.FinalizeCollisionParticleEffect(ref effect);
            }
            if (this.m_shape != null)
            {
                this.m_shape.Dispose();
                this.m_shape = null;
            }
        }

        private float ComputeDirecionalSparkMultiplier(float speed)
        {
            float num = 1f;
            float num3 = speed / 110f;
            return ((num3 * 10f) + ((1f - num3) * num));
        }

        private float ComputeDirecionalSparkScale(float impulse) => 
            ((impulse >= 1000f) ? ((impulse >= 10000f) ? ((impulse >= 100000f) ? 2.8f : (1.45f + (1.5E-05f * (impulse - 10000f)))) : (1f + (5E-05f * (impulse - 1000f)))) : 1f);

        private static void ConnectPiecesInBlock(HkdBreakableShape parent, List<HkdShapeInstanceInfo> shapeList)
        {
            int num = 0;
            while (num < shapeList.Count)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= shapeList.Count)
                    {
                        num++;
                        break;
                    }
                    if (num != num2)
                    {
                        MyGridShape.ConnectShapesWithChildren(parent, shapeList[num].Shape, shapeList[num2].Shape);
                    }
                    num2++;
                }
            }
        }

        public void ConsiderDisablingTOIs()
        {
            if (MySession.Static.GameplayFrameCounter <= (this.m_lastTOIFrame + 20))
            {
                this.m_savedQuality = this.RigidBody.Quality;
                this.RigidBody.Quality = HkCollidableQualityType.Debris;
                DisableGridTOIsOptimizer.Static.Register(this);
            }
        }

        public void ConvertToDynamic(bool doubledKinematic, bool isPredicted)
        {
            if (((this.RigidBody != null) && ((base.Entity != null) && !base.Entity.Closed)) && (base.HavokWorld != null))
            {
                this.Flags = doubledKinematic ? RigidBodyFlag.RBF_DOUBLED_KINEMATIC : RigidBodyFlag.RBF_DEFAULT;
                bool flag = true;
                if (base.IsWelded || (base.WeldInfo.Children.Count > 0))
                {
                    if ((base.WeldedRigidBody != null) && (base.WeldedRigidBody.Quality == HkCollidableQualityType.Fixed))
                    {
                        base.WeldedRigidBody.UpdateMotionType(HkMotionType.Dynamic);
                        base.WeldedRigidBody.Quality = HkCollidableQualityType.Moving;
                        if (!doubledKinematic || MyPerGameSettings.Destruction)
                        {
                            base.WeldedRigidBody.Layer = 15;
                        }
                        else
                        {
                            base.WeldedRigidBody.Layer = 0x10;
                        }
                    }
                    MyWeldingGroups.Static.GetGroup((VRage.Game.Entity.MyEntity) base.Entity).GroupData.UpdateParent((base.WeldInfo.Parent != null) ? ((VRage.Game.Entity.MyEntity) base.WeldInfo.Parent.Entity) : ((VRage.Game.Entity.MyEntity) base.Entity));
                    flag &= ReferenceEquals(base.WeldInfo.Parent, null);
                }
                if (flag)
                {
                    int num1;
                    if (Sync.IsServer || isPredicted)
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = 5;
                    }
                    HkMotionType type = (HkMotionType) num1;
                    if (type != this.RigidBody.GetMotionType())
                    {
                        base.NotifyConstraintsRemovedFromWorld();
                        this.RigidBody.UpdateMotionType(type);
                        base.NotifyConstraintsAddedToWorld();
                    }
                    this.RigidBody.Quality = HkCollidableQualityType.Moving;
                    if (!doubledKinematic || MyPerGameSettings.Destruction)
                    {
                        base.Flags = RigidBodyFlag.RBF_DEFAULT;
                        this.RigidBody.Layer = 15;
                    }
                    else
                    {
                        base.Flags = RigidBodyFlag.RBF_DOUBLED_KINEMATIC;
                        this.RigidBody.Layer = 0x10;
                    }
                    this.UpdateContactCallbackLimit();
                    this.RigidBody.AddGravity();
                    this.ActivateCollision();
                    base.HavokWorld.RefreshCollisionFilterOnEntity(this.RigidBody);
                    this.RigidBody.Activate();
                    if (this.RigidBody.InWorld)
                    {
                        base.HavokWorld.RigidBodyActivated(this.RigidBody);
                        base.InvokeOnBodyActiveStateChanged(true);
                    }
                }
                this.UpdateMass();
            }
        }

        public void ConvertToStatic()
        {
            base.Flags = RigidBodyFlag.RBF_STATIC;
            bool flag = true;
            if (base.IsWelded || (base.WeldInfo.Children.Count > 0))
            {
                if ((base.WeldedRigidBody != null) && (base.WeldedRigidBody.Quality != HkCollidableQualityType.Fixed))
                {
                    base.WeldedRigidBody.UpdateMotionType(HkMotionType.Fixed);
                    base.WeldedRigidBody.Quality = HkCollidableQualityType.Fixed;
                    base.WeldedRigidBody.Layer = 13;
                }
                MyWeldingGroups.Static.GetGroup((VRage.Game.Entity.MyEntity) base.Entity).GroupData.UpdateParent((base.WeldInfo.Parent != null) ? ((VRage.Game.Entity.MyEntity) base.WeldInfo.Parent.Entity) : ((VRage.Game.Entity.MyEntity) base.Entity));
                flag &= ReferenceEquals(base.WeldInfo.Parent, null);
            }
            this.UpdateMass();
            if (flag)
            {
                bool isActive = this.RigidBody.IsActive;
                base.NotifyConstraintsRemovedFromWorld();
                this.RigidBody.UpdateMotionType(HkMotionType.Fixed);
                this.RigidBody.Quality = HkCollidableQualityType.Fixed;
                this.RigidBody.Layer = 13;
                base.NotifyConstraintsAddedToWorld();
                this.ActivateCollision();
                base.HavokWorld.RefreshCollisionFilterOnEntity(this.RigidBody);
                this.RigidBody.Activate();
                if (this.RigidBody.InWorld)
                {
                    if (isActive)
                    {
                        base.InvokeOnBodyActiveStateChanged(false);
                    }
                    base.HavokWorld.RigidBodyActivated(this.RigidBody);
                }
                HkGroupFilter.GetSystemGroupFromFilterInfo(this.RigidBody.GetCollisionFilterInfo());
            }
            if (this.RigidBody2 != null)
            {
                if (this.RigidBody2.InWorld)
                {
                    base.HavokWorld.RemoveRigidBody(this.RigidBody2);
                }
                this.RigidBody2.Dispose();
            }
            this.RigidBody2 = null;
        }

        private void CreateBody()
        {
            if (this.m_shape == null)
            {
                this.m_shape = new MyGridShape(this.m_grid);
            }
            if ((this.m_grid.GridSizeEnum == MyCubeSize.Large) && !this.IsStatic)
            {
                base.InitialSolverDeactivation = HkSolverDeactivation.Off;
            }
            base.ContactPointDelay = 0;
            this.CreateFromCollisionObject((HkShape) this.m_shape, Vector3.Zero, MatrixD.Identity, this.m_shape.MassProperties, 15);
            this.RigidBody.ContactPointCallbackEnabled = true;
            this.RigidBody.ContactSoundCallbackEnabled = true;
            if (MyPerGameSettings.Destruction)
            {
                this.RigidBody.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback_Destruction);
                this.BreakableBody.BeforeControllerOperation += new BeforeBodyControllerOperation(this.BreakableBody_BeforeControllerOperation);
                this.BreakableBody.AfterControllerOperation += new AfterBodyControllerOperation(this.BreakableBody_AfterControllerOperation);
            }
            else
            {
                this.RigidBody.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                if (!Sync.IsServer)
                {
                    this.RigidBody.CollisionAddedCallback += new CollisionEventHandler(this.RigidBody_CollisionAddedCallbackClient);
                    this.RigidBody.CollisionRemovedCallback += new CollisionEventHandler(this.RigidBody_CollisionRemovedCallbackClient);
                }
            }
            this.RigidBody.LinearDamping = MyPerGameSettings.DefaultLinearDamping;
            this.RigidBody.AngularDamping = MyPerGameSettings.DefaultAngularDamping;
            if (this.m_grid.BlocksDestructionEnabled)
            {
                this.RigidBody.BreakLogicHandler = new Havok.BreakLogicHandler(this.BreakLogicHandler);
                this.RigidBody.BreakPartsHandler = new Havok.BreakPartsHandler(this.BreakPartsHandler);
            }
            if (this.RigidBody2 != null)
            {
                this.RigidBody2.ContactPointCallbackEnabled = true;
                if (!MyPerGameSettings.Destruction)
                {
                    this.RigidBody2.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                }
                if (this.m_grid.BlocksDestructionEnabled)
                {
                    this.RigidBody2.BreakPartsHandler = new Havok.BreakPartsHandler(this.BreakPartsHandler);
                    this.RigidBody2.BreakLogicHandler = new Havok.BreakLogicHandler(this.BreakLogicHandler);
                }
            }
            RigidBodyFlag flags = GetFlags(this.m_grid);
            this.SetDefaultRigidBodyMaxVelocities();
            if (this.IsStatic)
            {
                this.RigidBody.Layer = 13;
            }
            else if (this.m_grid.GridSizeEnum != MyCubeSize.Large)
            {
                if (this.m_grid.GridSizeEnum == MyCubeSize.Small)
                {
                    this.RigidBody.Layer = 15;
                }
            }
            else
            {
                sbyte num1;
                if ((flags != RigidBodyFlag.RBF_DOUBLED_KINEMATIC) || !MyFakes.ENABLE_DOUBLED_KINEMATIC)
                {
                    num1 = 15;
                }
                else
                {
                    num1 = 0x10;
                }
                this.RigidBody.Layer = num1;
            }
            if (this.RigidBody2 != null)
            {
                this.RigidBody2.Layer = 0x11;
            }
            if (MyPerGameSettings.BallFriendlyPhysics)
            {
                this.RigidBody.Restitution = 0f;
                if (this.RigidBody2 != null)
                {
                    this.RigidBody2.Restitution = 0f;
                }
            }
            this.Enabled = true;
        }

        protected override void CreateBody(ref HkShape shape, HkMassProperties? massProperties)
        {
            if (MyPerGameSettings.Destruction)
            {
                shape = this.CreateBreakableBody(shape, massProperties);
            }
            else
            {
                HkRigidBodyCinfo rbInfo = new HkRigidBodyCinfo {
                    AngularDamping = base.m_angularDamping,
                    LinearDamping = base.m_linearDamping,
                    Shape = shape,
                    SolverDeactivation = base.InitialSolverDeactivation,
                    ContactPointCallbackDelay = base.ContactPointDelay
                };
                if (massProperties != null)
                {
                    rbInfo.SetMassProperties(massProperties.Value);
                }
                GetInfoFromFlags(rbInfo, base.Flags);
                if (this.m_grid.IsStatic)
                {
                    rbInfo.MotionType = HkMotionType.Dynamic;
                    rbInfo.QualityType = HkCollidableQualityType.Moving;
                }
                this.RigidBody = new HkRigidBody(rbInfo);
                if (this.m_grid.IsStatic)
                {
                    this.RigidBody.UpdateMotionType(HkMotionType.Fixed);
                }
            }
        }

        private HkShape CreateBreakableBody(HkShape shape, HkMassProperties? massProperties)
        {
            HkMassProperties local1;
            if (massProperties != null)
            {
                local1 = massProperties.Value;
            }
            else
            {
                local1 = new HkMassProperties();
            }
            HkMassProperties properties = local1;
            if (!this.Shape.BreakableShape.IsValid())
            {
                this.Shape.CreateBreakableShape();
            }
            HkdBreakableShape breakableShape = this.Shape.BreakableShape;
            if (breakableShape.IsValid())
            {
                breakableShape.BuildMassProperties(ref properties);
            }
            else
            {
                breakableShape = new HkdBreakableShape(shape);
                if (massProperties == null)
                {
                    breakableShape.SetMassRecursively(50f);
                }
                else
                {
                    HkMassProperties properties3 = massProperties.Value;
                    breakableShape.SetMassProperties(ref properties3);
                }
            }
            shape = breakableShape.GetShape();
            HkRigidBodyCinfo rbInfo = new HkRigidBodyCinfo {
                AngularDamping = base.m_angularDamping,
                LinearDamping = base.m_linearDamping,
                SolverDeactivation = this.m_grid.IsStatic ? base.InitialSolverDeactivation : HkSolverDeactivation.Low,
                ContactPointCallbackDelay = base.ContactPointDelay,
                Shape = shape
            };
            rbInfo.SetMassProperties(properties);
            GetInfoFromFlags(rbInfo, base.Flags);
            if (this.m_grid.IsStatic)
            {
                rbInfo.MotionType = HkMotionType.Dynamic;
                rbInfo.QualityType = HkCollidableQualityType.Moving;
            }
            HkRigidBody body = new HkRigidBody(rbInfo);
            if (this.m_grid.IsStatic)
            {
                body.UpdateMotionType(HkMotionType.Fixed);
            }
            body.EnableDeactivation = true;
            this.BreakableBody = new HkdBreakableBody(breakableShape, body, null, Matrix.Identity);
            this.BreakableBody.AfterReplaceBody += new BreakableBodyReplaced(this.FracturedBody_AfterReplaceBody);
            return shape;
        }

        public static void CreateDestructionEffect(string effectName, Vector3D position, Vector3 direction, float scale)
        {
            MyParticleEffect effect;
            MatrixD xd = MatrixD.CreateFromDir(direction);
            if (MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.CreateWorld(position, xd.Forward, xd.Up), out effect))
            {
                effect.UserScale = scale;
            }
        }

        private void CreateEffect(GridEffect e)
        {
            MyParticleEffect effect;
            Vector3D position = e.Position;
            Vector3 normal = e.Normal;
            float scale = e.Scale;
            switch (e.Type)
            {
                case GridEffectType.Collision:
                {
                    float num2 = (float) Vector3D.DistanceSquared(MySector.MainCamera.Position, position);
                    scale = MyPerGameSettings.CollisionParticle.Scale;
                    float collisionSparkMultiplier = GetCollisionSparkMultiplier(e.SeparatingSpeed, this.m_grid.GridSizeEnum == MyCubeSize.Large);
                    float num4 = 0.5f * GetCollisionSparkScale(e.Impulse, this.m_grid.GridSizeEnum == MyCubeSize.Large);
                    string effectName = (this.m_grid.GridSizeEnum != MyCubeSize.Large) ? MyPerGameSettings.CollisionParticle.SmallGridClose : ((num2 > MyPerGameSettings.CollisionParticle.CloseDistanceSq) ? MyPerGameSettings.CollisionParticle.LargeGridDistant : MyPerGameSettings.CollisionParticle.LargeGridClose);
                    MatrixD xd = MatrixD.CreateFromDir(normal);
                    if (!MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.CreateWorld(position, xd.Forward, xd.Up), out effect))
                    {
                        break;
                    }
                    effect.UserScale = scale * num4;
                    effect.UserBirthMultiplier = collisionSparkMultiplier;
                    return;
                }
                case GridEffectType.Destruction:
                    scale = MyPerGameSettings.DestructionParticle.Scale;
                    if (this.m_grid.GridSizeEnum != MyCubeSize.Large)
                    {
                        scale = 0.05f;
                    }
                    MySyncDestructions.AddDestructionEffect(MyPerGameSettings.DestructionParticle.DestructionSmokeLarge, position, normal, scale);
                    return;

                case GridEffectType.Dust:
                    if (MyParticlesManager.TryCreateParticleEffect("PlanetCrashDust", MatrixD.CreateTranslation(position), out effect))
                    {
                        effect.UserScale = scale;
                    }
                    break;

                default:
                    return;
            }
        }

        private void CreateGridCollisionEffect(GridCollisionhit e)
        {
            double num = 0.5;
            for (int i = 0; i < this.m_collisionParticles.Count; i++)
            {
                CollisionParticleEffect effect = this.m_collisionParticles[i];
                if (Vector3D.DistanceSquared(effect.RelativePosition, e.RelativePosition) < num)
                {
                    if ((effect.RemainingTime < 20) || (effect.Impulse < e.Impulse))
                    {
                        effect.RelativePosition = e.RelativePosition;
                        effect.SeparatingVelocity = e.RelativeVelocity;
                        effect.Normal = e.Normal;
                        effect.RemainingTime = 20;
                        effect.Impulse = e.Impulse;
                    }
                    return;
                }
            }
            if (this.m_collisionParticles.Count < 3)
            {
                CollisionParticleEffect item = new CollisionParticleEffect();
                item.Effect = null;
                item.RemainingTime = 20;
                item.Normal = e.Normal;
                item.SeparatingVelocity = e.RelativeVelocity;
                item.RelativePosition = e.RelativePosition;
                item.Impulse = e.Impulse;
                this.m_collisionParticles.Add(item);
                this.m_grid.MarkForUpdate();
            }
        }

        public override void Deactivate(object world)
        {
            this.DisableTOIOptimization();
            this.UnmarkBreakable((HkWorld) world);
            base.Deactivate(world);
        }

        public override void DeactivateBatch(object world)
        {
            this.UnmarkBreakable((HkWorld) world);
            base.DeactivateBatch(world);
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.BREAKABLE_SHAPE_CONNECTIONS && (this.BreakableBody != null))
            {
                MySlimBlock cubeBlock = null;
                List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                MyPhysics.CastRay(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 25f), toList, 30);
                foreach (MyPhysics.HitInfo info in toList)
                {
                    if (info.HkHitInfo.GetHitEntity() is MyCubeGrid)
                    {
                        MyCubeGrid hitEntity = info.HkHitInfo.GetHitEntity() as MyCubeGrid;
                        cubeBlock = hitEntity.GetCubeBlock(hitEntity.WorldToGridInteger(info.Position + (MySector.MainCamera.ForwardVector * 0.2f)));
                        break;
                    }
                }
                int num = 0;
                List<HkdConnection> resultList = new List<HkdConnection>();
                this.BreakableBody.BreakableShape.GetConnectionList(resultList);
                foreach (HkdConnection connection in resultList)
                {
                    Vector3D coords = this.ClusterToWorld(Vector3.Transform(connection.PivotA, this.RigidBody.GetRigidBodyMatrix()));
                    Vector3D pointTo = this.ClusterToWorld(Vector3.Transform(connection.PivotB, this.RigidBody.GetRigidBodyMatrix()));
                    if ((cubeBlock != null) && (cubeBlock.CubeGrid.WorldToGridInteger(coords) == cubeBlock.Position))
                    {
                        coords += (pointTo - coords) * 0.05000000074505806;
                        MyRenderProxy.DebugDrawLine3D(coords, pointTo, Color.Red, Color.Blue, false, false);
                        MyRenderProxy.DebugDrawSphere(pointTo, 0.075f, Color.White, 1f, false, false, true, false);
                    }
                    if ((cubeBlock != null) && (cubeBlock.CubeGrid.WorldToGridInteger(pointTo) == cubeBlock.Position))
                    {
                        pointTo += Vector3.One * 0.02f;
                        MyRenderProxy.DebugDrawLine3D(coords + (Vector3.One * 0.02f), pointTo, Color.Red, Color.Green, false, false);
                        MyRenderProxy.DebugDrawSphere(pointTo, 0.025f, Color.Green, 1f, false, false, true, false);
                    }
                    if (num > 0x3e8)
                    {
                        break;
                    }
                }
            }
            this.Shape.DebugDraw();
            base.DebugDraw();
        }

        private unsafe void DeformBones(float deformationOffset, Vector3I gridPos, float softAreaPlanar, float softAreaVertical, Vector3 localNormal, Vector3 localPos, MyStringHash damageType, float offsetThreshold, float lowerRatioLimit, long attackerId, Vector3 up)
        {
            Vector3I vectori5;
            MySlimBlock existingCubeForBoneDeformations;
            bool flag2;
            int num8;
            if (!MySession.Static.Ready)
            {
                return;
            }
            if (m_tmpCubeList == null)
            {
                m_tmpCubeList = new List<Vector3I>(8);
            }
            float softAreaVerticalInv = 1f / softAreaVertical;
            float softAreaPlanarInv = 1f / softAreaPlanar;
            float num3 = 1f / this.m_grid.GridSize;
            float num4 = this.m_grid.GridSize * 0.5f;
            Vector3I vectori = Vector3I.Round((localPos + new Vector3(this.m_grid.GridSizeHalf)) / num4) - (gridPos * 2);
            float x = (softAreaPlanar * num3) * 2f;
            BoundingBox aABB = new MyOrientedBoundingBox((gridPos * 2) + vectori, new Vector3(x, x, (softAreaVertical * num3) * 2f), Quaternion.CreateFromForwardUp(localNormal, up)).GetAABB();
            Vector3I vectori2 = Vector3I.Max(Vector3I.Floor((Vector3) ((Vector3I.Floor(aABB.Min) - Vector3I.One) * 0.5f)), this.m_grid.Min);
            Vector3I vectori3 = Vector3I.Min(Vector3I.Ceiling((Vector3) ((Vector3I.Ceiling(aABB.Max) - Vector3I.One) * 0.5f)), this.m_grid.Max);
            bool isServer = Sync.IsServer;
            Vector3I vectori4 = gridPos * 2;
            Vector3 vector = new Vector3(this.m_grid.GridSize * 0.25f);
            float num7 = this.m_grid.GridSize * 0.7f;
            float single1 = offsetThreshold;
            offsetThreshold = single1 / ((this.m_grid.GridSizeEnum == MyCubeSize.Large) ? ((float) 1) : ((float) 5));
            bool flag = false;
            HashSet<Vector3I> dirtyCubes = m_dirtyCubesPool.Get();
            Dictionary<MySlimBlock, float> damagedCubes = m_damagedCubesPool.Get();
            MyDamageInformation damageInfo = new MyDamageInformation(true, 1f, MyDamageType.Deformation, attackerId);
            vectori2 = Vector3I.Max(vectori2, this.m_grid.Min);
            vectori3 = Vector3I.Min(vectori3, this.m_grid.Max);
            vectori5.X = vectori2.X;
            goto TR_0048;
        TR_0006:
            int* numPtr1 = (int*) ref vectori5.Z;
            numPtr1[0]++;
            goto TR_0042;
        TR_0007:
            num8++;
        TR_003B:
            while (true)
            {
                Vector3 vector3;
                if (num8 >= MyGridSkeleton.BoneOffsets.Length)
                {
                    break;
                }
                Vector3I vectori6 = MyGridSkeleton.BoneOffsets[num8];
                Vector3I pos = (Vector3I) ((vectori5 * 2) + vectori6);
                Vector3 vector2 = ((Vector3) ((pos * this.m_grid.GridSize) * 0.5f)) - vector;
                this.m_grid.Skeleton.GetBone(ref pos, out vector3);
                float num9 = CalculateSoften(softAreaPlanarInv, softAreaVerticalInv, ref localNormal, (vector3 + vector2) - localPos);
                if (num9 != 0f)
                {
                    flag2 = false;
                    float num10 = Math.Max(lowerRatioLimit, existingCubeForBoneDeformations.DeformationRatio);
                    if ((deformationOffset * num10) >= offsetThreshold)
                    {
                        int num1;
                        float num11 = deformationOffset * num9;
                        float num12 = num11 * num10;
                        vector3 += localNormal * num12;
                        float num13 = vector3.AbsMax();
                        if ((damageType == MyDamageType.Bullet) || (damageType == MyDamageType.Drill))
                        {
                            num1 = (int) (num13 < num7);
                        }
                        else
                        {
                            num1 = 1;
                        }
                        if ((num1 != 0) && (num12 > 0.05f))
                        {
                            Vector3I boneOffset = pos - vectori4;
                            if (num13 > num7)
                            {
                                m_tmpCubeList.Clear();
                                Vector3I vectori9 = boneOffset;
                                Vector3I cube = gridPos;
                                this.m_grid.Skeleton.Wrap(ref cube, ref vectori9);
                                this.m_grid.Skeleton.GetAffectedCubes(cube, vectori9, m_tmpCubeList, this.m_grid);
                                bool flag3 = true;
                                foreach (Vector3I vectori11 in m_tmpCubeList)
                                {
                                    MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori11);
                                    if ((cubeBlock != null) && !cubeBlock.IsDestroyed)
                                    {
                                        flag3 &= ((num11 * Math.Max(lowerRatioLimit, cubeBlock.DeformationRatio)) > num7) && cubeBlock.UsesDeformation;
                                    }
                                }
                                if (!flag3)
                                {
                                    goto TR_0007;
                                }
                                else
                                {
                                    foreach (Vector3I vectori12 in m_tmpCubeList)
                                    {
                                        MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori12);
                                        if ((cubeBlock != null) && !cubeBlock.IsDestroyed)
                                        {
                                            bool flag1 = MyFakes.DEFORMATION_LOGGING;
                                            if (!isServer)
                                            {
                                                this.AddDirtyBlock(cubeBlock);
                                                continue;
                                            }
                                            flag = true;
                                            this.m_removedCubes.TryAdd(cubeBlock, 0);
                                            damagedCubes.Remove(cubeBlock);
                                        }
                                    }
                                    goto TR_0007;
                                }
                            }
                            dirtyCubes.Add(vectori5);
                            if (isServer)
                            {
                                this.m_grid.Skeleton.SetBone(ref pos, ref vector3);
                                this.m_grid.AddDirtyBone(gridPos, boneOffset);
                                MyVoxelSegmentation bonesToSend = this.m_grid.BonesToSend;
                                lock (bonesToSend)
                                {
                                    this.m_grid.BonesToSend.AddInput(pos);
                                }
                                if (damageType != MyDamageType.Bullet)
                                {
                                    float num15 = 1f - (num13 / num7);
                                    if (num15 < (((IMyDestroyableObject) existingCubeForBoneDeformations).Integrity / existingCubeForBoneDeformations.MaxIntegrity))
                                    {
                                        float num16 = (existingCubeForBoneDeformations.MaxIntegrity * (1f - num15)) - existingCubeForBoneDeformations.CurrentDamage;
                                        if (num16 > 0f)
                                        {
                                            float num17;
                                            damagedCubes.TryGetValue(existingCubeForBoneDeformations, out num17);
                                            damagedCubes[existingCubeForBoneDeformations] = Math.Max(num16, num17);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (flag2 && (num8 >= 7))
                {
                    break;
                }
                goto TR_0007;
            }
            goto TR_0006;
        TR_0042:
            while (true)
            {
                if (vectori5.Z <= vectori3.Z)
                {
                    existingCubeForBoneDeformations = this.m_grid.GetExistingCubeForBoneDeformations(ref vectori5, ref damageInfo);
                    if (existingCubeForBoneDeformations == null)
                    {
                        goto TR_0006;
                    }
                    else if (existingCubeForBoneDeformations.IsDestroyed)
                    {
                        goto TR_0006;
                    }
                    else if (this.m_removedCubes.ContainsKey(existingCubeForBoneDeformations))
                    {
                        goto TR_0006;
                    }
                    else
                    {
                        flag2 = true;
                        num8 = 0;
                    }
                    goto TR_003B;
                }
                else
                {
                    int* numPtr2 = (int*) ref vectori5.Y;
                    numPtr2[0]++;
                }
                break;
            }
        TR_0045:
            while (true)
            {
                if (vectori5.Y > vectori3.Y)
                {
                    int* numPtr3 = (int*) ref vectori5.X;
                    numPtr3[0]++;
                    break;
                }
                vectori5.Z = vectori2.Z;
                goto TR_0042;
            }
        TR_0048:
            while (true)
            {
                if (vectori5.X <= vectori3.X)
                {
                    vectori5.Y = vectori2.Y;
                    break;
                }
                if (flag)
                {
                    this.ScheduleRemoveBlocksCallbacks();
                }
                MySandboxGame.Static.Invoke(delegate {
                    bool flag = true;
                    if (MySession.Static.Ready)
                    {
                        this.m_dirtyCubesInfo.DirtyBlocks.UnionWith(dirtyCubes);
                        this.m_grid.MarkForUpdate();
                        foreach (KeyValuePair<MySlimBlock, float> pair in damagedCubes)
                        {
                            if (pair.Key.IsDestroyed)
                            {
                                continue;
                            }
                            if (!pair.Key.CubeGrid.Closed)
                            {
                                MyHitInfo? hitInfo = null;
                                pair.Key.DoDamage(pair.Value, damageType, false, hitInfo, attackerId);
                            }
                        }
                        if (isServer)
                        {
                            Action <>9__1;
                            flag = false;
                            Action action = <>9__1;
                            if (<>9__1 == null)
                            {
                                Action local1 = <>9__1;
                                action = <>9__1 = delegate {
                                    MySlimBlock.SendDamageBatch(damagedCubes, damageType, attackerId);
                                    m_damagedCubesPool.Return(damagedCubes);
                                };
                            }
                            Parallel.Start(action);
                        }
                    }
                    m_dirtyCubesPool.Return(dirtyCubes);
                    if (flag)
                    {
                        m_damagedCubesPool.Return(damagedCubes);
                    }
                }, "DeformBones");
                return;
            }
            goto TR_0045;
        }

        public void DisableTOIOptimization()
        {
            if (this.IsTOIOptimized)
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.Quality = this.m_savedQuality;
                    this.m_savedQuality = HkCollidableQualityType.Invalid;
                }
                DisableGridTOIsOptimizer.Static.Unregister(this);
            }
        }

        private static void DisconnectBlock(MySlimBlock a)
        {
            a.DisconnectFaces.Add(Vector3I.Left);
            a.DisconnectFaces.Add(Vector3I.Right);
            a.DisconnectFaces.Add(Vector3I.Forward);
            a.DisconnectFaces.Add(Vector3I.Backward);
            a.DisconnectFaces.Add(Vector3I.Up);
            a.DisconnectFaces.Add(Vector3I.Down);
        }

        private void FinalizeCollisionParticleEffect(ref CollisionParticleEffect effect)
        {
            if (effect.Effect != null)
            {
                effect.Effect.Stop(false);
            }
        }

        private void FindFractureComponentBlocks()
        {
            foreach (KeyValuePair<MySlimBlock, List<HkdShapeInstanceInfo>> pair in this.m_fracturedSlimBlocksShapes)
            {
                MySlimBlock key = pair.Key;
                List<HkdShapeInstanceInfo> childShapes = pair.Value;
                if (!key.FatBlock.Components.Has<MyFractureComponentBase>())
                {
                    int totalBreakableShapeChildrenCount = key.GetTotalBreakableShapeChildrenCount();
                    if (!key.BlockDefinition.CreateFracturedPieces || (totalBreakableShapeChildrenCount != childShapes.Count))
                    {
                        foreach (HkdShapeInstanceInfo info2 in childShapes)
                        {
                            info2.SetTransform(ref Matrix.Identity);
                        }
                        HkdBreakableShape? oldParent = null;
                        HkdBreakableShape shape = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, childShapes);
                        shape.RecalcMassPropsFromChildren();
                        HkMassProperties massProperties = new HkMassProperties();
                        shape.BuildMassProperties(ref massProperties);
                        HkdBreakableShape parent = shape;
                        parent = new HkdBreakableShape(shape.GetShape(), ref massProperties);
                        foreach (HkdShapeInstanceInfo info3 in childShapes)
                        {
                            parent.AddShape(ref info3);
                        }
                        shape.RemoveReference();
                        ConnectPiecesInBlock(parent, childShapes);
                        MyFractureComponentBase.Info item = new MyFractureComponentBase.Info {
                            Entity = key.FatBlock,
                            Shape = parent,
                            Compound = true
                        };
                        this.m_fractureBlockComponentsCache.Add(item);
                    }
                }
            }
            this.m_fracturedSlimBlocksShapes.Clear();
        }

        private bool FindFractureComponentBlocks(MySlimBlock block, HkdShapeInstanceInfo shapeInst)
        {
            HkdBreakableShape shape = shapeInst.Shape;
            if (IsBreakableShapeCompound(shape))
            {
                bool flag = false;
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                shape.GetChildren(list);
                foreach (HkdShapeInstanceInfo info in list)
                {
                    flag |= this.FindFractureComponentBlocks(block, info);
                }
                return flag;
            }
            ushort? nullable = null;
            if (shape.HasProperty(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID))
            {
                HkSimpleValueProperty property = shape.GetProperty(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID);
                nullable = new ushort?((ushort) property.ValueUInt);
            }
            MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
            if (fatBlock != null)
            {
                if (nullable == null)
                {
                    return false;
                }
                MySlimBlock block3 = fatBlock.GetBlock(nullable.Value);
                if (block3 == null)
                {
                    return false;
                }
                block = block3;
            }
            if (!this.m_fracturedSlimBlocksShapes.ContainsKey(block))
            {
                this.m_fracturedSlimBlocksShapes[block] = new List<HkdShapeInstanceInfo>();
            }
            this.m_fracturedSlimBlocksShapes[block].Add(shapeInst);
            return true;
        }

        private void FindFracturedBlocks(HkdBreakableBodyInfo b)
        {
            HkdBreakableBodyHelper helper = new HkdBreakableBodyHelper(b);
            helper.GetRigidBodyMatrix();
            helper.GetChildren(this.m_children);
            foreach (HkdShapeInstanceInfo info in this.m_children)
            {
                if (!info.IsFracturePiece())
                {
                    continue;
                }
                Vector3I pos = info.Shape.GetProperty(HkdBreakableShape.PROPERTY_GRID_POSITION).Value;
                if (this.m_grid.CubeExists(pos))
                {
                    if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
                    {
                        if (!this.m_fracturedBlocksShapes.ContainsKey(pos))
                        {
                            this.m_fracturedBlocksShapes[pos] = new List<HkdShapeInstanceInfo>();
                        }
                        this.m_fracturedBlocksShapes[pos].Add(info);
                        continue;
                    }
                    MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(pos);
                    if ((cubeBlock != null) && !this.FindFractureComponentBlocks(cubeBlock, info))
                    {
                    }
                }
            }
            if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
            {
                using (Dictionary<Vector3I, List<HkdShapeInstanceInfo>>.KeyCollection.Enumerator enumerator2 = this.m_fracturedBlocksShapes.Keys.GetEnumerator())
                {
                    while (true)
                    {
                        MyFracturedBlock.Info info2;
                        while (true)
                        {
                            if (enumerator2.MoveNext())
                            {
                                Vector3I current = enumerator2.Current;
                                List<HkdShapeInstanceInfo> childShapes = this.m_fracturedBlocksShapes[current];
                                foreach (HkdShapeInstanceInfo info3 in childShapes)
                                {
                                    Matrix transform = info3.GetTransform();
                                    transform.Translation = Vector3.Zero;
                                    info3.SetTransform(ref transform);
                                }
                                HkdBreakableShape? oldParent = null;
                                HkdBreakableShape shape3 = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, childShapes);
                                shape3.RecalcMassPropsFromChildren();
                                HkMassProperties massProperties = new HkMassProperties();
                                shape3.BuildMassProperties(ref massProperties);
                                HkdBreakableShape parent = shape3;
                                parent = new HkdBreakableShape(shape3.GetShape(), ref massProperties);
                                foreach (HkdShapeInstanceInfo info4 in childShapes)
                                {
                                    parent.AddShape(ref info4);
                                }
                                shape3.RemoveReference();
                                ConnectPiecesInBlock(parent, childShapes);
                                info2 = new MyFracturedBlock.Info {
                                    Shape = parent,
                                    Position = current,
                                    Compound = true
                                };
                                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(current);
                                if (cubeBlock == null)
                                {
                                    parent.RemoveReference();
                                    continue;
                                }
                                if (cubeBlock.FatBlock is MyFracturedBlock)
                                {
                                    MyFracturedBlock fatBlock = cubeBlock.FatBlock as MyFracturedBlock;
                                    info2.OriginalBlocks = fatBlock.OriginalBlocks;
                                    info2.Orientations = fatBlock.Orientations;
                                    info2.MultiBlocks = fatBlock.MultiBlocks;
                                }
                                else
                                {
                                    if (cubeBlock.FatBlock is MyCompoundCubeBlock)
                                    {
                                        info2.OriginalBlocks = new List<MyDefinitionId>();
                                        info2.Orientations = new List<MyBlockOrientation>();
                                        bool flag = false;
                                        ListReader<MySlimBlock> blocks = (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks();
                                        foreach (MySlimBlock block4 in blocks)
                                        {
                                            info2.OriginalBlocks.Add(block4.BlockDefinition.Id);
                                            info2.Orientations.Add(block4.Orientation);
                                            flag = flag || block4.IsMultiBlockPart;
                                        }
                                        if (flag)
                                        {
                                            info2.MultiBlocks = new List<MyFracturedBlock.MultiBlockPartInfo>();
                                            foreach (MySlimBlock block5 in blocks)
                                            {
                                                if (!block5.IsMultiBlockPart)
                                                {
                                                    info2.MultiBlocks.Add(null);
                                                    continue;
                                                }
                                                MyFracturedBlock.MultiBlockPartInfo item = new MyFracturedBlock.MultiBlockPartInfo();
                                                item.MultiBlockDefinition = block5.MultiBlockDefinition.Id;
                                                item.MultiBlockId = block5.MultiBlockId;
                                                info2.MultiBlocks.Add(item);
                                            }
                                        }
                                        break;
                                    }
                                    info2.OriginalBlocks = new List<MyDefinitionId>();
                                    info2.Orientations = new List<MyBlockOrientation>();
                                    info2.OriginalBlocks.Add(cubeBlock.BlockDefinition.Id);
                                    info2.Orientations.Add(cubeBlock.Orientation);
                                    if (cubeBlock.IsMultiBlockPart)
                                    {
                                        info2.MultiBlocks = new List<MyFracturedBlock.MultiBlockPartInfo>();
                                        MyFracturedBlock.MultiBlockPartInfo item = new MyFracturedBlock.MultiBlockPartInfo();
                                        item.MultiBlockDefinition = cubeBlock.MultiBlockDefinition.Id;
                                        item.MultiBlockId = cubeBlock.MultiBlockId;
                                        info2.MultiBlocks.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                goto TR_0000;
                            }
                            break;
                        }
                        this.m_fractureBlocksCache.Add(info2);
                    }
                }
            }
        TR_0000:
            this.m_fracturedBlocksShapes.Clear();
            this.m_children.Clear();
        }

        public override unsafe void FracturedBody_AfterReplaceBody(ref HkdReplaceBodyEvent e)
        {
            if (!MyFakes.ENABLE_AFTER_REPLACE_BODY)
            {
                return;
            }
            if (!Sync.IsServer)
            {
                return;
            }
            if (this.m_recreateBody)
            {
                return;
            }
            base.HavokWorld.DestructionWorld.RemoveBreakableBody(e.OldBody);
            this.m_oldLinVel = this.RigidBody.LinearVelocity;
            this.m_oldAngVel = this.RigidBody.AngularVelocity;
            MyPhysics.RemoveDestructions(this.RigidBody);
            e.GetNewBodies(this.m_newBodies);
            if (this.m_newBodies.Count == 0)
            {
                return;
            }
            bool flag = false;
            m_tmpBlocksToDelete.Clear();
            m_tmpBlocksUpdateDamage.Clear();
            MySlimBlock b = null;
            using (List<HkdBreakableBodyInfo>.Enumerator enumerator = this.m_newBodies.GetEnumerator())
            {
                HkdBreakableBodyInfo current;
                HkdBreakableBody breakableBody;
                MySlimBlock cubeBlock;
                bool flag3;
                MyCompoundCubeBlock block3;
                bool flag4;
                MatrixD* xdPtr1;
                goto TR_006F;
            TR_0037:
                this.m_newBreakableBodies.Add(MyFracturedPiecesManager.Static.GetBreakableBody(current));
                this.FindFracturedBlocks(current);
                base.HavokWorld.DestructionWorld.RemoveBreakableBody(current);
                goto TR_006F;
            TR_0039:
                if (flag3)
                {
                    base.HavokWorld.DestructionWorld.RemoveBreakableBody(current);
                    MyFracturedPiecesManager.Static.ReturnToPool(breakableBody);
                }
            TR_006F:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        if (!current.IsFracture())
                        {
                            goto TR_0037;
                        }
                        else
                        {
                            if ((!MyFakes.ENABLE_FRACTURE_COMPONENT || ((this.m_grid.BlocksCount != 1) || !this.m_grid.IsStatic)) || !MyDestructionHelper.IsFixed(current))
                            {
                                breakableBody = MyFracturedPiecesManager.Static.GetBreakableBody(current);
                                Matrix rigidBodyMatrix = breakableBody.GetRigidBody().GetRigidBodyMatrix();
                                Vector3D vectord = this.ClusterToWorld(rigidBodyMatrix.Translation);
                                HkdBreakableShape breakableShape = breakableBody.BreakableShape;
                                HkVec3IProperty property = breakableShape.GetProperty(HkdBreakableShape.PROPERTY_GRID_POSITION);
                                if (!property.IsValid() && breakableShape.IsCompound())
                                {
                                    HkdBreakableShape shape = breakableShape.GetChild(0).Shape;
                                    property = shape.GetProperty(HkdBreakableShape.PROPERTY_GRID_POSITION);
                                }
                                flag3 = false;
                                cubeBlock = this.m_grid.GetCubeBlock(property.Value);
                                block3 = (cubeBlock != null) ? (cubeBlock.FatBlock as MyCompoundCubeBlock) : null;
                                if (cubeBlock == null)
                                {
                                    flag3 = true;
                                    goto TR_0039;
                                }
                                else
                                {
                                    if (b == null)
                                    {
                                        b = cubeBlock;
                                    }
                                    if (!flag)
                                    {
                                        this.AddDestructionEffect(this.m_grid.GridIntegerToWorld(cubeBlock.Position), Vector3.Forward);
                                        flag = true;
                                    }
                                    MatrixD worldMatrix = rigidBodyMatrix;
                                    worldMatrix.Translation = vectord;
                                    if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
                                    {
                                        xdPtr1 = (MatrixD*) ref worldMatrix;
                                        if (MyDestructionHelper.CreateFracturePiece(breakableBody, ref worldMatrix, null, (block3 != null) ? block3 : cubeBlock.FatBlock, true) == null)
                                        {
                                            flag3 = true;
                                        }
                                        goto TR_0039;
                                    }
                                    else
                                    {
                                        HkSimpleValueProperty property2 = breakableShape.GetProperty(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID);
                                        if (property2.IsValid())
                                        {
                                            m_tmpCompoundIds.Add((ushort) property2.ValueUInt);
                                        }
                                        else if (!property2.IsValid() && breakableShape.IsCompound())
                                        {
                                            m_tmpChildren_CompoundIds.Clear();
                                            breakableShape.GetChildren(m_tmpChildren_CompoundIds);
                                            foreach (HkdShapeInstanceInfo info3 in m_tmpChildren_CompoundIds)
                                            {
                                                HkSimpleValueProperty property3 = info3.Shape.GetProperty(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID);
                                                if (property3.IsValid())
                                                {
                                                    m_tmpCompoundIds.Add((ushort) property3.ValueUInt);
                                                }
                                            }
                                        }
                                        flag4 = true;
                                        if (m_tmpCompoundIds.Count > 0)
                                        {
                                            foreach (ushort num in m_tmpCompoundIds)
                                            {
                                                MySlimBlock block = block3.GetBlock(num);
                                                if (block == null)
                                                {
                                                    flag4 = false;
                                                    continue;
                                                }
                                                m_tmpDefinitions.Add(block.BlockDefinition.Id);
                                                flag4 &= this.RemoveShapesFromFracturedBlocks(breakableBody, block, new ushort?(num), m_tmpBlocksToDelete, m_tmpBlocksUpdateDamage);
                                            }
                                            break;
                                        }
                                        m_tmpDefinitions.Add(cubeBlock.BlockDefinition.Id);
                                        ushort? compoundId = null;
                                        flag4 = this.RemoveShapesFromFracturedBlocks(breakableBody, cubeBlock, compoundId, m_tmpBlocksToDelete, m_tmpBlocksUpdateDamage);
                                    }
                                }
                                break;
                            }
                            goto TR_0037;
                        }
                    }
                    else
                    {
                        goto TR_0034;
                    }
                    break;
                }
                if (!flag4)
                {
                    flag3 = true;
                }
                else if (MyDestructionHelper.CreateFracturePiece(breakableBody, ref (MatrixD) ref xdPtr1, m_tmpDefinitions, (block3 != null) ? block3 : cubeBlock.FatBlock, true) == null)
                {
                    flag3 = true;
                }
                m_tmpChildren_CompoundIds.Clear();
                m_tmpCompoundIds.Clear();
                m_tmpDefinitions.Clear();
                goto TR_0039;
            }
        TR_0034:
            this.m_newBodies.Clear();
            bool enable = this.m_grid.EnableGenerators(false, false);
            if (b != null)
            {
                MyAudioComponent.PlayDestructionSound(b);
            }
            if (MyFakes.ENABLE_FRACTURE_COMPONENT)
            {
                this.FindFractureComponentBlocks();
                foreach (MyFractureComponentBase.Info info4 in this.m_fractureBlockComponentsCache)
                {
                    m_tmpBlocksToDelete.Remove(((MyCubeBlock) info4.Entity).SlimBlock);
                }
                foreach (MySlimBlock block5 in m_tmpBlocksToDelete)
                {
                    m_tmpBlocksUpdateDamage.Remove(block5);
                }
                foreach (MySlimBlock block6 in m_tmpBlocksToDelete)
                {
                    if (block6.IsMultiBlockPart)
                    {
                        MyCubeGridMultiBlockInfo multiBlockInfo = block6.CubeGrid.GetMultiBlockInfo(block6.MultiBlockId);
                        if (((multiBlockInfo != null) && (multiBlockInfo.Blocks.Count > 1)) && (block6.GetFractureComponent() != null))
                        {
                            block6.ApplyDestructionDamage(0f);
                        }
                    }
                    if (block6.FatBlock != null)
                    {
                        block6.FatBlock.OnDestroy();
                    }
                    this.m_grid.RemoveBlockWithId(block6, true);
                }
                foreach (MySlimBlock block7 in m_tmpBlocksUpdateDamage)
                {
                    MyFractureComponentCubeBlock fractureComponent = block7.GetFractureComponent();
                    if (fractureComponent != null)
                    {
                        block7.ApplyDestructionDamage(fractureComponent.GetIntegrityRatioFromFracturedPieceCounts());
                    }
                }
            }
            else
            {
                foreach (MySlimBlock block9 in m_tmpBlocksToDelete)
                {
                    MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(block9.Position);
                    if (cubeBlock != null)
                    {
                        if (cubeBlock.FatBlock != null)
                        {
                            cubeBlock.FatBlock.OnDestroy();
                        }
                        this.m_grid.RemoveBlock(cubeBlock, true);
                    }
                }
            }
            this.m_grid.EnableGenerators(enable, false);
            m_tmpBlocksToDelete.Clear();
            m_tmpBlocksUpdateDamage.Clear();
            this.m_recreateBody = true;
        }

        public static float GetCollisionSparkMultiplier(float separatingVelocity, bool isLargeGrid)
        {
            float num = 0.1f;
            float num3 = 110f;
            float num4 = separatingVelocity / num3;
            float num5 = (num4 * 2f) + ((1f - num4) * num);
            return (!isLargeGrid ? num5 : (num5 * 2f));
        }

        public static float GetCollisionSparkScale(float impulseApplied, bool isLargeGrid) => 
            ((impulseApplied >= 1000f) ? ((impulseApplied >= 60000f) ? ((impulseApplied >= 1000000f) ? 1.3f : (0.63f + (7E-07f * (impulseApplied - 60000f)))) : (0.1f + (9E-06f * (impulseApplied - 1000f)))) : (0.05f + (5E-05f * impulseApplied)));

        internal ushort? GetContactCompoundId(Vector3I position, Vector3D constactPos)
        {
            List<HkdBreakableShape> shapesIntersectingSphere = new List<HkdBreakableShape>();
            base.GetRigidBodyMatrix(out base.m_bodyMatrix, true);
            Quaternion breakableBodyRotation = Quaternion.CreateFromRotationMatrix(base.m_bodyMatrix);
            if (this.BreakableBody == null)
            {
                MyLog.Default.WriteLine("BreakableBody was null in GetContactCounpoundId!");
            }
            if (base.HavokWorld.DestructionWorld == null)
            {
                MyLog.Default.WriteLine("HavokWorld.DestructionWorld was null in GetContactCompoundId!");
            }
            HkDestructionUtils.FindAllBreakableShapesIntersectingSphere(base.HavokWorld.DestructionWorld, this.BreakableBody, breakableBodyRotation, base.m_bodyMatrix.Translation, (Vector3) this.WorldToCluster(constactPos), 0.1f, shapesIntersectingSphere);
            ushort? nullable = null;
            foreach (HkdBreakableShape shape in shapesIntersectingSphere)
            {
                if (!shape.IsValid())
                {
                    continue;
                }
                HkVec3IProperty property = shape.GetProperty(HkdBreakableShape.PROPERTY_GRID_POSITION);
                if (property.IsValid() && (position == property.Value))
                {
                    HkSimpleValueProperty property2 = shape.GetProperty(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID);
                    if (property2.IsValid())
                    {
                        nullable = new ushort?((ushort) property2.ValueUInt);
                        break;
                    }
                }
            }
            return nullable;
        }

        private PredictionDisqualificationReason GetEligibilityForPredictedImpulses(IMyEntity otherEntity, out MyGridContactInfo.ContactFlags flag)
        {
            flag = 0;
            if (otherEntity == null)
            {
                return PredictionDisqualificationReason.NoEntity;
            }
            if ((otherEntity is MyVoxelBase) || (otherEntity is Sandbox.Game.WorldEnvironment.MyEnvironmentSector))
            {
                return PredictionDisqualificationReason.EntityIsStatic;
            }
            MyCubeGrid grid = otherEntity as MyCubeGrid;
            if (grid != null)
            {
                if (MyFixedGrids.IsRooted(grid))
                {
                    return PredictionDisqualificationReason.EntityIsStatic;
                }
                if (MyCubeGridGroups.Static.Physical.HasSameGroup(this.m_grid, grid))
                {
                    flag = MyGridContactInfo.ContactFlags.PredictedCollision_Disabled;
                    return PredictionDisqualificationReason.None;
                }
                if (otherEntity.Physics.LinearVelocity.LengthSquared() < 1f)
                {
                    return PredictionDisqualificationReason.EntityIsNotMoving;
                }
            }
            flag = MyGridContactInfo.ContactFlags.PredictedCollision;
            return PredictionDisqualificationReason.None;
        }

        private static RigidBodyFlag GetFlags(MyCubeGrid grid) => 
            (grid.IsStatic ? RigidBodyFlag.RBF_STATIC : ((grid.GridSizeEnum == MyCubeSize.Large) ? MyPerGameSettings.LargeGridRBFlag : RigidBodyFlag.RBF_DEFAULT));

        public List<MyFractureComponentBase.Info> GetFractureBlockComponents() => 
            this.m_fractureBlockComponentsCache;

        public List<MyFracturedBlock.Info> GetFracturedBlocks() => 
            this.m_fractureBlocksCache;

        private static Vector3 GetGridPosition(HkContactPoint contactPoint, HkRigidBody gridBody, MyCubeGrid grid, int body) => 
            Vector3.Transform(contactPoint.Position + (((body == 0) ? 0.1f : -0.1f) * contactPoint.Normal), Matrix.Invert(gridBody.GetRigidBodyMatrix()));

        public static float GetLargeShipMaxAngularVelocity() => 
            Math.Max(0f, Math.Min(LargeShipMaxAngularVelocityLimit, MySector.EnvironmentDefinition.LargeShipMaxAngularSpeedInRadians));

        public override MyStringHash GetMaterialAt(Vector3D worldPos)
        {
            Vector3I vectori;
            this.m_grid.FixTargetCubeLite(out vectori, Vector3.Transform((Vector3) worldPos, this.m_grid.PositionComp.WorldMatrixNormalizedInv) / ((double) this.m_grid.GridSize));
            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
            if (cubeBlock == null)
            {
                return base.GetMaterialAt(worldPos);
            }
            if (cubeBlock.FatBlock is MyCompoundCubeBlock)
            {
                cubeBlock = ((MyCompoundCubeBlock) cubeBlock.FatBlock).GetBlocks()[0];
            }
            MyStringHash subtypeId = cubeBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeId;
            return ((subtypeId != MyStringHash.NullOrEmpty) ? subtypeId : base.GetMaterialAt(worldPos));
        }

        public float GetMaxAngularVelocity() => 
            GetShipMaxAngularVelocity(this.m_grid.GridSizeEnum);

        public float GetMaxLinearVelocity() => 
            GetShipMaxLinearVelocity(this.m_grid.GridSizeEnum);

        public float GetMaxRelaxedAngularVelocity() => 
            (this.GetMaxAngularVelocity() * 100f);

        public float GetMaxRelaxedLinearVelocity() => 
            (this.GetMaxLinearVelocity() * 10f);

        public override HkShape GetShape() => 
            ((HkShape) this.Shape);

        public List<HkShape> GetShapesFromPosition(Vector3I pos) => 
            this.m_shape.GetShapesFromPosition(pos);

        public static float GetShipMaxAngularVelocity(MyCubeSize size) => 
            ((size == MyCubeSize.Large) ? GetLargeShipMaxAngularVelocity() : GetSmallShipMaxAngularVelocity());

        public static float GetShipMaxLinearVelocity(MyCubeSize size) => 
            ((size == MyCubeSize.Large) ? LargeShipMaxLinearVelocity() : SmallShipMaxLinearVelocity());

        public static float GetSmallShipMaxAngularVelocity() => 
            (!MyFakes.TESTING_VEHICLES ? Math.Max(0f, Math.Min(SmallShipMaxAngularVelocityLimit, MySector.EnvironmentDefinition.SmallShipMaxAngularSpeedInRadians)) : float.MaxValue);

        private static bool IsBreakableShapeCompound(HkdBreakableShape shape) => 
            (string.IsNullOrEmpty(shape.Name) || (shape.IsCompound() || (shape.GetChildrenCount() > 0)));

        public bool IsDirty() => 
            ((this.m_dirtyCubesInfo.DirtyBlocks.Count > 0) || !this.m_dirtyCubesInfo.DirtyParts.IsEmpty);

        public bool IsPlanetCrashing() => 
            this.m_planetCrash_IsStarted;

        public bool IsPlanetCrashing_PointConcealed(Vector3D point)
        {
            if (!this.m_planetCrash_IsStarted)
            {
                return false;
            }
            Vector3 vector = (Vector3) (point - this.m_planetCrash_CenterPoint.Value);
            float num2 = 4f * this.m_planetCrash_ScaleCurrent;
            return ((Math.Abs(Vector3.Dot(vector, this.m_planetCrash_Normal.Value)) < (2f * this.m_planetCrash_ScaleCurrent)) && (vector.LengthSquared() < (num2 * num2)));
        }

        public static float LargeShipMaxLinearVelocity() => 
            Math.Max(0f, Math.Min(1.498962E+08f, MySector.EnvironmentDefinition.LargeShipMaxSpeed));

        private void MarkBreakable(HkWorld world)
        {
            if (this.m_grid.BlocksDestructionEnabled)
            {
                this.m_shape.MarkBreakable(world, this.RigidBody);
                if (this.RigidBody2 != null)
                {
                    this.m_shape.MarkBreakable(world, this.RigidBody2);
                }
            }
        }

        private void MarkPredictedContactImpulse()
        {
            int predictedContactsCounter = this.PredictedContactsCounter;
            this.PredictedContactsCounter = predictedContactsCounter + 1;
            this.PredictedContactLastTime = MySandboxGame.Static.SimulationTime;
        }

        public override void OnMotion(HkRigidBody rbo, float step, bool fromParent = false)
        {
            base.OnMotion(rbo, step, fromParent);
            if ((this.LinearVelocity.LengthSquared() > 0.01f) || (this.AngularVelocity.LengthSquared() > 0.01f))
            {
                this.m_grid.MarkForUpdate();
            }
        }

        private void OnRefreshComplete()
        {
            this.m_shape.MarkBreakable((base.WeldedRigidBody != null) ? base.WeldedRigidBody : this.RigidBody);
            this.m_grid.SetInventoryMassDirty();
            this.m_shape.SetMass((base.WeldedRigidBody != null) ? base.WeldedRigidBody : this.RigidBody);
            this.m_shape.UpdateShape((base.WeldedRigidBody != null) ? base.WeldedRigidBody : this.RigidBody, (base.WeldedRigidBody != null) ? null : this.RigidBody2, this.BreakableBody);
            MyGridPhysicalHierarchy.Static.UpdateRoot(this.m_grid);
            this.m_grid.RaisePhysicsChanged();
        }

        protected override void OnUnwelded(MyPhysicsBody other)
        {
            base.OnUnwelded(other);
            this.Shape.RefreshMass();
            this.m_grid.HavokSystemIDChanged(this.HavokCollisionSystemID);
            if (!this.m_grid.IsStatic)
            {
                this.m_grid.RecalculateGravity();
            }
        }

        protected override void OnWelded(MyPhysicsBody other)
        {
            base.OnWelded(other);
            this.Shape.RefreshMass();
            if (this.m_grid.BlocksDestructionEnabled)
            {
                if (base.HavokWorld != null)
                {
                    base.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(this.RigidBody, this.Shape.BreakImpulse);
                }
                if (Sync.IsServer)
                {
                    if (this.RigidBody.BreakLogicHandler == null)
                    {
                        this.RigidBody.BreakLogicHandler = new Havok.BreakLogicHandler(this.BreakLogicHandler);
                    }
                    if (this.RigidBody.BreakPartsHandler == null)
                    {
                        this.RigidBody.BreakPartsHandler = new Havok.BreakPartsHandler(this.BreakPartsHandler);
                    }
                }
            }
            this.m_grid.HavokSystemIDChanged(other.HavokCollisionSystemID);
        }

        private unsafe bool PerformDeformation(ref HkBreakOffPointInfo pt, bool fromBreakParts, float separatingVelocity, VRage.Game.Entity.MyEntity otherEntity)
        {
            int num10;
            int* numPtr1;
            if (!this.m_grid.BlocksDestructionEnabled)
            {
                bool flag1 = MyFakes.DEFORMATION_LOGGING;
                return false;
            }
            bool flag = false;
            ulong simulationFrameCounter = MySandboxGame.Static.SimulationFrameCounter;
            if (this.m_frameCollided == simulationFrameCounter)
            {
                foreach (Vector3D vectord2 in this.m_contactPosCache)
                {
                    if (Vector3D.DistanceSquared(pt.ContactPosition, vectord2) < ((this.m_grid.GridSize * this.m_grid.GridSize) / 4f))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                if ((simulationFrameCounter - this.m_frameCollided) > 100)
                {
                    this.m_frameFirstImpact = simulationFrameCounter;
                    this.m_impactDot = 0f;
                }
                this.m_appliedSlowdownThisFrame = false;
                this.m_contactPosCache.Clear();
            }
            float num2 = separatingVelocity;
            bool flag2 = otherEntity is MyVoxelBase;
            bool flag3 = flag2 && !Vector3.IsZero(ref this.m_cachedGravity);
            bool flag4 = !flag2 && !(otherEntity is MyTrees);
            float num3 = !this.IsStatic ? Math.Min((float) 1f, (float) (this.Mass / MyFakes.DEFORMATION_MASS_THR)) : (!otherEntity.Physics.IsStatic ? Math.Min((float) 1f, (float) (otherEntity.Physics.Mass / MyFakes.DEFORMATION_MASS_THR)) : 1f);
            float num4 = 1f;
            float num5 = (num2 * num3) * MyFakes.DEFORMATION_OFFSET_RATIO;
            MyCubeGrid grid = otherEntity as MyCubeGrid;
            bool flag5 = this.m_grid.GridSizeEnum == MyCubeSize.Large;
            if (!this.IsStatic && !otherEntity.Physics.IsStatic)
            {
                if ((grid == null) || (this.m_grid.GridSizeEnum == grid.GridSizeEnum))
                {
                    num5 *= 0.5f;
                }
                else
                {
                    num5 = !flag5 ? (num5 * 1.6f) : (num5 * 0.105f);
                }
            }
            else if (!flag4)
            {
                if ((this.m_grid.PositionComp.LocalAABB.Volume() < 20f) && ((num2 / 60f) > (this.m_grid.GridSize / 5f)))
                {
                    num5 *= 30f;
                }
                else if (flag5 || !flag2)
                {
                    num5 *= 1.5f;
                }
            }
            else if ((grid == null) || (this.m_grid.GridSizeEnum == grid.GridSizeEnum))
            {
                num5 *= 0.5f;
            }
            else if (flag5)
            {
                num5 *= 0.09f;
            }
            else
            {
                num4 = 4.5f;
                num5 *= 0.22f;
            }
            if (!flag2)
            {
                num5 = Math.Min(num5, MyFakes.DEFORMATION_OFFSET_MAX);
            }
            else
            {
                float num12 = MyFakes.DEFORMATION_OFFSET_MAX * 10f;
                num5 = Math.Min(num5 / (flag5 ? 6.8f : 8f), num12);
            }
            if (num5 <= 0.1f)
            {
                bool flag9 = MyFakes.DEFORMATION_LOGGING;
                return false;
            }
            float softAreaPlanar = (flag5 ? 6f : 1.2f) * num4;
            float softAreaVertical = (flag5 ? 1.5f : 1f) * num5;
            MatrixD worldMatrixNormalizedInv = this.m_grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3D vectord = Vector3D.Transform(pt.ContactPosition, worldMatrixNormalizedInv);
            Vector3 normal = -this.GetVelocityAtPoint(pt.ContactPosition);
            Vector3 vector2 = (Vector3) (pt.ContactPointDirection * pt.ContactPoint.Normal);
            if (!normal.IsValid() || (normal.LengthSquared() < 25f))
            {
                normal = vector2;
            }
            Vector3 localNormal = Vector3.TransformNormal(normal, worldMatrixNormalizedInv);
            float num8 = localNormal.Normalize();
            bool flag6 = num8 < 3f;
            Vector3 vector4 = -normal;
            vector4.Normalize();
            float num9 = Math.Abs(Vector3.Dot(vector4, vector2));
            this.m_impactDot = (this.m_impactDot != 0f) ? ((this.m_impactDot * 0.5f) + (num9 * 0.5f)) : num9;
            if (flag2 & flag5)
            {
                softAreaVertical *= MyFakes.DEFORMATION_DAMAGE_MULTIPLIER;
                softAreaPlanar *= MyFakes.DEFORMATION_DAMAGE_MULTIPLIER * 2f;
                if (flag3)
                {
                    float num13 = 1f + ((this.m_impactDot * this.m_impactDot) / 2f);
                    softAreaVertical *= num13;
                    softAreaPlanar *= num13;
                }
            }
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                numPtr1 = (int*) ref num10;
                MyRenderProxy.DebugDrawArrow3D(pt.ContactPosition, pt.ContactPosition + (Vector3.Normalize(normal) * num10), Color.Red, new Color?(Color.Red), false, 0.1, base.Entity.DisplayName, 0.5f, true);
            }
            bool flag10 = MyFakes.DEFORMATION_LOGGING;
            if (num10 > 0)
            {
                if (!flag)
                {
                    this.m_contactPosCache.Add(pt.ContactPosition);
                }
                this.m_frameCollided = simulationFrameCounter;
            }
            if (!this.IsStatic)
            {
                if (MyFakes.DEFORMATION_APPLY_IMPULSE)
                {
                    HkRigidBody rigidBody = this.RigidBody;
                    rigidBody.ApplyPointImpulse((-(rigidBody.LinearVelocity * rigidBody.Mass) * num10) * MyFakes.DEFORMATION_IMPULSE_FACTOR, pt.ContactPoint.Position);
                }
                else if (otherEntity != null)
                {
                    if (flag2)
                    {
                        if (!this.m_appliedSlowdownThisFrame)
                        {
                            this.m_appliedSlowdownThisFrame = true;
                            MySandboxGame.Static.Invoke("ApplyColisionForce", this, delegate (object context) {
                                MyGridPhysics physics = (MyGridPhysics) context;
                                HkRigidBody rigidBody = physics.RigidBody;
                                if (rigidBody != null)
                                {
                                    int num1;
                                    bool flag = physics.m_grid.GridSizeEnum == MyCubeSize.Small;
                                    float impactDot = physics.m_impactDot;
                                    ulong num2 = MySandboxGame.Static.SimulationFrameCounter - physics.m_frameFirstImpact;
                                    if (num2 < 100)
                                    {
                                        num1 = (int) (rigidBody.LinearVelocity.LengthSquared() > 25f);
                                    }
                                    else
                                    {
                                        num1 = 0;
                                    }
                                    bool flag2 = ((num2 > 50) || ((impactDot > 0.8f) && (num2 > 10))) | flag;
                                    bool flag3 = (num2 > 100) | flag;
                                    if (num1 != 0)
                                    {
                                        Vector3 angularVelocity = rigidBody.AngularVelocity;
                                        rigidBody.AngularVelocity -= (angularVelocity * (1f - impactDot)) / 2f;
                                    }
                                    if (flag2)
                                    {
                                        float num3 = 1f - impactDot;
                                        float num4 = ((1f - (num3 * num3)) + impactDot) / 1.5f;
                                        if (impactDot < 0.5)
                                        {
                                            num4 /= 2f;
                                        }
                                        Vector3 linearVelocity = rigidBody.LinearVelocity;
                                        float maxLinearVelocity = physics.GetMaxLinearVelocity();
                                        float num6 = Math.Min((num4 * linearVelocity.Length()) / (maxLinearVelocity * 1.5f), flag3 ? 0.2f : 0.1f);
                                        if (impactDot > 0.5)
                                        {
                                            num6 *= 1f + (impactDot * 0.5f);
                                        }
                                        if (!Vector3.IsZero(ref physics.m_cachedGravity))
                                        {
                                            Vector3 projectedVector = -linearVelocity;
                                            Vector3 vector5 = physics.m_cachedGravity.Project(projectedVector);
                                            Vector3 vector6 = (vector5 * num6) * 2f;
                                            if (flag3)
                                            {
                                                vector6 += (projectedVector - vector5) * num6;
                                            }
                                            rigidBody.LinearVelocity += vector6;
                                        }
                                        else if (flag)
                                        {
                                            Vector3 vector8 = -linearVelocity;
                                            Vector3 vector9 = (Vector3) ((num6 * 1f) * vector8);
                                            rigidBody.LinearVelocity += vector9;
                                        }
                                    }
                                }
                            });
                        }
                    }
                    else if (num10 > 0)
                    {
                        HkRigidBody rigidBody = this.RigidBody;
                        Vector3 linearVelocity = rigidBody.LinearVelocity;
                        Vector3 vector7 = linearVelocity * rigidBody.Mass;
                        if (!otherEntity.Physics.IsStatic)
                        {
                            if (((grid != null) && (this.m_grid.GridSizeEnum != grid.GridSizeEnum)) && !flag5)
                            {
                                bool flag11 = MyFakes.DEFORMATION_LOGGING;
                                rigidBody.LinearVelocity = Vector3.Lerp(linearVelocity, grid.Physics.LinearVelocity, MyFakes.DEFORMATION_VELOCITY_RELAY);
                            }
                        }
                        else if (!flag4 || (grid == null))
                        {
                            if (!flag5)
                            {
                                rigidBody.ApplyPointImpulse(-vector7 * (((float) num10) / 40f), pt.ContactPoint.Position);
                            }
                        }
                        else
                        {
                            float amount = MyFakes.DEFORMATION_VELOCITY_RELAY_STATIC;
                            if (flag5)
                            {
                                amount /= 8f;
                            }
                            else if (grid.GridSizeEnum == MyCubeSize.Large)
                            {
                                amount *= 4f;
                            }
                            rigidBody.LinearVelocity = Vector3.Lerp(linearVelocity, Vector3.Zero, amount);
                        }
                    }
                }
            }
            if ((Sync.IsServer && (MyFakes.DEFORMATION_EXPLOSIONS && (this.m_grid.BlocksCount > 10))) && !flag6)
            {
                bool flag7 = this.m_grid.GridSizeEnum == MyCubeSize.Large;
                float num15 = Math.Min((float) 1f, (float) (num8 / 20f));
                float num16 = ((1f - this.m_impactDot) * 0.6f) + (flag7 ? 1.4f : 1.5f);
                float num17 = flag7 ? 0.25f : 0.06f;
                float num18 = flag7 ? 0.3f : 0.4f;
                bool flag8 = !Vector3.IsZero(this.m_cachedGravity);
                float num19 = Math.Min((float) ((((this.m_grid.GridSize + (((float) Math.Sqrt((double) num10)) * num17)) * num16) * num15) * MyFakes.DEFORMATION_VOXEL_CUTOUT_MULTIPLIER), (float) (MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS * num16));
                Vector3D vectord3 = pt.ContactPosition + ((vector2 * num19) * 0.5f);
                Vector3D vectord4 = vectord3 + (((vector4 * num19) * 0.95f) / (flag8 ? 2f : 1f));
                float num21 = Math.Min((float) ((num19 * ((num16 * 0.75f) - (this.m_impactDot * num18))) * MathHelper.Lerp(0.4f, 1f, Math.Min((float) 1f, (float) (num8 / 50f)))), (float) ((MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS * num16) * 1.5f));
                if (flag8 && (this.m_impactDot > 0.7))
                {
                    num21 *= 1.35f;
                }
                ExplosionInfo item = new ExplosionInfo {
                    Position = vectord3,
                    ExplosionType = MyExplosionTypeEnum.GRID_DESTRUCTION,
                    Radius = num19,
                    ShowParticles = false,
                    GenerateDebris = true
                };
                this.m_explosions.Add(item);
                ulong num22 = MySandboxGame.Static.SimulationFrameCounter - this.m_frameFirstImpact;
                if ((flag8 && (this.m_impactDot < 0.7)) && ((flag5 && (num22 < 10)) || !flag5))
                {
                    item = new ExplosionInfo {
                        Position = vectord4,
                        ExplosionType = MyExplosionTypeEnum.GRID_DESTRUCTION,
                        Radius = num21,
                        ShowParticles = false,
                        GenerateDebris = true
                    };
                    this.m_explosions.Add(item);
                }
                bool flag12 = MyFakes.DEFORMATION_LOGGING;
                this.m_grid.MarkForUpdateParallel();
            }
            return this.ApplyDeformation(num5, softAreaPlanar, softAreaVertical, (Vector3) vectord, localNormal, MyDamageType.Deformation, out (int) ref numPtr1, 0f, 0f, (otherEntity != null) ? otherEntity.EntityId : 0L);
        }

        private bool PerformDeformationOnGroup(VRage.Game.Entity.MyEntity entity, VRage.Game.Entity.MyEntity other, ref HkBreakOffPointInfo pt, float separatingVelocity)
        {
            bool flag = false;
            if (!entity.MarkedForClose)
            {
                if (entity.PositionComp.WorldAABB.Inflate((double) 0.10000000149011612).Contains(pt.ContactPosition) == ContainmentType.Disjoint)
                {
                    bool flag1 = MyFakes.DEFORMATION_LOGGING;
                }
                else
                {
                    MyGridPhysics physics = entity.Physics as MyGridPhysics;
                    if (physics != null)
                    {
                        flag = physics.PerformDeformation(ref pt, false, separatingVelocity, other);
                    }
                }
            }
            return flag;
        }

        public void PerformMeteoritDeformation(ref HkBreakOffPointInfo pt, float separatingVelocity)
        {
            float deformationOffset = Math.Min((float) ((0.3f + Math.Max((float) 0f, (float) (((float) Math.Sqrt(Math.Abs(separatingVelocity) + Math.Pow((double) pt.CollidingBody.Mass, 0.72))) / 10f))) * 6f), (float) 5f);
            float softAreaPlanar = (((float) Math.Pow((double) pt.CollidingBody.Mass, 0.15000000596046448)) - 0.3f) * ((this.m_grid.GridSizeEnum == MyCubeSize.Large) ? 4f : 1f);
            float softAreaVertical = deformationOffset * ((this.m_grid.GridSizeEnum == MyCubeSize.Large) ? 1f : 0.2f);
            MatrixD worldMatrixNormalizedInv = this.m_grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3D vectord = Vector3D.Transform(pt.ContactPosition, worldMatrixNormalizedInv);
            Vector3 localNormal = Vector3.TransformNormal(pt.ContactPoint.Normal, worldMatrixNormalizedInv) * pt.ContactPointDirection;
            bool flag = this.ApplyDeformation(deformationOffset, softAreaPlanar, softAreaVertical, (Vector3) vectord, localNormal, MyDamageType.Deformation, 0f, (this.m_grid.GridSizeEnum == MyCubeSize.Large) ? 0.6f : 0.16f, 0L);
            MyPhysics.CastRay(pt.ContactPoint.Position, pt.ContactPoint.Position - (softAreaVertical * Vector3.Normalize(pt.ContactPoint.Normal)), this.m_hitList, 0);
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = this.m_hitList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyEntity hitEntity = enumerator.Current.HkHitInfo.GetHitEntity();
                    if (!ReferenceEquals(hitEntity, this.m_grid.Components) && (hitEntity is MyCubeGrid))
                    {
                        MyCubeGrid grid = hitEntity as MyCubeGrid;
                        worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
                        vectord = Vector3D.Transform(pt.ContactPosition, worldMatrixNormalizedInv);
                        localNormal = Vector3.TransformNormal(pt.ContactPoint.Normal, worldMatrixNormalizedInv) * pt.ContactPointDirection;
                        grid.Physics.ApplyDeformation(deformationOffset, softAreaPlanar * ((this.m_grid.GridSizeEnum == grid.GridSizeEnum) ? 1f : ((grid.GridSizeEnum == MyCubeSize.Large) ? 2f : 0.25f)), softAreaVertical * ((this.m_grid.GridSizeEnum == grid.GridSizeEnum) ? 1f : ((grid.GridSizeEnum == MyCubeSize.Large) ? 2.5f : 0.2f)), (Vector3) vectord, localNormal, MyDamageType.Deformation, 0f, (grid.GridSizeEnum == MyCubeSize.Large) ? 0.6f : 0.16f, 0L);
                    }
                }
            }
            this.m_hitList.Clear();
            float num4 = Math.Max(this.m_grid.GridSize, deformationOffset * ((this.m_grid.GridSizeEnum == MyCubeSize.Large) ? 0.25f : 0.05f));
            if (!(((num4 > 0f) && (deformationOffset > (this.m_grid.GridSize / 2f))) & flag))
            {
                this.AddCollisionEffect(pt.ContactPosition, localNormal, 0f, 0f);
            }
            else
            {
                ExplosionInfo item = new ExplosionInfo {
                    Position = pt.ContactPosition,
                    ExplosionType = MyExplosionTypeEnum.GRID_DESTRUCTION,
                    Radius = num4,
                    ShowParticles = true,
                    GenerateDebris = true
                };
                this.m_explosions.Add(item);
                this.m_grid.MarkForUpdate();
            }
        }

        private float PlanetCrash_GetMultiplier() => 
            (this.m_planetCrash_generationMultiplier * 0.025f);

        private void PlanetCrashEffect_AddCollision(Vector3D position, float separationSpeed, Vector3 normal, MyVoxelBase voxel = null)
        {
            float mass;
            float num2;
            bool flag;
            if (!MyFakes.PLANET_CRASH_ENABLED)
            {
                return;
            }
            else if ((!this.IsStatic && !Sandbox.Engine.Platform.Game.IsDedicated) && (this.m_grid.BlocksCount >= 5))
            {
                mass = MyGridPhysicalGroupData.GetGroupSharedProperties(this.m_grid, false).Mass;
                num2 = separationSpeed * separationSpeed;
                flag = this.m_planetCrash_IsStarted;
                if (!flag && (((mass < 50000f) && (num2 < 2500f)) || ((((mass < 500000f) && (num2 < 900f)) || (((mass < 3000000f) && (num2 < 200f)) || (num2 < 50f))) || (mass < 15000f))))
                {
                    return;
                }
            }
            else
            {
                return;
            }
            if (!flag || ((mass >= 15000f) && (num2 >= 50f)))
            {
                Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                if (vector.LengthSquared() >= 0.01)
                {
                    if (!this.m_planetCrash_IsStarted)
                    {
                        this.m_planetCrash_TimeBetweenPoints = 0;
                        this.m_planetCrash_CrashAccumulation += 30;
                        if (this.m_planetCrash_CrashAccumulation < 90)
                        {
                            return;
                        }
                    }
                    float num3 = Math.Max((float) (((float) Math.Log10((double) (mass - 30000f))) - 3f), (float) 0.3f);
                    float num5 = Math.Max((float) 0f, (float) (((float) Math.Log10((double) num2)) - 2f));
                    this.m_planetCrash_ScaleGoal = (1.5f * num3) * num5;
                    if (!this.m_planetCrash_IsStarted)
                    {
                        this.m_planetCrash_ScaleCurrent = 0.8f * this.m_planetCrash_ScaleGoal;
                    }
                    this.m_planetCrash_IsStarted = true;
                    this.m_grid.MarkForUpdate();
                    if (this.m_planetCrash_CenterPoint == null)
                    {
                        this.m_planetCrash_CenterPoint = new Vector3D?(position);
                        this.m_planetCrash_Normal = new Vector3?(Vector3.Normalize(vector));
                    }
                    else
                    {
                        (position - this.m_planetCrash_CenterPoint.Value).Length();
                        this.m_planetCrash_CenterPoint = new Vector3D?(position);
                        this.m_planetCrash_Normal = new Vector3?(Vector3.Normalize((Vector3) ((0.85f * this.m_planetCrash_Normal.Value) + (0.15f * normal))));
                    }
                    this.m_planetCrash_generationMultiplier = 100f;
                }
            }
        }

        private void PlanetCrashEffect_Reduce()
        {
            int num = 60;
            if (this.m_planetCrash_TimeBetweenPoints < num)
            {
                this.m_planetCrash_generationMultiplier -= 0.3f;
            }
            else
            {
                this.m_planetCrash_generationMultiplier *= 0.92f;
            }
        }

        private void PlanetCrashEffect_Update()
        {
            if (MyFakes.PLANET_CRASH_ENABLED && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_planetCrash_TimeBetweenPoints++;
                if (this.m_planetCrash_CrashAccumulation > 0)
                {
                    this.m_planetCrash_CrashAccumulation--;
                }
                if (this.IsPlanetCrashing())
                {
                    this.PlanetCrashEffect_Reduce();
                    if ((this.m_planetCrash_Effect == null) && (this.m_planetCrash_generationMultiplier > 0.01f))
                    {
                        MyParticlesManager.TryCreateParticleEffect("PlanetCrash", Matrix.Identity, out this.m_planetCrash_Effect);
                    }
                    if (this.m_planetCrash_Effect != null)
                    {
                        this.m_planetCrash_Effect.UserBirthMultiplier = this.PlanetCrash_GetMultiplier();
                        this.m_planetCrash_Effect.WorldMatrix = Matrix.CreateWorld(this.m_planetCrash_CenterPoint.Value, -this.m_planetCrash_Normal.Value, Vector3.CalculatePerpendicularVector(this.m_planetCrash_Normal.Value));
                        this.m_planetCrash_Effect.UserScale = this.m_planetCrash_ScaleCurrent * 0.1f;
                    }
                    this.m_planetCrash_ScaleCurrent = (this.m_planetCrash_ScaleGoal <= this.m_planetCrash_ScaleCurrent) ? (this.m_planetCrash_ScaleCurrent * 0.995f) : (this.m_planetCrash_ScaleCurrent * 1.06f);
                    if (this.m_planetCrash_generationMultiplier < 0.01f)
                    {
                        this.m_planetCrash_generationMultiplier = 0f;
                        this.m_planetCrash_ScaleGoal = 1f;
                        this.m_planetCrash_ScaleCurrent = 1f;
                        this.m_planetCrash_CrashAccumulation = 0;
                        this.m_planetCrash_IsStarted = false;
                        if (this.m_planetCrash_Effect != null)
                        {
                            this.m_planetCrash_Effect.Stop(false);
                            this.m_planetCrash_Effect = null;
                        }
                    }
                }
            }
        }

        public bool PlanetCrashingNeedsUpdates() => 
            (this.IsPlanetCrashing() || (this.m_planetCrash_CrashAccumulation > 0));

        public void PlayCollisionParticlesInternal(IMyEntity otherEntity, ref Vector3D worldPosition, ref Vector3 normal, ref Vector3 separatingVelocity, float separatingSpeed, float impulse, VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes flags)
        {
            if ((flags & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_PlanetCrash) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
            {
                this.PlanetCrashEffect_AddCollision(worldPosition, separatingSpeed, normal, otherEntity as MyVoxelBase);
            }
            if ((flags & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_Collision) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
            {
                this.AddCollisionEffect(worldPosition, normal, separatingSpeed, impulse);
            }
            if ((flags & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_GridCollision) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
            {
                this.AddGridCollisionEffect(worldPosition - this.RigidBody.Position, normal, separatingVelocity, separatingSpeed, impulse);
            }
            if ((flags & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_Dust) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
            {
                float scale = MathHelper.Clamp((float) (Math.Abs((float) (separatingSpeed * (this.m_grid.Mass / 100000f))) / 10f), (float) 0.2f, (float) 2f);
                this.AddDustEffect(worldPosition, scale);
            }
        }

        private void PredictContactImpulse(IMyEntity otherEntity, ref HkContactPointEvent e)
        {
            if (e.FirstCallbackForFullManifold)
            {
                MyGridContactInfo.ContactFlags flag = e.ContactProperties.GetFlags();
                if ((flag & (MyGridContactInfo.ContactFlags.PredictedCollision_Disabled | MyGridContactInfo.ContactFlags.PredictedCollision)) == 0)
                {
                    PredictionDisqualificationReason eligibilityForPredictedImpulses = this.GetEligibilityForPredictedImpulses(otherEntity, out flag);
                    if (eligibilityForPredictedImpulses != PredictionDisqualificationReason.None)
                    {
                        if (eligibilityForPredictedImpulses == PredictionDisqualificationReason.EntityIsNotMoving)
                        {
                            this.MarkPredictedContactImpulse();
                        }
                        return;
                    }
                    e.ContactProperties.SetFlag(flag);
                }
                if ((flag & MyGridContactInfo.ContactFlags.PredictedCollision_Disabled) == 0)
                {
                    this.MarkPredictedContactImpulse();
                    float num = PredictContactMass(otherEntity);
                    float mass = MyGridPhysicalGroupData.GetGroupSharedProperties(this.m_grid, false).Mass;
                    if ((num > 0f) && (mass > 0f))
                    {
                        HkRigidBody rigidBody = this.RigidBody;
                        bool flag1 = e.Base.BodyA == rigidBody;
                        int bodyIndex = flag1 ? 0 : 1;
                        e.AccessVelocities(bodyIndex);
                        float num4 = mass + num;
                        float num5 = 1f - (mass / num4);
                        float num6 = (((otherEntity.Physics.LinearVelocity - this.LinearVelocity).Length() * num4) * num5) * PREDICTION_IMPULSE_SCALE;
                        Vector3 normal = e.ContactPoint.Normal;
                        if (!flag1)
                        {
                            normal = -normal;
                        }
                        Vector3 vector2 = (normal * num6) / mass;
                        rigidBody.LinearVelocity += vector2;
                        e.UpdateVelocities(bodyIndex);
                    }
                }
            }
        }

        private static float PredictContactMass(IMyEntity entity)
        {
            if (entity is MyEntitySubpart)
            {
                entity = entity.Parent;
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    entity = block.CubeGrid;
                }
            }
            MyCubeGrid localGrid = entity as MyCubeGrid;
            if (localGrid != null)
            {
                return MyGridPhysicalGroupData.GetGroupSharedProperties(localGrid, false).Mass;
            }
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                return character.CurrentMass;
            }
            if ((entity is MyDebrisBase) || (entity is MyFloatingObject))
            {
                return 1f;
            }
            MyInventoryBagEntity entity2 = entity as MyInventoryBagEntity;
            return ((entity2 == null) ? 0f : entity2.Physics.Mass);
        }

        private void RecreateBreakableBody(HashSet<Vector3I> dirtyBlocks)
        {
            bool isFixedOrKeyframed = this.RigidBody.IsFixedOrKeyframed;
            int layer = this.RigidBody.Layer;
            HkWorld havokWorld = this.m_grid.Physics.HavokWorld;
            foreach (HkdBreakableBody body in this.m_newBreakableBodies)
            {
                MyFracturedPiecesManager.Static.ReturnToPool(body);
            }
            HkRigidBody rigidBody = this.BreakableBody.GetRigidBody();
            Vector3 linearVelocity = rigidBody.LinearVelocity;
            Vector3 angularVelocity = rigidBody.AngularVelocity;
            if (this.m_grid.BlocksCount <= 0)
            {
                this.m_grid.Close();
            }
            else
            {
                this.Shape.UnmarkBreakable(this.RigidBody);
                this.Shape.RefreshBlocks(this.RigidBody, dirtyBlocks);
                this.Shape.MarkBreakable(this.RigidBody);
                this.Shape.UpdateShape(this.RigidBody, this.RigidBody2, this.BreakableBody);
                this.CloseRigidBody();
                HkShape shape = (HkShape) this.m_shape;
                HkMassProperties? massProperties = null;
                this.CreateBody(ref shape, massProperties);
                this.RigidBody.Layer = layer;
                this.RigidBody.ContactPointCallbackEnabled = true;
                this.RigidBody.ContactSoundCallbackEnabled = true;
                this.RigidBody.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback_Destruction);
                this.BreakableBody.BeforeControllerOperation += new BeforeBodyControllerOperation(this.BreakableBody_BeforeControllerOperation);
                this.BreakableBody.AfterControllerOperation += new AfterBodyControllerOperation(this.BreakableBody_AfterControllerOperation);
                Matrix worldMatrix = (Matrix) base.Entity.PositionComp.WorldMatrix;
                worldMatrix.Translation = (Vector3) this.WorldToCluster(base.Entity.PositionComp.GetPosition());
                this.RigidBody.SetWorldMatrix(worldMatrix);
                this.RigidBody.UserObject = this;
                base.Entity.Physics.LinearVelocity = this.m_oldLinVel;
                base.Entity.Physics.AngularVelocity = this.m_oldAngVel;
                this.m_grid.DetectDisconnectsAfterFrame();
                this.Shape.CreateConnectionToWorld(this.BreakableBody, havokWorld);
                base.HavokWorld.DestructionWorld.AddBreakableBody(this.BreakableBody);
            }
            this.m_newBreakableBodies.Clear();
            this.m_fractureBlocksCache.Clear();
        }

        private MyGridContactInfo ReduceVelocities(MyGridContactInfo info)
        {
            info.Event.AccessVelocities(0);
            info.Event.AccessVelocities(1);
            if (!info.CollidingEntity.Physics.IsStatic && (info.CollidingEntity.Physics.Mass < 600f))
            {
                MyPhysicsComponentBase physics = info.CollidingEntity.Physics;
                physics.LinearVelocity /= 2f;
            }
            if (!this.IsStatic && (MyDestructionHelper.MassFromHavok(this.Mass) < 600f))
            {
                this.LinearVelocity /= 2f;
            }
            info.Event.UpdateVelocities(0);
            info.Event.UpdateVelocities(1);
            return info;
        }

        private bool RemoveShapesFromFracturedBlocks(HkdBreakableBody bBody, MySlimBlock block, ushort? compoundId, HashSet<MySlimBlock> blocksToDelete, HashSet<MySlimBlock> blocksUpdateDamage)
        {
            MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
            if (fractureComponent == null)
            {
                blocksToDelete.Add(block);
            }
            else
            {
                ushort? nullable;
                bool flag = false;
                HkdBreakableShape breakableShape = bBody.BreakableShape;
                if (!IsBreakableShapeCompound(breakableShape))
                {
                    string name = bBody.BreakableShape.Name;
                    string[] shapeNames = new string[] { name };
                    flag = fractureComponent.RemoveChildShapes(shapeNames);
                    nullable = compoundId;
                    MySyncDestructions.RemoveShapeFromFractureComponent(block.CubeGrid.EntityId, block.Position, (nullable != null) ? nullable.GetValueOrDefault() : ((ushort) 0xffff), name);
                }
                else
                {
                    m_tmpShapeNames.Clear();
                    m_tmpChildren_RemoveShapes.Clear();
                    breakableShape.GetChildren(m_tmpChildren_RemoveShapes);
                    int count = m_tmpChildren_RemoveShapes.Count;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= count)
                        {
                            m_tmpChildren_RemoveShapes.ForEach(delegate (HkdShapeInstanceInfo c) {
                                string shapeName = c.ShapeName;
                                if (!string.IsNullOrEmpty(shapeName))
                                {
                                    m_tmpShapeNames.Add(shapeName);
                                }
                            });
                            if (m_tmpShapeNames.Count != 0)
                            {
                                flag = fractureComponent.RemoveChildShapes(m_tmpShapeNames);
                                nullable = compoundId;
                                MySyncDestructions.RemoveShapesFromFractureComponent(block.CubeGrid.EntityId, block.Position, (nullable != null) ? nullable.GetValueOrDefault() : ((ushort) 0xffff), m_tmpShapeNames);
                            }
                            m_tmpChildren_RemoveShapes.Clear();
                            m_tmpShapeNames.Clear();
                            break;
                        }
                        HkdShapeInstanceInfo info = m_tmpChildren_RemoveShapes[num2];
                        if (string.IsNullOrEmpty(info.ShapeName))
                        {
                            info.Shape.GetChildren(m_tmpChildren_RemoveShapes);
                        }
                        num2++;
                    }
                }
                if (flag)
                {
                    blocksToDelete.Add(block);
                }
                else
                {
                    blocksUpdateDamage.Add(block);
                }
            }
            return true;
        }

        private void RigidBody_CollisionAddedCallbackClient(ref HkCollisionEvent e)
        {
            if (this.PredictCollisions)
            {
                MyGridContactInfo.ContactFlags flags;
                VRage.Game.Entity.MyEntity otherEntity = (VRage.Game.Entity.MyEntity) e.GetOtherEntity(this.m_grid);
                PredictionDisqualificationReason eligibilityForPredictedImpulses = this.GetEligibilityForPredictedImpulses(otherEntity, out flags);
                if (eligibilityForPredictedImpulses != PredictionDisqualificationReason.None)
                {
                    if (eligibilityForPredictedImpulses == PredictionDisqualificationReason.EntityIsNotMoving)
                    {
                        this.m_predictedContactEntities[otherEntity] = true;
                    }
                }
                else
                {
                    e.Disable();
                    int nrContactPoints = e.NrContactPoints;
                    for (int i = 0; i < nrContactPoints; i++)
                    {
                        e.GetContactPointPropertiesAt(i).SetFlag(flags);
                    }
                }
            }
        }

        private void RigidBody_CollisionRemovedCallbackClient(ref HkCollisionEvent e)
        {
            VRage.Game.Entity.MyEntity otherEntity = (VRage.Game.Entity.MyEntity) e.GetOtherEntity(this.m_grid);
            if ((otherEntity != null) && this.m_predictedContactEntities.ContainsKey(otherEntity))
            {
                this.m_predictedContactEntities[otherEntity] = false;
            }
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent value)
        {
            this.RigidBody_ContactPointCallbackImpl(ref value);
        }

        private unsafe void RigidBody_ContactPointCallback_Destruction(ref HkContactPointEvent value)
        {
            MyGridContactInfo info = new MyGridContactInfo(ref value, this.m_grid);
            if (!info.IsKnown)
            {
                MyCubeGrid currentEntity = info.CurrentEntity;
                if (((currentEntity != null) && (currentEntity.Physics != null)) && (currentEntity.Physics.RigidBody != null))
                {
                    HkRigidBody rigidBody = currentEntity.Physics.RigidBody;
                    MyPhysicsBody physicsBody = value.GetPhysicsBody(0);
                    MyPhysicsBody body2 = value.GetPhysicsBody(1);
                    if ((physicsBody != null) && (body2 != null))
                    {
                        IMyEntity entity = physicsBody.Entity;
                        IMyEntity parent = body2.Entity;
                        if ((((entity != null) && ((parent != null) && (entity.Physics != null))) && (parent.Physics != null)) && (!(entity is MyFracturedPiece) || !(parent is MyFracturedPiece)))
                        {
                            HkRigidBody bodyA = value.Base.BodyA;
                            HkRigidBody bodyB = value.Base.BodyB;
                            info.HandleEvents();
                            if ((!bodyA.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT) && !bodyB.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT)) && ((!(info.CollidingEntity is MyCharacter) && (info.CollidingEntity != null)) && !info.CollidingEntity.MarkedForClose))
                            {
                                MyCubeGrid node = entity as MyCubeGrid;
                                MyCubeGrid grid3 = parent as MyCubeGrid;
                                if ((grid3 == null) && (parent is MyEntitySubpart))
                                {
                                    while (true)
                                    {
                                        if ((parent == null) || (parent is MyCubeGrid))
                                        {
                                            if (parent != null)
                                            {
                                                bodyB = (parent.Physics as MyPhysicsBody).RigidBody;
                                                grid3 = parent as MyCubeGrid;
                                            }
                                            break;
                                        }
                                        parent = parent.Parent;
                                    }
                                }
                                if (((node == null) || (grid3 == null)) || !ReferenceEquals(MyCubeGridGroups.Static.Physical.GetGroup(node), MyCubeGridGroups.Static.Physical.GetGroup(grid3)))
                                {
                                    MyStringHash hash;
                                    float single1;
                                    float single2;
                                    Math.Abs(value.SeparatingVelocity);
                                    Vector3 velocityAtPoint = bodyA.GetVelocityAtPoint(info.Event.ContactPoint.Position);
                                    Vector3 vector2 = bodyB.GetVelocityAtPoint(info.Event.ContactPoint.Position);
                                    float num = velocityAtPoint.Length();
                                    float num2 = vector2.Length();
                                    Vector3 vector3 = (num > 0f) ? Vector3.Normalize(velocityAtPoint) : Vector3.Zero;
                                    Vector3 vector4 = (num2 > 0f) ? Vector3.Normalize(vector2) : Vector3.Zero;
                                    float num3 = MyDestructionHelper.MassFromHavok(bodyA.Mass);
                                    float num4 = MyDestructionHelper.MassFromHavok(bodyB.Mass);
                                    float damage = num * num3;
                                    float num6 = num2 * num4;
                                    if (num <= 0f)
                                    {
                                        single1 = 0f;
                                    }
                                    else
                                    {
                                        single1 = Vector3.Dot(vector3, value.ContactPoint.Normal);
                                    }
                                    float num7 = single1;
                                    if (num2 <= 0f)
                                    {
                                        single2 = 0f;
                                    }
                                    else
                                    {
                                        single2 = Vector3.Dot(vector4, value.ContactPoint.Normal);
                                    }
                                    float num8 = single2;
                                    num *= Math.Abs(num7);
                                    num2 *= Math.Abs(num8);
                                    bool flag = num3 == 0f;
                                    bool flag2 = num4 == 0f;
                                    bool flag3 = (entity is MyFracturedPiece) || ((node != null) && (node.GridSizeEnum == MyCubeSize.Small));
                                    bool flag4 = (parent is MyFracturedPiece) || ((grid3 != null) && (grid3.GridSizeEnum == MyCubeSize.Small));
                                    Vector3.Dot(vector3, vector4);
                                    float maxDestructionRadius = 0.5f;
                                    damage *= info.ImpulseMultiplier;
                                    num6 *= info.ImpulseMultiplier;
                                    MyHitInfo hitInfo = new MyHitInfo();
                                    Vector3D contactPosition = info.ContactPosition;
                                    hitInfo.Normal = value.ContactPoint.Normal;
                                    if (num7 < 0f)
                                    {
                                        if (entity is MyFracturedPiece)
                                        {
                                            damage /= 10f;
                                        }
                                        damage *= Math.Abs(num7);
                                        if (((((damage > 2000f) && (num > 2f)) && !flag4) || ((damage > 500f) && (num > 10f))) && (flag2 || ((damage / num6) > 10f)))
                                        {
                                            MyHitInfo* infoPtr1 = (MyHitInfo*) ref hitInfo;
                                            infoPtr1->Position = contactPosition + (0.1f * hitInfo.Normal);
                                            damage -= num3;
                                            if (Sync.IsServer && (damage > 0f))
                                            {
                                                if (node != null)
                                                {
                                                    node.DoDamage(damage, hitInfo, new Vector3?(GetGridPosition(value.ContactPoint, bodyA, node, 0)), (grid3 != null) ? grid3.EntityId : 0L);
                                                }
                                                else
                                                {
                                                    MyDestructionHelper.TriggerDestruction(damage, (MyPhysicsBody) entity.Physics, info.ContactPosition, value.ContactPoint.Normal, maxDestructionRadius);
                                                }
                                                MyHitInfo* infoPtr2 = (MyHitInfo*) ref hitInfo;
                                                infoPtr2->Position = contactPosition - (0.1f * hitInfo.Normal);
                                                if (grid3 != null)
                                                {
                                                    grid3.DoDamage(damage, hitInfo, new Vector3?(GetGridPosition(value.ContactPoint, bodyB, grid3, 1)), (node != null) ? node.EntityId : 0L);
                                                }
                                                else
                                                {
                                                    MyDestructionHelper.TriggerDestruction(damage, (MyPhysicsBody) parent.Physics, info.ContactPosition, value.ContactPoint.Normal, maxDestructionRadius);
                                                }
                                                this.ReduceVelocities(info);
                                            }
                                            hash = new MyStringHash();
                                            hash = new MyStringHash();
                                            MyDecals.HandleAddDecal(entity, hitInfo, hash, hash, null, -1f);
                                            hash = new MyStringHash();
                                            hash = new MyStringHash();
                                            MyDecals.HandleAddDecal(parent, hitInfo, hash, hash, null, -1f);
                                        }
                                    }
                                    if (num8 < 0f)
                                    {
                                        if (parent is MyFracturedPiece)
                                        {
                                            num6 /= 10f;
                                        }
                                        num6 *= Math.Abs(num8);
                                        if (((((num6 > 2000f) && (num2 > 2f)) && !flag3) || ((num6 > 500f) && (num2 > 10f))) && (flag || ((num6 / damage) > 10f)))
                                        {
                                            MyHitInfo* infoPtr3 = (MyHitInfo*) ref hitInfo;
                                            infoPtr3->Position = contactPosition + (0.1f * hitInfo.Normal);
                                            num6 -= num4;
                                            if (Sync.IsServer && (num6 > 0f))
                                            {
                                                if (node != null)
                                                {
                                                    node.DoDamage(num6, hitInfo, new Vector3?(GetGridPosition(value.ContactPoint, bodyA, node, 0)), (grid3 != null) ? grid3.EntityId : 0L);
                                                }
                                                else
                                                {
                                                    MyDestructionHelper.TriggerDestruction(num6, (MyPhysicsBody) entity.Physics, info.ContactPosition, value.ContactPoint.Normal, maxDestructionRadius);
                                                }
                                                MyHitInfo* infoPtr4 = (MyHitInfo*) ref hitInfo;
                                                infoPtr4->Position = contactPosition - (0.1f * hitInfo.Normal);
                                                if (grid3 != null)
                                                {
                                                    grid3.DoDamage(num6, hitInfo, new Vector3?(GetGridPosition(value.ContactPoint, bodyB, grid3, 1)), (node != null) ? node.EntityId : 0L);
                                                }
                                                else
                                                {
                                                    MyDestructionHelper.TriggerDestruction(num6, (MyPhysicsBody) parent.Physics, info.ContactPosition, value.ContactPoint.Normal, maxDestructionRadius);
                                                }
                                                this.ReduceVelocities(info);
                                            }
                                            hash = new MyStringHash();
                                            hash = new MyStringHash();
                                            MyDecals.HandleAddDecal(entity, hitInfo, hash, hash, null, -1f);
                                            hash = new MyStringHash();
                                            hash = new MyStringHash();
                                            MyDecals.HandleAddDecal(parent, hitInfo, hash, hash, null, -1f);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void RigidBody_ContactPointCallbackImpl(ref HkContactPointEvent value)
        {
            if ((this.m_grid != null) && (this.m_grid.Physics != null))
            {
                bool flag;
                if (Math.Abs(value.SeparatingVelocity) < 0.3f)
                {
                    int isEnvironment;
                    if (!value.Base.GetRigidBody(0).IsEnvironment)
                    {
                        isEnvironment = (int) value.Base.GetRigidBody(1).IsEnvironment;
                    }
                    else
                    {
                        isEnvironment = 1;
                    }
                    if (isEnvironment != 0)
                    {
                        return;
                    }
                }
                IMyEntity otherEntity = value.GetOtherEntity(this.m_grid, out flag);
                if (otherEntity != null)
                {
                    if (this.PredictCollisions)
                    {
                        this.PredictContactImpulse(otherEntity, ref value);
                    }
                    MyGridContactInfo info = new MyGridContactInfo(ref value, this.m_grid, otherEntity as VRage.Game.Entity.MyEntity);
                    if (!(info.CollidingEntity is MyCharacter) && !info.CollidingEntity.MarkedForClose)
                    {
                        int num3;
                        if (flag)
                        {
                            value.ContactPoint.Flip();
                        }
                        HkContactPoint contactPoint = value.ContactPoint;
                        bool flag2 = (info.CollidingEntity is MyVoxelPhysics) || (info.CollidingEntity is MyVoxelMap);
                        if (flag2)
                        {
                            if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTACT_MATERIAL)
                            {
                                MyVoxelMaterialDefinition voxelSurfaceMaterial = info.VoxelSurfaceMaterial;
                                if (voxelSurfaceMaterial != null)
                                {
                                    string[] textArray1 = new string[] { voxelSurfaceMaterial.Id.SubtypeName, "(", voxelSurfaceMaterial.Friction.ToString("F2"), ";", voxelSurfaceMaterial.Restitution.ToString("F2"), ")" };
                                    MyRenderProxy.DebugDrawText3D(this.ClusterToWorld(contactPoint.Position), string.Concat(textArray1), Color.Red, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                }
                            }
                            if (this.m_grid.Render != null)
                            {
                                this.m_grid.Render.ResetLastVoxelContactTimer();
                            }
                        }
                        if (!MyPerGameSettings.EnableCollisionSparksEffect || this.IsPlanetCrashing())
                        {
                            num3 = 0;
                        }
                        else
                        {
                            num3 = (int) ((info.CollidingEntity is MyCubeGrid) | flag2);
                        }
                        bool flag3 = (bool) num3;
                        info.HandleEvents();
                        if (MyDebugDrawSettings.DEBUG_DRAW_FRICTION)
                        {
                            Vector3D worldPos = this.ClusterToWorld(contactPoint.Position);
                            Vector3 vector = -this.GetVelocityAtPoint(worldPos) * 0.1f;
                            float num = Math.Abs((float) (this.Gravity.Dot(contactPoint.Normal) * value.ContactProperties.Friction));
                            if (vector.Length() > 0.5f)
                            {
                                vector.Normalize();
                                MyRenderProxy.DebugDrawArrow3D(worldPos, worldPos + (num * vector), Color.Gray, new Color?(Color.Gray), false, 0.1, null, 0.5f, true);
                            }
                        }
                        if (!info.IsKnown)
                        {
                            MyVoxelMaterialDefinition voxelSurfaceMaterial = info.VoxelSurfaceMaterial;
                            if (voxelSurfaceMaterial != null)
                            {
                                HkContactPointProperties contactProperties = value.ContactProperties;
                                HkContactPointProperties* propertiesPtr1 = (HkContactPointProperties*) ref contactProperties;
                                propertiesPtr1.Friction *= voxelSurfaceMaterial.Friction;
                                HkContactPointProperties* propertiesPtr2 = (HkContactPointProperties*) ref contactProperties;
                                propertiesPtr2.Restitution *= voxelSurfaceMaterial.Restitution * ((this.m_grid.GridSizeEnum == MyCubeSize.Small) ? 0.4f : 0.25f);
                            }
                        }
                        MyCubeGrid grid = otherEntity as MyCubeGrid;
                        if (this.m_isServer && ((grid != null) | flag2))
                        {
                            VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes none = VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None;
                            Vector3 separatingVelocity = this.CalculateSeparatingVelocity(value.Base.BodyA, value.Base.BodyB, contactPoint.Position);
                            float separatingSpeed = separatingVelocity.Length();
                            if (flag2)
                            {
                                none |= VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_PlanetCrash;
                            }
                            if (info.EnableParticles)
                            {
                                if ((flag3 && (((separatingSpeed > 1f) || (value.ContactProperties.MaxImpulse > 5000f)) && ((this.IsStatic || (MyGridPhysicalGroupData.GetGroupSharedProperties(this.m_grid, false).Mass > 5000f)) && this.m_lastContacts.TryAdd(value.ContactPointId, MySandboxGame.TotalGamePlayTimeInMilliseconds)))) && (((separatingSpeed > 0.3f) && (value.ContactProperties.MaxImpulse > 20000f)) || (separatingSpeed > 0.8f)))
                                {
                                    Vector3 vector4 = separatingVelocity / separatingSpeed;
                                    if ((Math.Abs(Vector3.Dot(contactPoint.Normal, vector4)) > 0.75) || (separatingSpeed < 2f))
                                    {
                                        none |= VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_Collision;
                                    }
                                    else if (this.RigidBody != null)
                                    {
                                        none |= VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_GridCollision;
                                    }
                                }
                                if (((MyPerGameSettings.EnableCollisionSparksEffect & flag2) && !this.IsPlanetCrashing()) && (Math.Abs((float) (value.SeparatingVelocity * (this.m_grid.Mass / 100000f))) > 0.25f))
                                {
                                    none |= VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.Particle_Dust;
                                }
                            }
                            if (none != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
                            {
                                Vector3 normal = contactPoint.Normal;
                                Vector3D worldPosition = this.ClusterToWorld(contactPoint.Position);
                                if (!Sync.IsServer || (MyMultiplayer.Static == null))
                                {
                                    this.PlayCollisionParticlesInternal(otherEntity, ref worldPosition, ref normal, ref separatingVelocity, separatingSpeed, value.ContactProperties.MaxImpulse, none);
                                }
                                else
                                {
                                    MyCubeGrid entity = base.Entity as MyCubeGrid;
                                    Vector3 relativePosition = (Vector3) (worldPosition - entity.PositionComp.GetPosition());
                                    entity.UpdateParticleContactPoint(otherEntity.EntityId, ref relativePosition, ref normal, ref separatingVelocity, separatingSpeed, value.ContactProperties.MaxImpulse, none);
                                }
                            }
                        }
                        if (flag)
                        {
                            value.ContactPoint.Flip();
                        }
                    }
                }
            }
        }

        private void ScheduleRemoveBlocksCallbacks()
        {
            if (Interlocked.Exchange(ref this.m_removeBlocksCallbackScheduled, 1) == 0)
            {
                MySandboxGame.Static.Invoke(delegate {
                    this.m_removeBlocksCallbackScheduled = 0;
                    if (this.IsDirty())
                    {
                        this.m_grid.MarkForUpdate();
                    }
                    bool flag = true;
                    while (flag)
                    {
                        flag = false;
                        IEnumerator<KeyValuePair<MySlimBlock, byte>> enumerator = this.m_removedCubes.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                KeyValuePair<MySlimBlock, byte> current = enumerator.Current;
                                flag = true;
                                MySlimBlock key = current.Key;
                                this.m_removedCubes.Remove<MySlimBlock, byte>(key);
                                if (!key.IsDestroyed)
                                {
                                    key.CubeGrid.RemoveDestroyedBlock(key, 0L);
                                }
                            }
                        }
                        finally
                        {
                            if (enumerator == null)
                            {
                                continue;
                            }
                            enumerator.Dispose();
                        }
                    }
                }, "ApplyDeformation/RemoveDestroyedBlock");
            }
        }

        public void SetDefaultRigidBodyMaxVelocities()
        {
            if (this.IsStatic)
            {
                this.RigidBody.MaxLinearVelocity = LargeShipMaxLinearVelocity();
                this.RigidBody.MaxAngularVelocity = GetLargeShipMaxAngularVelocity();
            }
            else if (this.m_grid.GridSizeEnum == MyCubeSize.Large)
            {
                this.RigidBody.MaxLinearVelocity = LargeShipMaxLinearVelocity();
                this.RigidBody.MaxAngularVelocity = GetLargeShipMaxAngularVelocity();
            }
            else if (this.m_grid.GridSizeEnum == MyCubeSize.Small)
            {
                this.RigidBody.MaxLinearVelocity = SmallShipMaxLinearVelocity();
                this.RigidBody.MaxAngularVelocity = GetSmallShipMaxAngularVelocity();
            }
        }

        public void SetRelaxedRigidBodyMaxVelocities()
        {
            this.RigidBody.MaxLinearVelocity = this.GetMaxRelaxedLinearVelocity();
            this.RigidBody.MaxAngularVelocity = this.GetMaxRelaxedAngularVelocity();
        }

        public static float ShipMaxLinearVelocity() => 
            Math.Max(LargeShipMaxLinearVelocity(), SmallShipMaxLinearVelocity());

        public static float SmallShipMaxLinearVelocity() => 
            Math.Max(0f, Math.Min(1.498962E+08f, MySector.EnvironmentDefinition.SmallShipMaxSpeed));

        private void UnmarkBreakable(HkWorld world)
        {
            if (this.m_grid.BlocksDestructionEnabled)
            {
                if (this.m_shape != null)
                {
                    this.m_shape.UnmarkBreakable(world, this.RigidBody);
                }
                if (this.RigidBody2 != null)
                {
                    this.m_shape.UnmarkBreakable(world, this.RigidBody2);
                }
            }
        }

        public void UpdateAfterSimulation()
        {
            this.UpdateCollisionParticleEffects();
            this.PlanetCrashEffect_Update();
            this.UpdateExplosions();
        }

        public void UpdateBeforeSimulation()
        {
            this.UpdateShape();
            this.m_shape.RecomputeSharedTensorIfNeeded();
            this.UpdateTOIOptimizer();
        }

        private void UpdateCollisionParticleEffect(ref CollisionParticleEffect effect, bool countDown = true)
        {
            if (effect.RemainingTime < 20)
            {
                if ((effect.Effect != null) && (this.RigidBody != null))
                {
                    MatrixD worldMatrix = effect.Effect.WorldMatrix;
                    worldMatrix.Translation = effect.RelativePosition + this.RigidBody.Position;
                    effect.Effect.WorldMatrix = worldMatrix;
                }
            }
            else
            {
                float speed = effect.SeparatingVelocity.Length();
                Vector3 vector = effect.SeparatingVelocity / speed;
                Vector3 forward = -vector + ((1.1f * Vector3.Dot(vector, effect.Normal)) * effect.Normal);
                if (effect.Effect == null)
                {
                    MyParticlesManager.TryCreateParticleEffect("Collision_Sparks_Directional", MatrixD.CreateWorld(effect.RelativePosition + this.RigidBody.Position, forward, effect.Normal), out effect.Effect);
                }
                if ((effect.Effect != null) && (this.RigidBody != null))
                {
                    effect.Effect.WorldMatrix = MatrixD.CreateWorld(effect.RelativePosition + this.RigidBody.Position, forward, effect.Normal);
                    effect.Effect.UserBirthMultiplier = this.ComputeDirecionalSparkMultiplier(speed);
                    effect.Effect.UserScale = 0.5f * this.ComputeDirecionalSparkScale(effect.Impulse);
                }
            }
            if (countDown)
            {
                effect.RemainingTime--;
            }
        }

        public void UpdateCollisionParticleEffects()
        {
            int index = 0;
            while (index < this.m_collisionParticles.Count)
            {
                CollisionParticleEffect effect = this.m_collisionParticles[index];
                this.UpdateCollisionParticleEffect(ref effect, true);
                if (effect.RemainingTime >= 0)
                {
                    index++;
                    continue;
                }
                this.FinalizeCollisionParticleEffect(ref effect);
                this.m_collisionParticles.RemoveAt(index);
            }
        }

        private void UpdateContactCallbackLimit()
        {
            HkRigidBody rigidBody = this.RigidBody;
            if (rigidBody != null)
            {
                int num = 0;
                if (this.m_grid.IsClientPredicted)
                {
                    num = (this.m_grid.BlocksCount <= 5) ? 1 : 10;
                }
                rigidBody.CallbackLimit = num;
            }
        }

        private void UpdateExplosions()
        {
            if (this.m_explosions.Count > 0)
            {
                if (Sync.IsServer)
                {
                    this.m_grid.PerformCutouts(this.m_explosions);
                    float initialSpeed = this.m_grid.Physics.LinearVelocity.Length();
                    foreach (ExplosionInfo info in this.m_explosions)
                    {
                        if (initialSpeed <= 0f)
                        {
                            continue;
                        }
                        if (info.GenerateDebris && (this.m_debrisPerFrame < 3))
                        {
                            MyDebris.Static.CreateDirectedDebris((Vector3) info.Position, this.m_grid.Physics.LinearVelocity / initialSpeed, this.m_grid.GridSize, this.m_grid.GridSize * 1.5f, 0f, 1.570796f, 6, initialSpeed);
                            this.m_debrisPerFrame++;
                        }
                    }
                }
                this.m_explosions.Clear();
            }
        }

        public void UpdateMass()
        {
            if (this.RigidBody.GetMotionType() != HkMotionType.Keyframed)
            {
                float mass = this.RigidBody.Mass;
                this.m_shape.RefreshMass();
                if ((this.RigidBody.Mass != mass) && !this.RigidBody.IsActive)
                {
                    this.RigidBody.Activate();
                }
                this.m_grid.RaisePhysicsChanged();
                MyGridPhysicalHierarchy.Static.UpdateRoot(this.m_grid);
            }
        }

        public unsafe void UpdateShape()
        {
            this.m_debrisPerFrame = 0;
            while (this.m_gridEffects.Count > 0)
            {
                this.CreateEffect(this.m_gridEffects.Dequeue());
            }
            while (this.m_gridCollisionEffects.Count > 0)
            {
                this.CreateGridCollisionEffect(this.m_gridCollisionEffects.Dequeue());
            }
            if (this.m_lastContacts.Count > 0)
            {
                this.m_tmpContactId.Clear();
                foreach (KeyValuePair<ushort, int> pair in this.m_lastContacts)
                {
                    if (MySandboxGame.TotalGamePlayTimeInMilliseconds > (pair.Value + SparksEffectDelayPerContactMs))
                    {
                        this.m_tmpContactId.Add(pair.Key);
                    }
                }
                foreach (ushort num in this.m_tmpContactId)
                {
                    this.m_lastContacts.Remove(num);
                }
            }
            this.UpdateExplosions();
            if (this.m_grid.CanHavePhysics())
            {
                HashSet<Vector3I> dirtyBlocks = this.m_dirtyCubesInfo.DirtyBlocks;
                if (this.m_dirtyCubesInfo.DirtyBlocks.Count <= 100)
                {
                    MyUtils.Swap<HashSet<Vector3I>>(ref this.m_dirtyCubesInfo.DirtyBlocks, ref m_dirtyBlocksSmallCache);
                }
                else
                {
                    MyUtils.Swap<HashSet<Vector3I>>(ref this.m_dirtyCubesInfo.DirtyBlocks, ref m_dirtyBlocksLargeCache);
                }
                this.m_dirtyCubesInfo.DirtyParts.ApplyChanges();
                BoundingBox box = BoundingBox.CreateInvalid();
                bool flag = this.m_dirtyCubesInfo.DirtyParts.Count > 0;
                foreach (BoundingBoxI xi in this.m_dirtyCubesInfo.DirtyParts)
                {
                    Vector3I vectori;
                    vectori.X = xi.Min.X;
                    while (vectori.X <= xi.Max.X)
                    {
                        vectori.Y = xi.Min.Y;
                        while (true)
                        {
                            if (vectori.Y > xi.Max.Y)
                            {
                                int* numPtr3 = (int*) ref vectori.X;
                                numPtr3[0]++;
                                break;
                            }
                            vectori.Z = xi.Min.Z;
                            while (true)
                            {
                                if (vectori.Z > xi.Max.Z)
                                {
                                    int* numPtr2 = (int*) ref vectori.Y;
                                    numPtr2[0]++;
                                    break;
                                }
                                dirtyBlocks.Add(vectori);
                                box = box.Include((Vector3) (vectori * this.m_grid.GridSize));
                                int* numPtr1 = (int*) ref vectori.Z;
                                numPtr1[0]++;
                            }
                        }
                    }
                }
                this.m_dirtyCubesInfo.Clear();
                bool flag2 = dirtyBlocks.Count > 0;
                if (flag2)
                {
                    this.UpdateContactCallbackLimit();
                }
                if (this.m_recreateBody)
                {
                    this.RecreateBreakableBody(dirtyBlocks);
                    this.m_recreateBody = false;
                    this.m_grid.RaisePhysicsChanged();
                }
                else if (flag2)
                {
                    if (this.RigidBody.IsActive && !base.HavokWorld.ActiveRigidBodies.Contains(this.RigidBody))
                    {
                        base.HavokWorld.ActiveRigidBodies.Add(this.RigidBody);
                    }
                    if (flag)
                    {
                        box.Inflate((float) (0.5f + this.m_grid.GridSize));
                        MyPhysics.ActivateInBox(ref box.Transform(this.m_grid.WorldMatrix));
                    }
                    this.m_shape.UnmarkBreakable((base.WeldedRigidBody != null) ? base.WeldedRigidBody : this.RigidBody);
                    this.m_shape.RefreshBlocks((base.WeldedRigidBody != null) ? base.WeldedRigidBody : this.RigidBody, dirtyBlocks);
                    this.OnRefreshComplete();
                }
            }
        }

        public void UpdateTOIOptimizer()
        {
            if (!this.Enabled || (this.RigidBody == null))
            {
                this.m_lastTOIFrame = 0;
            }
            else
            {
                if (this.RigidBody.ReadAndResetToiCounter() >= 1)
                {
                    this.m_lastTOIFrame = MySession.Static.GameplayFrameCounter;
                }
                if (this.m_lastTOIFrame > 0)
                {
                    this.m_lastTOIFrame--;
                }
            }
        }

        public MyTimeSpan PredictedContactLastTime { get; private set; }

        public int PredictedContactsCounter { get; set; }

        public MyGridShape Shape =>
            this.m_shape;

        public bool PredictCollisions
        {
            get
            {
                ulong simulationFrameCounter = MySandboxGame.Static.SimulationFrameCounter;
                if (Volatile.Read(ref this.m_isClientPredictedLastFrameCheck) != simulationFrameCounter)
                {
                    int num1;
                    bool flag = this.m_grid.ClosestParentId != 0L;
                    if (Sync.IsServer || !this.m_grid.IsClientPredicted)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !flag;
                    }
                    bool flag2 = (bool) num1;
                    Volatile.Write(ref this.m_isClientPredicted, flag2);
                    this.m_isClientPredictedLastFrameCheck = simulationFrameCounter;
                }
                return this.m_isClientPredicted;
            }
        }

        public override float Mass =>
            ((this.RigidBody == null) ? 0f : this.RigidBody.Mass);

        public override HkRigidBody RigidBody
        {
            get => 
                base.RigidBody;
            protected set
            {
                base.RigidBody = value;
                this.UpdateContactCallbackLimit();
            }
        }

        public override int HavokCollisionSystemID
        {
            get => 
                base.HavokCollisionSystemID;
            protected set
            {
                if (this.HavokCollisionSystemID != value)
                {
                    base.HavokCollisionSystemID = value;
                    this.m_grid.HavokSystemIDChanged(value);
                }
            }
        }

        public override Vector3 Gravity
        {
            get => 
                this.m_cachedGravity;
            set
            {
                this.m_cachedGravity = value;
                HkRigidBody rigidBody = this.RigidBody;
                if (rigidBody != null)
                {
                    rigidBody.Gravity = value;
                }
            }
        }

        public bool NeedsPerFrameUpdate =>
            ((this.m_gridEffects.Count > 0) || ((this.m_gridCollisionEffects.Count > 0) || ((this.m_lastContacts.Count > 0) || ((this.m_explosions.Count > 0) || ((this.m_collisionParticles.Count > 0) || this.PlanetCrashingNeedsUpdates())))));

        private bool IsTOIOptimized =>
            (this.m_savedQuality != HkCollidableQualityType.Invalid);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridPhysics.<>c <>9 = new MyGridPhysics.<>c();
            public static Action<HkdShapeInstanceInfo> <>9__14_0;
            public static Action<object> <>9__147_0;

            internal void <.cctor>b__233_0(HashSet<Vector3I> x)
            {
                x.Clear();
            }

            internal void <.cctor>b__233_1(Dictionary<MySlimBlock, float> x)
            {
                x.Clear();
            }

            internal void <PerformDeformation>b__147_0(object context)
            {
                MyGridPhysics physics = (MyGridPhysics) context;
                HkRigidBody rigidBody = physics.RigidBody;
                if (rigidBody != null)
                {
                    int num1;
                    bool flag = physics.m_grid.GridSizeEnum == MyCubeSize.Small;
                    float impactDot = physics.m_impactDot;
                    ulong num2 = MySandboxGame.Static.SimulationFrameCounter - physics.m_frameFirstImpact;
                    if (num2 < 100)
                    {
                        num1 = (int) (rigidBody.LinearVelocity.LengthSquared() > 25f);
                    }
                    else
                    {
                        num1 = 0;
                    }
                    bool flag2 = ((num2 > 50) || ((impactDot > 0.8f) && (num2 > 10))) | flag;
                    bool flag3 = (num2 > 100) | flag;
                    if (num1 != 0)
                    {
                        Vector3 angularVelocity = rigidBody.AngularVelocity;
                        rigidBody.AngularVelocity -= (angularVelocity * (1f - impactDot)) / 2f;
                    }
                    if (flag2)
                    {
                        float num3 = 1f - impactDot;
                        float num4 = ((1f - (num3 * num3)) + impactDot) / 1.5f;
                        if (impactDot < 0.5)
                        {
                            num4 /= 2f;
                        }
                        Vector3 linearVelocity = rigidBody.LinearVelocity;
                        float maxLinearVelocity = physics.GetMaxLinearVelocity();
                        float num6 = Math.Min((num4 * linearVelocity.Length()) / (maxLinearVelocity * 1.5f), flag3 ? 0.2f : 0.1f);
                        if (impactDot > 0.5)
                        {
                            num6 *= 1f + (impactDot * 0.5f);
                        }
                        if (!Vector3.IsZero(ref physics.m_cachedGravity))
                        {
                            Vector3 projectedVector = -linearVelocity;
                            Vector3 vector5 = physics.m_cachedGravity.Project(projectedVector);
                            Vector3 vector6 = (vector5 * num6) * 2f;
                            if (flag3)
                            {
                                vector6 += (projectedVector - vector5) * num6;
                            }
                            rigidBody.LinearVelocity += vector6;
                        }
                        else if (flag)
                        {
                            Vector3 vector8 = -linearVelocity;
                            Vector3 vector9 = (Vector3) ((num6 * 1f) * vector8);
                            rigidBody.LinearVelocity += vector9;
                        }
                    }
                }
            }

            internal void <RemoveShapesFromFracturedBlocks>b__14_0(HkdShapeInstanceInfo c)
            {
                string shapeName = c.ShapeName;
                if (!string.IsNullOrEmpty(shapeName))
                {
                    MyGridPhysics.m_tmpShapeNames.Add(shapeName);
                }
            }
        }

        private class CollisionParticleEffect
        {
            public MyParticleEffect Effect;
            public Vector3D RelativePosition;
            public Vector3 Normal;
            public Vector3 SeparatingVelocity;
            public int RemainingTime;
            public float Impulse;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ExplosionInfo
        {
            public Vector3D Position;
            public MyExplosionTypeEnum ExplosionType;
            public float Radius;
            public bool ShowParticles;
            public bool GenerateDebris;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GridCollisionhit
        {
            public Vector3D RelativePosition;
            public Vector3 Normal;
            public Vector3 RelativeVelocity;
            public float SeparatingSpeed;
            public float Impulse;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GridEffect
        {
            public MyGridPhysics.GridEffectType Type;
            public Vector3D Position;
            public Vector3 Normal;
            public float Scale;
            public float SeparatingSpeed;
            public float Impulse;
        }

        private enum GridEffectType
        {
            Collision,
            Destruction,
            Dust
        }

        public class MyDirtyBlocksInfo
        {
            public ConcurrentCachingList<BoundingBoxI> DirtyParts = new ConcurrentCachingList<BoundingBoxI>();
            public HashSet<Vector3I> DirtyBlocks = new HashSet<Vector3I>();

            public void Clear()
            {
                this.DirtyParts.ClearList();
                this.DirtyBlocks.Clear();
            }
        }

        private enum PredictionDisqualificationReason : byte
        {
            None = 0,
            NoEntity = 1,
            EntityIsStatic = 2,
            EntityIsNotMoving = 3
        }
    }
}


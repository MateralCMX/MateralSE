namespace Sandbox.Game.Entities.Cube
{
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.StructuralIntegrity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner, MyCubeBlockType(typeof(MyObjectBuilder_CubeBlock))]
    public class MySlimBlock : IMyDestroyableObject, IMyDecalProxy, VRage.Game.ModAPI.IMySlimBlock, VRage.Game.ModAPI.Ingame.IMySlimBlock
    {
        private static List<VertexArealBoneIndexWeight> m_boneIndexWeightTmp;
        private static MySoundPair CONSTRUCTION_START = new MySoundPair("PrgConstrPh01Start", true);
        private static MySoundPair CONSTRUCTION_PROG = new MySoundPair("PrgConstrPh02Proc", true);
        private static MySoundPair CONSTRUCTION_END = new MySoundPair("PrgConstrPh03Fin", true);
        private static MySoundPair DECONSTRUCTION_START = new MySoundPair("PrgDeconstrPh01Start", true);
        private static MySoundPair DECONSTRUCTION_PROG = new MySoundPair("PrgDeconstrPh02Proc", true);
        private static MySoundPair DECONSTRUCTION_END = new MySoundPair("PrgDeconstrPh03Fin", true);
        [ThreadStatic]
        private static Dictionary<string, int> m_tmpComponentsPerThread;
        [ThreadStatic]
        private static List<MyStockpileItem> m_tmpItemListPerThread;
        [ThreadStatic]
        private static List<Vector3I> m_tmpCubeNeighboursPerThread;
        [ThreadStatic]
        private static List<MySlimBlock> m_tmpBlocksPerThread;
        [ThreadStatic]
        private static List<MySlimBlock> m_tmpMultiBlocksPerThread;
        [CompilerGenerated]
        private static Action<MyTerminalBlock, long> OnAnyBlockHackedChanged;
        public static readonly MyTimedItemCache ConstructionParticlesTimedCache = new MyTimedItemCache(350);
        public static double ConstructionParticleSpaceMapping = 1.0;
        private float m_accumulatedDamage;
        public MyCubeBlockDefinition BlockDefinition;
        public Vector3I Min;
        public Vector3I Max;
        public MyBlockOrientation Orientation = MyBlockOrientation.Identity;
        public Vector3I Position;
        public float BlockGeneralDamageModifier = 1f;
        private MyCubeGrid m_cubeGrid;
        public Vector3 ColorMaskHSV;
        public MyStringHash SkinSubtypeId;
        public float Dithering;
        public bool UsesDeformation = true;
        public float DeformationRatio = 1f;
        private MyComponentStack m_componentStack;
        private MyConstructionStockpile m_stockpile;
        private float m_cachedMaxDeformation;
        private long m_builtByID;
        public List<MySlimBlock> Neighbours = new List<MySlimBlock>();
        [CompilerGenerated]
        private Action<MySlimBlock, MyCubeGrid> CubeGridChanged;
        public float m_lastDamage;
        public long m_lastAttackerId;
        public MyStringHash m_lastDamageType = MyDamageType.Unknown;
        public MyMultiBlockDefinition MultiBlockDefinition;
        public int MultiBlockId;
        public int MultiBlockIndex = -1;
        private static readonly Dictionary<string, int> m_modelTotalFracturesCount = new Dictionary<string, int>();
        public List<Vector3I> DisconnectFaces = new List<Vector3I>();
        [ThreadStatic]
        private static List<uint> m_tmpIds;
        [ThreadStatic]
        private static List<MyTuple<Vector3I, float>> m_batchCache;

        public event Action<MySlimBlock, MyCubeGrid> CubeGridChanged
        {
            [CompilerGenerated] add
            {
                Action<MySlimBlock, MyCubeGrid> cubeGridChanged = this.CubeGridChanged;
                while (true)
                {
                    Action<MySlimBlock, MyCubeGrid> a = cubeGridChanged;
                    Action<MySlimBlock, MyCubeGrid> action3 = (Action<MySlimBlock, MyCubeGrid>) Delegate.Combine(a, value);
                    cubeGridChanged = Interlocked.CompareExchange<Action<MySlimBlock, MyCubeGrid>>(ref this.CubeGridChanged, action3, a);
                    if (ReferenceEquals(cubeGridChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySlimBlock, MyCubeGrid> cubeGridChanged = this.CubeGridChanged;
                while (true)
                {
                    Action<MySlimBlock, MyCubeGrid> source = cubeGridChanged;
                    Action<MySlimBlock, MyCubeGrid> action3 = (Action<MySlimBlock, MyCubeGrid>) Delegate.Remove(source, value);
                    cubeGridChanged = Interlocked.CompareExchange<Action<MySlimBlock, MyCubeGrid>>(ref this.CubeGridChanged, action3, source);
                    if (ReferenceEquals(cubeGridChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyTerminalBlock, long> OnAnyBlockHackedChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock, long> onAnyBlockHackedChanged = OnAnyBlockHackedChanged;
                while (true)
                {
                    Action<MyTerminalBlock, long> a = onAnyBlockHackedChanged;
                    Action<MyTerminalBlock, long> action3 = (Action<MyTerminalBlock, long>) Delegate.Combine(a, value);
                    onAnyBlockHackedChanged = Interlocked.CompareExchange<Action<MyTerminalBlock, long>>(ref OnAnyBlockHackedChanged, action3, a);
                    if (ReferenceEquals(onAnyBlockHackedChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock, long> onAnyBlockHackedChanged = OnAnyBlockHackedChanged;
                while (true)
                {
                    Action<MyTerminalBlock, long> source = onAnyBlockHackedChanged;
                    Action<MyTerminalBlock, long> action3 = (Action<MyTerminalBlock, long>) Delegate.Remove(source, value);
                    onAnyBlockHackedChanged = Interlocked.CompareExchange<Action<MyTerminalBlock, long>>(ref OnAnyBlockHackedChanged, action3, source);
                    if (ReferenceEquals(onAnyBlockHackedChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MySlimBlock()
        {
            this.UniqueId = MyRandom.Instance.Next();
            this.UseDamageSystem = true;
        }

        private void AcquireUnneededStockpileItems(List<MyStockpileItem> outputList)
        {
            if (this.m_stockpile != null)
            {
                foreach (MyStockpileItem item in this.m_stockpile.GetItems())
                {
                    bool flag = false;
                    MyCubeBlockDefinition.Component[] components = this.BlockDefinition.Components;
                    int index = 0;
                    while (true)
                    {
                        if (index >= components.Length)
                        {
                            if (!flag)
                            {
                                outputList.Add(item);
                            }
                            break;
                        }
                        if (components[index].Definition.Id.SubtypeId == item.Content.SubtypeId)
                        {
                            flag = true;
                        }
                        index++;
                    }
                }
            }
        }

        public void AddAuthorship()
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.m_builtByID);
            if (identity == null)
            {
                long builtByID = this.m_builtByID;
            }
            else
            {
                int pcu = 1;
                if (this.ComponentStack.IsFunctional)
                {
                    pcu = this.BlockDefinition.PCU;
                }
                identity.BlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
                if (!ReferenceEquals(identity.BlockLimits, MySession.Static.GlobalBlockLimits) && !ReferenceEquals(identity.BlockLimits, MySession.Static.PirateBlockLimits))
                {
                    MySession.Static.GlobalBlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
                }
            }
        }

        private static void AddBlockComponent(MyHudBlockInfo hudInfo, MyComponentStack.GroupInfo groupInfo, MyInventoryBase availableInventory)
        {
            MyHudBlockInfo.ComponentInfo item = new MyHudBlockInfo.ComponentInfo {
                DefinitionId = groupInfo.Component.Id,
                ComponentName = groupInfo.Component.DisplayNameText,
                Icons = groupInfo.Component.Icons,
                TotalCount = groupInfo.TotalCount,
                MountedCount = groupInfo.MountedCount
            };
            if (availableInventory != null)
            {
                item.AvailableAmount = (int) MyCubeBuilder.BuildComponent.GetItemAmountCombined(availableInventory, groupInfo.Component.Id);
            }
            hudInfo.Components.Add(item);
        }

        private void AddNeighbour(Vector3I pos, Vector3I dir)
        {
            MySlimBlock cubeBlock = this.CubeGrid.GetCubeBlock((Vector3I) (pos + dir));
            if ((cubeBlock != null) && !ReferenceEquals(cubeBlock, this))
            {
                if (!MyFakes.ENABLE_COMPOUND_BLOCKS)
                {
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = this.BlockDefinition.GetBuildProgressModelMountPoints(this.BuildLevelRatio);
                    MyCubeBlockDefinition.MountPoint[] mountPointsB = cubeBlock.BlockDefinition.GetBuildProgressModelMountPoints(cubeBlock.BuildLevelRatio);
                    if ((MyCubeGrid.CheckMountPointsForSide(this.BlockDefinition, buildProgressModelMountPoints, ref this.Orientation, ref this.Position, ref dir, cubeBlock.BlockDefinition, mountPointsB, ref cubeBlock.Orientation, ref cubeBlock.Position) && this.ConnectionAllowed(ref pos, ref dir, cubeBlock)) && !this.Neighbours.Contains(cubeBlock))
                    {
                        this.Neighbours.Add(cubeBlock);
                        cubeBlock.Neighbours.Add(this);
                    }
                }
                else if (!this.Neighbours.Contains(cubeBlock))
                {
                    MyCompoundCubeBlock fatBlock = this.FatBlock as MyCompoundCubeBlock;
                    MyCompoundCubeBlock block3 = cubeBlock.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock != null)
                    {
                        ListReader<MySlimBlock> blocks = fatBlock.GetBlocks();
                        using (List<MySlimBlock>.Enumerator enumerator = blocks.GetEnumerator())
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    MySlimBlock current = enumerator.Current;
                                    MyCubeBlockDefinition.MountPoint[] thisMountPoints = current.BlockDefinition.GetBuildProgressModelMountPoints(current.BuildLevelRatio);
                                    if (block3 != null)
                                    {
                                        using (List<MySlimBlock>.Enumerator enumerator2 = block3.GetBlocks().GetEnumerator())
                                        {
                                            while (true)
                                            {
                                                if (enumerator2.MoveNext())
                                                {
                                                    MySlimBlock otherBlock = enumerator2.Current;
                                                    MyCubeBlockDefinition.MountPoint[] otherMountPoints = otherBlock.BlockDefinition.GetBuildProgressModelMountPoints(otherBlock.BuildLevelRatio);
                                                    if (!AddNeighbour(ref dir, current, thisMountPoints, otherBlock, otherMountPoints, this, cubeBlock))
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            break;
                                        }
                                    }
                                    if (!AddNeighbour(ref dir, current, thisMountPoints, cubeBlock, cubeBlock.BlockDefinition.GetBuildProgressModelMountPoints(cubeBlock.BuildLevelRatio), this, cubeBlock))
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            return;
                        }
                    }
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = this.BlockDefinition.GetBuildProgressModelMountPoints(this.BuildLevelRatio);
                    if (block3 == null)
                    {
                        AddNeighbour(ref dir, this, buildProgressModelMountPoints, cubeBlock, cubeBlock.BlockDefinition.GetBuildProgressModelMountPoints(cubeBlock.BuildLevelRatio), this, cubeBlock);
                    }
                    else
                    {
                        foreach (MySlimBlock block6 in block3.GetBlocks())
                        {
                            MyCubeBlockDefinition.MountPoint[] otherMountPoints = block6.BlockDefinition.GetBuildProgressModelMountPoints(block6.BuildLevelRatio);
                            if (AddNeighbour(ref dir, this, buildProgressModelMountPoints, block6, otherMountPoints, this, cubeBlock))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static bool AddNeighbour(ref Vector3I dir, MySlimBlock thisBlock, MyCubeBlockDefinition.MountPoint[] thisMountPoints, MySlimBlock otherBlock, MyCubeBlockDefinition.MountPoint[] otherMountPoints, MySlimBlock thisParentBlock, MySlimBlock otherParentBlock)
        {
            if (!MyCubeGrid.CheckMountPointsForSide(thisBlock.BlockDefinition, thisMountPoints, ref thisBlock.Orientation, ref thisBlock.Position, ref dir, otherBlock.BlockDefinition, otherMountPoints, ref otherBlock.Orientation, ref otherBlock.Position) || !thisBlock.ConnectionAllowed(ref otherBlock.Position, ref dir, otherBlock))
            {
                return false;
            }
            thisParentBlock.Neighbours.Add(otherParentBlock);
            otherParentBlock.Neighbours.Add(thisParentBlock);
            return true;
        }

        public void AddNeighbours()
        {
            this.AddNeighbours(this.Min, new Vector3I(this.Min.X, this.Max.Y, this.Max.Z), -Vector3I.UnitX);
            this.AddNeighbours(this.Min, new Vector3I(this.Max.X, this.Min.Y, this.Max.Z), -Vector3I.UnitY);
            this.AddNeighbours(this.Min, new Vector3I(this.Max.X, this.Max.Y, this.Min.Z), -Vector3I.UnitZ);
            this.AddNeighbours(new Vector3I(this.Max.X, this.Min.Y, this.Min.Z), this.Max, Vector3I.UnitX);
            this.AddNeighbours(new Vector3I(this.Min.X, this.Max.Y, this.Min.Z), this.Max, Vector3I.UnitY);
            this.AddNeighbours(new Vector3I(this.Min.X, this.Min.Y, this.Max.Z), this.Max, Vector3I.UnitZ);
            if (this.FatBlock != null)
            {
                this.FatBlock.OnAddedNeighbours();
            }
        }

        private unsafe void AddNeighbours(Vector3I min, Vector3I max, Vector3I normalDirection)
        {
            Vector3I vectori;
            vectori.X = min.X;
            while (vectori.X <= max.X)
            {
                vectori.Y = min.Y;
                while (true)
                {
                    if (vectori.Y > max.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = min.Z;
                    while (true)
                    {
                        if (vectori.Z > max.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        this.AddNeighbour(vectori, normalDirection);
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
        }

        public void ApplyAccumulatedDamage(bool addDirtyParts = true, long attackerId = 0L)
        {
            if (MySession.Static.SurvivalMode)
            {
                this.EnsureConstructionStockpileExists();
            }
            float integrity = this.Integrity;
            if (this.m_stockpile == null)
            {
                this.m_componentStack.ApplyDamage(this.AccumulatedDamage, null);
            }
            else
            {
                this.m_stockpile.ClearSyncList();
                this.m_componentStack.ApplyDamage(this.AccumulatedDamage, this.m_stockpile);
                if (Sync.IsServer)
                {
                    this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                }
                this.m_stockpile.ClearSyncList();
            }
            if ((!this.BlockDefinition.RatioEnoughForDamageEffect(integrity / this.MaxIntegrity) && this.BlockDefinition.RatioEnoughForDamageEffect(this.Integrity / this.MaxIntegrity)) && (this.FatBlock != null))
            {
                this.FatBlock.OnIntegrityChanged(this.BuildIntegrity, this.Integrity, false, MySession.Static.LocalPlayerId, MyOwnershipShareModeEnum.Faction);
            }
            this.AccumulatedDamage = 0f;
            if (this.m_componentStack.IsDestroyed)
            {
                if (MyFakes.SHOW_DAMAGE_EFFECTS && (this.FatBlock != null))
                {
                    this.FatBlock.SetDamageEffect(false);
                }
                this.CubeGrid.RemoveDestroyedBlock(this, attackerId);
                if (addDirtyParts)
                {
                    this.CubeGrid.Physics.AddDirtyBlock(this);
                }
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseDestroyed(this, new MyDamageInformation(false, this.m_lastDamage, this.m_lastDamageType, this.m_lastAttackerId));
                }
            }
        }

        public void ApplyDestructionDamage(float integrityRatioFromFracturedPieces)
        {
            if ((MyFakes.ENABLE_FRACTURE_COMPONENT && Sync.IsServer) && MyPerGameSettings.Destruction)
            {
                MyHitInfo? nullable;
                float damage = (this.ComponentStack.IntegrityRatio - integrityRatioFromFracturedPieces) * this.BlockDefinition.MaxIntegrity;
                if (this.CanApplyDestructionDamage(damage))
                {
                    nullable = null;
                    this.DoDamage(damage, MyDamageType.Destruction, true, nullable, 0L);
                }
                else if (this.CanApplyDestructionDamage(MyDefinitionManager.Static.DestructionDefinition.DestructionDamage))
                {
                    nullable = null;
                    this.DoDamage(MyDefinitionManager.Static.DestructionDefinition.DestructionDamage, MyDamageType.Destruction, true, nullable, 0L);
                }
            }
        }

        public string CalculateCurrentModel(out Matrix orientation)
        {
            float buildLevelRatio = this.BuildLevelRatio;
            this.Orientation.GetMatrix(out orientation);
            if (((buildLevelRatio < 1f) && (this.BlockDefinition.BuildProgressModels != null)) && (this.BlockDefinition.BuildProgressModels.Length != 0))
            {
                for (int i = 0; i < this.BlockDefinition.BuildProgressModels.Length; i++)
                {
                    if (this.BlockDefinition.BuildProgressModels[i].BuildRatioUpperBound >= buildLevelRatio)
                    {
                        if (this.BlockDefinition.BuildProgressModels[i].RandomOrientation)
                        {
                            orientation = MyCubeGridDefinitions.AllPossible90rotations[Math.Abs(this.Position.GetHashCode()) % MyCubeGridDefinitions.AllPossible90rotations.Length].GetFloatMatrix();
                        }
                        return this.BlockDefinition.BuildProgressModels[i].File;
                    }
                }
            }
            return ((this.FatBlock != null) ? this.FatBlock.CalculateCurrentModel(out orientation) : this.BlockDefinition.Model);
        }

        public int CalculateCurrentModelID()
        {
            float buildLevelRatio = this.BuildLevelRatio;
            if (((buildLevelRatio < 1f) && (this.BlockDefinition.BuildProgressModels != null)) && (this.BlockDefinition.BuildProgressModels.Length != 0))
            {
                for (int i = 0; i < this.BlockDefinition.BuildProgressModels.Length; i++)
                {
                    if (this.BlockDefinition.BuildProgressModels[i].BuildRatioUpperBound >= buildLevelRatio)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private bool CanApplyDestructionDamage(float damage)
        {
            if (damage <= 0f)
            {
                return false;
            }
            if (!this.IsMultiBlockPart)
            {
                damage *= this.DamageRatio;
                damage *= this.DeformationRatio;
                damage += this.AccumulatedDamage;
                return ((this.Integrity - damage) > 1.525902E-05f);
            }
            MyCubeGridMultiBlockInfo multiBlockInfo = this.CubeGrid.GetMultiBlockInfo(this.MultiBlockId);
            if (multiBlockInfo == null)
            {
                return false;
            }
            float totalMaxIntegrity = multiBlockInfo.GetTotalMaxIntegrity();
            using (HashSet<MySlimBlock>.Enumerator enumerator = multiBlockInfo.Blocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    float num2 = ((((damage * current.MaxIntegrity) / totalMaxIntegrity) * current.DamageRatio) * current.DeformationRatio) + current.AccumulatedDamage;
                    if ((current.Integrity - num2) <= 1.525902E-05f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CanContinueBuild(MyInventoryBase sourceInventory)
        {
            if (this.IsFullIntegrity || ((sourceInventory == null) && !MySession.Static.CreativeMode))
            {
                return false;
            }
            return (((this.FatBlock == null) || this.FatBlock.CanContinueBuild()) && this.m_componentStack.CanContinueBuild(sourceInventory, this.m_stockpile));
        }

        internal void ChangeStockpile(List<MyStockpileItem> items)
        {
            this.EnsureConstructionStockpileExists();
            this.m_stockpile.Change(items);
            if (this.m_stockpile.IsEmpty())
            {
                this.ReleaseConstructionStockpile();
            }
        }

        public void ClearConstructionStockpile(MyInventoryBase outputInventory)
        {
            if (!this.StockpileEmpty)
            {
                VRage.Game.Entity.MyEntity entity = null;
                if ((outputInventory != null) && (outputInventory.Container != null))
                {
                    entity = outputInventory.Container.Entity as VRage.Game.Entity.MyEntity;
                }
                if ((entity != null) && (entity.InventoryOwnerType() == MyInventoryOwnerTypeEnum.Character))
                {
                    this.MoveItemsFromConstructionStockpile(outputInventory, MyItemFlags.None);
                }
                else
                {
                    this.m_stockpile.ClearSyncList();
                    m_tmpItemList.Clear();
                    foreach (MyStockpileItem item in this.m_stockpile.GetItems())
                    {
                        m_tmpItemList.Add(item);
                    }
                    foreach (MyStockpileItem item2 in m_tmpItemList)
                    {
                        this.RemoveFromConstructionStockpile(item2);
                    }
                    this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                    this.m_stockpile.ClearSyncList();
                }
            }
            this.ReleaseConstructionStockpile();
        }

        public static unsafe void ComputeMax(MyCubeBlockDefinition definition, MyBlockOrientation orientation, ref Vector3I min, out Vector3I max)
        {
            Vector3I result = definition.Size - 1;
            MatrixI matrix = new MatrixI(orientation);
            Vector3I* vectoriPtr1 = (Vector3I*) ref result;
            Vector3I.TransformNormal(ref (Vector3I) ref vectoriPtr1, ref matrix, out result);
            Vector3I* vectoriPtr2 = (Vector3I*) ref result;
            Vector3I.Abs(ref (Vector3I) ref vectoriPtr2, out result);
            max = (Vector3I) (min + result);
        }

        public static unsafe Vector3I ComputePositionInGrid(MatrixI localMatrix, MyCubeBlockDefinition blockDefinition, Vector3I min)
        {
            Vector3I vectori3;
            Vector3I vectori4;
            Vector3I center = blockDefinition.Center;
            Vector3I.TransformNormal(ref blockDefinition.Size - 1, ref localMatrix, out vectori3);
            Vector3I.TransformNormal(ref center, ref localMatrix, out vectori4);
            Vector3I vectori5 = Vector3I.Abs(vectori3);
            Vector3I vectori6 = (Vector3I) (vectori4 + min);
            if (vectori3.X != vectori5.X)
            {
                int* numPtr1 = (int*) ref vectori6.X;
                numPtr1[0] += vectori5.X;
            }
            if (vectori3.Y != vectori5.Y)
            {
                int* numPtr2 = (int*) ref vectori6.Y;
                numPtr2[0] += vectori5.Y;
            }
            if (vectori3.Z != vectori5.Z)
            {
                int* numPtr3 = (int*) ref vectori6.Z;
                numPtr3[0] += vectori5.Z;
            }
            return vectori6;
        }

        public void ComputeScaledCenter(out Vector3D scaledCenter)
        {
            scaledCenter = (this.Max + this.Min) * this.CubeGrid.GridSizeHalf;
        }

        public void ComputeScaledHalfExtents(out Vector3 scaledHalfExtents)
        {
            scaledHalfExtents = ((this.Max + 1) - this.Min) * this.CubeGrid.GridSizeHalf;
        }

        public void ComputeWorldCenter(out Vector3D worldCenter)
        {
            this.ComputeScaledCenter(out worldCenter);
            MatrixD worldMatrix = this.CubeGrid.WorldMatrix;
            Vector3D.Transform(ref worldCenter, ref worldMatrix, out worldCenter);
        }

        private bool ConnectionAllowed(ref Vector3I otherBlockPos, ref Vector3I faceNormal, MySlimBlock other)
        {
            if ((MyStructuralIntegrity.Enabled && (this.CubeGrid.StructuralIntegrity != null)) && !this.CubeGrid.StructuralIntegrity.IsConnectionFine(this, other))
            {
                return false;
            }
            return (((this.DisconnectFaces.Count <= 0) || !this.DisconnectFaces.Contains(faceNormal)) && ((this.FatBlock == null) || (!this.FatBlock.CheckConnectionAllowed || this.FatBlock.ConnectionAllowed(ref otherBlockPos, ref faceNormal, other.BlockDefinition))));
        }

        private unsafe void CreateConstructionSmokes()
        {
            Vector3 vector = new Vector3(this.CubeGrid.GridSize) / 2f;
            BoundingBox box = new BoundingBox(((Vector3) (this.Min * this.CubeGrid.GridSize)) - vector, (this.Max * this.CubeGrid.GridSize) + vector);
            if ((this.FatBlock != null) && (this.FatBlock.Model != null))
            {
                Matrix matrix;
                BoundingBox box2 = new BoundingBox(this.FatBlock.Model.BoundingBox.Min, this.FatBlock.Model.BoundingBox.Max);
                this.FatBlock.Orientation.GetMatrix(out matrix);
                BoundingBox box3 = BoundingBox.CreateInvalid();
                Vector3[] corners = box2.GetCorners();
                int index = 0;
                while (true)
                {
                    if (index >= corners.Length)
                    {
                        BoundingBox* boxPtr1 = (BoundingBox*) ref box;
                        boxPtr1 = (BoundingBox*) new BoundingBox(box3.Min + box.Center, box3.Max + box.Center);
                        break;
                    }
                    Vector3 position = corners[index];
                    box3 = box3.Include(Vector3.Transform(position, matrix));
                    index++;
                }
            }
            if (!ConstructionParticlesTimedCache.IsPlaceUsed(this.WorldPosition, ConstructionParticleSpaceMapping, MySandboxGame.TotalSimulationTimeInMilliseconds, true))
            {
                box.Inflate((float) -0.3f);
                Vector3[] corners = box.GetCorners();
                float num = 0.25f;
                int index = 0;
                while (index < MyOrientedBoundingBox.StartVertices.Length)
                {
                    Vector3 vector3 = corners[MyOrientedBoundingBox.StartVertices[index]];
                    float num4 = 0f;
                    float num5 = Vector3.Distance(vector3, corners[MyOrientedBoundingBox.EndVertices[index]]);
                    Vector3 vector4 = (Vector3) (num * Vector3.Normalize(corners[MyOrientedBoundingBox.EndVertices[index]] - corners[MyOrientedBoundingBox.StartVertices[index]]));
                    while (true)
                    {
                        MyParticleEffect effect;
                        if (num4 >= num5)
                        {
                            index++;
                            break;
                        }
                        Vector3D position = Vector3D.Transform(vector3, this.CubeGrid.WorldMatrix);
                        if (MyParticlesManager.TryCreateParticleEffect("Smoke_Construction", MatrixD.CreateTranslation(position), out effect))
                        {
                            effect.Velocity = this.CubeGrid.Physics.LinearVelocity;
                        }
                        num4 += num;
                        vector3 += vector4;
                    }
                }
            }
        }

        private void DeconstructStockpile(float deconstructAmount, MyInventoryBase outputInventory, bool useDefaultDeconstructEfficiency = false)
        {
            if (MySession.Static.CreativeMode)
            {
                this.ClearConstructionStockpile(outputInventory);
            }
            else
            {
                this.EnsureConstructionStockpileExists();
            }
            if (this.m_stockpile == null)
            {
                this.m_componentStack.DecreaseMountLevel(deconstructAmount, null, useDefaultDeconstructEfficiency);
            }
            else
            {
                this.m_stockpile.ClearSyncList();
                this.m_componentStack.DecreaseMountLevel(deconstructAmount, this.m_stockpile, useDefaultDeconstructEfficiency);
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
        }

        public void DecreaseMountLevel(float grinderAmount, MyInventoryBase outputInventory, bool useDefaultDeconstructEfficiency = false)
        {
            if (Sync.IsServer && !this.m_componentStack.IsFullyDismounted)
            {
                grinderAmount = (this.FatBlock == null) ? (grinderAmount / this.BlockDefinition.DisassembleRatio) : (grinderAmount / this.FatBlock.DisassembleRatio);
                grinderAmount *= this.BlockDefinition.IntegrityPointsPerSec;
                float buildRatio = this.m_componentStack.BuildRatio;
                this.DeconstructStockpile(grinderAmount, outputInventory, useDefaultDeconstructEfficiency);
                float newRatio = (this.BuildIntegrity - grinderAmount) / this.BlockDefinition.MaxIntegrity;
                if ((this.BlockDefinition.RatioEnoughForDamageEffect(this.BuildLevelRatio) && (this.FatBlock != null)) && (this.FatBlock.OwnerId != 0))
                {
                    this.FatBlock.OnIntegrityChanged(this.BuildIntegrity, this.Integrity, false, 0L, MyOwnershipShareModeEnum.Faction);
                }
                long grinderOwner = 0L;
                if ((outputInventory != null) && (outputInventory.Entity != null))
                {
                    MyIDModule module;
                    IMyComponentOwner<MyIDModule> entity = outputInventory.Entity as IMyComponentOwner<MyIDModule>;
                    if ((entity != null) && entity.GetComponent(out module))
                    {
                        grinderOwner = module.Owner;
                    }
                }
                this.UpdateHackingIndicator(newRatio, buildRatio, grinderOwner);
                MyIntegrityChangeEnum damage = MyIntegrityChangeEnum.Damage;
                if (this.BlockDefinition.ModelChangeIsNeeded(this.m_componentStack.BuildRatio, buildRatio))
                {
                    this.UpdateVisual(true);
                    if (this.FatBlock != null)
                    {
                        int num4 = this.CalculateCurrentModelID();
                        if ((num4 == -1) || (this.BuildLevelRatio == 0f))
                        {
                            damage = MyIntegrityChangeEnum.DeconstructionEnd;
                        }
                        else
                        {
                            damage = (num4 != (this.BlockDefinition.BuildProgressModels.Length - 1)) ? MyIntegrityChangeEnum.DeconstructionProcess : MyIntegrityChangeEnum.DeconstructionBegin;
                        }
                        this.FatBlock.SetDamageEffect(false);
                    }
                    this.PlayConstructionSound(damage, true);
                    this.CreateConstructionSmokes();
                    if (this.CubeGrid.GridSystems.GasSystem != null)
                    {
                        this.CubeGrid.GridSystems.GasSystem.OnSlimBlockBuildRatioLowered(this);
                    }
                }
                if ((MyFakes.ENABLE_GENERATED_BLOCKS && (!this.BlockDefinition.IsGeneratedBlock && (this.BlockDefinition.GeneratedBlockDefinitions != null))) && (this.BlockDefinition.GeneratedBlockDefinitions.Length != 0))
                {
                    this.UpdateProgressGeneratedBlocks(buildRatio);
                }
                this.CubeGrid.SendIntegrityChanged(this, damage, grinderOwner);
                this.CubeGrid.OnIntegrityChanged(this, false);
            }
        }

        public void DecreaseMountLevelToDesiredRatio(float desiredIntegrityRatio, MyInventoryBase outputInventory)
        {
            float num = desiredIntegrityRatio * this.MaxIntegrity;
            float num2 = this.Integrity - num;
            if (num2 > 0f)
            {
                num2 = (this.FatBlock == null) ? (num2 * this.BlockDefinition.DisassembleRatio) : (num2 * this.FatBlock.DisassembleRatio);
                this.DecreaseMountLevel(num2 / this.BlockDefinition.IntegrityPointsPerSec, outputInventory, true);
            }
        }

        public void DisableLastComponentYield()
        {
            this.m_componentStack.DisableLastComponentYield();
        }

        public bool DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId)
        {
            if (damage <= 0f)
            {
                return false;
            }
            if (!sync)
            {
                this.DoDamage(damage, damageType, hitInfo, true, attackerId);
            }
            else if (Sync.IsServer)
            {
                DoDamageSynced(this, damage, damageType, hitInfo, attackerId);
            }
            return true;
        }

        public void DoDamage(float damage, MyStringHash damageType, MyHitInfo? hitInfo = new MyHitInfo?(), bool addDirtyParts = true, long attackerId = 0L)
        {
            if (this.CubeGrid.BlocksDestructionEnabled || this.ForceBlockDestructible)
            {
                damage = ((damage * this.BlockGeneralDamageModifier) * this.CubeGrid.GridGeneralDamageModifier) * this.BlockDefinition.GeneralDamageMultiplier;
                this.DoDamageInternal(damage, damageType, addDirtyParts, hitInfo, attackerId);
            }
        }

        private void DoDamageInternal(float damage, MyStringHash damageType, bool addDirtyParts = true, MyHitInfo? hitInfo = new MyHitInfo?(), long attackerId = 0L)
        {
            damage *= this.DamageRatio;
            if (MySessionComponentSafeZones.IsActionAllowed(this.CubeGrid, MySafeZoneAction.Damage, 0L))
            {
                if (MyPerGameSettings.Destruction || MyFakes.ENABLE_VR_BLOCK_DEFORMATION_RATIO)
                {
                    damage *= this.DeformationRatio;
                }
                try
                {
                    if (((this.FatBlock != null) && (!this.FatBlock.Closed && (this.CubeGrid.Physics != null))) && this.CubeGrid.Physics.Enabled)
                    {
                        IMyDestroyableObject fatBlock = this.FatBlock as IMyDestroyableObject;
                        if (fatBlock != null)
                        {
                            MyHitInfo? nullable = null;
                            if (fatBlock.DoDamage(damage, damageType, false, nullable, attackerId))
                            {
                                return;
                            }
                        }
                    }
                }
                catch
                {
                }
                MyDamageInformation info = new MyDamageInformation(false, damage, damageType, attackerId);
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseBeforeDamageApplied(this, ref info);
                    damage = info.Amount;
                }
                MySession @static = MySession.Static;
                @static.NegativeIntegrityTotal += damage;
                this.AccumulatedDamage += damage;
                if ((this.m_componentStack.Integrity - this.AccumulatedDamage) <= 1.525902E-05f)
                {
                    this.ApplyAccumulatedDamage(addDirtyParts, attackerId);
                    this.CubeGrid.RemoveFromDamageApplication(this);
                }
                else if ((MyFakes.SHOW_DAMAGE_EFFECTS && ((this.FatBlock != null) && (!this.FatBlock.Closed && !this.BlockDefinition.RatioEnoughForDamageEffect(this.BuildIntegrity / this.MaxIntegrity)))) && this.BlockDefinition.RatioEnoughForDamageEffect((this.Integrity - damage) / this.MaxIntegrity))
                {
                    this.FatBlock.SetDamageEffect(true);
                }
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseAfterDamageApplied(this, info);
                }
                this.m_lastDamage = damage;
                this.m_lastAttackerId = attackerId;
                this.m_lastDamageType = damageType;
            }
        }

        [Event(null, 0xaab), Reliable, Broadcast]
        private static void DoDamageSlimBlock(DoDamageSlimBlockMsg msg)
        {
            MyCubeGrid grid;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(msg.GridEntityId, out grid, false))
            {
                MySlimBlock cubeBlock = grid.GetCubeBlock(msg.Position);
                if ((cubeBlock != null) && !cubeBlock.IsDestroyed)
                {
                    if ((msg.CompoundBlockId != uint.MaxValue) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
                    {
                        MySlimBlock block = (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlock((ushort) msg.CompoundBlockId);
                        if (block != null)
                        {
                            cubeBlock = block;
                        }
                    }
                    cubeBlock.DoDamage(msg.Damage, msg.Type, msg.HitInfo, true, msg.AttackerEntityId);
                }
            }
        }

        [Event(null, 0xa7f), Reliable, Broadcast]
        private static void DoDamageSlimBlockBatch(long gridId, List<MyTuple<Vector3I, float>> blocks, MyStringHash damageType, long attackerId)
        {
            MyCubeGrid grid;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(gridId, out grid, false))
            {
                foreach (MyTuple<Vector3I, float> tuple in blocks)
                {
                    MySlimBlock cubeBlock = grid.GetCubeBlock(tuple.Item1);
                    if ((cubeBlock == null) || cubeBlock.IsDestroyed)
                    {
                        break;
                    }
                    MyHitInfo? hitInfo = null;
                    cubeBlock.DoDamage(tuple.Item2, damageType, hitInfo, true, attackerId);
                }
            }
        }

        private static void DoDamageSynced(MySlimBlock block, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            SendDamage(block, damage, damageType, hitInfo, attackerId);
            block.DoDamage(damage, damageType, hitInfo, true, attackerId);
        }

        private void EnsureConstructionStockpileExists()
        {
            if (this.m_stockpile == null)
            {
                this.m_stockpile = new MyConstructionStockpile();
            }
        }

        public void FillConstructionStockpile()
        {
            if (!MySession.Static.CreativeMode)
            {
                this.EnsureConstructionStockpileExists();
                bool flag = false;
                int index = 0;
                while (true)
                {
                    if (index >= this.ComponentStack.GroupCount)
                    {
                        if (flag)
                        {
                            this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                        }
                        break;
                    }
                    MyComponentStack.GroupInfo groupInfo = this.ComponentStack.GetGroupInfo(index);
                    int count = groupInfo.TotalCount - groupInfo.MountedCount;
                    if (count > 0)
                    {
                        this.m_stockpile.AddItems(count, groupInfo.Component.Id, MyItemFlags.None);
                        flag = true;
                    }
                    index++;
                }
            }
        }

        public void FixBones(float oldDamage, float maxAllowedBoneMovement)
        {
            float factor = this.CurrentDamage / oldDamage;
            if (oldDamage == 0f)
            {
                factor = 0f;
            }
            float num2 = (1f - factor) * this.MaxDeformation;
            if ((this.MaxDeformation != 0f) && (num2 > maxAllowedBoneMovement))
            {
                factor = 1f - (maxAllowedBoneMovement / this.MaxDeformation);
            }
            if (factor == 0f)
            {
                this.CubeGrid.ResetBlockSkeleton(this, true);
            }
            if (factor > 0f)
            {
                this.CubeGrid.MultiplyBlockSkeleton(this, factor, true);
            }
        }

        public void FullyDismount(MyInventory outputInventory)
        {
            if (Sync.IsServer)
            {
                this.DeconstructStockpile(this.BuildIntegrity, outputInventory, false);
                float buildRatio = this.m_componentStack.BuildRatio;
                if (this.BlockDefinition.ModelChangeIsNeeded(this.m_componentStack.BuildRatio, buildRatio))
                {
                    this.UpdateVisual(true);
                    this.PlayConstructionSound(MyIntegrityChangeEnum.DeconstructionEnd, true);
                    this.CreateConstructionSmokes();
                    if (this.CubeGrid.GridSystems.GasSystem != null)
                    {
                        this.CubeGrid.GridSystems.GasSystem.OnSlimBlockBuildRatioLowered(this);
                    }
                }
            }
        }

        public int GetConstructionStockpileItemAmount(MyDefinitionId id) => 
            ((this.m_stockpile != null) ? this.m_stockpile.GetItemAmount(id, MyItemFlags.None) : 0);

        internal void GetConstructionStockpileItems(List<MyStockpileItem> m_cacheStockpileItems)
        {
            if (this.m_stockpile != null)
            {
                foreach (MyStockpileItem item in this.m_stockpile.GetItems())
                {
                    m_cacheStockpileItems.Add(item);
                }
            }
        }

        public MyObjectBuilder_CubeBlock GetCopyObjectBuilder() => 
            this.GetObjectBuilderInternal(true);

        public MyFractureComponentCubeBlock GetFractureComponent() => 
            this.FatBlock?.GetFractureComponent();

        public void GetLocalMatrix(out Matrix localMatrix)
        {
            Vector3 vector;
            this.Orientation.GetMatrix(out localMatrix);
            localMatrix.Translation = ((this.Min + this.Max) * 0.5f) * this.CubeGrid.GridSize;
            Vector3.TransformNormal(ref this.BlockDefinition.ModelOffset, ref localMatrix, out vector);
            localMatrix.Translation += vector;
        }

        public float GetMass()
        {
            Matrix matrix;
            return ((this.FatBlock == null) ? ((MyDestructionData.Static == null) ? this.BlockDefinition.Mass : MyDestructionData.Static.GetBlockMass(this.CalculateCurrentModel(out matrix), this.BlockDefinition)) : this.FatBlock.GetMass());
        }

        public void GetMissingComponents(Dictionary<string, int> addToDictionary)
        {
            this.m_componentStack.GetMissingComponents(addToDictionary, this.m_stockpile);
        }

        public MyObjectBuilder_CubeBlock GetObjectBuilder(bool copy = false) => 
            this.GetObjectBuilderInternal(copy);

        private MyObjectBuilder_CubeBlock GetObjectBuilderInternal(bool copy)
        {
            MyObjectBuilder_CubeBlock block = null;
            block = (this.FatBlock == null) ? ((MyObjectBuilder_CubeBlock) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) this.BlockDefinition.Id)) : this.FatBlock.GetObjectBuilderCubeBlock(copy);
            block.SubtypeName = this.BlockDefinition.Id.SubtypeName;
            block.Min = this.Min;
            block.BlockOrientation = this.Orientation;
            block.IntegrityPercent = this.m_componentStack.Integrity / this.m_componentStack.MaxIntegrity;
            block.BuildPercent = this.m_componentStack.BuildRatio;
            block.ColorMaskHSV = this.ColorMaskHSV;
            block.SkinSubtypeId = this.SkinSubtypeId;
            block.BuiltBy = this.m_builtByID;
            if ((this.m_stockpile == null) || (this.m_stockpile.GetItems().Count == 0))
            {
                block.ConstructionStockpile = null;
            }
            else
            {
                block.ConstructionStockpile = this.m_stockpile.GetObjectBuilder();
            }
            if (this.IsMultiBlockPart)
            {
                block.MultiBlockDefinition = new SerializableDefinitionId?((SerializableDefinitionId) this.MultiBlockDefinition.Id);
                block.MultiBlockId = this.MultiBlockId;
                block.MultiBlockIndex = this.MultiBlockIndex;
            }
            block.BlockGeneralDamageModifier = this.BlockGeneralDamageModifier;
            return block;
        }

        internal int GetTotalBreakableShapeChildrenCount()
        {
            if (this.FatBlock == null)
            {
                return 0;
            }
            string assetName = this.FatBlock.Model.AssetName;
            int num = 0;
            if (m_modelTotalFracturesCount.TryGetValue(assetName, out num))
            {
                return num;
            }
            MyModel modelOnlyData = MyModels.GetModelOnlyData(assetName);
            if (modelOnlyData.HavokBreakableShapes == null)
            {
                MyDestructionData.Static.LoadModelDestruction(assetName, this.BlockDefinition, Vector3.One, true, false);
            }
            int totalChildrenCount = modelOnlyData.HavokBreakableShapes[0].GetTotalChildrenCount();
            m_modelTotalFracturesCount.Add(assetName, totalChildrenCount);
            return totalChildrenCount;
        }

        public void GetWorldBoundingBox(out BoundingBoxD aabb, bool useAABBFromBlockCubes = false)
        {
            if ((this.FatBlock != null) && !useAABBFromBlockCubes)
            {
                aabb = this.FatBlock.PositionComp.WorldAABB;
            }
            else
            {
                float gridSize = this.CubeGrid.GridSize;
                aabb = new BoundingBoxD((Vector3D) ((this.Min * gridSize) - (gridSize / 2f)), (this.Max * gridSize) + (gridSize / 2f));
                aabb = aabb.TransformFast(this.CubeGrid.WorldMatrix);
            }
        }

        public bool IncreaseMountLevel(float welderMountAmount, long welderOwnerPlayerId, MyInventoryBase outputInventory = null, float maxAllowedBoneMovement = 0f, bool isHelping = false, MyOwnershipShareModeEnum sharing = 1, bool handWelded = false)
        {
            float integrity = this.ComponentStack.Integrity;
            bool isFunctional = this.ComponentStack.IsFunctional;
            welderMountAmount *= this.BlockDefinition.IntegrityPointsPerSec;
            MySession @static = MySession.Static;
            @static.PositiveIntegrityTotal += welderMountAmount;
            if (MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(MySession.Static.Players.TryGetSteamId(welderOwnerPlayerId)))
            {
                this.ClearConstructionStockpile(outputInventory);
            }
            else
            {
                VRage.Game.Entity.MyEntity entity = null;
                if ((outputInventory != null) && (outputInventory.Container != null))
                {
                    entity = outputInventory.Container.Entity as VRage.Game.Entity.MyEntity;
                }
                if ((entity != null) && (entity.InventoryOwnerType() == MyInventoryOwnerTypeEnum.Character))
                {
                    this.MoveItemsFromConstructionStockpile(outputInventory, MyItemFlags.Damaged);
                }
            }
            float buildRatio = this.m_componentStack.BuildRatio;
            float currentDamage = this.CurrentDamage;
            if ((this.BlockDefinition.RatioEnoughForOwnership(this.BuildLevelRatio) && ((this.FatBlock != null) && ((this.FatBlock.OwnerId != welderOwnerPlayerId) && (outputInventory != null)))) && !isHelping)
            {
                this.FatBlock.OnIntegrityChanged(this.BuildIntegrity, this.Integrity, true, welderOwnerPlayerId, sharing);
            }
            if ((MyFakes.SHOW_DAMAGE_EFFECTS && (this.FatBlock != null)) && !this.BlockDefinition.RatioEnoughForDamageEffect((this.Integrity + welderMountAmount) / this.MaxIntegrity))
            {
                this.FatBlock.SetDamageEffect(false);
            }
            bool flag2 = false;
            if (this.m_stockpile == null)
            {
                this.m_componentStack.IncreaseMountLevel(welderMountAmount, null);
            }
            else
            {
                this.m_stockpile.ClearSyncList();
                this.m_componentStack.IncreaseMountLevel(welderMountAmount, this.m_stockpile);
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
            if (this.m_componentStack.IsFullIntegrity)
            {
                this.ReleaseConstructionStockpile();
                flag2 = true;
            }
            MyIntegrityChangeEnum damage = MyIntegrityChangeEnum.Damage;
            if (!this.BlockDefinition.ModelChangeIsNeeded(buildRatio, this.m_componentStack.BuildRatio) && !this.BlockDefinition.ModelChangeIsNeeded(this.m_componentStack.BuildRatio, buildRatio))
            {
                if (this.m_componentStack.IsFunctional && !isFunctional)
                {
                    damage = MyIntegrityChangeEnum.ConstructionEnd;
                    this.PlayConstructionSound(damage, false);
                }
            }
            else
            {
                flag2 = true;
                if ((this.FatBlock != null) && this.m_componentStack.IsFunctional)
                {
                    damage = MyIntegrityChangeEnum.ConstructionEnd;
                }
                this.UpdateVisual(true);
                if (this.FatBlock != null)
                {
                    if (this.CalculateCurrentModelID() == 0)
                    {
                        damage = MyIntegrityChangeEnum.ConstructionBegin;
                    }
                    else if (!this.m_componentStack.IsFunctional)
                    {
                        damage = MyIntegrityChangeEnum.ConstructionProcess;
                    }
                }
                this.PlayConstructionSound(damage, false);
                this.CreateConstructionSmokes();
                if (this.CubeGrid.GridSystems.GasSystem != null)
                {
                    this.CubeGrid.GridSystems.GasSystem.OnSlimBlockBuildRatioRaised(this);
                }
            }
            if (this.HasDeformation)
            {
                this.CubeGrid.SetBlockDirty(this);
            }
            if (flag2)
            {
                this.CubeGrid.RenderData.RemoveDecals(this.Position);
            }
            this.CubeGrid.SendIntegrityChanged(this, damage, 0L);
            this.CubeGrid.OnIntegrityChanged(this, handWelded);
            if (this.ComponentStack.IsFunctional && !isFunctional)
            {
                MyCubeGrids.NotifyBlockFunctional(this.CubeGrid, this, handWelded);
            }
            if (maxAllowedBoneMovement != 0f)
            {
                this.FixBones(currentDamage, maxAllowedBoneMovement);
            }
            if ((MyFakes.ENABLE_GENERATED_BLOCKS && (!this.BlockDefinition.IsGeneratedBlock && (this.BlockDefinition.GeneratedBlockDefinitions != null))) && (this.BlockDefinition.GeneratedBlockDefinitions.Length != 0))
            {
                this.UpdateProgressGeneratedBlocks(buildRatio);
            }
            return ((this.ComponentStack.BuildIntegrity != this.ComponentStack.BuildIntegrity) || !(integrity == this.ComponentStack.Integrity));
        }

        public void IncreaseMountLevelToDesiredRatio(float desiredIntegrityRatio, long welderOwnerPlayerId, MyInventoryBase outputInventory = null, float maxAllowedBoneMovement = 0f, bool isHelping = false, MyOwnershipShareModeEnum sharing = 1)
        {
            float num = (desiredIntegrityRatio * this.MaxIntegrity) - this.Integrity;
            if (num > 0f)
            {
                this.IncreaseMountLevel(num / this.BlockDefinition.IntegrityPointsPerSec, welderOwnerPlayerId, outputInventory, maxAllowedBoneMovement, isHelping, sharing, false);
            }
        }

        public bool Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid, MyCubeBlock fatBlock)
        {
            this.FatBlock = fatBlock;
            if (objectBuilder is MyObjectBuilder_CompoundCubeBlock)
            {
                this.BlockDefinition = MyCompoundCubeBlock.GetCompoundCubeBlockDefinition();
            }
            else if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(objectBuilder.GetId(), out this.BlockDefinition))
            {
                return false;
            }
            if (this.BlockDefinition == null)
            {
                return false;
            }
            if ((this.BlockDefinition.CubeSize != cubeGrid.GridSizeEnum) && !MySession.Static.Settings.EnableSupergridding)
            {
                return false;
            }
            this.m_componentStack = new MyComponentStack(this.BlockDefinition, objectBuilder.IntegrityPercent, objectBuilder.BuildPercent);
            this.m_componentStack.IsFunctionalChanged += new Action(this.m_componentStack_IsFunctionalChanged);
            if (MyCubeGridDefinitions.GetCubeRotationOptions(this.BlockDefinition) == MyRotationOptionsEnum.None)
            {
                objectBuilder.BlockOrientation = MyBlockOrientation.Identity;
            }
            this.UsesDeformation = this.BlockDefinition.UsesDeformation;
            this.DeformationRatio = this.BlockDefinition.DeformationRatio;
            this.Min = (Vector3I) objectBuilder.Min;
            this.Orientation = (MyBlockOrientation) objectBuilder.BlockOrientation;
            if (!this.Orientation.IsValid)
            {
                this.Orientation = MyBlockOrientation.Identity;
            }
            this.CubeGrid = cubeGrid;
            this.ColorMaskHSV = (Vector3) objectBuilder.ColorMaskHSV;
            this.SkinSubtypeId = objectBuilder.SkinSubtypeId;
            if (this.BlockDefinition.CubeDefinition != null)
            {
                this.Orientation = MyCubeGridDefinitions.GetTopologyUniqueOrientation(this.BlockDefinition.CubeDefinition.CubeTopology, this.Orientation);
            }
            ComputeMax(this.BlockDefinition, this.Orientation, ref this.Min, out this.Max);
            this.Position = ComputePositionInGrid(new MatrixI(this.Orientation), this.BlockDefinition, this.Min);
            if (((objectBuilder.MultiBlockId != 0) && (objectBuilder.MultiBlockDefinition != null)) && (objectBuilder.MultiBlockIndex != -1))
            {
                this.MultiBlockDefinition = MyDefinitionManager.Static.TryGetMultiBlockDefinition(objectBuilder.MultiBlockDefinition.Value);
                if (this.MultiBlockDefinition != null)
                {
                    this.MultiBlockId = objectBuilder.MultiBlockId;
                    this.MultiBlockIndex = objectBuilder.MultiBlockIndex;
                }
            }
            this.UpdateShowParts(false);
            if ((this.FatBlock == null) && (!string.IsNullOrEmpty(this.BlockDefinition.Model) | ((this.BlockDefinition.BlockTopology == MyBlockTopology.Cube) && !this.ShowParts)))
            {
                this.FatBlock = new MyCubeBlock();
            }
            if (this.FatBlock != null)
            {
                this.FatBlock.SlimBlock = this;
                this.FatBlock.Init(objectBuilder, cubeGrid);
            }
            if (objectBuilder.ConstructionStockpile != null)
            {
                this.EnsureConstructionStockpileExists();
                this.m_stockpile.Init(objectBuilder.ConstructionStockpile);
            }
            else if (objectBuilder.ConstructionInventory != null)
            {
                this.EnsureConstructionStockpileExists();
                this.m_stockpile.Init(objectBuilder.ConstructionInventory);
            }
            if ((MyFakes.SHOW_DAMAGE_EFFECTS && (this.CubeGrid.CreatePhysics && ((this.FatBlock != null) && (!this.BlockDefinition.RatioEnoughForDamageEffect(this.BuildIntegrity / this.MaxIntegrity) && this.BlockDefinition.RatioEnoughForDamageEffect(this.Integrity / this.MaxIntegrity))))) && (this.CurrentDamage > 0.01f))
            {
                this.FatBlock.SetDamageEffectDelayed(true);
            }
            this.UpdateMaxDeformation();
            this.m_builtByID = objectBuilder.BuiltBy;
            this.BlockGeneralDamageModifier = objectBuilder.BlockGeneralDamageModifier;
            return true;
        }

        public void InitOrientation(MyBlockOrientation orientation)
        {
            if (!orientation.IsValid)
            {
                this.Orientation = MyBlockOrientation.Identity;
            }
            this.InitOrientation(orientation.Forward, orientation.Up);
        }

        public void InitOrientation(Base6Directions.Direction Forward, Base6Directions.Direction Up)
        {
            this.Orientation = (MyCubeGridDefinitions.GetCubeRotationOptions(this.BlockDefinition) != MyRotationOptionsEnum.None) ? new MyBlockOrientation(Forward, Up) : MyBlockOrientation.Identity;
            if (this.BlockDefinition.CubeDefinition != null)
            {
                this.Orientation = MyCubeGridDefinitions.GetTopologyUniqueOrientation(this.BlockDefinition.CubeDefinition.CubeTopology, this.Orientation);
            }
        }

        public void InitOrientation(ref Vector3I forward, ref Vector3I up)
        {
            this.InitOrientation(Base6Directions.GetDirection((Vector3I) forward), Base6Directions.GetDirection((Vector3I) up));
        }

        private void m_componentStack_IsFunctionalChanged()
        {
            if (!this.m_componentStack.IsFunctional)
            {
                MyIdentity identity2 = MySession.Static.Players.TryGetIdentity(this.m_builtByID);
                if (identity2 != null)
                {
                    int pcu = this.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
                    identity2.BlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, false);
                    if (!ReferenceEquals(identity2.BlockLimits, MySession.Static.GlobalBlockLimits) && !ReferenceEquals(identity2.BlockLimits, MySession.Static.PirateBlockLimits))
                    {
                        MySession.Static.GlobalBlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, false);
                    }
                }
            }
            else
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.m_builtByID);
                if (identity == null)
                {
                    long builtByID = this.m_builtByID;
                }
                else
                {
                    int pcu = this.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
                    identity.BlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, false);
                    if (!ReferenceEquals(identity.BlockLimits, MySession.Static.GlobalBlockLimits) && !ReferenceEquals(identity.BlockLimits, MySession.Static.PirateBlockLimits))
                    {
                        MySession.Static.GlobalBlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, false);
                    }
                }
            }
        }

        private bool ModelChangeIsNeeded(float a, float b) => 
            ((a <= b) ? this.BlockDefinition.ModelChangeIsNeeded(a, b) : this.BlockDefinition.ModelChangeIsNeeded(b, a));

        public bool MoveItemsFromConstructionStockpile(MyInventoryBase toInventory, MyItemFlags flags = 0)
        {
            bool flag = false;
            if (this.m_stockpile != null)
            {
                if (toInventory == null)
                {
                    return flag;
                }
                m_tmpItemList.Clear();
                foreach (MyStockpileItem item in this.m_stockpile.GetItems())
                {
                    if ((flags == MyItemFlags.None) || ((item.Content.Flags & flags) != MyItemFlags.None))
                    {
                        m_tmpItemList.Add(item);
                    }
                }
                this.m_stockpile.ClearSyncList();
                foreach (MyStockpileItem item2 in m_tmpItemList)
                {
                    int amount = Math.Min(toInventory.ComputeAmountThatFits(item2.Content.GetId(), 0f, 0f).ToIntSafe(), item2.Amount);
                    toInventory.AddItems(amount, item2.Content);
                    this.m_stockpile.RemoveItems(amount, item2.Content);
                    if (amount <= 0)
                    {
                        flag = true;
                    }
                }
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
            return flag;
        }

        public void MoveItemsToConstructionStockpile(MyInventoryBase fromInventory)
        {
            if (!MySession.Static.CreativeMode)
            {
                m_tmpComponents.Clear();
                this.GetMissingComponents(m_tmpComponents);
                if (m_tmpComponents.Count != 0)
                {
                    this.EnsureConstructionStockpileExists();
                    this.m_stockpile.ClearSyncList();
                    foreach (KeyValuePair<string, int> pair in m_tmpComponents)
                    {
                        MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), pair.Key);
                        int itemAmountCombined = (int) MyCubeBuilder.BuildComponent.GetItemAmountCombined(fromInventory, myDefinitionId);
                        int itemAmount = Math.Min(pair.Value, itemAmountCombined);
                        if (itemAmount > 0)
                        {
                            MyCubeBuilder.BuildComponent.RemoveItemsCombined(fromInventory, itemAmount, myDefinitionId);
                            this.m_stockpile.AddItems(itemAmount, new MyDefinitionId(typeof(MyObjectBuilder_Component), pair.Key), MyItemFlags.None);
                        }
                    }
                    this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                    this.m_stockpile.ClearSyncList();
                }
            }
        }

        public void MoveUnneededItemsFromConstructionStockpile(MyInventoryBase toInventory)
        {
            if ((this.m_stockpile != null) && (toInventory != null))
            {
                m_tmpItemList.Clear();
                this.AcquireUnneededStockpileItems(m_tmpItemList);
                this.m_stockpile.ClearSyncList();
                foreach (MyStockpileItem item in m_tmpItemList)
                {
                    int amount = Math.Min(toInventory.ComputeAmountThatFits(item.Content.GetId(), 0f, 0f).ToIntSafe(), item.Amount);
                    toInventory.AddItems(amount, item.Content);
                    this.m_stockpile.RemoveItems(amount, item.Content);
                }
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
        }

        public unsafe void OnDestroyVisual()
        {
            if (MyFakes.SHOW_DAMAGE_EFFECTS && !this.CubeGrid.IsLargeDestroyInProgress)
            {
                string str;
                MyExplosionInfo* infoPtr1;
                int num1;
                int num2;
                if ((this.FatBlock == null) || !this.FatBlock.IsBuilt)
                {
                    num1 = (int) ReferenceEquals(this.FatBlock, null);
                }
                else
                {
                    num1 = 1;
                }
                int local1 = num1;
                if (string.IsNullOrEmpty((local1 != 0) ? this.BlockDefinition.DestroyEffect : null))
                {
                    str = (this.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? "BlockDestroyed_Large3X" : "BlockDestroyed_Large";
                }
                MySoundPair objA = (local1 != 0) ? this.BlockDefinition.DestroySound : null;
                if ((objA == null) || ReferenceEquals(objA, MySoundPair.Empty))
                {
                    objA = (this.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? MyExplosion.LargePoofSound : MyExplosion.SmallPoofSound;
                }
                if (((this.FatBlock == null) || (this.FatBlock.Model == null)) || (this.FatBlock.Model.BoundingSphere.Radius <= 0.5f))
                {
                    num2 = (int) ReferenceEquals(this.FatBlock, null);
                }
                else
                {
                    num2 = 1;
                }
                bool flag = (bool) num2;
                Vector3D zero = Vector3D.Zero;
                if ((this.BlockDefinition.DestroyEffectOffset != null) && !this.BlockDefinition.DestroyEffectOffset.Value.Equals(Vector3.Zero))
                {
                    Matrix matrix;
                    this.Orientation.GetMatrix(out matrix);
                    zero = Vector3D.Rotate(new Vector3D(Vector3.RotateAndScale(this.BlockDefinition.DestroyEffectOffset.Value, matrix)), this.CubeGrid.WorldMatrix);
                }
                BoundingSphereD ed = BoundingSphereD.CreateFromBoundingBox(this.WorldAABB);
                BoundingSphereD* edPtr1 = (BoundingSphereD*) ref ed;
                edPtr1->Center = ed.Center + zero;
                if (MyFakes.DEBUG_DISPLAY_DESTROY_EFFECT_OFFSET)
                {
                    Matrix matrix2;
                    this.Orientation.GetMatrix(out matrix2);
                    MatrixD matrix = MatrixD.Multiply(new MatrixD(matrix2), this.CubeGrid.WorldMatrix);
                    matrix.Translation = this.WorldPosition;
                    MyRenderProxy.DebugDrawAxis(matrix, 1f, false, false, true);
                    MyRenderProxy.DebugDrawLine3D(this.WorldPosition, ed.Center, Color.Red, Color.Yellow, false, true);
                }
                bool flag2 = (this.CubeGrid.Physics != null) && this.CubeGrid.Physics.IsPlanetCrashing_PointConcealed(this.WorldPosition);
                MyExplosionInfo info2 = new MyExplosionInfo {
                    PlayerDamage = 0f,
                    Damage = 0f,
                    ExplosionType = MyExplosionTypeEnum.CUSTOM,
                    ExplosionSphere = ed,
                    LifespanMiliseconds = 700,
                    HitEntity = this.CubeGrid,
                    ParticleScale = 1f,
                    CustomEffect = str,
                    CustomSound = objA,
                    OwnerEntity = this.CubeGrid,
                    Direction = new Vector3?((Vector3) this.CubeGrid.WorldMatrix.Forward),
                    VoxelExplosionCenter = this.WorldPosition + zero
                };
                infoPtr1->ExplosionFlags = ((flag ? MyExplosionFlags.CREATE_DEBRIS : ((MyExplosionFlags) 0)) | MyExplosionFlags.CREATE_DECALS) | (flag2 ? ((MyExplosionFlags) 0) : MyExplosionFlags.CREATE_PARTICLE_EFFECT);
                infoPtr1 = (MyExplosionInfo*) ref info2;
                info2.VoxelCutoutScale = 0f;
                info2.PlaySound = true;
                info2.ApplyForceAndDamage = true;
                info2.ObjectsRemoveDelayInMiliseconds = 40;
                MyExplosionInfo explosionInfo = info2;
                if (this.CubeGrid.Physics != null)
                {
                    explosionInfo.Velocity = this.CubeGrid.Physics.LinearVelocity;
                }
                MyExplosions.AddExplosion(ref explosionInfo, false);
            }
        }

        public void PlayConstructionSound(MyIntegrityChangeEnum integrityChangeType, bool deconstruction = false)
        {
            MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
            if (emitter != null)
            {
                bool? nullable;
                if (this.FatBlock != null)
                {
                    emitter.SetPosition(new Vector3D?(this.FatBlock.PositionComp.GetPosition()));
                }
                else
                {
                    emitter.SetPosition(new Vector3D?(this.CubeGrid.PositionComp.GetPosition() + ((this.Position - 1) * this.CubeGrid.GridSize)));
                }
                switch (integrityChangeType)
                {
                    case MyIntegrityChangeEnum.ConstructionBegin:
                        if (deconstruction)
                        {
                            nullable = null;
                            emitter.PlaySound(DECONSTRUCTION_START, true, false, false, true, false, nullable);
                            return;
                        }
                        nullable = null;
                        emitter.PlaySound(CONSTRUCTION_START, true, false, false, true, false, nullable);
                        return;

                    case MyIntegrityChangeEnum.ConstructionEnd:
                        if (deconstruction)
                        {
                            nullable = null;
                            emitter.PlaySound(DECONSTRUCTION_END, true, false, false, true, false, nullable);
                            return;
                        }
                        nullable = null;
                        emitter.PlaySound(CONSTRUCTION_END, true, false, false, true, false, nullable);
                        return;

                    case MyIntegrityChangeEnum.ConstructionProcess:
                        if (deconstruction)
                        {
                            nullable = null;
                            emitter.PlaySound(DECONSTRUCTION_PROG, true, false, false, true, false, nullable);
                            return;
                        }
                        nullable = null;
                        emitter.PlaySound(CONSTRUCTION_PROG, true, false, false, true, false, nullable);
                        return;

                    case MyIntegrityChangeEnum.DeconstructionBegin:
                        nullable = null;
                        emitter.PlaySound(DECONSTRUCTION_START, true, false, false, true, false, nullable);
                        return;

                    case MyIntegrityChangeEnum.DeconstructionEnd:
                        nullable = null;
                        emitter.PlaySound(DECONSTRUCTION_END, true, false, false, true, false, nullable);
                        return;

                    case MyIntegrityChangeEnum.DeconstructionProcess:
                        nullable = null;
                        emitter.PlaySound(DECONSTRUCTION_PROG, true, false, false, true, false, nullable);
                        return;
                }
                nullable = null;
                emitter.PlaySound(MySoundPair.Empty, false, false, false, false, false, nullable);
            }
        }

        public void RandomizeBuildLevel()
        {
            float buildIntegrity = MyUtils.GetRandomFloat(0f, 1f) * this.BlockDefinition.MaxIntegrity;
            this.m_componentStack.SetIntegrity(buildIntegrity, buildIntegrity);
        }

        private void ReleaseConstructionStockpile()
        {
            if (this.m_stockpile != null)
            {
                if (MyFakes.ENABLE_GENERATED_BLOCKS)
                {
                    bool isGeneratedBlock = this.BlockDefinition.IsGeneratedBlock;
                }
                this.m_stockpile = null;
            }
        }

        private void ReleaseUnneededStockpileItems()
        {
            if ((this.m_stockpile != null) && Sync.IsServer)
            {
                m_tmpItemList.Clear();
                this.AcquireUnneededStockpileItems(m_tmpItemList);
                this.m_stockpile.ClearSyncList();
                BoundingBoxD box = new BoundingBoxD(this.CubeGrid.GridIntegerToWorld(this.Min), this.CubeGrid.GridIntegerToWorld(this.Max));
                foreach (MyStockpileItem item in m_tmpItemList)
                {
                    if (item.Amount >= 0.01f)
                    {
                        VRage.Game.Entity.MyEntity entity = MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(item.Amount, item.Content, 1f), box, this.CubeGrid.Physics);
                        if (entity != null)
                        {
                            entity.Physics.ApplyImpulse((MyUtils.GetRandomVector3Normalized() * entity.Physics.Mass) / 5f, entity.PositionComp.GetPosition());
                        }
                        this.m_stockpile.RemoveItems(item.Amount, item.Content);
                    }
                }
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
        }

        public void RemoveAuthorship()
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.m_builtByID);
            if (identity != null)
            {
                int pcu = 1;
                if (this.ComponentStack.IsFunctional)
                {
                    pcu = this.BlockDefinition.PCU;
                }
                identity.BlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
                if (!ReferenceEquals(identity.BlockLimits, MySession.Static.GlobalBlockLimits) && !ReferenceEquals(identity.BlockLimits, MySession.Static.PirateBlockLimits))
                {
                    MySession.Static.GlobalBlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
                }
            }
        }

        internal void RemoveFractureComponent()
        {
            if (this.FatBlock.Components.Has<MyFractureComponentBase>())
            {
                this.FatBlock.Components.Remove<MyFractureComponentBase>();
                this.FatBlock.Render.UpdateRenderObject(false, true);
                this.FatBlock.CreateRenderer(this.FatBlock.Render.PersistentFlags, this.FatBlock.Render.ColorMaskHsv, this.FatBlock.Render.ModelStorage);
                this.UpdateVisual(true);
                this.FatBlock.Render.UpdateRenderObject(true, true);
                MySlimBlock cubeBlock = this.CubeGrid.GetCubeBlock(this.Position);
                if (cubeBlock != null)
                {
                    cubeBlock.CubeGrid.UpdateBlockNeighbours(cubeBlock);
                }
            }
        }

        private void RemoveFromConstructionStockpile(MyStockpileItem item)
        {
            this.m_stockpile.RemoveItems(item.Amount, item.Content.GetId(), item.Content.Flags);
        }

        public void RemoveNeighbours()
        {
            bool flag = true;
            foreach (MySlimBlock block in this.Neighbours)
            {
                flag &= block.Neighbours.Remove(this);
            }
            this.Neighbours.Clear();
            if (this.FatBlock != null)
            {
                this.FatBlock.OnRemovedNeighbours();
            }
        }

        internal void RepairFracturedBlock(long toolOwnerId)
        {
            if (this.FatBlock != null)
            {
                this.RemoveFractureComponent();
                this.CubeGrid.GetGeneratedBlocks(this, m_tmpBlocks);
                foreach (MySlimBlock local1 in m_tmpBlocks)
                {
                    local1.RemoveFractureComponent();
                    local1.SetGeneratedBlockIntegrity(this);
                }
                m_tmpBlocks.Clear();
                this.UpdateProgressGeneratedBlocks(0f);
                if (Sync.IsServer)
                {
                    BoundingBoxD worldAABB = this.FatBlock.PositionComp.WorldAABB;
                    if (this.BlockDefinition.CubeSize == MyCubeSize.Large)
                    {
                        worldAABB.Inflate((double) -0.16);
                    }
                    else
                    {
                        worldAABB.Inflate((double) -0.04);
                    }
                    MyFracturedPiecesManager.Static.RemoveFracturesInBox(ref worldAABB, 0f);
                    this.CubeGrid.SendFractureComponentRepaired(this, toolOwnerId);
                }
            }
        }

        public void RepairFracturedBlockWithFullHealth(long toolOwnerId)
        {
            if (!this.BlockDefinition.IsGeneratedBlock)
            {
                if (!MyFakes.ENABLE_MULTIBLOCK_CONSTRUCTION || !this.IsMultiBlockPart)
                {
                    if (this.GetFractureComponent() != null)
                    {
                        this.RepairFracturedBlock(toolOwnerId);
                    }
                }
                else
                {
                    this.RepairMultiBlock(toolOwnerId);
                    if (!MySession.Static.SurvivalMode)
                    {
                        this.CubeGrid.AddMissingBlocksInMultiBlock(this.MultiBlockId, toolOwnerId);
                    }
                }
            }
        }

        private void RepairMultiBlock(long toolOwnerId)
        {
            MyCubeGridMultiBlockInfo multiBlockInfo = this.CubeGrid.GetMultiBlockInfo(this.MultiBlockId);
            if ((multiBlockInfo != null) && multiBlockInfo.IsFractured())
            {
                m_tmpMultiBlocks.AddRange(multiBlockInfo.Blocks);
                foreach (MySlimBlock block in m_tmpMultiBlocks)
                {
                    if (block.GetFractureComponent() != null)
                    {
                        block.RepairFracturedBlock(toolOwnerId);
                    }
                }
                m_tmpMultiBlocks.Clear();
            }
        }

        internal void RequestFillStockpile(MyInventory SourceInventory)
        {
            m_tmpComponents.Clear();
            this.GetMissingComponents(m_tmpComponents);
            foreach (KeyValuePair<string, int> pair in m_tmpComponents)
            {
                MyDefinitionId contentId = new MyDefinitionId(typeof(MyObjectBuilder_Component), pair.Key);
                if (SourceInventory.ContainItems(1, contentId, MyItemFlags.None))
                {
                    this.CubeGrid.RequestFillStockpile(this.Position, SourceInventory);
                    break;
                }
            }
        }

        public void ResumeDamageEffect()
        {
            if (this.FatBlock != null)
            {
                if ((!MyFakes.SHOW_DAMAGE_EFFECTS || this.BlockDefinition.RatioEnoughForDamageEffect(this.BuildIntegrity / this.MaxIntegrity)) || !this.BlockDefinition.RatioEnoughForDamageEffect(this.Integrity / this.MaxIntegrity))
                {
                    this.FatBlock.SetDamageEffect(false);
                }
                else if (this.CurrentDamage > 0f)
                {
                    this.FatBlock.SetDamageEffect(true);
                }
            }
        }

        public static void SendDamage(MySlimBlock block, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            DoDamageSlimBlockMsg msg = new DoDamageSlimBlockMsg {
                GridEntityId = block.CubeGrid.EntityId,
                Position = block.Position,
                Damage = damage,
                HitInfo = hitInfo,
                AttackerEntityId = attackerId,
                CompoundBlockId = uint.MaxValue,
                Type = damageType
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<DoDamageSlimBlockMsg>(s => new Action<DoDamageSlimBlockMsg>(MySlimBlock.DoDamageSlimBlock), msg, targetEndpoint, position);
        }

        public static void SendDamageBatch(Dictionary<MySlimBlock, float> blocks, MyStringHash damageType, long attackerId)
        {
            if (blocks.Count != 0)
            {
                MyCubeGrid cubeGrid = blocks.FirstPair<MySlimBlock, float>().Key.CubeGrid;
                if (!cubeGrid.MarkedForClose)
                {
                    using (MyUtils.ClearCollectionToken<List<MyTuple<Vector3I, float>>, MyTuple<Vector3I, float>> token = MyUtils.ReuseCollection<MyTuple<Vector3I, float>>(ref m_batchCache))
                    {
                        List<MyTuple<Vector3I, float>> collection = token.Collection;
                        foreach (KeyValuePair<MySlimBlock, float> pair2 in blocks)
                        {
                            MySlimBlock key = pair2.Key;
                            if ((cubeGrid.EntityId == key.CubeGrid.EntityId) && !key.IsDestroyed)
                            {
                                collection.Add(MyTuple.Create<Vector3I, float>(key.Position, pair2.Value));
                            }
                        }
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long, List<MyTuple<Vector3I, float>>, MyStringHash, long>(s => new Action<long, List<MyTuple<Vector3I, float>>, MyStringHash, long>(MySlimBlock.DoDamageSlimBlockBatch), cubeGrid.EntityId, collection, damageType, attackerId, targetEndpoint, position);
                    }
                }
            }
        }

        public static void SetBlockComponents(MyHudBlockInfo hudInfo, MyCubeBlockDefinition blockDefinition, MyInventoryBase availableInventory = null)
        {
            SetBlockComponentsInternal(hudInfo, blockDefinition, null, availableInventory);
        }

        public static void SetBlockComponents(MyHudBlockInfo hudInfo, MySlimBlock block, MyInventoryBase availableInventory = null)
        {
            SetBlockComponentsInternal(hudInfo, block.BlockDefinition, block, availableInventory);
        }

        private static unsafe void SetBlockComponentsInternal(MyHudBlockInfo hudInfo, MyCubeBlockDefinition blockDefinition, MySlimBlock block, MyInventoryBase availableInventory)
        {
            hudInfo.Components.Clear();
            MySlimBlock block1 = block;
            hudInfo.InitBlockInfo(blockDefinition);
            hudInfo.ShowAvailable = MyPerGameSettings.AlwaysShowAvailableBlocksOnHud;
            if (MyFakes.ENABLE_SMALL_GRID_BLOCK_COMPONENT_INFO || (blockDefinition.CubeSize != MyCubeSize.Small))
            {
                if (block != null)
                {
                    hudInfo.BlockIntegrity = block.Integrity / block.MaxIntegrity;
                }
                if ((block == null) || !block.IsMultiBlockPart)
                {
                    if ((block == null) && (blockDefinition.MultiBlock != null))
                    {
                        MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_MultiBlockDefinition), blockDefinition.MultiBlock);
                        MyMultiBlockDefinition definition3 = MyDefinitionManager.Static.TryGetMultiBlockDefinition(id);
                        if (definition3 != null)
                        {
                            foreach (MyMultiBlockDefinition.MyMultiBlockPartDefinition definition4 in definition3.BlockDefinitions)
                            {
                                MyCubeBlockDefinition definition5;
                                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition4.Id, out definition5))
                                {
                                    hudInfo.AddComponentsForBlock(definition5);
                                }
                            }
                            hudInfo.MergeSameComponents();
                            for (int i = 0; i < hudInfo.Components.Count; i++)
                            {
                                MyHudBlockInfo.ComponentInfo info5 = hudInfo.Components[i];
                                MyHudBlockInfo.ComponentInfo* infoPtr2 = (MyHudBlockInfo.ComponentInfo*) ref info5;
                                infoPtr2->AvailableAmount = (int) MyCubeBuilder.BuildComponent.GetItemAmountCombined(availableInventory, info5.DefinitionId);
                                hudInfo.Components[i] = info5;
                            }
                        }
                    }
                    else
                    {
                        int index = 0;
                        while (true)
                        {
                            if (index >= blockDefinition.Components.Length)
                            {
                                if ((block != null) && !block.StockpileEmpty)
                                {
                                    foreach (MyCubeBlockDefinition.Component component3 in block.BlockDefinition.Components)
                                    {
                                        int constructionStockpileItemAmount = block.GetConstructionStockpileItemAmount(component3.Definition.Id);
                                        if (constructionStockpileItemAmount > 0)
                                        {
                                            for (int i = 0; i < hudInfo.Components.Count; i++)
                                            {
                                                if (ReferenceEquals(block.ComponentStack.GetGroupInfo(i).Component, component3.Definition))
                                                {
                                                    if (block.ComponentStack.IsFullyDismounted)
                                                    {
                                                        return;
                                                    }
                                                    constructionStockpileItemAmount = SetHudInfoComponentAmount(hudInfo, constructionStockpileItemAmount, i);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            MyComponentStack.GroupInfo groupInfo = new MyComponentStack.GroupInfo();
                            if (block != null)
                            {
                                groupInfo = block.ComponentStack.GetGroupInfo(index);
                            }
                            else
                            {
                                MyCubeBlockDefinition.Component component2 = blockDefinition.Components[index];
                                groupInfo.Component = component2.Definition;
                                groupInfo.TotalCount = component2.Count;
                                groupInfo.MountedCount = 0;
                                groupInfo.AvailableCount = 0;
                                groupInfo.Integrity = 0f;
                                groupInfo.MaxIntegrity = component2.Count * component2.Definition.MaxIntegrity;
                            }
                            AddBlockComponent(hudInfo, groupInfo, availableInventory);
                            index++;
                        }
                    }
                }
                else
                {
                    MyCubeGridMultiBlockInfo multiBlockInfo = block.CubeGrid.GetMultiBlockInfo(block.MultiBlockId);
                    if (multiBlockInfo != null)
                    {
                        foreach (MyMultiBlockDefinition.MyMultiBlockPartDefinition definition in multiBlockInfo.MultiBlockDefinition.BlockDefinitions)
                        {
                            MyCubeBlockDefinition definition2;
                            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition.Id, out definition2))
                            {
                                hudInfo.AddComponentsForBlock(definition2);
                            }
                        }
                        hudInfo.MergeSameComponents();
                        foreach (MySlimBlock block2 in multiBlockInfo.Blocks)
                        {
                            int index = 0;
                            while (index < block2.BlockDefinition.Components.Length)
                            {
                                MyCubeBlockDefinition.Component component = block2.BlockDefinition.Components[index];
                                MyComponentStack.GroupInfo groupInfo = block2.ComponentStack.GetGroupInfo(index);
                                int num3 = 0;
                                while (true)
                                {
                                    if (num3 < hudInfo.Components.Count)
                                    {
                                        if (!(hudInfo.Components[num3].DefinitionId == component.Definition.Id))
                                        {
                                            num3++;
                                            continue;
                                        }
                                        MyHudBlockInfo.ComponentInfo info3 = hudInfo.Components[num3];
                                        int* numPtr1 = (int*) ref info3.MountedCount;
                                        numPtr1[0] += groupInfo.MountedCount;
                                        hudInfo.Components[num3] = info3;
                                    }
                                    index++;
                                    break;
                                }
                            }
                        }
                        for (int i = 0; i < hudInfo.Components.Count; i++)
                        {
                            if (availableInventory != null)
                            {
                                MyHudBlockInfo.ComponentInfo info4 = hudInfo.Components[i];
                                MyHudBlockInfo.ComponentInfo* infoPtr1 = (MyHudBlockInfo.ComponentInfo*) ref info4;
                                infoPtr1->AvailableAmount = (int) MyCubeBuilder.BuildComponent.GetItemAmountCombined(availableInventory, info4.DefinitionId);
                                hudInfo.Components[i] = info4;
                            }
                            int amount = 0;
                            foreach (MySlimBlock block3 in multiBlockInfo.Blocks)
                            {
                                if (!block3.StockpileEmpty)
                                {
                                    amount += block3.GetConstructionStockpileItemAmount(hudInfo.Components[i].DefinitionId);
                                }
                            }
                            if (amount > 0)
                            {
                                SetHudInfoComponentAmount(hudInfo, amount, i);
                            }
                        }
                    }
                }
            }
        }

        public void SetGeneratedBlockIntegrity(MySlimBlock generatingBlock)
        {
            if (this.BlockDefinition.IsGeneratedBlock)
            {
                float buildRatio = this.ComponentStack.BuildRatio;
                this.ComponentStack.SetIntegrity(generatingBlock.BuildLevelRatio * this.MaxIntegrity, generatingBlock.ComponentStack.IntegrityRatio * this.MaxIntegrity);
                if (this.ModelChangeIsNeeded(this.ComponentStack.BuildRatio, buildRatio))
                {
                    this.UpdateVisual(true);
                }
            }
        }

        private static int SetHudInfoComponentAmount(MyHudBlockInfo hudInfo, int amount, int i)
        {
            MyHudBlockInfo.ComponentInfo info = hudInfo.Components[i];
            int num = Math.Min(info.TotalCount - info.MountedCount, amount);
            info.StockpileCount = num;
            amount -= num;
            hudInfo.Components[i] = info;
            return amount;
        }

        public void SetIntegrity(float buildIntegrity, float integrity, MyIntegrityChangeEnum integrityChangeType, long grinderOwner)
        {
            float buildRatio = this.m_componentStack.BuildRatio;
            this.m_componentStack.SetIntegrity(buildIntegrity, integrity);
            if (((this.FatBlock != null) && !this.BlockDefinition.RatioEnoughForOwnership(buildRatio)) && this.BlockDefinition.RatioEnoughForOwnership(this.m_componentStack.BuildRatio))
            {
                this.FatBlock.OnIntegrityChanged(buildIntegrity, integrity, true, MySession.Static.LocalPlayerId, MyOwnershipShareModeEnum.Faction);
            }
            this.UpdateHackingIndicator(this.m_componentStack.BuildRatio, buildRatio, grinderOwner);
            if ((MyFakes.SHOW_DAMAGE_EFFECTS && (this.FatBlock != null)) && !this.BlockDefinition.RatioEnoughForDamageEffect(this.Integrity / this.MaxIntegrity))
            {
                this.FatBlock.SetDamageEffect(false);
            }
            bool isFullIntegrity = this.IsFullIntegrity;
            if (this.ModelChangeIsNeeded(this.m_componentStack.BuildRatio, buildRatio))
            {
                isFullIntegrity = true;
                this.UpdateVisual(true);
                if (integrityChangeType != MyIntegrityChangeEnum.Damage)
                {
                    this.CreateConstructionSmokes();
                }
                this.PlayConstructionSound(integrityChangeType, false);
                if (this.CubeGrid.GridSystems.GasSystem != null)
                {
                    if (buildRatio > this.m_componentStack.BuildRatio)
                    {
                        this.CubeGrid.GridSystems.GasSystem.OnSlimBlockBuildRatioLowered(this);
                    }
                    else
                    {
                        this.CubeGrid.GridSystems.GasSystem.OnSlimBlockBuildRatioRaised(this);
                    }
                }
            }
            if (isFullIntegrity)
            {
                this.CubeGrid.RenderData.RemoveDecals(this.Position);
            }
            if ((MyFakes.ENABLE_GENERATED_BLOCKS && (!this.BlockDefinition.IsGeneratedBlock && (this.BlockDefinition.GeneratedBlockDefinitions != null))) && (this.BlockDefinition.GeneratedBlockDefinitions.Length != 0))
            {
                this.UpdateProgressGeneratedBlocks(buildRatio);
            }
        }

        public void SetToConstructionSite()
        {
            this.m_componentStack.DestroyCompletely();
        }

        public void SpawnConstructionStockpile()
        {
            if (this.m_stockpile != null)
            {
                Vector3D vectord;
                MatrixD worldMatrix = this.CubeGrid.WorldMatrix;
                int num = this.Max.RectangularDistance(this.Min) + 3;
                Vector3D worldPoint = (Vector3D.Transform((Vector3D) (this.Min * this.CubeGrid.GridSize), worldMatrix) + Vector3D.Transform((Vector3D) (this.Max * this.CubeGrid.GridSize), worldMatrix)) / 2.0;
                Vector3 vector = MyGravityProviderSystem.CalculateTotalGravityInPoint(worldPoint);
                if (vector.Length() != 0f)
                {
                    vector.Normalize();
                    Vector3I? nullable = this.CubeGrid.RayCastBlocks(worldPoint, worldPoint + ((vector * num) * this.CubeGrid.GridSize));
                    if (nullable == null)
                    {
                        vectord = worldPoint;
                    }
                    else
                    {
                        vectord = Vector3D.Transform(nullable.Value * this.CubeGrid.GridSize, worldMatrix) - ((vector * this.CubeGrid.GridSize) * 0.1f);
                    }
                }
                foreach (MyStockpileItem item in this.m_stockpile.GetItems())
                {
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(item.Amount, item.Content, 1f), vectord, worldMatrix.Forward, worldMatrix.Up, this.CubeGrid.Physics, null);
                }
            }
        }

        public void SpawnFirstItemInConstructionStockpile()
        {
            if (!MySession.Static.CreativeMode)
            {
                this.EnsureConstructionStockpileExists();
                MyComponentStack.GroupInfo groupInfo = this.ComponentStack.GetGroupInfo(0);
                this.m_stockpile.ClearSyncList();
                this.m_stockpile.AddItems(1, groupInfo.Component.Id, MyItemFlags.None);
                this.CubeGrid.SendStockpileChanged(this, this.m_stockpile.GetSyncList());
                this.m_stockpile.ClearSyncList();
            }
        }

        public override string ToString() => 
            ((this.FatBlock != null) ? this.FatBlock.ToString() : this.BlockDefinition.DisplayNameText.ToString());

        public void TransferAuthorship(long newOwner)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.m_builtByID);
            MyIdentity identity2 = MySession.Static.Players.TryGetIdentity(newOwner);
            if ((identity != null) && (identity2 != null))
            {
                int pcu = 1;
                if (this.ComponentStack.IsFunctional)
                {
                    pcu = this.BlockDefinition.PCU;
                }
                identity.BlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
                this.m_builtByID = newOwner;
                identity2.BlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
            }
        }

        public void TransferAuthorshipClient(long newOwner)
        {
            this.m_builtByID = newOwner;
        }

        public void TransferLimits(MyBlockLimits oldLimits, MyBlockLimits newLimits)
        {
            int pcu = 1;
            if (this.ComponentStack.IsFunctional)
            {
                pcu = this.BlockDefinition.PCU;
            }
            oldLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
            newLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, this.CubeGrid, true);
        }

        internal void Transform(ref MatrixI transform)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori3;
            Vector3I.Transform(ref this.Min, ref transform, out vectori);
            Vector3I.Transform(ref this.Max, ref transform, out vectori2);
            Vector3I.Transform(ref this.Position, ref transform, out vectori3);
            Vector3I intVector = Base6Directions.GetIntVector(transform.GetDirection(this.Orientation.Forward));
            Vector3I up = Base6Directions.GetIntVector(transform.GetDirection(this.Orientation.Up));
            this.InitOrientation(ref intVector, ref up);
            this.Min = Vector3I.Min(vectori, vectori2);
            this.Max = Vector3I.Max(vectori, vectori2);
            this.Position = vectori3;
            if (this.FatBlock != null)
            {
                this.FatBlock.OnTransformed(ref transform);
            }
        }

        private void UpdateHackingIndicator(float newRatio, float oldRatio, long grinderOwner)
        {
            if (((newRatio < oldRatio) && (this.FatBlock != null)) && (this.FatBlock.IDModule != null))
            {
                MyRelationsBetweenPlayerAndBlock userRelationToOwner = this.FatBlock.IDModule.GetUserRelationToOwner(grinderOwner);
                if ((userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Enemies) || (userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Neutral))
                {
                    MyTerminalBlock fatBlock = this.FatBlock as MyTerminalBlock;
                    if (fatBlock != null)
                    {
                        fatBlock.HackAttemptTime = new int?(MySandboxGame.TotalSimulationTimeInMilliseconds);
                        if (OnAnyBlockHackedChanged != null)
                        {
                            OnAnyBlockHackedChanged(fatBlock, grinderOwner);
                        }
                    }
                }
            }
        }

        public void UpdateMaxDeformation()
        {
            this.m_cachedMaxDeformation = this.CubeGrid.Skeleton.MaxDeformation(this.Position, this.CubeGrid);
        }

        private void UpdateProgressGeneratedBlocks(float oldBuildRatio)
        {
            float buildRatio = this.ComponentStack.BuildRatio;
            if (oldBuildRatio != buildRatio)
            {
                if (oldBuildRatio < buildRatio)
                {
                    if ((oldBuildRatio < this.BlockDefinition.BuildProgressToPlaceGeneratedBlocks) && (buildRatio >= this.BlockDefinition.BuildProgressToPlaceGeneratedBlocks))
                    {
                        this.CubeGrid.AdditionalModelGenerators.ForEach(g => g.GenerateBlocks(this));
                    }
                }
                else if ((oldBuildRatio >= this.BlockDefinition.BuildProgressToPlaceGeneratedBlocks) && (buildRatio < this.BlockDefinition.BuildProgressToPlaceGeneratedBlocks))
                {
                    m_tmpBlocks.Clear();
                    this.CubeGrid.GetGeneratedBlocks(this, m_tmpBlocks);
                    this.CubeGrid.RazeGeneratedBlocks(m_tmpBlocks);
                    m_tmpBlocks.Clear();
                }
            }
        }

        private void UpdateShowParts(bool fixSkeleton)
        {
            if (this.BlockDefinition.BlockTopology != MyBlockTopology.Cube)
            {
                this.ShowParts = false;
            }
            else
            {
                float buildLevelRatio = this.BuildLevelRatio;
                if ((this.BlockDefinition.BuildProgressModels == null) || (this.BlockDefinition.BuildProgressModels.Length == 0))
                {
                    this.ShowParts = true;
                }
                else
                {
                    this.ShowParts = buildLevelRatio >= this.BlockDefinition.BuildProgressModels[this.BlockDefinition.BuildProgressModels.Length - 1].BuildRatioUpperBound;
                }
                if (fixSkeleton && !this.ShowParts)
                {
                    this.CubeGrid.FixSkeleton(this);
                }
            }
        }

        public void UpdateVisual(bool updatePhysics = true)
        {
            bool flag = false;
            this.UpdateShowParts(true);
            if (this.ShowParts)
            {
                if (this.FatBlock != null)
                {
                    Vector3D translation = this.FatBlock.WorldMatrix.Translation;
                    this.CubeGrid.Hierarchy.RemoveChild(this.FatBlock, false);
                    this.FatBlock.Close();
                    this.FatBlock = null;
                    flag = true;
                }
            }
            else if (this.FatBlock != null)
            {
                this.FatBlock.UpdateVisual();
            }
            else
            {
                this.FatBlock = new MyCubeBlock();
                this.FatBlock.SlimBlock = this;
                this.FatBlock.Init();
                this.CubeGrid.Hierarchy.AddChild(this.FatBlock, false, true);
            }
            this.CubeGrid.SetBlockDirty(this);
            if (flag)
            {
                this.CubeGrid.UpdateDirty(null, true);
            }
            if (updatePhysics && (this.CubeGrid.Physics != null))
            {
                this.CubeGrid.Physics.AddDirtyArea(this.Min, this.Max);
            }
        }

        public void UpgradeBuildLevel()
        {
            float buildRatio = this.m_componentStack.BuildRatio;
            float buildRatioUpperBound = 1f;
            foreach (MyCubeBlockDefinition.BuildProgressModel model in this.BlockDefinition.BuildProgressModels)
            {
                if ((model.BuildRatioUpperBound > buildRatio) && (model.BuildRatioUpperBound <= buildRatioUpperBound))
                {
                    buildRatioUpperBound = model.BuildRatioUpperBound;
                }
            }
            float num3 = MathHelper.Clamp((float) (buildRatioUpperBound * 1.001f), (float) 0f, (float) 1f);
            this.m_componentStack.SetIntegrity(num3 * this.BlockDefinition.MaxIntegrity, num3 * this.BlockDefinition.MaxIntegrity);
        }

        void VRage.Game.ModAPI.IMySlimBlock.AddNeighbours()
        {
            this.AddNeighbours();
        }

        void VRage.Game.ModAPI.IMySlimBlock.ApplyAccumulatedDamage(bool addDirtyParts)
        {
            this.ApplyAccumulatedDamage(addDirtyParts, 0L);
        }

        string VRage.Game.ModAPI.IMySlimBlock.CalculateCurrentModel(out Matrix orientation) => 
            this.CalculateCurrentModel(out orientation);

        bool VRage.Game.ModAPI.IMySlimBlock.CanContinueBuild(VRage.Game.ModAPI.IMyInventory sourceInventory) => 
            this.CanContinueBuild(sourceInventory as MyInventory);

        void VRage.Game.ModAPI.IMySlimBlock.ClearConstructionStockpile(VRage.Game.ModAPI.IMyInventory outputInventory)
        {
            this.ClearConstructionStockpile(outputInventory as MyInventoryBase);
        }

        void VRage.Game.ModAPI.IMySlimBlock.ComputeScaledCenter(out Vector3D scaledCenter)
        {
            this.ComputeScaledCenter(out scaledCenter);
        }

        void VRage.Game.ModAPI.IMySlimBlock.ComputeScaledHalfExtents(out Vector3 scaledHalfExtents)
        {
            this.ComputeScaledHalfExtents(out scaledHalfExtents);
        }

        void VRage.Game.ModAPI.IMySlimBlock.ComputeWorldCenter(out Vector3D worldCenter)
        {
            this.ComputeWorldCenter(out worldCenter);
        }

        void VRage.Game.ModAPI.IMySlimBlock.DecreaseMountLevel(float grinderAmount, VRage.Game.ModAPI.IMyInventory outputInventory, bool useDefaultDeconstructEfficiency)
        {
            this.DecreaseMountLevel(grinderAmount, outputInventory as MyInventoryBase, useDefaultDeconstructEfficiency);
        }

        void VRage.Game.ModAPI.IMySlimBlock.FixBones(float oldDamage, float maxAllowedBoneMovement)
        {
            this.FixBones(oldDamage, maxAllowedBoneMovement);
        }

        void VRage.Game.ModAPI.IMySlimBlock.FullyDismount(VRage.Game.ModAPI.IMyInventory outputInventory)
        {
            this.FullyDismount(outputInventory as MyInventory);
        }

        Vector3 VRage.Game.ModAPI.IMySlimBlock.GetColorMask() => 
            this.ColorMaskHSV;

        int VRage.Game.ModAPI.IMySlimBlock.GetConstructionStockpileItemAmount(MyDefinitionId id) => 
            this.GetConstructionStockpileItemAmount(id);

        MyObjectBuilder_CubeBlock VRage.Game.ModAPI.IMySlimBlock.GetCopyObjectBuilder() => 
            this.GetCopyObjectBuilder();

        MyObjectBuilder_CubeBlock VRage.Game.ModAPI.IMySlimBlock.GetObjectBuilder(bool copy) => 
            this.GetObjectBuilder(copy);

        void VRage.Game.ModAPI.IMySlimBlock.GetWorldBoundingBox(out BoundingBoxD aabb, bool useAABBFromBlockCubes)
        {
            this.GetWorldBoundingBox(out aabb, useAABBFromBlockCubes);
        }

        void VRage.Game.ModAPI.IMySlimBlock.IncreaseMountLevel(float welderMountAmount, long welderOwnerPlayerId, VRage.Game.ModAPI.IMyInventory outputInventory, float maxAllowedBoneMovement, bool isHelping, MyOwnershipShareModeEnum share)
        {
            this.IncreaseMountLevel(welderMountAmount, welderOwnerPlayerId, outputInventory as MyInventoryBase, maxAllowedBoneMovement, isHelping, share, false);
        }

        void VRage.Game.ModAPI.IMySlimBlock.InitOrientation(MyBlockOrientation orientation)
        {
            this.InitOrientation(orientation);
        }

        void VRage.Game.ModAPI.IMySlimBlock.InitOrientation(ref Vector3I forward, ref Vector3I up)
        {
            this.InitOrientation(ref forward, ref up);
        }

        void VRage.Game.ModAPI.IMySlimBlock.InitOrientation(Base6Directions.Direction Forward, Base6Directions.Direction Up)
        {
            this.InitOrientation(Forward, Up);
        }

        void VRage.Game.ModAPI.IMySlimBlock.MoveItemsFromConstructionStockpile(VRage.Game.ModAPI.IMyInventory toInventory, MyItemFlags flags)
        {
            this.MoveItemsFromConstructionStockpile(toInventory as MyInventory, flags);
        }

        void VRage.Game.ModAPI.IMySlimBlock.MoveItemsToConstructionStockpile(VRage.Game.ModAPI.IMyInventory fromInventory)
        {
            this.MoveItemsToConstructionStockpile(fromInventory as MyInventoryBase);
        }

        void VRage.Game.ModAPI.IMySlimBlock.RemoveNeighbours()
        {
            this.RemoveNeighbours();
        }

        void VRage.Game.ModAPI.IMySlimBlock.SetToConstructionSite()
        {
            this.SetToConstructionSite();
        }

        void VRage.Game.ModAPI.IMySlimBlock.SpawnConstructionStockpile()
        {
            this.SpawnConstructionStockpile();
        }

        void VRage.Game.ModAPI.IMySlimBlock.SpawnFirstItemInConstructionStockpile()
        {
            this.SpawnFirstItemInConstructionStockpile();
        }

        void VRage.Game.ModAPI.IMySlimBlock.UpdateVisual()
        {
            this.UpdateVisual(true);
        }

        void VRage.Game.ModAPI.Ingame.IMySlimBlock.GetMissingComponents(Dictionary<string, int> addToDictionary)
        {
            this.GetMissingComponents(addToDictionary);
        }

        unsafe void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
            MyCubeGrid.MyCubeGridHitInfo gridHitInfo = customdata as MyCubeGrid.MyCubeGridHitInfo;
            if (gridHitInfo != null)
            {
                MyDecalRenderInfo* infoPtr1;
                MyDecalRenderInfo info3 = new MyDecalRenderInfo {
                    Source = source
                };
                infoPtr1->Material = (material.GetHashCode() == 0) ? MyStringHash.GetOrCompute(this.BlockDefinition.PhysicalMaterial.Id.SubtypeName) : material;
                infoPtr1 = (MyDecalRenderInfo*) ref info3;
                MyDecalRenderInfo renderInfo = info3;
                if (this.FatBlock == null)
                {
                    renderInfo.Position = Vector3D.Transform(hitInfo.Position, this.CubeGrid.PositionComp.WorldMatrixInvScaled);
                    renderInfo.Normal = (Vector3) Vector3D.TransformNormal(hitInfo.Normal, this.CubeGrid.PositionComp.WorldMatrixInvScaled);
                    renderInfo.RenderObjectIds = this.CubeGrid.Render.RenderObjectIDs;
                }
                else
                {
                    renderInfo.Position = Vector3D.Transform(hitInfo.Position, this.FatBlock.PositionComp.WorldMatrixInvScaled);
                    renderInfo.Normal = (Vector3) Vector3D.TransformNormal(hitInfo.Normal, this.FatBlock.PositionComp.WorldMatrixInvScaled);
                    renderInfo.RenderObjectIds = this.FatBlock.Render.RenderObjectIDs;
                }
                VertexBoneIndicesWeights? affectingBoneIndicesWeights = gridHitInfo.Triangle.GetAffectingBoneIndicesWeights(ref m_boneIndexWeightTmp);
                if (affectingBoneIndicesWeights != null)
                {
                    renderInfo.BoneIndices = affectingBoneIndicesWeights.Value.Indices;
                    renderInfo.BoneWeights = affectingBoneIndicesWeights.Value.Weights;
                }
                if (m_tmpIds == null)
                {
                    m_tmpIds = new List<uint>();
                }
                else
                {
                    m_tmpIds.Clear();
                }
                decalHandler.AddDecal(ref renderInfo, m_tmpIds);
                foreach (uint num in m_tmpIds)
                {
                    this.CubeGrid.RenderData.AddDecal(this.Position, gridHitInfo, num);
                }
            }
        }

        void IMyDestroyableObject.OnDestroy()
        {
            if (this.FatBlock != null)
            {
                this.FatBlock.OnDestroy();
            }
            this.OnDestroyVisual();
            this.m_componentStack.DestroyCompletely();
            this.ReleaseUnneededStockpileItems();
            this.CubeGrid.RemoveFromDamageApplication(this);
            this.AccumulatedDamage = 0f;
        }

        private static Dictionary<string, int> m_tmpComponents =>
            MyUtils.Init<Dictionary<string, int>>(ref m_tmpComponentsPerThread);

        private static List<MyStockpileItem> m_tmpItemList =>
            MyUtils.Init<List<MyStockpileItem>>(ref m_tmpItemListPerThread);

        private static List<Vector3I> m_tmpCubeNeighbours =>
            MyUtils.Init<List<Vector3I>>(ref m_tmpCubeNeighboursPerThread);

        private static List<MySlimBlock> m_tmpBlocks =>
            MyUtils.Init<List<MySlimBlock>>(ref m_tmpBlocksPerThread);

        private static List<MySlimBlock> m_tmpMultiBlocks =>
            MyUtils.Init<List<MySlimBlock>>(ref m_tmpMultiBlocksPerThread);

        public float AccumulatedDamage
        {
            get => 
                this.m_accumulatedDamage;
            private set
            {
                this.m_accumulatedDamage = value;
                if (this.m_accumulatedDamage > 0f)
                {
                    this.CubeGrid.AddForDamageApplication(this);
                }
            }
        }

        public MyCubeBlock FatBlock { get; private set; }

        public Vector3D WorldPosition =>
            this.CubeGrid.GridIntegerToWorld(this.Position);

        public BoundingBoxD WorldAABB =>
            new BoundingBoxD((Vector3D) ((this.Min * this.CubeGrid.GridSize) - this.CubeGrid.GridSizeHalfVector), (this.Max * this.CubeGrid.GridSize) + this.CubeGrid.GridSizeHalfVector).TransformFast(this.CubeGrid.PositionComp.WorldMatrix);

        public MyCubeGrid CubeGrid
        {
            get => 
                this.m_cubeGrid;
            set
            {
                if (!ReferenceEquals(this.m_cubeGrid, value))
                {
                    bool flag = ReferenceEquals(this.m_cubeGrid, null);
                    MyCubeGrid cubeGrid = this.m_cubeGrid;
                    this.m_cubeGrid = value;
                    if ((this.FatBlock != null) && !flag)
                    {
                        this.FatBlock.OnCubeGridChanged(cubeGrid);
                        if (this.CubeGridChanged != null)
                        {
                            this.CubeGridChanged(this, cubeGrid);
                        }
                    }
                }
            }
        }

        public bool ShowParts { get; private set; }

        public bool IsFullIntegrity =>
            ((this.m_componentStack == null) || this.m_componentStack.IsFullIntegrity);

        public float BuildLevelRatio =>
            this.m_componentStack.BuildRatio;

        public float BuildIntegrity =>
            this.m_componentStack.BuildIntegrity;

        public bool IsFullyDismounted =>
            this.m_componentStack.IsFullyDismounted;

        public bool IsDestroyed =>
            this.m_componentStack.IsDestroyed;

        public bool UseDamageSystem { get; private set; }

        public float Integrity =>
            this.m_componentStack.Integrity;

        public float MaxIntegrity =>
            this.m_componentStack.MaxIntegrity;

        public float CurrentDamage =>
            (this.BuildIntegrity - this.Integrity);

        public float DamageRatio =>
            (2f - (this.m_componentStack.BuildIntegrity / this.MaxIntegrity));

        public bool StockpileAllocated =>
            (this.m_stockpile != null);

        public bool StockpileEmpty =>
            (!this.StockpileAllocated || this.m_stockpile.IsEmpty());

        public bool HasDeformation =>
            ((this.CubeGrid != null) && this.CubeGrid.Skeleton.IsDeformed(this.Position, 0f, this.CubeGrid, true));

        public float MaxDeformation =>
            this.m_cachedMaxDeformation;

        public MyComponentStack ComponentStack =>
            this.m_componentStack;

        public bool YieldLastComponent =>
            this.m_componentStack.YieldLastComponent;

        public long BuiltBy =>
            this.m_builtByID;

        public int UniqueId { get; private set; }

        public bool IsMultiBlockPart =>
            (MyFakes.ENABLE_MULTIBLOCK_PART_IDS && ((this.MultiBlockId != 0) && ((this.MultiBlockDefinition != null) && (this.MultiBlockIndex != -1))));

        public bool ForceBlockDestructible =>
            ((this.FatBlock != null) ? this.FatBlock.ForceBlockDestructible : false);

        public long OwnerId
        {
            get
            {
                MyGridOwnershipComponentBase base2;
                if ((this.FatBlock != null) && (this.FatBlock.OwnerId != 0))
                {
                    return this.FatBlock.OwnerId;
                }
                this.CubeGrid.Components.TryGet<MyGridOwnershipComponentBase>(out base2);
                return ((base2 == null) ? 0L : base2.GetBlockOwnerId(this));
            }
        }

        float IMyDestroyableObject.Integrity =>
            this.Integrity;

        VRage.Game.ModAPI.IMyCubeBlock VRage.Game.ModAPI.IMySlimBlock.FatBlock =>
            this.FatBlock;

        VRage.Game.ModAPI.Ingame.IMyCubeBlock VRage.Game.ModAPI.Ingame.IMySlimBlock.FatBlock =>
            this.FatBlock;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.AccumulatedDamage =>
            this.AccumulatedDamage;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.BuildIntegrity =>
            this.BuildIntegrity;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.BuildLevelRatio =>
            this.BuildLevelRatio;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.CurrentDamage =>
            this.CurrentDamage;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.DamageRatio =>
            this.DamageRatio;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.HasDeformation =>
            this.HasDeformation;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.IsDestroyed =>
            this.IsDestroyed;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.IsFullIntegrity =>
            this.IsFullIntegrity;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.IsFullyDismounted =>
            this.IsFullyDismounted;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.MaxDeformation =>
            this.MaxDeformation;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.MaxIntegrity =>
            this.MaxIntegrity;

        float VRage.Game.ModAPI.Ingame.IMySlimBlock.Mass =>
            this.GetMass();

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.ShowParts =>
            this.ShowParts;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.StockpileAllocated =>
            this.StockpileAllocated;

        bool VRage.Game.ModAPI.Ingame.IMySlimBlock.StockpileEmpty =>
            this.StockpileEmpty;

        Vector3I VRage.Game.ModAPI.Ingame.IMySlimBlock.Position =>
            this.Position;

        VRage.Game.ModAPI.Ingame.IMyCubeGrid VRage.Game.ModAPI.Ingame.IMySlimBlock.CubeGrid =>
            this.CubeGrid;

        Vector3 VRage.Game.ModAPI.Ingame.IMySlimBlock.ColorMaskHSV =>
            this.ColorMaskHSV;

        MyStringHash VRage.Game.ModAPI.Ingame.IMySlimBlock.SkinSubtypeId =>
            this.SkinSubtypeId;

        VRage.Game.ModAPI.IMyCubeGrid VRage.Game.ModAPI.IMySlimBlock.CubeGrid =>
            this.CubeGrid;

        MyDefinitionBase VRage.Game.ModAPI.IMySlimBlock.BlockDefinition =>
            this.BlockDefinition;

        Vector3I VRage.Game.ModAPI.IMySlimBlock.Max =>
            this.Max;

        Vector3I VRage.Game.ModAPI.IMySlimBlock.Min =>
            this.Min;

        MyBlockOrientation VRage.Game.ModAPI.IMySlimBlock.Orientation =>
            this.Orientation;

        List<VRage.Game.ModAPI.IMySlimBlock> VRage.Game.ModAPI.IMySlimBlock.Neighbours =>
            this.Neighbours.Cast<VRage.Game.ModAPI.IMySlimBlock>().ToList<VRage.Game.ModAPI.IMySlimBlock>();

        float VRage.Game.ModAPI.IMySlimBlock.Dithering
        {
            get => 
                this.Dithering;
            set
            {
                this.Dithering = value;
                this.UpdateVisual(false);
            }
        }

        long VRage.Game.ModAPI.Ingame.IMySlimBlock.OwnerId =>
            this.OwnerId;

        SerializableDefinitionId VRage.Game.ModAPI.Ingame.IMySlimBlock.BlockDefinition =>
            ((SerializableDefinitionId) this.BlockDefinition.Id);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySlimBlock.<>c <>9 = new MySlimBlock.<>c();
            public static Func<IMyEventOwner, Action<long, List<MyTuple<Vector3I, float>>, MyStringHash, long>> <>9__212_0;
            public static Func<IMyEventOwner, Action<MySlimBlock.DoDamageSlimBlockMsg>> <>9__214_0;

            internal Action<MySlimBlock.DoDamageSlimBlockMsg> <SendDamage>b__214_0(IMyEventOwner s) => 
                new Action<MySlimBlock.DoDamageSlimBlockMsg>(MySlimBlock.DoDamageSlimBlock);

            internal Action<long, List<MyTuple<Vector3I, float>>, MyStringHash, long> <SendDamageBatch>b__212_0(IMyEventOwner s) => 
                new Action<long, List<MyTuple<Vector3I, float>>, MyStringHash, long>(MySlimBlock.DoDamageSlimBlockBatch);
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        private struct DoDamageSlimBlockMsg
        {
            [ProtoMember(0xb13)]
            public long GridEntityId;
            [ProtoMember(0xb16)]
            public Vector3I Position;
            [ProtoMember(0xb19)]
            public float Damage;
            [ProtoMember(0xb1c)]
            public MyStringHash Type;
            [ProtoMember(0xb1f)]
            public MyHitInfo? HitInfo;
            [ProtoMember(0xb22)]
            public long AttackerEntityId;
            [ProtoMember(0xb25)]
            public uint CompoundBlockId;
        }
    }
}


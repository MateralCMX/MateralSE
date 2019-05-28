namespace Sandbox.Game.AI
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.EnvironmentItems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyAiTargetBase
    {
        private const int UNREACHABLE_ENTITY_TIMEOUT = 0x4e20;
        private const int UNREACHABLE_BLOCK_TIMEOUT = 0xea60;
        private const int UNREACHABLE_CHARACTER_TIMEOUT = 0x4e20;
        protected MyAiTargetEnum m_currentTarget;
        protected IMyEntityBot m_user;
        protected MyAgentBot m_bot;
        protected VRage.Game.Entity.MyEntity m_targetEntity;
        protected Vector3I m_targetCube = Vector3I.Zero;
        protected Vector3D m_targetPosition = Vector3D.Zero;
        protected Vector3I m_targetInVoxelCoord = Vector3I.Zero;
        protected ushort? m_compoundId;
        protected int m_targetTreeId;
        protected Dictionary<VRage.Game.Entity.MyEntity, int> m_unreachableEntities = new Dictionary<VRage.Game.Entity.MyEntity, int>();
        protected Dictionary<Tuple<VRage.Game.Entity.MyEntity, int>, int> m_unreachableTrees = new Dictionary<Tuple<VRage.Game.Entity.MyEntity, int>, int>();
        protected static List<VRage.Game.Entity.MyEntity> m_tmpEntities;
        protected static List<Tuple<VRage.Game.Entity.MyEntity, int>> m_tmpTrees;

        public MyAiTargetBase(IMyEntityBot bot)
        {
            this.m_user = bot;
            this.m_bot = bot as MyAgentBot;
            this.m_currentTarget = MyAiTargetEnum.NO_TARGET;
            MyAiTargetManager.AddAiTarget(this);
        }

        private void AddUnreachableEntity(VRage.Game.Entity.MyEntity entity, int timeout)
        {
            this.m_unreachableEntities[entity] = MySandboxGame.TotalGamePlayTimeInMilliseconds + timeout;
            entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableEntity);
            entity.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableEntity);
        }

        private void AddUnreachableTree(VRage.Game.Entity.MyEntity entity, int treeId, int timeout)
        {
            this.m_unreachableTrees[new Tuple<VRage.Game.Entity.MyEntity, int>(entity, treeId)] = MySandboxGame.TotalGamePlayTimeInMilliseconds + timeout;
            entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableTrees);
            entity.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableTrees);
        }

        public void AimAtTarget()
        {
            if (this.HasTarget())
            {
                if ((this.m_currentTarget == MyAiTargetEnum.POSITION) || (this.m_currentTarget == MyAiTargetEnum.VOXEL))
                {
                    this.m_bot.Navigation.AimAt(null, new Vector3D?(this.m_targetPosition));
                }
                else
                {
                    this.SetMTargetPosition(this.GetAimAtPosition(this.m_bot.Navigation.AimingPositionAndOrientation.Translation));
                    this.m_bot.Navigation.AimAt(this.m_targetEntity, new Vector3D?(this.m_targetPosition));
                }
            }
        }

        private void BlockRemoved(MySlimBlock block)
        {
            MyCubeGrid targetGrid = this.TargetGrid;
            if (this.GetCubeBlock() == null)
            {
                this.UnsetTargetEntity();
            }
        }

        public virtual void Cleanup()
        {
            MyAiTargetManager.RemoveAiTarget(this);
        }

        private void Clear()
        {
            this.m_currentTarget = MyAiTargetEnum.NO_TARGET;
            this.m_targetEntity = null;
            this.m_targetCube = Vector3I.Zero;
            this.m_targetPosition = Vector3D.Zero;
            this.m_targetInVoxelCoord = Vector3I.Zero;
            this.m_compoundId = null;
            this.m_targetTreeId = 0;
        }

        public void ClearUnreachableEntities()
        {
            this.m_unreachableEntities.Clear();
        }

        public virtual void DebugDraw()
        {
        }

        public virtual void DrawLineToTarget(Vector3D from)
        {
        }

        public Vector3D GetAimAtPosition(Vector3D startingPosition)
        {
            if (!this.HasTarget())
            {
                return Vector3D.Zero;
            }
            if (this.m_currentTarget == MyAiTargetEnum.POSITION)
            {
                return this.m_targetPosition;
            }
            if (this.m_currentTarget == MyAiTargetEnum.ENVIRONMENT_ITEM)
            {
                return this.m_targetPosition;
            }
            Vector3D position = this.m_targetEntity.PositionComp.GetPosition();
            if (this.m_currentTarget == MyAiTargetEnum.CUBE)
            {
                this.GetLocalCubeProjectedPosition(ref startingPosition);
                position = this.TargetCubeWorldPosition;
            }
            else if (this.m_currentTarget == MyAiTargetEnum.CHARACTER)
            {
                MyPositionComponentBase positionComp = (this.m_targetEntity as MyCharacter).PositionComp;
                position = Vector3D.Transform(positionComp.LocalVolume.Center, positionComp.WorldMatrix);
            }
            else if (this.m_currentTarget == MyAiTargetEnum.VOXEL)
            {
                position = this.m_targetPosition;
            }
            else if (((this.m_currentTarget == MyAiTargetEnum.ENTITY) && (this.m_targetPosition != Vector3D.Zero)) && (this.m_targetEntity is MyFracturedPiece))
            {
                position = this.m_targetPosition;
            }
            return position;
        }

        protected MySlimBlock GetCubeBlock()
        {
            if (this.m_compoundId == null)
            {
                return this.TargetGrid?.GetCubeBlock(this.m_targetCube);
            }
            MySlimBlock cubeBlock = this.TargetGrid.GetCubeBlock(this.m_targetCube);
            return ((cubeBlock != null) ? (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlock(this.m_compoundId.Value) : null);
        }

        protected Vector3D GetLocalCubeProjectedPosition(ref Vector3D toProject)
        {
            this.GetCubeBlock();
            Vector3D vectord = Vector3D.Transform(toProject, this.TargetGrid.PositionComp.WorldMatrixNormalizedInv) - ((this.m_targetCube + new Vector3(0.5f)) * this.TargetGrid.GridSize);
            float num = 1f;
            num = (Math.Abs(vectord.Y) <= Math.Abs(vectord.Z)) ? ((Math.Abs(vectord.Z) <= Math.Abs(vectord.X)) ? (1f / ((float) Math.Abs(vectord.X))) : (1f / ((float) Math.Abs(vectord.Z)))) : ((Math.Abs(vectord.Y) <= Math.Abs(vectord.X)) ? (1f / ((float) Math.Abs(vectord.X))) : (1f / ((float) Math.Abs(vectord.Y))));
            return ((vectord * num) * (this.TargetGrid.GridSize * 0.5f));
        }

        public Vector3D? GetMemoryTargetPosition(MyBBMemoryTarget targetMemory)
        {
            if (targetMemory != null)
            {
                switch (targetMemory.TargetType)
                {
                    case MyAiTargetEnum.GRID:
                    case MyAiTargetEnum.CHARACTER:
                    case MyAiTargetEnum.ENTITY:
                    {
                        MyCharacter entity = null;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCharacter>(targetMemory.EntityId.Value, out entity, false))
                        {
                            return new Vector3D?(entity.PositionComp.GetPosition());
                        }
                        return null;
                    }
                    case MyAiTargetEnum.CUBE:
                    case MyAiTargetEnum.COMPOUND_BLOCK:
                    {
                        MyCubeGrid entity = null;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(targetMemory.EntityId.Value, out entity, false) && entity.CubeExists(targetMemory.BlockPosition))
                        {
                            return new Vector3D?(entity.GridIntegerToWorld(targetMemory.BlockPosition));
                        }
                        return null;
                    }
                    case MyAiTargetEnum.POSITION:
                    case MyAiTargetEnum.ENVIRONMENT_ITEM:
                    case MyAiTargetEnum.VOXEL:
                        return new Vector3D?(this.m_targetPosition);
                }
            }
            return null;
        }

        public virtual MyObjectBuilder_AiTarget GetObjectBuilder()
        {
            long? nullable1;
            MyObjectBuilder_AiTarget target = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_AiTarget>();
            if (this.m_targetEntity != null)
            {
                nullable1 = new long?(this.m_targetEntity.EntityId);
            }
            else
            {
                nullable1 = null;
            }
            target.EntityId = nullable1;
            target.CurrentTarget = this.m_currentTarget;
            target.TargetCube = this.m_targetCube;
            target.TargetPosition = this.m_targetPosition;
            target.CompoundId = this.m_compoundId;
            target.UnreachableEntities = new List<MyObjectBuilder_AiTarget.UnreachableEntitiesData>();
            foreach (KeyValuePair<VRage.Game.Entity.MyEntity, int> pair in this.m_unreachableEntities)
            {
                MyObjectBuilder_AiTarget.UnreachableEntitiesData item = new MyObjectBuilder_AiTarget.UnreachableEntitiesData {
                    UnreachableEntityId = pair.Key.EntityId,
                    Timeout = pair.Value - MySandboxGame.TotalGamePlayTimeInMilliseconds
                };
                target.UnreachableEntities.Add(item);
            }
            return target;
        }

        public virtual bool GetRandomDirectedPosition(Vector3D initPosition, Vector3D direction, out Vector3D outPosition)
        {
            outPosition = MySession.Static.LocalCharacter.PositionComp.GetPosition();
            return true;
        }

        public MySlimBlock GetTargetBlock() => 
            ((this.m_currentTarget == MyAiTargetEnum.CUBE) ? ((this.TargetGrid != null) ? this.GetCubeBlock() : null) : null);

        public Vector3D GetTargetPosition(Vector3D startingPosition)
        {
            float num;
            Vector3D vectord;
            this.GetTargetPosition(startingPosition, out vectord, out num);
            return vectord;
        }

        public void GetTargetPosition(Vector3D startingPosition, out Vector3D targetPosition, out float radius)
        {
            targetPosition = new Vector3D();
            radius = 0f;
            if (this.HasTarget())
            {
                if (this.m_currentTarget == MyAiTargetEnum.POSITION)
                {
                    targetPosition = this.m_targetPosition;
                }
                else
                {
                    Vector3D position = this.m_targetEntity.PositionComp.GetPosition();
                    radius = 0.75f;
                    if (this.m_currentTarget == MyAiTargetEnum.CUBE)
                    {
                        Vector3D localCubeProjectedPosition = this.GetLocalCubeProjectedPosition(ref startingPosition);
                        radius = ((float) localCubeProjectedPosition.Length()) * 0.3f;
                        position = this.TargetCubeWorldPosition + localCubeProjectedPosition;
                    }
                    else if (this.m_currentTarget == MyAiTargetEnum.CHARACTER)
                    {
                        radius = 0.65f;
                        position = (this.m_targetEntity as MyCharacter).PositionComp.WorldVolume.Center;
                    }
                    else if (this.m_currentTarget == MyAiTargetEnum.ENVIRONMENT_ITEM)
                    {
                        position = this.m_targetPosition;
                        radius = 0.75f;
                    }
                    else if (this.m_currentTarget == MyAiTargetEnum.VOXEL)
                    {
                        position = this.m_targetPosition;
                    }
                    else if (this.m_currentTarget == MyAiTargetEnum.ENTITY)
                    {
                        if ((this.m_targetPosition != Vector3D.Zero) && (this.m_targetEntity is MyFracturedPiece))
                        {
                            position = this.m_targetPosition;
                        }
                        radius = this.m_targetEntity.PositionComp.LocalAABB.HalfExtents.Length();
                    }
                    targetPosition = position;
                }
            }
        }

        public void GotoFailed()
        {
            this.HasGotoFailed = true;
            if (this.m_currentTarget == MyAiTargetEnum.CHARACTER)
            {
                this.AddUnreachableEntity(this.m_targetEntity, 0x4e20);
            }
            else if (this.m_currentTarget == MyAiTargetEnum.CUBE)
            {
                VRage.Game.Entity.MyEntity targetEntity = this.m_targetEntity;
                MySlimBlock cubeBlock = this.GetCubeBlock();
                if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                {
                    this.AddUnreachableEntity(cubeBlock.FatBlock, 0xea60);
                }
            }
            else if ((this.m_targetEntity != null) && (this.m_targetEntity is MyTrees))
            {
                this.AddUnreachableTree(this.m_targetEntity, this.m_targetTreeId, 0x4e20);
            }
            else if ((this.m_targetEntity != null) && (this.m_currentTarget != MyAiTargetEnum.VOXEL))
            {
                this.AddUnreachableEntity(this.m_targetEntity, 0x4e20);
            }
            this.UnsetTarget();
        }

        public void GotoTarget()
        {
            if (this.HasTarget())
            {
                if ((this.m_currentTarget == MyAiTargetEnum.POSITION) || (this.m_currentTarget == MyAiTargetEnum.VOXEL))
                {
                    this.m_bot.Navigation.Goto(this.m_targetPosition, 0f, this.m_targetEntity);
                }
                else
                {
                    Vector3D vectord;
                    float num;
                    this.GetTargetPosition(this.m_bot.Navigation.PositionAndOrientation.Translation, out vectord, out num);
                    this.m_bot.Navigation.Goto(vectord, num, this.m_targetEntity);
                }
            }
        }

        public void GotoTargetNoPath(float radius, bool resetStuckDetection = true)
        {
            if (this.HasTarget())
            {
                if ((this.m_currentTarget == MyAiTargetEnum.POSITION) || (this.m_currentTarget == MyAiTargetEnum.VOXEL))
                {
                    this.m_bot.Navigation.GotoNoPath(this.m_targetPosition, radius, null, true);
                }
                else
                {
                    Vector3D vectord;
                    float num;
                    this.GetTargetPosition(this.m_bot.Navigation.PositionAndOrientation.Translation, out vectord, out num);
                    this.m_bot.Navigation.GotoNoPath(vectord, radius + num, null, resetStuckDetection);
                }
            }
        }

        public bool HasTarget() => 
            (this.m_currentTarget != MyAiTargetEnum.NO_TARGET);

        public virtual void Init(MyObjectBuilder_AiTarget builder)
        {
            this.m_currentTarget = builder.CurrentTarget;
            this.m_targetEntity = null;
            if (builder.EntityId == null)
            {
                this.m_currentTarget = MyAiTargetEnum.NO_TARGET;
            }
            else if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(builder.EntityId.Value, out this.m_targetEntity, false))
            {
                this.m_currentTarget = MyAiTargetEnum.NO_TARGET;
            }
            this.m_targetCube = builder.TargetCube;
            this.SetMTargetPosition(builder.TargetPosition);
            this.m_compoundId = builder.CompoundId;
            if (builder.UnreachableEntities != null)
            {
                foreach (MyObjectBuilder_AiTarget.UnreachableEntitiesData data in builder.UnreachableEntities)
                {
                    VRage.Game.Entity.MyEntity entity = null;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.UnreachableEntityId, out entity, false))
                    {
                        this.m_unreachableEntities.Add(entity, MySandboxGame.TotalGamePlayTimeInMilliseconds + data.Timeout);
                    }
                }
            }
        }

        public bool IsEntityReachable(VRage.Game.Entity.MyEntity entity)
        {
            if (entity == null)
            {
                return false;
            }
            bool flag = true;
            if (entity.Parent != null)
            {
                flag &= this.IsEntityReachable(entity.Parent);
            }
            return (flag && !this.m_unreachableEntities.ContainsKey(entity));
        }

        public virtual bool IsMemoryTargetValid(MyBBMemoryTarget targetMemory)
        {
            if (targetMemory != null)
            {
                switch (targetMemory.TargetType)
                {
                    case MyAiTargetEnum.GRID:
                    case MyAiTargetEnum.ENTITY:
                    {
                        VRage.Game.Entity.MyEntity entity = null;
                        return (Sandbox.Game.Entities.MyEntities.TryGetEntityById(targetMemory.EntityId.Value, out entity, false) && this.IsEntityReachable(entity));
                    }
                    case MyAiTargetEnum.CUBE:
                    case MyAiTargetEnum.COMPOUND_BLOCK:
                    {
                        MyCubeGrid grid = null;
                        if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(targetMemory.EntityId.Value, out grid, false))
                        {
                            return false;
                        }
                        MySlimBlock cubeBlock = grid.GetCubeBlock(targetMemory.BlockPosition);
                        return ((cubeBlock != null) ? ((cubeBlock.FatBlock == null) ? this.IsEntityReachable(grid) : this.IsEntityReachable(cubeBlock.FatBlock)) : false);
                    }
                    case MyAiTargetEnum.CHARACTER:
                    {
                        MyCharacter character = null;
                        return (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCharacter>(targetMemory.EntityId.Value, out character, false) && this.IsEntityReachable(character));
                    }
                    case MyAiTargetEnum.ENVIRONMENT_ITEM:
                    case MyAiTargetEnum.VOXEL:
                        return true;
                }
            }
            return false;
        }

        public bool IsTargetGridOrBlock(MyAiTargetEnum type) => 
            ((type == MyAiTargetEnum.CUBE) || (type == MyAiTargetEnum.GRID));

        public virtual bool IsTargetValid()
        {
            switch (this.m_currentTarget)
            {
                case MyAiTargetEnum.GRID:
                case MyAiTargetEnum.ENTITY:
                    return this.IsEntityReachable(this.m_targetEntity);

                case MyAiTargetEnum.CUBE:
                case MyAiTargetEnum.COMPOUND_BLOCK:
                {
                    MyCubeGrid targetEntity = this.m_targetEntity as MyCubeGrid;
                    if (targetEntity == null)
                    {
                        return false;
                    }
                    MySlimBlock cubeBlock = targetEntity.GetCubeBlock(this.m_targetCube);
                    return ((cubeBlock != null) ? ((cubeBlock.FatBlock == null) ? this.IsEntityReachable(targetEntity) : this.IsEntityReachable(cubeBlock.FatBlock)) : false);
                }
                case MyAiTargetEnum.CHARACTER:
                {
                    MyCharacter targetEntity = this.m_targetEntity as MyCharacter;
                    return ((targetEntity != null) && this.IsEntityReachable(targetEntity));
                }
                case MyAiTargetEnum.ENVIRONMENT_ITEM:
                case MyAiTargetEnum.VOXEL:
                    return true;
            }
            return false;
        }

        public bool IsTreeReachable(VRage.Game.Entity.MyEntity entity, int treeId)
        {
            if (entity == null)
            {
                return false;
            }
            bool flag = true;
            if (entity.Parent != null)
            {
                flag &= this.IsEntityReachable(entity.Parent);
            }
            return (flag && !this.m_unreachableTrees.ContainsKey(new Tuple<VRage.Game.Entity.MyEntity, int>(entity, treeId)));
        }

        public bool PositionIsNearTarget(Vector3D position, float radius)
        {
            Vector3D vectord;
            float num;
            if (!this.HasTarget())
            {
                return false;
            }
            this.GetTargetPosition(position, out vectord, out num);
            return (Vector3D.Distance(position, vectord) <= (radius + num));
        }

        private void RemoveUnreachableEntity(VRage.Game.Entity.MyEntity entity)
        {
            entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableEntity);
            this.m_unreachableEntities.Remove(entity);
        }

        private void RemoveUnreachableTree(Tuple<VRage.Game.Entity.MyEntity, int> tree)
        {
            this.m_unreachableTrees.Remove(tree);
        }

        private void RemoveUnreachableTrees(VRage.Game.Entity.MyEntity entity)
        {
            entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.RemoveUnreachableTrees);
            using (MyUtils.ReuseCollection<Tuple<VRage.Game.Entity.MyEntity, int>>(ref m_tmpTrees))
            {
                foreach (Tuple<VRage.Game.Entity.MyEntity, int> tuple in this.m_unreachableTrees.Keys)
                {
                    if (tuple.Item1 == entity)
                    {
                        m_tmpTrees.Add(tuple);
                    }
                }
                foreach (Tuple<VRage.Game.Entity.MyEntity, int> tuple2 in m_tmpTrees)
                {
                    this.RemoveUnreachableTree(tuple2);
                }
            }
        }

        private void SetMTargetPosition(Vector3D pos)
        {
            this.m_targetPosition = pos;
        }

        public void SetTargetBlock(MySlimBlock slimBlock, ushort? compoundId = new ushort?())
        {
            if (!ReferenceEquals(this.m_targetEntity, slimBlock.CubeGrid))
            {
                this.SetTargetEntity(slimBlock.CubeGrid);
            }
            this.m_targetCube = slimBlock.Position;
            this.m_currentTarget = MyAiTargetEnum.CUBE;
        }

        protected virtual void SetTargetEntity(VRage.Game.Entity.MyEntity entity)
        {
            switch (entity)
            {
                case (MyCubeBlock _):
                {
                    ushort? compoundId = null;
                    this.SetTargetBlock((entity as MyCubeBlock).SlimBlock, compoundId);
                    break;
                }
                case (null):
                    break;

                default:
                    this.UnsetTargetEntity();
                    break;
            }
        }

        public virtual bool SetTargetFromMemory(MyBBMemoryTarget memoryTarget)
        {
            if (memoryTarget.TargetType == MyAiTargetEnum.POSITION)
            {
                if (memoryTarget.Position == null)
                {
                    return false;
                }
                this.SetTargetPosition(memoryTarget.Position.Value);
                return true;
            }
            if (memoryTarget.TargetType == MyAiTargetEnum.ENVIRONMENT_ITEM)
            {
                if (memoryTarget.TreeId == null)
                {
                    return false;
                }
                MyEnvironmentItems.ItemInfo targetTree = new MyEnvironmentItems.ItemInfo {
                    LocalId = memoryTarget.TreeId.Value,
                    Transform = { Position = memoryTarget.Position.Value }
                };
                this.SetTargetTree(ref targetTree, memoryTarget.EntityId.Value);
                return true;
            }
            if (memoryTarget.TargetType == MyAiTargetEnum.NO_TARGET)
            {
                if (memoryTarget.TargetType == MyAiTargetEnum.NO_TARGET)
                {
                    this.UnsetTarget();
                    return true;
                }
                this.UnsetTarget();
                return false;
            }
            if (memoryTarget.EntityId == null)
            {
                return false;
            }
            VRage.Game.Entity.MyEntity entity = null;
            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(memoryTarget.EntityId.Value, out entity, false))
            {
                this.UnsetTarget();
                return false;
            }
            if ((memoryTarget.TargetType == MyAiTargetEnum.CUBE) || (memoryTarget.TargetType == MyAiTargetEnum.COMPOUND_BLOCK))
            {
                MySlimBlock cubeBlock = (entity as MyCubeGrid).GetCubeBlock(memoryTarget.BlockPosition);
                if (cubeBlock == null)
                {
                    return false;
                }
                if (memoryTarget.TargetType == MyAiTargetEnum.COMPOUND_BLOCK)
                {
                    MySlimBlock block = (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlock(memoryTarget.CompoundId.Value);
                    if (block == null)
                    {
                        return false;
                    }
                    cubeBlock = block;
                    this.m_compoundId = memoryTarget.CompoundId;
                }
                ushort? compoundId = null;
                this.SetTargetBlock(cubeBlock, compoundId);
            }
            else if (memoryTarget.TargetType == MyAiTargetEnum.ENTITY)
            {
                if ((memoryTarget.Position == null) || !(entity is MyFracturedPiece))
                {
                    this.SetMTargetPosition(entity.PositionComp.GetPosition());
                }
                else
                {
                    this.SetMTargetPosition(memoryTarget.Position.Value);
                }
                this.SetTargetEntity(entity);
                this.m_targetEntity = entity;
            }
            else if (memoryTarget.TargetType != MyAiTargetEnum.VOXEL)
            {
                this.SetTargetEntity(entity);
            }
            else
            {
                MyVoxelMap voxelMap = entity as MyVoxelMap;
                if (memoryTarget.Position == null)
                {
                    return false;
                }
                if (voxelMap == null)
                {
                    return false;
                }
                this.SetTargetVoxel(memoryTarget.Position.Value, voxelMap);
                this.m_targetEntity = voxelMap;
            }
            return true;
        }

        public void SetTargetPosition(Vector3D pos)
        {
            this.UnsetTarget();
            this.SetMTargetPosition(pos);
            this.m_currentTarget = MyAiTargetEnum.POSITION;
        }

        public void SetTargetTree(ref MyEnvironmentItems.ItemInfo targetTree, long treesId)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(treesId, out entity, false))
            {
                this.UnsetTarget();
                this.SetMTargetPosition(targetTree.Transform.Position);
                this.m_targetEntity = entity;
                this.m_targetTreeId = targetTree.LocalId;
                this.SetTargetEntity(entity);
            }
        }

        public void SetTargetVoxel(Vector3D pos, MyVoxelMap voxelMap)
        {
            this.UnsetTarget();
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref pos, out this.m_targetInVoxelCoord);
            this.SetMTargetPosition(pos);
            this.m_currentTarget = MyAiTargetEnum.VOXEL;
        }

        public virtual void UnsetTarget()
        {
            switch (this.m_currentTarget)
            {
                case MyAiTargetEnum.NO_TARGET:
                case MyAiTargetEnum.GRID:
                case MyAiTargetEnum.CUBE:
                case MyAiTargetEnum.CHARACTER:
                case MyAiTargetEnum.ENTITY:
                case MyAiTargetEnum.ENVIRONMENT_ITEM:
                case MyAiTargetEnum.VOXEL:
                    if (this.m_targetEntity != null)
                    {
                        this.UnsetTargetEntity();
                    }
                    break;

                default:
                    break;
            }
            this.Clear();
        }

        protected virtual void UnsetTargetEntity()
        {
            if (this.IsTargetGridOrBlock(this.m_currentTarget) && (this.m_targetEntity is MyCubeGrid))
            {
                (this.m_targetEntity as MyCubeGrid).OnBlockRemoved -= new Action<MySlimBlock>(this.BlockRemoved);
            }
            this.m_compoundId = null;
            this.m_targetEntity = null;
            this.m_currentTarget = MyAiTargetEnum.NO_TARGET;
        }

        public virtual void Update()
        {
            using (MyUtils.ReuseCollection<VRage.Game.Entity.MyEntity>(ref m_tmpEntities))
            {
                foreach (KeyValuePair<VRage.Game.Entity.MyEntity, int> pair in this.m_unreachableEntities)
                {
                    if ((pair.Value - MySandboxGame.TotalGamePlayTimeInMilliseconds) < 0)
                    {
                        m_tmpEntities.Add(pair.Key);
                    }
                }
                foreach (VRage.Game.Entity.MyEntity entity in m_tmpEntities)
                {
                    this.RemoveUnreachableEntity(entity);
                }
            }
            using (MyUtils.ReuseCollection<Tuple<VRage.Game.Entity.MyEntity, int>>(ref m_tmpTrees))
            {
                foreach (KeyValuePair<Tuple<VRage.Game.Entity.MyEntity, int>, int> pair2 in this.m_unreachableTrees)
                {
                    if ((pair2.Value - MySandboxGame.TotalGamePlayTimeInMilliseconds) < 0)
                    {
                        m_tmpTrees.Add(pair2.Key);
                    }
                }
                foreach (Tuple<VRage.Game.Entity.MyEntity, int> tuple in m_tmpTrees)
                {
                    this.RemoveUnreachableTree(tuple);
                }
            }
        }

        public MyAiTargetEnum TargetType =>
            this.m_currentTarget;

        public MyCubeGrid TargetGrid =>
            (this.m_targetEntity as MyCubeGrid);

        public VRage.Game.Entity.MyEntity TargetEntity =>
            this.m_targetEntity;

        public Vector3D TargetPosition
        {
            get
            {
                switch (this.m_currentTarget)
                {
                    case MyAiTargetEnum.NO_TARGET:
                        return Vector3D.Zero;

                    case MyAiTargetEnum.GRID:
                    case MyAiTargetEnum.CHARACTER:
                    case MyAiTargetEnum.ENTITY:
                        return this.m_targetEntity.PositionComp.GetPosition();

                    case MyAiTargetEnum.CUBE:
                    case MyAiTargetEnum.COMPOUND_BLOCK:
                    {
                        MyCubeGrid targetEntity = this.m_targetEntity as MyCubeGrid;
                        if (targetEntity == null)
                        {
                            return Vector3D.Zero;
                        }
                        MySlimBlock cubeBlock = targetEntity.GetCubeBlock(this.m_targetCube);
                        return ((cubeBlock != null) ? targetEntity.GridIntegerToWorld(cubeBlock.Position) : Vector3D.Zero);
                    }
                    case MyAiTargetEnum.POSITION:
                        return this.m_targetPosition;

                    case MyAiTargetEnum.ENVIRONMENT_ITEM:
                        return this.m_targetEntity.PositionComp.GetPosition();

                    case MyAiTargetEnum.VOXEL:
                        return this.m_targetPosition;
                }
                return Vector3D.Zero;
            }
        }

        public Vector3D TargetCubeWorldPosition
        {
            get
            {
                MySlimBlock cubeBlock = this.GetCubeBlock();
                if ((cubeBlock == null) || (cubeBlock.FatBlock == null))
                {
                    return this.TargetGrid.GridIntegerToWorld(this.m_targetCube);
                }
                return cubeBlock.FatBlock.PositionComp.WorldAABB.Center;
            }
        }

        public bool HasGotoFailed { get; set; }
    }
}


namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyAdditionalModelGeneratorBase : IMyBlockAdditionalModelGenerator
    {
        protected static Vector3I[] Forwards = new Vector3I[] { Vector3I.Forward, Vector3I.Right, Vector3I.Backward, Vector3I.Left };
        protected static Vector3I[] Rights = new Vector3I[] { Vector3I.Right, Vector3I.Backward, Vector3I.Left, Vector3I.Forward };
        protected static readonly MyStringId BUILD_TYPE_WALL = MyStringId.GetOrCompute("wall");
        private static readonly List<Tuple<MyCubeGrid.MyBlockLocation, MySlimBlock>> m_tmpLocationsAndRefBlocks = new List<Tuple<MyCubeGrid.MyBlockLocation, MySlimBlock>>();
        private static readonly List<Vector3I> m_tmpLocations = new List<Vector3I>();
        private static readonly List<Tuple<Vector3I, ushort>> m_tmpLocationsAndIds = new List<Tuple<Vector3I, ushort>>();
        protected static readonly List<Vector3I> m_tmpPositionsAdd = new List<Vector3I>(0x20);
        protected static readonly List<Vector3I> m_tmpPositionsRemove = new List<Vector3I>(0x20);
        protected static readonly List<MyGeneratedBlockLocation> m_tmpLocationsRemove = new List<MyGeneratedBlockLocation>(0x20);
        protected MyCubeGrid m_grid;
        private bool m_enabled;
        private readonly HashSet<MyGeneratedBlockLocation> m_addLocations = new HashSet<MyGeneratedBlockLocation>();
        private readonly HashSet<MyGeneratedBlockLocation> m_removeLocations = new HashSet<MyGeneratedBlockLocation>();
        private readonly List<MyGeneratedBlockLocation> m_removeLocationsForGridSplits = new List<MyGeneratedBlockLocation>();
        private readonly HashSet<MyGridInfo> m_splitGridInfos = new HashSet<MyGridInfo>();

        protected MyAdditionalModelGeneratorBase()
        {
        }

        private void AddBlocks()
        {
            foreach (MyGeneratedBlockLocation location in this.m_addLocations)
            {
                Quaternion quaternion;
                location.Orientation.GetQuaternion(out quaternion);
                MyCubeGrid.MyBlockLocation location2 = new MyCubeGrid.MyBlockLocation(location.BlockDefinition.Id, location.Position, location.Position, location.Position, quaternion, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), MySession.Static.LocalPlayerId);
                m_tmpLocationsAndRefBlocks.Add(new Tuple<MyCubeGrid.MyBlockLocation, MySlimBlock>(location2, location.RefBlock));
            }
            foreach (Tuple<MyCubeGrid.MyBlockLocation, MySlimBlock> tuple in m_tmpLocationsAndRefBlocks)
            {
                MySlimBlock block = this.m_grid.BuildGeneratedBlock(tuple.Item1, (Vector3) Vector3I.Zero);
                if (block != null)
                {
                    MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock != null)
                    {
                        foreach (MySlimBlock block4 in fatBlock.GetBlocks())
                        {
                            Quaternion quaternion2;
                            MyCubeGrid.MyBlockLocation location3 = tuple.Item1;
                            location3.Orientation.GetQuaternion(out quaternion2);
                            MyBlockOrientation orientation2 = new MyBlockOrientation(ref quaternion2);
                            if ((block4.Orientation == orientation2) && (block4.BlockDefinition.Id == tuple.Item1.BlockDefinition))
                            {
                                block = block4;
                                break;
                            }
                        }
                    }
                    MySlimBlock generatingBlock = tuple.Item2;
                    if (((block != null) && block.BlockDefinition.IsGeneratedBlock) && (generatingBlock != null))
                    {
                        block.SetGeneratedBlockIntegrity(generatingBlock);
                    }
                }
            }
            m_tmpLocationsAndRefBlocks.Clear();
        }

        protected void AddGeneratedBlock(MySlimBlock refBlock, MyCubeBlockDefinition generatedBlockDefinition, Vector3I position, Vector3I forward, Vector3I up)
        {
            MyBlockOrientation orientation = new MyBlockOrientation(Base6Directions.GetDirection(ref forward), Base6Directions.GetDirection(ref up));
            if (generatedBlockDefinition.Size == Vector3I.One)
            {
                ushort? blockIdInCompound = null;
                this.m_addLocations.Add(new MyGeneratedBlockLocation(refBlock, generatedBlockDefinition, position, orientation, blockIdInCompound, null));
            }
        }

        public virtual void BlockAddedToMergedGrid(MySlimBlock block)
        {
            this.Grid_OnBlockAdded(block);
        }

        protected bool CanGenerateFromBlock(MySlimBlock cube)
        {
            if (cube == null)
            {
                return false;
            }
            MyCompoundCubeBlock fatBlock = cube.FatBlock as MyCompoundCubeBlock;
            if ((!this.m_enabled || (!cube.CubeGrid.InScene || (cube.BlockDefinition.IsGeneratedBlock || (((fatBlock != null) && (fatBlock.GetBlocksCount() == 0)) || (((fatBlock == null) && MySession.Static.SurvivalMode) && (cube.ComponentStack.BuildRatio < cube.BlockDefinition.BuildProgressToPlaceGeneratedBlocks)))))) || ((MyFakes.ENABLE_FRACTURE_COMPONENT && (cube.FatBlock != null)) && cube.FatBlock.Components.Has<MyFractureComponentBase>()))
            {
                return false;
            }
            return !(cube.FatBlock is MyFracturedBlock);
        }

        protected bool CanPlaceBlock(Vector3I position, MyCubeBlockDefinition definition, Vector3I forward, Vector3I up)
        {
            MyBlockOrientation orientation = new MyBlockOrientation(Base6Directions.GetDirection(forward), Base6Directions.GetDirection(up));
            int? ignoreMultiblockId = null;
            return this.m_grid.CanPlaceBlock(position, position, orientation, definition, ignoreMultiblockId, false);
        }

        public virtual void Close()
        {
            this.m_grid.OnBlockAdded -= new Action<MySlimBlock>(this.Grid_OnBlockAdded);
            this.m_grid.OnBlockRemoved -= new Action<MySlimBlock>(this.Grid_OnBlockRemoved);
            this.m_grid.OnGridSplit -= new Action<MyCubeGrid, MyCubeGrid>(this.Grid_OnGridSplit);
        }

        protected bool CubeExistsOnPosition(Vector3I pos)
        {
            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(pos);
            if (cubeBlock != null)
            {
                if (!(cubeBlock.FatBlock is MyCompoundCubeBlock))
                {
                    return !cubeBlock.BlockDefinition.IsGeneratedBlock;
                }
                else
                {
                    using (List<MySlimBlock>.Enumerator enumerator = (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            if (!enumerator.Current.BlockDefinition.IsGeneratedBlock)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        protected bool CubeExistsOnPositions(List<Vector3I> positions)
        {
            using (List<Vector3I>.Enumerator enumerator = positions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Vector3I current = enumerator.Current;
                    if (this.CubeExistsOnPosition(current))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void EnableGenerator(bool enable)
        {
            this.m_enabled = enable;
        }

        public virtual void GenerateBlocks(MySlimBlock generatingBlock)
        {
            this.Grid_OnBlockAdded(generatingBlock);
        }

        protected bool GeneratedBlockExists(Vector3I pos, MyBlockOrientation orientation, MyCubeBlockDefinition definition)
        {
            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(pos);
            if (cubeBlock != null)
            {
                MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                if (!MyFakes.ENABLE_COMPOUND_BLOCKS || (fatBlock == null))
                {
                    return ((cubeBlock.BlockDefinition.Id.SubtypeId == definition.Id.SubtypeId) && (cubeBlock.Orientation == orientation));
                }
                using (List<MySlimBlock>.Enumerator enumerator = fatBlock.GetBlocks().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MySlimBlock current = enumerator.Current;
                        if ((current.BlockDefinition.Id.SubtypeId == definition.Id.SubtypeId) && (current.Orientation == orientation))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public abstract MySlimBlock GetGeneratingBlock(MySlimBlock generatedBlock);
        private void Grid_OnBlockAdded(MySlimBlock cube)
        {
            if (this.CanGenerateFromBlock(cube))
            {
                if (cube.FatBlock is MyCompoundCubeBlock)
                {
                    foreach (MySlimBlock block in (cube.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        if (this.CanGenerateFromBlock(block))
                        {
                            this.OnAddedCube(block);
                        }
                    }
                }
                else
                {
                    this.OnAddedCube(cube);
                }
            }
        }

        private void Grid_OnBlockRemoved(MySlimBlock cube)
        {
            if ((this.m_enabled && (cube.CubeGrid.InScene && !cube.BlockDefinition.IsGeneratedBlock)) && (!(cube.FatBlock is MyCompoundCubeBlock) || (((MyCompoundCubeBlock) cube.FatBlock).GetBlocksCount() != 0)))
            {
                this.OnRemovedCube(cube);
            }
        }

        private void Grid_OnGridSplit(MyCubeGrid originalGrid, MyCubeGrid newGrid)
        {
            this.ProcessChangedGrid(newGrid);
        }

        public virtual bool Initialize(MyCubeGrid grid, MyCubeSize gridSizeEnum)
        {
            this.m_grid = grid;
            this.m_enabled = true;
            if (!this.IsValid(gridSizeEnum))
            {
                return false;
            }
            this.m_grid.OnBlockAdded += new Action<MySlimBlock>(this.Grid_OnBlockAdded);
            this.m_grid.OnBlockRemoved += new Action<MySlimBlock>(this.Grid_OnBlockRemoved);
            this.m_grid.OnGridSplit += new Action<MyCubeGrid, MyCubeGrid>(this.Grid_OnGridSplit);
            return true;
        }

        protected static bool IsSameMaterial(MySlimBlock block1, MySlimBlock block2) => 
            (block1.BlockDefinition.BuildMaterial == block2.BlockDefinition.BuildMaterial);

        protected abstract bool IsValid(MyCubeSize gridSizeEnum);
        public abstract void OnAddedCube(MySlimBlock cube);
        public abstract void OnRemovedCube(MySlimBlock cube);
        private void ProcessChangedGrid(MyCubeGrid newGrid)
        {
            Vector3I position = Vector3I.Round((this.m_grid.PositionComp.GetPosition() - newGrid.PositionComp.GetPosition()) / ((double) this.m_grid.GridSize));
            Vector3 vec = (Vector3) Vector3D.TransformNormal(this.m_grid.WorldMatrix.Forward, newGrid.PositionComp.WorldMatrixNormalizedInv);
            Base6Directions.Direction closestDirection = Base6Directions.GetClosestDirection(vec);
            Base6Directions.Direction up = Base6Directions.GetClosestDirection((Vector3) Vector3D.TransformNormal(this.m_grid.WorldMatrix.Up, newGrid.PositionComp.WorldMatrixNormalizedInv));
            if (up == closestDirection)
            {
                up = Base6Directions.GetPerpendicular(closestDirection);
            }
            MyGridInfo item = new MyGridInfo {
                Grid = newGrid,
                Transform = new MatrixI(ref position, closestDirection, up)
            };
            this.m_splitGridInfos.Add(item);
            if (this.m_removeLocationsForGridSplits.Count > 0)
            {
                List<int> list1 = new List<int>();
                for (int i = 0; i < this.m_removeLocationsForGridSplits.Count; i++)
                {
                    MyGeneratedBlockLocation location = this.m_removeLocationsForGridSplits[i];
                    this.RemoveBlock(location, item, location.GeneratedBlockType);
                }
            }
            List<MySlimBlock> newGridBlocks = new List<MySlimBlock>();
            this.m_addLocations.RemoveWhere(delegate (MyGeneratedBlockLocation loc) {
                if ((loc.RefBlock == null) || !ReferenceEquals(loc.RefBlock.CubeGrid, newGrid))
                {
                    return false;
                }
                newGridBlocks.Add(loc.RefBlock);
                return true;
            });
            using (List<MySlimBlock>.Enumerator enumerator = newGridBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MySlimBlock newGridBlock;
                    newGridBlock.CubeGrid.AdditionalModelGenerators.ForEach(g => g.UpdateAfterGridSpawn(newGridBlock));
                }
            }
        }

        private bool RemoveBlock(MyGeneratedBlockLocation location, MyGridInfo gridInfo, MyStringId generatedBlockType)
        {
            MySlimBlock cubeBlock = null;
            if (gridInfo == null)
            {
                cubeBlock = this.m_grid.GetCubeBlock(location.Position);
            }
            else
            {
                Vector3I pos = Vector3I.Transform(location.Position, gridInfo.Transform);
                cubeBlock = gridInfo.Grid.GetCubeBlock(pos);
            }
            if (cubeBlock != null)
            {
                if (!(cubeBlock.FatBlock is MyCompoundCubeBlock))
                {
                    if ((cubeBlock.BlockDefinition.IsGeneratedBlock && (cubeBlock.BlockDefinition.GeneratedBlockType == generatedBlockType)) && (cubeBlock.Orientation == location.Orientation))
                    {
                        ushort? blockIdInCompound = null;
                        this.m_removeLocations.Add(new MyGeneratedBlockLocation(null, cubeBlock.BlockDefinition, cubeBlock.Position, cubeBlock.Orientation, blockIdInCompound, gridInfo));
                        return true;
                    }
                }
                else
                {
                    MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                    ListReader<MySlimBlock> blocks = fatBlock.GetBlocks();
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        MySlimBlock block3 = blocks[i];
                        if ((block3.BlockDefinition.IsGeneratedBlock && (block3.BlockDefinition.GeneratedBlockType == generatedBlockType)) && (block3.Orientation == location.Orientation))
                        {
                            ushort? blockId = fatBlock.GetBlockId(block3);
                            this.m_removeLocations.Add(new MyGeneratedBlockLocation(null, block3.BlockDefinition, block3.Position, block3.Orientation, blockId, gridInfo));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void RemoveBlocks(bool removeLocalBlocks = true)
        {
            if (removeLocalBlocks)
            {
                foreach (MyGeneratedBlockLocation location in this.m_removeLocations)
                {
                    if (location.GridInfo == null)
                    {
                        if (location.BlockIdInCompound == null)
                        {
                            m_tmpLocations.Add(location.Position);
                            continue;
                        }
                        m_tmpLocationsAndIds.Add(new Tuple<Vector3I, ushort>(location.Position, location.BlockIdInCompound.Value));
                    }
                }
                if (m_tmpLocations.Count > 0)
                {
                    this.m_grid.RazeGeneratedBlocks(m_tmpLocations);
                }
                if (m_tmpLocationsAndIds.Count > 0)
                {
                    this.m_grid.RazeGeneratedBlocksInCompoundBlock(m_tmpLocationsAndIds);
                }
            }
            foreach (MyGridInfo info in this.m_splitGridInfos)
            {
                m_tmpLocations.Clear();
                m_tmpLocationsAndIds.Clear();
                foreach (MyGeneratedBlockLocation location2 in this.m_removeLocations)
                {
                    if (ReferenceEquals(location2.GridInfo, info))
                    {
                        if (location2.BlockIdInCompound == null)
                        {
                            m_tmpLocations.Add(location2.Position);
                            continue;
                        }
                        m_tmpLocationsAndIds.Add(new Tuple<Vector3I, ushort>(location2.Position, location2.BlockIdInCompound.Value));
                    }
                }
                if (m_tmpLocations.Count > 0)
                {
                    info.Grid.RazeGeneratedBlocks(m_tmpLocations);
                }
                if (m_tmpLocationsAndIds.Count > 0)
                {
                    info.Grid.RazeGeneratedBlocksInCompoundBlock(m_tmpLocationsAndIds);
                }
            }
            m_tmpLocations.Clear();
            m_tmpLocationsAndIds.Clear();
        }

        protected void RemoveGeneratedBlock(MyStringId generatedBlockType, List<MyGeneratedBlockLocation> locations)
        {
            if ((locations != null) && (locations.Count != 0))
            {
                using (List<MyGeneratedBlockLocation>.Enumerator enumerator = locations.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyGeneratedBlockLocation location;
                        this.RemoveBlock(location, null, generatedBlockType);
                        MyGeneratedBlockLocation item = location;
                        item.GeneratedBlockType = generatedBlockType;
                        this.m_removeLocationsForGridSplits.Add(item);
                        if (this.m_addLocations.Count > 0)
                        {
                            this.m_addLocations.RemoveWhere(loc => MyGeneratedBlockLocation.IsSameGeneratedBlockLocation(loc, location, generatedBlockType));
                        }
                    }
                }
            }
        }

        public virtual void UpdateAfterGridSpawn(MySlimBlock block)
        {
            this.Grid_OnBlockAdded(block);
        }

        public virtual void UpdateAfterSimulation()
        {
            this.UpdateInternal();
        }

        public virtual void UpdateBeforeSimulation()
        {
            this.UpdateInternal();
        }

        private void UpdateInternal()
        {
            if (this.m_addLocations.Count > 0)
            {
                this.m_addLocations.RemoveWhere(loc => (loc.RefBlock != null) && !ReferenceEquals(loc.RefBlock.CubeGrid, this.m_grid));
                this.m_addLocations.RemoveWhere(loc => !this.m_grid.CanAddCube(loc.Position, new MyBlockOrientation?(loc.Orientation), loc.BlockDefinition, true));
                this.m_addLocations.RemoveWhere(delegate (MyGeneratedBlockLocation loc) {
                    MyGeneratedBlockLocation? nullable = null;
                    foreach (MyGeneratedBlockLocation location in this.m_removeLocations)
                    {
                        if (MyGeneratedBlockLocation.IsSameGeneratedBlockLocation(loc, location))
                        {
                            nullable = new MyGeneratedBlockLocation?(location);
                            break;
                        }
                    }
                    if (nullable == null)
                    {
                        return false;
                    }
                    this.m_removeLocations.Remove(nullable.Value);
                    return true;
                });
            }
            if (this.m_removeLocations.Count > 0)
            {
                this.RemoveBlocks(true);
            }
            if (this.m_addLocations.Count > 0)
            {
                this.AddBlocks();
            }
            this.m_addLocations.Clear();
            this.m_removeLocations.Clear();
            this.m_removeLocationsForGridSplits.Clear();
            this.m_splitGridInfos.Clear();
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MyGeneratedBlockLocation
        {
            public MySlimBlock RefBlock;
            public MyCubeBlockDefinition BlockDefinition;
            public Vector3I Position;
            public MyBlockOrientation Orientation;
            public ushort? BlockIdInCompound;
            public MyAdditionalModelGeneratorBase.MyGridInfo GridInfo;
            public MyStringId GeneratedBlockType;
            public MyGeneratedBlockLocation(MySlimBlock refBlock, MyCubeBlockDefinition blockDefinition, Vector3I position, MyBlockOrientation orientation, ushort? blockIdInCompound = new ushort?(), MyAdditionalModelGeneratorBase.MyGridInfo gridInfo = null)
            {
                this.RefBlock = refBlock;
                this.BlockDefinition = blockDefinition;
                this.Position = position;
                this.Orientation = orientation;
                this.BlockIdInCompound = blockIdInCompound;
                this.GridInfo = gridInfo;
                this.GeneratedBlockType = MyStringId.NullOrEmpty;
            }

            public MyGeneratedBlockLocation(MySlimBlock refBlock, MyCubeBlockDefinition blockDefinition, Vector3I position, Vector3I forward, Vector3I up, ushort? blockIdInCompound = new ushort?(), MyAdditionalModelGeneratorBase.MyGridInfo gridInfo = null)
            {
                this.RefBlock = refBlock;
                this.BlockDefinition = blockDefinition;
                this.Position = position;
                this.Orientation = new MyBlockOrientation(Base6Directions.GetDirection(ref forward), Base6Directions.GetDirection(ref up));
                this.BlockIdInCompound = blockIdInCompound;
                this.GridInfo = gridInfo;
                this.GeneratedBlockType = MyStringId.NullOrEmpty;
            }

            public static bool IsSameGeneratedBlockLocation(MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation blockLocAdded, MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation blockLocRemoved) => 
                ((ReferenceEquals(blockLocAdded.BlockDefinition, blockLocRemoved.BlockDefinition) && (blockLocAdded.Position == blockLocRemoved.Position)) && (blockLocAdded.Orientation == blockLocRemoved.Orientation));

            public static bool IsSameGeneratedBlockLocation(MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation blockLocAdded, MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation blockLocRemoved, MyStringId generatedBlockType) => 
                (((blockLocAdded.BlockDefinition.GeneratedBlockType == generatedBlockType) && (blockLocAdded.Position == blockLocRemoved.Position)) && (blockLocAdded.Orientation == blockLocRemoved.Orientation));

            public override bool Equals(object ob)
            {
                if (!(ob is MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation))
                {
                    return false;
                }
                MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation blockLocRemoved = (MyAdditionalModelGeneratorBase.MyGeneratedBlockLocation) ob;
                return IsSameGeneratedBlockLocation(this, blockLocRemoved);
            }

            public override int GetHashCode() => 
                ((this.BlockDefinition.Id.GetHashCode() + (0x11 * this.Position.GetHashCode())) + (0x89 * this.Orientation.GetHashCode()));
        }

        protected class MyGridInfo
        {
            public MyCubeGrid Grid;
            public MatrixI Transform;
        }
    }
}


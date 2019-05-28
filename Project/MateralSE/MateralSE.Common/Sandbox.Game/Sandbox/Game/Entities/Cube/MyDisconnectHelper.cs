namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyDisconnectHelper
    {
        private HashSet<MySlimBlock> m_disconnectHelper = new HashSet<MySlimBlock>();
        private Queue<MySlimBlock> m_neighborSearchBaseStack = new Queue<MySlimBlock>();
        private List<MySlimBlock> m_sortedBlocks = new List<MySlimBlock>();
        private List<Group> m_groups = new List<Group>();
        private Group m_largestGroupWithPhysics;
        private List<MySlimBlock> m_tmpBlocks = new List<MySlimBlock>();

        private void AddNeighbours(MySlimBlock firstBlock, out bool anyWithPhysics, MySlimBlock testBlock)
        {
            anyWithPhysics = false;
            if (this.m_disconnectHelper.Remove(firstBlock))
            {
                anyWithPhysics |= firstBlock.BlockDefinition.HasPhysics;
                this.m_sortedBlocks.Add(firstBlock);
                this.m_neighborSearchBaseStack.Enqueue(firstBlock);
            }
            while (this.m_neighborSearchBaseStack.Count > 0)
            {
                foreach (MySlimBlock block in this.m_neighborSearchBaseStack.Dequeue().Neighbours)
                {
                    if (ReferenceEquals(block, testBlock))
                    {
                        continue;
                    }
                    if (this.m_disconnectHelper.Remove(block))
                    {
                        anyWithPhysics |= block.BlockDefinition.HasPhysics;
                        this.m_sortedBlocks.Add(block);
                        this.m_neighborSearchBaseStack.Enqueue(block);
                    }
                }
            }
        }

        public bool Disconnect(MyCubeGrid grid, MyCubeGrid.MyTestDisconnectsReason reason, MySlimBlock testBlock = null, bool testDisconnect = false)
        {
            // Invalid method body.
        }

        public static bool IsDestroyedInVoxels(MySlimBlock block)
        {
            Vector3D vectord2;
            if ((block == null) || block.CubeGrid.IsStatic)
            {
                return false;
            }
            MyCubeGrid cubeGrid = block.CubeGrid;
            Vector3D pos = Vector3D.Transform(((block.Max + block.Min) * 0.5f) * cubeGrid.GridSize, cubeGrid.WorldMatrix);
            return Sandbox.Game.Entities.MyEntities.IsInsideVoxel(pos, Vector3D.Transform(((block.Max + block.Min) * 0.5f) * cubeGrid.GridSize, cubeGrid.WorldMatrix) - (cubeGrid.Physics.LinearVelocity * 1.5f), out vectord2);
        }

        public bool TryDisconnect(MySlimBlock testBlock) => 
            this.Disconnect(testBlock.CubeGrid, MyCubeGrid.MyTestDisconnectsReason.NoReason, testBlock, true);

        [StructLayout(LayoutKind.Sequential)]
        public struct Group
        {
            public int FirstBlockIndex;
            public int BlockCount;
            public bool IsValid;
            public long EntityId;
        }
    }
}


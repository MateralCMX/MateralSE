namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Game;
    using VRageMath;

    public class MyBlockGroup : Sandbox.ModAPI.IMyBlockGroup, Sandbox.ModAPI.Ingame.IMyBlockGroup
    {
        public StringBuilder Name = new StringBuilder();
        internal readonly HashSet<MyTerminalBlock> Blocks = new HashSet<MyTerminalBlock>();

        internal MyBlockGroup()
        {
        }

        internal MyObjectBuilder_BlockGroup GetObjectBuilder()
        {
            MyObjectBuilder_BlockGroup group = new MyObjectBuilder_BlockGroup {
                Name = this.Name.ToString()
            };
            foreach (MyTerminalBlock block in this.Blocks)
            {
                group.Blocks.Add(block.Position);
            }
            return group;
        }

        internal void Init(MyCubeGrid grid, MyObjectBuilder_BlockGroup builder)
        {
            this.Name.Clear().Append(builder.Name);
            foreach (Vector3I vectori in builder.Blocks)
            {
                MySlimBlock cubeBlock = grid.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    MyTerminalBlock fatBlock = cubeBlock.FatBlock as MyTerminalBlock;
                    if (fatBlock != null)
                    {
                        this.Blocks.Add(fatBlock);
                    }
                }
            }
        }

        void Sandbox.ModAPI.IMyBlockGroup.GetBlocks(List<Sandbox.ModAPI.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.IMyTerminalBlock, bool> collect)
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.Blocks)
            {
                if (((collect == null) || collect(block)) && (blocks != null))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.IMyBlockGroup.GetBlocksOfType<T>(List<Sandbox.ModAPI.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.IMyTerminalBlock, bool> collect) where T: class
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.Blocks)
            {
                if (!(block is T))
                {
                    continue;
                }
                if (((collect == null) || collect(block)) && (blocks != null))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyBlockGroup.GetBlocks(List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> collect)
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.Blocks)
            {
                if (!block.IsAccessibleForProgrammableBlock)
                {
                    continue;
                }
                if (((collect == null) || collect(block)) && (blocks != null))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyBlockGroup.GetBlocksOfType<T>(List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> collect) where T: class
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.Blocks)
            {
                if (!(block is T))
                {
                    continue;
                }
                if (block.IsAccessibleForProgrammableBlock && (((collect == null) || collect(block)) && (blocks != null)))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyBlockGroup.GetBlocksOfType<T>(List<T> blocks, Func<T, bool> collect) where T: class
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.Blocks)
            {
                T arg = block as T;
                if ((arg != null) && (block.IsAccessibleForProgrammableBlock && (((collect == null) || collect(arg)) && (blocks != null))))
                {
                    blocks.Add(arg);
                }
            }
        }

        public override string ToString() => 
            $"{this.Name} - {this.Blocks.Count} blocks";

        string Sandbox.ModAPI.Ingame.IMyBlockGroup.Name =>
            this.Name.ToString();
    }
}


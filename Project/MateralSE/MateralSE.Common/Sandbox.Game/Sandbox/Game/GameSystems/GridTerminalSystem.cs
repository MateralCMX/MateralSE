namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;

    public class GridTerminalSystem : Sandbox.ModAPI.IMyGridTerminalSystem, Sandbox.ModAPI.Ingame.IMyGridTerminalSystem
    {

        Sandbox.ModAPI.IMyTerminalBlock Sandbox.ModAPI.IMyGridTerminalSystem.GetBlockWithName(string name)
        {
            using (HashSet<MyTerminalBlock>.Enumerator enumerator = this.m_blocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyTerminalBlock current = enumerator.Current;
                    if (current.CustomName.ToString() == name)
                    {
                        return current;
                    }
                }
            }
            return null;
        }
        private readonly int m_oreDetectorCounterValue = 50;
        private readonly HashSet<MyTerminalBlock> m_blocks = new HashSet<MyTerminalBlock>();
        private readonly List<MyTerminalBlock> m_blockList = new List<MyTerminalBlock>();
        private readonly Dictionary<long, MyTerminalBlock> m_blockTable = new Dictionary<long, MyTerminalBlock>();
        private readonly HashSet<MyTerminalBlock> m_blocksToShowOnHud = new HashSet<MyTerminalBlock>();
        private readonly HashSet<MyTerminalBlock> m_currentlyHackedBlocks = new HashSet<MyTerminalBlock>();
        private readonly List<MyBlockGroup> m_blockGroups = new List<MyBlockGroup>();
        private readonly HashSet<MyTerminalBlock> m_blocksForHud = new HashSet<MyTerminalBlock>();
        private int m_lastHudIndex;
        private int m_oreDetectorUpdateCounter;
        [CompilerGenerated]
        private Action<MyTerminalBlock> BlockAdded;
        [CompilerGenerated]
        private Action<MyTerminalBlock> BlockRemoved;
        [CompilerGenerated]
        private Action BlockManipulationFinished;
        [CompilerGenerated]
        private Action<MyBlockGroup> GroupAdded;
        [CompilerGenerated]
        private Action<MyBlockGroup> GroupRemoved;
        private bool m_needsHudUpdate = true;
        private int m_hudLastUpdated;

        public event Action<MyTerminalBlock> BlockAdded
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> blockAdded = this.BlockAdded;
                while (true)
                {
                    Action<MyTerminalBlock> a = blockAdded;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    blockAdded = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.BlockAdded, action3, a);
                    if (ReferenceEquals(blockAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> blockAdded = this.BlockAdded;
                while (true)
                {
                    Action<MyTerminalBlock> source = blockAdded;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    blockAdded = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.BlockAdded, action3, source);
                    if (ReferenceEquals(blockAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action BlockManipulationFinished
        {
            [CompilerGenerated] add
            {
                Action blockManipulationFinished = this.BlockManipulationFinished;
                while (true)
                {
                    Action a = blockManipulationFinished;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    blockManipulationFinished = Interlocked.CompareExchange<Action>(ref this.BlockManipulationFinished, action3, a);
                    if (ReferenceEquals(blockManipulationFinished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action blockManipulationFinished = this.BlockManipulationFinished;
                while (true)
                {
                    Action source = blockManipulationFinished;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    blockManipulationFinished = Interlocked.CompareExchange<Action>(ref this.BlockManipulationFinished, action3, source);
                    if (ReferenceEquals(blockManipulationFinished, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> BlockRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> blockRemoved = this.BlockRemoved;
                while (true)
                {
                    Action<MyTerminalBlock> a = blockRemoved;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    blockRemoved = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.BlockRemoved, action3, a);
                    if (ReferenceEquals(blockRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> blockRemoved = this.BlockRemoved;
                while (true)
                {
                    Action<MyTerminalBlock> source = blockRemoved;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    blockRemoved = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.BlockRemoved, action3, source);
                    if (ReferenceEquals(blockRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyBlockGroup> GroupAdded
        {
            [CompilerGenerated] add
            {
                Action<MyBlockGroup> groupAdded = this.GroupAdded;
                while (true)
                {
                    Action<MyBlockGroup> a = groupAdded;
                    Action<MyBlockGroup> action3 = (Action<MyBlockGroup>) Delegate.Combine(a, value);
                    groupAdded = Interlocked.CompareExchange<Action<MyBlockGroup>>(ref this.GroupAdded, action3, a);
                    if (ReferenceEquals(groupAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyBlockGroup> groupAdded = this.GroupAdded;
                while (true)
                {
                    Action<MyBlockGroup> source = groupAdded;
                    Action<MyBlockGroup> action3 = (Action<MyBlockGroup>) Delegate.Remove(source, value);
                    groupAdded = Interlocked.CompareExchange<Action<MyBlockGroup>>(ref this.GroupAdded, action3, source);
                    if (ReferenceEquals(groupAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyBlockGroup> GroupRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyBlockGroup> groupRemoved = this.GroupRemoved;
                while (true)
                {
                    Action<MyBlockGroup> a = groupRemoved;
                    Action<MyBlockGroup> action3 = (Action<MyBlockGroup>) Delegate.Combine(a, value);
                    groupRemoved = Interlocked.CompareExchange<Action<MyBlockGroup>>(ref this.GroupRemoved, action3, a);
                    if (ReferenceEquals(groupRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyBlockGroup> groupRemoved = this.GroupRemoved;
                while (true)
                {
                    Action<MyBlockGroup> source = groupRemoved;
                    Action<MyBlockGroup> action3 = (Action<MyBlockGroup>) Delegate.Remove(source, value);
                    groupRemoved = Interlocked.CompareExchange<Action<MyBlockGroup>>(ref this.GroupRemoved, action3, source);
                    if (ReferenceEquals(groupRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Add(MyTerminalBlock block)
        {
            if (((!block.MarkedForClose && !block.IsBeingRemoved) && !Sandbox.Game.Entities.MyEntities.IsClosingAll) && !this.m_blockTable.ContainsKey(block.EntityId))
            {
                this.m_blockTable.Add(block.EntityId, block);
                this.m_blocks.Add(block);
                this.m_blockList.Add(block);
                Action<MyTerminalBlock> blockAdded = this.BlockAdded;
                if (blockAdded != null)
                {
                    blockAdded(block);
                }
            }
        }

        public MyBlockGroup AddUpdateGroup(MyBlockGroup gridGroup, bool fireEvent, bool modify = false)
        {
            if (gridGroup.Blocks.Count == 0)
            {
                return null;
            }
            MyBlockGroup item = this.BlockGroups.Find(x => x.Name.CompareTo(gridGroup.Name) == 0);
            if (item == null)
            {
                item = new MyBlockGroup();
                item.Name.Clear().AppendStringBuilder(gridGroup.Name);
                this.BlockGroups.Add(item);
            }
            if (modify)
            {
                item.Blocks.Clear();
            }
            item.Blocks.UnionWith(gridGroup.Blocks);
            if (fireEvent && (this.GroupAdded != null))
            {
                this.GroupAdded(gridGroup);
            }
            return gridGroup;
        }

        internal void BlockManipulationFinishedFunction()
        {
            Action blockManipulationFinished = this.BlockManipulationFinished;
            if (blockManipulationFinished != null)
            {
                blockManipulationFinished();
            }
        }

        public void CopyBlocksTo(List<MyTerminalBlock> result)
        {
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                result.Add(block);
            }
        }

        public void IncrementHudLastUpdated()
        {
            this.m_hudLastUpdated++;
        }

        private bool MeetsHudConditions(MyTerminalBlock terminalBlock)
        {
            if (terminalBlock.HasLocalPlayerAccess() && ((terminalBlock.ShowOnHUD || ((terminalBlock.IsBeingHacked && (terminalBlock.IDModule != null)) && (terminalBlock.IDModule.Owner != 0))) || ((terminalBlock is MyCockpit) && ((terminalBlock as MyCockpit).Pilot != null))))
            {
                return true;
            }
            if ((terminalBlock.HasLocalPlayerAccess() && (terminalBlock.IDModule != null)) && (terminalBlock.IDModule.Owner != 0))
            {
                IMyComponentOwner<MyOreDetectorComponent> owner1 = terminalBlock as IMyComponentOwner<MyOreDetectorComponent>;
            }
            return false;
        }

        public void Remove(MyTerminalBlock block)
        {
            if (!block.MarkedForClose && !Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                this.m_blockTable.Remove(block.EntityId);
                this.m_blocks.Remove(block);
                this.m_blockList.Remove(block);
                this.m_blocksForHud.Remove(block);
                for (int i = 0; i < this.BlockGroups.Count; i++)
                {
                    MyBlockGroup gridGroup = this.BlockGroups[i];
                    gridGroup.Blocks.Remove(block);
                    if (gridGroup.Blocks.Count == 0)
                    {
                        this.RemoveGroup(gridGroup, true);
                        i--;
                    }
                }
                Action<MyTerminalBlock> blockRemoved = this.BlockRemoved;
                if (blockRemoved != null)
                {
                    blockRemoved(block);
                }
            }
        }

        public void RemoveGroup(MyBlockGroup gridGroup, bool fireEvent)
        {
            MyBlockGroup item = this.BlockGroups.Find(x => x.Name.CompareTo(gridGroup.Name) == 0);
            if (item != null)
            {
                List<MyTerminalBlock> list = new List<MyTerminalBlock>();
                foreach (MyTerminalBlock block in gridGroup.Blocks)
                {
                    if (item.Blocks.Contains(block))
                    {
                        list.Add(block);
                    }
                }
                foreach (MyTerminalBlock block2 in list)
                {
                    item.Blocks.Remove(block2);
                }
                if (item.Blocks.Count == 0)
                {
                    this.BlockGroups.Remove(item);
                }
            }
            if (fireEvent && (this.GroupRemoved != null))
            {
                this.GroupRemoved(gridGroup);
            }
        }

        void Sandbox.ModAPI.IMyGridTerminalSystem.GetBlockGroups(List<Sandbox.ModAPI.IMyBlockGroup> blockGroups)
        {
            blockGroups.Clear();
            foreach (MyBlockGroup group in this.BlockGroups)
            {
                blockGroups.Add(group);
            }
        }

        Sandbox.ModAPI.IMyBlockGroup Sandbox.ModAPI.IMyGridTerminalSystem.GetBlockGroupWithName(string name)
        {
            using (List<MyBlockGroup>.Enumerator enumerator = this.BlockGroups.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyBlockGroup current = enumerator.Current;
                    if (current.Name.ToString() == name)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        void Sandbox.ModAPI.IMyGridTerminalSystem.GetBlocks(List<Sandbox.ModAPI.IMyTerminalBlock> blocks)
        {
            blocks.Clear();
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                blocks.Add(block);
            }
        }

        void Sandbox.ModAPI.IMyGridTerminalSystem.GetBlocksOfType<T>(List<Sandbox.ModAPI.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.IMyTerminalBlock, bool> collect)
        {
            blocks.Clear();
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                if (!(block is T))
                {
                    continue;
                }
                if ((collect == null) || collect(block))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.IMyGridTerminalSystem.SearchBlocksOfName(string name, List<Sandbox.ModAPI.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.IMyTerminalBlock, bool> collect)
        {
            blocks.Clear();
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                if (!block.CustomName.ToString().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if ((collect == null) || collect(block))
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlockGroups(List<Sandbox.ModAPI.Ingame.IMyBlockGroup> blockGroups, Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, bool> collect)
        {
            if (blockGroups != null)
            {
                blockGroups.Clear();
            }
            for (int i = 0; i < this.BlockGroups.Count; i++)
            {
                MyBlockGroup arg = this.BlockGroups[i];
                if (((collect == null) || collect(arg)) && (blockGroups != null))
                {
                    blockGroups.Add(arg);
                }
            }
        }

        Sandbox.ModAPI.Ingame.IMyBlockGroup Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlockGroupWithName(string name)
        {
            for (int i = 0; i < this.BlockGroups.Count; i++)
            {
                MyBlockGroup group = this.BlockGroups[i];
                if (group.Name.CompareTo(name) == 0)
                {
                    return group;
                }
            }
            return null;
        }

        void Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlocks(List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocks)
        {
            blocks.Clear();
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                if (block.IsAccessibleForProgrammableBlock)
                {
                    blocks.Add(block);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlocksOfType<T>(List<T> blocks, Func<T, bool> collect) where T: class
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                T arg = block as T;
                if ((arg != null) && (block.IsAccessibleForProgrammableBlock && (((collect == null) || collect(arg)) && (blocks != null))))
                {
                    blocks.Add(arg);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlocksOfType<T>(List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> collect) where T: class
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.m_blocks)
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

        Sandbox.ModAPI.Ingame.IMyTerminalBlock Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlockWithId(long id)
        {
            MyTerminalBlock block;
            if (!this.m_blockTable.TryGetValue(id, out block) || !block.IsAccessibleForProgrammableBlock)
            {
                return null;
            }
            return block;
        }

        Sandbox.ModAPI.Ingame.IMyTerminalBlock Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.GetBlockWithName(string name)
        {
            using (HashSet<MyTerminalBlock>.Enumerator enumerator = this.m_blocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyTerminalBlock current = enumerator.Current;
                    if ((current.CustomName.CompareTo(name) == 0) && current.IsAccessibleForProgrammableBlock)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        void Sandbox.ModAPI.Ingame.IMyGridTerminalSystem.SearchBlocksOfName(string name, List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> blocks, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> collect)
        {
            if (blocks != null)
            {
                blocks.Clear();
            }
            foreach (MyTerminalBlock block in this.m_blocks)
            {
                if (!block.CustomName.ToString().Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (block.IsAccessibleForProgrammableBlock && (((collect == null) || collect(block)) && (blocks != null)))
                {
                    blocks.Add(block);
                }
            }
        }

        public void UpdateGridBlocksOwnership(long ownerID)
        {
            foreach (MyTerminalBlock local1 in this.m_blocks)
            {
                local1.IsAccessibleForProgrammableBlock = local1.HasPlayerAccess(ownerID);
            }
        }

        public void UpdateHud()
        {
            if (this.NeedsHudUpdate)
            {
                if (this.m_lastHudIndex >= this.m_blocks.Count)
                {
                    this.m_lastHudIndex = 0;
                    this.NeedsHudUpdate = false;
                }
                else
                {
                    MyTerminalBlock terminalBlock = this.m_blockList[this.m_lastHudIndex];
                    if (this.MeetsHudConditions(terminalBlock))
                    {
                        this.m_blocksForHud.Add(terminalBlock);
                    }
                    else
                    {
                        this.m_blocksForHud.Remove(terminalBlock);
                    }
                    this.m_lastHudIndex++;
                }
                this.m_hudLastUpdated = 0;
            }
        }

        public static ModAPI.Ingame.IMyBlockGroup GetBlockWithName(string name)
        {
            throw new NotImplementedException();
        }

        public bool NeedsHudUpdate
        {
            get => 
                this.m_needsHudUpdate;
            set
            {
                if (this.m_needsHudUpdate != value)
                {
                    this.m_blocksForHud.ForEach<MyTerminalBlock>(x => x.CubeGrid.MarkForUpdate());
                    this.m_needsHudUpdate = value;
                }
            }
        }

        public int HudLastUpdated =>
            this.m_hudLastUpdated;

        public HashSetReader<MyTerminalBlock> Blocks =>
            new HashSetReader<MyTerminalBlock>(this.m_blocks);

        public HashSetReader<MyTerminalBlock> HudBlocks =>
            new HashSetReader<MyTerminalBlock>(this.m_blocksForHud);

        public List<MyBlockGroup> BlockGroups =>
            this.m_blockGroups;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly GridTerminalSystem.<>c <>9 = new GridTerminalSystem.<>GridTerminalSystem();
            public static Action<MyTerminalBlock> <>9__42_0;

            internal void <set_NeedsHudUpdate>GridTerminalSystem(MyTerminalBlock x)
            {
                x.CubeGrid.MarkForUpdate();
            }
        }
    }
}


namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;

    internal class MyCubeGridOwnershipManager
    {
        public Dictionary<long, int> PlayerOwnedBlocks;
        public Dictionary<long, int> PlayerOwnedValidBlocks;
        public List<long> BigOwners;
        public List<long> SmallOwners;
        public int MaxBlocks;
        public long gridEntityId;
        public bool NeedRecalculateOwners;

        public void ChangeBlockOwnership(MyCubeBlock block, long oldOwner, long newOwner)
        {
            this.DecreaseValue(ref this.PlayerOwnedBlocks, oldOwner);
            this.IncreaseValue(ref this.PlayerOwnedBlocks, newOwner);
            if (this.IsValidBlock(block))
            {
                this.DecreaseValue(ref this.PlayerOwnedValidBlocks, oldOwner);
                this.IncreaseValue(ref this.PlayerOwnedValidBlocks, newOwner);
            }
            this.NeedRecalculateOwners = true;
            block.CubeGrid.MarkForUpdate();
        }

        public void DecreaseValue(ref Dictionary<long, int> dict, long key)
        {
            if ((key != 0) && dict.ContainsKey(key))
            {
                long num = key;
                dict[num] -= 1;
                if (dict[key] == 0)
                {
                    dict.Remove(key);
                }
            }
        }

        public void IncreaseValue(ref Dictionary<long, int> dict, long key)
        {
            if (key != 0)
            {
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, 0);
                }
                long num = key;
                dict[num] += 1;
            }
        }

        public void Init(MyCubeGrid grid)
        {
            this.PlayerOwnedBlocks = new Dictionary<long, int>();
            this.PlayerOwnedValidBlocks = new Dictionary<long, int>();
            this.BigOwners = new List<long>();
            this.SmallOwners = new List<long>();
            this.MaxBlocks = 0;
            this.gridEntityId = grid.EntityId;
            foreach (MyCubeBlock block in grid.GetFatBlocks())
            {
                long ownerId = block.OwnerId;
                if (ownerId != 0)
                {
                    if (!this.PlayerOwnedBlocks.ContainsKey(ownerId))
                    {
                        this.PlayerOwnedBlocks.Add(ownerId, 0);
                    }
                    long num2 = ownerId;
                    this.PlayerOwnedBlocks[num2] += 1;
                    if (this.IsValidBlock(block))
                    {
                        if (!this.PlayerOwnedValidBlocks.ContainsKey(ownerId))
                        {
                            this.PlayerOwnedValidBlocks.Add(ownerId, 0);
                        }
                        num2 = block.OwnerId;
                        int num3 = this.PlayerOwnedValidBlocks[num2] + 1;
                        this.PlayerOwnedValidBlocks[num2] = num3;
                        if (num3 > this.MaxBlocks)
                        {
                            this.MaxBlocks = this.PlayerOwnedValidBlocks[ownerId];
                        }
                    }
                }
            }
            this.NeedRecalculateOwners = true;
        }

        private bool IsValidBlock(MyCubeBlock block) => 
            block.IsFunctional;

        internal void RecalculateOwners()
        {
            this.MaxBlocks = 0;
            foreach (long num in this.PlayerOwnedValidBlocks.Keys)
            {
                if (this.PlayerOwnedValidBlocks[num] > this.MaxBlocks)
                {
                    this.MaxBlocks = this.PlayerOwnedValidBlocks[num];
                }
            }
            this.BigOwners.Clear();
            foreach (long num2 in this.PlayerOwnedValidBlocks.Keys)
            {
                if (this.PlayerOwnedValidBlocks[num2] == this.MaxBlocks)
                {
                    this.BigOwners.Add(num2);
                }
            }
            if (this.SmallOwners.Contains(MySession.Static.LocalPlayerId))
            {
                MySession.Static.LocalHumanPlayer.RemoveGrid(this.gridEntityId);
            }
            this.SmallOwners.Clear();
            foreach (long num3 in this.PlayerOwnedBlocks.Keys)
            {
                this.SmallOwners.Add(num3);
                if (num3 == MySession.Static.LocalPlayerId)
                {
                    MySession.Static.LocalHumanPlayer.AddGrid(this.gridEntityId);
                }
            }
        }

        public void UpdateOnFunctionalChange(long ownerId, bool newFunctionalValue)
        {
            if (!newFunctionalValue)
            {
                this.DecreaseValue(ref this.PlayerOwnedValidBlocks, ownerId);
            }
            else
            {
                this.IncreaseValue(ref this.PlayerOwnedValidBlocks, ownerId);
            }
            this.NeedRecalculateOwners = true;
        }
    }
}


namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Runtime.InteropServices;

    public class MyBlockRemovalData
    {
        public MySlimBlock Block;
        public ushort? BlockIdInCompound;
        public bool CheckExisting;

        public MyBlockRemovalData(MySlimBlock block, ushort? blockIdInCompound = new ushort?(), bool checkExisting = false)
        {
            this.Block = block;
            this.BlockIdInCompound = blockIdInCompound;
            this.CheckExisting = checkExisting;
        }
    }
}


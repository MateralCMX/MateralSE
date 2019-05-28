namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    public abstract class MyEnvironmentDataView
    {
        public Vector2I Start;
        public Vector2I End;
        public int Lod;
        public List<Sandbox.Game.WorldEnvironment.ItemInfo> Items;
        public MyEnvironmentSector Listener;
        public List<int> SectorOffsets;
        public List<int> IntraSectorOffsets;
        public List<MyLogicalEnvironmentSectorBase> LogicalSectors;

        protected MyEnvironmentDataView()
        {
        }

        public abstract void Close();
        public void GetLogicalSector(int item, out int logicalItem, out MyLogicalEnvironmentSectorBase sector)
        {
            int num = this.SectorOffsets.BinaryIntervalSearch<int>(item, ((IComparer<int>) null)) - 1;
            logicalItem = (item - this.SectorOffsets[num]) + this.IntraSectorOffsets[num];
            sector = this.LogicalSectors[num];
        }
    }
}


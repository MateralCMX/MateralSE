namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyProceduralDataView : MyEnvironmentDataView
    {
        private readonly MyProceduralEnvironmentProvider m_provider;

        public MyProceduralDataView(MyProceduralEnvironmentProvider provider, int lod, ref Vector2I start, ref Vector2I end)
        {
            this.m_provider = provider;
            base.Start = start;
            base.End = end;
            base.Lod = lod;
            int capacity = ((end - start) + 1).Size();
            base.SectorOffsets = new List<int>(capacity);
            base.LogicalSectors = new List<MyLogicalEnvironmentSectorBase>(capacity);
            base.IntraSectorOffsets = new List<int>(capacity);
            base.Items = new List<Sandbox.Game.WorldEnvironment.ItemInfo>();
        }

        public void AddSector(MyProceduralLogicalSector sector)
        {
            base.SectorOffsets.Add(base.Items.Count);
            base.LogicalSectors.Add(sector);
            base.IntraSectorOffsets.Add(0);
        }

        public override void Close()
        {
            this.m_provider.CloseView(this);
        }

        public int GetSectorIndex(int x, int y) => 
            ((x - base.Start.X) + ((y - base.Start.Y) * ((base.End.X - base.Start.X) + 1)));
    }
}


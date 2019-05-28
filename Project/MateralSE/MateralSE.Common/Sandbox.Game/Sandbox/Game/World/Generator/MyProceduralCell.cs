namespace Sandbox.Game.World.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyProceduralCell
    {
        public int proxyId = -1;
        private MyDynamicAABBTreeD m_tree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);

        public MyProceduralCell(Vector3I cellId, double cellSize)
        {
            this.CellId = cellId;
            this.BoundingVolume = new BoundingBoxD((Vector3D) (this.CellId * cellSize), (this.CellId + 1) * cellSize);
        }

        public void AddObject(MyObjectSeed objectSeed)
        {
            BoundingBoxD boundingVolume = objectSeed.BoundingVolume;
            objectSeed.m_proxyId = this.m_tree.AddProxy(ref boundingVolume, objectSeed, 0, true);
        }

        public void GetAll(List<MyObjectSeed> list, bool clear = true)
        {
            this.m_tree.GetAll<MyObjectSeed>(list, clear, null);
        }

        public override int GetHashCode() => 
            this.CellId.GetHashCode();

        public void OverlapAllBoundingBox(ref BoundingBoxD box, List<MyObjectSeed> list, bool clear = false)
        {
            this.m_tree.OverlapAllBoundingBox<MyObjectSeed>(ref box, list, 0, clear);
        }

        public void OverlapAllBoundingSphere(ref BoundingSphereD sphere, List<MyObjectSeed> list, bool clear = false)
        {
            this.m_tree.OverlapAllBoundingSphere<MyObjectSeed>(ref sphere, list, clear);
        }

        public override string ToString() => 
            this.CellId.ToString();

        public Vector3I CellId { get; private set; }

        public BoundingBoxD BoundingVolume { get; private set; }
    }
}


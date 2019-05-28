namespace Sandbox.Game.World.Generator
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    public class MyObjectSeed
    {
        public MyObjectSeedParams Params;
        public int m_proxyId;

        public MyObjectSeed()
        {
            this.Params = new MyObjectSeedParams();
            this.m_proxyId = -1;
        }

        public MyObjectSeed(MyProceduralCell cell, Vector3D position, double size)
        {
            this.Params = new MyObjectSeedParams();
            this.m_proxyId = -1;
            this.Cell = cell;
            this.Size = (float) size;
            this.BoundingVolume = new BoundingBoxD(position - this.Size, position + this.Size);
        }

        public BoundingBoxD BoundingVolume { get; private set; }

        public float Size { get; private set; }

        public MyProceduralCell Cell { get; private set; }

        public Vector3I CellId =>
            this.Cell.CellId;

        public object UserData { get; set; }
    }
}


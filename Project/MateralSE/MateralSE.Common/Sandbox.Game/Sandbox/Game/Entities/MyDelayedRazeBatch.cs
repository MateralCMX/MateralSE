namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyDelayedRazeBatch
    {
        public Vector3I Pos;
        public Vector3UByte Size;
        public HashSet<MyCockpit> Occupied;

        public MyDelayedRazeBatch(Vector3I pos, Vector3UByte size)
        {
            this.Pos = pos;
            this.Size = size;
        }
    }
}


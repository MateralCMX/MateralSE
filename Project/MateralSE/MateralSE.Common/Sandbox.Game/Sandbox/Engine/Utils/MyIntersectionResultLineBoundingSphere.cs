namespace Sandbox.Engine.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyIntersectionResultLineBoundingSphere
    {
        public readonly float Distance;
        public readonly MyEntity PhysObject;
        public MyIntersectionResultLineBoundingSphere(float distance, MyEntity physObject)
        {
            this.Distance = distance;
            this.PhysObject = physObject;
        }

        public static MyIntersectionResultLineBoundingSphere? GetCloserIntersection(ref MyIntersectionResultLineBoundingSphere? a, ref MyIntersectionResultLineBoundingSphere? b)
        {
            if (((a == 0) && (b != 0)) || (((a != 0) && (b != 0)) && (b.Value.Distance < a.Value.Distance)))
            {
                return b;
            }
            return a;
        }
    }
}


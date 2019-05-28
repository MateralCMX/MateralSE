namespace VRage.Render11.Common
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCullAABB
    {
        private const int COUNT = 0x10;
        public const int MAX_OFFSET = 0x10;
        public readonly Vector3[] Data;
        public float LengthSquared
        {
            get => 
                this.Data[0x20].X;
            set => 
                (this.Data[0x20].X = value);
        }
        public MyCullAABB(BoundingBox aabb)
        {
            this.Data = new Vector3[0x21];
            this.Reset(ref aabb);
        }

        public void Reset(ref BoundingBox aabb)
        {
            Vector3 min = aabb.Min;
            Vector3 max = aabb.Max;
            for (int i = 0; i < 0x10; i++)
            {
                Axes axes = (Axes) i;
                this.Data[i] = new Vector3(((axes & Axes.X) > 0) ? min.X : max.X, ((axes & Axes.Y) > 0) ? min.Y : max.Y, ((axes & Axes.Z) > 0) ? min.Z : max.Z);
                this.Data[i + 0x10] = new Vector3(((axes & Axes.X) == 0) ? min.X : max.X, ((axes & Axes.Y) == 0) ? min.Y : max.Y, ((axes & Axes.Z) == 0) ? min.Z : max.Z);
            }
            this.LengthSquared = aabb.Size.LengthSquared();
        }
        [Flags]
        public enum Axes
        {
            X = 4,
            Y = 2,
            Z = 1
        }
    }
}


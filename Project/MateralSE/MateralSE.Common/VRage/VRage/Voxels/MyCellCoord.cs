namespace VRage.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCellCoord : IComparable<MyCellCoord>, IEquatable<MyCellCoord>
    {
        private const int BITS_LOD = 4;
        private const int BITS_X_32 = 10;
        private const int BITS_Y_32 = 8;
        private const int BITS_Z_32 = 10;
        private const int BITS_X_64 = 20;
        private const int BITS_Y_64 = 20;
        private const int BITS_Z_64 = 20;
        private const int SHIFT_Z_32 = 0;
        private const int SHIFT_Y_32 = 10;
        private const int SHIFT_X_32 = 0x12;
        private const int SHIFT_LOD_32 = 0x1c;
        private const int SHIFT_Z_64 = 0;
        private const int SHIFT_Y_64 = 20;
        private const int SHIFT_X_64 = 40;
        private const int SHIFT_LOD_64 = 60;
        private const int MASK_LOD = 15;
        private const int MASK_X_32 = 0x3ff;
        private const int MASK_Y_32 = 0xff;
        private const int MASK_Z_32 = 0x3ff;
        private const int MASK_X_64 = 0xfffff;
        private const int MASK_Y_64 = 0xfffff;
        private const int MASK_Z_64 = 0xfffff;
        public const int MAX_LOD_COUNT = 0x10;
        public int Lod;
        public Vector3I CoordInLod;
        public static readonly EqualityComparer Comparer;
        public override bool Equals(object obj) => 
            ((obj != null) ? ((obj is MyCellCoord) && this.Equals((MyCellCoord) obj)) : false);

        public override int GetHashCode() => 
            ((this.Lod * 0x18d) ^ this.CoordInLod.GetHashCode());

        public MyCellCoord(ulong packedId)
        {
            this.CoordInLod.Z = (int) (packedId & ((ulong) 0x3ffL));
            packedId = packedId >> 10;
            this.CoordInLod.Y = (int) (packedId & ((ulong) 0xffL));
            packedId = packedId >> 8;
            this.CoordInLod.X = (int) (packedId & ((ulong) 0x3ffL));
            packedId = packedId >> 10;
            this.Lod = (int) packedId;
        }

        public MyCellCoord(int lod, Vector3I coordInLod) : this(lod, ref coordInLod)
        {
        }

        public MyCellCoord(int lod, ref Vector3I coordInLod)
        {
            this.Lod = lod;
            this.CoordInLod = coordInLod;
        }

        public void SetUnpack(uint id)
        {
            this.CoordInLod.Z = ((int) id) & 0x3ff;
            id = id >> 10;
            this.CoordInLod.Y = ((int) id) & 0xff;
            id = id >> 8;
            this.CoordInLod.X = ((int) id) & 0x3ff;
            id = id >> 10;
            this.Lod = (int) id;
        }

        public void SetUnpack(ulong id)
        {
            this.CoordInLod.Z = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            this.CoordInLod.Y = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            this.CoordInLod.X = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            this.Lod = (int) id;
        }

        public static int UnpackLod(ulong id) => 
            ((int) (id >> 60));

        public static Vector3I UnpackCoord(ulong id)
        {
            Vector3I vectori;
            vectori.Z = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            vectori.Y = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            vectori.X = (int) (id & ((ulong) 0xfffffL));
            id = id >> 20;
            return vectori;
        }

        public static ulong PackId64Static(int lod, Vector3I coordInLod) => 
            ((ulong) ((((lod << 60) | (coordInLod.X << 40)) | (coordInLod.Y << 20)) | coordInLod.Z));

        public uint PackId32() => 
            ((uint) ((((this.Lod << 0x1c) | (this.CoordInLod.X << 0x12)) | (this.CoordInLod.Y << 10)) | this.CoordInLod.Z));

        public ulong PackId64() => 
            ((ulong) ((((this.Lod << 60) | (this.CoordInLod.X << 40)) | (this.CoordInLod.Y << 20)) | this.CoordInLod.Z));

        public bool IsCoord64Valid() => 
            (((this.CoordInLod.X & 0xfffff) == this.CoordInLod.X) ? (((this.CoordInLod.Y & 0xfffff) == this.CoordInLod.Y) ? ((this.CoordInLod.Z & 0xfffff) == this.CoordInLod.Z) : false) : false);

        public static ulong GetClipmapCellHash(uint clipmap, MyCellCoord cellId) => 
            GetClipmapCellHash(clipmap, cellId.PackId64());

        public static ulong GetClipmapCellHash(uint clipmap, ulong cellId) => 
            (((cellId * ((ulong) 0x3e5L)) * ((ulong) 0x18dL)) ^ (clipmap * 0x3e5));

        public static bool operator ==(MyCellCoord x, MyCellCoord y) => 
            ((x.CoordInLod.X == y.CoordInLod.X) && ((x.CoordInLod.Y == y.CoordInLod.Y) && ((x.CoordInLod.Z == y.CoordInLod.Z) && (x.Lod == y.Lod))));

        public static bool operator !=(MyCellCoord x, MyCellCoord y) => 
            ((x.CoordInLod.X != y.CoordInLod.X) || ((x.CoordInLod.Y != y.CoordInLod.Y) || ((x.CoordInLod.Z != y.CoordInLod.Z) || (x.Lod != y.Lod))));

        public bool Equals(MyCellCoord other) => 
            (this == other);

        public override string ToString() => 
            $"{this.Lod}, {this.CoordInLod}";

        static MyCellCoord()
        {
            Comparer = new EqualityComparer();
        }

        public int CompareTo(MyCellCoord other)
        {
            int num = this.CoordInLod.X - other.CoordInLod.X;
            int num2 = this.CoordInLod.Y - other.CoordInLod.Y;
            int num3 = this.CoordInLod.Z - other.CoordInLod.Z;
            int num4 = this.Lod - other.Lod;
            return ((num != 0) ? num : ((num2 != 0) ? num2 : ((num3 != 0) ? num3 : num4)));
        }
        public class EqualityComparer : IEqualityComparer<MyCellCoord>, IComparer<MyCellCoord>
        {
            public int Compare(MyCellCoord x, MyCellCoord y) => 
                x.CompareTo(y);

            public bool Equals(MyCellCoord x, MyCellCoord y) => 
                ((x.CoordInLod.X == y.CoordInLod.X) && ((x.CoordInLod.Y == y.CoordInLod.Y) && ((x.CoordInLod.Z == y.CoordInLod.Z) && (x.Lod == y.Lod))));

            public int GetHashCode(MyCellCoord obj) => 
                ((((((obj.CoordInLod.X * 0x18d) ^ obj.CoordInLod.Y) * 0x18d) ^ obj.CoordInLod.Z) * 0x18d) ^ obj.Lod);
        }
    }
}


namespace VRageRender.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelMaterialTriple
    {
        public byte I0;
        public byte I1;
        public byte I2;
        public static readonly IEqualityComparer<MyVoxelMaterialTriple> Comparer;
        public MyVoxelMaterialTriple(int i0, int i1, int i2)
        {
            this.I0 = (i0 == -1) ? ((byte) 0xff) : ((byte) i0);
            this.I1 = (i1 == -1) ? ((byte) 0xff) : ((byte) i1);
            this.I2 = (i2 == -1) ? ((byte) 0xff) : ((byte) i2);
        }

        public MyVoxelMaterialTriple(byte i0, byte i1, byte i2)
        {
            this.I0 = i0;
            this.I1 = i1;
            this.I2 = i2;
        }

        public bool MultiMaterial =>
            (this.I1 != 0xff);
        public bool SingleMaterial =>
            (this.I1 == 0xff);
        internal bool IsMultimaterial() => 
            ((this.I1 != 0xff) || (this.I2 != 0xff));

        static MyVoxelMaterialTriple()
        {
            Comparer = new comp();
        }
        private class comp : IEqualityComparer<MyVoxelMaterialTriple>
        {
            public bool Equals(MyVoxelMaterialTriple x, MyVoxelMaterialTriple y) => 
                ((x.I0 == y.I0) && ((x.I1 == y.I1) && (x.I2 == y.I2)));

            public int GetHashCode(MyVoxelMaterialTriple obj) => 
                (((obj.I0 << 0x10) | (obj.I1 << 8)) | obj.I2).GetHashCode();
        }
    }
}


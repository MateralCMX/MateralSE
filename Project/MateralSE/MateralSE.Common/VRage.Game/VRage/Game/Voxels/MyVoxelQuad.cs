namespace VRage.Game.Voxels
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MyVoxelQuad
    {
        public ushort V0;
        public ushort V1;
        public ushort V2;
        public ushort V3;
        public MyVoxelQuad(ushort v0, ushort v1, ushort v2, ushort v3)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.V3 = v3;
        }

        public ushort this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.V0;

                    case 1:
                        return this.V1;

                    case 2:
                        return this.V2;

                    case 3:
                        return this.V3;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.V0 = value;
                        return;

                    case 1:
                        this.V1 = value;
                        return;

                    case 2:
                        this.V2 = value;
                        return;

                    case 3:
                        this.V3 = value;
                        return;
                }
                throw new IndexOutOfRangeException();
            }
        }
        public int IndexOf(int vx) => 
            ((vx != this.V0) ? ((vx != this.V1) ? ((vx != this.V2) ? ((vx != this.V3) ? -1 : 3) : 2) : 1) : 0);

        public override string ToString()
        {
            object[] objArray1 = new object[9];
            objArray1[0] = "{";
            objArray1[1] = this.V0;
            objArray1[2] = ", ";
            objArray1[3] = this.V1;
            objArray1[4] = ", ";
            objArray1[5] = this.V2;
            objArray1[6] = ", ";
            objArray1[7] = this.V3;
            objArray1[8] = "}";
            return string.Concat(objArray1);
        }
    }
}


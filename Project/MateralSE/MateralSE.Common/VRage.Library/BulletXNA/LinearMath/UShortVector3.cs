namespace BulletXNA.LinearMath
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct UShortVector3
    {
        public ushort X;
        public ushort Y;
        public ushort Z;
        public ushort this[int i]
        {
            get => 
                ((i != 0) ? ((i != 1) ? ((i != 2) ? 0 : this.Z) : this.Y) : this.X);
            set
            {
                if (i == 0)
                {
                    this.X = value;
                }
                else if (i == 1)
                {
                    this.Y = value;
                }
                else if (i == 2)
                {
                    this.Z = value;
                }
            }
        }
    }
}


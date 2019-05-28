namespace VRage.Library.Collections
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SmallBitField
    {
        public const int BitCount = 0x40;
        public const ulong BitsEmpty = 0UL;
        public const ulong BitsFull = ulong.MaxValue;
        public static readonly SmallBitField Empty;
        public static readonly SmallBitField Full;
        public ulong Bits;
        public SmallBitField(bool value)
        {
            this.Bits = value ? ulong.MaxValue : ((ulong) 0L);
        }

        public void Reset(bool value)
        {
            this.Bits = value ? ulong.MaxValue : ((ulong) 0L);
        }

        public bool this[int index]
        {
            get => 
                (((this.Bits >> (index & 0x3f)) & ((ulong) 1L)) != 0L);
            set
            {
                if (value)
                {
                    this.Bits |= (ulong) (1 << (index & 0x1f));
                }
                else
                {
                    this.Bits &= (ulong) ~(1 << (index & 0x1f));
                }
            }
        }
        static SmallBitField()
        {
            Empty = new SmallBitField(false);
            Full = new SmallBitField(true);
        }
    }
}


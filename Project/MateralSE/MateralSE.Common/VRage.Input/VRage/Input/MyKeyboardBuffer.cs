namespace VRage.Input
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyKeyboardBuffer
    {
        [FixedBuffer(typeof(byte), 0x20)]
        private <m_data>e__FixedBuffer m_data;
        public unsafe void SetBit(byte bit, bool value)
        {
            if (bit != 0)
            {
                byte num2 = (byte) (1 << ((bit % 8) & 0x1f));
                byte* numPtr = &this.m_data.FixedElementField;
                if (value)
                {
                    byte* numPtr1 = numPtr + (bit / 8);
                    numPtr1[0] = (byte) (numPtr1[0] | num2);
                }
                else
                {
                    byte* numPtr2 = numPtr + (bit / 8);
                    numPtr2[0] = (byte) (numPtr2[0] & ~num2);
                }
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }

        public unsafe bool AnyBitSet()
        {
            long* numPtr = (long*) &this.m_data.FixedElementField;
            return ((((numPtr[0] + numPtr[1]) + numPtr[2]) + numPtr[3]) != 0L);
        }

        public unsafe bool GetBit(byte bit)
        {
            byte num2 = (byte) (1 << ((bit % 8) & 0x1f));
            return ((&this.m_data.FixedElementField[bit / 8] & num2) != 0);
        }
        [StructLayout(LayoutKind.Sequential, Size=0x20), CompilerGenerated, UnsafeValueType]
        public struct <m_data>e__FixedBuffer
        {
            public byte FixedElementField;
        }
    }
}


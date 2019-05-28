namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyOctreeNode
    {
        public const int CHILD_COUNT = 8;
        public const int SERIALIZED_SIZE = 9;
        [ThreadStatic]
        private static Dictionary<byte, int> Histogram;
        public static readonly FilterFunction ContentFilter;
        public static readonly FilterFunction MaterialFilter;
        public byte ChildMask;
        [FixedBuffer(typeof(byte), 8)]
        public <Data>e__FixedBuffer Data;
        static MyOctreeNode()
        {
            ContentFilter = new FilterFunction(MyOctreeNode.SignedDistanceFilterInternal);
            MaterialFilter = new FilterFunction(MyOctreeNode.HistogramFilterInternal);
        }

        public MyOctreeNode(byte allContent)
        {
            this.ChildMask = 0;
            this.SetAllData(allContent);
        }

        public bool HasChildren =>
            (this.ChildMask != 0);
        public void ClearChildren()
        {
            this.ChildMask = 0;
        }

        public void SetChildren()
        {
            this.ChildMask = 0xff;
        }

        public bool HasChild(int childIndex) => 
            ((this.ChildMask & (1 << (childIndex & 0x1f))) != 0);

        public void SetChild(int childIndex, bool childPresent)
        {
            int num = 1 << (childIndex & 0x1f);
            if (childPresent)
            {
                this.ChildMask = (byte) (this.ChildMask | ((byte) num));
            }
            else
            {
                this.ChildMask = (byte) (this.ChildMask & ((byte) ~num));
            }
        }

        public unsafe void SetAllData(byte value)
        {
            SetAllData(&this.Data.FixedElementField, value);
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public static unsafe void SetAllData(byte* dst, byte value)
        {
            for (int i = 0; i < 8; i++)
            {
                dst[i] = value;
            }
        }

        public unsafe void SetData(int childIndex, byte data)
        {
            &this.Data.FixedElementField[childIndex] = data;
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public unsafe byte GetData(int cellIndex) => 
            &this.Data.FixedElementField[cellIndex];

        public unsafe byte ComputeFilteredValue(FilterFunction filter, int lod)
        {
            byte* pData = &this.Data.FixedElementField;
            return filter(pData, lod);
        }

        public unsafe bool AllDataSame() => 
            AllDataSame(&this.Data.FixedElementField);

        public static unsafe bool AllDataSame(byte* pData)
        {
            byte num = pData[0];
            for (int i = 1; i < 8; i++)
            {
                if (pData[i] != num)
                {
                    return false;
                }
            }
            return true;
        }

        public unsafe bool AllDataSame(byte value) => 
            AllDataSame(&this.Data.FixedElementField, value);

        public static unsafe bool AllDataSame(byte* pData, byte value)
        {
            for (int i = 1; i < 8; i++)
            {
                if (pData[i] != value)
                {
                    return false;
                }
            }
            return true;
        }

        public override unsafe string ToString()
        {
            StringBuilder builder = new StringBuilder(20);
            builder.Append("0x").Append(this.ChildMask.ToString("X2")).Append(": ");
            byte* numPtr = &this.Data.FixedElementField;
            for (int i = 0; i < 8; i++)
            {
                if (i != 0)
                {
                    builder.Append(", ");
                }
                builder.Append(numPtr[i]);
            }
            fixed (byte* numRef = null)
            {
                return builder.ToString();
            }
        }

        [Conditional("DEBUG")]
        private void AssertChildIndex(int cellIndex)
        {
        }

        private static unsafe byte AverageFilter(byte* pData, int lod)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                num += pData[i];
            }
            return (byte) (num / 8);
        }

        private static float ToSignedDistance(byte value) => 
            (((((float) value) / 255f) * 2f) - 1f);

        private static byte FromSignedDistance(float value) => 
            ((byte) ((((value * 0.5f) + 0.5f) * 255f) + 0.5f));

        private static unsafe byte SignedDistanceFilterInternal(byte* pData, int lod)
        {
            float num = ToSignedDistance(pData[0]);
            if ((ToSignedDistance(AverageValueFilterInternal(pData, lod)) != num) || ((num != 1f) && (num != -1f)))
            {
                num *= 0.5f;
            }
            return FromSignedDistance(num);
        }

        private static unsafe byte AverageValueFilterInternal(byte* pData, int lod)
        {
            float num = 0f;
            for (int i = 0; i < 8; i++)
            {
                num += ToSignedDistance(pData[i]);
            }
            num /= 8f;
            if ((num != 1f) && (num != -1f))
            {
                num *= 0.5f;
            }
            return FromSignedDistance(num);
        }

        private static unsafe byte IsoSurfaceFilterInternal(byte* pData, int lod)
        {
            byte num = 0;
            byte num2 = 0xff;
            int num3 = 0;
            int num4 = 0;
            for (int i = 0; i < 8; i++)
            {
                byte num7 = pData[i];
                if (num7 < 0x7f)
                {
                    num4++;
                    if (num7 > num)
                    {
                        num = num7;
                    }
                }
                else
                {
                    num3++;
                    if (num7 < num2)
                    {
                        num2 = num7;
                    }
                }
            }
            float num5 = ((((num4 > num3) ? ((float) num) : ((float) num2)) / 255f) * 2f) - 1f;
            if ((num5 != 1f) && (num5 != -1f))
            {
                num5 *= 0.5f;
            }
            return (byte) (((num5 * 0.5f) + 0.5f) * 255f);
        }

        private static unsafe byte HistogramFilterInternal(byte* pdata, int lod)
        {
            if (Histogram == null)
            {
                Histogram = new Dictionary<byte, int>(8);
            }
            for (int i = 0; i < 8; i++)
            {
                byte key = pdata[i];
                if (key != 0xff)
                {
                    int num5;
                    Histogram.TryGetValue(key, out num5);
                    Histogram[key] = num5 + 1;
                }
            }
            if (Histogram.Count == 0)
            {
                return 0xff;
            }
            byte key = 0;
            int num2 = 0;
            foreach (KeyValuePair<byte, int> pair in Histogram)
            {
                if (pair.Value > num2)
                {
                    num2 = pair.Value;
                    key = pair.Key;
                }
            }
            Histogram.Clear();
            return key;
        }

        public unsafe bool AnyAboveIso()
        {
            byte* numPtr = &this.Data.FixedElementField;
            for (int i = 0; i < 8; i++)
            {
                if (numPtr[i] > 0x7f)
                {
                    return true;
                }
            }
            fixed (byte* numRef = null)
            {
                return false;
            }
        }
        [StructLayout(LayoutKind.Sequential, Size=8), CompilerGenerated, UnsafeValueType]
        public struct <Data>e__FixedBuffer
        {
            public byte FixedElementField;
        }

        public unsafe delegate byte FilterFunction(byte* pData, int lod);
    }
}


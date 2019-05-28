namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unsharper;
    using VRage.Library.Collections;

    [UnsharperDisableReflection]
    public static class BitStreamExtensions
    {
        private static void Serialize<T>(this BitStream bs, T[] data, int len, SerializeCallback<T> serializer)
        {
            for (int i = 0; i < len; i++)
            {
                serializer(bs, ref data[i]);
            }
        }

        public static void SerializeList(this BitStream bs, ref List<int> list)
        {
            bs.SerializeList<int>(ref list, b => b.ReadInt32(0x20), (b, v) => b.WriteInt32(v, 0x20));
        }

        public static void SerializeList(this BitStream bs, ref List<long> list)
        {
            bs.SerializeList<long>(ref list, b => b.ReadInt64(0x40), (b, v) => b.WriteInt64(v, 0x40));
        }

        public static void SerializeList(this BitStream bs, ref List<uint> list)
        {
            bs.SerializeList<uint>(ref list, b => b.ReadUInt32(0x20), (b, v) => b.WriteUInt32(v, 0x20));
        }

        public static void SerializeList(this BitStream bs, ref List<ulong> list)
        {
            bs.SerializeList<ulong>(ref list, b => b.ReadUInt64(0x40), (b, v) => b.WriteUInt64(v, 0x40));
        }

        public static void SerializeList<T>(this BitStream bs, ref List<T> list, SerializeCallback<T> serializer)
        {
            if (bs.Writing)
            {
                bs.WriteVariant((uint) list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    T item = list[i];
                    serializer(bs, ref item);
                }
            }
            else
            {
                T item = default(T);
                int capacity = (int) bs.ReadUInt32Variant();
                list = list ?? new List<T>(capacity);
                list.Clear();
                for (int i = 0; i < capacity; i++)
                {
                    serializer(bs, ref item);
                    list.Add(item);
                }
            }
        }

        public static void SerializeList<T>(this BitStream bs, ref List<T> list, Reader<T> reader, Writer<T> writer)
        {
            if (bs.Writing)
            {
                bs.WriteVariant((uint) list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    writer(bs, list[i]);
                }
            }
            else
            {
                int capacity = (int) bs.ReadUInt32Variant();
                list = list ?? new List<T>(capacity);
                list.Clear();
                for (int i = 0; i < capacity; i++)
                {
                    list.Add(reader(bs));
                }
            }
        }

        public static void SerializeListVariant(this BitStream bs, ref List<int> list)
        {
            bs.SerializeList<int>(ref list, b => b.ReadInt32Variant(), (b, v) => b.WriteVariantSigned(v));
        }

        public static void SerializeListVariant(this BitStream bs, ref List<long> list)
        {
            bs.SerializeList<long>(ref list, b => b.ReadInt64Variant(), (b, v) => b.WriteVariantSigned(v));
        }

        public static void SerializeListVariant(this BitStream bs, ref List<uint> list)
        {
            bs.SerializeList<uint>(ref list, b => b.ReadUInt32Variant(), (b, v) => b.WriteVariant(v));
        }

        public static void SerializeListVariant(this BitStream bs, ref List<ulong> list)
        {
            bs.SerializeList<ulong>(ref list, b => b.ReadUInt64Variant(), (b, v) => b.WriteVariant(v));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BitStreamExtensions.<>c <>9 = new BitStreamExtensions.<>c();
            public static BitStreamExtensions.Reader<int> <>9__6_0;
            public static BitStreamExtensions.Writer<int> <>9__6_1;
            public static BitStreamExtensions.Reader<uint> <>9__7_0;
            public static BitStreamExtensions.Writer<uint> <>9__7_1;
            public static BitStreamExtensions.Reader<long> <>9__8_0;
            public static BitStreamExtensions.Writer<long> <>9__8_1;
            public static BitStreamExtensions.Reader<ulong> <>9__9_0;
            public static BitStreamExtensions.Writer<ulong> <>9__9_1;
            public static BitStreamExtensions.Reader<int> <>9__10_0;
            public static BitStreamExtensions.Writer<int> <>9__10_1;
            public static BitStreamExtensions.Reader<uint> <>9__11_0;
            public static BitStreamExtensions.Writer<uint> <>9__11_1;
            public static BitStreamExtensions.Reader<long> <>9__12_0;
            public static BitStreamExtensions.Writer<long> <>9__12_1;
            public static BitStreamExtensions.Reader<ulong> <>9__13_0;
            public static BitStreamExtensions.Writer<ulong> <>9__13_1;

            internal int <SerializeList>b__6_0(BitStream b) => 
                b.ReadInt32(0x20);

            internal void <SerializeList>b__6_1(BitStream b, int v)
            {
                b.WriteInt32(v, 0x20);
            }

            internal uint <SerializeList>b__7_0(BitStream b) => 
                b.ReadUInt32(0x20);

            internal void <SerializeList>b__7_1(BitStream b, uint v)
            {
                b.WriteUInt32(v, 0x20);
            }

            internal long <SerializeList>b__8_0(BitStream b) => 
                b.ReadInt64(0x40);

            internal void <SerializeList>b__8_1(BitStream b, long v)
            {
                b.WriteInt64(v, 0x40);
            }

            internal ulong <SerializeList>b__9_0(BitStream b) => 
                b.ReadUInt64(0x40);

            internal void <SerializeList>b__9_1(BitStream b, ulong v)
            {
                b.WriteUInt64(v, 0x40);
            }

            internal int <SerializeListVariant>b__10_0(BitStream b) => 
                b.ReadInt32Variant();

            internal void <SerializeListVariant>b__10_1(BitStream b, int v)
            {
                b.WriteVariantSigned(v);
            }

            internal uint <SerializeListVariant>b__11_0(BitStream b) => 
                b.ReadUInt32Variant();

            internal void <SerializeListVariant>b__11_1(BitStream b, uint v)
            {
                b.WriteVariant(v);
            }

            internal long <SerializeListVariant>b__12_0(BitStream b) => 
                b.ReadInt64Variant();

            internal void <SerializeListVariant>b__12_1(BitStream b, long v)
            {
                b.WriteVariantSigned(v);
            }

            internal ulong <SerializeListVariant>b__13_0(BitStream b) => 
                b.ReadUInt64Variant();

            internal void <SerializeListVariant>b__13_1(BitStream b, ulong v)
            {
                b.WriteVariant(v);
            }
        }

        public delegate T Reader<T>(BitStream bs);

        public delegate void SerializeCallback<T>(BitStream stream, ref T item);

        public delegate void Writer<T>(BitStream bs, T value);
    }
}


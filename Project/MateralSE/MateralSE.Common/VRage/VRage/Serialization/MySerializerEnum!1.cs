namespace VRage.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;

    public class MySerializerEnum<TEnum> : MySerializer<TEnum> where TEnum: struct, IComparable, IFormattable, IConvertible
    {
        private static readonly int m_valueCount;
        private static readonly TEnum m_firstValue;
        private static readonly TEnum m_secondValue;
        private static readonly ulong m_firstUlong;
        private static readonly int m_bitCount;
        public static readonly bool HasNegativeValues;

        static MySerializerEnum()
        {
            MySerializerEnum<TEnum>.m_valueCount = MyEnum<TEnum>.Values.Length;
            MySerializerEnum<TEnum>.m_firstValue = MyEnum<TEnum>.Values.FirstOrDefault<TEnum>();
            MySerializerEnum<TEnum>.m_secondValue = MyEnum<TEnum>.Values.Skip<TEnum>(1).FirstOrDefault<TEnum>();
            MySerializerEnum<TEnum>.m_firstUlong = MyEnum<TEnum>.GetValue(MySerializerEnum<TEnum>.m_firstValue);
            MySerializerEnum<TEnum>.m_bitCount = ((int) Math.Log((double) MyEnum<TEnum>.GetValue(MyEnum<TEnum>.Range.Max), 2.0)) + 1;
            TEnum y = default(TEnum);
            MySerializerEnum<TEnum>.HasNegativeValues = Comparer<TEnum>.Default.Compare(MyEnum<TEnum>.Range.Min, y) < 0;
        }

        public override void Clone(ref TEnum value)
        {
        }

        public override bool Equals(ref TEnum a, ref TEnum b) => 
            (MyEnum<TEnum>.GetValue(a) == MyEnum<TEnum>.GetValue(b));

        public override void Read(BitStream stream, out TEnum value, MySerializeInfo info)
        {
            if (MySerializerEnum<TEnum>.m_valueCount == 1)
            {
                value = MySerializerEnum<TEnum>.m_firstValue;
            }
            else if (MySerializerEnum<TEnum>.m_valueCount == 2)
            {
                value = stream.ReadBool() ? MySerializerEnum<TEnum>.m_firstValue : MySerializerEnum<TEnum>.m_secondValue;
            }
            else if (MySerializerEnum<TEnum>.m_valueCount <= 2)
            {
                value = default(TEnum);
            }
            else if (MySerializerEnum<TEnum>.HasNegativeValues)
            {
                value = MyEnum<TEnum>.SetValue((ulong) stream.ReadInt64Variant());
            }
            else
            {
                value = MyEnum<TEnum>.SetValue(stream.ReadUInt64(MySerializerEnum<TEnum>.m_bitCount));
            }
        }

        public override void Write(BitStream stream, ref TEnum value, MySerializeInfo info)
        {
            ulong num = MyEnum<TEnum>.GetValue(value);
            if (MySerializerEnum<TEnum>.m_valueCount == 2)
            {
                stream.WriteBool(num == MySerializerEnum<TEnum>.m_firstUlong);
            }
            else if (MySerializerEnum<TEnum>.m_valueCount > 2)
            {
                if (MySerializerEnum<TEnum>.HasNegativeValues)
                {
                    stream.WriteVariantSigned((long) num);
                }
                else
                {
                    stream.WriteUInt64(num, MySerializerEnum<TEnum>.m_bitCount);
                }
            }
        }
    }
}


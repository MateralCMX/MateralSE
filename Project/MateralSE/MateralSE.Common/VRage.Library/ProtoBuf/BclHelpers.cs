namespace ProtoBuf
{
    using System;
    using System.Runtime.Serialization;
    using Unsharper;

    public static class BclHelpers
    {
        private const int FieldTimeSpanValue = 1;
        private const int FieldTimeSpanScale = 2;
        internal static readonly DateTime EpochOrigin = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
        private const int FieldDecimalLow = 1;
        private const int FieldDecimalHigh = 2;
        private const int FieldDecimalSignScale = 3;
        private const int FieldGuidLow = 1;
        private const int FieldGuidHigh = 2;
        private const int FieldExistingObjectKey = 1;
        private const int FieldNewObjectKey = 2;
        private const int FieldExistingTypeKey = 3;
        private const int FieldNewTypeKey = 4;
        private const int FieldTypeName = 8;
        private const int FieldObject = 10;

        public static object GetUninitializedObject(Type type) => 
            FormatterServices.GetUninitializedObject(type);

        public static DateTime ReadDateTime(ProtoReader source)
        {
            long num = ReadTimeSpanTicks(source);
            if (num == -9223372036854775808L)
            {
                return DateTime.MinValue;
            }
            if (num == 0x7fffffffffffffffL)
            {
                return DateTime.MaxValue;
            }
            return EpochOrigin.AddTicks(num);
        }

        public static decimal ReadDecimal(ProtoReader reader)
        {
            int num4;
            ulong num = 0UL;
            uint num2 = 0;
            uint num3 = 0;
            SubItemToken token = ProtoReader.StartSubItem(reader);
            while ((num4 = reader.ReadFieldHeader()) > 0)
            {
                switch (num4)
                {
                    case 1:
                    {
                        num = reader.ReadUInt64();
                        continue;
                    }
                    case 2:
                    {
                        num2 = reader.ReadUInt32();
                        continue;
                    }
                    case 3:
                    {
                        num3 = reader.ReadUInt32();
                        continue;
                    }
                }
                reader.SkipField();
            }
            ProtoReader.EndSubItem(token, reader);
            if ((num != 0) || (num2 != 0))
            {
                return new decimal((int) (num & 0xffffffffUL), (int) ((num >> 0x20) & 0xffffffffUL), (int) num2, (num3 & 1) == 1, (byte) ((num3 & 510) >> 1));
            }
            return 0M;
        }

        [UnsharperDisableReflection]
        public static Guid ReadGuid(ProtoReader source)
        {
            int num3;
            ulong num = 0UL;
            ulong num2 = 0UL;
            SubItemToken token = ProtoReader.StartSubItem(source);
            while ((num3 = source.ReadFieldHeader()) > 0)
            {
                if (num3 == 1)
                {
                    num = source.ReadUInt64();
                    continue;
                }
                if (num3 != 2)
                {
                    source.SkipField();
                    continue;
                }
                num2 = source.ReadUInt64();
            }
            ProtoReader.EndSubItem(token, source);
            if ((num == 0) && (num2 == 0))
            {
                return Guid.Empty;
            }
            uint num4 = (uint) (num >> 0x20);
            uint num6 = (uint) (num2 >> 0x20);
            uint num7 = (uint) num2;
            return new Guid((int) ((uint) num), (short) num4, (short) (num4 >> 0x10), (byte) num7, (byte) (num7 >> 8), (byte) (num7 >> 0x10), (byte) (num7 >> 0x18), (byte) num6, (byte) (num6 >> 8), (byte) (num6 >> 0x10), (byte) (num6 >> 0x18));
        }

        public static object ReadNetObject(object value, ProtoReader source, int key, Type type, NetObjectOptions options)
        {
            int num;
            SubItemToken token = ProtoReader.StartSubItem(source);
            int newObjectKey = -1;
            int num3 = -1;
            while ((num = source.ReadFieldHeader()) > 0)
            {
                int num4;
                switch (num)
                {
                    case 1:
                    {
                        num4 = source.ReadInt32();
                        value = source.NetCache.GetKeyedObject(num4);
                        continue;
                    }
                    case 2:
                    {
                        newObjectKey = source.ReadInt32();
                        continue;
                    }
                    case 3:
                    {
                        num4 = source.ReadInt32();
                        type = (Type) source.NetCache.GetKeyedObject(num4);
                        key = source.GetTypeKey(ref type);
                        continue;
                    }
                    case 4:
                    {
                        num3 = source.ReadInt32();
                        continue;
                    }
                    case 8:
                    {
                        string str = source.ReadString();
                        type = source.DeserializeType(str);
                        if (type == null)
                        {
                            throw new ProtoException("Unable to resolve type: " + str + " (you can use the TypeModel.DynamicTypeFormatting event to provide a custom mapping)");
                        }
                        if (type == typeof(string))
                        {
                            key = -1;
                            continue;
                        }
                        key = source.GetTypeKey(ref type);
                        if (key < 0)
                        {
                            throw new InvalidOperationException("Dynamic type is not a contract-type: " + type.Name);
                        }
                        continue;
                    }
                    case 10:
                    {
                        bool flag = type == typeof(string);
                        bool flag2 = value == null;
                        bool flag3 = flag2 && (flag || ((options & NetObjectOptions.LateSet) != NetObjectOptions.None));
                        if ((newObjectKey >= 0) && !flag3)
                        {
                            if (value == null)
                            {
                                source.TrapNextObject(newObjectKey);
                            }
                            else
                            {
                                source.NetCache.SetKeyedObject(newObjectKey, value);
                            }
                            if (num3 >= 0)
                            {
                                source.NetCache.SetKeyedObject(num3, type);
                            }
                        }
                        object keyedObject = value;
                        value = !flag ? ProtoReader.ReadTypedObject(keyedObject, key, source, type) : source.ReadString();
                        if (newObjectKey >= 0)
                        {
                            if (flag2 && !flag3)
                            {
                                keyedObject = source.NetCache.GetKeyedObject(newObjectKey);
                            }
                            if (flag3)
                            {
                                source.NetCache.SetKeyedObject(newObjectKey, value);
                                if (num3 >= 0)
                                {
                                    source.NetCache.SetKeyedObject(num3, type);
                                }
                            }
                        }
                        if (((newObjectKey >= 0) && !flag3) && (keyedObject != value))
                        {
                            throw new ProtoException("A reference-tracked object changed reference during deserialization");
                        }
                        if ((newObjectKey < 0) && (num3 >= 0))
                        {
                            source.NetCache.SetKeyedObject(num3, type);
                        }
                        continue;
                    }
                }
                source.SkipField();
            }
            if ((newObjectKey >= 0) && ((options & NetObjectOptions.AsReference) == NetObjectOptions.None))
            {
                throw new ProtoException("Object key in input stream, but reference-tracking was not expected");
            }
            ProtoReader.EndSubItem(token, source);
            return value;
        }

        public static TimeSpan ReadTimeSpan(ProtoReader source)
        {
            long num = ReadTimeSpanTicks(source);
            return ((num != -9223372036854775808L) ? ((num != 0x7fffffffffffffffL) ? TimeSpan.FromTicks(num) : TimeSpan.MaxValue) : TimeSpan.MinValue);
        }

        private static long ReadTimeSpanTicks(ProtoReader source)
        {
            int num;
            WireType wireType = source.WireType;
            if (wireType == WireType.Fixed64)
            {
                return source.ReadInt64();
            }
            if ((wireType - 2) > WireType.Fixed64)
            {
                throw new ProtoException("Unexpected wire-type: " + source.WireType.ToString());
            }
            SubItemToken token = ProtoReader.StartSubItem(source);
            TimeSpanScale days = TimeSpanScale.Days;
            long num2 = 0L;
            while ((num = source.ReadFieldHeader()) > 0)
            {
                if (num == 1)
                {
                    source.Assert(WireType.SignedVariant);
                    num2 = source.ReadInt64();
                    continue;
                }
                if (num == 2)
                {
                    days = (TimeSpanScale) source.ReadInt32();
                    continue;
                }
                source.SkipField();
            }
            ProtoReader.EndSubItem(token, source);
            switch (days)
            {
                case TimeSpanScale.Days:
                    return (num2 * 0xc92a69c000L);

                case TimeSpanScale.Hours:
                    return (num2 * 0x861c46800L);

                case TimeSpanScale.Minutes:
                    return (num2 * 0x23c34600L);

                case TimeSpanScale.Seconds:
                    return (num2 * 0x989680L);

                case TimeSpanScale.Milliseconds:
                    return (num2 * 0x2710L);

                case TimeSpanScale.Ticks:
                    return num2;
            }
            if (days != TimeSpanScale.MinMax)
            {
                throw new ProtoException("Unknown timescale: " + days.ToString());
            }
            if (num2 == -1L)
            {
                return -9223372036854775808L;
            }
            if (num2 != 1L)
            {
                throw new ProtoException("Unknown min/max value: " + num2.ToString());
            }
            return 0x7fffffffffffffffL;
        }

        public static void WriteDateTime(DateTime value, ProtoWriter dest)
        {
            TimeSpan timeSpan = ((dest.WireType - 2) > WireType.Fixed64) ? ((TimeSpan) (value - EpochOrigin)) : (!(value == DateTime.MaxValue) ? (!(value == DateTime.MinValue) ? ((TimeSpan) (value - EpochOrigin)) : TimeSpan.MinValue) : TimeSpan.MaxValue);
            WriteTimeSpan(timeSpan, dest);
        }

        public static void WriteDecimal(decimal value, ProtoWriter writer)
        {
            int[] bits = decimal.GetBits(value);
            ulong num3 = (ulong) ((bits[1] << 0x20) | (bits[0] & 0xffffffffUL));
            uint num4 = (uint) bits[2];
            uint num5 = (uint) (((bits[3] >> 15) & 510) | ((bits[3] >> 0x1f) & 1));
            SubItemToken token = ProtoWriter.StartSubItem(null, writer);
            if (num3 != 0)
            {
                ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
                ProtoWriter.WriteUInt64(num3, writer);
            }
            if (num4 != 0)
            {
                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                ProtoWriter.WriteUInt32(num4, writer);
            }
            if (num5 != 0)
            {
                ProtoWriter.WriteFieldHeader(3, WireType.Variant, writer);
                ProtoWriter.WriteUInt32(num5, writer);
            }
            ProtoWriter.EndSubItem(token, writer);
        }

        public static void WriteGuid(Guid value, ProtoWriter dest)
        {
            byte[] data = value.ToByteArray();
            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            if (value != Guid.Empty)
            {
                ProtoWriter.WriteFieldHeader(1, WireType.Fixed64, dest);
                ProtoWriter.WriteBytes(data, 0, 8, dest);
                ProtoWriter.WriteFieldHeader(2, WireType.Fixed64, dest);
                ProtoWriter.WriteBytes(data, 8, 8, dest);
            }
            ProtoWriter.EndSubItem(token, dest);
        }

        public static void WriteNetObject(object value, ProtoWriter dest, int key, NetObjectOptions options)
        {
            bool flag = (options & NetObjectOptions.DynamicType) != NetObjectOptions.None;
            WireType wireType = dest.WireType;
            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            bool flag3 = true;
            if ((options & NetObjectOptions.AsReference) != NetObjectOptions.None)
            {
                bool flag4;
                int num = dest.NetCache.AddObjectKey(value, out flag4);
                ProtoWriter.WriteFieldHeader(flag4 ? 1 : 2, WireType.Variant, dest);
                ProtoWriter.WriteInt32(num, dest);
                if (flag4)
                {
                    flag3 = false;
                }
            }
            if (flag3)
            {
                if (flag)
                {
                    bool flag5;
                    Type type = value.GetType();
                    if (!(value is string))
                    {
                        key = dest.GetTypeKey(ref type);
                        if (key < 0)
                        {
                            throw new InvalidOperationException("Dynamic type is not a contract-type: " + type.Name);
                        }
                    }
                    int num2 = dest.NetCache.AddObjectKey(type, out flag5);
                    ProtoWriter.WriteFieldHeader(flag5 ? 3 : 4, WireType.Variant, dest);
                    ProtoWriter.WriteInt32(num2, dest);
                    if (!flag5)
                    {
                        ProtoWriter.WriteFieldHeader(8, WireType.String, dest);
                        ProtoWriter.WriteString(dest.SerializeType(type), dest);
                    }
                }
                ProtoWriter.WriteFieldHeader(10, wireType, dest);
                if (value is string)
                {
                    ProtoWriter.WriteString((string) value, dest);
                }
                else
                {
                    ProtoWriter.WriteObject(value, key, dest);
                }
            }
            ProtoWriter.EndSubItem(token, dest);
        }

        public static void WriteTimeSpan(TimeSpan timeSpan, ProtoWriter dest)
        {
            WireType wireType = dest.WireType;
            if (wireType == WireType.Fixed64)
            {
                ProtoWriter.WriteInt64(timeSpan.Ticks, dest);
            }
            else
            {
                TimeSpanScale minMax;
                if ((wireType - 2) > WireType.Fixed64)
                {
                    throw new ProtoException("Unexpected wire-type: " + dest.WireType.ToString());
                }
                long ticks = timeSpan.Ticks;
                if (timeSpan == TimeSpan.MaxValue)
                {
                    ticks = 1L;
                    minMax = TimeSpanScale.MinMax;
                }
                else if (timeSpan == TimeSpan.MinValue)
                {
                    ticks = -1L;
                    minMax = TimeSpanScale.MinMax;
                }
                else if ((ticks % 0xc92a69c000L) == 0)
                {
                    minMax = TimeSpanScale.Days;
                    ticks /= 0xc92a69c000L;
                }
                else if ((ticks % 0x861c46800L) == 0)
                {
                    minMax = TimeSpanScale.Hours;
                    ticks /= 0x861c46800L;
                }
                else if ((ticks % 0x23c34600L) == 0)
                {
                    minMax = TimeSpanScale.Minutes;
                    ticks /= 0x23c34600L;
                }
                else if ((ticks % 0x989680L) == 0)
                {
                    minMax = TimeSpanScale.Seconds;
                    ticks /= 0x989680L;
                }
                else if ((ticks % 0x2710L) != 0)
                {
                    minMax = TimeSpanScale.Ticks;
                }
                else
                {
                    minMax = TimeSpanScale.Milliseconds;
                    ticks /= 0x2710L;
                }
                SubItemToken token = ProtoWriter.StartSubItem(null, dest);
                if (ticks != 0)
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.SignedVariant, dest);
                    ProtoWriter.WriteInt64(ticks, dest);
                }
                if (minMax != TimeSpanScale.Days)
                {
                    ProtoWriter.WriteFieldHeader(2, WireType.Variant, dest);
                    ProtoWriter.WriteInt32((int) minMax, dest);
                }
                ProtoWriter.EndSubItem(token, dest);
            }
        }

        [Flags]
        public enum NetObjectOptions : byte
        {
            None = 0,
            AsReference = 1,
            DynamicType = 2,
            UseConstructor = 4,
            LateSet = 8
        }
    }
}


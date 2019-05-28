namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using ProtoBuf.Serializers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ValueMember
    {
        private readonly int fieldNumber;
        private readonly MemberInfo member;
        private readonly Type parentType;
        private readonly Type itemType;
        private readonly Type defaultType;
        private readonly Type memberType;
        private object defaultValue;
        private readonly RuntimeTypeModel model;
        private IProtoSerializer serializer;
        private ProtoBuf.DataFormat dataFormat;
        private bool asReference;
        private bool dynamicType;
        private MethodInfo getSpecified;
        private MethodInfo setSpecified;
        private string name;
        private const byte OPTIONS_IsStrict = 1;
        private const byte OPTIONS_IsPacked = 2;
        private const byte OPTIONS_IsRequired = 4;
        private const byte OPTIONS_OverwriteList = 8;
        private const byte OPTIONS_SupportNull = 0x10;
        private byte flags;

        internal ValueMember(RuntimeTypeModel model, int fieldNumber, Type memberType, Type itemType, Type defaultType, ProtoBuf.DataFormat dataFormat)
        {
            if (memberType == null)
            {
                throw new ArgumentNullException("memberType");
            }
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            this.fieldNumber = fieldNumber;
            this.memberType = memberType;
            this.itemType = itemType;
            this.defaultType = defaultType;
            this.model = model;
            this.dataFormat = dataFormat;
        }

        public ValueMember(RuntimeTypeModel model, Type parentType, int fieldNumber, MemberInfo member, Type memberType, Type itemType, Type defaultType, ProtoBuf.DataFormat dataFormat, object defaultValue) : this(model, fieldNumber, memberType, itemType, defaultType, dataFormat)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            if (parentType == null)
            {
                throw new ArgumentNullException("parentType");
            }
            if ((fieldNumber < 1) && !Helpers.IsEnum(parentType))
            {
                throw new ArgumentOutOfRangeException("fieldNumber");
            }
            this.member = member;
            this.parentType = parentType;
            if ((fieldNumber < 1) && !Helpers.IsEnum(parentType))
            {
                throw new ArgumentOutOfRangeException("fieldNumber");
            }
            if ((defaultValue != null) && (model.MapType(defaultValue.GetType()) != memberType))
            {
                object obj1 = ParseDefaultValue(memberType, defaultValue);
                defaultValue = obj1;
            }
            this.defaultValue = defaultValue;
            MetaType type = model.FindWithoutAdd(memberType);
            if (type != null)
            {
                this.asReference = type.AsReferenceDefault;
            }
        }

        private IProtoSerializer BuildSerializer()
        {
            IProtoSerializer serializer2;
            int opaqueToken = 0;
            try
            {
                WireType type;
                this.model.TakeLock(ref opaqueToken);
                Type type2 = (this.itemType == null) ? this.memberType : this.itemType;
                IProtoSerializer tail = TryGetCoreSerializer(this.model, this.dataFormat, type2, out type, this.asReference, this.dynamicType, this.OverwriteList, true);
                if (tail == null)
                {
                    throw new InvalidOperationException("No serializer defined for type: " + type2.FullName);
                }
                if ((this.itemType == null) || !this.SupportNull)
                {
                    tail = new TagDecorator(this.fieldNumber, type, this.IsStrict, tail);
                }
                else
                {
                    if (this.IsPacked)
                    {
                        throw new NotSupportedException("Packed encodings cannot support null values");
                    }
                    tail = new TagDecorator(1, type, this.IsStrict, tail);
                    tail = new NullDecorator(this.model, tail);
                    tail = new TagDecorator(this.fieldNumber, WireType.StartGroup, false, tail);
                }
                if (this.itemType == null)
                {
                    if (((this.defaultValue != null) && !this.IsRequired) && (this.getSpecified == null))
                    {
                        tail = new DefaultValueDecorator(this.model, this.defaultValue, tail);
                    }
                }
                else
                {
                    Type itemType;
                    if (this.SupportNull)
                    {
                        itemType = this.itemType;
                    }
                    else
                    {
                        Type underlyingType = Helpers.GetUnderlyingType(this.itemType);
                        itemType = underlyingType ?? this.itemType;
                    }
                    Type type3 = itemType;
                    tail = !this.memberType.IsArray ? ((IProtoSerializer) new ListDecorator(this.model, this.memberType, this.defaultType, tail, this.fieldNumber, this.IsPacked, type, (this.member != null) && PropertyDecorator.CanWrite(this.model, this.member), this.OverwriteList, this.SupportNull)) : ((IProtoSerializer) new ArrayDecorator(this.model, tail, this.fieldNumber, this.IsPacked, type, this.memberType, this.OverwriteList, this.SupportNull));
                }
                if (this.memberType == this.model.MapType(typeof(Uri)))
                {
                    tail = new UriDecorator(this.model, tail);
                }
                if (this.member != null)
                {
                    if (this.member is PropertyInfo)
                    {
                        tail = new PropertyDecorator(this.model, this.parentType, (PropertyInfo) this.member, tail);
                    }
                    else
                    {
                        if (!(this.member is FieldInfo))
                        {
                            throw new InvalidOperationException();
                        }
                        tail = new FieldDecorator(this.parentType, (FieldInfo) this.member, tail);
                    }
                    if ((this.getSpecified != null) || (this.setSpecified != null))
                    {
                        tail = new MemberSpecifiedDecorator(this.getSpecified, this.setSpecified, tail);
                    }
                }
                serializer2 = tail;
            }
            finally
            {
                this.model.ReleaseLock(opaqueToken);
            }
            return serializer2;
        }

        private static WireType GetDateTimeWireType(ProtoBuf.DataFormat format)
        {
            switch (format)
            {
                case ProtoBuf.DataFormat.Default:
                    return WireType.String;

                case ProtoBuf.DataFormat.FixedSize:
                    return WireType.Fixed64;

                case ProtoBuf.DataFormat.Group:
                    return WireType.StartGroup;
            }
            throw new InvalidOperationException();
        }

        private static WireType GetIntWireType(ProtoBuf.DataFormat format, int width)
        {
            switch (format)
            {
                case ProtoBuf.DataFormat.Default:
                case ProtoBuf.DataFormat.TwosComplement:
                    return WireType.Variant;

                case ProtoBuf.DataFormat.ZigZag:
                    return WireType.SignedVariant;

                case ProtoBuf.DataFormat.FixedSize:
                    return ((width == 0x20) ? WireType.Fixed32 : WireType.Fixed64);
            }
            throw new InvalidOperationException();
        }

        internal object GetRawEnumValue() => 
            ((FieldInfo) this.member).GetRawConstantValue();

        internal string GetSchemaTypeName(bool applyNetObjectProxy, ref bool requiresBclImport)
        {
            Type itemType = this.ItemType;
            if (itemType == null)
            {
                itemType = this.MemberType;
            }
            return this.model.GetSchemaTypeName(itemType, this.DataFormat, applyNetObjectProxy && this.asReference, applyNetObjectProxy && this.dynamicType, ref requiresBclImport);
        }

        private bool HasFlag(byte flag) => 
            ((this.flags & flag) == flag);

        private static object ParseDefaultValue(Type type, object value)
        {
            Type underlyingType = Helpers.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            if (value is string)
            {
                string str = (string) value;
                if (Helpers.IsEnum(type))
                {
                    return Helpers.ParseEnum(type, str);
                }
                ProtoTypeCode typeCode = Helpers.GetTypeCode(type);
                switch (typeCode)
                {
                    case ProtoTypeCode.Boolean:
                        return bool.Parse(str);

                    case ProtoTypeCode.Char:
                        if (str.Length != 1)
                        {
                            throw new FormatException("Single character expected: \"" + str + "\"");
                        }
                        return str[0];

                    case ProtoTypeCode.SByte:
                        return sbyte.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Byte:
                        return byte.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Int16:
                        return short.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.UInt16:
                        return ushort.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Int32:
                        return int.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.UInt32:
                        return uint.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Int64:
                        return long.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.UInt64:
                        return ulong.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Single:
                        return float.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Double:
                        return double.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.Decimal:
                        return decimal.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

                    case ProtoTypeCode.DateTime:
                        return DateTime.Parse(str, CultureInfo.InvariantCulture);

                    case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                        break;

                    case ProtoTypeCode.String:
                        return str;

                    default:
                        switch (typeCode)
                        {
                            case ProtoTypeCode.TimeSpan:
                                return TimeSpan.Parse(str);

                            case ProtoTypeCode.Guid:
                                return new Guid(str);

                            case ProtoTypeCode.Uri:
                                return str;

                            default:
                                break;
                        }
                        break;
                }
            }
            return (!Helpers.IsEnum(type) ? Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : Enum.ToObject(type, value));
        }

        private void SetFlag(byte flag, bool value, bool throwIfFrozen)
        {
            if (throwIfFrozen && (this.HasFlag(flag) != value))
            {
                this.ThrowIfFrozen();
            }
            if (value)
            {
                this.flags = (byte) (this.flags | flag);
            }
            else
            {
                this.flags = (byte) (this.flags & ~flag);
            }
        }

        internal void SetName(string name)
        {
            this.ThrowIfFrozen();
            this.name = name;
        }

        public void SetSpecified(MethodInfo getSpecified, MethodInfo setSpecified)
        {
            ParameterInfo[] infoArray;
            if ((getSpecified != null) && (((getSpecified.ReturnType != this.model.MapType(typeof(bool))) || getSpecified.IsStatic) || (getSpecified.GetParameters().Length != 0)))
            {
                throw new ArgumentException("Invalid pattern for checking member-specified", "getSpecified");
            }
            if ((setSpecified != null) && (((setSpecified.ReturnType != this.model.MapType(typeof(void))) || (setSpecified.IsStatic || ((infoArray = setSpecified.GetParameters()).Length != 1))) || (infoArray[0].ParameterType != this.model.MapType(typeof(bool)))))
            {
                throw new ArgumentException("Invalid pattern for setting member-specified", "setSpecified");
            }
            this.ThrowIfFrozen();
            this.getSpecified = getSpecified;
            this.setSpecified = setSpecified;
        }

        private void ThrowIfFrozen()
        {
            if (this.serializer != null)
            {
                throw new InvalidOperationException("The type cannot be changed once a serializer has been generated");
            }
        }

        internal static IProtoSerializer TryGetCoreSerializer(RuntimeTypeModel model, ProtoBuf.DataFormat dataFormat, Type type, out WireType defaultWireType, bool asReference, bool dynamicType, bool overwriteList, bool allowComplexTypes)
        {
            Type underlyingType = Helpers.GetUnderlyingType(type);
            Type type3 = type;
            type = underlyingType ?? type3;
            if (Helpers.IsEnum(type))
            {
                if (!allowComplexTypes || (model == null))
                {
                    defaultWireType = WireType.None;
                    return null;
                }
                defaultWireType = WireType.Variant;
                return new EnumSerializer(type, model.GetEnumMap(type));
            }
            ProtoTypeCode typeCode = Helpers.GetTypeCode(type);
            switch (typeCode)
            {
                case ProtoTypeCode.Boolean:
                    defaultWireType = WireType.Variant;
                    return new BooleanSerializer(model);

                case ProtoTypeCode.Char:
                    defaultWireType = WireType.Variant;
                    return new CharSerializer(model);

                case ProtoTypeCode.SByte:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new SByteSerializer(model);

                case ProtoTypeCode.Byte:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new ByteSerializer(model);

                case ProtoTypeCode.Int16:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new Int16Serializer(model);

                case ProtoTypeCode.UInt16:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new UInt16Serializer(model);

                case ProtoTypeCode.Int32:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new Int32Serializer(model);

                case ProtoTypeCode.UInt32:
                    defaultWireType = GetIntWireType(dataFormat, 0x20);
                    return new UInt32Serializer(model);

                case ProtoTypeCode.Int64:
                    defaultWireType = GetIntWireType(dataFormat, 0x40);
                    return new Int64Serializer(model);

                case ProtoTypeCode.UInt64:
                    defaultWireType = GetIntWireType(dataFormat, 0x40);
                    return new UInt64Serializer(model);

                case ProtoTypeCode.Single:
                    defaultWireType = WireType.Fixed32;
                    return new SingleSerializer(model);

                case ProtoTypeCode.Double:
                    defaultWireType = WireType.Fixed64;
                    return new DoubleSerializer(model);

                case ProtoTypeCode.Decimal:
                    defaultWireType = WireType.String;
                    return new DecimalSerializer(model);

                case ProtoTypeCode.DateTime:
                    defaultWireType = GetDateTimeWireType(dataFormat);
                    return new DateTimeSerializer(model);

                case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                    break;

                case ProtoTypeCode.String:
                    defaultWireType = WireType.String;
                    return (!asReference ? ((IProtoSerializer) new ProtoBuf.Serializers.StringSerializer(model)) : ((IProtoSerializer) new NetObjectSerializer(model, model.MapType(typeof(string)), 0, BclHelpers.NetObjectOptions.AsReference)));

                default:
                    switch (typeCode)
                    {
                        case ProtoTypeCode.TimeSpan:
                            defaultWireType = GetDateTimeWireType(dataFormat);
                            return new TimeSpanSerializer(model);

                        case ProtoTypeCode.ByteArray:
                            defaultWireType = WireType.String;
                            return new BlobSerializer(model, overwriteList);

                        case ProtoTypeCode.Guid:
                            defaultWireType = WireType.String;
                            return new GuidSerializer(model);

                        case ProtoTypeCode.Uri:
                            defaultWireType = WireType.String;
                            return new ProtoBuf.Serializers.StringSerializer(model);

                        case ProtoTypeCode.Type:
                            defaultWireType = WireType.String;
                            return new SystemTypeSerializer(model);

                        default:
                            break;
                    }
                    break;
            }
            IProtoSerializer serializer = model.AllowParseableTypes ? ParseableSerializer.TryCreate(type, model) : null;
            if (serializer != null)
            {
                defaultWireType = WireType.String;
                return serializer;
            }
            if (allowComplexTypes && (model != null))
            {
                int key = model.GetKey(type, false, true);
                if (asReference | dynamicType)
                {
                    defaultWireType = WireType.String;
                    BclHelpers.NetObjectOptions none = BclHelpers.NetObjectOptions.None;
                    if (asReference)
                    {
                        none |= BclHelpers.NetObjectOptions.AsReference;
                    }
                    if (dynamicType)
                    {
                        none |= BclHelpers.NetObjectOptions.DynamicType;
                    }
                    if (key >= 0)
                    {
                        if (asReference && Helpers.IsValueType(type))
                        {
                            string message = "AsReference cannot be used with value-types";
                            message = (type.Name != "KeyValuePair`2") ? (message + ": " + type.FullName) : (message + "; please see http://stackoverflow.com/q/14436606/");
                            throw new InvalidOperationException(message);
                        }
                        MetaType type2 = model[type];
                        if (asReference && type2.IsAutoTuple)
                        {
                            none |= BclHelpers.NetObjectOptions.LateSet;
                        }
                        if (type2.UseConstructor)
                        {
                            none |= BclHelpers.NetObjectOptions.None | BclHelpers.NetObjectOptions.UseConstructor;
                        }
                    }
                    return new NetObjectSerializer(model, type, key, none);
                }
                if (key >= 0)
                {
                    defaultWireType = (dataFormat == ProtoBuf.DataFormat.Group) ? WireType.StartGroup : WireType.String;
                    return new SubItemSerializer(type, key, model[type], true);
                }
            }
            defaultWireType = WireType.None;
            return null;
        }

        public int FieldNumber =>
            this.fieldNumber;

        public MemberInfo Member =>
            this.member;

        public Type ItemType =>
            this.itemType;

        public Type MemberType =>
            this.memberType;

        public Type DefaultType =>
            this.defaultType;

        public Type ParentType =>
            this.parentType;

        public object DefaultValue
        {
            get => 
                this.defaultValue;
            set
            {
                this.ThrowIfFrozen();
                this.defaultValue = value;
            }
        }

        internal IProtoSerializer Serializer
        {
            get
            {
                if (this.serializer == null)
                {
                    this.serializer = this.BuildSerializer();
                }
                return this.serializer;
            }
        }

        public ProtoBuf.DataFormat DataFormat
        {
            get => 
                this.dataFormat;
            set
            {
                this.ThrowIfFrozen();
                this.dataFormat = value;
            }
        }

        public bool IsStrict
        {
            get => 
                this.HasFlag(1);
            set => 
                this.SetFlag(1, value, true);
        }

        public bool IsPacked
        {
            get => 
                this.HasFlag(2);
            set => 
                this.SetFlag(2, value, true);
        }

        public bool OverwriteList
        {
            get => 
                this.HasFlag(8);
            set => 
                this.SetFlag(8, value, true);
        }

        public bool IsRequired
        {
            get => 
                this.HasFlag(4);
            set => 
                this.SetFlag(4, value, true);
        }

        public bool AsReference
        {
            get => 
                this.asReference;
            set
            {
                this.ThrowIfFrozen();
                this.asReference = value;
            }
        }

        public bool DynamicType
        {
            get => 
                this.dynamicType;
            set
            {
                this.ThrowIfFrozen();
                this.dynamicType = value;
            }
        }

        public string Name =>
            (Helpers.IsNullOrEmpty(this.name) ? this.member.Name : this.name);

        public bool SupportNull
        {
            get => 
                this.HasFlag(0x10);
            set => 
                this.SetFlag(0x10, value, true);
        }

        internal class Comparer : IComparer, IComparer<ValueMember>
        {
            public static readonly ValueMember.Comparer Default = new ValueMember.Comparer();

            public int Compare(ValueMember x, ValueMember y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                return x.FieldNumber.CompareTo(y.FieldNumber);
            }

            public int Compare(object x, object y) => 
                this.Compare(x as ValueMember, y as ValueMember);
        }
    }
}


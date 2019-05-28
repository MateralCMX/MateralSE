namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using ProtoBuf.Serializers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MetaType : ISerializerProxy
    {
        private MetaType baseType;
        private BasicList subTypes;
        internal static readonly System.Type ienumerable = typeof(IEnumerable);
        private CallbackSet callbacks;
        private string name;
        private MethodInfo factory;
        private readonly RuntimeTypeModel model;
        private readonly System.Type type;
        private IProtoTypeSerializer serializer;
        private System.Type constructType;
        private System.Type surrogate;
        private readonly BasicList fields;
        private const byte OPTIONS_Pending = 1;
        private const byte OPTIONS_EnumPassThru = 2;
        private const byte OPTIONS_Frozen = 4;
        private const byte OPTIONS_PrivateOnApi = 8;
        private const byte OPTIONS_SkipConstructor = 0x10;
        private const byte OPTIONS_AsReferenceDefault = 0x20;
        private const byte OPTIONS_AutoTuple = 0x40;
        private const byte OPTIONS_IgnoreListHandling = 0x80;
        private volatile byte flags;

        internal MetaType(RuntimeTypeModel model, System.Type type, MethodInfo factory)
        {
            WireType type2;
            this.fields = new BasicList();
            this.factory = factory;
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (ValueMember.TryGetCoreSerializer(model, DataFormat.Default, type, out type2, false, false, false, false) != null)
            {
                throw new ArgumentException("Data of this type has inbuilt behaviour, and cannot be added to a model in this way: " + type.FullName);
            }
            this.type = type;
            this.model = model;
            if (Helpers.IsEnum(type))
            {
                this.EnumPassthru = type.IsDefined(model.MapType(typeof(FlagsAttribute)), false);
            }
        }

        public MetaType Add(params string[] memberNames)
        {
            int nextFieldNumber = this.GetNextFieldNumber();
            for (int i = 0; i < memberNames.Length; i++)
            {
                nextFieldNumber++;
                this.Add(nextFieldNumber, memberNames[i]);
            }
            return this;
        }

        private void Add(ValueMember member)
        {
            int opaqueToken = 0;
            try
            {
                this.model.TakeLock(ref opaqueToken);
                this.ThrowIfFrozen();
                this.fields.Add(member);
            }
            finally
            {
                this.model.ReleaseLock(opaqueToken);
            }
        }

        public MetaType Add(string memberName)
        {
            this.Add(this.GetNextFieldNumber(), memberName);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName)
        {
            this.AddField(fieldNumber, memberName, null, null, null);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName, object defaultValue)
        {
            this.AddField(fieldNumber, memberName, null, null, defaultValue);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType)
        {
            this.AddField(fieldNumber, memberName, itemType, defaultType, null);
            return this;
        }

        public ValueMember AddField(int fieldNumber, string memberName) => 
            this.AddField(fieldNumber, memberName, null, null, null);

        public ValueMember AddField(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType) => 
            this.AddField(fieldNumber, memberName, itemType, defaultType, null);

        private ValueMember AddField(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType, object defaultValue)
        {
            MemberInfo info = null;
            System.Type fieldType;
            MemberInfo[] infoArray = this.type.GetMember(memberName, Helpers.IsEnum(this.type) ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            if ((infoArray != null) && (infoArray.Length == 1))
            {
                info = infoArray[0];
            }
            if (info == null)
            {
                throw new ArgumentException("Unable to determine member: " + memberName, "memberName");
            }
            MemberTypes memberType = info.MemberType;
            if (memberType == MemberTypes.Field)
            {
                fieldType = ((FieldInfo) info).FieldType;
            }
            else
            {
                if (memberType != MemberTypes.Property)
                {
                    throw new NotSupportedException(info.MemberType.ToString());
                }
                fieldType = ((PropertyInfo) info).PropertyType;
            }
            ResolveListTypes(this.model, fieldType, ref itemType, ref defaultType);
            ValueMember member = new ValueMember(this.model, this.type, fieldNumber, info, fieldType, itemType, defaultType, DataFormat.Default, defaultValue);
            this.Add(member);
            return member;
        }

        public MetaType AddSubType(int fieldNumber, System.Type derivedType) => 
            this.AddSubType(fieldNumber, derivedType, DataFormat.Default);

        public MetaType AddSubType(int fieldNumber, System.Type derivedType, DataFormat dataFormat)
        {
            if (derivedType == null)
            {
                throw new ArgumentNullException("derivedType");
            }
            if (fieldNumber < 1)
            {
                throw new ArgumentOutOfRangeException("fieldNumber");
            }
            if ((!this.type.IsClass && !this.type.IsInterface) || this.type.IsSealed)
            {
                throw new InvalidOperationException("Sub-types can only be added to non-sealed classes");
            }
            if (!this.IsValidSubType(derivedType))
            {
                throw new ArgumentException(derivedType.Name + " is not a valid sub-type of " + this.type.Name, "derivedType");
            }
            MetaType type = this.model[derivedType];
            this.ThrowIfFrozen();
            type.ThrowIfFrozen();
            SubType type2 = new SubType(fieldNumber, type, dataFormat);
            this.ThrowIfFrozen();
            type.SetBaseType(this);
            if (this.subTypes == null)
            {
                this.subTypes = new BasicList();
            }
            this.subTypes.Add(type2);
            return this;
        }

        internal void ApplyDefaultBehaviour()
        {
            System.Type baseType = GetBaseType(this);
            if (((baseType != null) && (this.model.FindWithoutAdd(baseType) == null)) && (GetContractFamily(this.model, baseType, null) != AttributeFamily.None))
            {
                this.model.FindOrAddAuto(baseType, true, false, false);
            }
            AttributeMap[] attributes = AttributeMap.Create(this.model, this.type, false);
            AttributeFamily family = GetContractFamily(this.model, this.type, attributes);
            if (family == AttributeFamily.AutoTuple)
            {
                this.SetFlag(0x40, true, true);
            }
            bool isEnum = !this.EnumPassthru && Helpers.IsEnum(this.type);
            if ((family != AttributeFamily.None) || isEnum)
            {
                BasicList list = null;
                BasicList partialMembers = null;
                int dataMemberOffset = 0;
                int num2 = 1;
                bool inferTagFromNameDefault = this.model.InferTagFromNameDefault;
                ImplicitFields none = ImplicitFields.None;
                string str = null;
                for (int i = 0; i < attributes.Length; i++)
                {
                    object obj2;
                    AttributeMap map = attributes[i];
                    if (!isEnum && (map.AttributeType.FullName == "ProtoBuf.ProtoIncludeAttribute"))
                    {
                        int fieldNumber = 0;
                        if (map.TryGet("tag", out obj2))
                        {
                            fieldNumber = (int) obj2;
                        }
                        DataFormat dataFormat = DataFormat.Default;
                        if (map.TryGet("DataFormat", out obj2))
                        {
                            dataFormat = (DataFormat) ((int) obj2);
                        }
                        System.Type subType = null;
                        try
                        {
                            if (map.TryGet("knownTypeName", out obj2))
                            {
                                subType = this.model.GetType((string) obj2, this.type.Assembly);
                            }
                            else if (map.TryGet("knownType", out obj2))
                            {
                                subType = (System.Type) obj2;
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException("Unable to resolve sub-type of: " + this.type.FullName, exception);
                        }
                        if (subType == null)
                        {
                            throw new InvalidOperationException("Unable to resolve sub-type of: " + this.type.FullName);
                        }
                        if (this.IsValidSubType(subType))
                        {
                            this.AddSubType(fieldNumber, subType, dataFormat);
                        }
                    }
                    if (((map.AttributeType.FullName == "ProtoBuf.ProtoPartialIgnoreAttribute") && map.TryGet("MemberName", out obj2)) && (obj2 != null))
                    {
                        if (list == null)
                        {
                            list = new BasicList();
                        }
                        list.Add((string) obj2);
                    }
                    if (!isEnum && (map.AttributeType.FullName == "ProtoBuf.ProtoPartialMemberAttribute"))
                    {
                        if (partialMembers == null)
                        {
                            partialMembers = new BasicList();
                        }
                        partialMembers.Add(map);
                    }
                    if (map.AttributeType.FullName == "ProtoBuf.ProtoContractAttribute")
                    {
                        if (map.TryGet("Name", out obj2))
                        {
                            str = (string) obj2;
                        }
                        if (!isEnum)
                        {
                            if (map.TryGet("DataMemberOffset", out obj2))
                            {
                                dataMemberOffset = (int) obj2;
                            }
                            if ((map.TryGet("InferTagFromNameHasValue", false, out obj2) && ((bool) obj2)) && map.TryGet("InferTagFromName", out obj2))
                            {
                                inferTagFromNameDefault = (bool) obj2;
                            }
                            if (map.TryGet("ImplicitFields", out obj2) && (obj2 != null))
                            {
                                none = (ImplicitFields) ((int) obj2);
                            }
                            if (map.TryGet("SkipConstructor", out obj2))
                            {
                                this.UseConstructor = !((bool) obj2);
                            }
                            if (map.TryGet("IgnoreListHandling", out obj2))
                            {
                                this.IgnoreListHandling = (bool) obj2;
                            }
                            if (map.TryGet("AsReferenceDefault", out obj2))
                            {
                                this.AsReferenceDefault = (bool) obj2;
                            }
                            if (map.TryGet("ImplicitFirstTag", out obj2) && (((int) obj2) > 0))
                            {
                                num2 = (int) obj2;
                            }
                        }
                    }
                    if (((map.AttributeType.FullName == "System.Runtime.Serialization.DataContractAttribute") && (str == null)) && map.TryGet("Name", out obj2))
                    {
                        str = (string) obj2;
                    }
                    if (((map.AttributeType.FullName == "System.Xml.Serialization.XmlTypeAttribute") && (str == null)) && map.TryGet("TypeName", out obj2))
                    {
                        str = (string) obj2;
                    }
                }
                if (!Helpers.IsNullOrEmpty(str))
                {
                    this.Name = str;
                }
                if (none != ImplicitFields.None)
                {
                    family &= AttributeFamily.ProtoBuf;
                }
                MethodInfo[] callbacks = null;
                BasicList members = new BasicList();
                foreach (MemberInfo info in this.type.GetMembers(isEnum ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)))
                {
                    if (((info.DeclaringType == this.type) && !info.IsDefined(this.model.MapType(typeof(ProtoIgnoreAttribute)), true)) && ((list == null) || !list.Contains(info.Name)))
                    {
                        System.Type propertyType;
                        bool forced = false;
                        PropertyInfo property = info as PropertyInfo;
                        if (property != null)
                        {
                            if (!isEnum)
                            {
                                propertyType = property.PropertyType;
                                ApplyDefaultBehaviour_AddMembers(this.model, family, isEnum, partialMembers, dataMemberOffset, inferTagFromNameDefault, none, members, info, ref forced, Helpers.GetGetMethod(property, false, false) != null, false, ref propertyType);
                            }
                        }
                        else
                        {
                            FieldInfo info3 = info as FieldInfo;
                            if (info3 != null)
                            {
                                propertyType = info3.FieldType;
                                bool isPublic = info3.IsPublic;
                                bool isField = true;
                                if (!isEnum || info3.IsStatic)
                                {
                                    ApplyDefaultBehaviour_AddMembers(this.model, family, isEnum, partialMembers, dataMemberOffset, inferTagFromNameDefault, none, members, info, ref forced, isPublic, isField, ref propertyType);
                                }
                            }
                            else
                            {
                                MethodInfo info4;
                                if (((info4 = info as MethodInfo) != null) && !isEnum)
                                {
                                    AttributeMap[] mapArray2 = AttributeMap.Create(this.model, info4, false);
                                    if ((mapArray2 != null) && (mapArray2.Length != 0))
                                    {
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoBeforeSerializationAttribute", ref callbacks, 0);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoAfterSerializationAttribute", ref callbacks, 1);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoBeforeDeserializationAttribute", ref callbacks, 2);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoAfterDeserializationAttribute", ref callbacks, 3);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnSerializingAttribute", ref callbacks, 4);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnSerializedAttribute", ref callbacks, 5);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnDeserializingAttribute", ref callbacks, 6);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnDeserializedAttribute", ref callbacks, 7);
                                    }
                                }
                            }
                        }
                    }
                }
                ProtoMemberAttribute[] array = new ProtoMemberAttribute[members.Count];
                members.CopyTo(array, 0);
                if (inferTagFromNameDefault || (none != ImplicitFields.None))
                {
                    Array.Sort<ProtoMemberAttribute>(array);
                    int tag = num2;
                    foreach (ProtoMemberAttribute attribute in array)
                    {
                        if (!attribute.TagIsPinned)
                        {
                            tag++;
                            attribute.Rebase(tag);
                        }
                    }
                }
                foreach (ProtoMemberAttribute attribute2 in array)
                {
                    ValueMember member = this.ApplyDefaultBehaviour(isEnum, attribute2);
                    if (member != null)
                    {
                        this.Add(member);
                    }
                }
                if (callbacks != null)
                {
                    this.SetCallbacks(Coalesce(callbacks, 0, 4), Coalesce(callbacks, 1, 5), Coalesce(callbacks, 2, 6), Coalesce(callbacks, 3, 7));
                }
            }
        }

        private ValueMember ApplyDefaultBehaviour(bool isEnum, ProtoMemberAttribute normalizedAttribute)
        {
            MemberInfo info;
            AttributeMap map;
            object obj3;
            ValueMember member1;
            if ((normalizedAttribute == null) || ((info = normalizedAttribute.Member) == null))
            {
                return null;
            }
            System.Type memberType = Helpers.GetMemberType(info);
            System.Type itemType = null;
            System.Type defaultType = null;
            ResolveListTypes(this.model, memberType, ref itemType, ref defaultType);
            if (((itemType != null) && (this.model.FindOrAddAuto(memberType, false, true, false) >= 0)) && this.model[memberType].IgnoreListHandling)
            {
                itemType = null;
                defaultType = null;
            }
            AttributeMap[] attribs = AttributeMap.Create(this.model, info, true);
            object defaultValue = null;
            if (this.model.UseImplicitZeroDefaults)
            {
                ProtoTypeCode typeCode = Helpers.GetTypeCode(memberType);
                switch (typeCode)
                {
                    case ProtoTypeCode.Boolean:
                        defaultValue = false;
                        break;

                    case ProtoTypeCode.Char:
                        defaultValue = '\0';
                        break;

                    case ProtoTypeCode.SByte:
                        defaultValue = (sbyte) 0;
                        break;

                    case ProtoTypeCode.Byte:
                        defaultValue = (byte) 0;
                        break;

                    case ProtoTypeCode.Int16:
                        defaultValue = (short) 0;
                        break;

                    case ProtoTypeCode.UInt16:
                        defaultValue = (ushort) 0;
                        break;

                    case ProtoTypeCode.Int32:
                        defaultValue = 0;
                        break;

                    case ProtoTypeCode.UInt32:
                        defaultValue = 0;
                        break;

                    case ProtoTypeCode.Int64:
                        defaultValue = 0L;
                        break;

                    case ProtoTypeCode.UInt64:
                        defaultValue = (ulong) 0L;
                        break;

                    case ProtoTypeCode.Single:
                        defaultValue = 0f;
                        break;

                    case ProtoTypeCode.Double:
                        defaultValue = 0.0;
                        break;

                    case ProtoTypeCode.Decimal:
                        defaultValue = 0M;
                        break;

                    default:
                        if (typeCode == ProtoTypeCode.TimeSpan)
                        {
                            defaultValue = TimeSpan.Zero;
                        }
                        else if (typeCode == ProtoTypeCode.Guid)
                        {
                            defaultValue = Guid.Empty;
                        }
                        break;
                }
            }
            if (((map = GetAttribute(attribs, "System.ComponentModel.DefaultValueAttribute")) != null) && map.TryGet("Value", out obj3))
            {
                defaultValue = obj3;
            }
            if (isEnum || (normalizedAttribute.Tag > 0))
            {
                member1 = new ValueMember(this.model, this.type, normalizedAttribute.Tag, info, memberType, itemType, defaultType, normalizedAttribute.DataFormat, defaultValue);
            }
            else
            {
                member1 = null;
            }
            ValueMember member = member1;
            if (member != null)
            {
                System.Type type = this.type;
                PropertyInfo property = Helpers.GetProperty(type, info.Name + "Specified", true);
                MethodInfo getSpecified = Helpers.GetGetMethod(property, true, true);
                if ((getSpecified == null) || getSpecified.IsStatic)
                {
                    property = null;
                }
                if (property != null)
                {
                    member.SetSpecified(getSpecified, Helpers.GetSetMethod(property, true, true));
                }
                else
                {
                    MethodInfo info4 = Helpers.GetInstanceMethod(type, "ShouldSerialize" + info.Name, Helpers.EmptyTypes);
                    if ((info4 != null) && (info4.ReturnType == this.model.MapType(typeof(bool))))
                    {
                        member.SetSpecified(info4, null);
                    }
                }
                if (!Helpers.IsNullOrEmpty(normalizedAttribute.Name))
                {
                    member.SetName(normalizedAttribute.Name);
                }
                member.IsPacked = normalizedAttribute.IsPacked;
                member.IsRequired = normalizedAttribute.IsRequired;
                member.OverwriteList = normalizedAttribute.OverwriteList;
                if (normalizedAttribute.AsReferenceHasValue)
                {
                    member.AsReference = normalizedAttribute.AsReference;
                }
                member.DynamicType = normalizedAttribute.DynamicType;
            }
            return member;
        }

        private static void ApplyDefaultBehaviour_AddMembers(TypeModel model, AttributeFamily family, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferTagByName, ImplicitFields implicitMode, BasicList members, MemberInfo member, ref bool forced, bool isPublic, bool isField, ref System.Type effectiveType)
        {
            if (implicitMode != ImplicitFields.AllPublic)
            {
                if ((implicitMode == ImplicitFields.AllFields) && isField)
                {
                    forced = true;
                }
            }
            else if (isPublic)
            {
                forced = true;
            }
            if (effectiveType.IsSubclassOf(model.MapType(typeof(Delegate))))
            {
                effectiveType = null;
            }
            if (effectiveType != null)
            {
                ProtoMemberAttribute attribute = NormalizeProtoMember(model, member, family, forced, isEnum, partialMembers, dataMemberOffset, inferTagByName);
                if (attribute != null)
                {
                    members.Add(attribute);
                }
            }
        }

        private IProtoTypeSerializer BuildSerializer()
        {
            if (Helpers.IsEnum(this.type))
            {
                return new TagDecorator(1, WireType.Variant, false, new EnumSerializer(this.type, this.GetEnumMap()));
            }
            System.Type itemType = this.IgnoreListHandling ? null : TypeModel.GetListItemType(this.model, this.type);
            if (itemType != null)
            {
                if (this.surrogate != null)
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot use a surrogate");
                }
                if ((this.subTypes != null) && (this.subTypes.Count != 0))
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be subclassed");
                }
                ValueMember member = new ValueMember(this.model, 1, this.type, itemType, this.type, DataFormat.Default);
                int[] numArray1 = new int[] { 1 };
                IProtoSerializer[] serializerArray1 = new IProtoSerializer[] { member.Serializer };
                return new TypeSerializer(this.model, this.type, numArray1, serializerArray1, null, true, true, null, this.constructType, this.factory);
            }
            if (this.surrogate != null)
            {
                MetaType type4;
                MetaType type3 = this.model[this.surrogate];
                while ((type4 = type3.baseType) != null)
                {
                    type3 = type4;
                }
                return new SurrogateSerializer(this.type, this.surrogate, type3.Serializer);
            }
            if (this.IsAutoTuple)
            {
                MemberInfo[] infoArray2;
                ConstructorInfo ctor = ResolveTupleConstructor(this.type, out infoArray2);
                if (ctor == null)
                {
                    throw new InvalidOperationException();
                }
                return new TupleSerializer(this.model, ctor, infoArray2);
            }
            this.fields.Trim();
            int count = this.fields.Count;
            int num2 = (this.subTypes == null) ? 0 : this.subTypes.Count;
            int[] fieldNumbers = new int[count + num2];
            IProtoSerializer[] serializers = new IProtoSerializer[count + num2];
            int index = 0;
            if (num2 != 0)
            {
                foreach (SubType type5 in this.subTypes)
                {
                    if (!type5.DerivedType.IgnoreListHandling && this.model.MapType(ienumerable).IsAssignableFrom(type5.DerivedType.Type))
                    {
                        throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a subclass");
                    }
                    fieldNumbers[index] = type5.FieldNumber;
                    index++;
                    serializers[index] = type5.Serializer;
                }
            }
            if (count != 0)
            {
                foreach (ValueMember member2 in this.fields)
                {
                    fieldNumbers[index] = member2.FieldNumber;
                    index++;
                    serializers[index] = member2.Serializer;
                }
            }
            BasicList list = null;
            for (MetaType type2 = this.BaseType; type2 != null; type2 = type2.BaseType)
            {
                MethodInfo info2 = type2.HasCallbacks ? type2.Callbacks.BeforeDeserialize : null;
                if (info2 != null)
                {
                    if (list == null)
                    {
                        list = new BasicList();
                    }
                    list.Add(info2);
                }
            }
            MethodInfo[] array = null;
            if (list != null)
            {
                array = new MethodInfo[list.Count];
                list.CopyTo(array, 0);
                Array.Reverse(array);
            }
            return new TypeSerializer(this.model, this.type, fieldNumbers, serializers, array, ReferenceEquals(this.baseType, null), this.UseConstructor, this.callbacks, this.constructType, this.factory);
        }

        private static void CheckForCallback(MethodInfo method, AttributeMap[] attributes, string callbackTypeName, ref MethodInfo[] callbacks, int index)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].AttributeType.FullName == callbackTypeName)
                {
                    if (callbacks == null)
                    {
                        callbacks = new MethodInfo[8];
                    }
                    else if (callbacks[index] != null)
                    {
                        System.Type reflectedType = method.ReflectedType;
                        throw new ProtoException("Duplicate " + callbackTypeName + " callbacks on " + reflectedType.FullName);
                    }
                    callbacks[index] = method;
                }
            }
        }

        private static MethodInfo Coalesce(MethodInfo[] arr, int x, int y)
        {
            MethodInfo info = arr[x];
            if (info == null)
            {
                info = arr[y];
            }
            return info;
        }

        public void CompileInPlace()
        {
            this.serializer = CompiledSerializer.Wrap(this.Serializer, this.model);
        }

        internal void Freeze()
        {
            this.flags = (byte) (this.flags | 4);
        }

        private static AttributeMap GetAttribute(AttributeMap[] attribs, string fullName)
        {
            for (int i = 0; i < attribs.Length; i++)
            {
                AttributeMap map = attribs[i];
                if ((map != null) && (map.AttributeType.FullName == fullName))
                {
                    return map;
                }
            }
            return null;
        }

        private static System.Type GetBaseType(MetaType type) => 
            type.type.BaseType;

        internal static AttributeFamily GetContractFamily(RuntimeTypeModel model, System.Type type, AttributeMap[] attributes)
        {
            MemberInfo[] infoArray;
            AttributeFamily none = AttributeFamily.None;
            if (attributes == null)
            {
                attributes = AttributeMap.Create(model, type, false);
            }
            for (int i = 0; i < attributes.Length; i++)
            {
                string fullName = attributes[i].AttributeType.FullName;
                if (fullName == "ProtoBuf.ProtoContractAttribute")
                {
                    bool flag = false;
                    GetFieldBoolean(ref flag, attributes[i], "UseProtoMembersOnly");
                    if (flag)
                    {
                        return AttributeFamily.ProtoBuf;
                    }
                    none |= AttributeFamily.ProtoBuf;
                }
                else if (fullName != "System.Xml.Serialization.XmlTypeAttribute")
                {
                    if ((fullName == "System.Runtime.Serialization.DataContractAttribute") && !model.AutoAddProtoContractTypesOnly)
                    {
                        none |= AttributeFamily.DataContractSerialier;
                    }
                }
                else if (!model.AutoAddProtoContractTypesOnly)
                {
                    none |= AttributeFamily.XmlSerializer;
                }
            }
            if ((none == AttributeFamily.None) && (ResolveTupleConstructor(type, out infoArray) != null))
            {
                none |= AttributeFamily.AutoTuple;
            }
            return none;
        }

        private static void GetDataFormat(ref DataFormat value, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && (value == DataFormat.Default)) && (attrib.TryGet(memberName, out obj2) && (obj2 != null)))
            {
                value = (DataFormat) obj2;
            }
        }

        internal EnumSerializer.EnumPair[] GetEnumMap()
        {
            if (this.HasFlag(2))
            {
                return null;
            }
            EnumSerializer.EnumPair[] pairArray = new EnumSerializer.EnumPair[this.fields.Count];
            for (int i = 0; i < pairArray.Length; i++)
            {
                ValueMember member = (ValueMember) this.fields[i];
                int fieldNumber = member.FieldNumber;
                object rawEnumValue = member.GetRawEnumValue();
                pairArray[i] = new EnumSerializer.EnumPair(fieldNumber, rawEnumValue, member.MemberType);
            }
            return pairArray;
        }

        private static void GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName)
        {
            GetFieldBoolean(ref value, attrib, memberName, true);
        }

        private static bool GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName, bool publicOnly)
        {
            if (attrib == null)
            {
                return false;
            }
            if (!value)
            {
                object obj2;
                if (!attrib.TryGet(memberName, publicOnly, out obj2) || (obj2 == null))
                {
                    return false;
                }
                value = (bool) obj2;
            }
            return true;
        }

        private static void GetFieldName(ref string name, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && Helpers.IsNullOrEmpty(name)) && (attrib.TryGet(memberName, out obj2) && (obj2 != null)))
            {
                name = (string) obj2;
            }
        }

        private static void GetFieldNumber(ref int value, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && (value <= 0)) && (attrib.TryGet(memberName, out obj2) && (obj2 != null)))
            {
                value = (int) obj2;
            }
        }

        public ValueMember[] GetFields()
        {
            ValueMember[] array = new ValueMember[this.fields.Count];
            this.fields.CopyTo(array, 0);
            Array.Sort<ValueMember>(array, ValueMember.Comparer.Default);
            return array;
        }

        private static void GetIgnore(ref bool ignore, AttributeMap attrib, AttributeMap[] attribs, string fullName)
        {
            if (!ignore && (attrib != null))
            {
                ignore = GetAttribute(attribs, fullName) != null;
            }
        }

        internal int GetKey(bool demand, bool getBaseKey) => 
            this.model.GetKey(this.type, demand, getBaseKey);

        private int GetNextFieldNumber()
        {
            int fieldNumber = 0;
            foreach (ValueMember member in this.fields)
            {
                if (member.FieldNumber > fieldNumber)
                {
                    fieldNumber = member.FieldNumber;
                }
            }
            if (this.subTypes != null)
            {
                foreach (SubType type in this.subTypes)
                {
                    if (type.FieldNumber > fieldNumber)
                    {
                        fieldNumber = type.FieldNumber;
                    }
                }
            }
            return (fieldNumber + 1);
        }

        internal static MetaType GetRootType(MetaType source)
        {
            MetaType type3;
            while (source.serializer != null)
            {
                MetaType baseType = source.baseType;
                if (baseType == null)
                {
                    return source;
                }
                source = baseType;
            }
            RuntimeTypeModel model = source.model;
            int opaqueToken = 0;
            try
            {
                model.TakeLock(ref opaqueToken);
                while (true)
                {
                    MetaType baseType = source.baseType;
                    if (baseType == null)
                    {
                        type3 = source;
                        break;
                    }
                    source = baseType;
                }
            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
            return type3;
        }

        internal string GetSchemaTypeName()
        {
            if (this.surrogate != null)
            {
                return this.model[this.surrogate].GetSchemaTypeName();
            }
            if (!Helpers.IsNullOrEmpty(this.name))
            {
                return this.name;
            }
            if (!this.type.IsGenericType)
            {
                return this.type.Name;
            }
            StringBuilder builder = new StringBuilder(this.type.Name);
            int index = this.type.Name.IndexOf('`');
            if (index >= 0)
            {
                builder.Length = index;
            }
            foreach (System.Type type in this.type.GetGenericArguments())
            {
                MetaType type3;
                builder.Append('_');
                System.Type type2 = type;
                int key = this.model.GetKey(ref type2);
                if (((key < 0) || ((type3 = this.model[type2]) == null)) || (type3.surrogate != null))
                {
                    builder.Append(type2.Name);
                }
                else
                {
                    builder.Append(type3.GetSchemaTypeName());
                }
            }
            return builder.ToString();
        }

        public SubType[] GetSubtypes()
        {
            if ((this.subTypes == null) || (this.subTypes.Count == 0))
            {
                return new SubType[0];
            }
            SubType[] array = new SubType[this.subTypes.Count];
            this.subTypes.CopyTo(array, 0);
            Array.Sort<SubType>(array, SubType.Comparer.Default);
            return array;
        }

        internal MetaType GetSurrogateOrBaseOrSelf(bool deep)
        {
            if (this.surrogate != null)
            {
                return this.model[this.surrogate];
            }
            MetaType baseType = this.baseType;
            if (baseType == null)
            {
                return this;
            }
            if (!deep)
            {
                return baseType;
            }
            while (true)
            {
                MetaType type2 = baseType;
                baseType = baseType.baseType;
                if (baseType == null)
                {
                    return type2;
                }
            }
        }

        internal MetaType GetSurrogateOrSelf() => 
            ((this.surrogate == null) ? this : this.model[this.surrogate]);

        private static bool HasFamily(AttributeFamily value, AttributeFamily required) => 
            ((value & required) == required);

        private bool HasFlag(byte flag) => 
            ((this.flags & flag) == flag);

        internal bool IsDefined(int fieldNumber)
        {
            using (IEnumerator enumerator = this.fields.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ValueMember current = (ValueMember) enumerator.Current;
                    if (current.FieldNumber == fieldNumber)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool IsPrepared() => 
            (this.serializer is CompiledSerializer);

        private bool IsValidSubType(System.Type subType) => 
            this.type.IsAssignableFrom(subType);

        internal static StringBuilder NewLine(StringBuilder builder, int indent) => 
            Helpers.AppendLine(builder).Append(' ', indent * 3);

        private static ProtoMemberAttribute NormalizeProtoMember(TypeModel model, MemberInfo member, AttributeFamily family, bool forced, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferByTagName)
        {
            AttributeMap attribute;
            if ((member == null) || ((family == AttributeFamily.None) && !isEnum))
            {
                return null;
            }
            int num = -2147483648;
            int num2 = inferByTagName ? -1 : 1;
            string name = null;
            bool flag = false;
            bool ignore = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            DataFormat format = DataFormat.Default;
            if (isEnum)
            {
                forced = true;
            }
            AttributeMap[] attribs = AttributeMap.Create(model, member, true);
            if (isEnum)
            {
                if (GetAttribute(attribs, "ProtoBuf.ProtoIgnoreAttribute") != null)
                {
                    ignore = true;
                }
                else
                {
                    attribute = GetAttribute(attribs, "ProtoBuf.ProtoEnumAttribute");
                    num = Convert.ToInt32(((FieldInfo) member).GetRawConstantValue());
                    if (attribute != null)
                    {
                        object obj2;
                        GetFieldName(ref name, attribute, "Name");
                        if (((bool) Helpers.GetInstanceMethod(attribute.AttributeType, "HasValue").Invoke(attribute.Target, null)) && attribute.TryGet("Value", out obj2))
                        {
                            num = (int) obj2;
                        }
                    }
                }
                flag3 = true;
            }
            if (!ignore && !flag3)
            {
                attribute = GetAttribute(attribs, "ProtoBuf.ProtoMemberAttribute");
                GetIgnore(ref ignore, attribute, attribs, "ProtoBuf.ProtoIgnoreAttribute");
                if (!ignore && (attribute != null))
                {
                    GetFieldNumber(ref num, attribute, "Tag");
                    GetFieldName(ref name, attribute, "Name");
                    GetFieldBoolean(ref flag4, attribute, "IsRequired");
                    GetFieldBoolean(ref flag, attribute, "IsPacked");
                    GetFieldBoolean(ref flag9, attribute, "OverwriteList");
                    GetDataFormat(ref format, attribute, "DataFormat");
                    GetFieldBoolean(ref flag6, attribute, "AsReferenceHasValue", false);
                    if (flag6)
                    {
                        flag6 = GetFieldBoolean(ref flag5, attribute, "AsReference", true);
                    }
                    GetFieldBoolean(ref flag7, attribute, "DynamicType");
                    flag3 = flag8 = num > 0;
                }
                if (!flag3 && (partialMembers != null))
                {
                    foreach (AttributeMap map2 in partialMembers)
                    {
                        object obj3;
                        if (!map2.TryGet("MemberName", out obj3))
                        {
                            continue;
                        }
                        if (((string) obj3) == member.Name)
                        {
                            GetFieldNumber(ref num, map2, "Tag");
                            GetFieldName(ref name, map2, "Name");
                            GetFieldBoolean(ref flag4, map2, "IsRequired");
                            GetFieldBoolean(ref flag, map2, "IsPacked");
                            GetFieldBoolean(ref flag9, attribute, "OverwriteList");
                            GetDataFormat(ref format, map2, "DataFormat");
                            GetFieldBoolean(ref flag6, attribute, "AsReferenceHasValue", false);
                            if (flag6)
                            {
                                flag6 = GetFieldBoolean(ref flag5, map2, "AsReference", true);
                            }
                            GetFieldBoolean(ref flag7, map2, "DynamicType");
                            if (flag3 = flag8 = num > 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            if ((!ignore && !flag3) && HasFamily(family, AttributeFamily.DataContractSerialier))
            {
                attribute = GetAttribute(attribs, "System.Runtime.Serialization.DataMemberAttribute");
                if (attribute != null)
                {
                    GetFieldNumber(ref num, attribute, "Order");
                    GetFieldName(ref name, attribute, "Name");
                    GetFieldBoolean(ref flag4, attribute, "IsRequired");
                    flag3 = num >= num2;
                    if (flag3)
                    {
                        num += dataMemberOffset;
                    }
                }
            }
            if ((!ignore && !flag3) && HasFamily(family, AttributeFamily.XmlSerializer))
            {
                attribute = GetAttribute(attribs, "System.Xml.Serialization.XmlElementAttribute");
                if (attribute == null)
                {
                    attribute = GetAttribute(attribs, "System.Xml.Serialization.XmlArrayAttribute");
                }
                GetIgnore(ref ignore, attribute, attribs, "System.Xml.Serialization.XmlIgnoreAttribute");
                if ((attribute != null) && !ignore)
                {
                    GetFieldNumber(ref num, attribute, "Order");
                    GetFieldName(ref name, attribute, "ElementName");
                    flag3 = num >= num2;
                }
            }
            if ((!ignore && !flag3) && (GetAttribute(attribs, "System.NonSerializedAttribute") != null))
            {
                ignore = true;
            }
            if (ignore || ((num < num2) && !forced))
            {
                return null;
            }
            return new ProtoMemberAttribute(forced | inferByTagName, num) { 
                AsReference = flag5,
                AsReferenceHasValue = flag6,
                DataFormat = format,
                DynamicType = flag7,
                IsPacked = flag,
                OverwriteList = flag9,
                IsRequired = flag4,
                Name = Helpers.IsNullOrEmpty(name) ? member.Name : name,
                Member = member,
                TagIsPinned = flag8
            };
        }

        internal static void ResolveListTypes(TypeModel model, System.Type type, ref System.Type itemType, ref System.Type defaultType)
        {
            if (type != null)
            {
                if (type.IsArray)
                {
                    if (type.GetArrayRank() != 1)
                    {
                        throw new NotSupportedException("Multi-dimension arrays are supported");
                    }
                    itemType = type.GetElementType();
                    if (itemType != model.MapType(typeof(byte)))
                    {
                        defaultType = type;
                    }
                    else
                    {
                        System.Type type2;
                        itemType = (System.Type) (type2 = null);
                        defaultType = type2;
                    }
                }
                if (itemType == null)
                {
                    itemType = TypeModel.GetListItemType(model, type);
                }
                if (itemType != null)
                {
                    System.Type type3 = null;
                    System.Type type4 = null;
                    ResolveListTypes(model, itemType, ref type3, ref type4);
                    if (type3 != null)
                    {
                        throw TypeModel.CreateNestedListsNotSupported();
                    }
                }
                if ((itemType != null) && (defaultType == null))
                {
                    if ((type.IsClass && !type.IsAbstract) && (Helpers.GetConstructor(type, Helpers.EmptyTypes, true) != null))
                    {
                        defaultType = type;
                    }
                    if ((defaultType == null) && type.IsInterface)
                    {
                        System.Type[] typeArray;
                        if ((type.IsGenericType && (type.GetGenericTypeDefinition() == model.MapType(typeof(IDictionary<,>)))) && (itemType == model.MapType(typeof(KeyValuePair<,>)).MakeGenericType(typeArray = type.GetGenericArguments())))
                        {
                            defaultType = model.MapType(typeof(Dictionary<,>)).MakeGenericType(typeArray);
                        }
                        else
                        {
                            System.Type[] typeArguments = new System.Type[] { itemType };
                            defaultType = model.MapType(typeof(List<>)).MakeGenericType(typeArguments);
                        }
                    }
                    if ((defaultType != null) && !Helpers.IsAssignableFrom(type, defaultType))
                    {
                        defaultType = null;
                    }
                }
            }
        }

        private MethodInfo ResolveMethod(string name, bool instance) => 
            (!Helpers.IsNullOrEmpty(name) ? (instance ? Helpers.GetInstanceMethod(this.type, name) : Helpers.GetStaticMethod(this.type, name)) : null);

        internal static ConstructorInfo ResolveTupleConstructor(System.Type type, out MemberInfo[] mappedMembers)
        {
            mappedMembers = null;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsAbstract)
            {
                return null;
            }
            ConstructorInfo[] constructors = Helpers.GetConstructors(type, false);
            if ((constructors.Length == 0) || ((constructors.Length == 1) && (constructors[0].GetParameters().Length == 0)))
            {
                return null;
            }
            MemberInfo[] instanceFieldsAndProperties = Helpers.GetInstanceFieldsAndProperties(type, true);
            BasicList list = new BasicList();
            for (int i = 0; i < instanceFieldsAndProperties.Length; i++)
            {
                PropertyInfo property = instanceFieldsAndProperties[i] as PropertyInfo;
                if (property != null)
                {
                    if (!property.CanRead)
                    {
                        return null;
                    }
                    if (property.CanWrite && (Helpers.GetSetMethod(property, false, false) != null))
                    {
                        return null;
                    }
                    list.Add(property);
                }
                else
                {
                    FieldInfo info3 = instanceFieldsAndProperties[i] as FieldInfo;
                    if (info3 != null)
                    {
                        if (!info3.IsInitOnly)
                        {
                            return null;
                        }
                        list.Add(info3);
                    }
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            MemberInfo[] array = new MemberInfo[list.Count];
            list.CopyTo(array, 0);
            int[] numArray = new int[array.Length];
            int num = 0;
            ConstructorInfo info = null;
            mappedMembers = new MemberInfo[numArray.Length];
            for (int j = 0; j < constructors.Length; j++)
            {
                ParameterInfo[] parameters = constructors[j].GetParameters();
                if (parameters.Length == array.Length)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= numArray.Length)
                        {
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 >= parameters.Length)
                                {
                                    bool flag = false;
                                    int num7 = 0;
                                    while (true)
                                    {
                                        if (num7 < numArray.Length)
                                        {
                                            if (numArray[num7] >= 0)
                                            {
                                                mappedMembers[num7] = array[numArray[num7]];
                                                num7++;
                                                continue;
                                            }
                                            flag = true;
                                        }
                                        if (!flag)
                                        {
                                            num++;
                                            info = constructors[j];
                                        }
                                        break;
                                    }
                                    break;
                                }
                                string str = parameters[num5].Name.ToLower();
                                int num6 = 0;
                                while (true)
                                {
                                    if (num6 >= array.Length)
                                    {
                                        num5++;
                                        break;
                                    }
                                    if ((array[num6].Name.ToLower() == str) && (Helpers.GetMemberType(array[num6]) == parameters[num5].ParameterType))
                                    {
                                        numArray[num5] = num6;
                                    }
                                    num6++;
                                }
                            }
                            break;
                        }
                        numArray[index] = -1;
                        index++;
                    }
                }
            }
            return ((num == 1) ? info : null);
        }

        private void SetBaseType(MetaType baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException("baseType");
            }
            if (!ReferenceEquals(this.baseType, baseType))
            {
                if (this.baseType != null)
                {
                    throw new InvalidOperationException("A type can only participate in one inheritance hierarchy");
                }
                for (MetaType type = baseType; type != null; type = type.baseType)
                {
                    if (ReferenceEquals(type, this))
                    {
                        throw new InvalidOperationException("Cyclic inheritance is not allowed");
                    }
                }
                this.baseType = baseType;
            }
        }

        public MetaType SetCallbacks(MethodInfo beforeSerialize, MethodInfo afterSerialize, MethodInfo beforeDeserialize, MethodInfo afterDeserialize)
        {
            CallbackSet callbacks = this.Callbacks;
            callbacks.BeforeSerialize = beforeSerialize;
            callbacks.AfterSerialize = afterSerialize;
            callbacks.BeforeDeserialize = beforeDeserialize;
            callbacks.AfterDeserialize = afterDeserialize;
            return this;
        }

        public MetaType SetCallbacks(string beforeSerialize, string afterSerialize, string beforeDeserialize, string afterDeserialize)
        {
            if (this.IsValueType)
            {
                throw new InvalidOperationException();
            }
            CallbackSet callbacks = this.Callbacks;
            callbacks.BeforeSerialize = this.ResolveMethod(beforeSerialize, true);
            callbacks.AfterSerialize = this.ResolveMethod(afterSerialize, true);
            callbacks.BeforeDeserialize = this.ResolveMethod(beforeDeserialize, true);
            callbacks.AfterDeserialize = this.ResolveMethod(afterDeserialize, true);
            return this;
        }

        public MetaType SetFactory(MethodInfo factory)
        {
            this.model.VerifyFactory(factory, this.type);
            this.ThrowIfFrozen();
            this.factory = factory;
            return this;
        }

        public MetaType SetFactory(string factory) => 
            this.SetFactory(this.ResolveMethod(factory, false));

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

        public void SetSurrogate(System.Type surrogateType)
        {
            if (surrogateType == this.type)
            {
                surrogateType = null;
            }
            if (((surrogateType != null) && (surrogateType != null)) && Helpers.IsAssignableFrom(this.model.MapType(typeof(IEnumerable)), surrogateType))
            {
                throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a surrogate");
            }
            this.ThrowIfFrozen();
            this.surrogate = surrogateType;
        }

        protected internal void ThrowIfFrozen()
        {
            if ((this.flags & 4) != 0)
            {
                throw new InvalidOperationException("The type cannot be changed once a serializer has been generated for " + this.type.FullName);
            }
        }

        public override string ToString() => 
            this.type.ToString();

        internal void WriteSchema(StringBuilder builder, int indent, ref bool requiresBclImport)
        {
            if (this.surrogate == null)
            {
                ValueMember[] array = new ValueMember[this.fields.Count];
                this.fields.CopyTo(array, 0);
                Array.Sort<ValueMember>(array, ValueMember.Comparer.Default);
                if (this.IsList)
                {
                    string str = this.model.GetSchemaTypeName(TypeModel.GetListItemType(this.model, this.type), DataFormat.Default, false, false, ref requiresBclImport);
                    NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                    NewLine(builder, indent + 1).Append("repeated ").Append(str).Append(" items = 1;");
                    NewLine(builder, indent).Append('}');
                }
                else if (this.IsAutoTuple)
                {
                    MemberInfo[] infoArray;
                    if (ResolveTupleConstructor(this.type, out infoArray) != null)
                    {
                        NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                        for (int i = 0; i < infoArray.Length; i++)
                        {
                            System.Type propertyType;
                            if (infoArray[i] is PropertyInfo)
                            {
                                propertyType = ((PropertyInfo) infoArray[i]).PropertyType;
                            }
                            else
                            {
                                if (!(infoArray[i] is FieldInfo))
                                {
                                    throw new NotSupportedException("Unknown member type: " + infoArray[i].GetType().Name);
                                }
                                propertyType = ((FieldInfo) infoArray[i]).FieldType;
                            }
                            NewLine(builder, indent + 1).Append("optional ").Append(this.model.GetSchemaTypeName(propertyType, DataFormat.Default, false, false, ref requiresBclImport).Replace('.', '_')).Append(' ').Append(infoArray[i].Name).Append(" = ").Append((int) (i + 1)).Append(';');
                        }
                        NewLine(builder, indent).Append('}');
                    }
                }
                else if (Helpers.IsEnum(this.type))
                {
                    NewLine(builder, indent).Append("enum ").Append(this.GetSchemaTypeName()).Append(" {");
                    if ((array.Length != 0) || !this.EnumPassthru)
                    {
                        foreach (ValueMember member in array)
                        {
                            NewLine(builder, indent + 1).Append(member.Name).Append(" = ").Append(member.FieldNumber).Append(';');
                        }
                    }
                    else
                    {
                        if (this.type.IsDefined(this.model.MapType(typeof(FlagsAttribute)), false))
                        {
                            NewLine(builder, indent + 1).Append("// this is a composite/flags enumeration");
                        }
                        else
                        {
                            NewLine(builder, indent + 1).Append("// this enumeration will be passed as a raw value");
                        }
                        foreach (FieldInfo info in this.type.GetFields())
                        {
                            if (info.IsStatic && info.IsLiteral)
                            {
                                object rawConstantValue = info.GetRawConstantValue();
                                NewLine(builder, indent + 1).Append(info.Name).Append(" = ").Append(rawConstantValue).Append(";");
                            }
                        }
                    }
                    NewLine(builder, indent).Append('}');
                }
                else
                {
                    NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                    ValueMember[] memberArray3 = array;
                    int index = 0;
                    while (true)
                    {
                        if (index >= memberArray3.Length)
                        {
                            if ((this.subTypes != null) && (this.subTypes.Count != 0))
                            {
                                NewLine(builder, indent + 1).Append("// the following represent sub-types; at most 1 should have a value");
                                SubType[] typeArray = new SubType[this.subTypes.Count];
                                this.subTypes.CopyTo(typeArray, 0);
                                Array.Sort<SubType>(typeArray, SubType.Comparer.Default);
                                foreach (SubType type2 in typeArray)
                                {
                                    string str4 = type2.DerivedType.GetSchemaTypeName();
                                    NewLine(builder, indent + 1).Append("optional ").Append(str4).Append(" ").Append(str4).Append(" = ").Append(type2.FieldNumber).Append(';');
                                }
                            }
                            NewLine(builder, indent).Append('}');
                            break;
                        }
                        ValueMember member2 = memberArray3[index];
                        string str2 = (member2.ItemType != null) ? "repeated" : (member2.IsRequired ? "required" : "optional");
                        NewLine(builder, indent + 1).Append(str2).Append(' ');
                        if (member2.DataFormat == DataFormat.Group)
                        {
                            builder.Append("group ");
                        }
                        string schemaTypeName = member2.GetSchemaTypeName(true, ref requiresBclImport);
                        builder.Append(schemaTypeName).Append(" ").Append(member2.Name).Append(" = ").Append(member2.FieldNumber);
                        if (member2.DefaultValue != null)
                        {
                            if (member2.DefaultValue is string)
                            {
                                builder.Append(" [default = \"").Append(member2.DefaultValue).Append("\"]");
                            }
                            else if (member2.DefaultValue as bool)
                            {
                                builder.Append(((bool) member2.DefaultValue) ? " [default = true]" : " [default = false]");
                            }
                            else
                            {
                                builder.Append(" [default = ").Append(member2.DefaultValue).Append(']');
                            }
                        }
                        if ((member2.ItemType != null) && member2.IsPacked)
                        {
                            builder.Append(" [packed=true]");
                        }
                        builder.Append(';');
                        if (((schemaTypeName == "bcl.NetObjectProxy") && member2.AsReference) && !member2.DynamicType)
                        {
                            builder.Append(" // reference-tracked ").Append(member2.GetSchemaTypeName(false, ref requiresBclImport));
                        }
                        index++;
                    }
                }
            }
        }

        IProtoSerializer ISerializerProxy.Serializer =>
            this.Serializer;

        public MetaType BaseType =>
            this.baseType;

        internal TypeModel Model =>
            this.model;

        public bool IncludeSerializerMethod
        {
            get => 
                !this.HasFlag(8);
            set => 
                this.SetFlag(8, !value, true);
        }

        public bool AsReferenceDefault
        {
            get => 
                this.HasFlag(0x20);
            set => 
                this.SetFlag(0x20, value, true);
        }

        public bool HasCallbacks =>
            ((this.callbacks != null) && this.callbacks.NonTrivial);

        public bool HasSubtypes =>
            ((this.subTypes != null) && (this.subTypes.Count != 0));

        public CallbackSet Callbacks
        {
            get
            {
                if (this.callbacks == null)
                {
                    this.callbacks = new CallbackSet(this);
                }
                return this.callbacks;
            }
        }

        private bool IsValueType =>
            this.type.IsValueType;

        public string Name
        {
            get => 
                this.name;
            set
            {
                this.ThrowIfFrozen();
                this.name = value;
            }
        }

        public System.Type Type =>
            this.type;

        internal IProtoTypeSerializer Serializer
        {
            get
            {
                if (this.serializer == null)
                {
                    int opaqueToken = 0;
                    try
                    {
                        this.model.TakeLock(ref opaqueToken);
                        if (this.serializer == null)
                        {
                            this.SetFlag(4, true, false);
                            this.serializer = this.BuildSerializer();
                            if (this.model.AutoCompile)
                            {
                                this.CompileInPlace();
                            }
                        }
                    }
                    finally
                    {
                        this.model.ReleaseLock(opaqueToken);
                    }
                }
                return this.serializer;
            }
        }

        internal bool IsList =>
            ((this.IgnoreListHandling ? null : TypeModel.GetListItemType(this.model, this.type)) != null);

        public bool UseConstructor
        {
            get => 
                !this.HasFlag(0x10);
            set => 
                this.SetFlag(0x10, !value, true);
        }

        public System.Type ConstructType
        {
            get => 
                this.constructType;
            set
            {
                this.ThrowIfFrozen();
                this.constructType = value;
            }
        }

        public ValueMember this[int fieldNumber]
        {
            get
            {
                using (IEnumerator enumerator = this.fields.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        ValueMember current = (ValueMember) enumerator.Current;
                        if (current.FieldNumber == fieldNumber)
                        {
                            return current;
                        }
                    }
                }
                return null;
            }
        }

        public ValueMember this[MemberInfo member]
        {
            get
            {
                if (member != null)
                {
                    using (IEnumerator enumerator = this.fields.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            ValueMember current = (ValueMember) enumerator.Current;
                            if (current.Member == member)
                            {
                                return current;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public bool EnumPassthru
        {
            get => 
                this.HasFlag(2);
            set => 
                this.SetFlag(2, value, true);
        }

        public bool IgnoreListHandling
        {
            get => 
                this.HasFlag(0x80);
            set => 
                this.SetFlag(0x80, value, true);
        }

        internal bool Pending
        {
            get => 
                this.HasFlag(1);
            set => 
                this.SetFlag(1, value, false);
        }

        internal IEnumerable Fields =>
            this.fields;

        internal bool IsAutoTuple =>
            this.HasFlag(0x40);

        [Flags]
        internal enum AttributeFamily
        {
            None = 0,
            ProtoBuf = 1,
            DataContractSerialier = 2,
            XmlSerializer = 4,
            AutoTuple = 8
        }

        internal class Comparer : IComparer, IComparer<MetaType>
        {
            public static readonly MetaType.Comparer Default = new MetaType.Comparer();

            public int Compare(MetaType x, MetaType y) => 
                (!ReferenceEquals(x, y) ? ((x != null) ? ((y != null) ? string.Compare(x.GetSchemaTypeName(), y.GetSchemaTypeName(), StringComparison.Ordinal) : 1) : -1) : 0);

            public int Compare(object x, object y) => 
                this.Compare(x as MetaType, y as MetaType);
        }
    }
}


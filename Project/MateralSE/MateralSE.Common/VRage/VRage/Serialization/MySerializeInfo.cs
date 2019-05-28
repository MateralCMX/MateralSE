namespace VRage.Serialization
{
    using System;
    using System.Reflection;
    using System.Text;
    using VRage.Library.Collections;

    public class MySerializeInfo
    {
        public static readonly MySerializeInfo Default = new MySerializeInfo();
        public readonly MyObjectFlags Flags;
        public readonly MyPrimitiveFlags PrimitiveFlags;
        public readonly ushort FixedLength;
        public readonly DynamicSerializerDelegate DynamicSerializer;
        public readonly MySerializeInfo KeyInfo;
        public readonly MySerializeInfo ItemInfo;

        private MySerializeInfo()
        {
        }

        public MySerializeInfo(SerializeAttribute attribute, MySerializeInfo keyInfo, MySerializeInfo itemInfo)
        {
            if (attribute != null)
            {
                this.Flags = attribute.Flags;
                this.PrimitiveFlags = attribute.PrimitiveFlags;
                this.FixedLength = attribute.FixedLength;
                if (this.IsDynamic)
                {
                    IDynamicResolver resolver1 = (IDynamicResolver) Activator.CreateInstance(attribute.DynamicSerializerType);
                    this.DynamicSerializer = new DynamicSerializerDelegate(resolver1.Serialize);
                }
            }
            this.KeyInfo = keyInfo;
            this.ItemInfo = itemInfo;
        }

        public MySerializeInfo(MyObjectFlags flags, MyPrimitiveFlags primitiveFlags, ushort fixedLength, DynamicSerializerDelegate dynamicSerializer, MySerializeInfo keyInfo, MySerializeInfo itemInfo)
        {
            this.Flags = flags;
            this.PrimitiveFlags = primitiveFlags;
            this.FixedLength = fixedLength;
            this.KeyInfo = keyInfo;
            this.ItemInfo = itemInfo;
            this.DynamicSerializer = dynamicSerializer;
        }

        public static MySerializeInfo Create(ICustomAttributeProvider reflectionInfo)
        {
            SerializeAttribute first = new SerializeAttribute();
            SerializeAttribute attribute2 = null;
            SerializeAttribute attribute3 = null;
            foreach (SerializeAttribute attribute4 in reflectionInfo.GetCustomAttributes(typeof(SerializeAttribute), false))
            {
                if (attribute4.Kind == MySerializeKind.Default)
                {
                    first = Merge(first, attribute4);
                }
                else if (attribute4.Kind == MySerializeKind.Key)
                {
                    attribute2 = Merge(attribute2, attribute4);
                }
                else if (attribute4.Kind == MySerializeKind.Item)
                {
                    attribute3 = Merge(attribute3, attribute4);
                }
            }
            return new MySerializeInfo(first, ToInfo(attribute2), ToInfo(attribute3));
        }

        public static MySerializeInfo CreateForParameter(ParameterInfo[] parameters, int index) => 
            ((index < parameters.Length) ? Create(parameters[index]) : Default);

        private static SerializeAttribute Merge(SerializeAttribute first, SerializeAttribute second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            SerializeAttribute attribute1 = new SerializeAttribute();
            attribute1.Flags = first.Flags | second.Flags;
            attribute1.PrimitiveFlags = first.PrimitiveFlags | second.PrimitiveFlags;
            attribute1.FixedLength = (first.FixedLength != 0) ? first.FixedLength : second.FixedLength;
            SerializeAttribute local2 = attribute1;
            SerializeAttribute local3 = attribute1;
            local3.DynamicSerializerType = first.DynamicSerializerType ?? second.DynamicSerializerType;
            return local3;
        }

        private static MySerializeInfo ToInfo(SerializeAttribute attr) => 
            ((attr != null) ? new MySerializeInfo(attr, null, null) : null);

        public bool IsNullable =>
            (((this.Flags & MyObjectFlags.DefaultZero) != MyObjectFlags.None) || this.IsNullOrEmpty);

        public bool IsDynamic =>
            (((this.Flags & MyObjectFlags.Dynamic) != MyObjectFlags.None) || this.IsDynamicDefault);

        public bool IsNullOrEmpty =>
            ((this.Flags & MyObjectFlags.DefaultValueOrEmpty) != MyObjectFlags.None);

        public bool IsDynamicDefault =>
            ((this.Flags & MyObjectFlags.DynamicDefault) != MyObjectFlags.None);

        public bool IsSigned =>
            ((this.PrimitiveFlags & MyPrimitiveFlags.Signed) != MyPrimitiveFlags.None);

        public bool IsNormalized =>
            ((this.PrimitiveFlags & MyPrimitiveFlags.Normalized) != MyPrimitiveFlags.None);

        public bool IsVariant =>
            (!this.IsSigned && ((this.PrimitiveFlags & MyPrimitiveFlags.Variant) != MyPrimitiveFlags.None));

        public bool IsVariantSigned =>
            ((this.PrimitiveFlags & MyPrimitiveFlags.VariantSigned) != MyPrimitiveFlags.None);

        public bool IsFixed8 =>
            ((this.PrimitiveFlags & MyPrimitiveFlags.FixedPoint8) != MyPrimitiveFlags.None);

        public bool IsFixed16 =>
            ((this.PrimitiveFlags & MyPrimitiveFlags.FixedPoint16) != MyPrimitiveFlags.None);

        public System.Text.Encoding Encoding =>
            (((this.PrimitiveFlags & MyPrimitiveFlags.Ascii) != MyPrimitiveFlags.None) ? System.Text.Encoding.ASCII : System.Text.Encoding.UTF8);
    }
}


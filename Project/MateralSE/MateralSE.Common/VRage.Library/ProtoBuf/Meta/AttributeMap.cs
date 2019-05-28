namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal abstract class AttributeMap
    {
        protected AttributeMap()
        {
        }

        public static AttributeMap[] Create(TypeModel model, Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(false);
            AttributeMap[] mapArray = new AttributeMap[customAttributes.Length];
            for (int i = 0; i < customAttributes.Length; i++)
            {
                mapArray[i] = new ReflectionAttributeMap((Attribute) customAttributes[i]);
            }
            return mapArray;
        }

        public static AttributeMap[] Create(TypeModel model, MemberInfo member, bool inherit)
        {
            object[] customAttributes = member.GetCustomAttributes(inherit);
            AttributeMap[] mapArray = new AttributeMap[customAttributes.Length];
            for (int i = 0; i < customAttributes.Length; i++)
            {
                mapArray[i] = new ReflectionAttributeMap((Attribute) customAttributes[i]);
            }
            return mapArray;
        }

        public static AttributeMap[] Create(TypeModel model, Type type, bool inherit)
        {
            object[] customAttributes = type.GetCustomAttributes(inherit);
            AttributeMap[] mapArray = new AttributeMap[customAttributes.Length];
            for (int i = 0; i < customAttributes.Length; i++)
            {
                mapArray[i] = new ReflectionAttributeMap((Attribute) customAttributes[i]);
            }
            return mapArray;
        }

        [Obsolete("Please use AttributeType instead")]
        public Type GetType() => 
            this.AttributeType;

        public bool TryGet(string key, out object value) => 
            this.TryGet(key, true, out value);

        public abstract bool TryGet(string key, bool publicOnly, out object value);

        public abstract Type AttributeType { get; }

        public abstract object Target { get; }

        private class ReflectionAttributeMap : AttributeMap
        {
            private readonly Attribute attribute;

            public ReflectionAttributeMap(Attribute attribute)
            {
                this.attribute = attribute;
            }

            public override bool TryGet(string key, bool publicOnly, out object value)
            {
                foreach (MemberInfo info in Helpers.GetInstanceFieldsAndProperties(this.attribute.GetType(), publicOnly))
                {
                    if (string.Equals(info.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        PropertyInfo info2 = info as PropertyInfo;
                        if (info2 != null)
                        {
                            value = info2.GetValue(this.attribute, null);
                            return true;
                        }
                        FieldInfo info3 = info as FieldInfo;
                        if (info3 == null)
                        {
                            throw new NotSupportedException(info.GetType().Name);
                        }
                        value = info3.GetValue(this.attribute);
                        return true;
                    }
                }
                value = null;
                return false;
            }

            public override object Target =>
                this.attribute;

            public override Type AttributeType =>
                this.attribute.GetType();
        }
    }
}


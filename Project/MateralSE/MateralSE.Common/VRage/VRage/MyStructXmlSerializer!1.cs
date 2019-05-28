namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Serialization;

    public class MyStructXmlSerializer<TStruct> : MyXmlSerializerBase<TStruct> where TStruct: struct
    {
        public static FieldInfo m_defaultValueField;
        private static Dictionary<string, Accessor<TStruct>> m_accessorMap;

        public MyStructXmlSerializer()
        {
        }

        public MyStructXmlSerializer(ref TStruct data)
        {
            base.m_data = data;
        }

        private static void BuildAccessorsInfo()
        {
            if (MyStructXmlSerializer<TStruct>.m_defaultValueField == null)
            {
                Type type = typeof(TStruct);
                lock (type)
                {
                    if (MyStructXmlSerializer<TStruct>.m_defaultValueField == null)
                    {
                        MyStructXmlSerializer<TStruct>.m_defaultValueField = MyStructDefault.GetDefaultFieldInfo(typeof(TStruct));
                        if (MyStructXmlSerializer<TStruct>.m_defaultValueField == null)
                        {
                            throw new Exception("Missing default value for struct " + typeof(TStruct).FullName + ". Decorate one static read-only field with StructDefault attribute");
                        }
                        MyStructXmlSerializer<TStruct>.m_accessorMap = new Dictionary<string, Accessor<TStruct>>();
                        FieldInfo[] fields = typeof(TStruct).GetFields(BindingFlags.Public | BindingFlags.Instance);
                        int index = 0;
                        while (true)
                        {
                            if (index >= fields.Length)
                            {
                                PropertyInfo[] properties = typeof(TStruct).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                index = 0;
                                while (index < properties.Length)
                                {
                                    PropertyInfo info2 = properties[index];
                                    if ((info2.GetCustomAttribute(typeof(XmlIgnoreAttribute)) == null) && (info2.GetIndexParameters().Length == 0))
                                    {
                                        MyStructXmlSerializer<TStruct>.m_accessorMap.Add(info2.Name, new PropertyAccessor<TStruct>(info2));
                                    }
                                    index++;
                                }
                                break;
                            }
                            FieldInfo element = fields[index];
                            if (element.GetCustomAttribute(typeof(XmlIgnoreAttribute)) == null)
                            {
                                MyStructXmlSerializer<TStruct>.m_accessorMap.Add(element.Name, new FieldAccessor<TStruct>(element));
                            }
                            index++;
                        }
                    }
                }
            }
        }

        public static implicit operator MyStructXmlSerializer<TStruct>(TStruct data) => 
            new MyStructXmlSerializer<TStruct>(ref data);

        public override void ReadXml(XmlReader reader)
        {
            MyStructXmlSerializer<TStruct>.BuildAccessorsInfo();
            object obj2 = (TStruct) MyStructXmlSerializer<TStruct>.m_defaultValueField.GetValue(null);
            reader.MoveToElement();
            if (reader.IsEmptyElement)
            {
                reader.Skip();
            }
            else
            {
                reader.ReadStartElement();
                reader.MoveToContent();
                while ((reader.NodeType != XmlNodeType.EndElement) && (reader.NodeType != XmlNodeType.None))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        Accessor<TStruct> accessor;
                        if (!MyStructXmlSerializer<TStruct>.m_accessorMap.TryGetValue(reader.LocalName, out accessor))
                        {
                            reader.Skip();
                        }
                        else
                        {
                            object data;
                            if (accessor.IsPrimitiveType)
                            {
                                string str = reader.ReadElementString();
                                data = TypeDescriptor.GetConverter(accessor.Type).ConvertFrom(null, CultureInfo.InvariantCulture, str);
                            }
                            else if (accessor.SerializerType == null)
                            {
                                XmlSerializer orCreateSerializer = MyXmlSerializerManager.GetOrCreateSerializer(accessor.Type);
                                data = base.Deserialize(reader, orCreateSerializer, MyXmlSerializerManager.GetSerializedName(accessor.Type));
                            }
                            else
                            {
                                IMyXmlSerializable serializable1 = Activator.CreateInstance(accessor.SerializerType) as IMyXmlSerializable;
                                serializable1.ReadXml(reader.ReadSubtree());
                                data = serializable1.Data;
                                reader.ReadEndElement();
                            }
                            accessor.SetValue(obj2, data);
                        }
                    }
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
                base.m_data = (TStruct) obj2;
            }
        }

        private abstract class Accessor
        {
            protected Accessor()
            {
            }

            protected void CheckXmlElement(MemberInfo info)
            {
                XmlElementAttribute attribute = info.GetCustomAttribute(typeof(XmlElementAttribute), false) as XmlElementAttribute;
                if (((attribute != null) && (attribute.Type != null)) && typeof(IMyXmlSerializable).IsAssignableFrom(attribute.Type))
                {
                    this.SerializerType = attribute.Type;
                }
            }

            public abstract object GetValue(object obj);
            public abstract void SetValue(object obj, object value);

            public abstract System.Type Type { get; }

            public System.Type SerializerType { get; private set; }

            public bool IsPrimitiveType
            {
                get
                {
                    System.Type type = this.Type;
                    return (type.IsPrimitive || (type == typeof(string)));
                }
            }
        }

        private class FieldAccessor : MyStructXmlSerializer<TStruct>.Accessor
        {
            public FieldAccessor(FieldInfo field)
            {
                this.Field = field;
                base.CheckXmlElement(field);
            }

            public override object GetValue(object obj) => 
                this.Field.GetValue(obj);

            public override void SetValue(object obj, object value)
            {
                this.Field.SetValue(obj, value);
            }

            public FieldInfo Field { get; private set; }

            public override System.Type Type =>
                this.Field.FieldType;
        }

        private class PropertyAccessor : MyStructXmlSerializer<TStruct>.Accessor
        {
            public PropertyAccessor(PropertyInfo property)
            {
                this.Property = property;
                base.CheckXmlElement(property);
            }

            public override object GetValue(object obj) => 
                this.Property.GetValue(obj);

            public override void SetValue(object obj, object value)
            {
                this.Property.SetValue(obj, value);
            }

            public PropertyInfo Property { get; private set; }

            public override System.Type Type =>
                this.Property.PropertyType;
        }
    }
}


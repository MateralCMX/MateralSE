namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageRender;

    public class MyConstProperty<T> : IMyConstProperty
    {
        private string m_name;
        private T m_value;

        public MyConstProperty()
        {
            this.Init();
        }

        public MyConstProperty(string name) : this()
        {
            this.m_name = name;
        }

        public virtual void Deserialize(XmlReader reader)
        {
            object obj2;
            this.m_name = reader.GetAttribute("name");
            reader.ReadStartElement();
            this.DeserializeValue(reader, out obj2);
            this.m_value = (T) obj2;
            reader.ReadEndElement();
        }

        public virtual void DeserializeFromObjectBuilder(GenerationProperty property)
        {
            object valueInt;
            this.m_name = property.Name;
            string type = property.Type;
            uint num = <PrivateImplementationDetails>.ComputeStringHash(type);
            if (num > 0x528bdc96)
            {
                if (num > 0x840071c3)
                {
                    if (num == 0x890079a2)
                    {
                        if (type == "Vector4")
                        {
                            valueInt = property.ValueVector4;
                            goto TR_0000;
                        }
                    }
                    else if ((num == 0xf87415fe) && (type != "Int"))
                    {
                    }
                }
                else if (num == 0x604f4858)
                {
                    if (type == "String")
                    {
                        valueInt = property.ValueString;
                        goto TR_0000;
                    }
                }
                else if ((num == 0x840071c3) && (type == "Vector3"))
                {
                    valueInt = property.ValueVector3;
                    goto TR_0000;
                }
            }
            else if (num == 0x2f742c5d)
            {
                if (type == "Bool")
                {
                    valueInt = property.ValueBool;
                    goto TR_0000;
                }
            }
            else if (num == 0x4c816225)
            {
                if (type == "Float")
                {
                    valueInt = property.ValueFloat;
                    goto TR_0000;
                }
            }
            else if ((num == 0x528bdc96) && (type == "MyTransparentMaterial"))
            {
                valueInt = MyTransparentMaterials.GetMaterial(MyStringId.GetOrCompute(property.ValueString));
                goto TR_0000;
            }
            valueInt = property.ValueInt;
        TR_0000:
            this.m_value = (T) valueInt;
        }

        public virtual void DeserializeValue(XmlReader reader, out object value)
        {
            value = reader.Value;
            reader.Read();
        }

        public virtual IMyConstProperty Duplicate() => 
            null;

        protected virtual void Duplicate(IMyConstProperty targetProp)
        {
            targetProp.SetValue(this.GetValue<T>());
        }

        public U GetValue<U>() where U: T => 
            this.m_value;

        protected virtual Type GetValueTypeInternal() => 
            typeof(T);

        protected virtual void Init()
        {
        }

        public virtual void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("Value" + this.ValueType);
            this.SerializeValue(writer, this.m_value);
            writer.WriteEndElement();
        }

        public virtual void SerializeValue(XmlWriter writer, object value)
        {
        }

        public virtual void SetValue(object val)
        {
            this.SetValue((T) val);
        }

        public void SetValue(T val)
        {
            this.m_value = val;
        }

        object IMyConstProperty.GetValue() => 
            this.m_value;

        Type IMyConstProperty.GetValueType() => 
            this.GetValueTypeInternal();

        public string Name
        {
            get => 
                this.m_name;
            set => 
                (this.m_name = value);
        }

        public virtual string ValueType =>
            typeof(T).Name;

        public virtual string BaseValueType =>
            this.ValueType;

        public virtual bool Animated =>
            false;

        public virtual bool Is2D =>
            false;
    }
}


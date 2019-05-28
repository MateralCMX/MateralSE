namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyEnum : MyConstPropertyInt, IMyConstProperty
    {
        private Type m_enumType;
        private List<string> m_enumStrings;

        public MyConstPropertyEnum()
        {
        }

        public MyConstPropertyEnum(string name) : this(name, null, null)
        {
        }

        public MyConstPropertyEnum(string name, Type enumType, List<string> enumStrings) : base(name)
        {
            this.m_enumType = enumType;
            this.m_enumStrings = enumStrings;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyEnum targetProp = new MyConstPropertyEnum(base.Name);
            this.Duplicate(targetProp);
            targetProp.m_enumType = this.m_enumType;
            targetProp.m_enumStrings = this.m_enumStrings;
            return targetProp;
        }

        public List<string> GetEnumStrings() => 
            this.m_enumStrings;

        public Type GetEnumType() => 
            this.m_enumType;

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((int) value).ToString(CultureInfo.InvariantCulture));
        }

        public override void SetValue(object val)
        {
            int num = Convert.ToInt32(val);
            base.SetValue(num);
        }

        Type IMyConstProperty.GetValueType() => 
            this.m_enumType;

        public override string BaseValueType =>
            "Enum";
    }
}


namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyAnimatedProperty2DEnum : MyAnimatedProperty2DInt
    {
        private Type m_enumType;
        private List<string> m_enumStrings;

        public MyAnimatedProperty2DEnum(string name) : this(name, null, null)
        {
        }

        public MyAnimatedProperty2DEnum(string name, Type enumType, List<string> enumStrings) : this(name, null, enumType, enumStrings)
        {
        }

        public MyAnimatedProperty2DEnum(string name, MyAnimatedProperty<int>.InterpolatorDelegate interpolator, Type enumType, List<string> enumStrings) : base(name, interpolator)
        {
            this.m_enumType = enumType;
            this.m_enumStrings = enumStrings;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyInt num = new MyAnimatedPropertyInt(base.Name, base.m_interpolator2);
            num.Deserialize(reader);
            value = num;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DEnum targetProp = new MyAnimatedProperty2DEnum(base.Name);
            this.Duplicate(targetProp);
            targetProp.m_enumType = this.m_enumType;
            targetProp.m_enumStrings = this.m_enumStrings;
            return targetProp;
        }

        public List<string> GetEnumStrings() => 
            this.m_enumStrings;

        public Type GetEnumType() => 
            this.m_enumType;

        public override string BaseValueType =>
            "Enum";
    }
}


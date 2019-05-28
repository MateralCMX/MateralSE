namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;

    public class MyAnimatedPropertyEnum : MyAnimatedPropertyInt
    {
        private Type m_enumType;
        private List<string> m_enumStrings;

        public MyAnimatedPropertyEnum()
        {
        }

        public MyAnimatedPropertyEnum(string name) : this(name, null, null)
        {
        }

        public MyAnimatedPropertyEnum(string name, Type enumType, List<string> enumStrings) : this(name, null, enumType, enumStrings)
        {
        }

        public MyAnimatedPropertyEnum(string name, MyAnimatedProperty<int>.InterpolatorDelegate interpolator, Type enumType, List<string> enumStrings) : base(name, interpolator)
        {
            this.m_enumType = enumType;
            this.m_enumStrings = enumStrings;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyEnum targetProp = new MyAnimatedPropertyEnum(base.Name);
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


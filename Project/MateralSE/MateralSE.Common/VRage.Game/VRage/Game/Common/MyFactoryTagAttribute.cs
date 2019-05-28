namespace VRage.Game.Common
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public class MyFactoryTagAttribute : Attribute
    {
        public readonly Type ObjectBuilderType;
        public Type ProducedType;
        public bool IsMain;

        public MyFactoryTagAttribute(Type objectBuilderType, bool mainBuilder = true)
        {
            this.ObjectBuilderType = objectBuilderType;
            this.IsMain = mainBuilder;
        }
    }
}


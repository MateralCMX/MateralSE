namespace VRage.Game.Entity
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyEntityTypeAttribute : MyFactoryTagAttribute
    {
        public MyEntityTypeAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}


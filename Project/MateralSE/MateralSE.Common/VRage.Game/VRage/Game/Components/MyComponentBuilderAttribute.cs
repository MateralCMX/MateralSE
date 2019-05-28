namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyComponentBuilderAttribute : MyFactoryTagAttribute
    {
        public MyComponentBuilderAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}


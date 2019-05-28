namespace Sandbox.Game.World
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyEventTypeAttribute : MyFactoryTagAttribute
    {
        public MyEventTypeAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}


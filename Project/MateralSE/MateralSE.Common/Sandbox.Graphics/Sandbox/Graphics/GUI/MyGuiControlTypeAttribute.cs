namespace Sandbox.Graphics.GUI
{
    using System;
    using VRage.Game.Common;

    public class MyGuiControlTypeAttribute : MyFactoryTagAttribute
    {
        public MyGuiControlTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


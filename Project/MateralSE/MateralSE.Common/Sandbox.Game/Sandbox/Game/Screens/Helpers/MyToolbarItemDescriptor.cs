namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using VRage.Game.Common;

    public class MyToolbarItemDescriptor : MyFactoryTagAttribute
    {
        public MyToolbarItemDescriptor(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


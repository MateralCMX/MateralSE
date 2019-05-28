namespace Sandbox.Game.Entities.Cube
{
    using System;
    using VRage.Game.Common;

    public class MyCubeBlockTypeAttribute : MyFactoryTagAttribute
    {
        public MyCubeBlockTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


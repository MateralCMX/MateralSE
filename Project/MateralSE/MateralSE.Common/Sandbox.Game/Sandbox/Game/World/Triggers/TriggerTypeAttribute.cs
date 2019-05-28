namespace Sandbox.Game.World.Triggers
{
    using System;
    using VRage.Game.Common;

    public class TriggerTypeAttribute : MyFactoryTagAttribute
    {
        public TriggerTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


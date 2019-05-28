namespace Sandbox.Game.AI
{
    using System;
    using VRage.Game.Common;

    internal class MyAutopilotTypeAttribute : MyFactoryTagAttribute
    {
        public MyAutopilotTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


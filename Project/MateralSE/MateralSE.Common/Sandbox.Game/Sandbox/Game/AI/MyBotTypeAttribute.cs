namespace Sandbox.Game.AI
{
    using System;
    using VRage.Game.Common;

    public class MyBotTypeAttribute : MyFactoryTagAttribute
    {
        public MyBotTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}


namespace Sandbox.Game.Entities
{
    using System;
    using VRage.Game.Common;

    public class MyEntityStatEffectTypeAttribute : MyFactoryTagAttribute
    {
        public readonly Type MemoryType;

        public MyEntityStatEffectTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
            this.MemoryType = typeof(MyEntityStatRegenEffect);
        }

        public MyEntityStatEffectTypeAttribute(Type objectBuilderType, Type memoryType) : base(objectBuilderType, true)
        {
            this.MemoryType = memoryType;
        }
    }
}


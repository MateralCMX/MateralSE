namespace Sandbox.Game.AI
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class BehaviorActionImplAttribute : Attribute
    {
        public readonly Type LogicType;

        public BehaviorActionImplAttribute(Type logicType)
        {
            this.LogicType = logicType;
        }
    }
}


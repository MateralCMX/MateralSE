namespace Sandbox.Game.AI
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class BehaviorLogicAttribute : Attribute
    {
        public readonly string BehaviorSubtype;

        public BehaviorLogicAttribute(string behaviorSubtype)
        {
            this.BehaviorSubtype = behaviorSubtype;
        }
    }
}


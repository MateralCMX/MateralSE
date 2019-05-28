namespace Sandbox.Game.AI
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class TargetTypeAttribute : Attribute
    {
        public readonly string TargetType;

        public TargetTypeAttribute(string targetType)
        {
            this.TargetType = targetType;
        }
    }
}


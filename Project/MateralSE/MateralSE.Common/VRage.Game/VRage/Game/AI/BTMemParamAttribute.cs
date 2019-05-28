namespace VRage.Game.AI
{
    using System;
    using VRage.Game;

    [AttributeUsage(AttributeTargets.Parameter, Inherited=true)]
    public abstract class BTMemParamAttribute : Attribute
    {
        protected BTMemParamAttribute()
        {
        }

        public abstract MyMemoryParameterType MemoryType { get; }
    }
}


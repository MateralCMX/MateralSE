namespace VRage.Game.AI
{
    using System;
    using VRage.Game;

    [AttributeUsage(AttributeTargets.Parameter, Inherited=true)]
    public class BTInAttribute : BTMemParamAttribute
    {
        public override MyMemoryParameterType MemoryType =>
            MyMemoryParameterType.IN;
    }
}


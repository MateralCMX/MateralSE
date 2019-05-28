namespace VRage.Game.AI
{
    using System;
    using VRage.Game;

    [AttributeUsage(AttributeTargets.Parameter, Inherited=true)]
    public class BTInOutAttribute : BTMemParamAttribute
    {
        public override MyMemoryParameterType MemoryType =>
            (MyMemoryParameterType.IN | MyMemoryParameterType.OUT);
    }
}


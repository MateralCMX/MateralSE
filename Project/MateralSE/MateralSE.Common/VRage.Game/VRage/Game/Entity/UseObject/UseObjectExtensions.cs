namespace VRage.Game.Entity.UseObject
{
    using System;
    using System.Runtime.CompilerServices;

    public static class UseObjectExtensions
    {
        public static bool IsActionSupported(this IMyUseObject useObject, UseActionEnum action) => 
            ((useObject.SupportedActions & action) == action);
    }
}


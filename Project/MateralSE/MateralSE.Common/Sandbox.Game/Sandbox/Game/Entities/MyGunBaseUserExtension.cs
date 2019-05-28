namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyGunBaseUserExtension
    {
        public static bool PutConstraint(this IMyGunBaseUser obj) => 
            !string.IsNullOrEmpty(obj.ConstraintDisplayName);
    }
}


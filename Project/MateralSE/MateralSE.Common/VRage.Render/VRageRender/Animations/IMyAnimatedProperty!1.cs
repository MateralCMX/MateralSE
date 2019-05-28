namespace VRageRender.Animations
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Obfuscation(Feature="cw symbol renaming", Exclude=true, ApplyToMembers=true)]
    public interface IMyAnimatedProperty<T> : IMyAnimatedProperty, IMyConstProperty
    {
        [Obfuscation(Feature="cw symbol renaming", Exclude=true)]
        int AddKey<U>(float time, U val) where U: T;
        [Obfuscation(Feature="cw symbol renaming", Exclude=true)]
        void GetInterpolatedValue<U>(float time, out U value) where U: T;
    }
}


namespace VRageRender.Animations
{
    using System;

    public interface IMyAnimatedProperty2D : IMyAnimatedProperty, IMyConstProperty
    {
        IMyAnimatedProperty CreateEmptyKeys();
        void GetInterpolatedKeys(float overallTime, float multiplier, IMyAnimatedProperty interpolatedKeys);
    }
}


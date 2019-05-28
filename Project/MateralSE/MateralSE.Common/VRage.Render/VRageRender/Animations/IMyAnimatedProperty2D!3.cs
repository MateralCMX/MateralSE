namespace VRageRender.Animations
{
    using System;

    public interface IMyAnimatedProperty2D<T, V, W> : IMyAnimatedProperty2D, IMyAnimatedProperty, IMyConstProperty
    {
        void GetInterpolatedKeys(float overallTime, W variance, float multiplier, IMyAnimatedProperty interpolatedKeys);
        X GetInterpolatedValue<X>(float overallTime, float time) where X: V;
    }
}


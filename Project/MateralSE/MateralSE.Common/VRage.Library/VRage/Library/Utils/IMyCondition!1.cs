namespace VRage.Library.Utils
{
    using System;

    public interface IMyCondition<T>
    {
        bool Evaluate(T value);
    }
}


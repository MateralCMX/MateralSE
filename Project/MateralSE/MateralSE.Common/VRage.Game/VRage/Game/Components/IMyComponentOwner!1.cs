namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMyComponentOwner<T>
    {
        bool GetComponent(out T component);
    }
}


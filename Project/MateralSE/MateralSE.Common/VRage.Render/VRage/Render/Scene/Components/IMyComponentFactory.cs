namespace VRage.Render.Scene.Components
{
    using System;

    public interface IMyComponentFactory
    {
        MyActorComponent Create(Type type);
        void Deallocate(MyActorComponent item);
    }
}


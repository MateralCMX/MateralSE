namespace VRage.Render.Scene
{
    using System;
    using VRage.Render.Scene.Components;

    public interface IMyActorFactory
    {
        MyLightComponent CreateLight(string debugName);
        void Destroy(MyActor actor, bool fadeOut);
    }
}


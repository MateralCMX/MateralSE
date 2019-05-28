namespace VRage.Game.Components
{
    using System;

    public interface IMyComponentBase
    {
        void OnAddedToContainer();
        void OnAddedToScene();
        void OnRemovedFromContainer();
        void OnRemovedFromScene();
        void SetContainer(IMyComponentContainer container);
    }
}


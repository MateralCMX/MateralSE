namespace VRage.Game.ModAPI
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.ModAPI.Interfaces;

    public interface IMyEntityController
    {
        event Action<IMyControllableEntity, IMyControllableEntity> ControlledEntityChanged;

        void TakeControl(IMyControllableEntity entity);

        IMyControllableEntity ControlledEntity { get; }
    }
}


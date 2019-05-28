namespace VRage.Game.ModAPI
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IMyControllerInfo
    {
        event Action<IMyEntityController> ControlAcquired;

        event Action<IMyEntityController> ControlReleased;

        bool IsLocallyControlled();
        bool IsLocallyHumanControlled();
        bool IsRemotelyControlled();

        IMyEntityController Controller { get; }

        long ControllingIdentityId { get; }
    }
}


namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyAttachableTopBlock : IMyCubeBlock, IMyEntity
    {
        bool IsAttached { get; }

        IMyMechanicalConnectionBlock Base { get; }
    }
}


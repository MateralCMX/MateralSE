namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMyShipController : Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyShipController, IMyControllableEntity
    {
        bool HasFirstPersonCamera { get; }

        IMyCharacter LastPilot { get; }

        IMyCharacter Pilot { get; }

        bool IsShooting { get; }

        Vector3 MoveIndicator { get; }

        Vector2 RotationIndicator { get; }

        float RollIndicator { get; }
    }
}


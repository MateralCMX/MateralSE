namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyShipController : IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        MyShipMass CalculateShipMass();
        Vector3D GetArtificialGravity();
        Vector3D GetNaturalGravity();
        double GetShipSpeed();
        MyShipVelocities GetShipVelocities();
        Vector3D GetTotalGravity();
        bool TryGetPlanetElevation(MyPlanetElevation detail, out double elevation);
        bool TryGetPlanetPosition(out Vector3D position);

        bool CanControlShip { get; }

        bool IsUnderControl { get; }

        bool HasWheels { get; }

        bool ControlWheels { get; set; }

        bool ControlThrusters { get; set; }

        bool HandBrake { get; set; }

        bool DampenersOverride { get; set; }

        bool ShowHorizonIndicator { get; set; }

        Vector3 MoveIndicator { get; }

        Vector2 RotationIndicator { get; }

        float RollIndicator { get; }

        Vector3D CenterOfMass { get; }

        bool IsMainCockpit { get; set; }
    }
}


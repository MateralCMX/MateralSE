namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyRemoteControl : IMyShipController, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void AddWaypoint(MyWaypointInfo coords);
        void AddWaypoint(Vector3D coords, string name);
        void ClearWaypoints();
        bool GetNearestPlayer(out Vector3D playerPosition);
        void GetWaypointInfo(List<MyWaypointInfo> waypoints);
        void SetAutoPilotEnabled(bool enabled);
        void SetCollisionAvoidance(bool enabled);
        void SetDockingMode(bool enabled);

        bool IsAutoPilotEnabled { get; }

        float SpeedLimit { get; set; }

        Sandbox.ModAPI.Ingame.FlightMode FlightMode { get; set; }

        VRageMath.Base6Directions.Direction Direction { get; set; }

        MyWaypointInfo CurrentWaypoint { get; }
    }
}


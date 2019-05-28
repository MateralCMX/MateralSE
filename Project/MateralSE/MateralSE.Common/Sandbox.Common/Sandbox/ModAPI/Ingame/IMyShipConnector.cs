namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyShipConnector : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void Connect();
        void Disconnect();
        void ToggleConnect();

        bool ThrowOut { get; set; }

        bool CollectAll { get; set; }

        float PullStrength { get; set; }

        [Obsolete("Use the Status property")]
        bool IsLocked { get; }

        [Obsolete("Use the Status property")]
        bool IsConnected { get; }

        MyShipConnectorStatus Status { get; }

        IMyShipConnector OtherConnector { get; }
    }
}


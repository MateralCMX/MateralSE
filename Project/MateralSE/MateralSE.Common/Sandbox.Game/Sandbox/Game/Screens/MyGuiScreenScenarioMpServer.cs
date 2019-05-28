namespace Sandbox.Game.Screens
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.GameServices;

    internal class MyGuiScreenScenarioMpServer : MyGuiScreenScenarioMpBase
    {
        protected override void OnStartClicked(MyGuiControlButton sender)
        {
            MySession.Static.Settings.CanJoinRunning = false;
            if (!MySession.Static.Settings.CanJoinRunning)
            {
                MyMultiplayer.Static.SetLobbyType(MyLobbyType.Private);
            }
            MyScenarioSystem.Static.PrepareForStart();
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
        }
    }
}


namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyPlayersMenuControlHelper : MyAbstractControlMenuItem
    {
        public MyPlayersMenuControlHelper() : base("F3", MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PlayersScreen, Array.Empty<object>()));
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowPlayers);
    }
}


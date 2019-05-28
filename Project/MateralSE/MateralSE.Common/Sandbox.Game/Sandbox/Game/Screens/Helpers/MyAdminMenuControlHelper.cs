namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyAdminMenuControlHelper : MyAbstractControlMenuItem
    {
        public MyAdminMenuControlHelper() : base("F10", MySupportKeysEnum.ALT)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.AdminMenuScreen, Array.Empty<object>()));
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowAdminMenu);
    }
}


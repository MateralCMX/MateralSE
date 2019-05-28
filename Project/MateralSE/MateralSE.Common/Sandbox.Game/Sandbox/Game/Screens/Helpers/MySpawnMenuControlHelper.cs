namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MySpawnMenuControlHelper : MyAbstractControlMenuItem
    {
        public MySpawnMenuControlHelper() : base("F10", MySupportKeysEnum.NONE | MySupportKeysEnum.SHIFT)
        {
        }

        public override void Activate()
        {
            if (!MySession.Static.IsAdminMenuEnabled || (MyPerGameSettings.Game == GameEnum.UNKNOWN_GAME))
            {
                MyHud.Notifications.Add(MyNotificationSingletons.AdminMenuNotAvailable);
            }
            else
            {
                MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.VoxelMapEditingScreen, Array.Empty<object>()));
            }
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowSpawnMenu);

        public override bool Enabled =>
            (MySession.Static.IsAdminMenuEnabled && (MyPerGameSettings.Game != GameEnum.UNKNOWN_GAME));
    }
}


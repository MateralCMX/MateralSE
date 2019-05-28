namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyHelpMenuControlHelper : MyAbstractControlMenuItem
    {
        public MyHelpMenuControlHelper() : base(MyControlsSpace.HELP_SCREEN, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HelpScreen, Array.Empty<object>());
            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
            MyGuiSandbox.AddScreen(screen);
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowHelp);
    }
}


namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyShowBuildScreenControlHelper : MyAbstractControlMenuItem
    {
        private IMyControllableEntity m_entity;

        public MyShowBuildScreenControlHelper() : base(MyControlsSpace.BUILD_SCREEN, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiScreenHudSpace.Static.HideScreen();
            object[] args = new object[] { 0, this.m_entity as MyShipController };
            MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
            MyGuiSandbox.AddScreen(screen);
        }

        public void SetEntity(IMyControllableEntity entity)
        {
            this.m_entity = entity;
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowBuildScreen);
    }
}


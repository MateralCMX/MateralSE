namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyShowTerminalControlHelper : MyAbstractControlMenuItem
    {
        private IMyControllableEntity m_entity;

        public MyShowTerminalControlHelper() : base(MyControlsSpace.TERMINAL, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiScreenHudSpace.Static.HideScreen();
            this.m_entity.ShowTerminal();
        }

        public void SetEntity(IMyControllableEntity entity)
        {
            this.m_entity = entity;
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_Terminal);
    }
}


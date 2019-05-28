namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyBriefingMenuControlHelper : MyAbstractControlMenuItem
    {
        private IMyControllableEntity m_entity;

        public MyBriefingMenuControlHelper() : base(MyControlsSpace.MISSION_SETTINGS, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiSandbox.AddScreen(new MyGuiScreenBriefing());
        }

        public override bool Enabled =>
            (base.Enabled && MyFakes.ENABLE_MISSION_TRIGGERS);

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ScenarioBriefing);
    }
}


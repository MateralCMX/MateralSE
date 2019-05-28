namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyEnableStationRotationControlHelper : MyAbstractControlMenuItem
    {
        private IMyControllableEntity m_entity;

        public MyEnableStationRotationControlHelper() : base(MyControlsSpace.FREE_ROTATION, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.StationRotation_Static);
    }
}


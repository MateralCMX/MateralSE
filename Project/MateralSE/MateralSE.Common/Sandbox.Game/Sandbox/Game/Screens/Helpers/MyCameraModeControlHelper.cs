namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using VRage;

    public class MyCameraModeControlHelper : MyAbstractControlMenuItem
    {
        private string m_value;

        public MyCameraModeControlHelper() : base(MyControlsSpace.CAMERA_MODE, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyGuiScreenGamePlay.Static.SwitchCamera();
        }

        public override void Next()
        {
            this.Activate();
        }

        public override void Previous()
        {
            this.Activate();
        }

        public override void UpdateValue()
        {
            if (MySession.Static.CameraController.IsInFirstPersonView)
            {
                this.m_value = MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_FPP);
            }
            else
            {
                this.m_value = MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_TPP);
            }
        }

        public override bool Enabled =>
            MyGuiScreenGamePlay.Static.CanSwitchCamera;

        public override string CurrentValue =>
            this.m_value;

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_CameraMode);
    }
}


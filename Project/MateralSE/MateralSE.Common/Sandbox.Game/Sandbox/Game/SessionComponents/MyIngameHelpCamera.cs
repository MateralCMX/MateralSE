namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Input;

    [IngameObjective("IngameHelp_Camera", 20)]
    internal class MyIngameHelpCamera : MyIngameHelpObjective
    {
        private bool m_Cpressed;
        private bool m_AltWheelpressed;

        public MyIngameHelpCamera()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Camera_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Camera_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Camera_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.CAMERA_MODE) };
            detail.FinishCondition = new Func<bool>(this.CameraModeCondition);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Camera_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl("ALT + mouse wheel") };
            detail.FinishCondition = new Func<bool>(this.AltWheelCondition);
            detailArray1[2] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.RequiredCondition = new Func<bool>(this.ThirdPersonEnabledCondition);
        }

        private bool AltWheelCondition()
        {
            if (((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator) && MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND)) && ((MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue()) || (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())))
            {
                this.m_AltWheelpressed = true;
            }
            return this.m_AltWheelpressed;
        }

        private bool CameraModeCondition()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CAMERA_MODE))
            {
                this.m_Cpressed = true;
            }
            return this.m_Cpressed;
        }

        private bool ThirdPersonEnabledCondition() => 
            MySession.Static.Settings.Enable3rdPersonView;
    }
}


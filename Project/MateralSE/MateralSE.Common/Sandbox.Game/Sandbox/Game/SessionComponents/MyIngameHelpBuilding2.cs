namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Building2", 70)]
    internal class MyIngameHelpBuilding2 : MyIngameHelpObjective
    {
        private bool m_blockSizeChanged;
        private bool m_insertPressed;
        private bool m_deletePressed;
        private bool m_homePressed;
        private bool m_endPressed;
        private bool m_pageUpPressed;
        private bool m_pageDownPressed;

        public MyIngameHelpBuilding2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Building_Title;
            base.RequiredIds = new string[] { "IngameHelp_Building" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Building2_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Building2_Detail2;
            detail2.FinishCondition = new Func<bool>(this.SizeSelectCondition);
            detailArray1[1] = detail2;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Building2_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE), GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE), GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE), GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE), GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE), GetHighlightedControl(MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE) };
            detail.FinishCondition = new Func<bool>(this.RotateCondition);
            detailArray1[2] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.OnBlockSizeChanged += new Action(this.Static_OnBlockSizeChanged);
            }
        }

        private bool RotateCondition()
        {
            if (!MyCubeBuilder.Static.IsActivated || (MyCubeBuilder.Static.ToolbarBlockDefinition == null))
            {
                return false;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE))
            {
                this.m_insertPressed = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE))
            {
                this.m_deletePressed = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE))
            {
                this.m_homePressed = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE))
            {
                this.m_endPressed = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE))
            {
                this.m_pageUpPressed = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE))
            {
                this.m_pageDownPressed = true;
            }
            return (this.m_insertPressed && (this.m_deletePressed && (this.m_homePressed && (this.m_endPressed && (this.m_pageUpPressed && this.m_pageDownPressed)))));
        }

        private bool SizeSelectCondition() => 
            this.m_blockSizeChanged;

        private void Static_OnBlockSizeChanged()
        {
            this.m_blockSizeChanged = true;
        }
    }
}


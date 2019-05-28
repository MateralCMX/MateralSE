namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Grinder", 180)]
    internal class MyIngameHelpGrinder : MyIngameHelpObjective
    {
        public MyIngameHelpGrinder()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Grinder_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.PlayerHasGrinder));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Grinder_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Grinder_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.PRIMARY_TOOL_ACTION) };
            detail.FinishCondition = new Func<bool>(this.PlayerIsGrinding);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_GrinderTip";
        }

        private bool PlayerHasGrinder()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && (localCharacter.EquippedTool is MyAngleGrinder));
        }

        private bool PlayerIsGrinding()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.EquippedTool is MyAngleGrinder) && !string.IsNullOrEmpty((localCharacter.EquippedTool as MyAngleGrinder).EffectId)));
        }
    }
}


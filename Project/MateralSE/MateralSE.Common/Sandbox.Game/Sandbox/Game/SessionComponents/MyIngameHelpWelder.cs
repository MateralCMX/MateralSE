namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Welder", 190)]
    internal class MyIngameHelpWelder : MyIngameHelpObjective
    {
        public MyIngameHelpWelder()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Welder_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.PlayerHasWelder));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Welder_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Welder_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.PRIMARY_TOOL_ACTION) };
            detail.FinishCondition = new Func<bool>(this.PlayerIsWelding);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_WelderTip";
        }

        private bool PlayerHasWelder()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && (localCharacter.EquippedTool is MyWelder));
        }

        private bool PlayerIsWelding()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.EquippedTool is MyWelder) && (!string.IsNullOrEmpty((localCharacter.EquippedTool as MyWelder).EffectId) && ((localCharacter.EquippedTool as MyWelder).IsShooting && (localCharacter.EquippedTool as MyWelder).HasHitBlock))));
        }
    }
}


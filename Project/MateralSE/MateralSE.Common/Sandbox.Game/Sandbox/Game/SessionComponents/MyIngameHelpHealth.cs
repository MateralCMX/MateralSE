namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Health", 120)]
    internal class MyIngameHelpHealth : MyIngameHelpObjective
    {
        public MyIngameHelpHealth()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Health_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LowHealth));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Health_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Health_Detail2;
            detail2.FinishCondition = new Func<bool>(this.HealthReplenished);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_HealthTip";
        }

        private bool HealthReplenished()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.StatComp != null) && (localCharacter.StatComp.HealthRatio > 0.99f)));
        }

        private bool LowHealth()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.StatComp != null) && ((localCharacter.StatComp.HealthRatio > 0f) && (localCharacter.StatComp.HealthRatio < 0.9f))));
        }
    }
}


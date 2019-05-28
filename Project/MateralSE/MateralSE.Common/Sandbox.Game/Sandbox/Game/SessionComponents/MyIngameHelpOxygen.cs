namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Oxygen", 130)]
    internal class MyIngameHelpOxygen : MyIngameHelpObjective
    {
        public MyIngameHelpOxygen()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Oxygen_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LowOxygen));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Oxygen_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Oxygen_Detail2;
            detail2.FinishCondition = new Func<bool>(this.OxygenReplenished);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_OxygenTip";
        }

        private bool LowOxygen()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.OxygenComponent != null) && ((localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.OxygenId) > 0f) && (localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.OxygenId) < 0.5f))));
        }

        private bool OxygenReplenished()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.OxygenComponent != null) && (localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.OxygenId) > 0.99f)));
        }
    }
}


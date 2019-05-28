namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Hydrogen", 150)]
    internal class MyIngameHelpHydrogen : MyIngameHelpObjective
    {
        public MyIngameHelpHydrogen()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Hydrogen_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LowHydrogen));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Hydrogen_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Hydrogen_Detail2;
            detail2.FinishCondition = new Func<bool>(this.HydrogenReplenished);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_HydrogenTip";
        }

        private bool HydrogenReplenished()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.OxygenComponent != null) && (localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId) > 0.99f)));
        }

        private bool LowHydrogen()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.OxygenComponent != null) && ((localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId) > 0f) && (localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId) < 0.5f))));
        }
    }
}


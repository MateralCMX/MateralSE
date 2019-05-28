namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Energy", 140)]
    internal class MyIngameHelpEnergy : MyIngameHelpObjective
    {
        public MyIngameHelpEnergy()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Energy_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LowEnergy));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Energy_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Energy_Detail2;
            detail2.FinishCondition = new Func<bool>(this.EnergyReplenished);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_EnergyTip";
        }

        private bool EnergyReplenished()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && (localCharacter.SuitEnergyLevel > 0.99f));
        }

        private bool LowEnergy()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.SuitEnergyLevel > 0f) && (localCharacter.SuitEnergyLevel < 0.5f)));
        }
    }
}


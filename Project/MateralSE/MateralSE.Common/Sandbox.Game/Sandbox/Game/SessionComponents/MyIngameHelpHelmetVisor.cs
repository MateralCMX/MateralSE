namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_HelmetVisor", 330)]
    internal class MyIngameHelpHelmetVisor : MyIngameHelpObjective
    {
        private bool m_damageFromLowOxygen;
        private bool m_helmetClosed;

        public MyIngameHelpHelmetVisor()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_HelmetVisor_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.DamageFromLowOxygen));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_HelmetVisor_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HelmetVisor_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HELMET) };
            detail.FinishCondition = new Func<bool>(this.HelmetClosed);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_HelmetVisorTip";
        }

        private bool DamageFromLowOxygen() => 
            ((MySession.Static != null) && ((MySession.Static.LocalCharacter != null) && (ReferenceEquals(MySession.Static.ControlledEntity, MySession.Static.LocalCharacter) && ((MySession.Static.LocalCharacter.Breath != null) && (MySession.Static.LocalCharacter.Breath.CurrentState == MyCharacterBreath.State.Choking)))));

        private bool HelmetClosed()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELMET))
            {
                this.m_helmetClosed = true;
            }
            return this.m_helmetClosed;
        }

        public override bool IsCritical() => 
            this.DamageFromLowOxygen();
    }
}


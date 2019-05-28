namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Rifle", 200)]
    internal class MyIngameHelpRifle : MyIngameHelpObjective
    {
        public MyIngameHelpRifle()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Rifle_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.PlayerHasRifle));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Rifle_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Rifle_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HELP_SCREEN) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }

        private bool PlayerHasRifle()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && (localCharacter.EquippedTool is MyAutomaticRifleGun));
        }
    }
}


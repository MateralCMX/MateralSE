namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;

    [IngameObjective("IngameHelp_MagneticBoots", 160)]
    internal class MyIngameHelpMagneticBoots : MyIngameHelpObjective
    {
        private Queue<float> m_averageGravity = new Queue<float>();

        public MyIngameHelpMagneticBoots()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_MagneticBoots_Title;
            base.RequiredIds = new string[] { "IngameHelp_Jetpack2" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.ZeroGravity));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_MagneticBoots_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_MagneticBoots_Detail2;
            detail2.FinishCondition = new Func<bool>(this.BootsLocked);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_MagneticBootsTip";
        }

        private bool BootsLocked()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && localCharacter.IsMagneticBootsActive);
        }

        private bool ZeroGravity()
        {
            int num = 5;
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if ((localCharacter == null) || (localCharacter.CurrentMovementState != MyCharacterMovementEnum.Flying))
            {
                return false;
            }
            this.m_averageGravity.Enqueue(localCharacter.Gravity.LengthSquared());
            if (this.m_averageGravity.Count < num)
            {
                return false;
            }
            if (this.m_averageGravity.Count > num)
            {
                this.m_averageGravity.Dequeue();
            }
            return (((IEnumerable<float>) this.m_averageGravity).Average() < 0.001f);
        }
    }
}


namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;

    [IngameObjective("IngameHelp_Ownership", 90)]
    internal class MyIngameHelpOwnership : MyIngameHelpObjective
    {
        private bool m_accessDeniedHappened;
        private bool m_blockHacked;
        private HashSet<MyTerminalBlock> m_hackingBlocks = new HashSet<MyTerminalBlock>();

        public MyIngameHelpOwnership()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Ownership_Title;
            base.RequiredIds = new string[] { "IngameHelp_Building" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.AccessDeniedHappened));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Ownership_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Ownership_Detail2;
            detail2.FinishCondition = new Func<bool>(this.BlockHackedCondition);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_OwnershipTip";
            MyHud.Notifications.OnNotificationAdded += new Action<MyNotificationSingletons>(this.Notifications_OnNotificationAdded);
        }

        private bool AccessDeniedHappened() => 
            this.m_accessDeniedHappened;

        private bool BlockHackedCondition() => 
            this.m_blockHacked;

        private void MyTerminalBlock_OnAnyBlockHackedChanged(MyTerminalBlock obj, long grinderOwner)
        {
            MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
            if ((!this.m_hackingBlocks.Contains(obj) && (controlledEntity != null)) && (controlledEntity.GetPlayerIdentityId() == grinderOwner))
            {
                this.m_hackingBlocks.Add(obj);
                obj.OwnershipChanged += new Action<MyTerminalBlock>(this.obj_OwnershipChanged);
            }
        }

        private void Notifications_OnNotificationAdded(MyNotificationSingletons obj)
        {
            if (obj == MyNotificationSingletons.AccessDenied)
            {
                this.m_accessDeniedHappened = true;
            }
        }

        private void obj_OwnershipChanged(MyTerminalBlock obj)
        {
            MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
            if ((controlledEntity != null) && (controlledEntity.GetPlayerIdentityId() == obj.OwnerId))
            {
                this.m_blockHacked = true;
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MySlimBlock.OnAnyBlockHackedChanged += new Action<MyTerminalBlock, long>(this.MyTerminalBlock_OnAnyBlockHackedChanged);
        }
    }
}


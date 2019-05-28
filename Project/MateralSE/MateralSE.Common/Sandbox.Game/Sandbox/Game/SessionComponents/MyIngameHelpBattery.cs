namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Battery", 650)]
    internal class MyIngameHelpBattery : MyIngameHelpObjective
    {
        private bool m_batteryAdded;

        public MyIngameHelpBattery()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Battery_Title;
            base.RequiredIds = new string[] { "IngameHelp_Camera" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Battery_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.RequiredCondition = new Func<bool>(this.BlockAddedCondition);
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_Battery2";
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.OnBlockAdded += new Action<MyCubeBlockDefinition>(this.Static_OnBlockAdded);
            }
        }

        private bool BlockAddedCondition() => 
            this.m_batteryAdded;

        private void Static_OnBlockAdded(MyCubeBlockDefinition definition)
        {
            this.m_batteryAdded = definition.Id.TypeId.ToString() == "MyObjectBuilder_BatteryBlock";
        }
    }
}


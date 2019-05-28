namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_BasicRefinery", 550)]
    internal class MyIngameHelpBasicRefinery : MyIngameHelpObjective
    {
        private bool m_refineryAdded;

        public MyIngameHelpBasicRefinery()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_BasicRefinery_Title;
            base.RequiredIds = new string[] { "IngameHelp_Camera" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_BasicRefinery_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.RequiredCondition = new Func<bool>(this.BlockAddedCondition);
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.OnBlockAdded += new Action<MyCubeBlockDefinition>(this.Static_OnBlockAdded);
            }
        }

        private bool BlockAddedCondition() => 
            this.m_refineryAdded;

        private void Static_OnBlockAdded(MyCubeBlockDefinition definition)
        {
            this.m_refineryAdded = (definition.Id.SubtypeName == "Blast Furnace") || (definition.Id.SubtypeName == "BasicAssembler");
        }
    }
}


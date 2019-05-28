namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Turbine", 500)]
    internal class MyIngameHelpTurbine : MyIngameHelpObjective
    {
        private bool m_turbineAdded;

        public MyIngameHelpTurbine()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Turbine_Title;
            base.RequiredIds = new string[] { "IngameHelp_Camera" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Turbine_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.RequiredCondition = new Func<bool>(this.BlockAddedCondition);
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.OnBlockAdded += new Action<MyCubeBlockDefinition>(this.Static_OnBlockAdded);
            }
            base.FollowingId = "IngameHelp_Turbine2";
        }

        private bool BlockAddedCondition() => 
            this.m_turbineAdded;

        private void Static_OnBlockAdded(MyCubeBlockDefinition definition)
        {
            int num1;
            if (definition.Id.TypeId.ToString() != "MyObjectBuilder_WindTurbine")
            {
                num1 = (int) (definition.Id.TypeId.ToString() == "MyObjectBuilder_SolarPanel");
            }
            else
            {
                num1 = 1;
            }
            this.m_turbineAdded = (bool) num1;
        }
    }
}


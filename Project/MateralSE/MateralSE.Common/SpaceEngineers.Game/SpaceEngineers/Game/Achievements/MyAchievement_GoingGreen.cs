namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;

    public class MyAchievement_GoingGreen : MySteamAchievementBase
    {
        private int m_solarPanelsBuilt;
        private const string SolarPanelsBuiltStatName = "GoingGreen_SolarPanelsBuilt";

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_solarPanelsBuilt = MyGameService.GetStatInt("GoingGreen_SolarPanelsBuilt");
            }
        }

        private void MyCubeGridsOnBlockBuilt(MyCubeGrid myCubeGrid, MySlimBlock mySlimBlock)
        {
            if ((((((MySession.Static != null) && (mySlimBlock != null)) && (mySlimBlock.FatBlock != null)) && !MySession.Static.CreativeMode) && (mySlimBlock.BuiltBy == MySession.Static.LocalPlayerId)) && (mySlimBlock.FatBlock is MySolarPanel))
            {
                this.m_solarPanelsBuilt++;
                MyGameService.SetStat("GoingGreen_SolarPanelsBuilt", this.m_solarPanelsBuilt);
                if (this.m_solarPanelsBuilt >= 0x19)
                {
                    base.NotifyAchieved();
                    MyCubeGrids.BlockBuilt -= new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockBuilt);
                }
            }
        }

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                MyCubeGrids.BlockBuilt += new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockBuilt);
            }
        }

        public override string AchievementTag =>
            "MyAchievement_GoingGreen";

        public override bool NeedsUpdate =>
            false;
    }
}


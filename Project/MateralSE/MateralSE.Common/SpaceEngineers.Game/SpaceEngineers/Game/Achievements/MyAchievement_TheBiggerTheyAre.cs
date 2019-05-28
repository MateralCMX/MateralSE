namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;

    internal class MyAchievement_TheBiggerTheyAre : MySteamAchievementBase
    {
        private const float BUILT_BLOCK_MASS_KG = 1000000f;
        private int m_massBuilt;
        private const string MassBuiltStatName = "TheBiggerTheyAre_MassBuilt";

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_massBuilt = MyGameService.GetStatInt("TheBiggerTheyAre_MassBuilt");
            }
        }

        private void MyCubeGridsOnBlockBuilt(MyCubeGrid myCubeGrid, MySlimBlock mySlimBlock)
        {
            if (!MySession.Static.CreativeMode)
            {
                this.m_massBuilt += (int) mySlimBlock.GetMass();
                if (this.m_massBuilt < 1000000f)
                {
                    MyGameService.SetStat("TheBiggerTheyAre_MassBuilt", this.m_massBuilt);
                }
                if (this.m_massBuilt >= 1000000f)
                {
                    MyGameService.SetStat("TheBiggerTheyAre_MassBuilt", (float) 1000000f);
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
            "MyAchievement_TheBiggerTheyAre";

        public override bool NeedsUpdate =>
            false;
    }
}


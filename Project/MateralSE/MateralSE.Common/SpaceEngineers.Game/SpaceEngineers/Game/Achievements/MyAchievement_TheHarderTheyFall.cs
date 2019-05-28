namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;

    internal class MyAchievement_TheHarderTheyFall : MySteamAchievementBase
    {
        private const float DESTROY_BLOCK_MASS_KG = 1000000f;
        private float m_massDestroyed;
        private const string MassDestroyedStatName = "TheHarderTheyFall_MassDestroyed";

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_massDestroyed = MyGameService.GetStatInt("TheHarderTheyFall_MassDestroyed");
            }
        }

        private void MyCubeGridsOnBlockDestroyed(MyCubeGrid myCubeGrid, MySlimBlock mySlimBlock)
        {
            if (!MySession.Static.CreativeMode)
            {
                this.m_massDestroyed += mySlimBlock.GetMass();
                MyGameService.SetStat("TheHarderTheyFall_MassDestroyed", (int) Math.Floor((double) this.m_massDestroyed));
                if (this.m_massDestroyed >= 1000000f)
                {
                    base.NotifyAchieved();
                    MyCubeGrids.BlockDestroyed -= new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockDestroyed);
                }
            }
        }

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                MyCubeGrids.BlockDestroyed += new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockDestroyed);
            }
        }

        public override string AchievementTag =>
            "MyAchievement_TheHarderTheyFall";

        public override bool NeedsUpdate =>
            false;
    }
}


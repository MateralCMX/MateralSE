namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;

    public class MyAchievement_Monolith : MySteamAchievementBase
    {
        private const uint UPDATE_INTERVAL_S = 3;
        private bool m_globalConditions;
        private uint m_lastTimeUpdatedS;
        private readonly List<MyCubeGrid> m_monolithGrids = new List<MyCubeGrid>();

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                this.m_globalConditions = !MySession.Static.CreativeMode;
                if (this.m_globalConditions)
                {
                    this.m_lastTimeUpdatedS = 0;
                    this.m_monolithGrids.Clear();
                    foreach (MyCubeGrid grid in MyEntities.GetEntities())
                    {
                        if (grid == null)
                        {
                            continue;
                        }
                        if ((grid.BlocksCount == 1) && (grid.CubeBlocks.FirstElement<MySlimBlock>().BlockDefinition.Id.SubtypeId == MyStringHash.GetOrCompute("Monolith")))
                        {
                            this.m_monolithGrids.Add(grid);
                        }
                    }
                }
            }
        }

        public override void SessionUpdate()
        {
            if (!base.IsAchieved && (MySession.Static.LocalCharacter != null))
            {
                this.m_lastTimeUpdatedS = (uint) MySession.Static.ElapsedPlayTime.TotalSeconds;
                if (MySession.Static.LocalCharacter != null)
                {
                    Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
                    foreach (MyCubeGrid grid in this.m_monolithGrids)
                    {
                        Vector3D center = grid.PositionComp.WorldVolume.Center;
                        if (Vector3D.DistanceSquared(position, center) < (400.0 + grid.PositionComp.WorldVolume.Radius))
                        {
                            base.NotifyAchieved();
                            break;
                        }
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_Monolith";

        public override bool NeedsUpdate
        {
            get
            {
                if (!this.m_globalConditions)
                {
                    return false;
                }
                return ((((uint) MySession.Static.ElapsedPlayTime.TotalSeconds) - this.m_lastTimeUpdatedS) > 3);
            }
        }
    }
}


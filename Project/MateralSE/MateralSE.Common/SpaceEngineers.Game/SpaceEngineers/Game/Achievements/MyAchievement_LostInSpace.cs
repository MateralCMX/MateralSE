namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyAchievement_LostInSpace : MySteamAchievementBase
    {
        public const string StatNameTag = "LostInSpace_LostInSpaceStartedS";
        public const int CHECK_INTERVAL_MS = 0xbb8;
        public const int MAXIMUM_TIME_S = 0xe10;
        private int m_startedS;
        private double m_lastTimeChecked;
        private bool m_conditionsValid;
        private readonly List<MyPhysics.HitInfo> m_hitInfoResults = new List<MyPhysics.HitInfo>();

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_startedS = MyGameService.GetStatInt("LostInSpace_LostInSpaceStartedS");
                this.m_lastTimeChecked = 0.0;
            }
        }

        private bool IsThePlayerInSight(MyPlayer player)
        {
            if (player.Character == null)
            {
                return false;
            }
            if (MySession.Static.LocalCharacter == null)
            {
                return false;
            }
            Vector3D position = player.Character.PositionComp.GetPosition();
            Vector3D vectord2 = MySession.Static.LocalCharacter.PositionComp.GetPosition();
            if (Vector3D.DistanceSquared(position, vectord2) > 4000000.0)
            {
                return false;
            }
            this.m_hitInfoResults.Clear();
            MyPhysics.CastRay(position, vectord2, this.m_hitInfoResults, 0);
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = this.m_hitInfoResults.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!(enumerator.Current.HkHitInfo.GetHitEntity() is MyCharacter))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void SessionLoad()
        {
            this.m_conditionsValid = MyMultiplayer.Static != null;
            this.m_lastTimeChecked = 0.0;
        }

        public override void SessionUpdate()
        {
            if (this.m_conditionsValid)
            {
                double totalMilliseconds = MySession.Static.ElapsedPlayTime.TotalMilliseconds;
                double num2 = totalMilliseconds - this.m_lastTimeChecked;
                if (num2 > 3000.0)
                {
                    this.m_lastTimeChecked = totalMilliseconds;
                    this.m_startedS += (int) (num2 / 1000.0);
                    if (MySession.Static.Players.GetOnlinePlayerCount() == 1)
                    {
                        this.m_startedS = 0;
                    }
                    else
                    {
                        foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                        {
                            if (!ReferenceEquals(player, MySession.Static.LocalHumanPlayer) && this.IsThePlayerInSight(player))
                            {
                                this.m_startedS = 0;
                                break;
                            }
                        }
                        MyGameService.SetStat("LostInSpace_LostInSpaceStartedS", this.m_startedS);
                        if (this.m_startedS > 0xe10)
                        {
                            base.NotifyAchieved();
                        }
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_LostInSpace";

        public override bool NeedsUpdate =>
            this.m_conditionsValid;
    }
}


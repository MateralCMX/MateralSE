namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.ModAPI;
    using VRageMath;

    internal class MyAchievement_GiantLeapForMankind : MySteamAchievementBase
    {
        private const double CHECK_INTERVAL_S = 3.0;
        private const int DISTANCE_TO_BE_WALKED = 0x7b1;
        private const string WalkedMoonStatName = "GiantLeapForMankind_WalkedMoon";
        private float m_metersWalkedOnMoon;
        private float m_storedMetersWalkedOnMoon;
        private List<MyPhysics.HitInfo> m_hits = new List<MyPhysics.HitInfo>();
        private double m_lastCheckS;

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_metersWalkedOnMoon = MyGameService.GetStatFloat("GiantLeapForMankind_WalkedMoon");
            }
        }

        private bool IsWalkingOnMoon(MyCharacter character)
        {
            float num = MyConstants.DEFAULT_GROUND_SEARCH_DISTANCE;
            Vector3D from = character.PositionComp.GetPosition() + (character.PositionComp.WorldMatrix.Up * 0.5);
            MyPhysics.CastRay(from, from + (character.PositionComp.WorldMatrix.Down * num), this.m_hits, 0x12);
            int num2 = 0;
            while ((num2 < this.m_hits.Count) && ((this.m_hits[num2].HkHitInfo.Body == null) || ReferenceEquals(this.m_hits[num2].HkHitInfo.GetHitEntity(), character.Components)))
            {
                num2++;
            }
            if (this.m_hits.Count != 0)
            {
                if (num2 < this.m_hits.Count)
                {
                    MyPhysics.HitInfo local1 = this.m_hits[num2];
                    IMyEntity hitEntity = local1.HkHitInfo.GetHitEntity();
                    if (Vector3D.DistanceSquared(local1.Position, from) < (num * num))
                    {
                        MyVoxelBase base2 = hitEntity as MyVoxelBase;
                        if (((base2 != null) && ((base2.Storage != null) && (base2.Storage.DataProvider != null))) && (base2.Storage.DataProvider is MyPlanetStorageProvider))
                        {
                            MyPlanetStorageProvider dataProvider = base2.Storage.DataProvider as MyPlanetStorageProvider;
                            if ((dataProvider.Generator != null) && (dataProvider.Generator.FolderName == "Moon"))
                            {
                                this.m_hits.Clear();
                                return true;
                            }
                        }
                    }
                }
                this.m_hits.Clear();
            }
            return false;
        }

        public override void SessionBeforeStart()
        {
            this.m_lastCheckS = 0.0;
        }

        public override void SessionUpdate()
        {
            if (((MySession.Static != null) && (MySession.Static.LocalCharacter != null)) && (MySession.Static.LocalCharacter.Physics != null))
            {
                double num = MySession.Static.ElapsedPlayTime.TotalSeconds - this.m_lastCheckS;
                if (num >= 3.0)
                {
                    this.m_lastCheckS = MySession.Static.ElapsedPlayTime.TotalSeconds;
                    double num2 = MySession.Static.LocalCharacter.Physics.LinearVelocity.Length();
                    if (MyCharacter.IsWalkingState(MySession.Static.LocalCharacter.GetCurrentMovementState()) || MyCharacter.IsRunningState(MySession.Static.LocalCharacter.GetCurrentMovementState()))
                    {
                        MySession.Static.LocalCharacter.PositionComp.GetPosition();
                        if (this.IsWalkingOnMoon(MySession.Static.LocalCharacter))
                        {
                            this.m_metersWalkedOnMoon += (float) (num * num2);
                            MyGameService.SetStat("GiantLeapForMankind_WalkedMoon", this.m_metersWalkedOnMoon);
                            this.m_storedMetersWalkedOnMoon = this.m_metersWalkedOnMoon;
                            if (this.m_metersWalkedOnMoon >= 1969f)
                            {
                                base.NotifyAchieved();
                            }
                        }
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_GiantLeapForMankind";

        public override bool NeedsUpdate =>
            !base.IsAchieved;
    }
}


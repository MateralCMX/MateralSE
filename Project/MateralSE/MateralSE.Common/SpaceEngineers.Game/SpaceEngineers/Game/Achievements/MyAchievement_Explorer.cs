namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;

    public class MyAchievement_Explorer : MySteamAchievementBase
    {
        private const uint CHECK_INTERVAL_S = 3;
        private const uint PLANET_COUNT = 6;
        public const string StatNameTag = "Explorer_ExplorePlanetsData";
        public const string PlanetCountNameTag = "Explorer_PlanetsCount";
        private BitArray m_exploredPlanetData;
        private readonly int[] m_bitArrayConversionArray = new int[1];
        private int PlanetDiscovered;
        private uint m_lastCheckS;
        private readonly Dictionary<MyStringHash, int> m_planetNamesToIndexes = new Dictionary<MyStringHash, int>();
        private bool m_globalConditionsMet;

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.ReadSteamData();
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("Alien"), 0);
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("EarthLike"), 1);
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("Europa"), 2);
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("Mars"), 3);
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("Moon"), 4);
                this.m_planetNamesToIndexes.Add(MyStringHash.GetOrCompute("Titan"), 5);
            }
        }

        private void ReadSteamData()
        {
            int statInt = MyGameService.GetStatInt("Explorer_ExplorePlanetsData");
            int[] values = new int[] { statInt };
            this.m_exploredPlanetData = new BitArray(values);
        }

        public override void SessionLoad()
        {
            this.m_globalConditionsMet = !MySession.Static.CreativeMode;
            this.m_lastCheckS = 0;
        }

        public override void SessionUpdate()
        {
            if ((MySession.Static.LocalCharacter != null) && !base.IsAchieved)
            {
                uint totalSeconds = (uint) MySession.Static.ElapsedPlayTime.TotalSeconds;
                if (((totalSeconds - this.m_lastCheckS) > 3) && (MySession.Static.LocalCharacter != null))
                {
                    Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
                    this.m_lastCheckS = totalSeconds;
                    if (MyGravityProviderSystem.CalculateNaturalGravityInPoint(position) != Vector3.Zero)
                    {
                        int num2;
                        MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                        if (((closestPlanet != null) && this.m_planetNamesToIndexes.TryGetValue(closestPlanet.Generator.Id.SubtypeId, out num2)) && !this.m_exploredPlanetData[num2])
                        {
                            this.m_exploredPlanetData[num2] = true;
                            this.PlanetDiscovered = 0;
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= 6L)
                                {
                                    this.StoreSteamData();
                                    if (this.PlanetDiscovered >= 6L)
                                    {
                                        base.NotifyAchieved();
                                        break;
                                    }
                                    MyGameService.IndicateAchievementProgress(this.AchievementTag, (uint) this.PlanetDiscovered, 6);
                                    return;
                                }
                                if (this.m_exploredPlanetData[num3])
                                {
                                    this.PlanetDiscovered++;
                                }
                                num3++;
                            }
                        }
                    }
                }
            }
        }

        private void StoreSteamData()
        {
            this.m_exploredPlanetData.CopyTo(this.m_bitArrayConversionArray, 0);
            MyGameService.SetStat("Explorer_ExplorePlanetsData", this.m_bitArrayConversionArray[0]);
            MyGameService.SetStat("Explorer_PlanetsCount", this.PlanetDiscovered);
        }

        public override string AchievementTag =>
            "MyAchievement_Explorer";

        public override bool NeedsUpdate =>
            this.m_globalConditionsMet;
    }
}


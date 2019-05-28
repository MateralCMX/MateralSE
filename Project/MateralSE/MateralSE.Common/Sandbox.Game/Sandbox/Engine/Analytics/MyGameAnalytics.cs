namespace Sandbox.Engine.Analytics
{
    using GameAnalyticsSDK.Net;
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyGameAnalytics : IMyAnalytics
    {
        private static readonly object m_singletonGuard = new object();
        private static volatile MyGameAnalytics m_instance;
        private const string GameProgression01 = "Game";

        public MyGameAnalytics(string accessToken, string gameVersion)
        {
            GameAnalytics.ConfigureBuild(gameVersion);
            if (!string.IsNullOrWhiteSpace(accessToken) && accessToken.Contains<char>(':'))
            {
                char[] separator = new char[] { ':' };
                string[] strArray = accessToken.Split(separator);
                if (strArray.Length == 2)
                {
                    GameAnalytics.Initialize(strArray[0], strArray[1]);
                }
            }
        }

        public void FlushAndDispose()
        {
            GameAnalytics.OnStop();
        }

        public static MyGameAnalytics GetOrCreateInstance(string accessToken, string gameVersion)
        {
            object singletonGuard = m_singletonGuard;
            lock (singletonGuard)
            {
                if (m_instance == null)
                {
                    m_instance = new MyGameAnalytics(accessToken, gameVersion);
                }
            }
            return m_instance;
        }

        public static ulong GetTestValue() => 
            (!MyGameService.IsActive ? 0UL : (MyGameService.UserId % ((ulong) 2L)));

        public void IdentifyPlayer(string playerId, Dictionary<string, object> identificationData)
        {
        }

        public void ReportActivityEnd(string activityName, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f)
        {
        }

        public void ReportActivityStart(string activityName, string activityFocus, string activityType, string activityItemUsage, bool expectActivityEnd = true, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f)
        {
        }

        public void ReportCampaignProgression(string campaignName, string levelName, string completedState, int timeFromLastCompletion)
        {
        }

        public void ReportEvent(MyAnalyticsProgressionStatus status, Dictionary<string, object> eventData, double timestamp = 0.0)
        {
            object obj2;
            ulong testValue = GetTestValue();
            string str = "Test " + testValue;
            switch (status)
            {
                case MyAnalyticsProgressionStatus.GameQuit:
                    GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Complete, str, status.ToString());
                    return;

                case MyAnalyticsProgressionStatus.GameCrash:
                    object obj3;
                    if (eventData.TryGetValue("exception", out obj3))
                    {
                        GameAnalytics.AddErrorEvent(EGAErrorSeverity.Critical, obj3.ToString());
                        return;
                    }
                    return;

                case MyAnalyticsProgressionStatus.WorldStart:
                    if (eventData.TryGetValue("world_type", out obj2))
                    {
                        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, str, obj2.ToString());
                    }
                    else
                    {
                        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, "Game", "UnknownWorld");
                    }
                    foreach (KeyValuePair<string, object> pair in eventData)
                    {
                        if (pair.Value != null)
                        {
                            GameAnalytics.AddErrorEvent(EGAErrorSeverity.Info, $"{pair.Key}: {pair.Value}");
                        }
                    }
                    return;

                case MyAnalyticsProgressionStatus.WorldEnd:
                    break;

                default:
                    if (status == MyAnalyticsProgressionStatus.BugReport)
                    {
                        object obj4;
                        if (!eventData.TryGetValue("data", out obj4))
                        {
                            obj4 = "[Null]";
                        }
                        GameAnalytics.AddErrorEvent(EGAErrorSeverity.Info, obj4.ToString());
                        return;
                    }
                    else
                    {
                        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Undefined, "Game", status.ToString());
                    }
                    return;
            }
            if (eventData.TryGetValue("world_type", out obj2))
            {
                GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Complete, str, obj2.ToString());
            }
            else
            {
                GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Complete, "Game", "UnknownWorld");
            }
            foreach (KeyValuePair<string, object> pair2 in eventData)
            {
                if (pair2.Value != null)
                {
                    GameAnalytics.AddErrorEvent(EGAErrorSeverity.Info, $"{pair2.Key}: {pair2.Value}");
                }
            }
        }

        public void ReportModLoaded(string name)
        {
            GameAnalytics.AddErrorEvent(EGAErrorSeverity.Info, "mod_used: " + name);
        }

        public void ReportPlayerDeath(string deathType, string deathCause, string planetName = "", string planetType = "", bool campaign = false, bool official = false, string gameMode = "Survival", string modList = "", int modCount = 0)
        {
        }

        public void ReportScreenMouseClick(string screen, float positionX, float positionY, uint frame)
        {
        }

        public void ReportServerStatus(int playerCount, int maxPlayers, float simSpeedServer, int entitiesCount, int gridsCount, int blocksCount, int movingGridsCount, string hostName, string worldType, string worldName, uint worldAge)
        {
            GameAnalytics.AddDesignEvent("world_age", (double) worldAge);
            GameAnalytics.AddDesignEvent("player_count", (double) playerCount);
            GameAnalytics.AddDesignEvent("max_players", (double) maxPlayers);
            GameAnalytics.AddDesignEvent("sim_speed", (double) simSpeedServer);
            GameAnalytics.AddDesignEvent("entities_count", (double) entitiesCount);
            GameAnalytics.AddDesignEvent("grids_count", (double) gridsCount);
            GameAnalytics.AddDesignEvent("blocks_count", (double) blocksCount);
            GameAnalytics.AddDesignEvent("moving_grids_count", (double) movingGridsCount);
        }

        public void ReportSessionEnd(Dictionary<string, object> sessionData)
        {
            GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Complete, "Game");
        }

        public void ReportSessionStart(Dictionary<string, object> sessionData)
        {
            GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, "Game");
            foreach (KeyValuePair<string, object> pair in sessionData)
            {
                if (pair.Value != null)
                {
                    GameAnalytics.AddErrorEvent(EGAErrorSeverity.Info, $"{pair.Key}: {pair.Value}");
                }
            }
        }

        public void ReportToolbarPageSwitch(int page)
        {
        }

        public void ReportUsedNamespace(string namespaceName, int count)
        {
        }

        public void UpdatePlayerIdentity(Dictionary<string, object> identificationData)
        {
        }
    }
}


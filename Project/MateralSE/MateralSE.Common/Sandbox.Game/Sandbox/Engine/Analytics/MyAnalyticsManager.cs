namespace Sandbox.Engine.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class MyAnalyticsManager
    {
        private static MyAnalyticsManager m_instance;
        private List<IMyAnalytics> m_trackers = new List<IMyAnalytics>();

        [Conditional("WINDOWS")]
        internal void EndSession(Dictionary<string, object> sessionData)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportSessionEnd(sessionData);
                }
            }
        }

        public void FlushAndDispose()
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.FlushAndDispose();
                }
            }
            this.m_trackers.Clear();
        }

        [Conditional("WINDOWS")]
        internal void IdentifyPlayer(string playerId, string playerName, bool isSteamOnline, Dictionary<string, object> identificationData)
        {
            if (identificationData == null)
            {
                identificationData = new Dictionary<string, object>();
            }
            identificationData["player_name"] = playerName;
            identificationData["is_steam_online"] = isSteamOnline;
            identificationData["last_login"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.IdentifyPlayer(playerId, identificationData);
                }
            }
        }

        public void RegisterAnalyticsTracker(IMyAnalytics tracker)
        {
            if (((tracker != null) && (this.m_trackers != null)) && !this.m_trackers.Contains(tracker))
            {
                this.m_trackers.Add(tracker);
            }
        }

        internal void ReportActivityEnd(string activityName, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportActivityEnd(activityName, planetName, planetType, simSpeedPlayer, simSpeedServer);
                }
            }
        }

        internal void ReportActivityStart(string activityName, string activityFocus, string activityType, string activityItemUsage, bool expectActivityEnd, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportActivityStart(activityName, activityFocus, activityType, activityItemUsage, expectActivityEnd, planetName, planetType, simSpeedPlayer, simSpeedServer);
                }
            }
        }

        internal void ReportCampaignProgression(string campaignName, string levelName, string completedState, int timeFromLastCompletion)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportCampaignProgression(campaignName, levelName, completedState, timeFromLastCompletion);
                }
            }
        }

        [Conditional("WINDOWS")]
        internal void ReportEvent(MyAnalyticsProgressionStatus status, Dictionary<string, object> eventData, double timestamp = 0.0)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportEvent(status, eventData, timestamp);
                }
            }
        }

        internal void ReportModLoaded(string name)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportModLoaded(name);
                }
            }
        }

        internal void ReportPlayerDeath(string deathType, string deathCause, string planetName, string planetType, bool campaign, bool official, string gameMode, string modList, int modCount)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportPlayerDeath(deathType, deathCause, planetName, planetType, campaign, official, gameMode, modList, modCount);
                }
            }
        }

        internal void ReportScreenMouseClick(string screen, float x, float y, uint seconds)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportScreenMouseClick(screen, x, y, seconds);
                }
            }
        }

        internal void ReportServerStatus(int playerCount, int maxPlayers, float simSpeedServer, int entitiesCount, int gridCount, int blockCount, int movingGridsCount, string hostName, string worldType, string worldName, uint worldAge)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportServerStatus(playerCount, maxPlayers, simSpeedServer, entitiesCount, gridCount, blockCount, movingGridsCount, hostName, worldType, worldName, worldAge);
                }
            }
        }

        internal void ReportToolbarPageSwitch(int page)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportToolbarPageSwitch(page);
                }
            }
        }

        internal void ReportUsedNamespace(string namespaceName, int count)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportUsedNamespace(namespaceName, count);
                }
            }
        }

        [Conditional("WINDOWS")]
        internal void StartSession(Dictionary<string, object> sessionData)
        {
            using (List<IMyAnalytics>.Enumerator enumerator = this.m_trackers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReportSessionStart(sessionData);
                }
            }
        }

        public void UnregisterAnalyticsTracker(IMyAnalytics tracker)
        {
            if (((tracker != null) && (this.m_trackers != null)) && this.m_trackers.Contains(tracker))
            {
                this.m_trackers.Remove(tracker);
            }
        }

        public static MyAnalyticsManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyAnalyticsManager();
                }
                return m_instance;
            }
        }
    }
}


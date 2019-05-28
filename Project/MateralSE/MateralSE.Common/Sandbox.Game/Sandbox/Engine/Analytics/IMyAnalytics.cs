namespace Sandbox.Engine.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IMyAnalytics
    {
        void FlushAndDispose();
        void IdentifyPlayer(string playerId, Dictionary<string, object> identificationData);
        void ReportActivityEnd(string activityName, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f);
        void ReportActivityStart(string activityName, string activityFocus, string activityType, string activityItemUsage, bool expectActivityEnd = true, string planetName = "", string planetType = "", float simSpeedPlayer = 1f, float simSpeedServer = 1f);
        void ReportCampaignProgression(string campaignName, string levelName, string completedState, int timeFromLastCompletion);
        void ReportEvent(MyAnalyticsProgressionStatus status, Dictionary<string, object> eventData, double timestamp = 0.0);
        void ReportModLoaded(string name);
        void ReportPlayerDeath(string deathType, string deathCause, string planetName = "", string planetType = "", bool campaign = false, bool official = false, string gameMode = "Survival", string modList = "", int modCount = 0);
        void ReportScreenMouseClick(string screen, float positionX, float positionY, uint frame);
        void ReportServerStatus(int playerCount, int maxPlayers, float simSpeedServer, int entitiesCount, int gridsCount, int blocksCount, int movingGridsCount, string hostName, string worldType, string worldName, uint worldAge);
        void ReportSessionEnd(Dictionary<string, object> sessionData);
        void ReportSessionStart(Dictionary<string, object> sessionData);
        void ReportToolbarPageSwitch(int page);
        void ReportUsedNamespace(string namespaceName, int count);
    }
}


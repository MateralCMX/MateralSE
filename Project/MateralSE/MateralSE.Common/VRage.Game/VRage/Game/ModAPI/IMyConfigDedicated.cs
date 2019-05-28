namespace VRage.Game.ModAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public interface IMyConfigDedicated
    {
        void GenerateRemoteSecurityKey();
        string GetFilePath();
        void Load(string path = null);
        void Save(string path = null);
        void SetPassword(string password);

        List<string> Administrators { get; set; }

        int AsteroidAmount { get; set; }

        List<ulong> Banned { get; set; }

        List<ulong> Reserved { get; set; }

        ulong GroupID { get; set; }

        bool IgnoreLastSession { get; set; }

        string IP { get; set; }

        string LoadWorld { get; set; }

        bool PauseGameWhenEmpty { get; set; }

        string MessageOfTheDay { get; set; }

        string MessageOfTheDayUrl { get; set; }

        bool AutoRestartEnabled { get; set; }

        int AutoRestatTimeInMin { get; set; }

        bool AutoUpdateEnabled { get; set; }

        int AutoUpdateCheckIntervalInMin { get; set; }

        int AutoUpdateRestartDelayInMin { get; set; }

        bool AutoRestartSave { get; set; }

        string AutoUpdateSteamBranch { get; set; }

        string AutoUpdateBranchPassword { get; set; }

        string ServerName { get; set; }

        int ServerPort { get; set; }

        MyObjectBuilder_SessionSettings SessionSettings { get; set; }

        int SteamPort { get; set; }

        string WorldName { get; set; }

        string PremadeCheckpointPath { get; set; }

        string ServerDescription { get; set; }

        string ServerPasswordHash { get; set; }

        string ServerPasswordSalt { get; set; }

        bool RemoteApiEnabled { get; set; }

        string RemoteSecurityKey { get; set; }

        int RemoteApiPort { get; set; }

        List<string> Plugins { get; set; }

        float WatcherInterval { get; set; }

        float WatcherSimulationSpeedMinimum { get; set; }

        int ManualActionDelay { get; set; }

        string ManualActionChatMessage { get; set; }

        bool AutodetectDependencies { get; set; }

        bool SaveChatToLog { get; set; }
    }
}


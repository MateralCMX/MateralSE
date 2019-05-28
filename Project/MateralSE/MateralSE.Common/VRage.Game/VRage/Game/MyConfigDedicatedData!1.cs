namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("MyConfigDedicated")]
    public class MyConfigDedicatedData<T> where T: MyObjectBuilder_SessionSettings, new()
    {
        public T SessionSettings;
        public string LoadWorld;
        public string IP;
        public int SteamPort;
        public int ServerPort;
        public int AsteroidAmount;
        [XmlArrayItem("unsignedLong")]
        public List<string> Administrators;
        public List<ulong> Banned;
        public ulong GroupID;
        public string ServerName;
        public string WorldName;
        public bool PauseGameWhenEmpty;
        public string MessageOfTheDay;
        public string MessageOfTheDayUrl;
        public bool AutoRestartEnabled;
        public int AutoRestatTimeInMin;
        public bool AutoRestartSave;
        public bool AutoUpdateEnabled;
        public int AutoUpdateCheckIntervalInMin;
        public int AutoUpdateRestartDelayInMin;
        public string AutoUpdateSteamBranch;
        public string AutoUpdateBranchPassword;
        public bool IgnoreLastSession;
        public string PremadeCheckpointPath;
        public string ServerDescription;
        public string ServerPasswordHash;
        public string ServerPasswordSalt;
        public List<ulong> Reserved;
        public bool RemoteApiEnabled;
        public string RemoteSecurityKey;
        public int RemoteApiPort;
        public List<string> Plugins;
        public float WatcherInterval;
        public float WatcherSimulationSpeedMinimum;
        public int ManualActionDelay;
        public string ManualActionChatMessage;
        public bool AutodetectDependencies;
        public bool SaveChatToLog;

        public MyConfigDedicatedData()
        {
            this.SessionSettings = Activator.CreateInstance<T>();
            this.IP = "0.0.0.0";
            this.SteamPort = 0x223e;
            this.ServerPort = 0x6988;
            this.AsteroidAmount = 4;
            this.Administrators = new List<string>();
            this.Banned = new List<ulong>();
            this.ServerName = "";
            this.WorldName = "";
            this.MessageOfTheDay = string.Empty;
            this.MessageOfTheDayUrl = string.Empty;
            this.AutoRestartEnabled = true;
            this.AutoRestartSave = true;
            this.AutoUpdateCheckIntervalInMin = 10;
            this.AutoUpdateRestartDelayInMin = 15;
            this.PremadeCheckpointPath = "";
            this.Reserved = new List<ulong>();
            this.RemoteApiEnabled = true;
            this.RemoteApiPort = 0x1f90;
            this.Plugins = new List<string>();
            this.WatcherInterval = 30f;
            this.WatcherSimulationSpeedMinimum = 0.05f;
            this.ManualActionDelay = 5;
            this.ManualActionChatMessage = "Server will be shut down in {0} min(s).";
            this.AutodetectDependencies = true;
        }
    }
}


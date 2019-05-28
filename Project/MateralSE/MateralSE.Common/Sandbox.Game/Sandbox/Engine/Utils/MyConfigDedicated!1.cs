namespace Sandbox.Engine.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public class MyConfigDedicated<T> : IMyConfigDedicated where T: MyObjectBuilder_SessionSettings, new()
    {
        private XmlSerializer m_serializer;
        private string m_fileName;
        private MyConfigDedicatedData<T> m_data;

        public MyConfigDedicated(string fileName)
        {
            this.m_fileName = fileName;
            try
            {
                this.m_serializer = new XmlSerializer(typeof(MyConfigDedicatedData<T>));
            }
            catch (Exception)
            {
            }
            this.SetDefault();
        }

        public void GenerateRemoteSecurityKey()
        {
            byte[] buffer;
            RandomNumberGenerator.Create().GetBytes(buffer = new byte[0x10]);
            this.RemoteSecurityKey = Convert.ToBase64String(buffer);
        }

        private void GenerateSalt()
        {
            byte[] buffer;
            RandomNumberGenerator.Create().GetBytes(buffer = new byte[0x10]);
            this.ServerPasswordSalt = Convert.ToBase64String(buffer);
        }

        public string GetFilePath() => 
            Path.Combine(MyFileSystem.UserDataPath, this.m_fileName);

        public void Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = this.GetFilePath();
            }
            if (!File.Exists(path))
            {
                this.SetDefault();
            }
            else
            {
                try
                {
                    using (FileStream stream = File.OpenRead(path))
                    {
                        this.m_data = (MyConfigDedicatedData<T>) this.m_serializer.Deserialize(stream);
                    }
                }
                catch (Exception exception)
                {
                    if (MyLog.Default != null)
                    {
                        MyLog.Default.WriteLine("Exception during DS config load: " + exception.ToString());
                    }
                    this.SetDefault();
                    return;
                }
                if (string.IsNullOrEmpty(this.ServerPasswordSalt) && string.IsNullOrEmpty(this.ServerPasswordHash))
                {
                    this.GenerateSalt();
                }
            }
        }

        public void Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = this.GetFilePath();
            }
            using (FileStream stream = File.Create(path))
            {
                this.m_serializer.Serialize((Stream) stream, this.m_data);
            }
        }

        private void SetDefault()
        {
            this.m_data = new MyConfigDedicatedData<T>();
            this.GenerateSalt();
            this.GenerateRemoteSecurityKey();
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(this.ServerPasswordSalt))
            {
                this.GenerateSalt();
            }
            byte[] salt = Convert.FromBase64String(this.ServerPasswordSalt);
            byte[] bytes = new Rfc2898DeriveBytes(password, salt, 0x2710).GetBytes(20);
            this.ServerPasswordHash = Convert.ToBase64String(bytes);
        }

        string IMyConfigDedicated.GetFilePath() => 
            this.GetFilePath();

        void IMyConfigDedicated.Save(string path = null)
        {
            this.Save(path);
        }

        public T SessionSettings
        {
            get => 
                this.m_data.SessionSettings;
            set => 
                (this.m_data.SessionSettings = value);
        }

        public string LoadWorld
        {
            get => 
                this.m_data.LoadWorld;
            set => 
                (this.m_data.LoadWorld = value);
        }

        public string IP
        {
            get => 
                this.m_data.IP;
            set => 
                (this.m_data.IP = value);
        }

        public int SteamPort
        {
            get => 
                this.m_data.SteamPort;
            set => 
                (this.m_data.SteamPort = value);
        }

        public int ServerPort
        {
            get => 
                this.m_data.ServerPort;
            set => 
                (this.m_data.ServerPort = value);
        }

        public int AsteroidAmount
        {
            get => 
                this.m_data.AsteroidAmount;
            set => 
                (this.m_data.AsteroidAmount = value);
        }

        public ulong GroupID
        {
            get => 
                this.m_data.GroupID;
            set => 
                (this.m_data.GroupID = value);
        }

        public List<string> Administrators
        {
            get => 
                this.m_data.Administrators;
            set => 
                (this.m_data.Administrators = value);
        }

        public List<ulong> Banned
        {
            get => 
                this.m_data.Banned;
            set => 
                (this.m_data.Banned = value);
        }

        public List<ulong> Reserved
        {
            get => 
                this.m_data.Reserved;
            set => 
                (this.m_data.Reserved = value);
        }

        public string ServerName
        {
            get => 
                this.m_data.ServerName;
            set => 
                (this.m_data.ServerName = value);
        }

        public string WorldName
        {
            get => 
                this.m_data.WorldName;
            set => 
                (this.m_data.WorldName = value);
        }

        public string PremadeCheckpointPath
        {
            get => 
                this.m_data.PremadeCheckpointPath;
            set => 
                (this.m_data.PremadeCheckpointPath = value);
        }

        public bool PauseGameWhenEmpty
        {
            get => 
                this.m_data.PauseGameWhenEmpty;
            set => 
                (this.m_data.PauseGameWhenEmpty = value);
        }

        public string MessageOfTheDay
        {
            get => 
                this.m_data.MessageOfTheDay;
            set => 
                (this.m_data.MessageOfTheDay = value);
        }

        public string MessageOfTheDayUrl
        {
            get => 
                this.m_data.MessageOfTheDayUrl;
            set => 
                (this.m_data.MessageOfTheDayUrl = value);
        }

        public bool AutoRestartEnabled
        {
            get => 
                this.m_data.AutoRestartEnabled;
            set => 
                (this.m_data.AutoRestartEnabled = value);
        }

        public int AutoRestatTimeInMin
        {
            get => 
                this.m_data.AutoRestatTimeInMin;
            set => 
                (this.m_data.AutoRestatTimeInMin = value);
        }

        public bool AutoRestartSave
        {
            get => 
                this.m_data.AutoRestartSave;
            set => 
                (this.m_data.AutoRestartSave = value);
        }

        public bool AutoUpdateEnabled
        {
            get => 
                this.m_data.AutoUpdateEnabled;
            set => 
                (this.m_data.AutoUpdateEnabled = value);
        }

        public int AutoUpdateCheckIntervalInMin
        {
            get => 
                this.m_data.AutoUpdateCheckIntervalInMin;
            set => 
                (this.m_data.AutoUpdateCheckIntervalInMin = value);
        }

        public int AutoUpdateRestartDelayInMin
        {
            get => 
                this.m_data.AutoUpdateRestartDelayInMin;
            set => 
                (this.m_data.AutoUpdateRestartDelayInMin = value);
        }

        public string AutoUpdateSteamBranch
        {
            get => 
                this.m_data.AutoUpdateSteamBranch;
            set => 
                (this.m_data.AutoUpdateSteamBranch = value);
        }

        public string AutoUpdateBranchPassword
        {
            get => 
                this.m_data.AutoUpdateBranchPassword;
            set => 
                (this.m_data.AutoUpdateBranchPassword = value);
        }

        public bool IgnoreLastSession
        {
            get => 
                this.m_data.IgnoreLastSession;
            set => 
                (this.m_data.IgnoreLastSession = value);
        }

        public string ServerDescription
        {
            get => 
                this.m_data.ServerDescription;
            set => 
                (this.m_data.ServerDescription = value);
        }

        public string ServerPasswordHash
        {
            get => 
                this.m_data.ServerPasswordHash;
            set => 
                (this.m_data.ServerPasswordHash = value);
        }

        public string ServerPasswordSalt
        {
            get => 
                this.m_data.ServerPasswordSalt;
            set => 
                (this.m_data.ServerPasswordSalt = value);
        }

        public bool RemoteApiEnabled
        {
            get => 
                this.m_data.RemoteApiEnabled;
            set => 
                (this.m_data.RemoteApiEnabled = value);
        }

        public string RemoteSecurityKey
        {
            get => 
                this.m_data.RemoteSecurityKey;
            set => 
                (this.m_data.RemoteSecurityKey = value);
        }

        public int RemoteApiPort
        {
            get => 
                this.m_data.RemoteApiPort;
            set => 
                (this.m_data.RemoteApiPort = value);
        }

        public List<string> Plugins
        {
            get => 
                this.m_data.Plugins;
            set => 
                (this.m_data.Plugins = value);
        }

        public float WatcherInterval
        {
            get => 
                this.m_data.WatcherInterval;
            set => 
                (this.m_data.WatcherInterval = value);
        }

        public float WatcherSimulationSpeedMinimum
        {
            get => 
                this.m_data.WatcherSimulationSpeedMinimum;
            set => 
                (this.m_data.WatcherSimulationSpeedMinimum = value);
        }

        public int ManualActionDelay
        {
            get => 
                this.m_data.ManualActionDelay;
            set => 
                (this.m_data.ManualActionDelay = value);
        }

        public string ManualActionChatMessage
        {
            get => 
                this.m_data.ManualActionChatMessage;
            set => 
                (this.m_data.ManualActionChatMessage = value);
        }

        public bool AutodetectDependencies
        {
            get => 
                this.m_data.AutodetectDependencies;
            set => 
                (this.m_data.AutodetectDependencies = value);
        }

        List<string> IMyConfigDedicated.Administrators
        {
            get => 
                this.Administrators;
            set => 
                (this.Administrators = value);
        }

        int IMyConfigDedicated.AsteroidAmount
        {
            get => 
                this.AsteroidAmount;
            set => 
                (this.AsteroidAmount = value);
        }

        List<ulong> IMyConfigDedicated.Banned
        {
            get => 
                this.Banned;
            set => 
                (this.Banned = value);
        }

        List<ulong> IMyConfigDedicated.Reserved
        {
            get => 
                this.Reserved;
            set => 
                (this.Reserved = value);
        }

        ulong IMyConfigDedicated.GroupID
        {
            get => 
                this.GroupID;
            set => 
                (this.GroupID = value);
        }

        string IMyConfigDedicated.LoadWorld
        {
            get => 
                this.LoadWorld;
            set => 
                (this.LoadWorld = value);
        }

        bool IMyConfigDedicated.PauseGameWhenEmpty
        {
            get => 
                this.PauseGameWhenEmpty;
            set => 
                (this.PauseGameWhenEmpty = value);
        }

        string IMyConfigDedicated.MessageOfTheDay
        {
            get => 
                this.MessageOfTheDay;
            set => 
                (this.MessageOfTheDay = value);
        }

        string IMyConfigDedicated.MessageOfTheDayUrl
        {
            get => 
                this.MessageOfTheDayUrl;
            set => 
                (this.MessageOfTheDayUrl = value);
        }

        bool IMyConfigDedicated.AutoRestartEnabled
        {
            get => 
                this.AutoRestartEnabled;
            set => 
                (this.AutoRestartEnabled = value);
        }

        int IMyConfigDedicated.AutoRestatTimeInMin
        {
            get => 
                this.AutoRestatTimeInMin;
            set => 
                (this.AutoRestatTimeInMin = value);
        }

        bool IMyConfigDedicated.AutoUpdateEnabled
        {
            get => 
                this.AutoUpdateEnabled;
            set => 
                (this.AutoUpdateEnabled = value);
        }

        int IMyConfigDedicated.AutoUpdateCheckIntervalInMin
        {
            get => 
                this.AutoUpdateCheckIntervalInMin;
            set => 
                (this.AutoUpdateCheckIntervalInMin = value);
        }

        int IMyConfigDedicated.AutoUpdateRestartDelayInMin
        {
            get => 
                this.AutoUpdateRestartDelayInMin;
            set => 
                (this.AutoUpdateRestartDelayInMin = value);
        }

        string IMyConfigDedicated.AutoUpdateSteamBranch
        {
            get => 
                this.AutoUpdateSteamBranch;
            set => 
                (this.AutoUpdateSteamBranch = value);
        }

        string IMyConfigDedicated.AutoUpdateBranchPassword
        {
            get => 
                this.AutoUpdateBranchPassword;
            set => 
                (this.AutoUpdateBranchPassword = value);
        }

        bool IMyConfigDedicated.AutoRestartSave
        {
            get => 
                this.AutoRestartSave;
            set => 
                (this.AutoRestartSave = value);
        }

        string IMyConfigDedicated.ServerName
        {
            get => 
                this.ServerName;
            set => 
                (this.ServerName = value);
        }

        MyObjectBuilder_SessionSettings IMyConfigDedicated.SessionSettings
        {
            get => 
                this.SessionSettings;
            set => 
                (this.SessionSettings = (T) value);
        }

        string IMyConfigDedicated.WorldName
        {
            get => 
                this.WorldName;
            set => 
                (this.WorldName = value);
        }

        string IMyConfigDedicated.ServerPasswordHash
        {
            get => 
                this.ServerPasswordHash;
            set => 
                (this.ServerPasswordHash = value);
        }

        string IMyConfigDedicated.ServerPasswordSalt
        {
            get => 
                this.ServerPasswordSalt;
            set => 
                (this.ServerPasswordSalt = value);
        }

        string IMyConfigDedicated.RemoteSecurityKey
        {
            get => 
                this.RemoteSecurityKey;
            set => 
                (this.RemoteSecurityKey = value);
        }

        bool IMyConfigDedicated.RemoteApiEnabled
        {
            get => 
                this.RemoteApiEnabled;
            set => 
                (this.RemoteApiEnabled = value);
        }

        int IMyConfigDedicated.RemoteApiPort
        {
            get => 
                this.RemoteApiPort;
            set => 
                (this.RemoteApiPort = value);
        }

        List<string> IMyConfigDedicated.Plugins
        {
            get => 
                this.Plugins;
            set => 
                (this.Plugins = value);
        }

        float IMyConfigDedicated.WatcherInterval
        {
            get => 
                this.WatcherInterval;
            set => 
                (this.WatcherInterval = value);
        }

        float IMyConfigDedicated.WatcherSimulationSpeedMinimum
        {
            get => 
                this.WatcherSimulationSpeedMinimum;
            set => 
                (this.WatcherSimulationSpeedMinimum = value);
        }

        int IMyConfigDedicated.ManualActionDelay
        {
            get => 
                this.ManualActionDelay;
            set => 
                (this.ManualActionDelay = value);
        }

        string IMyConfigDedicated.ManualActionChatMessage
        {
            get => 
                this.ManualActionChatMessage;
            set => 
                (this.ManualActionChatMessage = value);
        }

        bool IMyConfigDedicated.AutodetectDependencies
        {
            get => 
                this.AutodetectDependencies;
            set => 
                (this.AutodetectDependencies = value);
        }

        public bool SaveChatToLog
        {
            get => 
                this.m_data.SaveChatToLog;
            set => 
                (this.m_data.SaveChatToLog = value);
        }

        bool IMyConfigDedicated.SaveChatToLog
        {
            get => 
                this.SaveChatToLog;
            set => 
                (this.SaveChatToLog = value);
        }
    }
}


namespace VRage.GameServices
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    public interface IMyGameServer
    {
        event Action PlatformConnected;

        event Action<string> PlatformConnectionFailed;

        event Action<string> PlatformDisconnected;

        event Action<sbyte> PolicyResponse;

        event Action<ulong, ulong, bool, bool> UserGroupStatusResponse;

        event Action<ulong, JoinResult, ulong> ValidateAuthTicketResponse;

        bool BeginAuthSession(ulong userId, byte[] token);
        void BrowserUpdateUserData(ulong userId, string playerName, int score);
        void ClearAllKeyValues();
        void EnableHeartbeats(bool enable);
        void EndAuthSession(ulong userId);
        uint GetPublicIP();
        void LogOff();
        void LogOnAnonymous();
        bool RequestGroupStatus(ulong userId, ulong groupId);
        void SendUserDisconnect(ulong userId);
        void SetBotPlayerCount(int count);
        void SetDedicated(bool isDedicated);
        void SetGameData(string data);
        void SetGameTags(string tags);
        void SetKeyValue(string key, string value);
        void SetMapName(string mapName);
        void SetMaxPlayerCount(int count);
        void SetModDir(string directory);
        void SetPasswordProtected(bool passwdProtected);
        void SetServerName(string serverName);
        void Shutdown();
        bool Start(IPEndPoint serverEndpoint, ushort steamPort, string versionString);
        void Update();
        bool UserHasLicenseForApp(ulong steamId, uint appId);

        string GameDescription { get; set; }

        string ProductName { get; set; }

        ulong ServerId { get; }

        bool Running { get; }
    }
}


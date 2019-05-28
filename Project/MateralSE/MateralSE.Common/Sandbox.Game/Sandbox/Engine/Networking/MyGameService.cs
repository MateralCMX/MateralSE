namespace Sandbox.Engine.Networking
{
    using Sandbox.Engine.Platform;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.GameServices;

    public static class MyGameService
    {
        private static IMyGameService m_gameServiceCache;
        private static MyNullPeer2Peer m_nullPeer2Peer;

        public static  event EventHandler<MyGameItemsEventArgs> CheckItemDataReady
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.CheckItemDataReady += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.CheckItemDataReady -= value;
                }
            }
        }

        public static  event EventHandler InventoryRefreshed
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.InventoryRefreshed += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.InventoryRefreshed -= value;
                }
            }
        }

        public static  event EventHandler<MyGameItemsEventArgs> ItemsAdded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.ItemsAdded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.ItemsAdded -= value;
                }
            }
        }

        public static  event MyLobbyJoinRequested LobbyJoinRequested
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnJoinLobbyRequested += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnJoinLobbyRequested -= value;
                }
            }
        }

        public static  event EventHandler NoItemsRecieved
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.NoItemsRecieved += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.NoItemsRecieved -= value;
                }
            }
        }

        public static  event EventHandler<int> OnDedicatedServerListResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnDedicatedServerListResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnDedicatedServerListResponded -= value;
                }
            }
        }

        public static  event EventHandler<MyMatchMakingServerResponse> OnDedicatedServersCompleteResponse
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnDedicatedServersCompleteResponse += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnDedicatedServersCompleteResponse -= value;
                }
            }
        }

        public static  event Action<uint> OnDLCInstalled
        {
            add
            {
                if (EnsureGameService() && m_gameServiceCache.IsActive)
                {
                    m_gameServiceCache.OnDLCInstalled += value;
                }
            }
            remove
            {
                if (EnsureGameService() && m_gameServiceCache.IsActive)
                {
                    m_gameServiceCache.OnDLCInstalled -= value;
                }
            }
        }

        public static  event EventHandler<int> OnFavoritesServerListResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFavoritesServerListResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFavoritesServerListResponded -= value;
                }
            }
        }

        public static  event EventHandler<MyMatchMakingServerResponse> OnFavoritesServersCompleteResponse
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFavoritesServersCompleteResponse += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFavoritesServersCompleteResponse -= value;
                }
            }
        }

        public static  event EventHandler<int> OnFriendsServerListResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFriendsServerListResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFriendsServerListResponded -= value;
                }
            }
        }

        public static  event EventHandler<MyMatchMakingServerResponse> OnFriendsServersCompleteResponse
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFriendsServersCompleteResponse += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnFriendsServersCompleteResponse -= value;
                }
            }
        }

        public static  event EventHandler<int> OnHistoryServerListResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnHistoryServerListResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnHistoryServerListResponded -= value;
                }
            }
        }

        public static  event EventHandler<MyMatchMakingServerResponse> OnHistoryServersCompleteResponse
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnHistoryServersCompleteResponse += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnHistoryServersCompleteResponse -= value;
                }
            }
        }

        public static  event EventHandler<int> OnLANServerListResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnLANServerListResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnLANServerListResponded -= value;
                }
            }
        }

        public static  event EventHandler<MyMatchMakingServerResponse> OnLANServersCompleteResponse
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnLANServersCompleteResponse += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnLANServersCompleteResponse -= value;
                }
            }
        }

        public static  event Action<byte> OnOverlayActivated
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnOverlayActivated += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnOverlayActivated -= value;
                }
            }
        }

        public static  event EventHandler OnPingServerFailedToRespond
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnPingServerFailedToRespond += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnPingServerFailedToRespond -= value;
                }
            }
        }

        public static  event EventHandler<MyGameServerItem> OnPingServerResponded
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnPingServerResponded += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnPingServerResponded -= value;
                }
            }
        }

        public static  event MyLobbyServerChangeRequested ServerChangeRequested
        {
            add
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnServerChangeRequested += value;
                }
            }
            remove
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.OnServerChangeRequested -= value;
                }
            }
        }

        public static void AddFavoriteGame(uint ip, ushort connPort, ushort queryPort)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.AddFavoriteGame(ip, connPort, queryPort);
            }
        }

        internal static void AddFriendLobbies(List<IMyLobby> lobbies)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.AddFriendLobbies(lobbies);
            }
        }

        public static void AddHistoryGame(uint ip, ushort connPort, ushort queryPort)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.AddHistoryGame(ip, connPort, queryPort);
            }
        }

        internal static void AddLobbyFilter(string key, string value)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.AddLobbyFilter(key, value);
            }
        }

        internal static void AddPublicLobbies(List<IMyLobby> lobbies)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.AddPublicLobbies(lobbies);
            }
        }

        internal static void CancelFavoritesServersRequest()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CancelFavoritesServersRequest();
            }
        }

        internal static void CancelFriendsServersRequest()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CancelFriendsServersRequest();
            }
        }

        internal static void CancelHistoryServersRequest()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CancelHistoryServersRequest();
            }
        }

        internal static void CancelInternetServersRequest()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CancelInternetServersRequest();
            }
        }

        public static void CancelLANServersRequest()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CancelLANServersRequest();
            }
        }

        internal static List<MyGameInventoryItem> CheckItemData(byte[] checkData, out bool checkResult)
        {
            if (EnsureGameService())
            {
                return m_gameServiceCache.CheckItemData(checkData, out checkResult);
            }
            checkResult = false;
            return null;
        }

        public static void ClearCache()
        {
            m_gameServiceCache = null;
        }

        internal static void CommitPublishedFileUpdate(ulong updateHandle, Action<bool, MyRemoteStorageUpdatePublishedFileResult> onCallResult)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CommitPublishedFileUpdate(updateHandle, onCallResult);
            }
        }

        internal static void ConsumeItem(MyGameInventoryItem item)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.ConsumeItem(item);
            }
        }

        internal static bool CraftSkin(MyGameInventoryItemQuality quality) => 
            (EnsureGameService() ? m_gameServiceCache.CraftSkin(quality) : false);

        internal static IMyLobby CreateLobby(ulong lobbyId) => 
            (EnsureGameService() ? m_gameServiceCache.CreateLobby(lobbyId) : null);

        internal static void CreateLobby(MyLobbyType type, uint maxPlayers, MyLobbyCreated createdResponse)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.CreateLobby(type, maxPlayers, createdResponse);
            }
        }

        internal static ulong CreatePublishedFileUpdateRequest(ulong publishedFileId) => 
            (EnsureGameService() ? m_gameServiceCache.CreatePublishedFileUpdateRequest(publishedFileId) : 0UL);

        public static MyWorkshopItem CreateWorkshopItem() => 
            (EnsureGameService() ? m_gameServiceCache.CreateWorkshopItem() : null);

        public static MyWorkshopItemPublisher CreateWorkshopPublisher() => 
            (EnsureGameService() ? m_gameServiceCache.CreateWorkshopPublisher() : null);

        public static MyWorkshopItemPublisher CreateWorkshopPublisher(MyWorkshopItem item) => 
            (EnsureGameService() ? m_gameServiceCache.CreateWorkshopPublisher(item) : null);

        public static MyWorkshopQuery CreateWorkshopQuery() => 
            (EnsureGameService() ? m_gameServiceCache.CreateWorkshopQuery() : null);

        internal static MyVoiceResult DecompressVoice(byte[] compressedBuffer, uint size, byte[] uncompressedBuffer, out uint writtenBytes)
        {
            if (EnsureGameService())
            {
                return m_gameServiceCache.DecompressVoice(compressedBuffer, size, uncompressedBuffer, out writtenBytes);
            }
            writtenBytes = 0;
            return MyVoiceResult.NotInitialized;
        }

        public static bool DeleteFromCloud(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.DeleteFromCloud(fileName) : false);

        internal static void DisposeVoiceRecording()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.DisposeVoiceRecording();
            }
        }

        private static bool EnsureGameService()
        {
            if (m_gameServiceCache == null)
            {
                m_gameServiceCache = MyServiceManager.Instance.GetService<IMyGameService>();
            }
            return (m_gameServiceCache != null);
        }

        internal static void FileDelete(string steamItemFileName)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.FileDelete(steamItemFileName);
            }
        }

        internal static bool FileExists(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.FileExists(fileName) : false);

        internal static void FileShare(string file, Action<bool, MyRemoteStorageFileShareResult> onCallResult)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.FileShare(file, onCallResult);
            }
        }

        internal static void FileWriteStreamClose(ulong handle)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.FileWriteStreamClose(handle);
            }
        }

        internal static ulong FileWriteStreamOpen(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.FileWriteStreamOpen(fileName) : 0UL);

        internal static void FileWriteStreamWriteChunk(ulong handle, byte[] buffer, int size)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.FileWriteStreamWriteChunk(handle, buffer, size);
            }
        }

        internal static void GetAllInventoryItems()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.GetAllInventoryItems();
            }
        }

        public static bool GetAuthSessionTicket(out uint ticketHandle, byte[] buffer, out uint length)
        {
            length = 0;
            ticketHandle = 0;
            return (EnsureGameService() ? m_gameServiceCache.GetAuthSessionTicket(out ticketHandle, buffer, out length) : false);
        }

        internal static MyVoiceResult GetAvailableVoice(out uint size)
        {
            if (EnsureGameService())
            {
                return m_gameServiceCache.GetAvailableVoice(out size);
            }
            size = 0;
            return MyVoiceResult.NotInitialized;
        }

        internal static int GetChatMaxMessageSize() => 
            (EnsureGameService() ? m_gameServiceCache.GetChatMaxMessageSize() : 0);

        public static string GetClanName(ulong groupId) => 
            (EnsureGameService() ? m_gameServiceCache.GetClanName(groupId) : string.Empty);

        public static List<MyCloudFileInfo> GetCloudFiles(string directoryFilter) => 
            (EnsureGameService() ? m_gameServiceCache.GetCloudFiles(directoryFilter) : null);

        internal static uint GetCraftingCost(MyGameInventoryItemQuality quality) => 
            (EnsureGameService() ? m_gameServiceCache.GetCraftingCost(quality) : 0);

        internal static MyGameServerItem GetDedicatedServerDetails(int server) => 
            (EnsureGameService() ? m_gameServiceCache.GetDedicatedServerDetails(server) : null);

        public static int GetDLCCount()
        {
            if (!EnsureGameService() || !m_gameServiceCache.IsActive)
            {
                return 0;
            }
            return m_gameServiceCache.GetDLCCount();
        }

        public static bool GetDLCDataByIndex(int index, out uint dlcId, out bool available, out string name, int nameBufferSize)
        {
            if (EnsureGameService())
            {
                return m_gameServiceCache.GetDLCDataByIndex(index, out dlcId, out available, out name, nameBufferSize);
            }
            dlcId = 0;
            available = false;
            name = null;
            return false;
        }

        internal static MyGameServerItem GetFavoritesServerDetails(int server) => 
            (EnsureGameService() ? m_gameServiceCache.GetFavoritesServerDetails(server) : null);

        public static byte[] GetFileBufferFromCloud(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.GetFileBufferFromCloud(fileName) : null);

        internal static int GetFileSize(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.GetFileSize(fileName) : 0);

        public static ulong GetFriendIdByIndex(int index) => 
            (EnsureGameService() ? m_gameServiceCache.GetFriendIdByIndex(index) : 0UL);

        public static string GetFriendNameByIndex(int index) => 
            (EnsureGameService() ? m_gameServiceCache.GetFriendNameByIndex(index) : string.Empty);

        public static int GetFriendsCount() => 
            (EnsureGameService() ? m_gameServiceCache.GetFriendsCount() : -1);

        internal static MyGameServerItem GetFriendsServerDetails(int server) => 
            (EnsureGameService() ? m_gameServiceCache.GetFriendsServerDetails(server) : null);

        internal static MyGameServerItem GetHistoryServerDetails(int server) => 
            (EnsureGameService() ? m_gameServiceCache.GetHistoryServerDetails(server) : null);

        internal static MyGameInventoryItemDefinition GetInventoryItemDefinition(string assetModifierId) => 
            (EnsureGameService() ? m_gameServiceCache.GetInventoryItemDefinition(assetModifierId) : null);

        internal static void GetItemCheckData(MyGameInventoryItem item)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.GetItemCheckData(item);
            }
        }

        internal static void GetItemsCheckData(List<MyGameInventoryItem> items, Action<byte[]> completedAction)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.GetItemsCheckData(items, completedAction);
            }
        }

        public static MyGameServerItem GetLANServerDetails(int server) => 
            (EnsureGameService() ? m_gameServiceCache.GetLANServerDetails(server) : null);

        public static uint GetNumWorkshopSubscribedItems() => 
            (EnsureGameService() ? m_gameServiceCache.GetNumWorkshopSubscribedItems() : 0);

        public static string GetPersonaName(ulong userId) => 
            (EnsureGameService() ? m_gameServiceCache.GetPersonaName(userId) : string.Empty);

        public static void GetPlayerDetails(uint ip, ushort port, PlayerDetailsResponse completedAction, Action failedAction)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.GetPlayerDetails(ip, port, completedAction, failedAction);
            }
        }

        internal static uint GetRecyclingReward(MyGameInventoryItemQuality quality) => 
            (EnsureGameService() ? m_gameServiceCache.GetRecyclingReward(quality) : 0);

        public static int GetRemoteStorageFileCount() => 
            (EnsureGameService() ? m_gameServiceCache.GetRemoteStorageFileCount() : 0);

        public static string GetRemoteStorageFileNameAndSize(int fileIndex, out int fileSizeInBytes)
        {
            fileSizeInBytes = 0;
            return (EnsureGameService() ? m_gameServiceCache.GetRemoteStorageFileNameAndSize(fileIndex, out fileSizeInBytes) : string.Empty);
        }

        public static bool GetRemoteStorageQuota(out ulong totalBytes, out ulong availableBytes)
        {
            totalBytes = availableBytes = 0UL;
            return (EnsureGameService() ? m_gameServiceCache.GetRemoteStorageQuota(out totalBytes, out availableBytes) : true);
        }

        internal static MyGameServiceAccountType GetServerAccountType(ulong steamId) => 
            (EnsureGameService() ? m_gameServiceCache.GetServerAccountType(steamId) : MyGameServiceAccountType.Invalid);

        public static void GetServerRules(uint ip, ushort port, ServerRulesResponse completedAction, Action failedAction)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.GetServerRules(ip, port, completedAction, failedAction);
            }
        }

        public static float GetStatFloat(string name) => 
            (EnsureGameService() ? m_gameServiceCache.GetStatFloat(name) : 0f);

        public static int GetStatInt(string name) => 
            (EnsureGameService() ? m_gameServiceCache.GetStatInt(name) : 0);

        internal static MyVoiceResult GetVoice(byte[] buffer, out uint bytesWritten)
        {
            if (EnsureGameService())
            {
                return m_gameServiceCache.GetVoice(buffer, out bytesWritten);
            }
            bytesWritten = 0;
            return MyVoiceResult.NotInitialized;
        }

        internal static int GetVoiceSampleRate() => 
            (EnsureGameService() ? m_gameServiceCache.SampleRate : 0);

        public static bool HasFriend(ulong userId) => 
            (EnsureGameService() ? m_gameServiceCache.HasFriend(userId) : false);

        internal static bool HasInventoryItem(ulong id) => 
            (EnsureGameService() ? m_gameServiceCache.HasInventoryItem(id) : false);

        public static void IndicateAchievementProgress(string name, uint value, uint maxValue)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.IndicateAchievementProgress(name, value, maxValue);
            }
        }

        internal static void InitializeVoiceRecording()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.InitializeVoiceRecording();
            }
        }

        public static bool IsAchieved(string achievementName) => 
            (EnsureGameService() ? m_gameServiceCache.IsAchieved(achievementName) : false);

        public static bool IsDlcInstalled(uint dlcId) => 
            (EnsureGameService() && (m_gameServiceCache.IsActive && m_gameServiceCache.IsDlcInstalled(dlcId)));

        public static bool IsRemoteStorageFilePersisted(string file) => 
            (EnsureGameService() ? m_gameServiceCache.IsRemoteStorageFilePersisted(file) : false);

        public static bool IsUpdateAvailable() => 
            false;

        public static bool IsUserInGroup(ulong groupId) => 
            (EnsureGameService() ? m_gameServiceCache.IsUserInGroup(groupId) : false);

        internal static void JoinLobby(ulong lobbyId, MyJoinResponseDelegate reponseDelegate)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.JoinLobby(lobbyId, reponseDelegate);
            }
        }

        public static byte[] LoadFromCloud(string fileName) => 
            (EnsureGameService() ? m_gameServiceCache.LoadFromCloud(fileName) : null);

        public static bool LoadFromCloudAsync(string fileName, Action<bool> completedAction) => 
            (EnsureGameService() ? m_gameServiceCache.LoadFromCloudAsync(fileName, completedAction) : false);

        public static void LoadStats()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.LoadStats();
            }
        }

        public static void OpenInviteOverlay()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.OpenInviteOverlay();
            }
        }

        public static void OpenOverlayUrl(string url)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.OpenOverlayUrl(url);
            }
        }

        public static void OpenOverlayUser(ulong id)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.OpenOverlayUser(id);
            }
        }

        public static void PingServer(uint ip, ushort port)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.PingServer(ip, port);
            }
        }

        internal static void PublishWorkshopFile(string file, string previewFile, string title, string description, string longDescription, MyPublishedFileVisibility visibility, string[] tags, Action<bool, MyRemoteStoragePublishFileResult> onCallResult)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.PublishWorkshopFile(file, previewFile, title, description, longDescription, visibility, tags, onCallResult);
            }
        }

        internal static bool RecycleItem(MyGameInventoryItem item) => 
            (EnsureGameService() ? m_gameServiceCache.RecycleItem(item) : false);

        public static bool RemoteStorageFileForget(string file) => 
            (EnsureGameService() ? m_gameServiceCache.RemoteStorageFileForget(file) : false);

        public static void RemoveFavoriteGame(uint ip, ushort connPort, ushort queryPort)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RemoveFavoriteGame(ip, connPort, queryPort);
            }
        }

        internal static void RequestFavoritesServerList(string filterOps)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestFavoritesServerList(filterOps);
            }
        }

        internal static void RequestFriendsServerList(string filterOps)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestFriendsServerList(filterOps);
            }
        }

        internal static void RequestHistoryServerList(string filterOps)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestHistoryServerList(filterOps);
            }
        }

        internal static void RequestInternetServerList(string filterOps)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestInternetServerList(filterOps);
            }
        }

        public static void RequestLANServerList()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestLANServerList();
            }
        }

        internal static void RequestLobbyList(Action<bool> completed)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.RequestLobbyList(completed);
            }
        }

        public static void ResetAllStats(bool achievementsToo)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.ResetAllStats(achievementsToo);
            }
        }

        public static bool SaveToCloud(string fileName, byte[] buffer) => 
            (EnsureGameService() ? m_gameServiceCache.SaveToCloud(fileName, buffer) : false);

        public static bool SaveToCloudAsync(string fileName, byte[] buffer, Action<bool> completedAction) => 
            (EnsureGameService() ? m_gameServiceCache.SaveToCloudAsync(fileName, buffer, completedAction) : false);

        public static void ServerUpdate()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.ServerUpdate();
            }
        }

        public static void SetAchievement(string name)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SetAchievement(name);
            }
        }

        internal static void SetNotificationPosition(NotificationPosition notificationPosition)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SetNotificationPosition(notificationPosition);
            }
        }

        internal static void SetServerModTemporaryDirectory()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SetServerModTemporaryDirectory();
            }
        }

        public static void SetStat(string name, int value)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SetStat(name, value);
            }
        }

        public static void SetStat(string name, float value)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SetStat(name, value);
            }
        }

        public static void ShutDown()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.ShutDown();
            }
        }

        internal static void StartVoiceRecording()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.StartVoiceRecording();
            }
        }

        internal static void StopVoiceRecording()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.StopVoiceRecording();
            }
        }

        public static void StoreStats()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.StoreStats();
            }
        }

        internal static void SubscribePublishedFile(ulong publishedFileId, Action<bool, MyRemoteStorageSubscribePublishedFileResult> onCallResult)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.SubscribePublishedFile(publishedFileId, onCallResult);
            }
        }

        internal static void TriggerCompetitiveContainer()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.TriggerCompetitiveContainer();
            }
        }

        internal static void TriggerPersonalContainer()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.TriggerPersonalContainer();
            }
        }

        public static void Update()
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.Update();
            }
        }

        internal static void UpdatePublishedFileFile(ulong updateHandle, string steamItemFileName)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.UpdatePublishedFileFile(updateHandle, steamItemFileName);
            }
        }

        internal static void UpdatePublishedFilePreviewFile(ulong updateHandle, string steamPreviewFileName)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.UpdatePublishedFilePreviewFile(updateHandle, steamPreviewFileName);
            }
        }

        internal static void UpdatePublishedFileTags(ulong updateHandle, string[] tags)
        {
            if (EnsureGameService())
            {
                m_gameServiceCache.UpdatePublishedFileTags(updateHandle, tags);
            }
        }

        public static IMyGameServer GameServer =>
            (EnsureGameService() ? m_gameServiceCache.GameServer : null);

        public static IMyPeer2Peer Peer2Peer =>
            (!EnsureGameService() ? (m_nullPeer2Peer ?? (m_nullPeer2Peer = new MyNullPeer2Peer())) : m_gameServiceCache.Peer2Peer);

        public static uint AppId =>
            (EnsureGameService() ? m_gameServiceCache.AppId : 0);

        public static bool IsActive =>
            (EnsureGameService() && m_gameServiceCache.IsActive);

        public static bool IsOnline =>
            (EnsureGameService() && m_gameServiceCache.IsOnline);

        public static bool IsOverlayEnabled =>
            (EnsureGameService() && m_gameServiceCache.IsOverlayEnabled);

        public static bool OwnsGame =>
            (EnsureGameService() && (IsActive && m_gameServiceCache.OwnsGame));

        public static ulong UserId
        {
            get => 
                (EnsureGameService() ? m_gameServiceCache.UserId : ulong.MaxValue);
            set
            {
                if (EnsureGameService())
                {
                    m_gameServiceCache.UserId = value;
                }
            }
        }

        public static string UserName =>
            (!EnsureGameService() ? string.Empty : m_gameServiceCache.UserName);

        public static MyGameServiceUniverse UserUniverse =>
            (!EnsureGameService() ? MyGameServiceUniverse.Invalid : m_gameServiceCache.UserUniverse);

        public static string BranchName
        {
            get
            {
                if (Game.IsDedicated)
                {
                    return "DedicatedServer";
                }
                if (!IsActive)
                {
                    return "SVN";
                }
                if (!EnsureGameService() || string.IsNullOrEmpty(m_gameServiceCache.BranchName))
                {
                    return "";
                }
                return m_gameServiceCache.BranchName;
            }
        }

        public static int RecycleTokens =>
            (!EnsureGameService() ? 0 : m_gameServiceCache.RecycleTokens);

        public static string BranchNameFriendly =>
            (!string.IsNullOrEmpty(BranchName) ? BranchName : "default");

        public static ICollection<MyGameInventoryItem> InventoryItems =>
            (EnsureGameService() ? m_gameServiceCache.InventoryItems : null);

        public static ICollection<MyGameInventoryItemDefinition> Definitions =>
            (EnsureGameService() ? m_gameServiceCache.Definitions : null);

        public static bool HasGameServer =>
            (EnsureGameService() ? m_gameServiceCache.HasGameServer : false);
    }
}


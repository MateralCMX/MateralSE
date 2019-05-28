namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public interface IMyGameService
    {
        event EventHandler<MyGameItemsEventArgs> CheckItemDataReady;

        event EventHandler InventoryRefreshed;

        event EventHandler<MyGameItemsEventArgs> ItemsAdded;

        event EventHandler NoItemsRecieved;

        event EventHandler<int> OnDedicatedServerListResponded;

        event EventHandler<MyMatchMakingServerResponse> OnDedicatedServersCompleteResponse;

        event Action<uint> OnDLCInstalled;

        event EventHandler<int> OnFavoritesServerListResponded;

        event EventHandler<MyMatchMakingServerResponse> OnFavoritesServersCompleteResponse;

        event EventHandler<int> OnFriendsServerListResponded;

        event EventHandler<MyMatchMakingServerResponse> OnFriendsServersCompleteResponse;

        event EventHandler<int> OnHistoryServerListResponded;

        event EventHandler<MyMatchMakingServerResponse> OnHistoryServersCompleteResponse;

        event MyLobbyJoinRequested OnJoinLobbyRequested;

        event EventHandler<int> OnLANServerListResponded;

        event EventHandler<MyMatchMakingServerResponse> OnLANServersCompleteResponse;

        event Action<byte> OnOverlayActivated;

        event EventHandler OnPingServerFailedToRespond;

        event EventHandler<MyGameServerItem> OnPingServerResponded;

        event MyLobbyServerChangeRequested OnServerChangeRequested;

        void AddFavoriteGame(uint ip, ushort connPort, ushort queryPort);
        void AddFriendLobbies(List<IMyLobby> lobbyList);
        void AddHistoryGame(uint ip, ushort connPort, ushort queryPort);
        void AddLobbyFilter(string key, string value);
        void AddPublicLobbies(List<IMyLobby> lobbyList);
        void CancelFavoritesServersRequest();
        void CancelFriendsServersRequest();
        void CancelHistoryServersRequest();
        void CancelInternetServersRequest();
        void CancelLANServersRequest();
        List<MyGameInventoryItem> CheckItemData(byte[] checkData, out bool checkResult);
        void CommitPublishedFileUpdate(ulong updateHandle, Action<bool, MyRemoteStorageUpdatePublishedFileResult> onCallResult);
        void ConsumeItem(MyGameInventoryItem item);
        bool CraftSkin(MyGameInventoryItemQuality quality);
        IMyLobby CreateLobby(ulong lobbyId);
        void CreateLobby(MyLobbyType type, uint maxPlayers, MyLobbyCreated createdResponse);
        ulong CreatePublishedFileUpdateRequest(ulong publishedFileId);
        MyWorkshopItem CreateWorkshopItem();
        MyWorkshopItemPublisher CreateWorkshopPublisher();
        MyWorkshopItemPublisher CreateWorkshopPublisher(MyWorkshopItem item);
        MyWorkshopQuery CreateWorkshopQuery();
        MyVoiceResult DecompressVoice(byte[] compressedBuffer, uint size, byte[] uncompressedBuffer, out uint writtenBytes);
        bool DeleteFromCloud(string fileName);
        void DisposeVoiceRecording();
        void FileDelete(string steamItemFileName);
        bool FileExists(string fileName);
        void FileShare(string file, Action<bool, MyRemoteStorageFileShareResult> onCallResult);
        void FileWriteStreamClose(ulong handle);
        ulong FileWriteStreamOpen(string fileName);
        void FileWriteStreamWriteChunk(ulong handle, byte[] buffer, int size);
        void GetAllInventoryItems();
        bool GetAuthSessionTicket(out uint ticketHandle, byte[] buffer, out uint length);
        MyVoiceResult GetAvailableVoice(out uint size);
        int GetChatMaxMessageSize();
        string GetClanName(ulong groupId);
        List<MyCloudFileInfo> GetCloudFiles(string directoryFilter);
        uint GetCraftingCost(MyGameInventoryItemQuality quality);
        MyGameServerItem GetDedicatedServerDetails(int server);
        int GetDLCCount();
        bool GetDLCDataByIndex(int index, out uint dlcId, out bool available, out string name, int nameBufferSize);
        MyGameServerItem GetFavoritesServerDetails(int server);
        byte[] GetFileBufferFromCloud(string fileName);
        int GetFileSize(string fileName);
        ulong GetFriendIdByIndex(int index);
        string GetFriendNameByIndex(int index);
        int GetFriendsCount();
        MyGameServerItem GetFriendsServerDetails(int server);
        MyGameServerItem GetHistoryServerDetails(int server);
        MyGameInventoryItemDefinition GetInventoryItemDefinition(string assetModifierId);
        void GetItemCheckData(MyGameInventoryItem item);
        void GetItemsCheckData(List<MyGameInventoryItem> items, Action<byte[]> completedAction);
        MyGameServerItem GetLANServerDetails(int server);
        uint GetNumWorkshopSubscribedItems();
        string GetPersonaName(ulong userId);
        void GetPlayerDetails(uint ip, ushort port, PlayerDetailsResponse completedAction, Action failedAction);
        uint GetRecyclingReward(MyGameInventoryItemQuality quality);
        int GetRemoteStorageFileCount();
        string GetRemoteStorageFileNameAndSize(int fileIndex, out int fileSizeInBytes);
        bool GetRemoteStorageQuota(out ulong totalBytes, out ulong availableBytes);
        MyGameServiceAccountType GetServerAccountType(ulong steamId);
        void GetServerRules(uint ip, ushort port, ServerRulesResponse completedAction, Action failedAction);
        float GetStatFloat(string name);
        int GetStatInt(string name);
        MyVoiceResult GetVoice(byte[] buffer, out uint bytesWritten);
        bool HasFriend(ulong userId);
        bool HasInventoryItem(ulong id);
        void IndicateAchievementProgress(string name, uint value, uint maxValue);
        void InitializeVoiceRecording();
        bool IsAchieved(string achievementName);
        bool IsDlcInstalled(uint dlcId);
        bool IsRemoteStorageFilePersisted(string file);
        bool IsUserInGroup(ulong groupId);
        void JoinLobby(ulong lobbyId, MyJoinResponseDelegate responseDelegate);
        byte[] LoadFromCloud(string fileName);
        bool LoadFromCloudAsync(string fileName, Action<bool> completedAction);
        void LoadStats();
        void OpenInviteOverlay();
        void OpenOverlayUrl(string url);
        void OpenOverlayUser(ulong id);
        void PingServer(uint ip, ushort port);
        void PublishWorkshopFile(string file, string previewFile, string title, string description, string longDescription, MyPublishedFileVisibility visibility, string[] tags, Action<bool, MyRemoteStoragePublishFileResult> onCallResult);
        bool RecycleItem(MyGameInventoryItem item);
        bool RemoteStorageFileForget(string file);
        void RemoveFavoriteGame(uint ip, ushort connPort, ushort queryPort);
        void RequestFavoritesServerList(string filterOps);
        void RequestFriendsServerList(string filterOps);
        void RequestHistoryServerList(string filterOps);
        void RequestInternetServerList(string filterOps);
        void RequestLANServerList();
        void RequestLobbyList(Action<bool> completed);
        void ResetAllStats(bool achievementsToo);
        bool SaveToCloud(string fileName, byte[] buffer);
        bool SaveToCloudAsync(string fileName, byte[] buffer, Action<bool> completedAction);
        void ServerUpdate();
        void SetAchievement(string name);
        void SetNotificationPosition(NotificationPosition notificationPosition);
        void SetServerModTemporaryDirectory();
        void SetStat(string name, int value);
        void SetStat(string name, float value);
        void ShutDown();
        void StartVoiceRecording();
        void StopVoiceRecording();
        void StoreStats();
        void SubscribePublishedFile(ulong publishedFileId, Action<bool, MyRemoteStorageSubscribePublishedFileResult> onCallResult);
        void TriggerCompetitiveContainer();
        void TriggerPersonalContainer();
        void Update();
        void UpdatePublishedFileFile(ulong updateHandle, string steamItemFileName);
        void UpdatePublishedFilePreviewFile(ulong updateHandle, string steamPreviewFileName);
        void UpdatePublishedFileTags(ulong updateHandle, string[] tags);

        uint AppId { get; }

        bool IsActive { get; }

        bool IsOnline { get; }

        bool IsOverlayEnabled { get; }

        bool OwnsGame { get; }

        ulong UserId { get; set; }

        string UserName { get; }

        MyGameServiceUniverse UserUniverse { get; }

        string BranchName { get; }

        string BranchNameFriendly { get; }

        IMyGameServer GameServer { get; }

        ICollection<MyGameInventoryItem> InventoryItems { get; }

        ICollection<MyGameInventoryItemDefinition> Definitions { get; }

        IMyPeer2Peer Peer2Peer { get; }

        bool HasGameServer { get; }

        int RecycleTokens { get; }

        int SampleRate { get; }
    }
}


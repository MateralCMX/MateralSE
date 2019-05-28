namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public interface IMyLobby
    {
        event MessageReceivedDelegate OnChatReceived;

        event MessageScriptedReceivedDelegate OnChatScriptedReceived;

        event MyLobbyChatUpdated OnChatUpdated;

        event MyLobbyDataUpdated OnDataReceived;

        event KickedDelegate OnKicked;

        bool DeleteData(string key);
        void GetChatMessage(int msgId, out string result, out ulong senderId);
        string GetData(string key);
        ulong GetMemberByIndex(int index);
        bool IsJoinable();
        void Join(MyJoinResponseDelegate reponseDelegate);
        void Leave();
        bool RequestData();
        bool SendChatMessage(string text, byte channel, long targetId = 0L);
        bool SendChatMessageScripted(string text, byte channel, long targetId = 0L, string customAuthor = null);
        bool SetData(string key, string value);
        bool SetJoinable(bool joinable);

        ulong LobbyId { get; }

        bool IsValid { get; }

        ulong OwnerId { get; set; }

        MyLobbyType LobbyType { get; set; }

        int MemberCount { get; }

        int MemberLimit { get; set; }

        IEnumerable<ulong> MemberList { get; }
    }
}


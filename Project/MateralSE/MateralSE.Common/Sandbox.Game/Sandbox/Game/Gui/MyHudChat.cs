namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI.HudViewers;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    public class MyHudChat
    {
        private static readonly int MAX_MESSAGES_IN_CHAT_DEFAULT = 10;
        private static readonly int MAX_MESSAGE_TIME_DEFAULT = 0x3a98;
        public static int MaxMessageTime = MAX_MESSAGE_TIME_DEFAULT;
        public static int MaxMessageCount = MAX_MESSAGES_IN_CHAT_DEFAULT;
        public Queue<MyChatItem> MessagesQueue = new Queue<MyChatItem>();
        public List<MyChatItem> MessageHistory = new List<MyChatItem>();
        private int m_lastUpdateTime = 0x7fffffff;
        private int m_lastScreenUpdateTime = 0x7fffffff;
        public MyHudControlChat ChatControl;
        private bool m_chatScreenOpen;

        public MyHudChat()
        {
            this.Timestamp = 0;
        }

        public void ChatClosed()
        {
            this.m_chatScreenOpen = false;
        }

        public void ChatOpened()
        {
            this.m_chatScreenOpen = true;
        }

        private void Multiplayer_ChatMessageReceived(ulong steamUserId, string messageText, ChatChannel channel, long targetId, string customAuthorName = null)
        {
            if (MyGameService.IsActive)
            {
                string memberName = string.Empty;
                if (channel != ChatChannel.Private)
                {
                    memberName = MyMultiplayer.Static.GetMemberName(steamUserId);
                }
                else if (targetId == MySession.Static.LocalPlayerId)
                {
                    memberName = MyMultiplayer.Static.GetMemberName(steamUserId);
                    memberName = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_From), memberName);
                }
                else
                {
                    ulong steamUserID = MySession.Static.Players.TryGetSteamId(targetId);
                    if (steamUserID != 0)
                    {
                        memberName = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_To), MyMultiplayer.Static.GetMemberName(steamUserID));
                    }
                }
                long identityId = MySession.Static.Players.TryGetIdentityId(steamUserId, 0);
                Color relationColor = MyChatSystem.GetRelationColor(identityId);
                Color channelColor = MyChatSystem.GetChannelColor(channel);
                this.ShowMessage(string.IsNullOrEmpty(customAuthorName) ? memberName : customAuthorName, messageText, relationColor, channelColor);
                if (channel == ChatChannel.GlobalScripted)
                {
                    MySession.Static.ChatSystem.ChatHistory.EnqueueMessageScripted(messageText, string.IsNullOrEmpty(customAuthorName) ? MyTexts.GetString(MySpaceTexts.ChatBotName) : customAuthorName, "Blue");
                }
                else
                {
                    DateTime? timestamp = null;
                    MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(messageText, channel, identityId, targetId, timestamp, "Blue");
                }
            }
        }

        public void multiplayer_ScriptedChatMessageReceived(string message, string author, string font)
        {
            if (MyGameService.IsActive)
            {
                this.ShowMessage(author, message, font);
                MySession.Static.ChatSystem.ChatHistory.EnqueueMessageScripted(message, author, font);
            }
        }

        public void RegisterChat(MyMultiplayerBase multiplayer)
        {
            if (multiplayer != null)
            {
                multiplayer.ChatMessageReceived += new Action<ulong, string, ChatChannel, long, string>(this.Multiplayer_ChatMessageReceived);
                multiplayer.ScriptedChatMessageReceived += new Action<string, string, string>(this.multiplayer_ScriptedChatMessageReceived);
            }
        }

        public static void ResetChatSettings()
        {
            MaxMessageTime = MAX_MESSAGE_TIME_DEFAULT;
            MaxMessageCount = MAX_MESSAGES_IN_CHAT_DEFAULT;
        }

        public void ShowMessage(string sender, string messageText, string font = "Blue")
        {
            MyChatItem item = new MyChatItem(sender, messageText, font, Color.White);
            this.MessagesQueue.Enqueue(item);
            this.MessageHistory.Add(item);
            this.m_lastScreenUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.MessagesQueue.Count > MaxMessageCount)
            {
                this.MessagesQueue.Dequeue();
            }
            this.UpdateTimestamp();
        }

        public void ShowMessage(string sender, string message, Color senderColor)
        {
            this.ShowMessage(sender, message, senderColor, Color.White);
        }

        public void ShowMessage(string sender, string message, Color senderColor, Color messageColor)
        {
            MyChatItem item = new MyChatItem(sender, message, "White", senderColor, messageColor);
            this.MessagesQueue.Enqueue(item);
            this.MessageHistory.Add(item);
            this.m_lastScreenUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.MessagesQueue.Count > MaxMessageCount)
            {
                this.MessagesQueue.Dequeue();
            }
            this.UpdateTimestamp();
        }

        public void ShowMessageColoredSP(string text, ChatChannel channel, long targetId = 0L, string customAuthorName = null)
        {
            string displayName = string.Empty;
            if (channel != ChatChannel.Private)
            {
                displayName = MySession.Static.LocalHumanPlayer.DisplayName;
            }
            else if (targetId != MySession.Static.LocalPlayerId)
            {
                displayName = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_To), targetId);
            }
            else
            {
                displayName = MySession.Static.LocalHumanPlayer.DisplayName;
                displayName = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_From), displayName);
            }
            long identityId = MySession.Static.Players.TryGetIdentityId(Sync.MyId, 0);
            Color relationColor = MyChatSystem.GetRelationColor(identityId);
            Color channelColor = MyChatSystem.GetChannelColor(channel);
            this.ShowMessage(string.IsNullOrEmpty(customAuthorName) ? displayName : customAuthorName, text, relationColor, channelColor);
            if (channel == ChatChannel.GlobalScripted)
            {
                MySession.Static.ChatSystem.ChatHistory.EnqueueMessageScripted(text, string.IsNullOrEmpty(customAuthorName) ? MyTexts.GetString(MySpaceTexts.ChatBotName) : customAuthorName, "Blue");
            }
            else
            {
                DateTime? timestamp = null;
                MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(text, channel, identityId, targetId, timestamp, "Blue");
            }
        }

        public void ShowMessageScripted(string sender, string messageText)
        {
            Color paleGoldenrod = Color.PaleGoldenrod;
            this.ShowMessage(sender, messageText, paleGoldenrod, Color.White);
        }

        public void UnregisterChat(MyMultiplayerBase multiplayer)
        {
            if (multiplayer != null)
            {
                multiplayer.ChatMessageReceived -= new Action<ulong, string, ChatChannel, long, string>(this.Multiplayer_ChatMessageReceived);
                multiplayer.ScriptedChatMessageReceived -= new Action<string, string, string>(this.multiplayer_ScriptedChatMessageReceived);
                this.MessagesQueue.Clear();
                this.UpdateTimestamp();
            }
        }

        public void Update()
        {
            if (this.m_chatScreenOpen)
            {
                this.m_lastScreenUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
            if (((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime) > MaxMessageTime) && (this.MessagesQueue.Count > 0))
            {
                this.MessagesQueue.Dequeue();
                this.UpdateTimestamp();
            }
        }

        private void UpdateTimestamp()
        {
            int timestamp = this.Timestamp;
            this.Timestamp = timestamp + 1;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public int Timestamp { get; private set; }

        public int LastUpdateTime =>
            this.m_lastUpdateTime;

        public int TimeSinceLastUpdate =>
            (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastScreenUpdateTime);
    }
}


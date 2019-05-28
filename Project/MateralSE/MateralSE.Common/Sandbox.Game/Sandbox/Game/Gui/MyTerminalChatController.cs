namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game.ModAPI;
    using VRageMath;

    internal class MyTerminalChatController
    {
        private MyGuiControlListbox m_playerList;
        private MyGuiControlListbox m_factionList;
        private MyGuiControlListbox.Item m_chatBotItem;
        private MyGuiControlListbox.Item m_broadcastItem;
        private MyGuiControlListbox.Item m_globalItem;
        private MyGuiControlMultilineText m_chatHistory;
        private MyGuiControlTextbox m_chatbox;
        private MyGuiControlButton m_sendButton;
        private readonly StringBuilder m_emptyText = new StringBuilder();
        private StringBuilder m_chatboxText = new StringBuilder();
        private StringBuilder m_tempStringBuilder = new StringBuilder();
        private bool m_closed = true;
        private bool m_pendingUpdatePlayerList;
        private bool m_waitedOneFrameBeforeUpdating;
        private int m_frameCount;
        private List<MyIdentity> m_tempOnlinePlayers = new List<MyIdentity>();
        private List<MyIdentity> m_tempOfflinePlayers = new List<MyIdentity>();

        private void ChatSystem_FactionHistoryDeleted()
        {
            if (!this.m_closed)
            {
                this.UpdateFactionList(true);
                if (this.m_factionList.SelectedItems.Count > 0)
                {
                    this.RefreshFactionChatHistory((MyFaction) this.m_factionList.SelectedItems[0].UserData);
                }
            }
        }

        private void ChatSystem_PlayerHistoryDeleted()
        {
            if (!this.m_closed)
            {
                this.UpdatePlayerList();
                if (this.m_factionList.SelectedItems.Count > 0)
                {
                    this.RefreshFactionChatHistory((MyFaction) this.m_factionList.SelectedItems[0].UserData);
                }
            }
        }

        private void ClearChat()
        {
            this.m_chatHistory.Clear();
            this.m_chatbox.SetText(this.m_emptyText);
        }

        public void Close()
        {
            this.m_closed = false;
            this.m_playerList.ItemsSelected -= new Action<MyGuiControlListbox>(this.m_playerList_ItemsSelected);
            this.m_factionList.ItemsSelected -= new Action<MyGuiControlListbox>(this.m_factionList_ItemsSelected);
            this.m_sendButton.ButtonClicked -= new Action<MyGuiControlButton>(this.m_sendButton_ButtonClicked);
            this.m_chatbox.TextChanged -= new Action<MyGuiControlTextbox>(this.m_chatbox_TextChanged);
            this.m_chatbox.EnterPressed -= new Action<MyGuiControlTextbox>(this.m_chatbox_EnterPressed);
            if (MyMultiplayer.Static != null)
            {
                MyMultiplayer.Static.ChatMessageReceived -= new Action<ulong, string, ChatChannel, long, string>(this.Multiplayer_ChatMessageReceived);
            }
            if (MySession.Static.LocalCharacter != null)
            {
                MySession.Static.ChatSystem.PlayerMessageReceived -= new Action<long>(this.MyChatSystem_PlayerMessageReceived);
                MySession.Static.ChatSystem.FactionMessageReceived -= new Action<long>(this.MyChatSystem_FactionMessageReceived);
            }
            MySession.Static.Players.PlayersChanged -= new Action<bool, MyPlayer.PlayerId>(this.Players_PlayersChanged);
        }

        public void Init(IMyGuiControlsParent controlsParent)
        {
            this.m_playerList = (MyGuiControlListbox) controlsParent.Controls.GetControlByName("PlayerListbox");
            this.m_factionList = (MyGuiControlListbox) controlsParent.Controls.GetControlByName("FactionListbox");
            this.m_chatHistory = (MyGuiControlMultilineText) controlsParent.Controls.GetControlByName("ChatHistory");
            this.m_chatbox = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("Chatbox");
            this.m_chatbox.SetToolTip(MyTexts.GetString(MySpaceTexts.ChatScreen_TerminaMessageBox));
            this.m_playerList.ItemsSelected += new Action<MyGuiControlListbox>(this.m_playerList_ItemsSelected);
            this.m_playerList.MultiSelect = false;
            this.m_factionList.ItemsSelected += new Action<MyGuiControlListbox>(this.m_factionList_ItemsSelected);
            this.m_factionList.MultiSelect = false;
            this.m_sendButton = (MyGuiControlButton) controlsParent.Controls.GetControlByName("SendButton");
            this.m_sendButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ChatScreen_TerminalSendMessage));
            this.m_sendButton.ButtonClicked += new Action<MyGuiControlButton>(this.m_sendButton_ButtonClicked);
            this.m_sendButton.ShowTooltipWhenDisabled = true;
            this.m_chatbox.TextChanged += new Action<MyGuiControlTextbox>(this.m_chatbox_TextChanged);
            this.m_chatbox.EnterPressed += new Action<MyGuiControlTextbox>(this.m_chatbox_EnterPressed);
            if (MySession.Static.LocalCharacter != null)
            {
                MySession.Static.ChatSystem.PlayerMessageReceived += new Action<long>(this.MyChatSystem_PlayerMessageReceived);
                MySession.Static.ChatSystem.FactionMessageReceived += new Action<long>(this.MyChatSystem_FactionMessageReceived);
            }
            MySession.Static.Players.PlayersChanged += new Action<bool, MyPlayer.PlayerId>(this.Players_PlayersChanged);
            this.RefreshLists();
            this.m_chatbox.SetText(this.m_emptyText);
            this.m_sendButton.Enabled = false;
            if (MyMultiplayer.Static != null)
            {
                MyMultiplayer.Static.ChatMessageReceived += new Action<ulong, string, ChatChannel, long, string>(this.Multiplayer_ChatMessageReceived);
            }
            this.m_closed = false;
        }

        private void m_chatbox_EnterPressed(MyGuiControlTextbox obj)
        {
            if (this.m_chatboxText.Length > 0)
            {
                this.SendMessage();
            }
        }

        private void m_chatbox_TextChanged(MyGuiControlTextbox obj)
        {
            this.m_chatboxText.Clear();
            obj.GetText(this.m_chatboxText);
            if (this.m_chatboxText.Length == 0)
            {
                this.m_sendButton.Enabled = false;
                this.m_sendButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ChatScreen_TerminalSendMessageDisabled));
            }
            else
            {
                if (MySession.Static.LocalCharacter != null)
                {
                    this.m_sendButton.Enabled = true;
                    this.m_sendButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ChatScreen_TerminalSendMessage));
                }
                if (this.m_chatboxText.Length > 200)
                {
                    this.m_chatboxText.Length = 200;
                    this.m_chatbox.SetText(this.m_chatboxText);
                }
            }
        }

        private void m_factionList_ItemsSelected(MyGuiControlListbox obj)
        {
            if (this.m_factionList.SelectedItems.Count > 0)
            {
                MyFaction userData = (MyFaction) this.m_factionList.SelectedItems[0].UserData;
                this.RefreshFactionChatHistory(userData);
                MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId);
                this.m_chatbox.SetText(this.m_emptyText);
            }
        }

        private void m_playerList_ItemsSelected(MyGuiControlListbox obj)
        {
            if (this.m_playerList.SelectedItems.Count > 0)
            {
                MyGuiControlListbox.Item objA = this.m_playerList.SelectedItems[0];
                if (ReferenceEquals(objA, this.m_globalItem))
                {
                    this.RefreshGlobalChatHistory();
                }
                else if (ReferenceEquals(objA, this.m_chatBotItem))
                {
                    this.RefreshChatBotHistory();
                }
                else
                {
                    MyIdentity userData = (MyIdentity) objA.UserData;
                    this.RefreshPlayerChatHistory(userData);
                }
                this.m_chatbox.SetText(this.m_emptyText);
            }
        }

        private void m_sendButton_ButtonClicked(MyGuiControlButton obj)
        {
            this.SendMessage();
        }

        private void Multiplayer_ChatMessageReceived(ulong steamUserId, string messageText, ChatChannel channel, long targetId, string customAuthorName = null)
        {
            if (this.m_playerList.SelectedItems.Count <= 0)
            {
                if (this.m_factionList.SelectedItems.Count > 0)
                {
                    MyFaction userData = (MyFaction) this.m_factionList.SelectedItems[0].UserData;
                    if (userData.FactionId == targetId)
                    {
                        this.RefreshFactionChatHistory(userData);
                    }
                }
            }
            else
            {
                MyGuiControlListbox.Item objA = this.m_playerList.SelectedItems[0];
                if (ReferenceEquals(objA, this.m_globalItem))
                {
                    this.RefreshGlobalChatHistory();
                }
                else if (ReferenceEquals(objA, this.m_chatBotItem))
                {
                    this.RefreshChatBotHistory();
                }
                else
                {
                    MyPlayer.PlayerId id;
                    MyIdentity userData = (MyIdentity) objA.UserData;
                    if ((MySession.Static.Players.TryGetPlayerId(userData.IdentityId, out id) && (MySession.Static.LocalPlayerId == targetId)) && (steamUserId == id.SteamId))
                    {
                        this.RefreshPlayerChatHistory(userData);
                    }
                }
            }
        }

        private void MyChatSystem_FactionMessageReceived(long factionId)
        {
            if (this.m_factionList.SelectedItems.Count > 0)
            {
                MyFaction userData = (MyFaction) this.m_factionList.SelectedItems[0].UserData;
                if (userData.FactionId == factionId)
                {
                    this.RefreshFactionChatHistory(userData);
                }
            }
        }

        private void MyChatSystem_PlayerMessageReceived(long playerId)
        {
            if (((this.m_playerList != null) && (this.m_playerList.SelectedItems != null)) && (this.m_playerList.SelectedItems.Count > 0))
            {
                MyIdentity userData = (MyIdentity) this.m_playerList.SelectedItems[0].UserData;
            }
        }

        private void OnChatBotResponse(string text)
        {
            MyUnifiedChatItem item = MyUnifiedChatItem.CreateChatbotMessage(text, DateTime.UtcNow, 0L, MySession.Static.LocalPlayerId, MyTexts.GetString(MySpaceTexts.ChatBotName), "Blue");
            MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(ref item);
            this.RefreshChatBotHistory();
        }

        private void Players_PlayersChanged(bool added, MyPlayer.PlayerId playerId)
        {
            if (!this.m_closed)
            {
                this.UpdatePlayerList();
            }
        }

        private void RefreshChatBotHistory()
        {
            this.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetChatbotHistory(ref list);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity((item.SenderId != 0) ? item.SenderId : item.TargetId);
                if (identity != null)
                {
                    Vector4 one = Vector4.One;
                    Color white = Color.White;
                    if (item.CustomAuthor.Length > 0)
                    {
                        this.m_chatHistory.AppendText(item.CustomAuthor, "White", this.m_chatHistory.TextScale, one);
                    }
                    else
                    {
                        this.m_chatHistory.AppendText(identity.DisplayName, "White", this.m_chatHistory.TextScale, one);
                    }
                    this.m_chatHistory.AppendText(": ", "White", this.m_chatHistory.TextScale, one);
                    this.m_chatHistory.Parse(item.Text, "White", this.m_chatHistory.TextScale, white);
                    this.m_chatHistory.AppendLine();
                }
            }
            this.m_factionList.SelectedItems.Clear();
            this.m_chatHistory.ScrollbarOffsetV = 1f;
        }

        private void RefreshFactionChatHistory(MyFaction faction)
        {
            this.m_chatHistory.Clear();
            if ((MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId) != null) || MySession.Static.IsUserAdmin(Sync.MyId))
            {
                List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
                MySession.Static.ChatSystem.ChatHistory.GetFactionHistory(ref list, faction.FactionId);
                foreach (MyUnifiedChatItem item in list)
                {
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(item.SenderId);
                    if (identity != null)
                    {
                        Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                        this.m_chatHistory.AppendText(identity.DisplayName, "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                        this.m_chatHistory.AppendText(": ", "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                        this.m_chatHistory.AppendText(item.Text, "White", this.m_chatHistory.TextScale, (Vector4) channelColor);
                        this.m_chatHistory.AppendLine();
                    }
                }
                this.m_playerList.SelectedItems.Clear();
                this.m_chatHistory.ScrollbarOffsetV = 1f;
            }
        }

        private void RefreshFactionList()
        {
            IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId);
            if (faction == null)
            {
                this.m_factionList.SelectedItems.Clear();
                this.m_factionList.Items.Clear();
                this.m_factionList.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Chat_NoFaction));
            }
            else
            {
                this.m_tempStringBuilder.Clear();
                this.m_tempStringBuilder.Append(MyStatControlText.SubstituteTexts(faction.Name, null));
                object userData = faction;
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(this.m_tempStringBuilder, this.m_tempStringBuilder.ToString(), null, userData, null);
                int? position = null;
                this.m_factionList.Add(item, position);
                this.m_factionList.SetToolTip(string.Empty);
            }
        }

        private void RefreshGlobalChatHistory()
        {
            this.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetGeneralHistory(ref list);
            foreach (MyUnifiedChatItem item in list)
            {
                if (item.Channel == ChatChannel.GlobalScripted)
                {
                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    if (item.CustomAuthor.Length > 0)
                    {
                        this.m_chatHistory.AppendText(item.CustomAuthor + ": ", item.AuthorFont, this.m_chatHistory.TextScale, (Vector4) relationColor);
                    }
                    else
                    {
                        this.m_chatHistory.AppendText(MyTexts.GetString(MySpaceTexts.ChatBotName) + ": ", item.AuthorFont, this.m_chatHistory.TextScale, (Vector4) relationColor);
                    }
                    this.m_chatHistory.AppendText(item.Text, "White", this.m_chatHistory.TextScale, (Vector4) channelColor);
                    this.m_chatHistory.AppendLine();
                    continue;
                }
                if (item.Channel == ChatChannel.Global)
                {
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(item.SenderId);
                    if (identity != null)
                    {
                        Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                        this.m_chatHistory.AppendText(identity.DisplayName, "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                        this.m_chatHistory.AppendText(": ", "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                        this.m_chatHistory.AppendText(item.Text, "White", this.m_chatHistory.TextScale, (Vector4) channelColor);
                        this.m_chatHistory.AppendLine();
                    }
                }
            }
            this.m_factionList.SelectedItems.Clear();
            this.m_chatHistory.ScrollbarOffsetV = 1f;
        }

        private void RefreshLists()
        {
            this.RefreshPlayerList();
            this.RefreshFactionList();
        }

        private void RefreshPlayerChatHistory(MyIdentity playerIdentity)
        {
            this.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetPrivateHistory(ref list, playerIdentity.IdentityId);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(item.SenderId);
                if (identity != null)
                {
                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    this.m_chatHistory.AppendText(identity.DisplayName, "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                    this.m_chatHistory.AppendText(": ", "White", this.m_chatHistory.TextScale, (Vector4) relationColor);
                    this.m_chatHistory.AppendText(item.Text, "White", this.m_chatHistory.TextScale, (Vector4) channelColor);
                    this.m_chatHistory.AppendLine();
                }
            }
            this.m_factionList.SelectedItems.Clear();
            this.m_chatHistory.ScrollbarOffsetV = 1f;
        }

        private void RefreshPlayerList()
        {
            this.m_globalItem = new MyGuiControlListbox.Item(MyTexts.Get(MySpaceTexts.TerminalTab_Chat_ChatHistory), MyTexts.GetString(MySpaceTexts.TerminalTab_Chat_ChatHistory), null, null, null);
            int? position = null;
            this.m_playerList.Add(this.m_globalItem, position);
            this.m_tempStringBuilder.Clear();
            this.m_tempStringBuilder.Append(MyTexts.Get(MySpaceTexts.TerminalTab_Chat_GlobalChat));
            this.m_tempStringBuilder.Clear();
            this.m_tempStringBuilder.Append("-");
            this.m_tempStringBuilder.Append(MyTexts.Get(MySpaceTexts.ChatBotName));
            this.m_tempStringBuilder.Append("-");
            this.m_chatBotItem = new MyGuiControlListbox.Item(this.m_tempStringBuilder, this.m_tempStringBuilder.ToString(), null, null, null);
            position = null;
            this.m_playerList.Add(this.m_chatBotItem, position);
            this.m_tempOnlinePlayers.Clear();
            this.m_tempOfflinePlayers.Clear();
            foreach (MyPlayer.PlayerId id in MySession.Static.Players.GetAllPlayers())
            {
                MyIdentity item = MySession.Static.Players.TryGetIdentity(MySession.Static.Players.TryGetIdentityId(id.SteamId, id.SerialId));
                if ((item != null) && ((item.IdentityId != MySession.Static.LocalPlayerId) && (id.SerialId == 0)))
                {
                    if (item.Character == null)
                    {
                        this.m_tempOfflinePlayers.Add(item);
                        continue;
                    }
                    this.m_tempOnlinePlayers.Add(item);
                }
            }
            foreach (MyIdentity identity2 in this.m_tempOnlinePlayers)
            {
                this.m_tempStringBuilder.Clear();
                this.m_tempStringBuilder.Append(identity2.DisplayName);
                object userData = identity2;
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(this.m_tempStringBuilder, this.m_tempStringBuilder.ToString(), null, userData, null);
                position = null;
                this.m_playerList.Add(item, position);
            }
        }

        private void SendMessage()
        {
            if (MySession.Static.LocalCharacter != null)
            {
                this.m_chatboxText.Clear();
                this.m_chatbox.GetText(this.m_chatboxText);
                if (this.m_playerList.SelectedItems.Count <= 0)
                {
                    if (this.m_factionList.SelectedItems.Count > 0)
                    {
                        MyFaction userData = (MyFaction) this.m_factionList.SelectedItems[0].UserData;
                        if (!userData.IsMember(MySession.Static.LocalPlayerId))
                        {
                            return;
                        }
                        if (MyMultiplayer.Static != null)
                        {
                            MyMultiplayer.Static.SendChatMessage(this.m_chatboxText.ToString(), ChatChannel.Faction, userData.FactionId);
                        }
                        else if (MyGameService.IsActive)
                        {
                            MyHud.Chat.ShowMessageColoredSP(this.m_chatboxText.ToString(), ChatChannel.Faction, userData.FactionId, null);
                        }
                        else
                        {
                            MyHud.Chat.ShowMessage((MySession.Static.LocalHumanPlayer == null) ? "Player" : MySession.Static.LocalHumanPlayer.DisplayName, this.m_chatboxText.ToString(), "Blue");
                        }
                        this.RefreshFactionChatHistory(userData);
                    }
                }
                else
                {
                    MyGuiControlListbox.Item objA = this.m_playerList.SelectedItems[0];
                    if (ReferenceEquals(objA, this.m_globalItem))
                    {
                        if (MyMultiplayer.Static != null)
                        {
                            MyMultiplayer.Static.SendChatMessage(this.m_chatboxText.ToString(), ChatChannel.Global, 0L);
                        }
                        else if (MyGameService.IsActive)
                        {
                            MyHud.Chat.ShowMessageColoredSP(this.m_chatboxText.ToString(), ChatChannel.Global, 0L, null);
                        }
                        else
                        {
                            MyHud.Chat.ShowMessage((MySession.Static.LocalHumanPlayer == null) ? "Player" : MySession.Static.LocalHumanPlayer.DisplayName, this.m_chatboxText.ToString(), "Blue");
                        }
                        this.RefreshGlobalChatHistory();
                    }
                    else if (!ReferenceEquals(objA, this.m_chatBotItem))
                    {
                        MyIdentity userData = (MyIdentity) objA.UserData;
                        MyMultiplayer.Static.SendChatMessage(this.m_chatboxText.ToString(), ChatChannel.Private, userData.IdentityId);
                        this.RefreshPlayerChatHistory(userData);
                    }
                    else
                    {
                        string text = this.m_chatboxText.ToString();
                        MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(text, ChatChannel.ChatBot, MySession.Static.LocalPlayerId, -1L, new DateTime?(DateTime.UtcNow), "Blue");
                        this.RefreshChatBotHistory();
                        if (!MySession.Static.ChatBot.FilterMessage(text, new Action<string>(this.OnChatBotResponse)))
                        {
                            MySession.Static.ChatBot.FilterMessage($"? {text}", new Action<string>(this.OnChatBotResponse));
                        }
                    }
                }
                this.m_chatbox.SetText(this.m_emptyText);
            }
        }

        public void Update()
        {
            if (!this.m_closed)
            {
                this.UpdateLists();
            }
        }

        private void UpdateFactionList(bool forceRefresh)
        {
            MyFactionCollection source = MySession.Static.Factions;
            if (source.TryGetPlayerFaction(MySession.Static.LocalPlayerId) == null)
            {
                if (this.m_factionList.Items.Count != 0)
                {
                    this.RefreshFactionList();
                }
            }
            else if (forceRefresh || (this.m_factionList.Items.Count != source.Count<KeyValuePair<long, MyFaction>>()))
            {
                long factionId = -1L;
                if (this.m_factionList.SelectedItems.Count > 0)
                {
                    factionId = ((MyFaction) this.m_factionList.SelectedItems[0].UserData).FactionId;
                }
                int firstVisibleRow = this.m_factionList.FirstVisibleRow;
                this.m_factionList.SelectedItems.Clear();
                this.m_factionList.Items.Clear();
                this.RefreshFactionList();
                if (factionId != -1L)
                {
                    bool flag = false;
                    foreach (MyGuiControlListbox.Item item in this.m_factionList.Items)
                    {
                        if (((MyFaction) item.UserData).FactionId == factionId)
                        {
                            this.m_factionList.SelectedItems.Clear();
                            this.m_factionList.SelectedItems.Add(item);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        this.ClearChat();
                    }
                }
                if (firstVisibleRow >= this.m_factionList.Items.Count)
                {
                    firstVisibleRow = this.m_factionList.Items.Count - 1;
                }
                this.m_factionList.FirstVisibleRow = firstVisibleRow;
            }
        }

        private void UpdateLists()
        {
            this.UpdateFactionList(false);
            if (this.m_frameCount > 100)
            {
                this.m_frameCount = 0;
                this.UpdatePlayerList();
            }
            this.m_frameCount++;
        }

        private void UpdatePlayerList()
        {
            long identityId = -1L;
            bool flag = false;
            bool flag2 = false;
            if (this.m_playerList.SelectedItems.Count > 0)
            {
                if (this.m_playerList.SelectedItems[0] == this.m_globalItem)
                {
                    flag = true;
                }
                else if (this.m_playerList.SelectedItems[0] == this.m_chatBotItem)
                {
                    flag2 = true;
                }
                else
                {
                    identityId = ((MyIdentity) this.m_playerList.SelectedItems[0].UserData).IdentityId;
                }
            }
            int firstVisibleRow = this.m_playerList.FirstVisibleRow;
            this.m_playerList.SelectedItems.Clear();
            this.m_playerList.Items.Clear();
            this.RefreshPlayerList();
            if (identityId == -1L)
            {
                if (flag)
                {
                    this.m_playerList.SelectedItems.Clear();
                    this.m_playerList.SelectedItems.Add(this.m_globalItem);
                }
                else if (flag2)
                {
                    this.m_playerList.SelectedItems.Clear();
                    this.m_playerList.SelectedItems.Add(this.m_chatBotItem);
                }
            }
            else
            {
                bool flag3 = false;
                foreach (MyGuiControlListbox.Item item in this.m_playerList.Items)
                {
                    if ((item.UserData != null) && (((MyIdentity) item.UserData).IdentityId == identityId))
                    {
                        this.m_playerList.SelectedItems.Clear();
                        this.m_playerList.SelectedItems.Add(item);
                        flag3 = true;
                        break;
                    }
                }
                if (!flag3)
                {
                    this.ClearChat();
                }
            }
            if (firstVisibleRow >= this.m_playerList.Items.Count)
            {
                firstVisibleRow = this.m_playerList.Items.Count - 1;
            }
            this.m_playerList.FirstVisibleRow = firstVisibleRow;
        }
    }
}


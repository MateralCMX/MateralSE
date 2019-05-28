namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.Graphics.GUI.IME;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Compiler;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenChat : MyGuiScreenBase
    {
        private readonly MyGuiControlTextbox m_chatTextbox;
        private readonly MyGuiControlLabel m_channelInfo;
        public static MyGuiScreenChat Static = null;
        private const int MESSAGE_HISTORY_SIZE = 20;
        private static StringBuilder[] m_messageHistory = new StringBuilder[20];
        private static int m_messageHistoryPushTo = 0;
        private static int m_messageHistoryShown = 0;
        private MyNameFillState m_currentNameFillState;
        private string[] NAMEFILL_BASES;
        private string m_namefillPrefix_completeBefore;
        private string m_namefillPrefix_completeNew;
        private string m_namefillPrefix_name;
        private string m_namefillPrefix_command;
        private int m_currentNamefillIndex;
        private List<MyPlayer> m_currentPlayerList;

        static MyGuiScreenChat()
        {
            for (int i = 0; i < 20; i++)
            {
                m_messageHistory[i] = new StringBuilder();
            }
        }

        public MyGuiScreenChat(Vector2 position) : base(new Vector2?(position), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), size, false, null, 0f, 0f)
        {
            this.m_currentNameFillState = MyNameFillState.Inactive;
            this.NAMEFILL_BASES = new string[] { "/w \"", "/w " };
            this.m_namefillPrefix_completeBefore = string.Empty;
            this.m_namefillPrefix_completeNew = string.Empty;
            this.m_namefillPrefix_name = string.Empty;
            this.m_namefillPrefix_command = string.Empty;
            this.m_currentNamefillIndex = 0x7fffffff;
            Vector2? size = null;
            MySandboxGame.Log.WriteLine("MyGuiScreenChat.ctor START");
            base.EnabledBackgroundFade = false;
            base.m_isTopMostScreen = true;
            base.CanHideOthers = false;
            base.DrawMouseCursor = false;
            base.m_closeOnEsc = true;
            VRageMath.Vector4? textColor = null;
            this.m_chatTextbox = new MyGuiControlTextbox(new Vector2?(Vector2.Zero), null, MyGameService.GetChatMaxMessageSize(), textColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_chatTextbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_chatTextbox.Size = new Vector2(0.27f, 0.05f);
            this.m_chatTextbox.TextScale = 0.8f;
            this.m_chatTextbox.VisualStyle = MyGuiControlTextboxStyleEnum.Default;
            ChatChannel currentChannel = MySession.Static.ChatSystem.CurrentChannel;
            string text = string.Empty;
            Color channelColor = MyChatSystem.GetChannelColor(currentChannel);
            switch (currentChannel)
            {
                case ChatChannel.Global:
                    text = MyTexts.GetString(MyCommonTexts.Chat_NameModifier_Global);
                    break;

                case ChatChannel.Faction:
                {
                    string tag = "faction";
                    IMyFaction faction = MySession.Static.Factions.TryGetFactionById(MySession.Static.ChatSystem.CurrentTarget);
                    if (faction != null)
                    {
                        tag = faction.Tag;
                    }
                    text = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_ToBracketed), tag);
                    break;
                }
                case ChatChannel.Private:
                {
                    string str3 = "player";
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(MySession.Static.ChatSystem.CurrentTarget);
                    if (identity != null)
                    {
                        str3 = (identity.DisplayName.Length > 9) ? identity.DisplayName.Substring(0, 9) : identity.DisplayName;
                    }
                    text = string.Format(MyTexts.GetString(MyCommonTexts.Chat_NameModifier_ToBracketed), str3);
                    break;
                }
                default:
                    text = MyTexts.GetString(MyCommonTexts.Chat_NameModifier_ReportThis);
                    break;
            }
            size = null;
            textColor = null;
            this.m_channelInfo = new MyGuiControlLabel(new Vector2(-0.016f, -0.042f), size, text, textColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_channelInfo.ColorMask = (VRageMath.Vector4) channelColor;
            this.Controls.Add(this.m_chatTextbox);
            this.Controls.Add(this.m_channelInfo);
            this.m_chatTextbox.Size = new Vector2(0.3215f - this.m_channelInfo.Size.X, 0.032f);
            this.m_chatTextbox.Position = new Vector2(-0.01f, -0.06f) + new Vector2(this.m_channelInfo.Size.X, 0f);
            MySandboxGame.Log.WriteLine("MyGuiScreenChat.ctor END");
            MyHud.Chat.ChatOpened();
        }

        private void CycleNamefill()
        {
            if (this.m_currentPlayerList.Count != 0)
            {
                this.m_currentNamefillIndex++;
                if (this.m_currentNamefillIndex >= this.m_currentPlayerList.Count)
                {
                    this.m_currentNamefillIndex = 0;
                }
                this.m_namefillPrefix_completeNew = this.m_namefillPrefix_command + this.m_currentPlayerList[this.m_currentNamefillIndex].DisplayName;
                this.m_chatTextbox.Text = this.m_namefillPrefix_completeNew;
            }
        }

        public override bool Draw() => 
            base.Draw();

        public override string GetFriendlyName() => 
            "MyGuiScreenChat";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Down))
            {
                this.HardResetFill();
                this.HistoryUp();
            }
            else if (MyInput.Static.IsNewKeyPressed(MyKeys.Up))
            {
                this.HardResetFill();
                this.HistoryDown();
            }
            else if (MyInput.Static.IsKeyPress(MyKeys.PageUp))
            {
                this.HardResetFill();
                MyHud.Chat.ChatControl.ScrollUp();
            }
            else if (MyInput.Static.IsKeyPress(MyKeys.PageDown))
            {
                this.HardResetFill();
                MyHud.Chat.ChatControl.ScrollDown();
            }
            else if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape) && (this.m_currentNameFillState == MyNameFillState.Active))
            {
                this.SoftResetFill();
            }
            else if (MyInput.Static.IsNewKeyPressed(MyKeys.Tab))
            {
                switch (this.m_currentNameFillState)
                {
                    case MyNameFillState.Disabled:
                        break;

                    case MyNameFillState.Inactive:
                        if (!this.InitiateNamefill())
                        {
                            break;
                        }
                        this.CycleNamefill();
                        return;

                    case MyNameFillState.Active:
                        if (this.m_chatTextbox.Text.Equals(this.m_namefillPrefix_completeNew))
                        {
                            this.CycleNamefill();
                            return;
                        }
                        this.SoftResetFill();
                        if (this.InitiateNamefill())
                        {
                            this.CycleNamefill();
                        }
                        break;

                    default:
                        return;
                }
            }
            else
            {
                base.HandleInput(receivedFocusInThisUpdate);
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) && ((MyImeProcessor.Instance == null) || !MyImeProcessor.Instance.IsComposing))
                {
                    string text = this.m_chatTextbox.Text;
                    PushHistory(text);
                    if (MySession.Static.ChatSystem.CommandSystem.CanHandle(text))
                    {
                        MyHud.Chat.ShowMessage(MySession.Static.LocalHumanPlayer.DisplayName, text, "Blue");
                        MySession.Static.ChatSystem.CommandSystem.Handle(text);
                    }
                    else if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (MySession.Static.ChatBot.FilterMessage(text, new Action<string>(this.OnChatBotResponse)))
                        {
                            MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(text, ChatChannel.ChatBot, MySession.Static.LocalPlayerId, -1L, new DateTime?(DateTime.UtcNow), "Blue");
                            MyHud.Chat.ShowMessage((MySession.Static.LocalHumanPlayer == null) ? "Player" : MySession.Static.LocalHumanPlayer.DisplayName, text, "Blue");
                        }
                        else
                        {
                            bool sendToOthers = true;
                            MyAPIUtilities.Static.EnterMessage(text, ref sendToOthers);
                            if (sendToOthers)
                            {
                                SendChatMessage(text);
                            }
                        }
                    }
                    this.m_chatTextbox.FocusEnded();
                    this.CloseScreenNow();
                }
            }
        }

        private void HardResetFill()
        {
            if (this.m_currentNameFillState == MyNameFillState.Active)
            {
                this.m_chatTextbox.Text = this.m_namefillPrefix_completeBefore;
            }
            this.SoftResetFill();
        }

        public override bool HideScreen()
        {
            this.UnloadContent();
            return base.HideScreen();
        }

        private void HistoryDown()
        {
            int num = HistoryIndexDown(m_messageHistoryShown);
            if (num != m_messageHistoryPushTo)
            {
                m_messageHistoryShown = num;
                string text1 = m_messageHistory[m_messageHistoryShown].ToString();
                this.m_chatTextbox.Text = text1 ?? "";
            }
        }

        private static int HistoryIndexDown(int index)
        {
            index--;
            return ((index >= 0) ? index : 0x13);
        }

        private static int HistoryIndexUp(int index)
        {
            index++;
            return ((index < 20) ? index : 0);
        }

        private void HistoryUp()
        {
            if (m_messageHistoryShown != m_messageHistoryPushTo)
            {
                m_messageHistoryShown = HistoryIndexUp(m_messageHistoryShown);
                string text1 = m_messageHistory[m_messageHistoryShown].ToString();
                this.m_chatTextbox.Text = text1 ?? "";
            }
        }

        private bool InitiateNamefill()
        {
            string text = this.m_chatTextbox.Text;
            string command = string.Empty;
            string nameStump = string.Empty;
            if (!this.TestNamefillPrefixse(text, out command, out nameStump))
            {
                return false;
            }
            this.m_currentNameFillState = MyNameFillState.Active;
            this.m_namefillPrefix_completeBefore = this.m_namefillPrefix_completeNew = text;
            this.m_namefillPrefix_command = command;
            this.m_namefillPrefix_name = nameStump;
            this.m_currentPlayerList = MySession.Static.Players.GetPlayersStartingNameWith(nameStump);
            this.m_currentNamefillIndex = this.m_currentPlayerList.Count - 1;
            return true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Static = this;
        }

        private void OnChatBotResponse(string text)
        {
            MyUnifiedChatItem item = MyUnifiedChatItem.CreateChatbotMessage(text, DateTime.UtcNow, 0L, MySession.Static.LocalPlayerId, MyTexts.GetString(MySpaceTexts.ChatBotName), "Blue");
            MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(ref item);
            MyHud.Chat.ShowMessage(MyTexts.GetString(MySpaceTexts.ChatBotName), text, "Blue");
        }

        protected override void OnClosed()
        {
            MyHud.Chat.ChatClosed();
            base.OnClosed();
        }

        private void Process(string message)
        {
            string str = message.Substring(1);
            IlCompiler.Buffer.Append(str);
        }

        private static void PushHistory(string message)
        {
            m_messageHistory[m_messageHistoryPushTo].Clear().Append(message);
            m_messageHistoryPushTo = HistoryIndexUp(m_messageHistoryPushTo);
            m_messageHistoryShown = m_messageHistoryPushTo;
            m_messageHistory[m_messageHistoryPushTo].Clear();
        }

        public static void SendChatMessage(string message)
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.SendChatMessage(message, MySession.Static.ChatSystem.CurrentChannel, MySession.Static.ChatSystem.CurrentTarget);
            }
            else if (MyGameService.IsActive)
            {
                MyHud.Chat.ShowMessageColoredSP(message, MySession.Static.ChatSystem.CurrentChannel, MySession.Static.ChatSystem.CurrentTarget, null);
            }
            else
            {
                MyHud.Chat.ShowMessage(MySession.Static.LocalHumanPlayer.DisplayName, message, "Blue");
            }
        }

        private void SoftResetFill()
        {
            if (this.m_currentNameFillState == MyNameFillState.Active)
            {
                string str;
                this.m_currentNameFillState = MyNameFillState.Inactive;
                this.m_namefillPrefix_name = str = string.Empty;
                this.m_namefillPrefix_command = str = str;
                this.m_namefillPrefix_completeBefore = this.m_namefillPrefix_completeNew = str;
                this.m_currentNamefillIndex = 0x7fffffff;
                this.m_currentPlayerList = null;
            }
        }

        private bool TestNamefillPrefixse(string complete, out string command, out string nameStump)
        {
            command = string.Empty;
            nameStump = string.Empty;
            foreach (string str in this.NAMEFILL_BASES)
            {
                if ((complete.Length >= str.Length) && str.Equals(complete.Substring(0, str.Length)))
                {
                    command = str;
                    nameStump = complete.Substring(str.Length, complete.Length - str.Length);
                    return true;
                }
            }
            return false;
        }

        public override void UnloadContent()
        {
            if (this.m_chatTextbox != null)
            {
                this.m_chatTextbox.FocusEnded();
            }
            Static = null;
            base.UnloadContent();
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            Vector2 vector = MyGuiScreenHudBase.ConvertHudToNormalizedGuiPosition(ref base.m_position);
            return true;
        }

        public MyGuiControlTextbox ChatTextbox =>
            this.m_chatTextbox;

        private enum MyNameFillState
        {
            Disabled,
            Inactive,
            Active
        }
    }
}


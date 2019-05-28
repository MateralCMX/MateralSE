namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI.HudViewers;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenScenarioMpBase : MyGuiScreenBase
    {
        public static MyGuiScreenScenarioMpBase Static;
        private MyGuiControlMultilineText m_descriptionBox;
        protected MyGuiControlButton m_kickPlayerButton;
        protected MyGuiControlTable m_connectedPlayers;
        private MyGuiControlLabel m_timeoutLabel;
        private MyGuiControlLabel m_canJoinRunningLabel;
        protected MyGuiControlCheckbox m_canJoinRunning;
        protected MyHudControlChat m_chatControl;
        protected MyGuiControlTextbox m_chatTextbox;
        protected MyGuiControlButton m_sendChatButton;
        protected MyGuiControlButton m_startButton;
        private bool m_update;
        private StringBuilder m_editBoxStringBuilder;
        protected static HashSet<ulong> m_readyPlayers = new HashSet<ulong>();

        public MyGuiScreenScenarioMpBase() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(new Vector2(1620f, 1125f) / MyGuiConstants.GUI_OPTIMAL_SIZE), false, null, 0f, 0f)
        {
            this.m_editBoxStringBuilder = new StringBuilder();
            this.RecreateControls(true);
            base.CanHideOthers = false;
            MySyncScenario.PlayerReadyToStartScenario += new Action<ulong>(this.MySyncScenario_PlayerReady);
            MySyncScenario.TimeoutReceived += new Action<int>(this.MySyncScenario_SetTimeout);
            MySyncScenario.CanJoinRunningReceived += new Action<bool>(this.MySyncScenario_SetCanJoinRunning);
            this.m_canJoinRunning.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_canJoinRunning.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnJoinRunningChecked));
            Static = this;
        }

        protected bool CanKick()
        {
            if (!Sync.IsServer)
            {
                return false;
            }
            MyPlayer player = (this.m_connectedPlayers.SelectedRow != null) ? (this.m_connectedPlayers.SelectedRow.UserData as MyPlayer) : null;
            return ((player != null) && (player.Identity.IdentityId != MySession.Static.LocalPlayerId));
        }

        private void ChatTextbox_EnterPressed(MyGuiControlTextbox textBox)
        {
            this.SendMessageFromChatTextBox();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMpScenario";

        public void MySyncScenario_PlayerReady(ulong Id)
        {
            m_readyPlayers.Add(Id);
        }

        public void MySyncScenario_SetCanJoinRunning(bool canJoin)
        {
            this.m_canJoinRunning.IsChecked = canJoin;
        }

        public void MySyncScenario_SetTimeout(int index)
        {
            this.TimeoutCombo.SelectItemByIndex(index);
        }

        protected override void OnClosed()
        {
            MySyncScenario.PlayerReadyToStartScenario -= new Action<ulong>(this.MySyncScenario_PlayerReady);
            MySyncScenario.TimeoutReceived -= new Action<int>(this.MySyncScenario_SetTimeout);
            MySyncScenario.CanJoinRunningReceived -= new Action<bool>(this.MySyncScenario_SetCanJoinRunning);
            this.m_canJoinRunning.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_canJoinRunning.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnJoinRunningChecked));
            m_readyPlayers.Clear();
            base.OnClosed();
            if (base.Cancelled)
            {
                MySessionLoader.UnloadAndExitToMenu();
            }
        }

        private void OnJoinRunningChecked(MyGuiControlCheckbox source)
        {
            MySession.Static.Settings.CanJoinRunning = source.IsChecked;
            MySyncScenario.SetJoinRunning(source.IsChecked);
        }

        private void OnKick2Clicked(MyGuiControlButton sender)
        {
            MyPlayer player = (this.m_connectedPlayers.SelectedRow != null) ? (this.m_connectedPlayers.SelectedRow.UserData as MyPlayer) : null;
            if ((player != null) && (player.Identity.IdentityId != MySession.Static.LocalPlayerId))
            {
                MyMultiplayer.Static.KickClient(player.Id.SteamId, true, true);
            }
        }

        private void OnSendChatClicked(MyGuiControlButton sender)
        {
            this.SendMessageFromChatTextBox();
        }

        protected virtual void OnStartClicked(MyGuiControlButton sender)
        {
        }

        private void OnTimeoutSelected()
        {
            MyScenarioSystem.LoadTimeout = 60 * ((int) this.TimeoutCombo.GetSelectedKey());
            if (Sync.IsServer)
            {
                MySyncScenario.SetTimeout(this.TimeoutCombo.GetSelectedIndex());
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            MyLayoutTable table = new MyLayoutTable(this);
            table.SetColumnWidthsNormalized(new float[] { 50f, 300f, 300f, 300f, 300f, 300f, 50f });
            table.SetRowHeightsNormalized(new float[] { 50f, 450f, 70f, 70f, 70f, 400f, 70f, 70f, 50f });
            MyGuiControlScrollablePanel panel1 = new MyGuiControlScrollablePanel(new MyGuiControlParent());
            panel1.Name = "BriefingScrollableArea";
            panel1.ScrollbarVEnabled = true;
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            panel1.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
            MyGuiControlScrollablePanel control = panel1;
            table.AddWithSize(control, MyAlignH.Left, MyAlignV.Top, 1, 1, 4, 3);
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_descriptionBox = new MyGuiControlMultilineText(new Vector2(-0.227f, 5f), new Vector2(control.Size.X - 0.02f, 11f), backgroundColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Controls.Add(this.m_descriptionBox);
            this.m_connectedPlayers = new MyGuiControlTable();
            this.m_connectedPlayers.Size = new Vector2(490f, 150f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            this.m_connectedPlayers.VisibleRowsCount = 8;
            this.m_connectedPlayers.ColumnsCount = 2;
            float[] p = new float[] { 0.7f, 0.3f };
            this.m_connectedPlayers.SetCustomColumnWidths(p);
            this.m_connectedPlayers.SetColumnName(0, MyTexts.Get(MySpaceTexts.GuiScenarioPlayerName));
            this.m_connectedPlayers.SetColumnName(1, MyTexts.Get(MySpaceTexts.GuiScenarioPlayerStatus));
            Vector2? position = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.Kick);
            backgroundColor = null;
            visibleLinesCount = null;
            this.m_kickPlayerButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(190f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnKick2Clicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_kickPlayerButton.Enabled = this.CanKick();
            position = null;
            position = null;
            backgroundColor = null;
            this.m_timeoutLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.GuiScenarioTimeout), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.TimeoutCombo = new MyGuiControlCombobox();
            this.TimeoutCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnTimeoutSelected);
            visibleLinesCount = null;
            this.TimeoutCombo.AddItem(3L, MyTexts.Get(MySpaceTexts.GuiScenarioTimeout3min), visibleLinesCount, null);
            visibleLinesCount = null;
            this.TimeoutCombo.AddItem(5L, MyTexts.Get(MySpaceTexts.GuiScenarioTimeout5min), visibleLinesCount, null);
            visibleLinesCount = null;
            this.TimeoutCombo.AddItem((long) 10, MyTexts.Get(MySpaceTexts.GuiScenarioTimeout10min), visibleLinesCount, null);
            visibleLinesCount = null;
            this.TimeoutCombo.AddItem(-1L, MyTexts.Get(MySpaceTexts.GuiScenarioTimeoutUnlimited), visibleLinesCount, null);
            this.TimeoutCombo.SelectItemByIndex(0);
            this.TimeoutCombo.Enabled = Sync.IsServer;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_canJoinRunningLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.ScenarioSettings_CanJoinRunningShort), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_canJoinRunning = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_canJoinRunningLabel.Enabled = false;
            this.m_canJoinRunning.Enabled = false;
            position = null;
            text = MyTexts.Get(MySpaceTexts.GuiScenarioStart);
            backgroundColor = null;
            visibleLinesCount = null;
            this.m_startButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(200f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnStartClicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_startButton.Enabled = Sync.IsServer;
            position = null;
            visibleLinesCount = null;
            this.m_chatControl = new MyHudControlChat(MyHud.Chat, position, new Vector2?(new Vector2(1400f, 300f) / MyGuiConstants.GUI_OPTIMAL_SIZE), new VRageMath.Vector4?((VRageMath.Vector4) MyGuiConstants.THEMED_GUI_BACKGROUND_COLOR), "DarkBlue", 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, visibleLinesCount, false);
            this.m_chatControl.BorderEnabled = true;
            this.m_chatControl.BorderColor = (VRageMath.Vector4) Color.CornflowerBlue;
            position = null;
            backgroundColor = null;
            this.m_chatTextbox = new MyGuiControlTextbox(position, null, MyGameService.GetChatMaxMessageSize(), backgroundColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_chatTextbox.Size = new Vector2(1400f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            this.m_chatTextbox.TextScale = 0.8f;
            this.m_chatTextbox.VisualStyle = MyGuiControlTextboxStyleEnum.Default;
            this.m_chatTextbox.EnterPressed += new Action<MyGuiControlTextbox>(this.ChatTextbox_EnterPressed);
            position = null;
            text = MyTexts.Get(MySpaceTexts.GuiScenarioSend);
            backgroundColor = null;
            visibleLinesCount = null;
            this.m_sendChatButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(190f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnSendChatClicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            table.AddWithSize(this.m_connectedPlayers, MyAlignH.Left, MyAlignV.Top, 1, 4, 2, 2);
            table.AddWithSize(this.m_kickPlayerButton, MyAlignH.Left, MyAlignV.Center, 2, 5, 1, 1);
            table.AddWithSize(this.m_timeoutLabel, MyAlignH.Left, MyAlignV.Center, 3, 4, 1, 1);
            table.AddWithSize(this.TimeoutCombo, MyAlignH.Left, MyAlignV.Center, 3, 5, 1, 1);
            table.AddWithSize(this.m_canJoinRunningLabel, MyAlignH.Left, MyAlignV.Center, 4, 4, 1, 1);
            table.AddWithSize(this.m_canJoinRunning, MyAlignH.Right, MyAlignV.Center, 4, 5, 1, 1);
            table.AddWithSize(this.m_chatControl, MyAlignH.Left, MyAlignV.Top, 5, 1, 1, 5);
            table.AddWithSize(this.m_chatTextbox, MyAlignH.Left, MyAlignV.Top, 6, 1, 1, 4);
            table.AddWithSize(this.m_sendChatButton, MyAlignH.Right, MyAlignV.Top, 6, 5, 1, 1);
            table.AddWithSize(this.m_startButton, MyAlignH.Left, MyAlignV.Top, 7, 2, 1, 1);
        }

        private void SendChatMessage(string message)
        {
        }

        private void SendMessageFromChatTextBox()
        {
            this.m_chatTextbox.GetText(this.m_editBoxStringBuilder.Clear());
            string message = this.m_editBoxStringBuilder.ToString();
            this.SendChatMessage(message);
            this.m_chatTextbox.SetText(this.m_editBoxStringBuilder.Clear());
        }

        public override bool Update(bool hasFocus)
        {
            this.m_update = true;
            this.UpdateControls();
            this.m_update = false;
            return base.Update(hasFocus);
        }

        private void UpdateControls()
        {
            if ((MyMultiplayer.Static != null) && (MySession.Static != null))
            {
                this.m_kickPlayerButton.Enabled = this.CanKick();
                UpdatePlayerList(this.m_connectedPlayers);
            }
        }

        private static void UpdatePlayerList(MyGuiControlTable table)
        {
            MyPlayer objB = (table.SelectedRow != null) ? (table.SelectedRow.UserData as MyPlayer) : null;
            table.Clear();
            foreach (MyPlayer player2 in Sync.Players.GetOnlinePlayers())
            {
                string displayName = player2.DisplayName;
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(player2);
                Color? textColor = null;
                MyGuiHighlightTexture? icon = null;
                row.AddCell(new MyGuiControlTable.Cell(displayName, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                if (Sync.ServerId == player2.Id.SteamId)
                {
                    textColor = null;
                    icon = null;
                    row.AddCell(new MyGuiControlTable.Cell("SERVER", null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                }
                else if (m_readyPlayers.Contains(player2.Id.SteamId))
                {
                    textColor = null;
                    icon = null;
                    row.AddCell(new MyGuiControlTable.Cell("ready", null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                }
                else
                {
                    textColor = null;
                    icon = null;
                    row.AddCell(new MyGuiControlTable.Cell("", null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                }
                table.Add(row);
                if (ReferenceEquals(player2, objB))
                {
                    table.SelectedRow = row;
                }
            }
        }

        public MyGuiControlCombobox TimeoutCombo { get; protected set; }

        public string Briefing
        {
            set => 
                (this.m_descriptionBox.Text = new StringBuilder(value));
        }
    }
}


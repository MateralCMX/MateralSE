namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    internal class MyTerminalFactionController
    {
        internal static readonly Color COLOR_CUSTOM_RED = new Color(0xe4, 0x3e, 0x3e);
        internal static readonly Color COLOR_CUSTOM_GREEN = new Color(0x65, 0xb2, 0x5b);
        internal static readonly Color COLOR_CUSTOM_GREY = new Color(0x95, 0xa9, 0xb3);
        private IMyGuiControlsParent m_controlsParent;
        private bool m_userIsFounder;
        private bool m_userIsLeader;
        private long m_selectedUserId;
        private string m_selectedUserName;
        private IMyFaction m_userFaction;
        private IMyFaction m_selectedFaction;
        private MyGuiControlTable m_tableFactions;
        private MyGuiControlButton m_buttonCreate;
        private MyGuiControlButton m_buttonJoin;
        private MyGuiControlButton m_buttonCancelJoin;
        private MyGuiControlButton m_buttonLeave;
        private MyGuiControlButton m_buttonSendPeace;
        private MyGuiControlButton m_buttonCancelPeace;
        private MyGuiControlButton m_buttonAcceptPeace;
        private MyGuiControlButton m_buttonMakeEnemy;
        private MyGuiControlLabel m_labelFactionName;
        private MyGuiControlLabel m_labelFactionDesc;
        private MyGuiControlLabel m_labelFactionPriv;
        private MyGuiControlLabel m_labelMembers;
        private MyGuiControlLabel m_labelAutoAcceptMember;
        private MyGuiControlLabel m_labelAutoAcceptPeace;
        private MyGuiControlCheckbox m_checkAutoAcceptMember;
        private MyGuiControlCheckbox m_checkAutoAcceptPeace;
        private MyGuiControlMultilineText m_textFactionDesc;
        private MyGuiControlMultilineText m_textFactionPriv;
        private MyGuiControlTable m_tableMembers;
        private MyGuiControlButton m_buttonEdit;
        private MyGuiControlButton m_buttonKick;
        private MyGuiControlButton m_buttonPromote;
        private MyGuiControlButton m_buttonDemote;
        private MyGuiControlButton m_buttonAcceptJoin;
        private MyGuiControlButton m_buttonShareProgress;
        private MyGuiControlButton m_buttonAddNpc;

        private void AcceptJoin()
        {
            MyFactionCollection.AcceptJoin(this.TargetFaction.FactionId, this.m_selectedUserId);
        }

        private void AddFaction(IMyFaction faction, Color? color = new Color?(), MyGuiHighlightTexture? icon = new MyGuiHighlightTexture?(), string iconToolTip = null)
        {
            if (this.m_tableFactions != null)
            {
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(faction);
                StringBuilder text = new StringBuilder(faction.Tag);
                StringBuilder builder2 = new StringBuilder(MyStatControlText.SubstituteTexts(faction.Name, null));
                Color? textColor = color;
                MyGuiHighlightTexture? nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(text, text, text.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                textColor = color;
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(builder2, builder2, MyStatControlText.SubstituteTexts(faction.Name, null), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                textColor = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, iconToolTip, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                this.m_tableFactions.Add(row);
            }
        }

        private void AddMember(long playerId, string playerName, bool isLeader, MyMemberComparerEnum status, MyStringId textEnum, Color? color = new Color?())
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(new MyFactionMember(playerId, isLeader, false));
            string toolTip = playerName;
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(playerName), playerId, toolTip, color, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            toolTip = MyTexts.GetString(textEnum);
            icon = null;
            row.AddCell(new MyGuiControlTable.Cell(MyTexts.Get(textEnum), status, toolTip, color, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            this.m_tableMembers.Add(row);
        }

        public void Close()
        {
            this.UnregisterEvents();
            this.m_selectedFaction = null;
            this.m_tableFactions = null;
            this.m_buttonCreate = null;
            this.m_buttonJoin = null;
            this.m_buttonCancelJoin = null;
            this.m_buttonLeave = null;
            this.m_buttonSendPeace = null;
            this.m_buttonCancelPeace = null;
            this.m_buttonAcceptPeace = null;
            this.m_buttonMakeEnemy = null;
            this.m_labelFactionName = null;
            this.m_labelFactionDesc = null;
            this.m_labelFactionPriv = null;
            this.m_labelMembers = null;
            this.m_labelAutoAcceptMember = null;
            this.m_labelAutoAcceptPeace = null;
            this.m_checkAutoAcceptMember = null;
            this.m_checkAutoAcceptPeace = null;
            this.m_textFactionDesc = null;
            this.m_textFactionPriv = null;
            this.m_tableMembers = null;
            this.m_buttonKick = null;
            this.m_buttonAcceptJoin = null;
            this.m_buttonShareProgress = null;
            this.m_controlsParent = null;
        }

        private void Demote()
        {
            MyFactionCollection.DemoteMember(this.TargetFaction.FactionId, this.m_selectedUserId);
        }

        public void Init(IMyGuiControlsParent controlsParent)
        {
            this.m_controlsParent = controlsParent;
            this.RefreshUserInfo();
            this.m_tableFactions = (MyGuiControlTable) controlsParent.Controls.GetControlByName("FactionsTable");
            this.m_tableFactions.SetColumnComparison(0, (a, b) => ((StringBuilder) a.UserData).CompareToIgnoreCase((StringBuilder) b.UserData));
            this.m_tableFactions.SetColumnComparison(1, (a, b) => ((StringBuilder) a.UserData).CompareToIgnoreCase((StringBuilder) b.UserData));
            this.m_tableFactions.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnFactionsTableItemSelected);
            this.RefreshTableFactions();
            MyGuiControlTable.SortStateEnum? sortState = null;
            this.m_tableFactions.SortByColumn(1, sortState, true);
            this.m_buttonCreate = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonCreate");
            this.m_buttonJoin = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonJoin");
            this.m_buttonCancelJoin = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonCancelJoin");
            this.m_buttonLeave = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonLeave");
            this.m_buttonSendPeace = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonSendPeace");
            this.m_buttonCancelPeace = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonCancelPeace");
            this.m_buttonAcceptPeace = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonAcceptPeace");
            this.m_buttonMakeEnemy = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonEnemy");
            this.m_buttonMakeEnemy.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_EnemyToolTip));
            this.m_buttonLeave.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_LeaveToolTip));
            this.m_buttonCreate.ShowTooltipWhenDisabled = true;
            this.m_buttonCreate.TextEnum = MySpaceTexts.TerminalTab_Factions_Create;
            this.m_buttonJoin.TextEnum = MySpaceTexts.TerminalTab_Factions_Join;
            this.m_buttonCancelJoin.TextEnum = MySpaceTexts.TerminalTab_Factions_CancelJoin;
            this.m_buttonLeave.TextEnum = MySpaceTexts.TerminalTab_Factions_Leave;
            this.m_buttonSendPeace.TextEnum = MySpaceTexts.TerminalTab_Factions_Friend;
            this.m_buttonCancelPeace.TextEnum = MySpaceTexts.TerminalTab_Factions_CancelPeaceRequest;
            this.m_buttonAcceptPeace.TextEnum = MySpaceTexts.TerminalTab_Factions_AcceptPeaceRequest;
            this.m_buttonMakeEnemy.TextEnum = MySpaceTexts.TerminalTab_Factions_Enemy;
            this.m_buttonJoin.SetToolTip(MySpaceTexts.TerminalTab_Factions_JoinToolTip);
            this.m_buttonSendPeace.SetToolTip(MySpaceTexts.TerminalTab_Factions_FriendToolTip);
            this.m_buttonCreate.ButtonClicked += new Action<MyGuiControlButton>(this.OnCreateClicked);
            this.m_buttonJoin.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinClicked);
            this.m_buttonCancelJoin.ButtonClicked += new Action<MyGuiControlButton>(this.OnCancelJoinClicked);
            this.m_buttonLeave.ButtonClicked += new Action<MyGuiControlButton>(this.OnLeaveClicked);
            this.m_buttonSendPeace.ButtonClicked += new Action<MyGuiControlButton>(this.OnFriendClicked);
            this.m_buttonCancelPeace.ButtonClicked += new Action<MyGuiControlButton>(this.OnCancelPeaceRequestClicked);
            this.m_buttonAcceptPeace.ButtonClicked += new Action<MyGuiControlButton>(this.OnAcceptFriendClicked);
            this.m_buttonMakeEnemy.ButtonClicked += new Action<MyGuiControlButton>(this.OnEnemyClicked);
            this.m_labelFactionName = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionName");
            this.m_labelFactionDesc = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionDesc");
            this.m_labelFactionPriv = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionPrivate");
            this.m_labelMembers = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionMembers");
            this.m_labelAutoAcceptMember = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionMembersAcceptEveryone");
            this.m_labelAutoAcceptPeace = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelFactionMembersAcceptPeace");
            this.m_labelFactionDesc.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Factions_CreateFactionDescription).ToString();
            this.m_labelFactionPriv.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Factions_Private).ToString();
            this.m_labelMembers.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Factions_Members).ToString();
            this.m_labelAutoAcceptMember.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Factions_AutoAccept).ToString();
            this.m_labelAutoAcceptPeace.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Factions_AutoAcceptRequest).ToString();
            this.m_labelAutoAcceptMember.SetToolTip(MySpaceTexts.TerminalTab_Factions_AutoAcceptToolTip);
            this.m_labelAutoAcceptPeace.SetToolTip(MySpaceTexts.TerminalTab_Factions_AutoAcceptRequestToolTip);
            this.m_textFactionDesc = (MyGuiControlMultilineText) controlsParent.Controls.GetControlByName("textFactionDesc");
            this.m_textFactionPriv = (MyGuiControlMultilineText) controlsParent.Controls.GetControlByName("textFactionPrivate");
            this.m_textFactionDesc.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            this.m_textFactionPriv.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            this.m_tableMembers = (MyGuiControlTable) controlsParent.Controls.GetControlByName("tableMembers");
            this.m_tableMembers.SetColumnComparison(1, (a, b) => ((int) ((MyMemberComparerEnum) a.UserData)).CompareTo((int) ((MyMemberComparerEnum) b.UserData)));
            this.m_tableMembers.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_checkAutoAcceptMember = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("checkFactionMembersAcceptEveryone");
            this.m_checkAutoAcceptPeace = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("checkFactionMembersAcceptPeace");
            this.m_checkAutoAcceptMember.SetToolTip(MySpaceTexts.TerminalTab_Factions_AutoAcceptToolTip);
            this.m_checkAutoAcceptPeace.SetToolTip(MySpaceTexts.TerminalTab_Factions_AutoAcceptRequestToolTip);
            this.m_checkAutoAcceptMember.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkAutoAcceptMember.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
            this.m_checkAutoAcceptPeace.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkAutoAcceptPeace.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
            this.m_buttonEdit = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonEdit");
            this.m_buttonPromote = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonPromote");
            this.m_buttonKick = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonKick");
            this.m_buttonAcceptJoin = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonAcceptJoin");
            this.m_buttonDemote = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonDemote");
            this.m_buttonShareProgress = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonShareProgress");
            this.m_buttonAddNpc = (MyGuiControlButton) controlsParent.Controls.GetControlByName("buttonAddNpc");
            this.m_buttonEdit.SetToolTip(MySpaceTexts.TerminalTab_Factions_FriendToolTip);
            this.m_buttonPromote.SetToolTip(MySpaceTexts.TerminalTab_Factions_PromoteToolTip);
            this.m_buttonKick.SetToolTip(MySpaceTexts.TerminalTab_Factions_KickToolTip);
            this.m_buttonDemote.SetToolTip(MySpaceTexts.TerminalTab_Factions_DemoteToolTip);
            this.m_buttonAcceptJoin.SetToolTip(MySpaceTexts.TerminalTab_Factions_JoinToolTip);
            this.m_buttonShareProgress.SetToolTip(MySpaceTexts.TerminalTab_Factions_ShareProgressToolTip);
            this.m_buttonAddNpc.SetToolTip(MySpaceTexts.AddNpcToFactionHelp);
            this.m_buttonEdit.TextEnum = MyCommonTexts.Edit;
            this.m_buttonPromote.TextEnum = MyCommonTexts.Promote;
            this.m_buttonKick.TextEnum = MyCommonTexts.Kick;
            this.m_buttonAcceptJoin.TextEnum = MyCommonTexts.Accept;
            this.m_buttonDemote.TextEnum = MyCommonTexts.Demote;
            this.m_buttonShareProgress.TextEnum = MySpaceTexts.ShareProgress;
            this.m_buttonAddNpc.TextEnum = MySpaceTexts.AddNpcToFaction;
            this.m_buttonEdit.ButtonClicked += new Action<MyGuiControlButton>(this.OnEditClicked);
            this.m_buttonPromote.ButtonClicked += new Action<MyGuiControlButton>(this.OnPromotePlayerClicked);
            this.m_buttonKick.ButtonClicked += new Action<MyGuiControlButton>(this.OnKickPlayerClicked);
            this.m_buttonAcceptJoin.ButtonClicked += new Action<MyGuiControlButton>(this.OnAcceptJoinClicked);
            this.m_buttonDemote.ButtonClicked += new Action<MyGuiControlButton>(this.OnDemoteClicked);
            this.m_buttonShareProgress.ButtonClicked += new Action<MyGuiControlButton>(this.OnShareProgressClicked);
            this.m_buttonAddNpc.ButtonClicked += new Action<MyGuiControlButton>(this.OnNewNpcClicked);
            MySession.Static.Factions.FactionCreated += new Action<long>(this.OnFactionCreated);
            MySession.Static.Factions.FactionEdited += new Action<long>(this.OnFactionEdited);
            MySession.Static.Factions.FactionStateChanged += new Action<MyFactionStateChange, long, long, long, long>(this.OnFactionsStateChanged);
            MySession.Static.Factions.FactionAutoAcceptChanged += new Action<long, bool, bool>(this.OnAutoAcceptChanged);
            this.Refresh();
        }

        private void KickPlayer()
        {
            MyFactionCollection.KickMember(this.TargetFaction.FactionId, this.m_selectedUserId);
        }

        private void LeaveFaction()
        {
            if (this.m_userFaction != null)
            {
                MyFactionCollection.MemberLeaves(this.m_userFaction.FactionId, MySession.Static.LocalPlayerId);
                this.m_userFaction = null;
                this.Refresh();
            }
        }

        [Event(null, 0x1d8), Reliable, Server]
        public static void NewNpcClickedInternal(long factionId, string npcName)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyIdentity identity = Sync.Players.CreateNewNpcIdentity(npcName, 0L);
                MyFactionCollection.SendJoinRequest(factionId, identity.IdentityId);
            }
        }

        private void OnAcceptFriendClicked(MyGuiControlButton sender)
        {
            MyFactionCollection.AcceptPeace(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId);
        }

        private void OnAcceptJoinClicked(MyGuiControlButton sender)
        {
            if (this.m_tableMembers.SelectedRow != null)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmFactionsAcceptJoin, this.m_selectedUserName), new Action(this.AcceptJoin));
            }
        }

        private void OnAutoAcceptChanged(MyGuiControlCheckbox sender)
        {
            IMyFaction targetFaction = this.TargetFaction;
            if (targetFaction != null)
            {
                MySession.Static.Factions.ChangeAutoAccept(targetFaction.FactionId, MySession.Static.LocalPlayerId, this.m_checkAutoAcceptMember.IsChecked, this.m_checkAutoAcceptPeace.IsChecked);
            }
        }

        private void OnAutoAcceptChanged(long factionId, bool autoAcceptMember, bool autoAcceptPeace)
        {
            this.RefreshFactionProperties();
        }

        private void OnCancelJoinClicked(MyGuiControlButton sender)
        {
            MyFactionCollection.CancelJoinRequest(this.m_selectedFaction.FactionId, MySession.Static.LocalPlayerId);
        }

        private void OnCancelPeaceRequestClicked(MyGuiControlButton sender)
        {
            MyFactionCollection.CancelPeaceRequest(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId);
        }

        private void OnCreateClicked(MyGuiControlButton sender)
        {
            MyGuiScreenCreateOrEditFaction screen = (MyGuiScreenCreateOrEditFaction) MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.CreateFactionScreen, Array.Empty<object>());
            screen.Init(ref this.m_userFaction);
            MyGuiSandbox.AddScreen(screen);
        }

        private void OnDemoteClicked(MyGuiControlButton sender)
        {
            if (this.m_tableMembers.SelectedRow != null)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmFactionsDemote, this.m_selectedUserName), new Action(this.Demote));
            }
        }

        private void OnEditClicked(MyGuiControlButton sender)
        {
            IMyFaction targetFaction = this.TargetFaction;
            if (targetFaction != null)
            {
                MyGuiScreenCreateOrEditFaction screen = (MyGuiScreenCreateOrEditFaction) MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.CreateFactionScreen, Array.Empty<object>());
                screen.Init(ref targetFaction);
                MyGuiSandbox.AddScreen(screen);
            }
        }

        private void OnEnemyClicked(MyGuiControlButton sender)
        {
            MyFactionCollection.DeclareWar(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId);
        }

        private void OnFactionCreated(long insertedId)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetFactionById(insertedId);
            MyGuiHighlightTexture? icon = null;
            this.AddFaction(faction, new Color?(faction.IsMember(MySession.Static.LocalPlayerId) ? COLOR_CUSTOM_GREEN : COLOR_CUSTOM_RED), icon, null);
            this.Refresh();
            this.RefreshTableFactions();
            this.m_tableFactions.Sort(false);
            this.m_tableFactions.SelectedRowIndex = new int?(this.m_tableFactions.FindIndex(row => ((MyFaction) row.UserData).FactionId == insertedId));
            MyGuiControlTable.EventArgs args = new MyGuiControlTable.EventArgs();
            this.OnFactionsTableItemSelected(this.m_tableFactions, args);
        }

        private void OnFactionEdited(long editedId)
        {
            this.RefreshTableFactions();
            this.m_tableFactions.SelectedRowIndex = new int?(this.m_tableFactions.FindIndex(row => ((MyFaction) row.UserData).FactionId == editedId));
            MyGuiControlTable.EventArgs args = new MyGuiControlTable.EventArgs();
            this.OnFactionsTableItemSelected(this.m_tableFactions, args);
            this.Refresh();
        }

        private void OnFactionsStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (((MySession.Static != null) && (MySession.Static.Factions != null)) && (this.m_tableFactions != null))
            {
                MyGuiHighlightTexture? nullable2;
                IMyFaction faction = MySession.Static.Factions.TryGetFactionById(fromFactionId);
                IMyFaction faction2 = MySession.Static.Factions.TryGetFactionById(toFactionId);
                switch (action)
                {
                    case MyFactionStateChange.RemoveFaction:
                        this.RemoveFaction(toFactionId);
                        break;

                    case MyFactionStateChange.SendPeaceRequest:
                        if (this.m_userFaction == null)
                        {
                            return;
                        }
                        if (this.m_userFaction.FactionId == fromFactionId)
                        {
                            this.RemoveFaction(toFactionId);
                            this.AddFaction(faction2, new Color?(COLOR_CUSTOM_RED), new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_SENT_WHITE_FLAG), MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_SentPeace));
                        }
                        else if (this.m_userFaction.FactionId == toFactionId)
                        {
                            this.RemoveFaction(fromFactionId);
                            this.AddFaction(faction, new Color?(COLOR_CUSTOM_RED), new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_WHITE_FLAG), MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_PendingPeace));
                        }
                        break;

                    case MyFactionStateChange.CancelPeaceRequest:
                    case MyFactionStateChange.DeclareWar:
                        if (this.m_userFaction == null)
                        {
                            return;
                        }
                        if (this.m_userFaction.FactionId == fromFactionId)
                        {
                            this.RemoveFaction(toFactionId);
                            nullable2 = null;
                            this.AddFaction(faction2, new Color?(COLOR_CUSTOM_RED), nullable2, null);
                        }
                        else if (this.m_userFaction.FactionId == toFactionId)
                        {
                            this.RemoveFaction(fromFactionId);
                            nullable2 = null;
                            this.AddFaction(faction, new Color?(COLOR_CUSTOM_RED), nullable2, null);
                        }
                        break;

                    case MyFactionStateChange.AcceptPeace:
                        Color? nullable;
                        if (this.m_userFaction == null)
                        {
                            return;
                        }
                        if (this.m_userFaction.FactionId == fromFactionId)
                        {
                            this.RemoveFaction(toFactionId);
                            nullable = null;
                            nullable2 = null;
                            this.AddFaction(faction2, nullable, nullable2, null);
                        }
                        else if (this.m_userFaction.FactionId == toFactionId)
                        {
                            this.RemoveFaction(fromFactionId);
                            nullable = null;
                            nullable2 = null;
                            this.AddFaction(faction, nullable, nullable2, null);
                        }
                        break;

                    default:
                        this.OnMemberStateChanged(action, faction, playerId);
                        break;
                }
                this.m_tableFactions.Sort(false);
                this.m_tableFactions.SelectedRowIndex = new int?(this.m_tableFactions.FindIndex(row => ((MyFaction) row.UserData).FactionId == toFactionId));
                MyGuiControlTable.EventArgs args = new MyGuiControlTable.EventArgs();
                this.OnFactionsTableItemSelected(this.m_tableFactions, args);
                this.Refresh();
            }
        }

        private void OnFactionsTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            if (sender.SelectedRow != null)
            {
                this.m_selectedFaction = (MyFaction) sender.SelectedRow.UserData;
                this.m_labelFactionName.Text = $"{this.m_selectedFaction.Tag}.{MyStatControlText.SubstituteTexts(this.m_selectedFaction.Name, null)}";
                this.m_textFactionDesc.Text = new StringBuilder(this.m_selectedFaction.Description);
                this.m_textFactionPriv.Text = new StringBuilder(this.m_selectedFaction.PrivateInfo);
                this.RefreshTableMembers();
            }
            this.m_tableMembers.Sort(false);
            this.RefreshJoinButton();
            this.RefreshDiplomacyButtons();
            this.RefreshFactionProperties();
        }

        private void OnFriendClicked(MyGuiControlButton sender)
        {
            MyFactionCollection.SendPeaceRequest(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId);
        }

        private void OnJoinClicked(MyGuiControlButton sender)
        {
            if ((MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION) && !MyBlockLimits.IsFactionChangePossible(MySession.Static.LocalPlayerId, this.m_selectedFaction.FactionId))
            {
                this.ShowErrorBox(new StringBuilder(MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_JoinLimitsExceeded)));
            }
            MyFactionCollection.SendJoinRequest(this.m_selectedFaction.FactionId, MySession.Static.LocalPlayerId);
        }

        private void OnKickPlayerClicked(MyGuiControlButton sender)
        {
            if (this.m_tableMembers.SelectedRow != null)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmFactionsKickPlayer, this.m_selectedUserName), new Action(this.KickPlayer));
            }
        }

        private void OnLeaveClicked(MyGuiControlButton sender)
        {
            if (this.m_selectedFaction.FactionId == this.m_userFaction.FactionId)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmFactionsLeave, MyStatControlText.SubstituteTexts(this.m_userFaction.Name, null)), new Action(this.LeaveFaction));
            }
        }

        private void OnMemberStateChanged(MyFactionStateChange action, IMyFaction fromFaction, long playerId)
        {
            MyIdentity identity = Sync.Players.TryGetIdentity(playerId);
            if (identity == null)
            {
                object[] objArray1 = new object[] { "ERROR: Faction ", MyStatControlText.SubstituteTexts(fromFaction.Name, null), " member ", playerId, " does not exists! " };
                MyLog.Default.WriteLine(string.Concat(objArray1));
            }
            else
            {
                Color? nullable;
                this.RemoveMember(playerId);
                switch (action)
                {
                    case MyFactionStateChange.FactionMemberSendJoin:
                        this.AddMember(playerId, identity.DisplayName, false, MyMemberComparerEnum.Applicant, MyCommonTexts.Applicant, new Color?(COLOR_CUSTOM_GREY));
                        break;

                    case MyFactionStateChange.FactionMemberAcceptJoin:
                    case MyFactionStateChange.FactionMemberDemote:
                        nullable = null;
                        this.AddMember(playerId, identity.DisplayName, false, MyMemberComparerEnum.Member, MyCommonTexts.Member, nullable);
                        break;

                    case MyFactionStateChange.FactionMemberPromote:
                        nullable = null;
                        this.AddMember(playerId, identity.DisplayName, true, MyMemberComparerEnum.Leader, MyCommonTexts.Leader, nullable);
                        break;

                    default:
                        break;
                }
                this.RefreshUserInfo();
                this.RefreshTableFactions();
                this.m_tableMembers.Sort(false);
            }
        }

        private void OnNewNpcClicked(MyGuiControlButton sender)
        {
            string str = this.TargetFaction.Tag + " NPC" + MyRandom.Instance.Next(0x3e8, 0x270f);
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, string>(x => new Action<long, string>(MyTerminalFactionController.NewNpcClickedInternal), this.TargetFaction.FactionId, str, targetEndpoint, position);
        }

        private void OnPromotePlayerClicked(MyGuiControlButton sender)
        {
            if (this.m_tableMembers.SelectedRow != null)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmFactionsPromote, this.m_selectedUserName), new Action(this.PromotePlayer));
            }
        }

        private void OnShareProgressClicked(MyGuiControlButton sender)
        {
            if (this.m_tableMembers.SelectedRow != null)
            {
                this.ShowConfirmBox(new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxConfirmShareResearch, this.m_selectedUserName), new Action(this.ShareProgress));
            }
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            this.RefreshRightSideButtons(sender.SelectedRow);
        }

        private void PromotePlayer()
        {
            MyFactionCollection.PromoteMember(this.TargetFaction.FactionId, this.m_selectedUserId);
        }

        private void Refresh()
        {
            this.RefreshUserInfo();
            this.RefreshCreateButton();
            this.RefreshJoinButton();
            this.RefreshDiplomacyButtons();
            this.RefreshRightSideButtons(null);
            this.RefreshFactionProperties();
        }

        private void RefreshCreateButton()
        {
            if (this.m_buttonCreate != null)
            {
                if (this.m_userFaction != null)
                {
                    this.m_buttonCreate.Enabled = false;
                    this.m_buttonCreate.SetToolTip(MySpaceTexts.TerminalTab_Factions_BeforeCreateLeave);
                }
                else if ((MySession.Static.MaxFactionsCount == 0) || ((MySession.Static.MaxFactionsCount > 0) && (MySession.Static.Factions.HumansCount() < MySession.Static.MaxFactionsCount)))
                {
                    this.m_buttonCreate.Enabled = true;
                    this.m_buttonCreate.SetToolTip(MySpaceTexts.TerminalTab_Factions_CreateToolTip);
                }
                else
                {
                    this.m_buttonCreate.Enabled = false;
                    this.m_buttonCreate.SetToolTip(MySpaceTexts.TerminalTab_Factions_MaxCountReachedToolTip);
                }
            }
        }

        private void RefreshDiplomacyButtons()
        {
            this.m_buttonSendPeace.Enabled = false;
            this.m_buttonCancelPeace.Enabled = false;
            this.m_buttonAcceptPeace.Enabled = false;
            this.m_buttonMakeEnemy.Enabled = false;
            this.m_buttonCancelPeace.Visible = false;
            this.m_buttonAcceptPeace.Visible = false;
            if ((!this.m_userIsLeader || (this.m_selectedFaction == null)) || (this.m_selectedFaction.FactionId == this.m_userFaction.FactionId))
            {
                this.m_buttonSendPeace.Visible = true;
            }
            else if (!MySession.Static.Factions.AreFactionsEnemies(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId))
            {
                this.m_buttonMakeEnemy.Enabled = true;
            }
            else if (MySession.Static.Factions.IsPeaceRequestStateSent(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId))
            {
                this.m_buttonSendPeace.Visible = false;
                this.m_buttonCancelPeace.Visible = true;
                this.m_buttonCancelPeace.Enabled = true;
            }
            else if (!MySession.Static.Factions.IsPeaceRequestStatePending(this.m_userFaction.FactionId, this.m_selectedFaction.FactionId))
            {
                this.m_buttonSendPeace.Visible = true;
                this.m_buttonSendPeace.Enabled = true;
            }
            else
            {
                this.m_buttonSendPeace.Visible = false;
                this.m_buttonAcceptPeace.Visible = true;
                this.m_buttonAcceptPeace.Enabled = true;
            }
        }

        private void RefreshFactionProperties()
        {
            bool flag = MySession.Static.IsUserAdmin(Sync.MyId);
            this.m_checkAutoAcceptMember.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkAutoAcceptMember.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
            this.m_checkAutoAcceptPeace.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkAutoAcceptPeace.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
            this.m_checkAutoAcceptMember.Enabled = false;
            this.m_checkAutoAcceptPeace.Enabled = false;
            this.m_buttonEdit.Enabled = false;
            this.m_buttonKick.Enabled = false;
            this.m_buttonPromote.Enabled = false;
            this.m_buttonDemote.Enabled = false;
            this.m_buttonAcceptJoin.Enabled = false;
            this.m_buttonShareProgress.Enabled = false;
            this.m_buttonAddNpc.Enabled = false;
            if (this.m_tableFactions.SelectedRow == null)
            {
                this.m_tableMembers.Clear();
                goto TR_0000;
            }
            else
            {
                this.m_selectedFaction = (MyFaction) this.m_tableFactions.SelectedRow.UserData;
                this.m_labelFactionName.Text = $"{this.m_selectedFaction.Tag}.{MyStatControlText.SubstituteTexts(this.m_selectedFaction.Name, null)}";
                this.m_textFactionDesc.Text = new StringBuilder(this.m_selectedFaction.Description);
                this.m_checkAutoAcceptMember.IsChecked = this.m_selectedFaction.AutoAcceptMember;
                this.m_checkAutoAcceptPeace.IsChecked = this.m_selectedFaction.AutoAcceptPeace;
                if (!flag && ((this.m_userFaction == null) || (this.m_userFaction.FactionId != this.m_selectedFaction.FactionId)))
                {
                    this.m_textFactionPriv.Text = null;
                    goto TR_0000;
                }
            }
            this.m_textFactionPriv.Text = new StringBuilder(this.m_selectedFaction.PrivateInfo);
            if (this.m_userIsLeader | flag)
            {
                this.m_checkAutoAcceptMember.Enabled = true;
                this.m_checkAutoAcceptPeace.Enabled = true;
                this.m_buttonEdit.Enabled = true;
            }
            if (MySession.Static.IsUserSpaceMaster(MySession.Static.LocalHumanPlayer.Client.SteamUserId) | flag)
            {
                this.m_buttonAddNpc.Enabled = true;
            }
        TR_0000:
            this.m_checkAutoAcceptMember.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkAutoAcceptMember.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
            this.m_checkAutoAcceptPeace.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkAutoAcceptPeace.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
        }

        private void RefreshJoinButton()
        {
            this.m_buttonLeave.Visible = false;
            this.m_buttonJoin.Visible = false;
            this.m_buttonCancelJoin.Visible = false;
            this.m_buttonLeave.Enabled = false;
            this.m_buttonJoin.Enabled = false;
            this.m_buttonCancelJoin.Enabled = false;
            if ((this.m_userFaction != null) && (MySession.Static.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.PER_FACTION))
            {
                int num1;
                this.m_buttonLeave.Visible = true;
                if ((this.m_tableFactions.SelectedRow == null) || (this.m_tableFactions.SelectedRow.UserData != this.m_userFaction))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = 1;
                }
                this.m_buttonLeave.Enabled = (bool) num1;
            }
            else if (this.m_tableFactions.SelectedRow == null)
            {
                this.m_buttonJoin.Visible = true;
                this.m_buttonJoin.Enabled = false;
            }
            else if (this.m_selectedFaction.JoinRequests.ContainsKey(MySession.Static.LocalPlayerId))
            {
                this.m_buttonCancelJoin.Visible = true;
                this.m_buttonCancelJoin.Enabled = true;
                this.m_buttonJoin.Visible = false;
            }
            else if (!this.m_selectedFaction.AcceptHumans || ReferenceEquals(this.m_userFaction, this.m_selectedFaction))
            {
                this.m_buttonJoin.Visible = true;
                this.m_buttonJoin.Enabled = false;
            }
            else
            {
                this.m_buttonJoin.Visible = true;
                this.m_buttonJoin.Enabled = true;
            }
        }

        private void RefreshRightSideButtons(MyGuiControlTable.Row selected)
        {
            bool flag = MySession.Static.IsUserAdmin(Sync.MyId);
            this.m_buttonPromote.Enabled = false;
            this.m_buttonKick.Enabled = false;
            this.m_buttonAcceptJoin.Enabled = false;
            this.m_buttonDemote.Enabled = false;
            this.m_buttonShareProgress.Enabled = false;
            if (selected != null)
            {
                MyFactionMember userData = (MyFactionMember) selected.UserData;
                this.m_selectedUserId = userData.PlayerId;
                MyIdentity identity = Sync.Players.TryGetIdentity(userData.PlayerId);
                this.m_selectedUserName = identity.DisplayName;
                if ((this.m_selectedUserId != MySession.Static.LocalPlayerId) | flag)
                {
                    if ((this.m_userIsFounder | flag) && this.TargetFaction.IsLeader(this.m_selectedUserId))
                    {
                        this.m_buttonKick.Enabled = true;
                        this.m_buttonDemote.Enabled = true;
                    }
                    else if ((this.m_userIsFounder | flag) && this.TargetFaction.IsMember(this.m_selectedUserId))
                    {
                        this.m_buttonKick.Enabled = true;
                        this.m_buttonPromote.Enabled = true;
                    }
                    else if (((this.m_userIsLeader | flag) && (this.TargetFaction.IsMember(this.m_selectedUserId) && !this.TargetFaction.IsLeader(this.m_selectedUserId))) && !this.TargetFaction.IsFounder(this.m_selectedUserId))
                    {
                        this.m_buttonKick.Enabled = true;
                    }
                    else if (((this.m_userIsLeader || this.m_userIsFounder) | flag) && this.TargetFaction.JoinRequests.ContainsKey(this.m_selectedUserId))
                    {
                        this.m_buttonAcceptJoin.Enabled = true;
                    }
                    if ((this.m_userFaction != null) && this.TargetFaction.IsMember(this.m_selectedUserId))
                    {
                        this.m_buttonShareProgress.Enabled = true;
                    }
                }
            }
        }

        private void RefreshTableFactions()
        {
            this.m_tableFactions.Clear();
            foreach (KeyValuePair<long, MyFaction> pair in MySession.Static.Factions)
            {
                MyFaction faction = pair.Value;
                Color? color = null;
                MyGuiHighlightTexture? icon = null;
                string iconToolTip = null;
                if (this.m_userFaction == null)
                {
                    color = new Color?(COLOR_CUSTOM_RED);
                    if (faction.JoinRequests.ContainsKey(MySession.Static.LocalPlayerId))
                    {
                        icon = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_SENT_JOIN_REQUEST);
                        iconToolTip = MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_SentJoinToolTip);
                    }
                }
                else
                {
                    if (this.m_userFaction.FactionId == faction.FactionId)
                    {
                        color = new Color?(COLOR_CUSTOM_GREEN);
                    }
                    else if (MySession.Static.Factions.AreFactionsEnemies(this.m_userFaction.FactionId, faction.FactionId))
                    {
                        color = new Color?(COLOR_CUSTOM_RED);
                    }
                    if (MySession.Static.Factions.IsPeaceRequestStateSent(this.m_userFaction.FactionId, faction.FactionId))
                    {
                        icon = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_SENT_WHITE_FLAG);
                        iconToolTip = MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_SentPeace);
                    }
                    else if (MySession.Static.Factions.IsPeaceRequestStatePending(this.m_userFaction.FactionId, faction.FactionId))
                    {
                        icon = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_WHITE_FLAG);
                        iconToolTip = MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_PendingPeace);
                    }
                }
                this.AddFaction(faction, color, icon, iconToolTip);
            }
            this.m_tableFactions.Sort(false);
        }

        private void RefreshTableMembers()
        {
            string displayName;
            MyGuiHighlightTexture? nullable2;
            this.m_tableMembers.Clear();
            DictionaryReader<long, MyFactionMember> members = this.m_selectedFaction.Members;
            foreach (KeyValuePair<long, MyFactionMember> pair in members)
            {
                MyFactionMember userData = pair.Value;
                MyIdentity identity = Sync.Players.TryGetIdentity(userData.PlayerId);
                if (identity != null)
                {
                    MyGuiControlTable.Row row = new MyGuiControlTable.Row(userData);
                    MyMemberComparerEnum member = MyMemberComparerEnum.Member;
                    MyStringId founder = MyCommonTexts.Member;
                    Color? textColor = null;
                    if (this.m_selectedFaction.IsFounder(userData.PlayerId))
                    {
                        member = MyMemberComparerEnum.Founder;
                        founder = MyCommonTexts.Founder;
                    }
                    else if (this.m_selectedFaction.IsLeader(userData.PlayerId))
                    {
                        member = MyMemberComparerEnum.Leader;
                        founder = MyCommonTexts.Leader;
                    }
                    else if (this.m_selectedFaction.JoinRequests.ContainsKey(userData.PlayerId))
                    {
                        textColor = new Color?(COLOR_CUSTOM_GREY);
                        member = MyMemberComparerEnum.Applicant;
                        founder = MyCommonTexts.Applicant;
                    }
                    displayName = identity.DisplayName;
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(identity.DisplayName), pair, displayName, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    displayName = MyTexts.GetString(founder);
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(MyTexts.Get(founder), member, displayName, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    this.m_tableMembers.Add(row);
                }
            }
            foreach (KeyValuePair<long, MyFactionMember> pair2 in this.m_selectedFaction.JoinRequests)
            {
                MyFactionMember userData = pair2.Value;
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(userData);
                MyIdentity identity2 = Sync.Players.TryGetIdentity(userData.PlayerId);
                if (identity2 != null)
                {
                    displayName = identity2.DisplayName;
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(identity2.DisplayName), pair2, displayName, new Color?(COLOR_CUSTOM_GREY), nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(MyTexts.Get(MyCommonTexts.Applicant), MyMemberComparerEnum.Applicant, MyTexts.GetString(MyCommonTexts.Applicant), new Color?(COLOR_CUSTOM_GREY), nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    this.m_tableMembers.Add(row);
                }
            }
        }

        private void RefreshUserInfo()
        {
            this.m_userIsFounder = false;
            this.m_userIsLeader = false;
            this.m_userFaction = MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId);
            if (this.m_userFaction != null)
            {
                this.m_userIsFounder = this.m_userFaction.IsFounder(MySession.Static.LocalPlayerId);
                this.m_userIsLeader = this.m_userFaction.IsLeader(MySession.Static.LocalPlayerId);
            }
        }

        private void RemoveFaction(long factionId)
        {
            if (this.m_tableFactions != null)
            {
                this.m_tableFactions.Remove(row => ((MyFaction) row.UserData).FactionId == factionId);
            }
        }

        private void RemoveMember(long playerId)
        {
            this.m_tableMembers.Remove(row => ((MyFactionMember) row.UserData).PlayerId == playerId);
        }

        private void ShareProgress()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MySessionComponentResearch.CallShareResearch), this.m_selectedUserId, targetEndpoint, position);
        }

        private void ShowConfirmBox(StringBuilder text, Action callback)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, text, messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                if (retval == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    callback();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.NO, true, size);
            screen.SkipTransition = true;
            screen.CloseBeforeCallback = true;
            screen.CanHideOthers = false;
            MyGuiSandbox.AddScreen(screen);
        }

        private void ShowErrorBox(StringBuilder text)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, text, messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            screen.SkipTransition = true;
            screen.CloseBeforeCallback = true;
            screen.CanHideOthers = false;
            MyGuiSandbox.AddScreen(screen);
        }

        private void UnregisterEvents()
        {
            if (this.m_controlsParent != null)
            {
                MySession.Static.Factions.FactionCreated -= new Action<long>(this.OnFactionCreated);
                MySession.Static.Factions.FactionEdited -= new Action<long>(this.OnFactionEdited);
                MySession.Static.Factions.FactionStateChanged -= new Action<MyFactionStateChange, long, long, long, long>(this.OnFactionsStateChanged);
                MySession.Static.Factions.FactionAutoAcceptChanged -= new Action<long, bool, bool>(this.OnAutoAcceptChanged);
                this.m_tableFactions.ItemSelected -= new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnFactionsTableItemSelected);
                this.m_tableMembers.ItemSelected -= new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
                this.m_checkAutoAcceptMember.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkAutoAcceptMember.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
                this.m_checkAutoAcceptPeace.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkAutoAcceptPeace.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAutoAcceptChanged));
                this.m_buttonCreate.ButtonClicked -= new Action<MyGuiControlButton>(this.OnCreateClicked);
                this.m_buttonJoin.ButtonClicked -= new Action<MyGuiControlButton>(this.OnJoinClicked);
                this.m_buttonCancelJoin.ButtonClicked -= new Action<MyGuiControlButton>(this.OnCancelJoinClicked);
                this.m_buttonLeave.ButtonClicked -= new Action<MyGuiControlButton>(this.OnLeaveClicked);
                this.m_buttonSendPeace.ButtonClicked -= new Action<MyGuiControlButton>(this.OnFriendClicked);
                this.m_buttonAcceptPeace.ButtonClicked -= new Action<MyGuiControlButton>(this.OnAcceptFriendClicked);
                this.m_buttonMakeEnemy.ButtonClicked -= new Action<MyGuiControlButton>(this.OnEnemyClicked);
                this.m_buttonEdit.ButtonClicked -= new Action<MyGuiControlButton>(this.OnEditClicked);
                this.m_buttonPromote.ButtonClicked -= new Action<MyGuiControlButton>(this.OnPromotePlayerClicked);
                this.m_buttonKick.ButtonClicked -= new Action<MyGuiControlButton>(this.OnKickPlayerClicked);
                this.m_buttonAcceptJoin.ButtonClicked -= new Action<MyGuiControlButton>(this.OnAcceptJoinClicked);
                this.m_buttonDemote.ButtonClicked -= new Action<MyGuiControlButton>(this.OnDemoteClicked);
                this.m_buttonShareProgress.ButtonClicked -= new Action<MyGuiControlButton>(this.OnShareProgressClicked);
                this.m_buttonAddNpc.ButtonClicked -= new Action<MyGuiControlButton>(this.OnNewNpcClicked);
            }
        }

        private IMyFaction TargetFaction
        {
            get
            {
                if ((this.m_selectedFaction == null) || !MySession.Static.IsUserAdmin(Sync.MyId))
                {
                    return this.m_userFaction;
                }
                return this.m_selectedFaction;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalFactionController.<>c <>9 = new MyTerminalFactionController.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__40_0;
            public static Comparison<MyGuiControlTable.Cell> <>9__40_1;
            public static Comparison<MyGuiControlTable.Cell> <>9__40_2;
            public static Func<IMyEventOwner, Action<long>> <>9__66_0;
            public static Func<IMyEventOwner, Action<long, string>> <>9__67_0;

            internal int <Init>b__40_0(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                ((StringBuilder) a.UserData).CompareToIgnoreCase(((StringBuilder) b.UserData));

            internal int <Init>b__40_1(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                ((StringBuilder) a.UserData).CompareToIgnoreCase(((StringBuilder) b.UserData));

            internal int <Init>b__40_2(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                ((int) ((MyTerminalFactionController.MyMemberComparerEnum) a.UserData)).CompareTo((int) ((MyTerminalFactionController.MyMemberComparerEnum) b.UserData));

            internal Action<long, string> <OnNewNpcClicked>b__67_0(IMyEventOwner x) => 
                new Action<long, string>(MyTerminalFactionController.NewNpcClickedInternal);

            internal Action<long> <ShareProgress>b__66_0(IMyEventOwner x) => 
                new Action<long>(MySessionComponentResearch.CallShareResearch);
        }

        internal enum MyMemberComparerEnum
        {
            Founder,
            Leader,
            Member,
            Applicant
        }
    }
}


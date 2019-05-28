namespace Sandbox.Game.Screens.Terminal
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalPropertiesController
    {
        private MyGuiControlCombobox m_shipsInRange;
        private MyGuiControlButton m_button;
        private MyGuiControlTable m_shipsData;
        private VRage.Game.Entity.MyEntity m_interactedEntityRepresentative;
        private VRage.Game.Entity.MyEntity m_openInventoryInteractedEntityRepresentative;
        private VRage.Game.Entity.MyEntity m_interactedEntity;
        private bool m_isRemote;
        private int m_columnToSort;
        private HashSet<MyDataReceiver> m_tmpAntennas = new HashSet<MyDataReceiver>();
        [CompilerGenerated]
        private Action ButtonClicked;
        private Dictionary<long, CubeGridInfo> m_tmpGridInfoOutput = new Dictionary<long, CubeGridInfo>();
        private HashSet<MyDataBroadcaster> m_tmpBroadcasters = new HashSet<MyDataBroadcaster>();
        private List<MyDataBroadcaster> m_tempBroadcasters = new List<MyDataBroadcaster>();
        private List<MyDataBroadcaster> m_tempSendingToGrid = new List<MyDataBroadcaster>();
        private List<MyDataBroadcaster> m_tempReceivingFromGrid = new List<MyDataBroadcaster>();
        private HashSet<MyAntennaSystem.BroadcasterInfo> previousMutualConnectionGrids;
        private HashSet<CubeGridInfo> previousShipInfo;
        private int cnt;

        public event Action ButtonClicked
        {
            [CompilerGenerated] add
            {
                Action buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action a = buttonClicked;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    buttonClicked = Interlocked.CompareExchange<Action>(ref this.ButtonClicked, action3, a);
                    if (ReferenceEquals(buttonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action source = buttonClicked;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    buttonClicked = Interlocked.CompareExchange<Action>(ref this.ButtonClicked, action3, source);
                    if (ReferenceEquals(buttonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        private MyRefuseReason CanTakeTerminal(CubeGridInfo gridInfo)
        {
            if (!gridInfo.Owned)
            {
                return MyRefuseReason.NoOwner;
            }
            if (((gridInfo.Status == MyCubeGridConnectionStatus.OutOfBroadcastingRange) && (MySession.Static.ControlledEntity.Entity is MyCharacter)) && !(MySession.Static.ControlledEntity.Entity as MyCharacter).RadioBroadcaster.Enabled)
            {
                return MyRefuseReason.PlayerBroadcastOff;
            }
            if ((gridInfo.Status == MyCubeGridConnectionStatus.OutOfBroadcastingRange) || (gridInfo.Status == MyCubeGridConnectionStatus.OutOfReceivingRange))
            {
                return MyRefuseReason.NoStableConnection;
            }
            return MyRefuseReason.NoProblem;
        }

        private bool CanTakeTerminalOuter(CubeGridInfo gridInfo)
        {
            bool flag = true;
            MyRefuseReason reason = this.CanTakeTerminal(gridInfo);
            if (((reason - 2) <= MyRefuseReason.NoMainRemoteControl) || ((reason - 5) <= MyRefuseReason.NoMainRemoteControl))
            {
                flag = false;
            }
            return flag;
        }

        public void Close()
        {
            if (this.m_shipsInRange != null)
            {
                this.m_shipsInRange.ItemSelected -= new MyGuiControlCombobox.ItemSelectedDelegate(this.shipsInRange_ItemSelected);
                this.m_shipsInRange.ClearItems();
                this.m_shipsInRange = null;
            }
            if (this.m_shipsData != null)
            {
                this.m_shipsData.ColumnClicked -= new Action<MyGuiControlTable, int>(this.shipsData_ColumnClicked);
                this.m_shipsData.Clear();
                this.m_shipsData = null;
            }
            if (this.m_button != null)
            {
                this.m_button.ButtonClicked -= new Action<MyGuiControlButton>(this.Menu_ButtonClicked);
                this.m_button = null;
            }
        }

        private MyGuiControlTable.Cell CreateControlCell(CubeGridInfo gridInfo, bool isActive)
        {
            Color? textColor = null;
            MyGuiHighlightTexture? icon = null;
            MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(null, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            Vector2 vector = new Vector2(0.1f, this.m_shipsData.RowHeight * 0.8f);
            MyRefuseReason remoteStatus = gridInfo.RemoteStatus;
            if ((remoteStatus <= MyRefuseReason.NoMainRemoteControl) || (remoteStatus == MyRefuseReason.Forbidden))
            {
                isActive = false;
            }
            isActive &= this.CanTakeTerminalOuter(gridInfo);
            Vector2? position = null;
            StringBuilder text = MyTexts.Get(MySpaceTexts.BroadcastScreen_TakeControlButton);
            Vector4? colorMask = null;
            int? buttonIndex = null;
            cell.Control = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(vector), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnButtonClicked_TakeControl), GuiSounds.MouseClick, 1f, buttonIndex, false);
            cell.Control.ShowTooltipWhenDisabled = true;
            cell.Control.Enabled = isActive;
            if (cell.Control.Enabled)
            {
                cell.Control.SetToolTip(MySpaceTexts.BroadcastScreen_TakeControlButton_ToolTip);
            }
            else
            {
                cell.Control.SetToolTip(MySpaceTexts.BroadcastScreen_TakeControlButtonDisabled_ToolTip);
            }
            this.m_shipsData.Controls.Add(cell.Control);
            return cell;
        }

        private MyGuiControlTable.Cell CreateStatusIcons(CubeGridInfo gridInfo, bool isActive)
        {
            bool flag2;
            bool flag3;
            MyStringId id2;
            Color? textColor = null;
            MyGuiHighlightTexture? icon = null;
            MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(null, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            float y = this.m_shipsData.RowHeight * 0.7f;
            bool flag = flag2 = flag3 = isActive;
            MyStringId text = id2 = MyStringId.NullOrEmpty;
            StringBuilder builder = new StringBuilder();
            MyGuiControlParent control = new MyGuiControlParent {
                CanPlaySoundOnMouseOver = false
            };
            MyRefuseReason reason = this.CanTakeTerminal(gridInfo);
            MyRefuseReason remoteStatus = gridInfo.RemoteStatus;
            switch (reason)
            {
                case MyRefuseReason.NoStableConnection:
                    flag = false;
                    text = MySpaceTexts.BroadcastScreen_TerminalButton_NoStableConnectionToolTip;
                    break;

                case MyRefuseReason.NoProblem:
                    text = MySpaceTexts.BroadcastScreen_TerminalButton_StableConnectionToolTip;
                    break;

                case MyRefuseReason.PlayerBroadcastOff:
                    flag = false;
                    text = MySpaceTexts.BroadcastScreen_TerminalButton_PlayerBroadcastOffToolTip;
                    break;

                case MyRefuseReason.Forbidden:
                    flag = false;
                    text = MySpaceTexts.BroadcastScreen_NoOwnership;
                    break;

                default:
                    break;
            }
            Vector4? backgroundColor = null;
            MyGuiControlImage image = new MyGuiControlImage(new Vector2(-1.25f * y, 0f), new Vector2(y * 0.78f, y), backgroundColor, flag ? @"Textures\GUI\Icons\BroadcastStatus\AntennaOn.png" : @"Textures\GUI\Icons\BroadcastStatus\AntennaOff.png", null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            image.SetToolTip(text);
            control.Controls.Add(image);
            switch (remoteStatus)
            {
                case MyRefuseReason.NoRemoteControl:
                    id2 = MySpaceTexts.BroadcastScreen_TakeControlButton_NoRemoteToolTip;
                    flag3 = false;
                    break;

                case MyRefuseReason.NoMainRemoteControl:
                    id2 = MySpaceTexts.BroadcastScreen_TakeControlButton_NoMainRemoteControl;
                    flag3 = false;
                    break;

                case MyRefuseReason.NoOwner:
                case MyRefuseReason.NoProblem:
                    id2 = MySpaceTexts.BroadcastScreen_TakeControlButton_RemoteToolTip;
                    break;

                default:
                    break;
            }
            backgroundColor = null;
            MyGuiControlImage image2 = new MyGuiControlImage(new Vector2(-0.25f * y, 0f), new Vector2(y * 0.78f, y), backgroundColor, flag3 ? @"Textures\GUI\Icons\BroadcastStatus\RemoteOn.png" : @"Textures\GUI\Icons\BroadcastStatus\RemoteOff.png", null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            image2.SetToolTip(id2);
            control.Controls.Add(image2);
            if (((reason == MyRefuseReason.NoStableConnection) || (reason == MyRefuseReason.PlayerBroadcastOff)) && (remoteStatus == MyRefuseReason.NoRemoteControl))
            {
                builder.Append(MyTexts.Get(MySpaceTexts.BroadcastScreen_UnavailableControlButton));
                flag2 = false;
            }
            if (flag2 && (((reason == MyRefuseReason.NoOwner) || ((remoteStatus == MyRefuseReason.Forbidden) || (reason == MyRefuseReason.NoStableConnection))) || (reason == MyRefuseReason.PlayerBroadcastOff)))
            {
                flag2 = false;
                builder.Append(MyTexts.Get(MySpaceTexts.BroadcastScreen_NoOwnership));
            }
            if (reason == MyRefuseReason.NoOwner)
            {
                builder.AppendLine();
                builder.Append(MyTexts.Get(MySpaceTexts.DisplayName_Block_Antenna));
            }
            if (remoteStatus == MyRefuseReason.Forbidden)
            {
                builder.AppendLine();
                builder.Append(MyTexts.Get(MySpaceTexts.DisplayName_Block_RemoteControl));
            }
            if (flag2)
            {
                builder.Append(MyTexts.Get(MySpaceTexts.BroadcastScreen_Ownership));
            }
            backgroundColor = null;
            MyGuiControlImage image3 = new MyGuiControlImage(new Vector2(0.75f * y, 0f), new Vector2(y * 0.78f, y), backgroundColor, flag2 ? @"Textures\GUI\Icons\BroadcastStatus\KeyOn.png" : @"Textures\GUI\Icons\BroadcastStatus\KeyOff.png", null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            image3.SetToolTip(builder.ToString());
            control.Controls.Add(image3);
            cell.Control = control;
            this.m_shipsData.Controls.Add(control);
            return cell;
        }

        private MyGuiControlTable.Cell CreateTerminalCell(CubeGridInfo gridInfo, bool isActive)
        {
            Color? textColor = null;
            MyGuiHighlightTexture? icon = null;
            MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(null, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            Vector2 vector = new Vector2(0.1f, this.m_shipsData.RowHeight * 0.8f);
            isActive &= this.CanTakeTerminalOuter(gridInfo);
            Vector2? position = null;
            StringBuilder text = MyTexts.Get(MySpaceTexts.BroadcastScreen_TerminalButton);
            Vector4? colorMask = null;
            int? buttonIndex = null;
            cell.Control = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(vector), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnButtonClicked_OpenTerminal), GuiSounds.MouseClick, 1f, buttonIndex, false);
            cell.Control.ShowTooltipWhenDisabled = true;
            cell.Control.Enabled = isActive;
            if (cell.Control.Enabled)
            {
                cell.Control.SetToolTip(MySpaceTexts.BroadcastScreen_TerminalButton_ToolTip);
            }
            else
            {
                cell.Control.SetToolTip(MySpaceTexts.BroadcastScreen_TerminalButtonDisabled_ToolTip);
            }
            this.m_shipsData.Controls.Add(cell.Control);
            return cell;
        }

        private void FindRemoteControlAndTakeControl(long gridEntityId, long remoteEntityId)
        {
            MyRemoteControl control;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRemoteControl>(remoteEntityId, out control, false);
            if (control == null)
            {
                if (!Sync.IsServer)
                {
                    MyGuiScreenTerminal.RequestReplicable(gridEntityId, remoteEntityId, x => this.FindRemoteControlAndTakeControl(gridEntityId, x));
                }
            }
            else
            {
                this.m_tmpAntennas.Clear();
                MyAntennaSystem.Static.GetEntityReceivers(control, ref this.m_tmpAntennas, MySession.Static.LocalPlayerId);
                using (HashSet<MyDataReceiver>.Enumerator enumerator = this.m_tmpAntennas.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateBroadcastersInRange();
                    }
                }
                control.RequestControl();
            }
        }

        private HashSet<CubeGridInfo> GetAllCubeGridsInfo()
        {
            HashSet<CubeGridInfo> set = new HashSet<CubeGridInfo>();
            this.m_tmpGridInfoOutput.Clear();
            this.m_tmpBroadcasters.Clear();
            if (MySession.Static.LocalCharacter != null)
            {
                foreach (MyDataBroadcaster broadcaster in MyAntennaSystem.Static.GetAllRelayedBroadcasters(this.m_interactedEntityRepresentative, MySession.Static.LocalPlayerId, false, this.m_tmpBroadcasters))
                {
                    if (ReferenceEquals(broadcaster, MySession.Static.LocalCharacter.RadioBroadcaster))
                    {
                        continue;
                    }
                    if (broadcaster.ShowInTerminal)
                    {
                        CubeGridInfo info;
                        double playerBroadcasterDistance = this.GetPlayerBroadcasterDistance(broadcaster);
                        MyCubeGridConnectionStatus broadcasterStatus = this.GetBroadcasterStatus(broadcaster);
                        if (!this.m_tmpGridInfoOutput.TryGetValue(broadcaster.Info.EntityId, out info))
                        {
                            CubeGridInfo info1 = new CubeGridInfo();
                            info1.EntityId = broadcaster.Info.EntityId;
                            info1.Distance = playerBroadcasterDistance;
                            info1.AppendedDistance = new StringBuilder().AppendDecimal(playerBroadcasterDistance, 0).Append(" m");
                            info1.Name = broadcaster.Info.Name;
                            info1.Status = broadcasterStatus;
                            info1.Owned = broadcaster.CanBeUsedByPlayer(MySession.Static.LocalPlayerId);
                            info1.RemoteStatus = this.GetRemoteStatus(broadcaster);
                            info1.RemoteId = broadcaster.MainRemoteControlId;
                            this.m_tmpGridInfoOutput.Add(broadcaster.Info.EntityId, info1);
                            continue;
                        }
                        if (info.Status > broadcasterStatus)
                        {
                            info.Status = broadcasterStatus;
                        }
                        if (info.Distance > playerBroadcasterDistance)
                        {
                            info.Distance = playerBroadcasterDistance;
                            info.AppendedDistance = new StringBuilder().AppendDecimal(playerBroadcasterDistance, 0).Append(" m");
                        }
                        if (!info.Owned && broadcaster.CanBeUsedByPlayer(MySession.Static.LocalPlayerId))
                        {
                            info.Owned = true;
                        }
                    }
                }
                foreach (CubeGridInfo info2 in this.m_tmpGridInfoOutput.Values)
                {
                    set.Add(info2);
                }
            }
            return set;
        }

        private MyCubeGridConnectionStatus GetBroadcasterStatus(MyDataBroadcaster broadcaster) => 
            (MyAntennaSystem.Static.CheckConnection(broadcaster.Receiver, this.m_openInventoryInteractedEntityRepresentative, MySession.Static.LocalPlayerId, false) ? (MyAntennaSystem.Static.CheckConnection(this.m_openInventoryInteractedEntityRepresentative, broadcaster, MySession.Static.LocalPlayerId, false) ? MyCubeGridConnectionStatus.Connected : MyCubeGridConnectionStatus.OutOfReceivingRange) : MyCubeGridConnectionStatus.OutOfBroadcastingRange);

        private VRage.Game.Entity.MyEntity GetInteractedEntityRepresentative(VRage.Game.Entity.MyEntity controlledEntity) => 
            (!(controlledEntity is MyCubeBlock) ? ((VRage.Game.Entity.MyEntity) MySession.Static.LocalCharacter) : ((VRage.Game.Entity.MyEntity) MyAntennaSystem.Static.GetLogicalGroupRepresentative((controlledEntity as MyCubeBlock).CubeGrid)));

        private double GetPlayerBroadcasterDistance(MyDataBroadcaster broadcaster)
        {
            if ((MySession.Static.ControlledEntity == null) || (MySession.Static.ControlledEntity.Entity == null))
            {
                return double.MaxValue;
            }
            return Vector3D.Distance(MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition(), broadcaster.BroadcastPosition);
        }

        private MyRefuseReason GetRemoteStatus(MyDataBroadcaster broadcaster)
        {
            if (!broadcaster.HasRemoteControl)
            {
                return MyRefuseReason.NoRemoteControl;
            }
            long? mainRemoteControlOwner = broadcaster.MainRemoteControlOwner;
            if (mainRemoteControlOwner == null)
            {
                return MyRefuseReason.NoMainRemoteControl;
            }
            MyRelationsBetweenPlayers relationsBetweenPlayers = MyPlayer.GetRelationsBetweenPlayers(mainRemoteControlOwner.Value, MySession.Static.LocalHumanPlayer.Identity.IdentityId);
            if (relationsBetweenPlayers == MyRelationsBetweenPlayers.Self)
            {
                return MyRefuseReason.NoProblem;
            }
            MyOwnershipShareModeEnum mainRemoteControlSharing = broadcaster.MainRemoteControlSharing;
            if ((mainRemoteControlSharing == MyOwnershipShareModeEnum.All) || ((mainRemoteControlSharing == MyOwnershipShareModeEnum.Faction) && (relationsBetweenPlayers == MyRelationsBetweenPlayers.Allies)))
            {
                return MyRefuseReason.NoProblem;
            }
            return ((mainRemoteControlOwner.Value != 0) ? MyRefuseReason.Forbidden : MyRefuseReason.NoOwner);
        }

        private MyCubeGridConnectionStatus GetShipStatus(MyCubeGrid grid)
        {
            HashSet<MyDataBroadcaster> output = new HashSet<MyDataBroadcaster>();
            MyAntennaSystem.Static.GetEntityBroadcasters(grid, ref output, MySession.Static.LocalPlayerId);
            MyCubeGridConnectionStatus outOfReceivingRange = MyCubeGridConnectionStatus.OutOfReceivingRange;
            using (HashSet<MyDataBroadcaster>.Enumerator enumerator = output.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyDataBroadcaster current = enumerator.Current;
                    MyCubeGridConnectionStatus broadcasterStatus = this.GetBroadcasterStatus(current);
                    if (broadcasterStatus != MyCubeGridConnectionStatus.Connected)
                    {
                        if (broadcasterStatus != MyCubeGridConnectionStatus.OutOfBroadcastingRange)
                        {
                            continue;
                        }
                        outOfReceivingRange = broadcasterStatus;
                        continue;
                    }
                    return broadcasterStatus;
                }
            }
            return outOfReceivingRange;
        }

        public void Init(MyGuiControlParent menuParent, MyGuiControlParent panelParent, VRage.Game.Entity.MyEntity interactedEntity, VRage.Game.Entity.MyEntity openInventoryInteractedEntity, bool isRemote)
        {
            this.m_interactedEntityRepresentative = this.GetInteractedEntityRepresentative(interactedEntity);
            this.m_openInventoryInteractedEntityRepresentative = this.GetInteractedEntityRepresentative(openInventoryInteractedEntity);
            this.m_interactedEntity = interactedEntity ?? MySession.Static.LocalCharacter;
            this.m_isRemote = isRemote;
            if (menuParent == null)
            {
                MySandboxGame.Log.WriteLine("menuParent is null");
            }
            if (panelParent == null)
            {
                MySandboxGame.Log.WriteLine("panelParent is null");
            }
            if ((menuParent != null) && (panelParent != null))
            {
                this.m_shipsInRange = (MyGuiControlCombobox) menuParent.Controls.GetControlByName("ShipsInRange");
                this.m_button = (MyGuiControlButton) menuParent.Controls.GetControlByName("SelectShip");
                this.m_shipsData = (MyGuiControlTable) panelParent.Controls.GetControlByName("ShipsData");
                this.m_columnToSort = 1;
                this.m_button.ButtonClicked += new Action<MyGuiControlButton>(this.Menu_ButtonClicked);
                this.m_shipsData.ColumnClicked += new Action<MyGuiControlTable, int>(this.shipsData_ColumnClicked);
                this.m_shipsInRange.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.shipsInRange_ItemSelected);
                this.Refresh();
            }
        }

        private void Menu_ButtonClicked(MyGuiControlButton button)
        {
            if (this.ButtonClicked != null)
            {
                this.ButtonClicked();
            }
        }

        private void OnButtonClicked_OpenTerminal(MyGuiControlButton obj)
        {
            MyGuiControlTable.EventArgs args;
            args.MouseButton = MyMouseButtonsEnum.None;
            args.RowIndex = -1;
            this.shipsData_ItemDoubleClicked(null, args);
        }

        private void OnButtonClicked_TakeControl(MyGuiControlButton obj)
        {
            if (this.m_shipsData.SelectedRow != null)
            {
                UserData userData = (UserData) this.m_shipsData.SelectedRow.UserData;
                if (userData.IsSelectable && (userData.RemoteEntityId != null))
                {
                    this.FindRemoteControlAndTakeControl(userData.GridEntityId, userData.RemoteEntityId.Value);
                }
            }
        }

        private void OpenPropertiesByEntityId(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false);
            if ((entity == null) && !Sync.IsServer)
            {
                MyGuiScreenTerminal.RequestReplicable(entityId, entityId, new Action<long>(this.OpenPropertiesByEntityId));
            }
            else if (entity is MyCharacter)
            {
                MyGuiScreenTerminal.ChangeInteractedEntity(null, false);
            }
            else if ((entity != null) && (entity is MyCubeGrid))
            {
                MyCubeGrid broadcastingEntity = entity as MyCubeGrid;
                if (MyAntennaSystem.Static.CheckConnection(broadcastingEntity, this.m_openInventoryInteractedEntityRepresentative, MySession.Static.LocalHumanPlayer, true) || ReferenceEquals(this.m_openInventoryInteractedEntityRepresentative, broadcastingEntity))
                {
                    this.m_tmpAntennas.Clear();
                    MyAntennaSystem.Static.GetEntityReceivers(broadcastingEntity, ref this.m_tmpAntennas, MySession.Static.LocalPlayerId);
                    using (HashSet<MyDataReceiver>.Enumerator enumerator = this.m_tmpAntennas.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.UpdateBroadcastersInRange();
                        }
                    }
                    if (this.m_tmpAntennas.Count <= 0)
                    {
                        MyGuiScreenTerminal.ChangeInteractedEntity(MyGuiScreenTerminal.InteractedEntity, false);
                    }
                    else
                    {
                        MyGuiScreenTerminal.ChangeInteractedEntity(this.m_tmpAntennas.ElementAt<MyDataReceiver>(0).Entity as MyTerminalBlock, true);
                    }
                }
            }
        }

        private void PopulateMutuallyConnectedCubeGrids(HashSet<MyAntennaSystem.BroadcasterInfo> playerMutualConnection)
        {
            this.m_shipsInRange.ClearItems();
            VRage.Game.Entity.MyEntity openInventoryInteractedEntityRepresentative = this.m_openInventoryInteractedEntityRepresentative;
            int? sortOrder = null;
            this.m_shipsInRange.AddItem(this.m_openInventoryInteractedEntityRepresentative.EntityId, new StringBuilder(this.m_openInventoryInteractedEntityRepresentative.DisplayName), sortOrder, null);
            foreach (MyAntennaSystem.BroadcasterInfo info in playerMutualConnection)
            {
                if (this.m_shipsInRange.TryGetItemByKey(info.EntityId) == null)
                {
                    sortOrder = null;
                    this.m_shipsInRange.AddItem(info.EntityId, new StringBuilder(info.Name), sortOrder, null);
                }
            }
            this.m_shipsInRange.Visible = true;
            this.m_button.Visible = true;
            this.m_shipsInRange.SortItemsByValueText();
            if ((this.m_shipsInRange.TryGetItemByKey(this.m_interactedEntityRepresentative.EntityId) == null) && (this.m_interactedEntityRepresentative is MyCubeGrid))
            {
                sortOrder = null;
                this.m_shipsInRange.AddItem(this.m_interactedEntityRepresentative.EntityId, new StringBuilder((this.m_interactedEntityRepresentative as MyCubeGrid).DisplayName), sortOrder, null);
            }
            this.m_shipsInRange.SelectItemByKey(this.m_interactedEntityRepresentative.EntityId, true);
        }

        private void PopulateOwnedCubeGrids(HashSet<CubeGridInfo> gridInfoList)
        {
            float amount = this.m_shipsData.ScrollBar.Value;
            this.m_shipsData.Clear();
            this.m_shipsData.Controls.Clear();
            foreach (CubeGridInfo info in gridInfoList)
            {
                UserData data;
                MyGuiControlTable.Cell cell;
                MyGuiControlTable.Cell cell2;
                MyGuiControlTable.Cell cell3;
                MyGuiControlTable.Cell cell4;
                MyGuiControlTable.Cell cell5;
                Color? nullable;
                MyGuiHighlightTexture? nullable2;
                data.GridEntityId = info.EntityId;
                data.RemoteEntityId = info.RemoteId;
                if (((info.Status != MyCubeGridConnectionStatus.Connected) && (info.Status != MyCubeGridConnectionStatus.PhysicallyConnected)) && (info.Status != MyCubeGridConnectionStatus.Me))
                {
                    data.IsSelectable = false;
                    nullable = new Color?(Color.Gray);
                    nullable2 = null;
                    cell = new MyGuiControlTable.Cell(new StringBuilder(info.Name), null, info.Name, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    cell2 = this.CreateControlCell(info, false);
                    nullable2 = null;
                    cell3 = new MyGuiControlTable.Cell(MyTexts.Get(MySpaceTexts.NotAvailable), 1.7976931348623157E+308, MyTexts.GetString(MySpaceTexts.NotAvailable), new Color?(Color.Gray), nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    cell4 = this.CreateStatusIcons(info, true);
                    cell5 = this.CreateTerminalCell(info, false);
                }
                else
                {
                    StringBuilder text = new StringBuilder();
                    if (info.Status == MyCubeGridConnectionStatus.Connected)
                    {
                        text = info.AppendedDistance;
                    }
                    data.IsSelectable = true;
                    nullable = new Color?(Color.White);
                    nullable2 = null;
                    cell = new MyGuiControlTable.Cell(new StringBuilder(info.Name), null, info.Name, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    cell2 = this.CreateControlCell(info, true);
                    string toolTip = text.ToString();
                    nullable2 = null;
                    cell3 = new MyGuiControlTable.Cell(text, info.Distance, toolTip, new Color?(Color.White), nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    cell4 = this.CreateStatusIcons(info, true);
                    cell5 = this.CreateTerminalCell(info, true);
                }
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(data);
                row.AddCell(cell);
                row.AddCell(cell3);
                row.AddCell(cell4);
                row.AddCell(cell2);
                row.AddCell(cell5);
                this.m_shipsData.Add(row);
                this.m_shipsData.SortByColumn(this.m_columnToSort, 1, false);
            }
            this.m_shipsData.ScrollBar.ChangeValue(amount);
        }

        public void Refresh()
        {
            this.PopulateMutuallyConnectedCubeGrids(MyAntennaSystem.Static.GetConnectedGridsInfo(this.m_openInventoryInteractedEntityRepresentative, null, true));
            this.PopulateOwnedCubeGrids(this.GetAllCubeGridsInfo());
        }

        private void shipsData_ColumnClicked(MyGuiControlTable sender, int column)
        {
            this.m_columnToSort = column;
        }

        private void shipsData_ItemDoubleClicked(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            if (this.m_shipsData.SelectedRow != null)
            {
                UserData userData = (UserData) this.m_shipsData.SelectedRow.UserData;
                if (userData.IsSelectable)
                {
                    this.OpenPropertiesByEntityId(userData.GridEntityId);
                }
            }
        }

        private void shipsInRange_ItemSelected()
        {
            if ((this.m_shipsInRange.IsMouseOver || this.m_shipsInRange.HasFocus) && (this.m_shipsInRange.GetSelectedKey() != this.m_interactedEntityRepresentative.EntityId))
            {
                this.OpenPropertiesByEntityId(this.m_shipsInRange.GetSelectedKey());
            }
        }

        public bool TestConnection()
        {
            if ((this.m_openInventoryInteractedEntityRepresentative.EntityId == this.m_interactedEntityRepresentative.EntityId) && !this.m_isRemote)
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if ((this.m_interactedEntity != null) && (localCharacter != null))
                {
                    return (this.m_interactedEntity.PositionComp.WorldAABB.DistanceSquared(localCharacter.PositionComp.GetPosition()) < (MyConstants.DEFAULT_INTERACTIVE_DISTANCE * MyConstants.DEFAULT_INTERACTIVE_DISTANCE));
                }
            }
            return (!(this.m_interactedEntityRepresentative is MyCubeGrid) || (this.GetShipStatus(this.m_interactedEntityRepresentative as MyCubeGrid) == MyCubeGridConnectionStatus.Connected));
        }

        public void Update()
        {
            int num = this.cnt + 1;
            this.cnt = num;
            this.cnt = num % 30;
            if (this.cnt == 0)
            {
                if (this.previousMutualConnectionGrids == null)
                {
                    this.previousMutualConnectionGrids = MyAntennaSystem.Static.GetConnectedGridsInfo(this.m_openInventoryInteractedEntityRepresentative, null, true);
                }
                if (this.previousShipInfo == null)
                {
                    this.previousShipInfo = this.GetAllCubeGridsInfo();
                }
                HashSet<MyAntennaSystem.BroadcasterInfo> other = MyAntennaSystem.Static.GetConnectedGridsInfo(this.m_openInventoryInteractedEntityRepresentative, null, true);
                HashSet<CubeGridInfo> allCubeGridsInfo = this.GetAllCubeGridsInfo();
                if (!this.previousMutualConnectionGrids.SetEquals(other))
                {
                    this.PopulateMutuallyConnectedCubeGrids(other);
                }
                if (!this.previousShipInfo.SequenceEqual<CubeGridInfo>(allCubeGridsInfo))
                {
                    this.PopulateOwnedCubeGrids(allCubeGridsInfo);
                }
                this.previousMutualConnectionGrids = other;
                this.previousShipInfo = allCubeGridsInfo;
            }
        }

        private class CubeGridInfo
        {
            public long EntityId;
            public double Distance;
            public string Name;
            public StringBuilder AppendedDistance;
            public MyTerminalPropertiesController.MyCubeGridConnectionStatus Status;
            public bool Owned;
            public MyTerminalPropertiesController.MyRefuseReason RemoteStatus;
            public long? RemoteId;

            public override bool Equals(object obj)
            {
                if (!(obj is MyTerminalPropertiesController.CubeGridInfo))
                {
                    return false;
                }
                MyTerminalPropertiesController.CubeGridInfo info = obj as MyTerminalPropertiesController.CubeGridInfo;
                string str = (this.Name == null) ? "" : this.Name;
                string str2 = (info.Name == null) ? "" : info.Name;
                return (this.EntityId.Equals(info.EntityId) && (str.Equals(str2) && (this.AppendedDistance.Equals(info.AppendedDistance) && (this.Status == info.Status))));
            }

            public override int GetHashCode()
            {
                string str = (this.Name == null) ? "" : this.Name;
                return ((((((this.EntityId.GetHashCode() * 0x18d) ^ str.GetHashCode()) * 0x18d) ^ this.AppendedDistance.GetHashCode()) * 0x18d) ^ ((int) this.Status));
            }
        }

        private enum MyCubeGridConnectionStatus
        {
            PhysicallyConnected,
            Connected,
            OutOfBroadcastingRange,
            OutOfReceivingRange,
            Me,
            IsPreviewGrid
        }

        private enum MyRefuseReason
        {
            NoRemoteControl,
            NoMainRemoteControl,
            NoStableConnection,
            NoOwner,
            NoProblem,
            PlayerBroadcastOff,
            Forbidden
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UserData
        {
            public long GridEntityId;
            public long? RemoteEntityId;
            public bool IsSelectable;
        }
    }
}


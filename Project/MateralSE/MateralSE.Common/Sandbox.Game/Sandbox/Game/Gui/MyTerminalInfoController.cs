namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.GUI.HudViewers;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    internal class MyTerminalInfoController
    {
        private static MyGuiControlTabPage m_infoPage;
        private static MyCubeGrid m_grid;
        private static List<MyBlockLimits.MyGridLimitData> m_infoGrids = new List<MyBlockLimits.MyGridLimitData>();
        private static List<MyPlayer.PlayerId> m_playerIds = new List<MyPlayer.PlayerId>();
        private static bool m_controlsDirty;

        private static void assignCombobox_ItemSelected(MyIdentity locallIdentity, long entityId, MyPlayer.PlayerId playerId)
        {
            MyBlockLimits.MyGridLimitData gridLimitData;
            if (locallIdentity.BlockLimits.BlocksBuiltByGrid.TryGetValue(entityId, out gridLimitData))
            {
                ulong steamId = playerId.SteamId;
                MyIdentity identity = MySession.Static.Players.TryGetPlayerIdentity(playerId);
                if (identity != null)
                {
                    object[] args = new object[] { gridLimitData.GridName, identity.DisplayName };
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextConfirmTransferGrid), args), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            if (MySession.Static.Players.GetOnlinePlayers().Contains(MySession.Static.Players.GetPlayerById(playerId)))
                            {
                                EndpointId targetEndpoint = new EndpointId();
                                Vector3D? position = null;
                                MyMultiplayer.RaiseStaticEvent<MyBlockLimits.MyGridLimitData, long, long, ulong>(x => new Action<MyBlockLimits.MyGridLimitData, long, long, ulong>(MyBlockLimits.SendTransferRequestMessage), gridLimitData, MySession.Static.LocalPlayerId, identity.IdentityId, steamId, targetEndpoint, position);
                            }
                            else
                            {
                                ShowPlayerNotOnlineMessage(identity);
                            }
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
                }
            }
        }

        private void centerBtn_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            MyCubeGrid.ShowCenterOfMass = obj.IsChecked;
            (from x in Sandbox.Game.Entities.MyEntities.GetEntities()
                where x is MyCubeGrid
                select x).Cast<MyCubeGrid>().ForEach<MyCubeGrid>(x => x.MarkForDraw());
        }

        internal void Close()
        {
            MySession.Static.Players.TryGetIdentity(MySession.Static.LocalPlayerId).BlockLimits.BlockLimitsChanged -= new Action(this.grid_OnAuthorshipChanged);
            Sandbox.Game.Entities.MyEntities.OnEntityDelete -= new Action<VRage.Game.Entity.MyEntity>(this.grid_OnClose);
            if ((m_grid != null) && (m_infoPage != null))
            {
                MyGuiControlButton controlByName = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertBtn");
                if (controlByName != null)
                {
                    controlByName.ButtonClicked -= new Action<MyGuiControlButton>(this.convertBtn_ButtonClicked);
                }
                MyGuiControlButton button2 = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertToStationBtn");
                if (button2 != null)
                {
                    button2.ButtonClicked -= new Action<MyGuiControlButton>(this.convertToStationBtn_ButtonClicked);
                }
                m_grid.OnBlockAdded -= new Action<MySlimBlock>(this.grid_OnBlockAdded);
                m_grid.OnBlockRemoved -= new Action<MySlimBlock>(this.grid_OnBlockRemoved);
                m_grid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.grid_OnPhysicsChanged);
                m_grid.OnBlockOwnershipChanged -= new Action<MyCubeGrid>(this.grid_OnBlockOwnershipChanged);
                m_grid = null;
                m_infoPage = null;
            }
        }

        private void convertBtn_ButtonClicked(MyGuiControlButton obj)
        {
            m_grid.RequestConversionToShip(new Action(this.convertBtn_Fail));
        }

        private void convertBtn_Fail()
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextConvertToShipFail), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
        }

        private void convertToStationBtn_ButtonClicked(MyGuiControlButton obj)
        {
            m_grid.RequestConversionToStation();
        }

        private static void deleteBuiltByIdBlocksButton_ButtonClicked(MyGuiControlButton obj)
        {
            if (obj.Index < m_infoGrids.Count)
            {
                MyBlockLimits.MyGridLimitData gridInfo = m_infoGrids[obj.Index];
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextConfirmDeleteGrid, gridInfo.GridName), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long, long>(x => new Action<long, long>(MyBlockLimits.RemoveBlocksBuiltByID), gridInfo.EntityId, MySession.Static.LocalPlayerId, targetEndpoint, position);
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
            }
        }

        private void grid_OnAuthorshipChanged()
        {
            RecreateControls();
        }

        private void grid_OnBlockAdded(MySlimBlock obj)
        {
            RecreateControls();
        }

        private void grid_OnBlockOwnershipChanged(VRage.Game.Entity.MyEntity obj)
        {
            RecreateControls();
        }

        private void grid_OnBlockRemoved(MySlimBlock obj)
        {
            RecreateControls();
        }

        private void grid_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            if (obj is MyCubeGrid)
            {
                using (List<MyBlockLimits.MyGridLimitData>.Enumerator enumerator = m_infoGrids.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.EntityId == obj.EntityId)
                        {
                            RecreateControls();
                            break;
                        }
                    }
                }
            }
        }

        private void grid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            RecreateControls();
        }

        internal void Init(MyGuiControlTabPage infoPage, MyCubeGrid grid)
        {
            m_grid = grid;
            m_infoPage = infoPage;
            m_playerIds.Clear();
            m_controlsDirty = false;
            MySession.Static.Players.TryGetIdentity(MySession.Static.LocalPlayerId).BlockLimits.BlockLimitsChanged += new Action(this.grid_OnAuthorshipChanged);
            RecreateControls();
            Sandbox.Game.Entities.MyEntities.OnEntityDelete += new Action<VRage.Game.Entity.MyEntity>(this.grid_OnClose);
            if (grid != null)
            {
                grid.OnBlockAdded += new Action<MySlimBlock>(this.grid_OnBlockAdded);
                grid.OnBlockRemoved += new Action<MySlimBlock>(this.grid_OnBlockRemoved);
                grid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.grid_OnPhysicsChanged);
                grid.OnBlockOwnershipChanged += new Action<MyCubeGrid>(this.grid_OnBlockOwnershipChanged);
                if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                {
                    MyGuiControlButton button3 = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("RenameShipButton");
                    if (button3 != null)
                    {
                        button3.ButtonClicked += new Action<MyGuiControlButton>(this.renameBtn_ButtonClicked);
                    }
                }
                MyGuiControlButton controlByName = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertBtn");
                if (controlByName != null)
                {
                    controlByName.ButtonClicked += new Action<MyGuiControlButton>(this.convertBtn_ButtonClicked);
                }
                MyGuiControlButton button2 = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertToStationBtn");
                if (button2 != null)
                {
                    button2.ButtonClicked += new Action<MyGuiControlButton>(this.convertToStationBtn_ButtonClicked);
                }
            }
        }

        private bool IsPlayerOwner(MyCubeGrid grid) => 
            ((grid != null) && grid.BigOwners.Contains(MySession.Static.LocalPlayerId));

        public void MarkControlsDirty()
        {
            m_controlsDirty = true;
        }

        private void pivotBtn_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            MyCubeGrid.ShowGridPivot = obj.IsChecked;
            (from x in Sandbox.Game.Entities.MyEntities.GetEntities()
                where x is MyCubeGrid
                select x).Cast<MyCubeGrid>().ForEach<MyCubeGrid>(x => x.MarkForDraw());
        }

        private static void RecreateControls()
        {
            if (m_infoPage != null)
            {
                m_controlsDirty = true;
            }
        }

        private void renameBtn_ButtonClicked(MyGuiControlButton obj)
        {
            MyGuiControlTextbox controlByName = (MyGuiControlTextbox) m_infoPage.Controls.GetControlByName("RenameShipText");
            m_grid.ChangeDisplayNameRequest(controlByName.Text);
        }

        private void renameBtn_Update(MyGuiControlTextbox obj)
        {
            if (obj.Enabled)
            {
                MyGuiControlTextbox controlByName = (MyGuiControlTextbox) m_infoPage.Controls.GetControlByName("RenameShipText");
                m_grid.ChangeDisplayNameRequest(controlByName.Text);
            }
        }

        public static void RequestServerLimitInfo(long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, ulong>(x => new Action<long, ulong>(MyTerminalInfoController.ServerLimitInfo_Implementation), identityId, MySession.Static.LocalHumanPlayer.Id.SteamId, targetEndpoint, position);
        }

        [Event(null, 0x13b), Reliable, Server]
        public static unsafe void ServerLimitInfo_Implementation(long identityId, ulong clientId)
        {
            if (MySession.Static != null)
            {
                List<GridBuiltByIdInfo> list = new List<GridBuiltByIdInfo>();
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
                if (identity != null)
                {
                    foreach (KeyValuePair<long, MyBlockLimits.MyGridLimitData> pair in identity.BlockLimits.BlocksBuiltByGrid)
                    {
                        MyCubeGrid grid;
                        GridBuiltByIdInfo* infoPtr1;
                        HashSet<MySlimBlock> source = null;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(pair.Key, out grid, false))
                        {
                            source = grid.FindBlocksBuiltByID(identity.IdentityId);
                            if (!source.Any<MySlimBlock>())
                            {
                                continue;
                            }
                        }
                        GridBuiltByIdInfo item = new GridBuiltByIdInfo {
                            GridName = pair.Value.GridName,
                            EntityId = pair.Key,
                            UnsafeBlocks = new List<string>()
                        };
                        if (source != null)
                        {
                            item.BlockCount = source.Count;
                            infoPtr1->PCUBuilt = source.Sum<MySlimBlock>(x => x.BlockDefinition.PCU);
                        }
                        if (MyUnsafeGridsSessionComponent.UnsafeGrids.TryGetValue(pair.Key, out grid))
                        {
                            foreach (MyCubeBlock block in grid.UnsafeBlocks)
                            {
                                infoPtr1 = (GridBuiltByIdInfo*) ref item;
                                item.UnsafeBlocks.Add(block.DisplayNameText);
                            }
                        }
                        list.Add(item);
                    }
                }
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<List<GridBuiltByIdInfo>>(x => new Action<List<GridBuiltByIdInfo>>(MyTerminalInfoController.ServerLimitInfo_Received), list, new EndpointId(clientId), position);
            }
        }

        [Event(null, 0x16e), Reliable, Client]
        private static void ServerLimitInfo_Received(List<GridBuiltByIdInfo> gridsWithBuiltById)
        {
            if (m_infoPage != null)
            {
                MyGuiControlList controlByName = (MyGuiControlList) m_infoPage.Controls.GetControlByName("InfoList");
                if (controlByName != null)
                {
                    controlByName.Controls.Clear();
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(MySession.Static.LocalPlayerId);
                    if (identity != null)
                    {
                        Vector2? nullable;
                        VRageMath.Vector4? nullable2;
                        if (MySession.Static.MaxBlocksPerPlayer > 0)
                        {
                            nullable = null;
                            nullable = null;
                            object[] args = new object[] { MyTexts.Get(MySpaceTexts.TerminalTab_Info_YouBuilt), identity.BlockLimits.BlocksBuilt, identity.BlockLimits.MaxBlocks, MyTexts.Get(MySpaceTexts.TerminalTab_Info_BlocksLower) };
                            nullable2 = null;
                            MyGuiControlLabel control = new MyGuiControlLabel(nullable, nullable, string.Format("{0} {1}/{2} {3}", args), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                            controlByName.Controls.Add(control);
                        }
                        foreach (KeyValuePair<string, short> pair in MySession.Static.BlockTypeLimits)
                        {
                            MyBlockLimits.MyTypeLimitData data;
                            identity.BlockLimits.BlockTypeBuilt.TryGetValue(pair.Key, out data);
                            MyCubeBlockDefinitionGroup group = MyDefinitionManager.Static.TryGetDefinitionGroup(pair.Key);
                            if ((group != null) && (data != null))
                            {
                                nullable = null;
                                nullable = null;
                                object[] args = new object[] { MyTexts.Get(MySpaceTexts.TerminalTab_Info_YouBuilt), data.BlocksBuilt, MySession.Static.GetBlockTypeLimit(pair.Key), group.Any.DisplayNameText };
                                nullable2 = null;
                                MyGuiControlLabel control = new MyGuiControlLabel(nullable, nullable, string.Format("{0} {1}/{2} {3}", args), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                                controlByName.Controls.Add(control);
                            }
                        }
                        m_infoGrids.Clear();
                        if (gridsWithBuiltById != null)
                        {
                            foreach (GridBuiltByIdInfo info in gridsWithBuiltById)
                            {
                                MyGuiControlParent control = new MyGuiControlParent();
                                bool flag = info.UnsafeBlocks.Count > 0;
                                control.Size = new Vector2(control.Size.X, 0.1f);
                                if (m_infoGrids.Count == 0)
                                {
                                    MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
                                    nullable2 = null;
                                    list3.AddHorizontal(new Vector2(-0.15f, -0.052f), 0.3f, 0.002f, nullable2);
                                    control.Controls.Add(list3);
                                }
                                string gridName = info.GridName;
                                if ((gridName != null) && (gridName.Length >= 0x10))
                                {
                                    gridName = gridName.Substring(0, 15);
                                    gridName = gridName.Insert(gridName.Length, "...");
                                }
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiControlLabel label3 = new MyGuiControlLabel(nullable, nullable, gridName, nullable2, 0.7005405f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiControlLabel label4 = new MyGuiControlLabel(nullable, nullable, $"{info.BlockCount} {MyTexts.Get(MySpaceTexts.TerminalTab_Info_BlocksLower)} ({info.PCUBuilt} PCU)", nullable2, 0.7005405f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiControlLabel label5 = new MyGuiControlLabel(nullable, nullable, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_Assign), nullable2, 0.7005405f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                                nullable = null;
                                nullable2 = null;
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyGuiControlCombobox assignCombobox = new MyGuiControlCombobox(nullable, new Vector2(0.11f, 0.008f), nullable2, nullable, 10, nullable, false, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, nullable2);
                                MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
                                label3.Position = new Vector2(-0.12f, -0.025f);
                                label4.Position = new Vector2(-0.12f, 0f);
                                label5.Position = new Vector2(0f, 0.035f);
                                assignCombobox.Position = new Vector2(0.121f, 0.055f);
                                GridBuiltByIdInfo gridSelected = info;
                                assignCombobox.ItemSelected += () => assignCombobox_ItemSelected(identity, gridSelected.EntityId, m_playerIds[(int) assignCombobox.GetSelectedKey()]);
                                m_playerIds.Clear();
                                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                                {
                                    if (!ReferenceEquals(MySession.Static.LocalHumanPlayer, player))
                                    {
                                        int? sortOrder = null;
                                        assignCombobox.AddItem((long) m_playerIds.Count, player.DisplayName, sortOrder, null);
                                        m_playerIds.Add(player.Id);
                                    }
                                }
                                if (MySession.Static.Settings.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
                                {
                                    assignCombobox.Enabled = false;
                                    assignCombobox.SetTooltip(MyTexts.GetString(MySpaceTexts.Terminal_AuthorshipNotAvailable));
                                    assignCombobox.ShowTooltipWhenDisabled = true;
                                }
                                else if (assignCombobox.GetItemsCount() == 0)
                                {
                                    assignCombobox.Enabled = false;
                                }
                                nullable2 = null;
                                list2.AddHorizontal(new Vector2(-0.15f, 0.063f), 0.3f, flag ? 0.002f : 0.003f, nullable2);
                                control.Controls.Add(label3);
                                control.Controls.Add(label4);
                                control.Controls.Add(label5);
                                control.Controls.Add(assignCombobox);
                                control.Controls.Add(list2);
                                if (MySession.Static.EnableRemoteBlockRemoval)
                                {
                                    nullable = null;
                                    nullable = null;
                                    nullable2 = null;
                                    MyGuiControlLabel label6 = new MyGuiControlLabel(nullable, nullable, MyTexts.GetString(MySpaceTexts.buttonRemove), nullable2, 0.7005405f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                                    nullable = null;
                                    nullable = null;
                                    nullable2 = null;
                                    StringBuilder text = new StringBuilder("X");
                                    MyGuiControlButton button = new MyGuiControlButton(nullable, MyGuiControlButtonStyleEnum.SquareSmall, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_RemoveGrid), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(MyTerminalInfoController.deleteBuiltByIdBlocksButton_ButtonClicked), GuiSounds.MouseClick, 1f, new int?(m_infoGrids.Count), false);
                                    label6.Position = new Vector2(0.082f, -0.02f);
                                    button.Position = new Vector2(0.1215f, -0.02f);
                                    control.Controls.Add(label6);
                                    control.Controls.Add(button);
                                }
                                if (identity.BlockLimits.BlocksBuiltByGrid.ContainsKey(info.EntityId))
                                {
                                    m_infoGrids.Add(identity.BlockLimits.BlocksBuiltByGrid[info.EntityId]);
                                }
                                else if (MySession.Static.Settings.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
                                {
                                    MyBlockLimits.MyGridLimitData item = new MyBlockLimits.MyGridLimitData();
                                    item.EntityId = info.EntityId;
                                    item.GridName = info.GridName;
                                    m_infoGrids.Add(item);
                                }
                                controlByName.Controls.Add(control);
                                if (flag)
                                {
                                    MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
                                    text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                                    text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                                    text1.TextScale = 0.7f;
                                    text1.TextColor = Color.Red;
                                    MyGuiControlMultilineText text = text1;
                                    label3.ColorMask = (VRageMath.Vector4) Color.Red;
                                    label4.ColorMask = (VRageMath.Vector4) Color.Red;
                                    StringBuilder builder2 = text.Text;
                                    builder2.AppendLine(MyTexts.GetString(MyCommonTexts.ScreenTerminalInfo_UnsafeBlocks));
                                    foreach (string str2 in info.UnsafeBlocks)
                                    {
                                        builder2.AppendLine(str2);
                                    }
                                    text.RefreshText(false);
                                    text.Size = new Vector2(1f, text.TextSize.Y);
                                    MyGuiControlParent parent1 = new MyGuiControlParent();
                                    parent1.Size = new Vector2(1f, text.TextSize.Y - 0.01f);
                                    MyGuiControlParent parent2 = parent1;
                                    parent2.Controls.Add(text);
                                    controlByName.Controls.Add(parent2);
                                    text.PositionX -= 0.12f;
                                    text.PositionY -= (parent2.Size.Y / 2f) - 0.012f;
                                    MyGuiControlParent parent4 = new MyGuiControlParent();
                                    parent4.Size = new Vector2(1f, 0.02f);
                                    MyGuiControlParent parent3 = parent4;
                                    MyGuiControlSeparatorList list4 = new MyGuiControlSeparatorList();
                                    nullable2 = null;
                                    list4.AddHorizontal(new Vector2(-0.15f, 0f), 0.3f, 0.003f, nullable2);
                                    parent3.Controls.Add(list4);
                                    controlByName.Controls.Add(parent3);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void setDestructibleBlocks_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            m_grid.DestructibleBlocks = obj.IsChecked;
        }

        private void setDestructibleBlocksBtn_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            m_grid.DestructibleBlocks = obj.IsChecked;
        }

        private void showAntenaGizmos_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            MyCubeGrid.ShowAntennaGizmos = obj.IsChecked;
            (from x in Sandbox.Game.Entities.MyEntities.GetEntities()
                where x is MyCubeGrid
                select x).Cast<MyCubeGrid>().ForEach<MyCubeGrid>(x => x.MarkForDraw());
        }

        private void showGravityGizmos_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            MyCubeGrid.ShowGravityGizmos = obj.IsChecked;
            (from x in Sandbox.Game.Entities.MyEntities.GetEntities()
                where x is MyCubeGrid
                select x).Cast<MyCubeGrid>().ForEach<MyCubeGrid>(x => x.MarkForDraw());
        }

        private static void ShowPlayerNotOnlineMessage(MyIdentity identity)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextPlayerNotOnline, identity.DisplayName), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, result => RecreateControls(), 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
        }

        private void showSenzorGizmos_IsCheckedChanged(MyGuiControlCheckbox obj)
        {
            MyCubeGrid.ShowSenzorGizmos = obj.IsChecked;
            (from x in Sandbox.Game.Entities.MyEntities.GetEntities()
                where x is MyCubeGrid
                select x).Cast<MyCubeGrid>().ForEach<MyCubeGrid>(x => x.MarkForDraw());
        }

        public void UpdateBeforeDraw()
        {
            if (m_controlsDirty)
            {
                m_controlsDirty = false;
                if (MyFakes.ENABLE_CENTER_OF_MASS)
                {
                    MyGuiControlCheckbox checkbox1 = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("CenterBtn");
                    checkbox1.IsChecked = MyCubeGrid.ShowCenterOfMass;
                    checkbox1.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.centerBtn_IsCheckedChanged);
                    MyGuiControlCheckbox checkbox2 = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("PivotBtn");
                    checkbox2.IsChecked = MyCubeGrid.ShowGridPivot;
                    checkbox2.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.pivotBtn_IsCheckedChanged);
                }
                MyGuiControlCheckbox controlByName = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("ShowGravityGizmo");
                controlByName.IsChecked = MyCubeGrid.ShowGravityGizmos;
                controlByName.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.showGravityGizmos_IsCheckedChanged);
                MyGuiControlCheckbox checkbox4 = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("ShowSenzorGizmo");
                checkbox4.IsChecked = MyCubeGrid.ShowSenzorGizmos;
                checkbox4.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.showSenzorGizmos_IsCheckedChanged);
                MyGuiControlCheckbox checkbox5 = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("ShowAntenaGizmo");
                checkbox5.IsChecked = MyCubeGrid.ShowAntennaGizmos;
                checkbox5.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.showAntenaGizmos_IsCheckedChanged);
                MyGuiControlSlider slider1 = (MyGuiControlSlider) m_infoPage.Controls.GetControlByName("FriendAntennaRange");
                slider1.Value = MyHudMarkerRender.FriendAntennaRange;
                slider1.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider1.ValueChanged, s => MyHudMarkerRender.FriendAntennaRange = s.Value);
                slider1.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_FriendlyAntennaRange_ToolTip));
                MyGuiControlSlider slider2 = (MyGuiControlSlider) m_infoPage.Controls.GetControlByName("EnemyAntennaRange");
                slider2.Value = MyHudMarkerRender.EnemyAntennaRange;
                slider2.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider2.ValueChanged, s => MyHudMarkerRender.EnemyAntennaRange = s.Value);
                slider2.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_EnemyAntennaRange_ToolTip));
                MyGuiControlSlider slider3 = (MyGuiControlSlider) m_infoPage.Controls.GetControlByName("OwnedAntennaRange");
                slider3.Value = MyHudMarkerRender.OwnerAntennaRange;
                slider3.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider3.ValueChanged, s => MyHudMarkerRender.OwnerAntennaRange = s.Value);
                slider3.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_OwnedAntennaRange_ToolTip));
                if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                {
                    bool flag = this.IsPlayerOwner(m_grid);
                    MyGuiControlLabel label13 = (MyGuiControlLabel) m_infoPage.Controls.GetControlByName("RenameShipLabel");
                    MyGuiControlTextbox textbox = (MyGuiControlTextbox) m_infoPage.Controls.GetControlByName("RenameShipText");
                    if (textbox != null)
                    {
                        if (m_grid != null)
                        {
                            textbox.Text = m_grid.DisplayName;
                        }
                        textbox.Enabled = flag;
                    }
                    label13.Enabled = flag;
                    ((MyGuiControlButton) m_infoPage.Controls.GetControlByName("RenameShipButton")).Enabled = flag;
                }
                MyGuiControlButton button = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertBtn");
                MyGuiControlButton button2 = (MyGuiControlButton) m_infoPage.Controls.GetControlByName("ConvertToStationBtn");
                MyGuiControlList list = (MyGuiControlList) m_infoPage.Controls.GetControlByName("InfoList");
                list.Controls.Clear();
                MyGuiControlCheckbox checkbox = (MyGuiControlCheckbox) m_infoPage.Controls.GetControlByName("SetDestructibleBlocks");
                checkbox.Visible = MySession.Static.Settings.ScenarioEditMode || MySession.Static.IsScenario;
                checkbox.Enabled = MySession.Static.Settings.ScenarioEditMode;
                if ((m_grid == null) || (m_grid.Physics == null))
                {
                    button.Enabled = false;
                    button2.Enabled = false;
                    ((MyGuiControlLabel) m_infoPage.Controls.GetControlByName("Infolabel")).Text = MyTexts.GetString(MySpaceTexts.TerminalTab_Info_Overview);
                    RequestServerLimitInfo(MySession.Static.LocalPlayerId);
                }
                else
                {
                    if (!m_grid.IsStatic)
                    {
                        button.Enabled = false;
                        button2.Enabled = true;
                    }
                    else
                    {
                        button.Enabled = true;
                        button2.Enabled = false;
                    }
                    if (m_grid.GridSizeEnum == MyCubeSize.Small)
                    {
                        button2.Enabled = false;
                    }
                    if (!m_grid.BigOwners.Contains(MySession.Static.LocalPlayerId) && !MySession.Static.IsUserSpaceMaster(Sync.MyId))
                    {
                        button.Enabled = false;
                        button2.Enabled = false;
                    }
                    checkbox.IsChecked = m_grid.DestructibleBlocks;
                    checkbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.setDestructibleBlocks_IsCheckedChanged);
                    int number = 0;
                    if (m_grid.BlocksCounters.ContainsKey(typeof(MyObjectBuilder_GravityGenerator)))
                    {
                        number = m_grid.BlocksCounters[typeof(MyObjectBuilder_GravityGenerator)];
                    }
                    int num2 = 0;
                    if (m_grid.BlocksCounters.ContainsKey(typeof(MyObjectBuilder_VirtualMass)))
                    {
                        num2 = m_grid.BlocksCounters[typeof(MyObjectBuilder_VirtualMass)];
                    }
                    int num3 = 0;
                    if (m_grid.BlocksCounters.ContainsKey(typeof(MyObjectBuilder_InteriorLight)))
                    {
                        num3 = m_grid.BlocksCounters[typeof(MyObjectBuilder_InteriorLight)];
                    }
                    int num4 = 0;
                    foreach (MyObjectBuilderType type in m_grid.BlocksCounters.Keys)
                    {
                        System.Type producedType = MyCubeBlockFactory.GetProducedType(type);
                        if (typeof(IMyConveyorSegmentBlock).IsAssignableFrom(producedType) || typeof(IMyConveyorEndpointBlock).IsAssignableFrom(producedType))
                        {
                            num4 += m_grid.BlocksCounters[type];
                        }
                    }
                    int num5 = 0;
                    foreach (MySlimBlock block in m_grid.GetBlocks())
                    {
                        if (block.FatBlock != null)
                        {
                            num5 += block.FatBlock.Model.GetTrianglesCount();
                        }
                    }
                    using (IEnumerator<MyCubeGridRenderCell> enumerator3 = m_grid.RenderData.Cells.Values.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            IEnumerator<KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>>> enumerator = enumerator3.Current.CubeParts.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>> current = enumerator.Current;
                                    num5 += current.Key.Model.GetTrianglesCount();
                                }
                            }
                            finally
                            {
                                if (enumerator == null)
                                {
                                    continue;
                                }
                                enumerator.Dispose();
                            }
                        }
                    }
                    int thrustCount = 0;
                    MyEntityThrustComponent component = m_grid.Components.Get<MyEntityThrustComponent>();
                    if (component != null)
                    {
                        thrustCount = component.ThrustCount;
                    }
                    Vector2? position = null;
                    position = null;
                    VRageMath.Vector4? colorMask = null;
                    MyGuiControlLabel label = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Thrusters)).AppendInt32(thrustCount).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label2 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Triangles)).AppendInt32(num5).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label2.SetToolTip(MySpaceTexts.TerminalTab_Info_TrianglesTooltip);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label3 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Blocks)).AppendInt32(m_grid.GetBlocks().Count).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label3.SetToolTip(MySpaceTexts.TerminalTab_Info_BlocksTooltip);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label4 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(new StringBuilder("PCU: ")).AppendInt32(m_grid.BlocksPCU).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    label3.SetToolTip(MySpaceTexts.TerminalTab_Info_BlocksTooltip);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label5 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_NonArmor)).AppendInt32(m_grid.Hierarchy.Children.Count).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label6 = new MyGuiControlLabel(position, position, new StringBuilder().Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Lights)).AppendInt32(num3).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label7 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Reflectors)).AppendInt32(m_grid.GridSystems.ReflectorLightSystem.ReflectorCount).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label8 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_GravGens)).AppendInt32(number).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label9 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_VirtualMass)).AppendInt32(num2).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label10 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Conveyors)).AppendInt32(num4).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label11 = new MyGuiControlLabel(position, position, new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.TerminalTab_Info_GridMass)).AppendInt32(m_grid.GetCurrentMass()).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    position = null;
                    position = null;
                    colorMask = null;
                    MyGuiControlLabel label12 = new MyGuiControlLabel(position, position, string.Format(MyTexts.Get(MySpaceTexts.TerminalTab_Info_Shapes).ToString(), m_grid.ShapeCount, 0x10000), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    MyGuiControlBase[] controls = new MyGuiControlBase[12];
                    controls[0] = label3;
                    controls[1] = label5;
                    controls[2] = label4;
                    controls[3] = label10;
                    controls[4] = label;
                    controls[5] = label6;
                    controls[6] = label7;
                    controls[7] = label8;
                    controls[8] = label9;
                    controls[9] = label2;
                    controls[10] = label11;
                    controls[11] = label12;
                    list.InitControls(controls);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalInfoController.<>c <>9 = new MyTerminalInfoController.<>c();
            public static Action<MyGuiControlSlider> <>9__8_0;
            public static Action<MyGuiControlSlider> <>9__8_1;
            public static Action<MyGuiControlSlider> <>9__8_2;
            public static Func<IMyEventOwner, Action<long, ulong>> <>9__11_0;
            public static Func<MySlimBlock, int> <>9__13_1;
            public static Func<IMyEventOwner, Action<List<MyTerminalInfoController.GridBuiltByIdInfo>>> <>9__13_0;
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__16_0;
            public static Action<MyCubeGrid> <>9__16_1;
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__17_0;
            public static Action<MyCubeGrid> <>9__17_1;
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__18_0;
            public static Action<MyCubeGrid> <>9__18_1;
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__19_0;
            public static Action<MyCubeGrid> <>9__19_1;
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__20_0;
            public static Action<MyCubeGrid> <>9__20_1;
            public static Func<IMyEventOwner, Action<long, long>> <>9__26_1;
            public static Func<IMyEventOwner, Action<MyBlockLimits.MyGridLimitData, long, long, ulong>> <>9__27_1;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__28_0;

            internal Action<MyBlockLimits.MyGridLimitData, long, long, ulong> <assignCombobox_ItemSelected>b__27_1(IMyEventOwner x) => 
                new Action<MyBlockLimits.MyGridLimitData, long, long, ulong>(MyBlockLimits.SendTransferRequestMessage);

            internal bool <centerBtn_IsCheckedChanged>b__19_0(VRage.Game.Entity.MyEntity x) => 
                (x is MyCubeGrid);

            internal void <centerBtn_IsCheckedChanged>b__19_1(MyCubeGrid x)
            {
                x.MarkForDraw();
            }

            internal Action<long, long> <deleteBuiltByIdBlocksButton_ButtonClicked>b__26_1(IMyEventOwner x) => 
                new Action<long, long>(MyBlockLimits.RemoveBlocksBuiltByID);

            internal bool <pivotBtn_IsCheckedChanged>b__20_0(VRage.Game.Entity.MyEntity x) => 
                (x is MyCubeGrid);

            internal void <pivotBtn_IsCheckedChanged>b__20_1(MyCubeGrid x)
            {
                x.MarkForDraw();
            }

            internal Action<long, ulong> <RequestServerLimitInfo>b__11_0(IMyEventOwner x) => 
                new Action<long, ulong>(MyTerminalInfoController.ServerLimitInfo_Implementation);

            internal Action<List<MyTerminalInfoController.GridBuiltByIdInfo>> <ServerLimitInfo_Implementation>b__13_0(IMyEventOwner x) => 
                new Action<List<MyTerminalInfoController.GridBuiltByIdInfo>>(MyTerminalInfoController.ServerLimitInfo_Received);

            internal int <ServerLimitInfo_Implementation>b__13_1(MySlimBlock x) => 
                x.BlockDefinition.PCU;

            internal bool <showAntenaGizmos_IsCheckedChanged>b__16_0(VRage.Game.Entity.MyEntity x) => 
                (x is MyCubeGrid);

            internal void <showAntenaGizmos_IsCheckedChanged>b__16_1(MyCubeGrid x)
            {
                x.MarkForDraw();
            }

            internal bool <showGravityGizmos_IsCheckedChanged>b__18_0(VRage.Game.Entity.MyEntity x) => 
                (x is MyCubeGrid);

            internal void <showGravityGizmos_IsCheckedChanged>b__18_1(MyCubeGrid x)
            {
                x.MarkForDraw();
            }

            internal void <ShowPlayerNotOnlineMessage>b__28_0(MyGuiScreenMessageBox.ResultEnum result)
            {
                MyTerminalInfoController.RecreateControls();
            }

            internal bool <showSenzorGizmos_IsCheckedChanged>b__17_0(VRage.Game.Entity.MyEntity x) => 
                (x is MyCubeGrid);

            internal void <showSenzorGizmos_IsCheckedChanged>b__17_1(MyCubeGrid x)
            {
                x.MarkForDraw();
            }

            internal void <UpdateBeforeDraw>b__8_0(MyGuiControlSlider s)
            {
                MyHudMarkerRender.FriendAntennaRange = s.Value;
            }

            internal void <UpdateBeforeDraw>b__8_1(MyGuiControlSlider s)
            {
                MyHudMarkerRender.EnemyAntennaRange = s.Value;
            }

            internal void <UpdateBeforeDraw>b__8_2(MyGuiControlSlider s)
            {
                MyHudMarkerRender.OwnerAntennaRange = s.Value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GridBuiltByIdInfo
        {
            public string GridName;
            public long EntityId;
            public int PCUBuilt;
            public int BlockCount;
            public List<string> UnsafeBlocks;
        }
    }
}


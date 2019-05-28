namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlGenericFunctionalBlock : MyGuiControlBase
    {
        private List<ITerminalControl> m_currentControls;
        private MyGuiControlSeparatorList m_separatorList;
        private MyGuiControlList m_terminalControlList;
        private MyGuiControlMultilineText m_blockPropertiesMultilineText;
        private MyTerminalBlock[] m_currentBlocks;
        private Dictionary<ITerminalControl, int> m_tmpControlDictionary;
        private bool m_recreatingControls;
        private MyGuiControlCombobox m_transferToCombobox;
        private MyGuiControlCombobox m_shareModeCombobox;
        private MyGuiControlLabel m_ownershipLabel;
        private MyGuiControlLabel m_ownerLabel;
        private MyGuiControlLabel m_transferToLabel;
        private MyGuiControlLabel m_shareWithLabel;
        private MyGuiControlButton m_npcButton;
        private List<MyCubeGrid.MySingleOwnershipRequest> m_requests;
        private bool m_askForConfirmation;
        private bool m_canChangeShareMode;
        private MyScenarioBuildingBlock dummy;

        internal MyGuiControlGenericFunctionalBlock(MyTerminalBlock block) : this(blockArray1)
        {
            MyTerminalBlock[] blockArray1 = new MyTerminalBlock[] { block };
        }

        internal MyGuiControlGenericFunctionalBlock(MyTerminalBlock[] blocks) : base(textOffset, textOffset, backgroundColor, null, null, false, true, true, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_currentControls = new List<ITerminalControl>();
            this.m_tmpControlDictionary = new Dictionary<ITerminalControl, int>(InstanceComparer<ITerminalControl>.Default);
            this.m_requests = new List<MyCubeGrid.MySingleOwnershipRequest>();
            this.m_askForConfirmation = true;
            this.m_canChangeShareMode = true;
            this.dummy = new MyScenarioBuildingBlock();
            Vector2? textOffset = null;
            textOffset = null;
            VRageMath.Vector4? backgroundColor = null;
            this.m_currentBlocks = blocks;
            this.m_separatorList = new MyGuiControlSeparatorList();
            base.Elements.Add(this.m_separatorList);
            this.m_terminalControlList = new MyGuiControlList();
            this.m_terminalControlList.VisualStyle = MyGuiControlListStyleEnum.Simple;
            this.m_terminalControlList.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_terminalControlList.Position = new Vector2(0.1f, 0.1f);
            base.Elements.Add(this.m_terminalControlList);
            backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_blockPropertiesMultilineText = new MyGuiControlMultilineText(new Vector2(0.05f, -0.195f), new Vector2(0.4f, 0.635f), backgroundColor, "Blue", 0.85f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            this.m_blockPropertiesMultilineText.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockPropertiesMultilineText.Text = new StringBuilder();
            base.Elements.Add(this.m_blockPropertiesMultilineText);
            backgroundColor = null;
            textOffset = null;
            textOffset = null;
            backgroundColor = null;
            this.m_transferToCombobox = new MyGuiControlCombobox(new Vector2?(Vector2.Zero), new Vector2(0.245f, 0.1f), backgroundColor, textOffset, 10, textOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            this.m_transferToCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_transferToCombobox_ItemSelected);
            this.m_transferToCombobox.SetToolTip(MyTexts.GetString(MySpaceTexts.ControlScreen_TransferCombobox));
            this.m_transferToCombobox.ShowTooltipWhenDisabled = true;
            base.Elements.Add(this.m_transferToCombobox);
            backgroundColor = null;
            textOffset = null;
            textOffset = null;
            backgroundColor = null;
            this.m_shareModeCombobox = new MyGuiControlCombobox(new Vector2?(Vector2.Zero), new Vector2(0.287f, 0.1f), backgroundColor, textOffset, 10, textOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            this.m_shareModeCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_shareModeCombobox_ItemSelected);
            this.m_shareModeCombobox.SetToolTip(MyTexts.GetString(MySpaceTexts.ControlScreen_ShareCombobox));
            this.m_shareModeCombobox.ShowTooltipWhenDisabled = true;
            base.Elements.Add(this.m_shareModeCombobox);
            textOffset = null;
            backgroundColor = null;
            this.m_ownershipLabel = new MyGuiControlLabel(new Vector2?(Vector2.Zero), textOffset, MyTexts.GetString(MySpaceTexts.BlockOwner_Owner) + ":", backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_ownershipLabel);
            textOffset = null;
            backgroundColor = null;
            this.m_ownerLabel = new MyGuiControlLabel(new Vector2?(Vector2.Zero), textOffset, string.Empty, backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_ownerLabel);
            textOffset = null;
            backgroundColor = null;
            this.m_transferToLabel = new MyGuiControlLabel(new Vector2?(Vector2.Zero), textOffset, MyTexts.GetString(MySpaceTexts.BlockOwner_TransferTo), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_transferToLabel);
            textOffset = null;
            backgroundColor = null;
            this.m_shareWithLabel = new MyGuiControlLabel(new Vector2?(Vector2.Zero), textOffset, MyTexts.GetString(MySpaceTexts.ControlScreen_ShareLabel), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_shareWithLabel);
            backgroundColor = null;
            visibleLinesCount = null;
            this.m_npcButton = new MyGuiControlButton(new Vector2(0.27f, -0.13f), MyGuiControlButtonStyleEnum.Rectangular, new Vector2(0.04f, 0.053f), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MyCommonTexts.AddNewNPC), new StringBuilder("+"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnNewNpcClick), GuiSounds.MouseClick, 0.75f, visibleLinesCount, false);
            base.Elements.Add(this.m_npcButton);
            this.m_npcButton.Enabled = false;
            this.m_npcButton.Enabled = MySession.Static.IsUserSpaceMaster(Sync.MyId);
            this.RecreateBlockControls();
            this.RecreateOwnershipControls();
            if (this.m_currentBlocks.Length != 0)
            {
                this.m_currentBlocks[0].PropertiesChanged += new Action<MyTerminalBlock>(this.block_PropertiesChanged);
                this.m_currentBlocks[0].IsOpenedInTerminal = true;
            }
            MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
            for (int i = 0; i < currentBlocks.Length; i++)
            {
                MyTerminalBlock block1 = currentBlocks[i];
                block1.OwnershipChanged += new Action<MyTerminalBlock>(this.block_OwnershipChanged);
                block1.VisibilityChanged += new Action<MyTerminalBlock>(this.block_VisibilityChanged);
            }
            Sync.Players.IdentitiesChanged += new Action(this.Players_IdentitiesChanged);
            this.UpdateDetailedInfo();
            base.Size = new Vector2(0.595f, 0.64f);
        }

        private void block_OwnershipChanged(MyTerminalBlock sender)
        {
            if (this.m_canChangeShareMode)
            {
                this.RecreateOwnershipControls();
                this.UpdateOwnerGui();
            }
        }

        private void block_PropertiesChanged(MyTerminalBlock sender)
        {
            if (this.m_canChangeShareMode)
            {
                MyScrollbar scrollBar = this.m_terminalControlList.GetScrollBar();
                float num = scrollBar.Value;
                this.RecreateBlockControls();
                using (List<ITerminalControl>.Enumerator enumerator = this.m_currentControls.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateVisual();
                    }
                }
                scrollBar.Value = MathHelper.Min(num, scrollBar.MaxSize);
                this.UpdateDetailedInfo();
            }
        }

        private void block_VisibilityChanged(MyTerminalBlock obj)
        {
            foreach (ITerminalControl control in this.m_currentControls)
            {
                if (control.GetGuiControl().Visible != control.IsVisible(obj))
                {
                    control.GetGuiControl().Visible = control.IsVisible(obj);
                }
            }
        }

        private bool GetOwnershipStatus(out long? owner)
        {
            bool flag = false;
            owner = 0;
            foreach (MyTerminalBlock block in this.m_currentBlocks)
            {
                if (block.IDModule != null)
                {
                    if (owner == 0)
                    {
                        owner = new long?(block.IDModule.Owner);
                    }
                    else if (owner.Value != block.IDModule.Owner)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        private bool GetShareMode(out MyOwnershipShareModeEnum? shareMode)
        {
            bool flag = false;
            shareMode = 0;
            foreach (MyTerminalBlock block in this.m_currentBlocks)
            {
                if (block.IDModule != null)
                {
                    if (shareMode == 0)
                    {
                        shareMode = new MyOwnershipShareModeEnum?(block.IDModule.ShareMode);
                    }
                    else if (((MyOwnershipShareModeEnum) shareMode.Value) != block.IDModule.ShareMode)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        public override MyGuiControlBase HandleInput()
        {
            base.HandleInput();
            return base.HandleInputElements();
        }

        private void m_shareModeCombobox_ItemSelected()
        {
            if (this.m_canChangeShareMode)
            {
                this.m_canChangeShareMode = false;
                bool flag = false;
                MyOwnershipShareModeEnum selectedKey = (MyOwnershipShareModeEnum) ((int) this.m_shareModeCombobox.GetSelectedKey());
                if (this.m_currentBlocks.Length != 0)
                {
                    this.m_requests.Clear();
                    MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
                    int index = 0;
                    while (true)
                    {
                        if (index >= currentBlocks.Length)
                        {
                            if (this.m_requests.Count > 0)
                            {
                                MyCubeGrid.ChangeOwnersRequest(selectedKey, this.m_requests, MySession.Static.LocalPlayerId);
                            }
                            break;
                        }
                        MyTerminalBlock block = currentBlocks[index];
                        if (((block.IDModule != null) && (selectedKey >= MyOwnershipShareModeEnum.None)) && ((block.OwnerId == MySession.Static.LocalPlayerId) || MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals)))
                        {
                            MyCubeGrid.MySingleOwnershipRequest item = new MyCubeGrid.MySingleOwnershipRequest {
                                BlockId = block.EntityId,
                                Owner = block.IDModule.Owner
                            };
                            this.m_requests.Add(item);
                            flag = true;
                        }
                        index++;
                    }
                }
                this.m_canChangeShareMode = true;
                if (flag)
                {
                    this.block_PropertiesChanged(null);
                }
            }
        }

        private void m_transferToCombobox_ItemSelected()
        {
            if (this.m_transferToCombobox.GetSelectedIndex() != -1)
            {
                if (!this.m_askForConfirmation)
                {
                    this.UpdateOwnerGui();
                }
                else
                {
                    long ownerKey = this.m_transferToCombobox.GetSelectedKey();
                    int selectedIndex = this.m_transferToCombobox.GetSelectedIndex();
                    StringBuilder builder = this.m_transferToCombobox.GetItemByIndex(selectedIndex).Value;
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextChangeOwner), builder.ToString()), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                        if (retval != MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            this.m_askForConfirmation = false;
                            this.m_transferToCombobox.SelectItemByIndex(-1);
                            this.m_askForConfirmation = true;
                        }
                        else
                        {
                            if (this.m_currentBlocks.Length != 0)
                            {
                                this.m_requests.Clear();
                                MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
                                int index = 0;
                                while (true)
                                {
                                    if (index >= currentBlocks.Length)
                                    {
                                        if (this.m_requests.Count > 0)
                                        {
                                            if (MySession.Static.IsUserSpaceMaster(MySession.Static.LocalHumanPlayer.Client.SteamUserId) && Sync.Players.IdentityIsNpc(ownerKey))
                                            {
                                                MyCubeGrid.ChangeOwnersRequest(MyOwnershipShareModeEnum.Faction, this.m_requests, MySession.Static.LocalPlayerId);
                                            }
                                            else if (MySession.Static.LocalPlayerId == ownerKey)
                                            {
                                                MyCubeGrid.ChangeOwnersRequest(MyOwnershipShareModeEnum.Faction, this.m_requests, MySession.Static.LocalPlayerId);
                                            }
                                            else
                                            {
                                                MyCubeGrid.ChangeOwnersRequest(MyOwnershipShareModeEnum.None, this.m_requests, MySession.Static.LocalPlayerId);
                                            }
                                        }
                                        break;
                                    }
                                    MyTerminalBlock block = currentBlocks[index];
                                    if ((block.IDModule != null) && (((block.OwnerId == 0) || (block.OwnerId == MySession.Static.LocalPlayerId)) || MySession.Static.IsUserSpaceMaster(Sync.MyId)))
                                    {
                                        MyCubeGrid.MySingleOwnershipRequest item = new MyCubeGrid.MySingleOwnershipRequest {
                                            BlockId = block.EntityId,
                                            Owner = ownerKey
                                        };
                                        this.m_requests.Add(item);
                                    }
                                    index++;
                                }
                            }
                            this.RecreateOwnershipControls();
                            this.UpdateOwnerGui();
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.NO, true, size);
                    screen.CanHideOthers = false;
                    MyGuiSandbox.AddScreen(screen);
                }
            }
        }

        private void OnNewNpcClick(MyGuiControlButton button)
        {
            Sync.Players.RequestNewNpcIdentity();
        }

        public override void OnRemoving()
        {
            this.m_currentControls.Clear();
            if (this.m_currentBlocks.Length != 0)
            {
                this.m_currentBlocks[0].PropertiesChanged -= new Action<MyTerminalBlock>(this.block_PropertiesChanged);
                this.m_currentBlocks[0].IsOpenedInTerminal = false;
            }
            MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
            for (int i = 0; i < currentBlocks.Length; i++)
            {
                MyTerminalBlock block1 = currentBlocks[i];
                block1.OwnershipChanged -= new Action<MyTerminalBlock>(this.block_OwnershipChanged);
                block1.VisibilityChanged -= new Action<MyTerminalBlock>(this.block_VisibilityChanged);
            }
            Sync.Players.IdentitiesChanged -= new Action(this.Players_IdentitiesChanged);
            base.OnRemoving();
        }

        protected override void OnSizeChanged()
        {
            if (this.m_currentBlocks.Length != 0)
            {
                Vector2 vector = base.Size * -0.5f;
                Vector2 vector2 = new Vector2(0.3f, 0.55f);
                this.m_separatorList.Clear();
                VRageMath.Vector4? color = null;
                this.m_separatorList.AddHorizontal(vector + new Vector2(vector2.X + 0.008f, 0.11f), vector2.X * 0.96f, 0f, color);
                this.m_terminalControlList.Position = vector + new Vector2((vector2.X * 0.5f) - 0.006f, -0.032f);
                this.m_terminalControlList.Size = new Vector2(vector2.X - 0.013f, 0.675f);
                float num = 0.06f;
                if (MyFakes.SHOW_FACTIONS_GUI)
                {
                    MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
                    for (int i = 0; i < currentBlocks.Length; i++)
                    {
                        if (currentBlocks[i].IDModule != null)
                        {
                            num = 0.22f;
                            color = null;
                            this.m_separatorList.AddHorizontal(vector + new Vector2(vector2.X + 0.008f, num + 0.11f), vector2.X * 0.96f, 0f, color);
                            break;
                        }
                    }
                }
                this.m_blockPropertiesMultilineText.Position = vector + new Vector2(vector2.X + 0.012f, num + 0.133f);
                this.m_blockPropertiesMultilineText.Size = ((Vector2) (0.5f * base.Size)) - this.m_blockPropertiesMultilineText.Position;
                base.OnSizeChanged();
            }
        }

        private void Players_IdentitiesChanged()
        {
            this.UpdateOwnerGui();
        }

        private void RecreateBlockControls()
        {
            if (!this.m_recreatingControls)
            {
                this.m_currentControls.Clear();
                this.m_terminalControlList.Controls.Clear();
                try
                {
                    this.m_recreatingControls = true;
                    MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
                    int index = 0;
                    while (true)
                    {
                        if (index >= currentBlocks.Length)
                        {
                            if (MySession.Static.Settings.ScenarioEditMode && MyFakes.ENABLE_NEW_TRIGGERS)
                            {
                                foreach (ITerminalControl control2 in MyTerminalControlFactory.GetControls(typeof(MyTerminalBlock)))
                                {
                                    this.m_tmpControlDictionary[control2] = this.m_currentBlocks.Length;
                                }
                            }
                            int length = this.m_currentBlocks.Length;
                            foreach (KeyValuePair<ITerminalControl, int> pair in this.m_tmpControlDictionary)
                            {
                                bool flag = pair.Value != 0;
                                if (((length <= 1) || pair.Key.SupportsMultipleBlocks) && ((pair.Value == length) && (pair.Key.GetGuiControl() != null)))
                                {
                                    pair.Key.GetGuiControl().Visible = flag;
                                    this.m_terminalControlList.Controls.Add(pair.Key.GetGuiControl());
                                    pair.Key.TargetBlocks = this.m_currentBlocks;
                                    pair.Key.UpdateVisual();
                                    this.m_currentControls.Add(pair.Key);
                                }
                            }
                            break;
                        }
                        MyTerminalBlock block = currentBlocks[index];
                        block.GetType();
                        foreach (ITerminalControl control in MyTerminalControls.Static.GetControls(block))
                        {
                            int num3;
                            this.m_tmpControlDictionary.TryGetValue(control, out num3);
                            this.m_tmpControlDictionary[control] = num3 + (control.IsVisible(block) ? 1 : 0);
                        }
                        index++;
                    }
                }
                finally
                {
                    this.m_tmpControlDictionary.Clear();
                    this.m_recreatingControls = false;
                }
            }
        }

        private void RecreateOwnershipControls()
        {
            bool flag = false;
            MyTerminalBlock[] currentBlocks = this.m_currentBlocks;
            for (int i = 0; i < currentBlocks.Length; i++)
            {
                if (currentBlocks[i].IDModule != null)
                {
                    flag = true;
                }
            }
            if (!flag || !MyFakes.SHOW_FACTIONS_GUI)
            {
                this.m_ownershipLabel.Visible = false;
                this.m_ownerLabel.Visible = false;
                this.m_transferToLabel.Visible = false;
                this.m_shareWithLabel.Visible = false;
                this.m_transferToCombobox.Visible = false;
                this.m_shareModeCombobox.Visible = false;
                if (this.m_npcButton != null)
                {
                    this.m_npcButton.Visible = false;
                }
            }
            else
            {
                this.m_ownershipLabel.Visible = true;
                this.m_ownerLabel.Visible = true;
                this.m_transferToLabel.Visible = true;
                this.m_shareWithLabel.Visible = true;
                this.m_transferToCombobox.Visible = true;
                this.m_shareModeCombobox.Visible = true;
                if (this.m_npcButton != null)
                {
                    this.m_npcButton.Visible = true;
                }
                Vector2 vector = Vector2.One * -0.5f;
                Vector2 vector2 = new Vector2(0.3f, 0.55f);
                this.m_ownershipLabel.Position = vector + new Vector2(vector2.X + 0.212f, 0.315f);
                this.m_ownerLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_ownerLabel.Position = this.m_ownershipLabel.Position + new Vector2(this.m_ownershipLabel.Size.X + 0.015f, 0f);
                this.m_transferToLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_transferToLabel.Position = vector + new Vector2(vector2.X + 0.212f, 0.335f);
                this.m_transferToCombobox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_transferToCombobox.Position = vector + new Vector2(vector2.X + 0.212f, 0.368f);
                this.m_shareWithLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_shareWithLabel.Position = vector + new Vector2(vector2.X + 0.212f, 0.42f);
                this.m_shareModeCombobox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_shareModeCombobox.Position = vector + new Vector2(vector2.X + 0.212f, 0.45f);
                this.m_shareModeCombobox.ClearItems();
                int? sortOrder = null;
                this.m_shareModeCombobox.AddItem(0L, MyTexts.Get(MySpaceTexts.BlockOwner_ShareNone), sortOrder, null);
                sortOrder = null;
                this.m_shareModeCombobox.AddItem(1L, MyTexts.Get(MySpaceTexts.BlockOwner_ShareFaction), sortOrder, null);
                sortOrder = null;
                this.m_shareModeCombobox.AddItem(2L, MyTexts.Get(MySpaceTexts.BlockOwner_ShareAll), sortOrder, null);
                this.UpdateOwnerGui();
            }
        }

        private void UpdateDetailedInfo()
        {
            this.m_blockPropertiesMultilineText.Text.Clear();
            if (this.m_currentBlocks.Length == 1)
            {
                MyTerminalBlock block = this.m_currentBlocks[0];
                this.m_blockPropertiesMultilineText.Text.AppendStringBuilder(block.DetailedInfo);
                if (block.CustomInfo.Length > 0)
                {
                    this.m_blockPropertiesMultilineText.Text.TrimTrailingWhitespace().AppendLine();
                    this.m_blockPropertiesMultilineText.Text.AppendStringBuilder(block.CustomInfo);
                }
                this.m_blockPropertiesMultilineText.Text.Autowrap(0.29f, "Blue", 0.8f * MyGuiManager.LanguageTextScale);
                this.m_blockPropertiesMultilineText.RefreshText(false);
            }
        }

        private void UpdateOwnerGui()
        {
            long? nullable;
            bool ownershipStatus = this.GetOwnershipStatus(out nullable);
            this.m_transferToCombobox.ClearItems();
            if (ownershipStatus || (nullable != null))
            {
                int? nullable2;
                if (ownershipStatus || (nullable.Value != 0))
                {
                    nullable2 = null;
                    this.m_transferToCombobox.AddItem(0L, MyTexts.Get(MySpaceTexts.BlockOwner_Nobody), nullable2, null);
                }
                if (ownershipStatus || (nullable.Value != MySession.Static.LocalPlayerId))
                {
                    nullable2 = null;
                    this.m_transferToCombobox.AddItem(MySession.Static.LocalPlayerId, MyTexts.Get(MySpaceTexts.BlockOwner_Me), nullable2, null);
                }
                if (MySession.Static.IsUserAdmin(Sync.MyId))
                {
                    foreach (KeyValuePair<long, MyIdentity> pair in MySession.Static.Players.GetAllIdentitiesOrderByName())
                    {
                        if (pair.Value.IdentityId == MySession.Static.LocalPlayerId)
                        {
                            continue;
                        }
                        if (!MySession.Static.Players.IdentityIsNpc(pair.Value.IdentityId) && (((MySession.Static.LocalHumanPlayer.GetRelationTo(pair.Value.IdentityId) != MyRelationsBetweenPlayerAndBlock.Enemies) || MySession.Static.CreativeMode) || MySession.Static.CreativeToolsEnabled(Sync.MyId)))
                        {
                            nullable2 = null;
                            this.m_transferToCombobox.AddItem(pair.Value.IdentityId, new StringBuilder(pair.Value.DisplayName), nullable2, null);
                        }
                    }
                }
                else
                {
                    using (IEnumerator<MyPlayer> enumerator2 = Sync.Players.GetOnlinePlayers().GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            MyIdentity identity = enumerator2.Current.Identity;
                            if ((identity.IdentityId != MySession.Static.LocalPlayerId) && (!identity.IsDead && (((MySession.Static.LocalHumanPlayer.GetRelationTo(identity.IdentityId) != MyRelationsBetweenPlayerAndBlock.Enemies) || MySession.Static.CreativeMode) || MySession.Static.CreativeToolsEnabled(Sync.MyId))))
                            {
                                nullable2 = null;
                                this.m_transferToCombobox.AddItem(identity.IdentityId, new StringBuilder(identity.DisplayName), nullable2, null);
                            }
                        }
                    }
                }
                foreach (long num in Sync.Players.GetNPCIdentities())
                {
                    MyIdentity identity2 = Sync.Players.TryGetIdentity(num);
                    if ((identity2 != null) && (((MySession.Static.LocalHumanPlayer.GetRelationTo(identity2.IdentityId) != MyRelationsBetweenPlayerAndBlock.Enemies) || MySession.Static.CreativeMode) || MySession.Static.CreativeToolsEnabled(Sync.MyId)))
                    {
                        nullable2 = null;
                        this.m_transferToCombobox.AddItem(identity2.IdentityId, new StringBuilder(identity2.DisplayName), nullable2, null);
                    }
                }
                if (ownershipStatus)
                {
                    this.m_shareModeCombobox.Enabled = true;
                    this.m_shareModeCombobox.SetToolTip(MyTexts.GetString(MySpaceTexts.ControlScreen_ShareCombobox));
                    this.m_ownerLabel.Text = "";
                    this.m_canChangeShareMode = false;
                    this.m_shareModeCombobox.SelectItemByIndex(-1);
                    this.m_canChangeShareMode = true;
                }
                else
                {
                    MyOwnershipShareModeEnum? nullable3;
                    if ((nullable.Value == MySession.Static.LocalPlayerId) || MySession.Static.IsUserSpaceMaster(Sync.MyId))
                    {
                        this.m_shareModeCombobox.Enabled = true;
                        this.m_shareModeCombobox.SetToolTip(MyTexts.GetString(MySpaceTexts.ControlScreen_ShareCombobox));
                    }
                    else
                    {
                        this.m_shareModeCombobox.Enabled = false;
                        this.m_shareModeCombobox.SetToolTip(MyTexts.GetString(MySpaceTexts.ControlScreen_ShareComboboxDisabled));
                    }
                    if (nullable.Value == 0)
                    {
                        this.m_transferToCombobox.Enabled = true;
                        this.m_ownerLabel.TextEnum = MySpaceTexts.BlockOwner_Nobody;
                    }
                    else
                    {
                        this.m_transferToCombobox.Enabled = (nullable.Value == MySession.Static.LocalPlayerId) || MySession.Static.IsUserSpaceMaster(Sync.MyId);
                        this.m_ownerLabel.TextEnum = MySpaceTexts.BlockOwner_Me;
                        if (nullable.Value != MySession.Static.LocalPlayerId)
                        {
                            MyIdentity identity3 = Sync.Players.TryGetIdentity(nullable.Value);
                            if (identity3 != null)
                            {
                                this.m_ownerLabel.Text = identity3.DisplayName + (identity3.IsDead ? (" [" + MyTexts.Get(MyCommonTexts.PlayerInfo_Dead).ToString() + "]") : "");
                            }
                            else
                            {
                                this.m_ownerLabel.TextEnum = MySpaceTexts.BlockOwner_Unknown;
                            }
                        }
                    }
                    ownershipStatus = this.GetShareMode(out nullable3);
                    this.m_canChangeShareMode = false;
                    if ((ownershipStatus || (nullable3 == null)) || (nullable.Value == 0))
                    {
                        this.m_shareModeCombobox.SelectItemByIndex(-1);
                    }
                    else
                    {
                        this.m_shareModeCombobox.SelectItemByKey((long) nullable3.Value, true);
                    }
                    this.m_canChangeShareMode = true;
                }
            }
        }
    }
}


namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalControlPanel
    {
        private static readonly MyTerminalComparer m_nameComparer = new MyTerminalComparer();
        private IMyGuiControlsParent m_controlsParent;
        private MyGuiControlListbox m_blockListbox;
        private MyGuiControlLabel m_blockNameLabel;
        private MyGuiControlBase m_blockControl;
        private MyGridTerminalSystem m_terminalSystem;
        private List<MyBlockGroup> m_currentGroups = new List<MyBlockGroup>();
        private MyBlockGroup m_tmpGroup;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlTextbox m_groupName;
        private MyGuiControlButton m_groupSave;
        private MyGuiControlButton m_showAll;
        private MyGuiControlButton m_groupDelete;
        private List<MyBlockGroup> m_oldGroups = new List<MyBlockGroup>();
        private MyTerminalBlock m_originalBlock;
        private static bool m_showAllTerminalBlocks = false;
        private MyGridColorHelper m_colorHelper;
        private MyPlayer m_controller;

        private MyGuiControlListbox.Item AddBlockToList(MyTerminalBlock block, bool? visibility = new bool?())
        {
            StringBuilder result = new StringBuilder();
            block.GetTerminalName(result);
            MyDLCs.MyDLC firstMissingDefinitionDLC = MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(block.BlockDefinition, Sync.MyId);
            if (firstMissingDefinitionDLC != null)
            {
                result.Append(" (").Append(MyDLCs.GetRequiredDLCTooltip(firstMissingDefinitionDLC.AppId)).Append(")");
            }
            object userData = block;
            MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(null, result.ToString(), null, userData, null);
            this.UpdateItemAppearance(block, item);
            block.CustomNameChanged += new Action<MyTerminalBlock>(this.block_CustomNameChanged);
            block.PropertiesChanged += new Action<MyTerminalBlock>(this.block_CustomNameChanged);
            block.ShowInTerminalChanged += new Action<MyTerminalBlock>(this.block_ShowInTerminalChanged);
            if (visibility != null)
            {
                item.Visible = visibility.Value;
            }
            int? position = null;
            this.m_blockListbox.Add(item, position);
            return item;
        }

        private void AddGroupToList(MyBlockGroup group, int? position = new int?())
        {
            foreach (MyGuiControlListbox.Item item2 in this.m_blockListbox.Items)
            {
                MyBlockGroup group2 = item2.UserData as MyBlockGroup;
                if ((group2 != null) && (group2.Name.CompareTo(group.Name) == 0))
                {
                    this.m_blockListbox.Items.Remove(item2);
                    break;
                }
            }
            object userData = group;
            MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(null, group.Name.ToString(), null, userData, null);
            item.Text.Clear().Append("*").AppendStringBuilder(group.Name).Append("*");
            this.m_blockListbox.Add(item, position);
        }

        private void block_CustomNameChanged(MyTerminalBlock obj)
        {
            if (this.m_blockListbox != null)
            {
                foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                {
                    if (item.UserData == obj)
                    {
                        this.UpdateItemAppearance(obj, item);
                        break;
                    }
                }
                if ((this.CurrentBlocks.Count > 0) && (this.CurrentBlocks.FirstElement<MyTerminalBlock>() == obj))
                {
                    this.m_blockNameLabel.Text = obj.CustomName.ToString();
                }
            }
        }

        private void block_ShowInTerminalChanged(MyTerminalBlock obj)
        {
            MyTerminalBlock[] selectedBlocks = null;
            if (this.m_blockListbox != null)
            {
                List<MyGuiControlListbox.Item> selectedItems = this.m_blockListbox.SelectedItems;
                selectedBlocks = new MyTerminalBlock[selectedItems.Count];
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    if (selectedItems[i].UserData is MyTerminalBlock)
                    {
                        selectedBlocks[i] = (MyTerminalBlock) selectedItems[i].UserData;
                    }
                }
            }
            this.ClearBlockList();
            this.PopulateBlockList(selectedBlocks);
            if (this.m_blockListbox != null)
            {
                this.m_blockListbox.ScrollToolbarToTop();
            }
            this.blockSearch_TextChanged(this.m_searchBox.SearchText);
        }

        private void blockListbox_ItemSelected(MyGuiControlListbox sender)
        {
            this.m_oldGroups.Clear();
            this.m_oldGroups.AddList<MyBlockGroup>(this.m_currentGroups);
            this.m_currentGroups.Clear();
            this.m_tmpGroup.Blocks.Clear();
            foreach (MyGuiControlListbox.Item item in sender.SelectedItems)
            {
                if (item.UserData is MyBlockGroup)
                {
                    this.m_currentGroups.Add((MyBlockGroup) item.UserData);
                    continue;
                }
                if (item.UserData is MyTerminalBlock)
                {
                    this.CurrentBlocks.Add(item.UserData as MyTerminalBlock);
                }
            }
            for (int i = 0; i < this.m_currentGroups.Count; i++)
            {
                if (!this.m_oldGroups.Contains(this.m_currentGroups[i]) || (this.m_currentGroups[i].Blocks.Intersect<MyTerminalBlock>(this.CurrentBlocks).Count<MyTerminalBlock>() == 0))
                {
                    foreach (MyTerminalBlock block in this.m_currentGroups[i].Blocks)
                    {
                        if (!this.CurrentBlocks.Contains(block))
                        {
                            this.CurrentBlocks.Add(block);
                        }
                    }
                }
            }
            this.SelectBlocks();
        }

        private void blockSearch_TextChanged(string text)
        {
            if (this.m_blockListbox != null)
            {
                if (text != "")
                {
                    char[] separator = new char[] { ' ' };
                    string[] strArray = text.Split(separator);
                    foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                    {
                        bool flag = true;
                        if (item.UserData is MyTerminalBlock)
                        {
                            int num1;
                            if (((MyTerminalBlock) item.UserData).ShowInTerminal || m_showAllTerminalBlocks)
                            {
                                num1 = 1;
                            }
                            else
                            {
                                num1 = (int) (item.UserData == this.m_originalBlock);
                            }
                            flag = (bool) num1;
                        }
                        if (flag)
                        {
                            string str = item.Text.ToString().ToLower();
                            string[] strArray2 = strArray;
                            int index = 0;
                            while (true)
                            {
                                if (index < strArray2.Length)
                                {
                                    string str2 = strArray2[index];
                                    if (str.Contains(str2.ToLower()))
                                    {
                                        index++;
                                        continue;
                                    }
                                    flag = false;
                                }
                                item.Visible = flag;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (MyGuiControlListbox.Item item2 in this.m_blockListbox.Items)
                    {
                        int num2;
                        if (!(item2.UserData is MyTerminalBlock))
                        {
                            item2.Visible = true;
                            continue;
                        }
                        MyTerminalBlock userData = (MyTerminalBlock) item2.UserData;
                        if (userData.ShowInTerminal || m_showAllTerminalBlocks)
                        {
                            num2 = 1;
                        }
                        else
                        {
                            num2 = (int) ReferenceEquals(userData, this.m_originalBlock);
                        }
                        item2.Visible = (bool) num2;
                    }
                }
                this.m_blockListbox.ScrollToolbarToTop();
            }
        }

        public void ClearBlockList()
        {
            if (this.m_blockListbox != null)
            {
                foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                {
                    if (item.UserData is MyTerminalBlock)
                    {
                        MyTerminalBlock userData = (MyTerminalBlock) item.UserData;
                        userData.CustomNameChanged -= new Action<MyTerminalBlock>(this.block_CustomNameChanged);
                        userData.PropertiesChanged -= new Action<MyTerminalBlock>(this.block_CustomNameChanged);
                        userData.ShowInTerminalChanged -= new Action<MyTerminalBlock>(this.block_ShowInTerminalChanged);
                    }
                }
                this.m_blockListbox.Items.Clear();
            }
        }

        public void Close()
        {
            if (this.m_terminalSystem != null)
            {
                if (this.m_blockListbox != null)
                {
                    this.ClearBlockList();
                    this.m_blockListbox.ItemsSelected -= new Action<MyGuiControlListbox>(this.blockListbox_ItemSelected);
                }
                this.m_terminalSystem.BlockAdded -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
                this.m_terminalSystem.BlockRemoved -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
                this.m_terminalSystem.BlockManipulationFinished -= new Action(this.TerminalSystem_BlockManipulationFinished);
                this.m_terminalSystem.GroupAdded -= new Action<MyBlockGroup>(this.TerminalSystem_GroupAdded);
                this.m_terminalSystem.GroupRemoved -= new Action<MyBlockGroup>(this.TerminalSystem_GroupRemoved);
            }
            if (this.m_tmpGroup != null)
            {
                this.m_tmpGroup.Blocks.Clear();
            }
            if (this.m_showAll != null)
            {
                this.m_showAll.ButtonClicked -= new Action<MyGuiControlButton>(this.showAll_Clicked);
            }
            this.m_controlsParent = null;
            this.m_blockListbox = null;
            this.m_blockNameLabel = null;
            this.m_terminalSystem = null;
            this.m_currentGroups.Clear();
        }

        private void groupDelete_ButtonClicked(MyGuiControlButton obj)
        {
            bool flag = false;
            using (List<MyBlockGroup>.Enumerator enumerator = this.m_currentGroups.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    using (HashSet<MyTerminalBlock>.Enumerator enumerator2 = enumerator.Current.Blocks.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (!enumerator2.Current.HasLocalPlayerAccess())
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                while (this.m_currentGroups.Count > 0)
                {
                    this.m_terminalSystem.RemoveGroup(this.m_currentGroups[0], true);
                }
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextCannotDeleteGroup), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void groupSave_ButtonClicked(MyGuiControlButton obj)
        {
            bool flag = false;
            using (HashSet<MyTerminalBlock>.Enumerator enumerator = this.m_tmpGroup.Blocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.HasLocalPlayerAccess())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextCannotCreateGroup), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            else if (this.m_groupName.Text != "")
            {
                this.m_currentGroups.Clear();
                this.m_tmpGroup.Name.Clear().Append(this.m_groupName.Text);
                this.m_tmpGroup = this.m_terminalSystem.AddUpdateGroup(this.m_tmpGroup, true, true);
                this.m_currentGroups.Add(this.m_tmpGroup);
                this.m_tmpGroup = new MyBlockGroup();
                this.CurrentBlocks.UnionWith(this.m_currentGroups[0].Blocks);
                this.SelectBlocks();
            }
        }

        public void Init(IMyGuiControlsParent controlsParent, MyPlayer controller, MyCubeGrid grid, MyTerminalBlock currentBlock, MyGridColorHelper colorHelper)
        {
            this.m_controlsParent = controlsParent;
            this.m_controller = controller;
            this.m_colorHelper = colorHelper;
            if (grid == null)
            {
                using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = controlsParent.Controls.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Visible = false;
                    }
                }
                MyGuiControlLabel control = MyGuiScreenTerminal.CreateErrorLabel(MySpaceTexts.ScreenTerminalError_ShipNotConnected, "ErrorMessage");
                controlsParent.Controls.Add(control);
            }
            else
            {
                this.m_terminalSystem = grid.GridSystems.TerminalSystem;
                this.m_tmpGroup = new MyBlockGroup();
                this.m_searchBox = (MyGuiControlSearchBox) this.m_controlsParent.Controls.GetControlByName("FunctionalBlockSearch");
                this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.blockSearch_TextChanged);
                this.m_blockListbox = (MyGuiControlListbox) this.m_controlsParent.Controls.GetControlByName("FunctionalBlockListbox");
                this.m_blockNameLabel = (MyGuiControlLabel) this.m_controlsParent.Controls.GetControlByName("BlockNameLabel");
                this.m_blockNameLabel.Text = "";
                this.m_groupName = (MyGuiControlTextbox) this.m_controlsParent.Controls.GetControlByName("GroupName");
                this.m_groupName.TextChanged += new Action<MyGuiControlTextbox>(this.m_groupName_TextChanged);
                this.m_groupName.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroup));
                this.m_groupName.ShowTooltipWhenDisabled = true;
                this.m_showAll = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("ShowAll");
                this.m_showAll.Selected = m_showAllTerminalBlocks;
                this.m_showAll.ButtonClicked += new Action<MyGuiControlButton>(this.showAll_Clicked);
                this.m_showAll.SetToolTip(MySpaceTexts.Terminal_ShowAllInTerminal);
                this.m_showAll.IconRotation = 0f;
                MyGuiHighlightTexture texture = new MyGuiHighlightTexture {
                    Normal = @"Textures\GUI\Controls\button_hide.dds",
                    Highlight = @"Textures\GUI\Controls\button_unhide.dds",
                    SizePx = new Vector2(40f, 40f)
                };
                this.m_showAll.Icon = new MyGuiHighlightTexture?(texture);
                this.m_showAll.Size = new Vector2(0f, 0f);
                this.m_showAll.HighlightType = MyGuiControlHighlightType.FORCED;
                this.m_groupSave = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("GroupSave");
                this.m_groupSave.TextEnum = MySpaceTexts.TerminalButton_GroupSave;
                this.m_groupSave.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
                this.m_groupSave.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
                this.m_groupSave.ButtonClicked += new Action<MyGuiControlButton>(this.groupSave_ButtonClicked);
                this.m_groupSave.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupSave));
                this.m_groupSave.ShowTooltipWhenDisabled = true;
                this.m_groupDelete = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("GroupDelete");
                this.m_groupDelete.ButtonClicked += new Action<MyGuiControlButton>(this.groupDelete_ButtonClicked);
                this.m_groupDelete.ShowTooltipWhenDisabled = true;
                this.m_groupDelete.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupDeleteDisabled));
                this.m_groupDelete.Enabled = false;
                this.m_blockListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.blockListbox_ItemSelected);
                this.m_originalBlock = currentBlock;
                MyTerminalBlock[] selectedBlocks = null;
                if (this.m_originalBlock != null)
                {
                    selectedBlocks = new MyTerminalBlock[] { this.m_originalBlock };
                }
                this.RefreshBlockList(selectedBlocks);
                this.m_terminalSystem.BlockAdded += new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
                this.m_terminalSystem.BlockRemoved += new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
                this.m_terminalSystem.BlockManipulationFinished += new Action(this.TerminalSystem_BlockManipulationFinished);
                this.m_terminalSystem.GroupAdded += new Action<MyBlockGroup>(this.TerminalSystem_GroupAdded);
                this.m_terminalSystem.GroupRemoved += new Action<MyBlockGroup>(this.TerminalSystem_GroupRemoved);
                this.blockSearch_TextChanged(this.m_searchBox.SearchText);
                this.m_blockListbox.ScrollToFirstSelection();
            }
        }

        private void m_groupName_TextChanged(MyGuiControlTextbox obj)
        {
            if (string.IsNullOrEmpty(obj.Text) || (this.CurrentBlocks.Count == 0))
            {
                this.m_groupSave.Enabled = false;
                this.m_groupSave.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupSaveDisabled));
            }
            else
            {
                this.m_groupSave.Enabled = true;
                this.m_groupSave.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupSave));
            }
        }

        public void PopulateBlockList(MyTerminalBlock[] selectedBlocks = null)
        {
            if (this.m_terminalSystem != null)
            {
                if (this.m_terminalSystem.BlockGroups == null)
                {
                    MySandboxGame.Log.WriteLine("m_terminalSystem.BlockGroups is null");
                }
                if (!this.m_terminalSystem.Blocks.IsValid)
                {
                    MySandboxGame.Log.WriteLine("m_terminalSystem.Blocks.IsValid is false");
                }
                if (this.CurrentBlocks == null)
                {
                    MySandboxGame.Log.WriteLine("CurrentBlocks is null");
                }
                if (this.m_blockListbox == null)
                {
                    MySandboxGame.Log.WriteLine("m_blockListbox is null");
                }
                MyBlockGroup[] array = this.m_terminalSystem.BlockGroups.ToArray();
                Array.Sort<MyBlockGroup>(array, MyTerminalComparer.Static);
                MyBlockGroup[] groupArray = array;
                int index = 0;
                while (index < groupArray.Length)
                {
                    MyBlockGroup group = groupArray[index];
                    int? position = null;
                    this.AddGroupToList(group, position);
                    index++;
                }
                MyTerminalBlock[] localArray2 = this.m_terminalSystem.Blocks.ToArray();
                Array.Sort<MyTerminalBlock>(localArray2, MyTerminalComparer.Static);
                this.m_blockListbox.SelectedItems.Clear();
                MyTerminalBlock[] blockArray = localArray2;
                index = 0;
                while (true)
                {
                    int showAllTerminalBlocks;
                    if (index >= blockArray.Length)
                    {
                        if (selectedBlocks == null)
                        {
                            if (this.CurrentBlocks.Count > 0)
                            {
                                this.SelectBlocks();
                                return;
                            }
                            foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                            {
                                if (item.UserData is MyTerminalBlock)
                                {
                                    MyTerminalBlock[] blocks = new MyTerminalBlock[] { (MyTerminalBlock) item.UserData };
                                    this.SelectBlocks(blocks);
                                    break;
                                }
                            }
                            break;
                        }
                        this.SelectBlocks(selectedBlocks);
                        break;
                    }
                    MyTerminalBlock objA = blockArray[index];
                    if (ReferenceEquals(objA, this.m_originalBlock) || objA.ShowInTerminal)
                    {
                        showAllTerminalBlocks = 1;
                    }
                    else
                    {
                        showAllTerminalBlocks = (int) m_showAllTerminalBlocks;
                    }
                    this.AddBlockToList(objA, new bool?((bool) showAllTerminalBlocks));
                    index++;
                }
            }
        }

        public void RefreshBlockList(MyTerminalBlock[] selectedBlocks = null)
        {
            if (this.m_blockListbox != null)
            {
                this.ClearBlockList();
                this.PopulateBlockList(selectedBlocks);
            }
        }

        public void SelectAllBlocks()
        {
            if (this.m_blockListbox != null)
            {
                this.m_blockListbox.SelectAllVisible();
            }
        }

        private void SelectBlocks()
        {
            if (this.m_blockControl != null)
            {
                this.m_controlsParent.Controls.Remove(this.m_blockControl);
                this.m_blockControl = null;
            }
            this.m_blockNameLabel.Text = "";
            this.m_groupName.Text = "";
            if (this.m_currentGroups.Count == 1)
            {
                this.m_blockNameLabel.Text = this.m_currentGroups[0].Name.ToString();
                this.m_groupName.Text = this.m_blockNameLabel.Text;
            }
            if (this.CurrentBlocks.Count > 0)
            {
                if (this.CurrentBlocks.Count == 1)
                {
                    this.m_blockNameLabel.Text = this.CurrentBlocks.FirstElement<MyTerminalBlock>().CustomName.ToString();
                }
                this.m_blockControl = new MyGuiControlGenericFunctionalBlock(this.CurrentBlocks.ToArray<MyTerminalBlock>());
                this.m_controlsParent.Controls.Add(this.m_blockControl);
                this.m_blockControl.Size = new Vector2(0.595f, 0.64f);
                this.m_blockControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_blockControl.Position = new Vector2(-0.1415f, -0.3f);
            }
            this.UpdateGroupControl();
            this.m_blockListbox.SelectedItems.Clear();
            foreach (MyTerminalBlock block in this.CurrentBlocks)
            {
                foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                {
                    if (item.UserData == block)
                    {
                        this.m_blockListbox.SelectedItems.Add(item);
                        break;
                    }
                }
            }
            foreach (MyBlockGroup group in this.m_currentGroups)
            {
                foreach (MyGuiControlListbox.Item item2 in this.m_blockListbox.Items)
                {
                    if (item2.UserData == group)
                    {
                        this.m_blockListbox.SelectedItems.Add(item2);
                        break;
                    }
                }
            }
        }

        public void SelectBlocks(MyTerminalBlock[] blocks)
        {
            this.m_tmpGroup.Blocks.Clear();
            this.m_tmpGroup.Blocks.UnionWith(blocks);
            this.m_currentGroups.Clear();
            this.CurrentBlocks.Clear();
            foreach (MyTerminalBlock block in blocks)
            {
                if (block != null)
                {
                    this.CurrentBlocks.Add(block);
                }
            }
            this.SelectBlocks();
        }

        private void showAll_Clicked(MyGuiControlButton obj)
        {
            m_showAllTerminalBlocks = !m_showAllTerminalBlocks;
            this.m_showAll.Selected = m_showAllTerminalBlocks;
            List<MyGuiControlListbox.Item> selectedItems = this.m_blockListbox.SelectedItems;
            MyTerminalBlock[] selectedBlocks = new MyTerminalBlock[selectedItems.Count];
            for (int i = 0; i < selectedItems.Count; i++)
            {
                if (selectedItems[i].UserData is MyTerminalBlock)
                {
                    selectedBlocks[i] = (MyTerminalBlock) selectedItems[i].UserData;
                }
            }
            this.ClearBlockList();
            this.PopulateBlockList(selectedBlocks);
            this.m_blockListbox.ScrollToolbarToTop();
            this.blockSearch_TextChanged(this.m_searchBox.SearchText);
        }

        private void TerminalSystem_BlockAdded(MyTerminalBlock obj)
        {
            bool? visibility = null;
            this.AddBlockToList(obj, visibility);
        }

        private void TerminalSystem_BlockManipulationFinished()
        {
            this.blockSearch_TextChanged(this.m_searchBox.SearchText);
        }

        private void TerminalSystem_BlockRemoved(MyTerminalBlock obj)
        {
            obj.CustomNameChanged -= new Action<MyTerminalBlock>(this.block_CustomNameChanged);
            obj.PropertiesChanged -= new Action<MyTerminalBlock>(this.block_CustomNameChanged);
            if ((this.m_blockListbox != null) && (obj.ShowInTerminal || m_showAllTerminalBlocks))
            {
                this.m_blockListbox.Remove(item => item.UserData == obj);
            }
        }

        private void TerminalSystem_GroupAdded(MyBlockGroup group)
        {
            if (this.m_blockListbox != null)
            {
                this.AddGroupToList(group, 0);
            }
        }

        private void TerminalSystem_GroupRemoved(MyBlockGroup group)
        {
            if (this.m_blockListbox != null)
            {
                foreach (MyGuiControlListbox.Item item in this.m_blockListbox.Items)
                {
                    if (item.UserData == group)
                    {
                        this.m_blockListbox.Items.Remove(item);
                        break;
                    }
                }
            }
        }

        public void UpdateCubeBlock(MyTerminalBlock block)
        {
            if (block != null)
            {
                if (this.m_terminalSystem != null)
                {
                    this.m_terminalSystem.BlockAdded -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
                    this.m_terminalSystem.BlockRemoved -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
                    this.m_terminalSystem.BlockManipulationFinished -= new Action(this.TerminalSystem_BlockManipulationFinished);
                    this.m_terminalSystem.GroupAdded -= new Action<MyBlockGroup>(this.TerminalSystem_GroupAdded);
                    this.m_terminalSystem.GroupRemoved -= new Action<MyBlockGroup>(this.TerminalSystem_GroupRemoved);
                }
                MyCubeGrid cubeGrid = block.CubeGrid;
                this.m_terminalSystem = cubeGrid.GridSystems.TerminalSystem;
                this.m_tmpGroup = new MyBlockGroup();
                this.m_terminalSystem.BlockAdded += new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
                this.m_terminalSystem.BlockRemoved += new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
                this.m_terminalSystem.BlockManipulationFinished += new Action(this.TerminalSystem_BlockManipulationFinished);
                this.m_terminalSystem.GroupAdded += new Action<MyBlockGroup>(this.TerminalSystem_GroupAdded);
                this.m_terminalSystem.GroupRemoved += new Action<MyBlockGroup>(this.TerminalSystem_GroupRemoved);
                MyTerminalBlock[] blocks = new MyTerminalBlock[] { block };
                this.SelectBlocks(blocks);
            }
        }

        private void UpdateGroupControl()
        {
            if (this.m_currentGroups.Count > 0)
            {
                this.m_groupDelete.Enabled = true;
                this.m_groupDelete.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupDelete));
            }
            else
            {
                this.m_groupDelete.Enabled = false;
                this.m_groupDelete.SetTooltip(MyTexts.GetString(MySpaceTexts.ControlScreen_TerminalBlockGroupDeleteDisabled));
            }
        }

        private void UpdateItemAppearance(MyTerminalBlock block, MyGuiControlListbox.Item item)
        {
            item.Text.Clear();
            block.GetTerminalName(item.Text);
            if (!block.IsFunctional)
            {
                item.ColorMask = VRageMath.Vector4.One;
                item.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Terminal_BlockIncomplete));
                item.FontOverride = "Red";
            }
            else
            {
                MyTerminalBlock.AccessRightsResult result;
                if ((result = block.HasPlayerAccessReason(this.m_controller.Identity.IdentityId)) == MyTerminalBlock.AccessRightsResult.Granted)
                {
                    if (!block.ShowInTerminal)
                    {
                        item.ColorMask = (VRageMath.Vector4) (0.6f * this.m_colorHelper.GetGridColor(block.CubeGrid).ToVector4());
                        item.FontOverride = null;
                    }
                    else
                    {
                        item.ColorMask = this.m_colorHelper.GetGridColor(block.CubeGrid).ToVector4();
                        item.FontOverride = null;
                    }
                }
                else
                {
                    item.ColorMask = VRageMath.Vector4.One;
                    if (result != MyTerminalBlock.AccessRightsResult.MissingDLC)
                    {
                        item.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Terminal_BlockAccessDenied));
                    }
                    else
                    {
                        string[] dLCs = block.BlockDefinition.DLCs;
                        for (int i = 0; i < dLCs.Length; i++)
                        {
                            MyDLCs.MyDLC ydlc;
                            if (MyDLCs.TryGetDLC(dLCs[i], out ydlc))
                            {
                                item.Text.Append(" (").Append(MyTexts.Get(MyCommonTexts.RequiresAnyDlc)).Append(")");
                            }
                        }
                    }
                    item.FontOverride = "Red";
                }
            }
        }

        private HashSet<MyTerminalBlock> CurrentBlocks =>
            this.m_tmpGroup.Blocks;

        public MyGridTerminalSystem TerminalSystem =>
            this.m_terminalSystem;
    }
}


namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
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
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalProductionController
    {
        public static readonly int BLUEPRINT_GRID_ROWS = 7;
        public static readonly int QUEUE_GRID_ROWS = 2;
        public static readonly int INVENTORY_GRID_ROWS = 3;
        private static readonly Vector4 ERROR_ICON_COLOR_MASK = new Vector4(1f, 0.5f, 0.5f, 1f);
        private static StringBuilder m_textCache = new StringBuilder();
        private static Dictionary<MyDefinitionId, MyFixedPoint> m_requiredCountCache = new Dictionary<MyDefinitionId, MyFixedPoint>(MyDefinitionId.Comparer);
        private static List<MyBlueprintDefinitionBase.ProductionInfo> m_blueprintCache = new List<MyBlueprintDefinitionBase.ProductionInfo>();
        private IMyGuiControlsParent m_controlsParent;
        private MyGridTerminalSystem m_terminalSystem;
        private Dictionary<int, MyAssembler> m_assemblersByKey = new Dictionary<int, MyAssembler>();
        private int m_assemblerKeyCounter;
        private MyGuiControlSearchBox m_blueprintsSearchBox;
        private MyGuiControlCombobox m_comboboxAssemblers;
        private MyGuiControlGrid m_blueprintsGrid;
        private MyAssembler m_selectedAssembler;
        private MyGuiControlRadioButtonGroup m_blueprintButtonGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlRadioButtonGroup m_modeButtonGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlGrid m_queueGrid;
        private MyGuiControlGrid m_inventoryGrid;
        private MyGuiControlComponentList m_materialsList;
        private MyGuiControlScrollablePanel m_blueprintsArea;
        private MyGuiControlScrollablePanel m_queueArea;
        private MyGuiControlScrollablePanel m_inventoryArea;
        private MyGuiControlBase m_blueprintsBgPanel;
        private MyGuiControlBase m_blueprintsLabel;
        private MyGuiControlCheckbox m_repeatCheckbox;
        private MyGuiControlCheckbox m_slaveCheckbox;
        private MyGuiControlButton m_disassembleAllButton;
        private MyGuiControlButton m_controlPanelButton;
        private MyGuiControlButton m_inventoryButton;
        private MyGuiControlLabel m_materialsLabel;
        private MyDragAndDropInfo m_dragAndDropInfo;
        private MyGuiControlGridDragAndDrop m_dragAndDrop;
        private StringBuilder m_incompleteAssemblerName = new StringBuilder();

        private void AddBlueprintClassButton(MyBlueprintClassDefinition classDef, ref float xOffset, bool selected = false)
        {
            if (classDef != null)
            {
                Vector4? colorMask = null;
                MyGuiControlRadioButton radioButton = new MyGuiControlRadioButton(new Vector2?(this.m_blueprintsLabel.Position + new Vector2(xOffset, this.m_blueprintsLabel.Size.Y + 0.012f)), new Vector2?(new Vector2(46f, 46f) / MyGuiConstants.GUI_OPTIMAL_SIZE), 0, colorMask);
                xOffset += radioButton.Size.X;
                MyGuiHighlightTexture texture = new MyGuiHighlightTexture {
                    Normal = classDef.Icons[0],
                    Highlight = classDef.HighlightIcon,
                    SizePx = new Vector2(46f, 46f)
                };
                radioButton.Icon = new MyGuiHighlightTexture?(texture);
                radioButton.UserData = classDef;
                radioButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                if (classDef.DisplayNameEnum != null)
                {
                    radioButton.SetToolTip(classDef.DisplayNameEnum.Value);
                }
                else
                {
                    radioButton.SetToolTip(classDef.DisplayNameString);
                }
                this.m_blueprintButtonGroup.Add(radioButton);
                this.m_controlsParent.Controls.Add(radioButton);
                radioButton.Selected = selected;
            }
        }

        private void AddComponentPrerequisites(MyBlueprintDefinitionBase blueprint, MyFixedPoint multiplier, Dictionary<MyDefinitionId, MyFixedPoint> outputAmounts)
        {
            MyFixedPoint point = (MyFixedPoint) (1f / ((this.m_selectedAssembler != null) ? this.m_selectedAssembler.GetEfficiencyMultiplierForBlueprint(blueprint) : MySession.Static.AssemblerEfficiencyMultiplier));
            foreach (MyBlueprintDefinitionBase.Item item in blueprint.Prerequisites)
            {
                if (!outputAmounts.ContainsKey(item.Id))
                {
                    outputAmounts[item.Id] = 0;
                }
                Dictionary<MyDefinitionId, MyFixedPoint> dictionary = outputAmounts;
                MyDefinitionId id = item.Id;
                dictionary[id] += (item.Amount * multiplier) * point;
            }
        }

        private void assembler_CurrentModeChanged(MyAssembler assembler)
        {
            this.SelectModeButton(assembler);
            this.RefreshRepeatMode(assembler.RepeatEnabled);
            this.RefreshSlaveMode(assembler.IsSlave);
            this.RefreshProgress();
            this.RefreshAssemblerModeView();
            this.RefreshMaterialsPreview();
        }

        private void assembler_CurrentProgressChanged(MyAssembler assembler)
        {
            this.RefreshProgress();
        }

        private void assembler_CurrentStateChanged(MyAssembler obj)
        {
            this.RefreshProgress();
        }

        private void assembler_CustomNameChanged(MyTerminalBlock block)
        {
            foreach (KeyValuePair<int, MyAssembler> pair in this.m_assemblersByKey)
            {
                if (pair.Value == block)
                {
                    this.m_comboboxAssemblers.TryGetItemByKey((long) pair.Key).Value.Clear().AppendStringBuilder(block.CustomName);
                }
            }
        }

        private void assembler_QueueChanged(MyProductionBlock block)
        {
            this.RefreshQueue();
            this.RefreshMaterialsPreview();
        }

        private void Assemblers_ItemSelected()
        {
            if ((this.m_assemblersByKey.Count > 0) && this.m_assemblersByKey.ContainsKey((int) this.m_comboboxAssemblers.GetSelectedKey()))
            {
                this.SelectAndShowAssembler(this.m_assemblersByKey[(int) this.m_comboboxAssemblers.GetSelectedKey()]);
            }
        }

        private void blueprintButtonGroup_SelectedChanged(MyGuiControlRadioButtonGroup obj)
        {
            this.RefreshBlueprints();
        }

        private void blueprintsGrid_ItemClicked(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            MyGuiGridItem itemAt = control.GetItemAt(args.ItemIndex);
            if (itemAt != null)
            {
                MyBlueprintDefinitionBase userData = (MyBlueprintDefinitionBase) itemAt.UserData;
                this.EnqueueBlueprint(userData, (MyInput.Static.IsAnyShiftKeyPressed() ? 100 : 1) * (MyInput.Static.IsAnyCtrlKeyPressed() ? 10 : 1));
            }
        }

        private void blueprintsGrid_MouseOverIndexChanged(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            this.RefreshMaterialsPreview();
        }

        internal void Close()
        {
            this.UnregisterEvents();
            this.UnregisterAssemblerEvents(this.m_selectedAssembler);
            this.m_assemblersByKey.Clear();
            this.m_blueprintButtonGroup.Clear();
            this.m_modeButtonGroup.Clear();
            this.m_selectedAssembler = null;
            this.m_controlsParent = null;
            this.m_terminalSystem = null;
            this.m_comboboxAssemblers = null;
            this.m_dragAndDrop = null;
            this.m_dragAndDropInfo = null;
        }

        private void controlPanelButton_ButtonClicked(MyGuiControlButton control)
        {
            MyGuiScreenTerminal.SwitchToControlPanelBlock(this.m_selectedAssembler);
        }

        private void disassembleAllButton_ButtonClicked(MyGuiControlButton obj)
        {
            if ((this.CurrentAssemblerMode == AssemblerMode.Disassembling) && !this.m_selectedAssembler.RepeatEnabled)
            {
                this.m_selectedAssembler.RequestDisassembleAll();
            }
        }

        private void dragDrop_OnItemDropped(object sender, MyDragAndDropEventArgs eventArgs)
        {
            if ((this.m_selectedAssembler != null) && (eventArgs.DropTo != null))
            {
                MyProductionBlock.QueueItem userData = (MyProductionBlock.QueueItem) eventArgs.Item.UserData;
                this.m_selectedAssembler.MoveQueueItemRequest(userData.ItemId, eventArgs.DropTo.ItemIndex);
            }
            this.m_dragAndDropInfo = null;
        }

        private void EnqueueBlueprint(MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            m_blueprintCache.Clear();
            blueprint.GetBlueprints(m_blueprintCache);
            foreach (MyBlueprintDefinitionBase.ProductionInfo info in m_blueprintCache)
            {
                this.m_selectedAssembler.InsertQueueItemRequest(-1, info.Blueprint, info.Amount * amount);
            }
            m_blueprintCache.Clear();
        }

        private void FillMaterialList(Dictionary<MyDefinitionId, MyFixedPoint> materials)
        {
            bool flag = this.CurrentAssemblerMode == AssemblerMode.Disassembling;
            foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> pair in materials)
            {
                string text1;
                MyFixedPoint point = this.m_selectedAssembler.InventoryAggregate.GetItemAmount(pair.Key, MyItemFlags.None, false);
                if (flag || (pair.Value <= point))
                {
                    text1 = "White";
                }
                else
                {
                    text1 = "Red";
                }
                string font = text1;
                this.m_materialsList.Add(pair.Key, (double) pair.Value, (double) point, font);
            }
        }

        private static string GetAssemblerStateText(MyAssembler.StateEnum state)
        {
            MyStringId blank = MySpaceTexts.Blank;
            switch (state)
            {
                case MyAssembler.StateEnum.Ok:
                    blank = MySpaceTexts.Blank;
                    break;

                case MyAssembler.StateEnum.Disabled:
                    blank = MySpaceTexts.AssemblerState_Disabled;
                    break;

                case MyAssembler.StateEnum.NotWorking:
                    blank = MySpaceTexts.AssemblerState_NotWorking;
                    break;

                case MyAssembler.StateEnum.NotEnoughPower:
                    blank = MySpaceTexts.AssemblerState_NotEnoughPower;
                    break;

                case MyAssembler.StateEnum.MissingItems:
                    blank = MySpaceTexts.AssemblerState_MissingItems;
                    break;

                case MyAssembler.StateEnum.InventoryFull:
                    blank = MySpaceTexts.AssemblerState_InventoryFull;
                    break;

                default:
                    break;
            }
            return MyTexts.GetString(blank);
        }

        private static void HideError(IMyGuiControlsParent controlsParent)
        {
            controlsParent.Controls.RemoveControlByName("ErrorMessage");
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = controlsParent.Controls.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = true;
                }
            }
        }

        public void Init(IMyGuiControlsParent controlsParent, MyCubeGrid grid, MyCubeBlock currentBlock)
        {
            if (grid == null)
            {
                ShowError(MySpaceTexts.ScreenTerminalError_ShipNotConnected, controlsParent);
            }
            else
            {
                Func<KeyValuePair<int, MyAssembler>, int> <>9__0;
                grid.OnTerinalOpened();
                this.m_assemblerKeyCounter = 0;
                this.m_assemblersByKey.Clear();
                foreach (MyAssembler assembler in grid.GridSystems.TerminalSystem.Blocks)
                {
                    if (assembler == null)
                    {
                        continue;
                    }
                    if (assembler.HasLocalPlayerAccess())
                    {
                        int assemblerKeyCounter = this.m_assemblerKeyCounter;
                        this.m_assemblerKeyCounter = assemblerKeyCounter + 1;
                        this.m_assemblersByKey.Add(assemblerKeyCounter, assembler);
                    }
                }
                this.m_controlsParent = controlsParent;
                this.m_terminalSystem = grid.GridSystems.TerminalSystem;
                this.m_blueprintsArea = (MyGuiControlScrollablePanel) controlsParent.Controls.GetControlByName("BlueprintsScrollableArea");
                this.m_blueprintsSearchBox = (MyGuiControlSearchBox) controlsParent.Controls.GetControlByName("BlueprintsSearchBox");
                this.m_queueArea = (MyGuiControlScrollablePanel) controlsParent.Controls.GetControlByName("QueueScrollableArea");
                this.m_inventoryArea = (MyGuiControlScrollablePanel) controlsParent.Controls.GetControlByName("InventoryScrollableArea");
                this.m_blueprintsBgPanel = controlsParent.Controls.GetControlByName("BlueprintsBackgroundPanel");
                this.m_blueprintsLabel = controlsParent.Controls.GetControlByName("BlueprintsLabel");
                this.m_comboboxAssemblers = (MyGuiControlCombobox) controlsParent.Controls.GetControlByName("AssemblersCombobox");
                this.m_blueprintsGrid = (MyGuiControlGrid) this.m_blueprintsArea.ScrolledControl;
                this.m_queueGrid = (MyGuiControlGrid) this.m_queueArea.ScrolledControl;
                this.m_inventoryGrid = (MyGuiControlGrid) this.m_inventoryArea.ScrolledControl;
                this.m_materialsList = (MyGuiControlComponentList) controlsParent.Controls.GetControlByName("MaterialsList");
                this.m_repeatCheckbox = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("RepeatCheckbox");
                this.m_slaveCheckbox = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("SlaveCheckbox");
                this.m_disassembleAllButton = (MyGuiControlButton) controlsParent.Controls.GetControlByName("DisassembleAllButton");
                this.m_controlPanelButton = (MyGuiControlButton) controlsParent.Controls.GetControlByName("ControlPanelButton");
                this.m_inventoryButton = (MyGuiControlButton) controlsParent.Controls.GetControlByName("InventoryButton");
                this.m_materialsLabel = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("RequiredLabel");
                this.m_controlPanelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ProductionScreen_TerminalControlScreen));
                this.m_inventoryButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ProductionScreen_TerminalInventoryScreen));
                MyGuiControlRadioButton controlByName = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("AssemblingButton");
                controlByName.VisualStyle = MyGuiControlRadioButtonStyleEnum.TerminalAssembler;
                MyGuiControlRadioButton radioButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("DisassemblingButton");
                radioButton.VisualStyle = MyGuiControlRadioButtonStyleEnum.TerminalAssembler;
                controlByName.Key = 0;
                radioButton.Key = 1;
                this.m_modeButtonGroup.Add(controlByName);
                this.m_modeButtonGroup.Add(radioButton);
                Func<KeyValuePair<int, MyAssembler>, int> keySelector = <>9__0;
                if (<>9__0 == null)
                {
                    Func<KeyValuePair<int, MyAssembler>, int> local1 = <>9__0;
                    keySelector = <>9__0 = delegate (KeyValuePair<int, MyAssembler> x) {
                        MyAssembler objA = x.Value;
                        return !ReferenceEquals(objA, currentBlock) ? ((objA.IsFunctional ? 0 : 0x2710) + objA.GUIPriority) : -1;
                    };
                }
                foreach (KeyValuePair<int, MyAssembler> pair in this.m_assemblersByKey.OrderBy<KeyValuePair<int, MyAssembler>, int>(keySelector))
                {
                    int? nullable;
                    MyAssembler assembler2 = pair.Value;
                    if (assembler2.IsFunctional)
                    {
                        nullable = null;
                        this.m_comboboxAssemblers.AddItem((long) pair.Key, assembler2.CustomName, nullable, null);
                        continue;
                    }
                    this.m_incompleteAssemblerName.Clear();
                    this.m_incompleteAssemblerName.AppendStringBuilder(assembler2.CustomName);
                    this.m_incompleteAssemblerName.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Terminal_BlockIncomplete));
                    nullable = null;
                    this.m_comboboxAssemblers.AddItem((long) pair.Key, this.m_incompleteAssemblerName, nullable, null);
                }
                this.m_comboboxAssemblers.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.Assemblers_ItemSelected);
                this.m_comboboxAssemblers.SetToolTip(MyTexts.GetString(MySpaceTexts.ProductionScreen_AssemblerList));
                this.m_comboboxAssemblers.SelectItemByIndex(0);
                this.m_dragAndDrop = new MyGuiControlGridDragAndDrop(MyGuiConstants.DRAG_AND_DROP_BACKGROUND_COLOR, MyGuiConstants.DRAG_AND_DROP_TEXT_COLOR, 0.7f, MyGuiConstants.DRAG_AND_DROP_TEXT_OFFSET, true);
                controlsParent.Controls.Add(this.m_dragAndDrop);
                this.m_dragAndDrop.DrawBackgroundTexture = false;
                this.m_dragAndDrop.ItemDropped += new OnItemDropped(this.dragDrop_OnItemDropped);
                this.RefreshBlueprints();
                this.Assemblers_ItemSelected();
                this.RegisterEvents();
                if (this.m_assemblersByKey.Count == 0)
                {
                    ShowError(MySpaceTexts.ScreenTerminalError_NoAssemblers, controlsParent);
                }
            }
        }

        private void InputInventory_ContentsChanged(MyInventoryBase obj)
        {
            if (this.CurrentAssemblerMode == AssemblerMode.Assembling)
            {
                this.RefreshBlueprintGridColors();
            }
            this.RefreshMaterialsPreview();
        }

        private void inventoryButton_ButtonClicked(MyGuiControlButton control)
        {
            MyGuiScreenTerminal.SwitchToInventory(this.m_selectedAssembler);
        }

        private void inventoryGrid_ItemClicked(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            if (this.CurrentAssemblerMode != AssemblerMode.Assembling)
            {
                MyGuiGridItem itemAt = control.GetItemAt(args.ItemIndex);
                if (itemAt != null)
                {
                    MyPhysicalInventoryItem userData = (MyPhysicalInventoryItem) itemAt.UserData;
                    MyBlueprintDefinitionBase blueprint = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(userData.Content.GetId());
                    if (blueprint != null)
                    {
                        this.EnqueueBlueprint(blueprint, (MyInput.Static.IsAnyShiftKeyPressed() ? 100 : 1) * (MyInput.Static.IsAnyCtrlKeyPressed() ? 10 : 1));
                    }
                }
            }
        }

        private void inventoryGrid_MouseOverIndexChanged(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            if (this.CurrentAssemblerMode != AssemblerMode.Assembling)
            {
                this.RefreshMaterialsPreview();
            }
        }

        private void modeButtonGroup_SelectedChanged(MyGuiControlRadioButtonGroup obj)
        {
            this.m_selectedAssembler.CurrentModeChanged -= new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            bool newDisassembleEnabled = obj.SelectedButton.Key == 1;
            this.m_selectedAssembler.RequestDisassembleEnabled(newDisassembleEnabled);
            if (newDisassembleEnabled)
            {
                this.m_slaveCheckbox.Enabled = false;
                this.m_slaveCheckbox.Visible = false;
            }
            if (!newDisassembleEnabled && this.m_selectedAssembler.SupportsAdvancedFunctions)
            {
                this.m_slaveCheckbox.Enabled = true;
                this.m_slaveCheckbox.Visible = true;
            }
            this.m_selectedAssembler.CurrentModeChanged += new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            this.m_repeatCheckbox.IsCheckedChanged = null;
            this.m_repeatCheckbox.IsChecked = this.m_selectedAssembler.RepeatEnabled;
            this.m_repeatCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.repeatCheckbox_IsCheckedChanged);
            this.m_slaveCheckbox.IsCheckedChanged = null;
            this.m_slaveCheckbox.IsChecked = this.m_selectedAssembler.IsSlave;
            this.m_slaveCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.slaveCheckbox_IsCheckedChanged);
            this.RefreshProgress();
            this.RefreshAssemblerModeView();
        }

        private void OnSearchTextChanged(string text)
        {
            this.RefreshBlueprints();
        }

        private void OutputInventory_ContentsChanged(MyInventoryBase obj)
        {
            this.RefreshInventory();
            this.RefreshMaterialsPreview();
        }

        private void queueGrid_ItemClicked(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            if (((this.CurrentAssemblerMode != AssemblerMode.Disassembling) || !this.m_selectedAssembler.RepeatEnabled) && (args.Button == MySharedButtonsEnum.Secondary))
            {
                this.m_selectedAssembler.RemoveQueueItemRequest(args.ItemIndex, -1, 0f);
            }
        }

        private void queueGrid_ItemDragged(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            this.StartDragging(MyDropHandleType.MouseRelease, control, ref args);
        }

        private void queueGrid_MouseOverIndexChanged(MyGuiControlGrid control, MyGuiControlGrid.EventArgs args)
        {
            this.RefreshMaterialsPreview();
        }

        private void RefreshAssemblerModeView()
        {
            bool flag = this.CurrentAssemblerMode == AssemblerMode.Assembling;
            bool repeatEnabled = this.m_selectedAssembler.RepeatEnabled;
            this.m_blueprintsArea.Enabled = true;
            this.m_blueprintsBgPanel.Enabled = true;
            this.m_blueprintsLabel.Enabled = true;
            using (IEnumerator<MyGuiControlRadioButton> enumerator = this.m_blueprintButtonGroup.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = true;
                }
            }
            this.m_materialsLabel.Text = flag ? MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_RequiredAndAvailable) : MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_GainedAndAvailable);
            this.m_queueGrid.Enabled = flag || !repeatEnabled;
            this.m_disassembleAllButton.Visible = !flag && !repeatEnabled;
            this.RefreshBlueprintGridColors();
        }

        private void RefreshBlueprintGridColors()
        {
            int num2;
            this.m_selectedAssembler.InventoryOwnersDirty = true;
            int rowIdx = 0;
            goto TR_001C;
        TR_0002:
            num2++;
        TR_0019:
            while (true)
            {
                if (num2 < this.m_blueprintsGrid.ColumnsCount)
                {
                    MyGuiGridItem item = this.m_blueprintsGrid.TryGetItemAt(rowIdx, num2);
                    if (item == null)
                    {
                        goto TR_0002;
                    }
                    else
                    {
                        MyBlueprintDefinitionBase userData = item.UserData as MyBlueprintDefinitionBase;
                        if (userData == null)
                        {
                            goto TR_0002;
                        }
                        else
                        {
                            item.IconColorMask = Vector4.One;
                            if (this.m_selectedAssembler == null)
                            {
                                goto TR_0002;
                            }
                            else
                            {
                                this.AddComponentPrerequisites(userData, 1, m_requiredCountCache);
                                if (this.CurrentAssemblerMode == AssemblerMode.Assembling)
                                {
                                    foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> pair in m_requiredCountCache)
                                    {
                                        if (!this.m_selectedAssembler.CheckConveyorResources(new MyFixedPoint?(pair.Value), pair.Key))
                                        {
                                            item.IconColorMask = ERROR_ICON_COLOR_MASK;
                                            break;
                                        }
                                    }
                                }
                                else if (this.CurrentAssemblerMode == AssemblerMode.Disassembling)
                                {
                                    MyFixedPoint? amount = null;
                                    if (!this.m_selectedAssembler.CheckConveyorResources(amount, userData.Results[0].Id))
                                    {
                                        item.IconColorMask = ERROR_ICON_COLOR_MASK;
                                    }
                                }
                            }
                        }
                    }
                    m_requiredCountCache.Clear();
                    goto TR_0002;
                }
                else
                {
                    rowIdx++;
                }
                break;
            }
        TR_001C:
            while (true)
            {
                if (rowIdx >= this.m_blueprintsGrid.RowsCount)
                {
                    return;
                }
                num2 = 0;
                break;
            }
            goto TR_0019;
        }

        private void RefreshBlueprints()
        {
            if (this.m_blueprintButtonGroup.SelectedButton != null)
            {
                MyBlueprintClassDefinition userData = this.m_blueprintButtonGroup.SelectedButton.UserData as MyBlueprintClassDefinition;
                if (userData != null)
                {
                    this.m_blueprintsGrid.Clear();
                    bool flag = !string.IsNullOrEmpty(this.m_blueprintsSearchBox.SearchText);
                    int num = 0;
                    foreach (MyBlueprintDefinitionBase base2 in userData)
                    {
                        if (!base2.Public)
                        {
                            continue;
                        }
                        if (!flag || base2.DisplayNameText.Contains(this.m_blueprintsSearchBox.SearchText, StringComparison.OrdinalIgnoreCase))
                        {
                            MyGuiGridItem item = new MyGuiGridItem(base2.Icons, null, base2.DisplayNameText, base2, true);
                            this.m_blueprintsGrid.Add(item, 0);
                            num++;
                        }
                    }
                    this.m_blueprintsGrid.RowsCount = Math.Max(1 + (num / this.m_blueprintsGrid.ColumnsCount), BLUEPRINT_GRID_ROWS);
                    this.RefreshBlueprintGridColors();
                }
            }
        }

        private void RefreshInventory()
        {
            this.m_inventoryGrid.Clear();
            foreach (MyPhysicalInventoryItem item in this.m_selectedAssembler.OutputInventory.GetItems())
            {
                this.m_inventoryGrid.Add(MyGuiControlInventoryOwner.CreateInventoryGridItem(item), 0);
            }
            int count = this.m_selectedAssembler.OutputInventory.GetItems().Count;
            this.m_inventoryGrid.RowsCount = Math.Max(1 + (count / this.m_inventoryGrid.ColumnsCount), INVENTORY_GRID_ROWS);
        }

        private void RefreshMaterialsPreview()
        {
            this.m_materialsList.Clear();
            if (this.m_blueprintsGrid.MouseOverItem != null)
            {
                this.ShowBlueprintComponents((MyBlueprintDefinitionBase) this.m_blueprintsGrid.MouseOverItem.UserData, 1);
            }
            else if ((this.m_inventoryGrid.MouseOverItem != null) && (this.CurrentAssemblerMode == AssemblerMode.Disassembling))
            {
                MyPhysicalInventoryItem userData = (MyPhysicalInventoryItem) this.m_inventoryGrid.MouseOverItem.UserData;
                if (MyDefinitionManager.Static.HasBlueprint(userData.Content.GetId()))
                {
                    this.ShowBlueprintComponents(MyDefinitionManager.Static.GetBlueprintDefinition(userData.Content.GetId()), 1);
                }
            }
            else if (this.m_queueGrid.MouseOverItem != null)
            {
                MyProductionBlock.QueueItem userData = (MyProductionBlock.QueueItem) this.m_queueGrid.MouseOverItem.UserData;
                this.ShowBlueprintComponents(userData.Blueprint, userData.Amount);
            }
            else if (this.m_selectedAssembler != null)
            {
                foreach (MyProductionBlock.QueueItem item3 in this.m_selectedAssembler.Queue)
                {
                    this.AddComponentPrerequisites(item3.Blueprint, item3.Amount, m_requiredCountCache);
                }
                this.FillMaterialList(m_requiredCountCache);
            }
            m_requiredCountCache.Clear();
        }

        private void RefreshProgress()
        {
            int currentItemIndex = this.m_selectedAssembler.CurrentItemIndex;
            MyGuiGridItem item = this.m_queueGrid.TryGetItemAt(currentItemIndex);
            if (item != null)
            {
                MyProductionBlock.QueueItem userData = (MyProductionBlock.QueueItem) item.UserData;
                item.OverlayPercent = MathHelper.Clamp(this.m_selectedAssembler.CurrentProgress, 0f, 1f);
                item.ToolTip.ToolTips.Clear();
                m_textCache.Clear().AppendFormat("{0}: {1}%", userData.Blueprint.DisplayNameText, (int) (this.m_selectedAssembler.CurrentProgress * 100f));
                item.ToolTip.AddToolTip(m_textCache.ToString(), 0.7f, "Blue");
            }
            int itemIdx = 0;
            while (true)
            {
                if (itemIdx < this.m_queueGrid.GetItemsCount())
                {
                    item = this.m_queueGrid.TryGetItemAt(itemIdx);
                    if (item != null)
                    {
                        if (itemIdx < currentItemIndex)
                        {
                            item.IconColorMask = ERROR_ICON_COLOR_MASK;
                            item.OverlayColorMask = Color.Red.ToVector4();
                            item.ToolTip.ToolTips.Clear();
                            item.ToolTip.AddToolTip(GetAssemblerStateText(MyAssembler.StateEnum.MissingItems), 0.7f, "Red");
                        }
                        else
                        {
                            Vector4 vector1;
                            Vector4 vector2;
                            if (this.m_selectedAssembler.CurrentState != MyAssembler.StateEnum.Ok)
                            {
                                vector1 = ERROR_ICON_COLOR_MASK;
                            }
                            else
                            {
                                vector1 = Color.White.ToVector4();
                            }
                            item.IconColorMask = vector1;
                            if (this.m_selectedAssembler.CurrentState != MyAssembler.StateEnum.Ok)
                            {
                                vector2 = Color.Red.ToVector4();
                            }
                            else
                            {
                                vector2 = Color.White.ToVector4();
                            }
                            item.OverlayColorMask = vector2;
                            if (itemIdx != currentItemIndex)
                            {
                                item.ToolTip.ToolTips.Clear();
                            }
                            if (this.m_selectedAssembler.CurrentState != MyAssembler.StateEnum.Ok)
                            {
                                item.ToolTip.AddToolTip(GetAssemblerStateText(this.m_selectedAssembler.CurrentState), 0.7f, (this.m_selectedAssembler.CurrentState == MyAssembler.StateEnum.Ok) ? "White" : "Red");
                            }
                        }
                        itemIdx++;
                        continue;
                    }
                }
                return;
            }
        }

        private void RefreshQueue()
        {
            this.m_queueGrid.Clear();
            int num = 0;
            foreach (MyProductionBlock.QueueItem item in this.m_selectedAssembler.Queue)
            {
                m_textCache.Clear().Append((int) item.Amount).Append('x');
                MyGuiGridItem item2 = new MyGuiGridItem(item.Blueprint.Icons, null, item.Blueprint.DisplayNameText, item, true);
                item2.AddText(m_textCache, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                if (MyFakes.SHOW_PRODUCTION_QUEUE_ITEM_IDS)
                {
                    m_textCache.Clear().Append((int) item.ItemId);
                    item2.AddText(m_textCache, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                }
                this.m_queueGrid.Add(item2, 0);
                num++;
            }
            this.m_queueGrid.RowsCount = Math.Max(1 + (num / this.m_queueGrid.ColumnsCount), QUEUE_GRID_ROWS);
            this.RefreshProgress();
        }

        private void RefreshRepeatMode(bool repeatModeEnabled)
        {
            if (this.m_selectedAssembler.IsSlave & repeatModeEnabled)
            {
                this.RefreshSlaveMode(false);
            }
            this.m_selectedAssembler.CurrentModeChanged -= new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            this.m_selectedAssembler.RequestRepeatEnabled(repeatModeEnabled);
            this.m_selectedAssembler.CurrentModeChanged += new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            this.m_repeatCheckbox.IsCheckedChanged = null;
            this.m_repeatCheckbox.IsChecked = this.m_selectedAssembler.RepeatEnabled;
            this.m_repeatCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.repeatCheckbox_IsCheckedChanged);
            this.m_repeatCheckbox.Visible = this.m_selectedAssembler.SupportsAdvancedFunctions;
        }

        private void RefreshSlaveMode(bool slaveModeEnabled)
        {
            if (this.m_selectedAssembler.RepeatEnabled & slaveModeEnabled)
            {
                this.RefreshRepeatMode(false);
            }
            if (this.m_selectedAssembler.DisassembleEnabled)
            {
                this.m_slaveCheckbox.Enabled = false;
                this.m_slaveCheckbox.Visible = false;
            }
            if (!this.m_selectedAssembler.DisassembleEnabled)
            {
                this.m_slaveCheckbox.Enabled = true;
                this.m_slaveCheckbox.Visible = true;
            }
            this.m_selectedAssembler.CurrentModeChanged -= new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            this.m_selectedAssembler.IsSlave = slaveModeEnabled;
            this.m_selectedAssembler.CurrentModeChanged += new Action<MyAssembler>(this.assembler_CurrentModeChanged);
            this.m_slaveCheckbox.IsCheckedChanged = null;
            this.m_slaveCheckbox.IsChecked = this.m_selectedAssembler.IsSlave;
            this.m_slaveCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.slaveCheckbox_IsCheckedChanged);
            if (!this.m_selectedAssembler.SupportsAdvancedFunctions)
            {
                this.m_slaveCheckbox.Visible = false;
            }
        }

        private void RegisterAssemblerEvents(MyAssembler assembler)
        {
            if (assembler != null)
            {
                assembler.CurrentModeChanged += new Action<MyAssembler>(this.assembler_CurrentModeChanged);
                assembler.QueueChanged += new Action<MyProductionBlock>(this.assembler_QueueChanged);
                assembler.CurrentProgressChanged += new Action<MyAssembler>(this.assembler_CurrentProgressChanged);
                assembler.CurrentStateChanged += new Action<MyAssembler>(this.assembler_CurrentStateChanged);
                assembler.InputInventory.ContentsChanged += new Action<MyInventoryBase>(this.InputInventory_ContentsChanged);
                assembler.OutputInventory.ContentsChanged += new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
            }
        }

        private void RegisterEvents()
        {
            foreach (KeyValuePair<int, MyAssembler> pair in this.m_assemblersByKey)
            {
                pair.Value.CustomNameChanged += new Action<MyTerminalBlock>(this.assembler_CustomNameChanged);
            }
            this.m_terminalSystem.BlockAdded += new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
            this.m_terminalSystem.BlockRemoved += new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
            this.m_blueprintButtonGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.blueprintButtonGroup_SelectedChanged);
            this.m_modeButtonGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.modeButtonGroup_SelectedChanged);
            this.m_blueprintsSearchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChanged);
            this.m_blueprintsGrid.ItemClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.blueprintsGrid_ItemClicked);
            this.m_blueprintsGrid.MouseOverIndexChanged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.blueprintsGrid_MouseOverIndexChanged);
            this.m_inventoryGrid.ItemClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.inventoryGrid_ItemClicked);
            this.m_inventoryGrid.MouseOverIndexChanged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.inventoryGrid_MouseOverIndexChanged);
            this.m_repeatCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.repeatCheckbox_IsCheckedChanged);
            this.m_slaveCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.slaveCheckbox_IsCheckedChanged);
            this.m_queueGrid.ItemClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_ItemClicked);
            this.m_queueGrid.ItemDragged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_ItemDragged);
            this.m_queueGrid.MouseOverIndexChanged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_MouseOverIndexChanged);
            this.m_controlPanelButton.ButtonClicked += new Action<MyGuiControlButton>(this.controlPanelButton_ButtonClicked);
            this.m_inventoryButton.ButtonClicked += new Action<MyGuiControlButton>(this.inventoryButton_ButtonClicked);
            this.m_disassembleAllButton.ButtonClicked += new Action<MyGuiControlButton>(this.disassembleAllButton_ButtonClicked);
        }

        private void repeatCheckbox_IsCheckedChanged(MyGuiControlCheckbox control)
        {
            this.RefreshRepeatMode(control.IsChecked);
            this.RefreshAssemblerModeView();
        }

        private void SelectAndShowAssembler(MyAssembler assembler)
        {
            this.UnregisterAssemblerEvents(this.m_selectedAssembler);
            this.m_selectedAssembler = assembler;
            this.RegisterAssemblerEvents(assembler);
            this.RefreshRepeatMode(assembler.RepeatEnabled);
            this.RefreshSlaveMode(assembler.IsSlave);
            this.SelectModeButton(assembler);
            this.UpdateBlueprintClassGui();
            this.m_blueprintsSearchBox.SearchText = string.Empty;
            this.RefreshQueue();
            this.RefreshInventory();
            this.RefreshProgress();
            this.RefreshAssemblerModeView();
        }

        private void SelectModeButton(MyAssembler assembler)
        {
            bool supportsAdvancedFunctions = assembler.SupportsAdvancedFunctions;
            using (IEnumerator<MyGuiControlRadioButton> enumerator = this.m_modeButtonGroup.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = supportsAdvancedFunctions;
                }
            }
            AssemblerMode mode = assembler.DisassembleEnabled ? AssemblerMode.Disassembling : AssemblerMode.Assembling;
            this.m_modeButtonGroup.SelectByKey((int) mode);
        }

        private void ShowBlueprintComponents(MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            this.m_materialsList.Clear();
            if (blueprint != null)
            {
                this.AddComponentPrerequisites(blueprint, amount, m_requiredCountCache);
                this.FillMaterialList(m_requiredCountCache);
                m_requiredCountCache.Clear();
            }
        }

        private static void ShowError(MyStringId errorText, IMyGuiControlsParent controlsParent)
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = controlsParent.Controls.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = false;
                }
            }
            MyGuiControlLabel controlByName = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("ErrorMessage");
            if (controlByName == null)
            {
                controlByName = MyGuiScreenTerminal.CreateErrorLabel(errorText, "ErrorMessage");
            }
            controlByName.TextEnum = errorText;
            if (!controlsParent.Controls.Contains(controlByName))
            {
                controlsParent.Controls.Add(controlByName);
            }
        }

        private void slaveCheckbox_IsCheckedChanged(MyGuiControlCheckbox control)
        {
            this.RefreshSlaveMode(control.IsChecked);
            this.RefreshAssemblerModeView();
        }

        private void StartDragging(MyDropHandleType dropHandlingType, MyGuiControlGrid gridControl, ref MyGuiControlGrid.EventArgs args)
        {
            this.m_dragAndDropInfo = new MyDragAndDropInfo();
            this.m_dragAndDropInfo.Grid = gridControl;
            this.m_dragAndDropInfo.ItemIndex = args.ItemIndex;
            MyGuiGridItem itemAt = this.m_dragAndDropInfo.Grid.GetItemAt(this.m_dragAndDropInfo.ItemIndex);
            this.m_dragAndDrop.StartDragging(dropHandlingType, args.Button, itemAt, this.m_dragAndDropInfo, false);
        }

        private void TerminalSystem_BlockAdded(MyTerminalBlock obj)
        {
            MyAssembler assembler = obj as MyAssembler;
            if (assembler != null)
            {
                if (this.m_assemblersByKey.Count == 0)
                {
                    HideError(this.m_controlsParent);
                }
                int assemblerKeyCounter = this.m_assemblerKeyCounter;
                this.m_assemblerKeyCounter = assemblerKeyCounter + 1;
                int key = assemblerKeyCounter;
                this.m_assemblersByKey.Add(key, assembler);
                int? sortOrder = null;
                this.m_comboboxAssemblers.AddItem((long) key, assembler.CustomName, sortOrder, null);
                if (this.m_assemblersByKey.Count == 1)
                {
                    this.m_comboboxAssemblers.SelectItemByIndex(0);
                }
                assembler.CustomNameChanged += new Action<MyTerminalBlock>(this.assembler_CustomNameChanged);
            }
        }

        private void TerminalSystem_BlockRemoved(MyTerminalBlock obj)
        {
            MyAssembler objA = obj as MyAssembler;
            if (objA != null)
            {
                objA.CustomNameChanged -= new Action<MyTerminalBlock>(this.assembler_CustomNameChanged);
                int? nullable = null;
                foreach (KeyValuePair<int, MyAssembler> pair in this.m_assemblersByKey)
                {
                    if (pair.Value == objA)
                    {
                        nullable = new int?(pair.Key);
                        break;
                    }
                }
                if (nullable != null)
                {
                    this.m_assemblersByKey.Remove(nullable.Value);
                    this.m_comboboxAssemblers.RemoveItem((long) nullable.Value);
                }
                if (ReferenceEquals(objA, this.m_selectedAssembler))
                {
                    if (this.m_assemblersByKey.Count > 0)
                    {
                        this.m_comboboxAssemblers.SelectItemByIndex(0);
                    }
                    else
                    {
                        ShowError(MySpaceTexts.ScreenTerminalError_NoAssemblers, this.m_controlsParent);
                    }
                }
            }
        }

        private void UnregisterAssemblerEvents(MyAssembler assembler)
        {
            if (assembler != null)
            {
                this.m_selectedAssembler.CurrentModeChanged -= new Action<MyAssembler>(this.assembler_CurrentModeChanged);
                this.m_selectedAssembler.QueueChanged -= new Action<MyProductionBlock>(this.assembler_QueueChanged);
                this.m_selectedAssembler.CurrentProgressChanged -= new Action<MyAssembler>(this.assembler_CurrentProgressChanged);
                this.m_selectedAssembler.CurrentStateChanged -= new Action<MyAssembler>(this.assembler_CurrentStateChanged);
                if (assembler.InputInventory != null)
                {
                    assembler.InputInventory.ContentsChanged -= new Action<MyInventoryBase>(this.InputInventory_ContentsChanged);
                }
                if (this.m_selectedAssembler.OutputInventory != null)
                {
                    this.m_selectedAssembler.OutputInventory.ContentsChanged -= new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
                }
            }
        }

        private void UnregisterEvents()
        {
            if (this.m_controlsParent != null)
            {
                foreach (KeyValuePair<int, MyAssembler> pair in this.m_assemblersByKey)
                {
                    pair.Value.CustomNameChanged -= new Action<MyTerminalBlock>(this.assembler_CustomNameChanged);
                }
                if (this.m_terminalSystem != null)
                {
                    this.m_terminalSystem.BlockAdded -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockAdded);
                    this.m_terminalSystem.BlockRemoved -= new Action<MyTerminalBlock>(this.TerminalSystem_BlockRemoved);
                }
                this.m_blueprintButtonGroup.SelectedChanged -= new Action<MyGuiControlRadioButtonGroup>(this.blueprintButtonGroup_SelectedChanged);
                this.m_modeButtonGroup.SelectedChanged -= new Action<MyGuiControlRadioButtonGroup>(this.modeButtonGroup_SelectedChanged);
                this.m_blueprintsSearchBox.OnTextChanged -= new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChanged);
                this.m_blueprintsGrid.ItemClicked -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.blueprintsGrid_ItemClicked);
                this.m_blueprintsGrid.MouseOverIndexChanged -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.blueprintsGrid_MouseOverIndexChanged);
                this.m_inventoryGrid.ItemClicked -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.inventoryGrid_ItemClicked);
                this.m_inventoryGrid.MouseOverIndexChanged -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.inventoryGrid_MouseOverIndexChanged);
                this.m_repeatCheckbox.IsCheckedChanged = null;
                this.m_slaveCheckbox.IsCheckedChanged = null;
                this.m_queueGrid.ItemClicked -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_ItemClicked);
                this.m_queueGrid.ItemDragged -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_ItemDragged);
                this.m_queueGrid.MouseOverIndexChanged -= new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.queueGrid_MouseOverIndexChanged);
                this.m_controlPanelButton.ButtonClicked -= new Action<MyGuiControlButton>(this.controlPanelButton_ButtonClicked);
                this.m_inventoryButton.ButtonClicked -= new Action<MyGuiControlButton>(this.inventoryButton_ButtonClicked);
                this.m_disassembleAllButton.ButtonClicked -= new Action<MyGuiControlButton>(this.disassembleAllButton_ButtonClicked);
            }
        }

        private void UpdateBlueprintClassGui()
        {
            foreach (MyGuiControlRadioButton button in this.m_blueprintButtonGroup)
            {
                this.m_controlsParent.Controls.Remove(button);
            }
            this.m_blueprintButtonGroup.Clear();
            float xOffset = 0f;
            if (this.m_selectedAssembler.BlockDefinition is MyProductionBlockDefinition)
            {
                List<MyBlueprintClassDefinition> blueprintClasses = (this.m_selectedAssembler.BlockDefinition as MyProductionBlockDefinition).BlueprintClasses;
                for (int i = 0; i < blueprintClasses.Count; i++)
                {
                    this.AddBlueprintClassButton(blueprintClasses[i], ref xOffset, (i == 0) || (blueprintClasses[i].Id.SubtypeName == "Components"));
                }
            }
        }

        private AssemblerMode CurrentAssemblerMode =>
            ((AssemblerMode) this.m_modeButtonGroup.SelectedButton.Key);

        private enum AssemblerMode
        {
            Assembling,
            Disassembling
        }
    }
}


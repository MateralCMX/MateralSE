namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlToolbar : MyGuiControlBase
    {
        protected static StringBuilder m_textCache = new StringBuilder();
        protected MyGuiControlGrid m_toolbarItemsGrid;
        protected MyGuiControlLabel m_selectedItemLabel;
        protected MyGuiControlPanel m_colorVariantPanel;
        protected MyGuiControlContextMenu m_contextMenu;
        protected List<MyGuiControlLabel> m_pageLabelList;
        protected MyToolbar m_shownToolbar;
        protected MyObjectBuilder_ToolbarControlVisualStyle m_style;
        protected MyObjectBuilder_GuiTexture m_itemVarianTtexture;
        protected List<MyStatControls> m_statControls;
        protected MyGuiCompositeTexture m_pageCompositeTexture;
        protected MyGuiCompositeTexture m_pageHighlightCompositeTexture;
        private bool m_gridSizeLargeBlock;
        protected int m_contextMenuItemIndex;
        public bool UseContextMenu;

        public MyGuiControlToolbar(MyObjectBuilder_ToolbarControlVisualStyle toolbarStyle) : base(nullable, nullable, nullable2, null, null, true, false, true, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_pageLabelList = new List<MyGuiControlLabel>();
            this.m_statControls = new List<MyStatControls>();
            this.m_gridSizeLargeBlock = true;
            this.m_contextMenuItemIndex = -1;
            this.UseContextMenu = true;
            Vector2? nullable = null;
            nullable = null;
            MyToolbarComponent.CurrentToolbarChanged += new Action(this.ToolbarComponent_CurrentToolbarChanged);
            this.m_style = toolbarStyle;
            this.RecreateControls(true);
            this.ShowToolbar(MyToolbarComponent.CurrentToolbar);
        }

        private void contextMenu_ItemClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs args)
        {
            int itemIndex = args.ItemIndex;
            MyToolbar currentToolbar = MyToolbarComponent.CurrentToolbar;
            if (currentToolbar != null)
            {
                int slot = currentToolbar.IndexToSlot(this.m_contextMenuItemIndex);
                if (currentToolbar.IsValidSlot(slot))
                {
                    MyToolbarItem slotItem = currentToolbar.GetSlotItem(slot);
                    MyToolbarItemActions actions = slotItem as MyToolbarItemActions;
                    if (slotItem != null)
                    {
                        if ((itemIndex < 0) || (itemIndex >= actions.PossibleActions(this.ShownToolbar.ToolbarType).Count))
                        {
                            this.RemoveToolbarItem(slot);
                        }
                        else
                        {
                            actions.ActionId = (string) args.UserData;
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= MyToolbarComponent.CurrentToolbar.SlotCount)
                                {
                                    MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, actions);
                                    break;
                                }
                                MyToolbarItem item = currentToolbar.GetSlotItem(num3);
                                if ((item != null) && item.Equals(actions))
                                {
                                    MyToolbarComponent.CurrentToolbar.SetItemAtSlot(num3, null);
                                }
                                num3++;
                            }
                        }
                    }
                }
                this.m_contextMenuItemIndex = -1;
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if ((this.m_style.VisibleCondition == null) || this.m_style.VisibleCondition.Eval())
            {
                this.m_colorVariantPanel.ColorMask = new Vector3(MyPlayer.SelectedColor.X, MathHelper.Clamp((float) (MyPlayer.SelectedColor.Y + 0.8f), (float) 0f, (float) 1f), MathHelper.Clamp((float) (MyPlayer.SelectedColor.Z + 0.55f), (float) 0f, (float) 1f)).HSVtoColor().ToVector4();
                this.m_statControls.ForEach(x => x.Draw(transitionAlpha, backgroundTransitionAlpha));
                base.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        private void grid_ItemClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (eventArgs.Button == MySharedButtonsEnum.Secondary)
            {
                int columnIndex = eventArgs.ColumnIndex;
                MyToolbar currentToolbar = MyToolbarComponent.CurrentToolbar;
                MyToolbarItem slotItem = currentToolbar.GetSlotItem(columnIndex);
                if (slotItem == null)
                {
                    return;
                }
                if (!(slotItem is MyToolbarItemActions))
                {
                    this.RemoveToolbarItem(eventArgs.ColumnIndex);
                }
                else
                {
                    ListReader<ITerminalAction> reader = (slotItem as MyToolbarItemActions).PossibleActions(this.ShownToolbar.ToolbarType);
                    if (!this.UseContextMenu || (reader.Count <= 0))
                    {
                        this.RemoveToolbarItem(eventArgs.ColumnIndex);
                    }
                    else
                    {
                        this.m_contextMenu.CreateNewContextMenu();
                        foreach (ITerminalAction action in reader)
                        {
                            this.m_contextMenu.AddItem(action.Name, "", action.Icon, action.Id);
                        }
                        this.m_contextMenu.AddItem(MyTexts.Get(MySpaceTexts.BlockAction_RemoveFromToolbar), "", "", null);
                        this.m_contextMenu.Enabled = true;
                        this.m_contextMenuItemIndex = currentToolbar.SlotToIndex(columnIndex);
                    }
                }
            }
            if (this.m_shownToolbar.IsValidIndex(eventArgs.ColumnIndex))
            {
                this.m_shownToolbar.ActivateItemAtSlot(eventArgs.ColumnIndex, true, true, true);
            }
        }

        private void grid_ItemDoubleClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            this.RemoveToolbarItem(eventArgs.ColumnIndex);
            if (this.m_shownToolbar.IsValidIndex(eventArgs.ColumnIndex))
            {
                this.m_shownToolbar.ActivateItemAtSlot(eventArgs.ColumnIndex, false, true, true);
            }
        }

        public void HandleDragAndDrop(object sender, MyDragAndDropEventArgs eventArgs)
        {
            MyToolbarItem userData = eventArgs.Item.UserData as MyToolbarItem;
            if (userData != null)
            {
                int itemIndex = MyToolbarComponent.CurrentToolbar.GetItemIndex(userData);
                if ((eventArgs.DropTo != null) && this.IsToolbarGrid(eventArgs.DropTo.Grid))
                {
                    int slot = MyToolbarComponent.CurrentToolbar.IndexToSlot(itemIndex);
                    int num3 = eventArgs.DropTo.ItemIndex;
                    MyToolbarComponent.CurrentToolbar.SetItemAtSlot(num3, userData);
                    MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, MyToolbarComponent.CurrentToolbar.GetItemAtSlot(eventArgs.DropTo.ItemIndex));
                }
                else
                {
                    MyToolbarComponent.CurrentToolbar.SetItemAtIndex(itemIndex, null);
                }
            }
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                base2 = base.HandleInputElements();
            }
            if ((this.UseContextMenu && MyInput.Static.IsMouseReleased(MyMouseButtonsEnum.Right)) && this.m_contextMenu.Enabled)
            {
                this.m_contextMenu.Enabled = false;
                this.m_contextMenu.Activate(true);
            }
            return base2;
        }

        private void HighlightCurrentPageLabel()
        {
            int currentPage = this.m_shownToolbar.CurrentPage;
            for (int i = 0; i < this.m_pageLabelList.Count; i++)
            {
                if ((i != currentPage) && ReferenceEquals(this.m_pageLabelList[i].BackgroundTexture, this.m_pageHighlightCompositeTexture))
                {
                    this.m_pageLabelList[i].BackgroundTexture = this.m_pageCompositeTexture;
                }
                else if ((i == currentPage) && ReferenceEquals(this.m_pageLabelList[i].BackgroundTexture, this.m_pageCompositeTexture))
                {
                    this.m_pageLabelList[i].BackgroundTexture = this.m_pageHighlightCompositeTexture;
                }
            }
        }

        private void InitStatConditions(ConditionBase conditionBase)
        {
            StatCondition condition = conditionBase as StatCondition;
            if (condition != null)
            {
                condition.SetStat(MyHud.Stats.GetStat(condition.StatId));
            }
            else
            {
                Condition condition2 = conditionBase as Condition;
                if (condition2 != null)
                {
                    foreach (ConditionBase base2 in condition2.Terms)
                    {
                        this.InitStatConditions(base2);
                    }
                }
            }
        }

        private void InitStatControls()
        {
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            Vector2 size = new Vector2((float) fullscreenRectangle.Width, (float) fullscreenRectangle.Height);
            if (this.m_style.StatControls != null)
            {
                foreach (MyObjectBuilder_StatControls controls in this.m_style.StatControls)
                {
                    MyStatControls item = new MyStatControls(controls, controls.ApplyHudScale ? (MyGuiManager.GetSafeScreenScale() * MyHud.HudElementsScaleMultiplier) : MyGuiManager.GetSafeScreenScale()) {
                        Position = MyUtils.AlignCoord(controls.Position * size, size, controls.OriginAlign)
                    };
                    this.m_statControls.Add(item);
                }
            }
        }

        public bool IsToolbarGrid(MyGuiControlGrid grid) => 
            ReferenceEquals(this.m_toolbarItemsGrid, grid);

        protected override void OnPositionChanged()
        {
            this.m_statControls.ForEach(x => x.Position = base.Position);
        }

        public override void OnRemoving()
        {
            MyToolbarComponent.CurrentToolbarChanged -= new Action(this.ToolbarComponent_CurrentToolbarChanged);
            if (this.m_shownToolbar != null)
            {
                this.m_shownToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                this.m_shownToolbar.ItemUpdated -= new Action<MyToolbar, MyToolbar.IndexArgs, MyToolbarItem.ChangeInfo>(this.Toolbar_ItemUpdated);
                this.m_shownToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_SelectedSlotChanged);
                this.m_shownToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.Toolbar_SlotActivated);
                this.m_shownToolbar.ItemEnabledChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_ItemEnabledChanged);
                this.m_shownToolbar.CurrentPageChanged -= new Action<MyToolbar, MyToolbar.PageChangeArgs>(this.Toolbar_CurrentPageChanged);
                this.m_shownToolbar = null;
            }
            MyToolbarComponent.IsToolbarControlShown = false;
            base.OnRemoving();
        }

        protected override void OnSizeChanged()
        {
            this.RefreshInternals();
            base.OnSizeChanged();
        }

        protected override void OnVisibleChanged()
        {
            base.OnVisibleChanged();
            MyToolbarComponent.IsToolbarControlShown = base.Visible;
        }

        private void RecreateControls(bool contructor)
        {
            if (this.m_style.VisibleCondition != null)
            {
                this.InitStatConditions(this.m_style.VisibleCondition);
            }
            MyGuiControlGrid grid1 = new MyGuiControlGrid();
            grid1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            grid1.VisualStyle = MyGuiControlGridStyleEnum.Toolbar;
            grid1.ColumnsCount = MyToolbarComponent.CurrentToolbar.SlotCount + 1;
            grid1.RowsCount = 1;
            this.m_toolbarItemsGrid = grid1;
            this.m_toolbarItemsGrid.ItemDoubleClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemDoubleClicked);
            this.m_toolbarItemsGrid.ItemClickedWithoutDoubleClick += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemClicked);
            this.m_selectedItemLabel = new MyGuiControlLabel();
            Vector2? position = null;
            VRageMath.Vector4? backgroundColor = null;
            this.m_colorVariantPanel = new MyGuiControlPanel(position, new Vector2?(this.m_style.ColorPanelStyle.Size), backgroundColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_colorVariantPanel.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
            this.m_contextMenu = new MyGuiControlContextMenu();
            this.m_contextMenu.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            this.m_contextMenu.Deactivate();
            this.m_contextMenu.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.contextMenu_ItemClicked);
            base.Elements.Add(this.m_colorVariantPanel);
            base.Elements.Add(this.m_selectedItemLabel);
            base.Elements.Add(this.m_toolbarItemsGrid);
            base.Elements.Add(this.m_contextMenu);
            this.m_colorVariantPanel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            this.m_selectedItemLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            this.m_toolbarItemsGrid.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            this.m_contextMenu.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            this.SetupToolbarStyle();
            this.RefreshInternals();
        }

        private void RefreshInternals()
        {
            this.RepositionControls();
        }

        private void RefreshSelectedItem(MyToolbar toolbar)
        {
            this.m_toolbarItemsGrid.SelectedIndex = toolbar.SelectedSlot;
            MyToolbarItem selectedItem = toolbar.SelectedItem;
            if (selectedItem != null)
            {
                this.m_selectedItemLabel.Text = selectedItem.DisplayName.ToString();
                this.m_colorVariantPanel.Visible = (selectedItem is MyToolbarItemCubeBlock) && MyFakes.ENABLE_BLOCK_COLORING;
            }
            else
            {
                this.m_colorVariantPanel.Visible = false;
                this.m_selectedItemLabel.Text = string.Empty;
            }
        }

        private void RemoveToolbarItem(int slot)
        {
            if (slot < MyToolbarComponent.CurrentToolbar.SlotCount)
            {
                MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, null);
            }
        }

        private unsafe void RepositionControls()
        {
            Vector2 pagesOffset = base.Size * 0.5f;
            this.m_toolbarItemsGrid.Position = pagesOffset;
            this.m_selectedItemLabel.Position = this.m_style.SelectedItemPosition;
            if (this.m_style.SelectedItemTextScale != null)
            {
                this.m_selectedItemLabel.TextScale = this.m_style.SelectedItemTextScale.Value;
            }
            this.m_colorVariantPanel.Position = this.m_style.ColorPanelStyle.Offset;
            pagesOffset = this.m_style.PageStyle.PagesOffset;
            foreach (MyGuiControlLabel label in this.m_pageLabelList)
            {
                label.Position = pagesOffset + new Vector2(label.Size.X * 0.5f, -label.Size.Y * 0.5f);
                float* singlePtr1 = (float*) ref pagesOffset.X;
                singlePtr1[0] += label.Size.X + 0.001f;
            }
            if (this.UseContextMenu)
            {
                base.Elements.Remove(this.m_contextMenu);
                base.Elements.Add(this.m_contextMenu);
            }
        }

        private void SetGridItemAt(int slot, MyToolbarItem item, bool clear = false)
        {
            if (item != null)
            {
                this.SetGridItemAt(slot, item, item.Icons, item.SubIcon, item.DisplayName.ToString(), new ColoredIcon?(this.GetSymbol(slot)), clear);
            }
            else
            {
                this.SetGridItemAt(slot, null, null, null, null, new ColoredIcon?(this.GetSymbol(slot)), clear);
            }
        }

        protected virtual void SetGridItemAt(int slot, MyToolbarItem item, string[] icons, string subicon, string tooltip, ColoredIcon? symbol = new ColoredIcon?(), bool clear = false)
        {
            MyGuiGridItem itemAt = this.m_toolbarItemsGrid.GetItemAt(slot);
            if (itemAt == null)
            {
                MyGuiGridItem item1 = new MyGuiGridItem(icons, subicon, tooltip, item, true);
                item1.SubIconOffset = this.m_style.ItemStyle.VariantOffset;
                itemAt = item1;
                this.m_toolbarItemsGrid.SetItemAt(slot, itemAt);
            }
            else
            {
                itemAt.UserData = item;
                itemAt.Icons = icons;
                itemAt.SubIcon = subicon;
                itemAt.SubIconOffset = this.m_style.ItemStyle.VariantOffset;
                if (itemAt.ToolTip == null)
                {
                    itemAt.ToolTip = new MyToolTips();
                }
                itemAt.ToolTip.ToolTips.Clear();
                itemAt.ToolTip.AddToolTip(tooltip, 0.7f, "Blue");
            }
            if (ReferenceEquals(item, null) | clear)
            {
                itemAt.ClearAllText();
            }
            if (this.DrawNumbers)
            {
                itemAt.AddText(MyToolbarComponent.GetSlotControlText(slot), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            }
            if (item != null)
            {
                item.FillGridItem(itemAt);
            }
            itemAt.Enabled = (item != null) ? item.Enabled : true;
            if (symbol != null)
            {
                itemAt.AddIcon(symbol.Value, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            }
        }

        private void SetupToolbarStyle()
        {
            MyGuiBorderThickness thickness;
            if (this.m_style.ItemStyle.Margin != null)
            {
                thickness = new MyGuiBorderThickness(this.m_style.ItemStyle.Margin.Value.Left, this.m_style.ItemStyle.Margin.Value.Right, this.m_style.ItemStyle.Margin.Value.Top, this.m_style.ItemStyle.Margin.Value.Botton);
            }
            else
            {
                thickness = new MyGuiBorderThickness(2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            }
            MyObjectBuilder_GuiTexture texture = MyGuiTextures.Static.GetTexture(this.m_style.ItemStyle.Texture);
            Vector2 vector = new Vector2(texture.SizePx.X * this.m_style.ItemStyle.ItemTextureScale.X, texture.SizePx.Y * this.m_style.ItemStyle.ItemTextureScale.Y);
            MyGuiHighlightTexture texture3 = new MyGuiHighlightTexture {
                Normal = texture.Path,
                Highlight = MyGuiTextures.Static.GetTexture(this.m_style.ItemStyle.TextureHighlight).Path,
                SizePx = vector
            };
            MyGuiStyleDefinition definition1 = new MyGuiStyleDefinition();
            definition1.ItemTexture = texture3;
            definition1.ItemFontNormal = this.m_style.ItemStyle.FontNormal;
            definition1.ItemFontHighlight = this.m_style.ItemStyle.FontHighlight;
            definition1.SizeOverride = new Vector2?(texture3.SizeGui * new Vector2(10f, 1f));
            definition1.ItemMargin = thickness;
            definition1.ItemTextScale = this.m_style.ItemStyle.TextScale;
            definition1.FitSizeToItems = true;
            MyGuiStyleDefinition styleDef = definition1;
            this.m_toolbarItemsGrid.SetCustomStyleDefinition(styleDef);
            this.m_pageCompositeTexture = MyGuiCompositeTexture.CreateFromDefinition(this.m_style.PageStyle.PageCompositeTexture);
            this.m_pageHighlightCompositeTexture = MyGuiCompositeTexture.CreateFromDefinition(this.m_style.PageStyle.PageHighlightCompositeTexture);
            this.m_itemVarianTtexture = MyGuiTextures.Static.GetTexture(this.m_style.ItemStyle.VariantTexture);
            this.m_colorVariantPanel.BackgroundTexture = MyGuiCompositeTexture.CreateFromDefinition(this.m_style.ColorPanelStyle.Texture);
            this.InitStatControls();
        }

        public void ShowToolbar(MyToolbar toolbar)
        {
            if (this.m_shownToolbar != null)
            {
                this.m_shownToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                this.m_shownToolbar.ItemUpdated -= new Action<MyToolbar, MyToolbar.IndexArgs, MyToolbarItem.ChangeInfo>(this.Toolbar_ItemUpdated);
                this.m_shownToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_SelectedSlotChanged);
                this.m_shownToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.Toolbar_SlotActivated);
                this.m_shownToolbar.ItemEnabledChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_ItemEnabledChanged);
                this.m_shownToolbar.CurrentPageChanged -= new Action<MyToolbar, MyToolbar.PageChangeArgs>(this.Toolbar_CurrentPageChanged);
                foreach (MyGuiControlLabel label in this.m_pageLabelList)
                {
                    base.Elements.Remove(label);
                }
                this.m_pageLabelList.Clear();
            }
            this.m_shownToolbar = toolbar;
            if (this.m_shownToolbar == null)
            {
                this.m_toolbarItemsGrid.Enabled = false;
                this.m_toolbarItemsGrid.Visible = false;
            }
            else
            {
                int slotCount = toolbar.SlotCount;
                this.m_toolbarItemsGrid.ColumnsCount = slotCount + (toolbar.ShowHolsterSlot ? 1 : 0);
                for (int i = 0; i < slotCount; i++)
                {
                    this.SetGridItemAt(i, toolbar.GetSlotItem(i), true);
                }
                this.m_selectedItemLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
                this.m_colorVariantPanel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                this.m_colorVariantPanel.Visible = MyFakes.ENABLE_BLOCK_COLORING;
                if (toolbar.ShowHolsterSlot)
                {
                    string[] icons = new string[] { @"Textures\GUI\Icons\HideWeapon.dds" };
                    ColoredIcon? symbol = null;
                    this.SetGridItemAt(slotCount, new MyToolbarItemEmpty(), icons, null, MyTexts.GetString(MyCommonTexts.HideWeapon), symbol, false);
                }
                if (toolbar.PageCount > 1)
                {
                    for (int j = 0; j < toolbar.PageCount; j++)
                    {
                        m_textCache.Clear();
                        m_textCache.AppendInt32(j + 1);
                        Vector2? position = null;
                        position = null;
                        string text1 = MyToolbarComponent.GetSlotControlText(j).ToString();
                        VRageMath.Vector4? colorMask = null;
                        MyGuiControlLabel item = new MyGuiControlLabel(position, position, text1 ?? m_textCache.ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                            BackgroundTexture = this.m_pageCompositeTexture
                        };
                        float? numberSize = this.m_style.PageStyle.NumberSize;
                        item.TextScale = (numberSize != null) ? numberSize.GetValueOrDefault() : 0.7f;
                        item.Size = this.m_toolbarItemsGrid.ItemSize * new Vector2(0.5f, 0.35f);
                        item.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
                        this.m_pageLabelList.Add(item);
                        base.Elements.Add(item);
                    }
                }
                this.RepositionControls();
                this.HighlightCurrentPageLabel();
                this.RefreshSelectedItem(toolbar);
                this.m_shownToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                this.m_shownToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                this.m_shownToolbar.ItemUpdated -= new Action<MyToolbar, MyToolbar.IndexArgs, MyToolbarItem.ChangeInfo>(this.Toolbar_ItemUpdated);
                this.m_shownToolbar.ItemUpdated += new Action<MyToolbar, MyToolbar.IndexArgs, MyToolbarItem.ChangeInfo>(this.Toolbar_ItemUpdated);
                this.m_shownToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_SelectedSlotChanged);
                this.m_shownToolbar.SelectedSlotChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_SelectedSlotChanged);
                this.m_shownToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.Toolbar_SlotActivated);
                this.m_shownToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.Toolbar_SlotActivated);
                this.m_shownToolbar.ItemEnabledChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_ItemEnabledChanged);
                this.m_shownToolbar.ItemEnabledChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.Toolbar_ItemEnabledChanged);
                this.m_shownToolbar.CurrentPageChanged -= new Action<MyToolbar, MyToolbar.PageChangeArgs>(this.Toolbar_CurrentPageChanged);
                this.m_shownToolbar.CurrentPageChanged += new Action<MyToolbar, MyToolbar.PageChangeArgs>(this.Toolbar_CurrentPageChanged);
                Vector2 vector = new Vector2(this.m_toolbarItemsGrid.Size.X, (this.m_toolbarItemsGrid.Size.Y + this.m_selectedItemLabel.Size.Y) + this.m_colorVariantPanel.Size.Y);
                base.MinSize = vector;
                base.MaxSize = vector;
                this.m_toolbarItemsGrid.Enabled = true;
                this.m_toolbarItemsGrid.Visible = true;
            }
        }

        private void Toolbar_CurrentPageChanged(MyToolbar toolbar, MyToolbar.PageChangeArgs args)
        {
            if (this.UseContextMenu)
            {
                this.m_contextMenu.Deactivate();
            }
            this.HighlightCurrentPageLabel();
            for (int i = 0; i < MyToolbarComponent.CurrentToolbar.SlotCount; i++)
            {
                this.SetGridItemAt(i, toolbar.GetSlotItem(i), true);
            }
        }

        private void Toolbar_ItemChanged(MyToolbar toolbar, MyToolbar.IndexArgs args)
        {
            this.UpdateItemAtIndex(toolbar, args.ItemIndex);
        }

        private void Toolbar_ItemEnabledChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            if (args.SlotNumber != null)
            {
                int index = args.SlotNumber.Value;
                this.m_toolbarItemsGrid.GetItemAt(index).Enabled = toolbar.IsEnabled(toolbar.SlotToIndex(index));
            }
            else
            {
                for (int i = 0; i < this.m_toolbarItemsGrid.ColumnsCount; i++)
                {
                    this.m_toolbarItemsGrid.GetItemAt(i).Enabled = toolbar.IsEnabled(toolbar.SlotToIndex(i));
                }
            }
        }

        private void Toolbar_ItemUpdated(MyToolbar toolbar, MyToolbar.IndexArgs args, MyToolbarItem.ChangeInfo changes)
        {
            if (changes == MyToolbarItem.ChangeInfo.Icon)
            {
                this.UpdateItemIcon(toolbar, args);
            }
            else
            {
                this.UpdateItemAtIndex(toolbar, args.ItemIndex);
            }
        }

        private void Toolbar_SelectedSlotChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            this.RefreshSelectedItem(toolbar);
        }

        private void Toolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            this.m_toolbarItemsGrid.blinkSlot(args.SlotNumber);
        }

        private void ToolbarComponent_CurrentToolbarChanged()
        {
            this.ShowToolbar(MyToolbarComponent.CurrentToolbar);
        }

        public override void Update()
        {
            base.Update();
        }

        private void UpdateItemAtIndex(MyToolbar toolbar, int index)
        {
            int slot = toolbar.IndexToSlot(index);
            if (toolbar.IsValidIndex(index) && toolbar.IsValidSlot(slot))
            {
                this.SetGridItemAt(slot, toolbar[index], true);
                int? selectedSlot = toolbar.SelectedSlot;
                int num2 = slot;
                if ((selectedSlot.GetValueOrDefault() == num2) & (selectedSlot != null))
                {
                    this.RefreshSelectedItem(toolbar);
                }
            }
        }

        private void UpdateItemIcon(MyToolbar toolbar, MyToolbar.IndexArgs args)
        {
            if (toolbar.IsValidIndex(args.ItemIndex))
            {
                int index = toolbar.IndexToSlot(args.ItemIndex);
                if (index != -1)
                {
                    this.m_toolbarItemsGrid.GetItemAt(index).Icons = toolbar.GetItemIcons(args.ItemIndex);
                }
            }
            else
            {
                for (int i = 0; i < this.m_toolbarItemsGrid.ColumnsCount; i++)
                {
                    this.m_toolbarItemsGrid.GetItemAt(i).Icons = toolbar.GetItemIcons(toolbar.SlotToIndex(i));
                }
            }
        }

        public MyToolbar ShownToolbar =>
            this.m_shownToolbar;

        public MyGuiControlGrid ToolbarGrid =>
            this.m_toolbarItemsGrid;

        public bool DrawNumbers =>
            MyToolbarComponent.CurrentToolbar.DrawNumbers;

        public Func<int, ColoredIcon> GetSymbol =>
            MyToolbarComponent.CurrentToolbar.GetSymbol;
    }
}


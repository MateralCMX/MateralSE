namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlGrid))]
    public class MyGuiControlGrid : MyGuiControlBase
    {
        private static MyGuiStyleDefinition[] m_styles;
        public const int INVALID_INDEX = -1;
        public Vector4 ItemBackgroundColorMask;
        private Vector2 m_doubleClickFirstPosition;
        private int? m_doubleClickStarted;
        private bool m_isItemDraggingLeft;
        private bool m_isItemDraggingRight;
        private Vector2 m_mouseDragStartPosition;
        protected RectangleF m_itemsRectangle;
        protected Vector2 m_itemStep;
        private readonly List<MyGuiGridItem> m_items;
        private MyToolTips m_emptyItemToolTip;
        private EventArgs? m_singleClickEvents;
        private EventArgs? m_itemClicked;
        private int? m_lastClick;
        public Dictionary<int, Color> ModalItems;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemChanged;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemClicked;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemReleased;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemClickedWithoutDoubleClick;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemDoubleClicked;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemDragged;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> ItemSelected;
        [CompilerGenerated]
        private Action<MyGuiControlGrid, EventArgs> MouseOverIndexChanged;
        private int m_columnsCount;
        private int m_rowsCount;
        private int m_maxItemCount;
        private int m_mouseOverIndex;
        private int? m_selectedIndex;
        private MyGuiControlGridStyleEnum m_visualStyle;
        protected MyGuiStyleDefinition m_styleDef;
        private float m_itemTextScale;
        private float m_itemTextScaleWithLanguage;
        public string EmptyItemIcon;
        public bool SelectionEnabled;
        public bool ShowEmptySlots;

        public event Action<MyGuiControlGrid, EventArgs> ItemChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemChanged = this.ItemChanged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemChanged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemChanged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemChanged, action3, a);
                    if (ReferenceEquals(itemChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemChanged = this.ItemChanged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemChanged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemChanged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemChanged, action3, source);
                    if (ReferenceEquals(itemChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemClicked;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemClicked, action3, a);
                    if (ReferenceEquals(itemClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemClicked;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemClicked, action3, source);
                    if (ReferenceEquals(itemClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemClickedWithoutDoubleClick
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemClickedWithoutDoubleClick = this.ItemClickedWithoutDoubleClick;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemClickedWithoutDoubleClick;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemClickedWithoutDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemClickedWithoutDoubleClick, action3, a);
                    if (ReferenceEquals(itemClickedWithoutDoubleClick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemClickedWithoutDoubleClick = this.ItemClickedWithoutDoubleClick;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemClickedWithoutDoubleClick;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemClickedWithoutDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemClickedWithoutDoubleClick, action3, source);
                    if (ReferenceEquals(itemClickedWithoutDoubleClick, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemDoubleClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemDoubleClicked;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemDoubleClicked, action3, a);
                    if (ReferenceEquals(itemDoubleClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemDoubleClicked;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemDoubleClicked, action3, source);
                    if (ReferenceEquals(itemDoubleClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemDragged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemDragged = this.ItemDragged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemDragged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemDragged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemDragged, action3, a);
                    if (ReferenceEquals(itemDragged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemDragged = this.ItemDragged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemDragged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemDragged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemDragged, action3, source);
                    if (ReferenceEquals(itemDragged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemReleased
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemReleased = this.ItemReleased;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemReleased;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemReleased = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemReleased, action3, a);
                    if (ReferenceEquals(itemReleased, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemReleased = this.ItemReleased;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemReleased;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemReleased = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemReleased, action3, source);
                    if (ReferenceEquals(itemReleased, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> ItemSelected
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> itemSelected = this.ItemSelected;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = itemSelected;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    itemSelected = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemSelected, action3, a);
                    if (ReferenceEquals(itemSelected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> itemSelected = this.ItemSelected;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = itemSelected;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    itemSelected = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.ItemSelected, action3, source);
                    if (ReferenceEquals(itemSelected, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlGrid, EventArgs> MouseOverIndexChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlGrid, EventArgs> mouseOverIndexChanged = this.MouseOverIndexChanged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> a = mouseOverIndexChanged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Combine(a, value);
                    mouseOverIndexChanged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.MouseOverIndexChanged, action3, a);
                    if (ReferenceEquals(mouseOverIndexChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlGrid, EventArgs> mouseOverIndexChanged = this.MouseOverIndexChanged;
                while (true)
                {
                    Action<MyGuiControlGrid, EventArgs> source = mouseOverIndexChanged;
                    Action<MyGuiControlGrid, EventArgs> action3 = (Action<MyGuiControlGrid, EventArgs>) Delegate.Remove(source, value);
                    mouseOverIndexChanged = Interlocked.CompareExchange<Action<MyGuiControlGrid, EventArgs>>(ref this.MouseOverIndexChanged, action3, source);
                    if (ReferenceEquals(mouseOverIndexChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlGrid()
        {
            MyGuiBorderThickness thickness = new MyGuiBorderThickness(4f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            MyGuiBorderThickness thickness2 = new MyGuiBorderThickness(2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles = new MyGuiStyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlGridStyleEnum>() + 1];
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.LeftTop = new MyGuiSizedTexture(MyGuiConstants.TEXTURE_SCREEN_BACKGROUND);
            MyGuiStyleDefinition definition1 = new MyGuiStyleDefinition();
            definition1.BackgroundTexture = texture1;
            definition1.BackgroundPaddingSize = MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.PaddingSizeGui;
            definition1.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition1.ItemFontNormal = "Blue";
            definition1.ItemFontHighlight = "White";
            definition1.ItemPadding = thickness;
            m_styles[0] = definition1;
            MyGuiStyleDefinition definition2 = new MyGuiStyleDefinition();
            definition2.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition2.ItemFontNormal = "Blue";
            definition2.ItemFontHighlight = "White";
            definition2.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_GRID_ITEM.SizeGui * new Vector2(10f, 1f));
            definition2.ItemMargin = thickness2;
            definition2.ItemPadding = thickness;
            definition2.ItemTextScale = 0.6f;
            definition2.FitSizeToItems = true;
            m_styles[1] = definition2;
            MyGuiStyleDefinition definition3 = new MyGuiStyleDefinition();
            definition3.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM_SMALL;
            definition3.ItemFontNormal = "Blue";
            definition3.ItemFontHighlight = "White";
            definition3.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_GRID_ITEM_SMALL.SizeGui * new Vector2(10f, 1f));
            definition3.ItemMargin = thickness2;
            definition3.ItemPadding = thickness;
            definition3.ItemTextScale = 0.6f;
            definition3.FitSizeToItems = true;
            m_styles[2] = definition3;
            MyGuiCompositeTexture texture3 = new MyGuiCompositeTexture(null);
            texture3.Center = new MyGuiSizedTexture(MyGuiConstants.TEXTURE_SCREEN_TOOLS_BACKGROUND_BLOCKS);
            MyGuiStyleDefinition definition4 = new MyGuiStyleDefinition();
            definition4.BackgroundTexture = texture3;
            definition4.BackgroundPaddingSize = MyGuiConstants.TEXTURE_SCREEN_TOOLS_BACKGROUND_BLOCKS.PaddingSizeGui;
            definition4.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition4.ItemFontNormal = "Blue";
            definition4.ItemFontHighlight = "White";
            definition4.ItemMargin = thickness2;
            definition4.ItemPadding = thickness;
            m_styles[3] = definition4;
            MyGuiCompositeTexture texture4 = new MyGuiCompositeTexture(null);
            texture4.LeftTop = new MyGuiSizedTexture(MyGuiConstants.TEXTURE_SCREEN_TOOLS_BACKGROUND_WEAPONS);
            MyGuiStyleDefinition definition5 = new MyGuiStyleDefinition();
            definition5.BackgroundTexture = texture4;
            definition5.BackgroundPaddingSize = MyGuiConstants.TEXTURE_SCREEN_TOOLS_BACKGROUND_WEAPONS.PaddingSizeGui;
            definition5.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition5.ItemFontNormal = "Blue";
            definition5.ItemFontHighlight = "White";
            definition5.ItemMargin = thickness2;
            definition5.ItemPadding = thickness;
            definition5.FitSizeToItems = true;
            m_styles[4] = definition5;
            MyGuiStyleDefinition definition6 = new MyGuiStyleDefinition();
            definition6.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition6.ItemFontNormal = "Blue";
            definition6.ItemFontHighlight = "White";
            definition6.ItemMargin = thickness2;
            definition6.ItemPadding = thickness;
            definition6.SizeOverride = new Vector2?(new Vector2(593f, 91f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            definition6.ItemTextScale = 0.64f;
            definition6.BorderEnabled = true;
            definition6.BorderColor = new Vector4(0.37f, 0.58f, 0.68f, 1f);
            definition6.FitSizeToItems = true;
            definition6.ContentPadding = new MyGuiBorderThickness(1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles[5] = definition6;
            MyGuiStyleDefinition definition7 = new MyGuiStyleDefinition();
            definition7.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM_TINY;
            definition7.ItemFontNormal = "Blue";
            definition7.ItemFontHighlight = "White";
            definition7.ItemMargin = thickness2;
            definition7.ItemPadding = thickness;
            definition7.SizeOverride = new Vector2?(new Vector2(593f, 91f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            definition7.ItemTextScale = 0.64f;
            definition7.FitSizeToItems = false;
            definition7.ContentPadding = new MyGuiBorderThickness(1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles[6] = definition7;
        }

        public MyGuiControlGrid() : base(new Vector2?(Vector2.Zero), new Vector2(0.05f, 0.05f), new Vector4?(MyGuiConstants.LISTBOX_BACKGROUND_COLOR), null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.ItemBackgroundColorMask = Vector4.One;
            this.m_maxItemCount = 0x7fffffff;
            this.SelectionEnabled = true;
            this.ShowEmptySlots = true;
            this.m_items = new List<MyGuiGridItem>();
            this.RefreshVisualStyle();
            this.RowsCount = 1;
            this.ColumnsCount = 1;
            base.Name = "Grid";
            base.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.EnableSelectEmptyCell = true;
            this.ItemBackgroundColorMask = base.ColorMask;
        }

        public void Add(MyGuiGridItem item, int startingRow = 0)
        {
            int count;
            if (!this.TryFindEmptyIndex(out count, startingRow))
            {
                count = this.m_items.Count;
                this.m_items.Add(null);
            }
            this.m_items[count] = item;
            if (this.ItemChanged != null)
            {
                EventArgs args = new EventArgs();
                int? rowIdx = null;
                rowIdx = null;
                this.PrepareEventArgs(ref args, count, rowIdx, rowIdx);
                this.ItemChanged(this, args);
            }
            float num2 = (count / this.m_columnsCount) + 1f;
            this.RowsCount = Math.Max(this.RowsCount, (int) num2);
        }

        public void AddRows(int numberOfRows)
        {
            if ((numberOfRows > 0) && (this.ColumnsCount > 0))
            {
                while ((this.m_items.Count % this.ColumnsCount) != 0)
                {
                    this.m_items.Add(null);
                }
                int num = 0;
                while (num < numberOfRows)
                {
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= this.ColumnsCount)
                        {
                            num++;
                            break;
                        }
                        this.m_items.Add(null);
                        num2++;
                    }
                }
                this.RecalculateRowsCount();
            }
        }

        public void blinkSlot(int? slot)
        {
            if (slot != null)
            {
                this.m_items[slot.Value].startBlinking();
            }
        }

        public virtual void Clear()
        {
            this.m_items.Clear();
            this.m_selectedIndex = null;
            this.RowsCount = 0;
        }

        private int ComputeColumn(int itemIndex) => 
            (itemIndex % this.ColumnsCount);

        private int ComputeIndex(Vector2 normalizedPosition)
        {
            Vector2I vectori;
            if (!this.m_itemsRectangle.Contains(normalizedPosition))
            {
                return -1;
            }
            vectori.X = (int) ((normalizedPosition.X - this.m_itemsRectangle.Position.X) / this.m_itemStep.X);
            vectori.Y = (int) ((normalizedPosition.Y - this.m_itemsRectangle.Position.Y) / this.m_itemStep.Y);
            int itemIndex = (vectori.Y * this.ColumnsCount) + vectori.X;
            return (this.IsValidCellIndex(itemIndex) ? itemIndex : -1);
        }

        public int ComputeIndex(int row, int col) => 
            ((row * this.ColumnsCount) + col);

        private int ComputeRow(int itemIndex) => 
            (itemIndex / this.ColumnsCount);

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(new Vector2(this.m_itemsRectangle.X, this.m_itemsRectangle.Y), new Vector2(this.m_itemsRectangle.Width, this.m_itemsRectangle.Height), Color.White, 1);
            if (this.IsValidIndex(this.MouseOverIndex))
            {
                MyGuiBorderThickness itemPadding = this.m_styleDef.ItemPadding;
                int num = this.ComputeColumn(this.MouseOverIndex);
                int num2 = this.ComputeRow(this.MouseOverIndex);
                MyGuiManager.DrawBorders((this.m_itemsRectangle.Position + (this.m_itemStep * new Vector2((float) num, (float) num2))) + itemPadding.TopLeftOffset, this.ItemSize - itemPadding.SizeChange, Color.White, 1);
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            this.RefreshItemsRectangle();
            this.DrawItemBackgrounds(backgroundTransitionAlpha);
            this.DrawItems(transitionAlpha);
            this.DrawItemTexts(transitionAlpha);
        }

        private void DrawItemBackgrounds(float transitionAlpha)
        {
            string normal = this.m_styleDef.ItemTexture.Normal;
            string highlight = this.m_styleDef.ItemTexture.Highlight;
            int num = Math.Min(this.m_maxItemCount, this.RowsCount * this.ColumnsCount);
            int itemIdx = 0;
            while (true)
            {
                while (true)
                {
                    int num6;
                    if (itemIdx >= num)
                    {
                        return;
                    }
                    int num3 = itemIdx / this.ColumnsCount;
                    int num4 = itemIdx % this.ColumnsCount;
                    Vector2 positionLeftTop = this.m_itemsRectangle.Position + (this.m_itemStep * new Vector2((float) num4, (float) num3));
                    MyGuiGridItem item = this.TryGetItemAt(itemIdx);
                    bool enabled = base.Enabled && ((item != null) ? item.Enabled : true);
                    bool flag2 = false;
                    float num5 = 1f;
                    if (item != null)
                    {
                        flag2 = (MyGuiManager.TotalTimeInMilliseconds - item.blinkCount) <= 400L;
                        if (flag2)
                        {
                            num5 = item.blinkingTransparency();
                        }
                    }
                    if (!enabled || ((this.MouseOverIndex != -1) && !this.IsValidIndex(this.MouseOverIndex)))
                    {
                        num6 = 0;
                    }
                    else
                    {
                        int num1;
                        if (itemIdx == this.MouseOverIndex)
                        {
                            num1 = 1;
                        }
                        else
                        {
                            int? selectedIndex = this.SelectedIndex;
                            num1 = (int) ((itemIdx == selectedIndex.GetValueOrDefault()) & (selectedIndex != null));
                        }
                        num6 = num1 | flag2;
                    }
                    bool flag3 = (bool) num6;
                    Vector4 itemBackgroundColorMask = this.ItemBackgroundColorMask;
                    if ((this.ModalItems != null) && (this.ModalItems.Count > 0))
                    {
                        if (!this.ModalItems.ContainsKey(itemIdx))
                        {
                            break;
                        }
                        itemBackgroundColorMask = this.ModalItems[itemIdx];
                        MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.Draw(positionLeftTop, this.ItemSize, Color.Yellow, 1f);
                    }
                    if (this.ShowEmptySlots)
                    {
                        MyGuiManager.DrawSpriteBatch(flag3 ? highlight : normal, positionLeftTop, this.ItemSize, ApplyColorMaskModifiers(itemBackgroundColorMask, enabled, transitionAlpha * num5), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                    else if (item != null)
                    {
                        MyGuiManager.DrawSpriteBatch(flag3 ? highlight : normal, positionLeftTop, this.ItemSize, ApplyColorMaskModifiers(itemBackgroundColorMask, enabled, transitionAlpha * num5), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                    break;
                }
                itemIdx++;
            }
        }

        private void DrawItems(float transitionAlpha)
        {
            int num = Math.Min(this.m_maxItemCount, this.RowsCount * this.ColumnsCount);
            for (int i = 0; i < num; i++)
            {
                int num3 = i / this.ColumnsCount;
                int num4 = i % this.ColumnsCount;
                MyGuiGridItem item = this.TryGetItemAt(i);
                Vector2 normalizedCoord = this.m_itemsRectangle.Position + (this.m_itemStep * new Vector2((float) num4, (float) num3));
                Vector4 colorMask = base.ColorMask;
                bool flag = true;
                if (((this.ModalItems == null) || (this.ModalItems.Count <= 0)) || this.ModalItems.ContainsKey(i))
                {
                    Vector2 vector3;
                    if ((item == null) || (item.SubIconOffset == null))
                    {
                        vector3 = new Vector2(0.8888889f, 0.4444444f);
                    }
                    else
                    {
                        vector3 = item.SubIconOffset.Value;
                    }
                    Vector2 vector4 = this.m_itemsRectangle.Position + (this.m_itemStep * (new Vector2((float) num4, (float) num3) + vector3));
                    Vector2 vector5 = this.m_itemsRectangle.Position + (this.m_itemStep * (new Vector2((float) num4, (float) num3) + Vector2.One));
                    if ((item == null) || (item.Icons == null))
                    {
                        if (this.EmptyItemIcon != null)
                        {
                            bool enabled = base.Enabled & flag;
                            MyGuiManager.DrawSpriteBatch(this.EmptyItemIcon, normalizedCoord, this.ItemSize, ApplyColorMaskModifiers(base.ColorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                        }
                    }
                    else
                    {
                        bool enabled = (base.Enabled && item.Enabled) & flag;
                        int index = 0;
                        while (true)
                        {
                            if (index >= item.Icons.Length)
                            {
                                if (!string.IsNullOrWhiteSpace(item.SubIcon))
                                {
                                    MyGuiManager.DrawSpriteBatch(item.SubIcon, vector4, this.ItemSize / 3f, ApplyColorMaskModifiers(colorMask * item.IconColorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, false);
                                }
                                if (!string.IsNullOrWhiteSpace(item.SubIcon2))
                                {
                                    MyGuiManager.DrawSpriteBatch(item.SubIcon2, vector5, this.ItemSize / 3.5f, ApplyColorMaskModifiers(colorMask * item.IconColorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, false);
                                }
                                if (item.OverlayPercent != 0f)
                                {
                                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", normalizedCoord, this.ItemSize * new Vector2(item.OverlayPercent, 1f), ApplyColorMaskModifiers(colorMask * item.OverlayColorMask, enabled, transitionAlpha * 0.5f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                                }
                                break;
                            }
                            MyGuiManager.DrawSpriteBatch(item.Icons[index], normalizedCoord, this.ItemSize, ApplyColorMaskModifiers(colorMask * item.IconColorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                            index++;
                        }
                    }
                    if (item != null)
                    {
                        foreach (KeyValuePair<MyGuiDrawAlignEnum, ColoredIcon> pair in item.IconsByAlign)
                        {
                            if (!string.IsNullOrEmpty(pair.Value.Icon))
                            {
                                MyGuiManager.DrawSpriteBatch(pair.Value.Icon, normalizedCoord + (this.m_itemStep * new Vector2(0.05555556f, 0.1111111f)), this.ItemSize / 3f, ApplyColorMaskModifiers(pair.Value.Color, true, transitionAlpha), pair.Key, false, true);
                            }
                        }
                    }
                }
            }
        }

        private void DrawItemTexts(float transitionAlpha)
        {
            MyGuiBorderThickness itemPadding = this.m_styleDef.ItemPadding;
            string itemFontNormal = this.m_styleDef.ItemFontNormal;
            string itemFontHighlight = this.m_styleDef.ItemFontHighlight;
            int num = Math.Min(this.m_maxItemCount, this.RowsCount * this.ColumnsCount);
            int itemIdx = 0;
            while (true)
            {
                while (true)
                {
                    if (itemIdx >= num)
                    {
                        return;
                    }
                    int num3 = itemIdx / this.ColumnsCount;
                    int num4 = itemIdx % this.ColumnsCount;
                    MyGuiGridItem item = this.TryGetItemAt(itemIdx);
                    if (item != null)
                    {
                        using (Dictionary<MyGuiDrawAlignEnum, StringBuilder>.Enumerator enumerator = item.TextsByAlign.GetEnumerator())
                        {
                            KeyValuePair<MyGuiDrawAlignEnum, StringBuilder> current;
                            RectangleF ef;
                            Vector2 coordAlignedFromRectangle;
                            bool flag;
                            string text1;
                            goto TR_000B;
                        TR_0003:
                            MyGuiManager.DrawString(text1, current.Value, coordAlignedFromRectangle, this.ItemTextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(base.ColorMask, flag, transitionAlpha)), current.Key, false, ef.Size.X);
                        TR_000B:
                            while (true)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                current = enumerator.Current;
                                Vector2 vector = this.m_itemsRectangle.Position + (this.m_itemStep * new Vector2((float) num4, (float) num3));
                                ef = new RectangleF(vector + itemPadding.TopLeftOffset, this.ItemSize - itemPadding.SizeChange);
                                coordAlignedFromRectangle = MyUtils.GetCoordAlignedFromRectangle(ref ef, current.Key);
                                flag = base.Enabled && item.Enabled;
                                if (itemIdx != this.MouseOverIndex)
                                {
                                    int? selectedIndex = this.SelectedIndex;
                                    if (!((itemIdx == selectedIndex.GetValueOrDefault()) & (selectedIndex != null)))
                                    {
                                        text1 = itemFontNormal;
                                        goto TR_0003;
                                    }
                                }
                                text1 = itemFontHighlight;
                                goto TR_0003;
                            }
                            break;
                        }
                    }
                    break;
                }
                itemIdx++;
            }
        }

        public MyGuiGridItem GetItemAt(int index)
        {
            if (!this.IsValidIndex(index))
            {
                int num1 = MathHelper.Clamp(index, 0, this.m_items.Count - 1);
                index = num1;
            }
            return this.m_items[index];
        }

        public MyGuiGridItem GetItemAt(int rowIdx, int colIdx) => 
            this.m_items[this.ComputeIndex(rowIdx, colIdx)];

        public int GetItemsCount() => 
            this.m_items.Count;

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlGrid objectBuilder = (MyObjectBuilder_GuiControlGrid) base.GetObjectBuilder();
            objectBuilder.VisualStyle = this.VisualStyle;
            objectBuilder.DisplayRowsCount = this.RowsCount;
            objectBuilder.DisplayColumnsCount = this.ColumnsCount;
            return objectBuilder;
        }

        public static MyGuiStyleDefinition GetVisualStyle(MyGuiControlGridStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase captureInput = base.HandleInput();
            if (captureInput != null)
            {
                this.MouseOverIndex = -1;
                return captureInput;
            }
            if (base.Enabled)
            {
                if (!base.IsMouseOver)
                {
                    this.TryTriggerSingleClickEvent();
                    return captureInput;
                }
                this.MouseOverIndex = base.IsMouseOver ? this.ComputeIndex(MyGuiManager.MouseCursorPosition) : -1;
                if (((this.MouseOverIndex != this.MouseOverIndex) && base.Enabled) && (this.MouseOverIndex != -1))
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
                }
                this.HandleNewMousePress(ref captureInput);
                this.HandleMouseDrag(ref captureInput, MySharedButtonsEnum.Primary, ref this.m_isItemDraggingLeft);
                this.HandleMouseDrag(ref captureInput, MySharedButtonsEnum.Secondary, ref this.m_isItemDraggingRight);
                if ((this.m_singleClickEvents != null) && (this.m_singleClickEvents.Value.Button == MySharedButtonsEnum.Secondary))
                {
                    this.TryTriggerSingleClickEvent();
                }
                if ((this.m_doubleClickStarted != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) >= 500f))
                {
                    this.m_doubleClickStarted = null;
                    this.TryTriggerSingleClickEvent();
                }
            }
            return captureInput;
        }

        private void HandleMouseDrag(ref MyGuiControlBase captureInput, MySharedButtonsEnum button, ref bool isDragging)
        {
            if (MyInput.Static.IsNewButtonPressed(button))
            {
                isDragging = true;
                this.m_mouseDragStartPosition = MyGuiManager.MouseCursorPosition;
            }
            else if (!MyInput.Static.IsButtonPressed(button))
            {
                isDragging = false;
            }
            else if (isDragging && (this.SelectedItem != null))
            {
                if ((MyGuiManager.MouseCursorPosition - this.m_mouseDragStartPosition).Length() != 0f)
                {
                    if (this.ItemDragged != null)
                    {
                        int index = this.ComputeIndex(MyGuiManager.MouseCursorPosition);
                        if (this.IsValidIndex(index) && (this.GetItemAt(index) != null))
                        {
                            EventArgs args;
                            this.MakeEventArgs(out args, index, button);
                            this.ItemDragged(this, args);
                        }
                    }
                    isDragging = false;
                }
                captureInput = this;
            }
        }

        private void HandleNewMousePress(ref MyGuiControlBase captureInput)
        {
            bool flag = this.m_itemsRectangle.Contains(MyGuiManager.MouseCursorPosition);
            if (MyInput.Static.IsNewPrimaryButtonReleased() || MyInput.Static.IsNewSecondaryButtonReleased())
            {
                if (flag)
                {
                    int? mouseOverIndex = new int?(this.ComputeIndex(MyGuiManager.MouseCursorPosition));
                    if (!this.IsValidIndex(mouseOverIndex.Value))
                    {
                        mouseOverIndex = null;
                    }
                    this.SelectMouseOverItem(mouseOverIndex);
                    if (((this.SelectedIndex != null) && ((this.m_itemClicked != null) && ((this.m_lastClick != null) && ((mouseOverIndex != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_lastClick.Value) < 500f))))) && (this.m_itemClicked.Value.ItemIndex == mouseOverIndex.Value))
                    {
                        EventArgs args;
                        captureInput = this;
                        MySharedButtonsEnum none = MySharedButtonsEnum.None;
                        if (MyInput.Static.IsNewPrimaryButtonReleased())
                        {
                            none = MySharedButtonsEnum.Primary;
                        }
                        else if (MyInput.Static.IsNewSecondaryButtonReleased())
                        {
                            none = MySharedButtonsEnum.Secondary;
                        }
                        this.MakeEventArgs(out args, this.SelectedIndex.Value, none);
                        Action<MyGuiControlGrid, EventArgs> itemReleased = this.ItemReleased;
                        if (itemReleased != null)
                        {
                            itemReleased(this, args);
                        }
                    }
                }
                this.m_itemClicked = null;
                this.m_lastClick = null;
            }
            if (MyInput.Static.IsAnyNewMouseOrJoystickPressed() & flag)
            {
                this.m_lastClick = new int?(MyGuiManager.TotalTimeInMilliseconds);
                int? mouseOverIndex = new int?(this.ComputeIndex(MyGuiManager.MouseCursorPosition));
                if (!this.IsValidIndex(mouseOverIndex.Value))
                {
                    mouseOverIndex = null;
                }
                this.SelectMouseOverItem(mouseOverIndex);
                captureInput = this;
                if ((this.SelectedIndex != null) && ((this.ItemClicked != null) || (this.ItemClickedWithoutDoubleClick != null)))
                {
                    EventArgs args2;
                    MySharedButtonsEnum none = MySharedButtonsEnum.None;
                    if (MyInput.Static.IsNewPrimaryButtonPressed())
                    {
                        none = MySharedButtonsEnum.Primary;
                    }
                    else if (MyInput.Static.IsNewSecondaryButtonPressed())
                    {
                        none = MySharedButtonsEnum.Secondary;
                    }
                    this.MakeEventArgs(out args2, this.SelectedIndex.Value, none);
                    Action<MyGuiControlGrid, EventArgs> itemClicked = this.ItemClicked;
                    if (itemClicked != null)
                    {
                        itemClicked(this, args2);
                    }
                    this.m_singleClickEvents = new EventArgs?(args2);
                    this.m_itemClicked = new EventArgs?(args2);
                    if (MyInput.Static.IsAnyCtrlKeyPressed() || MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        MyGuiSoundManager.PlaySound(GuiSounds.Item);
                    }
                }
            }
            if (MyInput.Static.IsNewPrimaryButtonPressed() & flag)
            {
                if (this.m_doubleClickStarted == null)
                {
                    this.m_doubleClickStarted = new int?(MyGuiManager.TotalTimeInMilliseconds);
                    this.m_doubleClickFirstPosition = MyGuiManager.MouseCursorPosition;
                }
                else if (((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) <= 500f) && ((this.m_doubleClickFirstPosition - MyGuiManager.MouseCursorPosition).Length() <= 0.005f))
                {
                    if (((this.SelectedIndex != null) && (this.TryGetItemAt(this.SelectedIndex.Value) != null)) && (this.ItemDoubleClicked != null))
                    {
                        EventArgs args3;
                        this.m_singleClickEvents = null;
                        this.MakeEventArgs(out args3, this.SelectedIndex.Value, MySharedButtonsEnum.Primary);
                        this.ItemDoubleClicked(this, args3);
                        MyGuiSoundManager.PlaySound(GuiSounds.Item);
                    }
                    this.m_doubleClickStarted = null;
                    captureInput = this;
                }
            }
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlGrid grid = (MyObjectBuilder_GuiControlGrid) objectBuilder;
            this.VisualStyle = grid.VisualStyle;
            this.RowsCount = grid.DisplayRowsCount;
            this.ColumnsCount = grid.DisplayColumnsCount;
        }

        private bool IsValidCellIndex(int itemIndex) => 
            ((0 <= itemIndex) && (itemIndex < this.m_maxItemCount));

        public bool IsValidIndex(int index) => 
            ((((this.ModalItems == null) || (this.ModalItems.Count <= 0)) || this.ModalItems.ContainsKey(index)) && ((0 <= index) && ((index < this.m_items.Count) && (index < this.m_maxItemCount))));

        public bool IsValidIndex(int row, int col) => 
            this.IsValidIndex(this.ComputeIndex(row, col));

        private void MakeEventArgs(out EventArgs args, int itemIndex, MySharedButtonsEnum button)
        {
            args.ItemIndex = itemIndex;
            args.RowIndex = this.ComputeRow(itemIndex);
            args.ColumnIndex = this.ComputeColumn(itemIndex);
            args.Button = button;
        }

        protected override void OnColorMaskChanged()
        {
            base.OnColorMaskChanged();
            this.ItemBackgroundColorMask = base.ColorMask;
        }

        private void PrepareEventArgs(ref EventArgs args, int itemIndex, int? rowIdx = new int?(), int? columnIdx = new int?())
        {
            args.ItemIndex = itemIndex;
            int? nullable = columnIdx;
            args.ColumnIndex = (nullable != null) ? nullable.GetValueOrDefault() : this.ComputeColumn(itemIndex);
            nullable = rowIdx;
            args.RowIndex = (nullable != null) ? nullable.GetValueOrDefault() : this.ComputeRow(itemIndex);
        }

        public void RecalculateRowsCount()
        {
            float num = this.m_items.Count / this.m_columnsCount;
            this.RowsCount = Math.Max(this.RowsCount, (int) num);
        }

        private void RefreshInternals()
        {
            if (this.m_styleDef.FitSizeToItems)
            {
                base.Size = (this.m_styleDef.ContentPadding.SizeChange + this.m_styleDef.ItemMargin.TopLeftOffset) + (this.m_itemStep * new Vector2((float) this.ColumnsCount, (float) this.RowsCount));
            }
            int num = Math.Min(this.m_maxItemCount, this.RowsCount * this.ColumnsCount);
            while (this.m_items.Count < num)
            {
                this.m_items.Add(null);
            }
            this.RefreshItemsRectangle();
        }

        private void RefreshItemsRectangle()
        {
            this.m_itemsRectangle.Position = ((base.GetPositionAbsoluteTopLeft() + this.m_styleDef.BackgroundPaddingSize) + this.m_styleDef.ContentPadding.TopLeftOffset) + this.m_styleDef.ItemMargin.TopLeftOffset;
            this.m_itemsRectangle.Size = this.m_itemStep * new Vector2((float) this.ColumnsCount, (float) this.RowsCount);
        }

        private void RefreshVisualStyle()
        {
            if (this.VisualStyle != MyGuiControlGridStyleEnum.Custom)
            {
                this.m_styleDef = GetVisualStyle(this.VisualStyle);
            }
            base.BackgroundTexture = this.m_styleDef.BackgroundTexture;
            this.ItemSize = this.m_styleDef.ItemTexture.SizeGui;
            this.m_itemStep = this.ItemSize + this.m_styleDef.ItemMargin.MarginStep;
            this.ItemTextScale = this.m_styleDef.ItemTextScale;
            base.BorderEnabled = this.m_styleDef.BorderEnabled;
            base.BorderColor = this.m_styleDef.BorderColor;
            if (!this.m_styleDef.FitSizeToItems)
            {
                Vector2? sizeOverride = this.m_styleDef.SizeOverride;
                this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
            }
            this.RefreshInternals();
        }

        public void SelectLastItem()
        {
            int? nullable1;
            if (this.m_items.Count > 0)
            {
                nullable1 = new int?(this.m_items.Count - 1);
            }
            else
            {
                nullable1 = null;
            }
            this.SelectedIndex = nullable1;
        }

        private void SelectMouseOverItem(int? mouseOverIndex)
        {
            if (!this.SelectionEnabled || (mouseOverIndex == null))
            {
                this.SelectedIndex = null;
            }
            else if (this.EnableSelectEmptyCell)
            {
                this.SelectedIndex = new int?(mouseOverIndex.Value);
            }
            else if (this.TryGetItemAt(mouseOverIndex.Value) != null)
            {
                this.SelectedIndex = new int?(mouseOverIndex.Value);
            }
        }

        public void SetCustomStyleDefinition(MyGuiStyleDefinition styleDef)
        {
            this.m_styleDef = styleDef;
            this.ItemSize = this.m_styleDef.ItemTexture.SizeGui;
            this.m_itemStep = this.ItemSize + this.m_styleDef.ItemMargin.MarginStep;
            this.ItemTextScale = this.m_styleDef.ItemTextScale;
            base.BorderEnabled = this.m_styleDef.BorderEnabled;
            base.BorderColor = this.m_styleDef.BorderColor;
            if (!this.m_styleDef.FitSizeToItems)
            {
                Vector2? sizeOverride = this.m_styleDef.SizeOverride;
                this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
            }
            this.RefreshInternals();
        }

        public void SetEmptyItemToolTip(string toolTip)
        {
            if (toolTip == null)
            {
                this.m_emptyItemToolTip = null;
            }
            else
            {
                this.m_emptyItemToolTip = new MyToolTips(toolTip);
            }
        }

        public void SetItemAt(int index, MyGuiGridItem item)
        {
            this.m_items[index] = item;
            if (this.ItemChanged != null)
            {
                EventArgs args = new EventArgs();
                int? rowIdx = null;
                rowIdx = null;
                this.PrepareEventArgs(ref args, index, rowIdx, rowIdx);
                this.ItemChanged(this, args);
            }
            float num = (index / this.m_columnsCount) + 1;
            this.RowsCount = Math.Max(this.RowsCount, (int) num);
        }

        public void SetItemAt(int rowIdx, int colIdx, MyGuiGridItem item)
        {
            int itemIndex = this.ComputeIndex(rowIdx, colIdx);
            if ((itemIndex >= 0) && (itemIndex < this.m_items.Count))
            {
                this.m_items[itemIndex] = item;
                if (this.ItemChanged != null)
                {
                    EventArgs args = new EventArgs();
                    this.PrepareEventArgs(ref args, itemIndex, new int?(rowIdx), new int?(colIdx));
                    this.ItemChanged(this, args);
                }
                this.RowsCount = Math.Max(this.RowsCount, rowIdx + 1);
            }
        }

        public void SetItemsToDefault()
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                this.m_items[i] = null;
            }
            this.RowsCount = 0;
        }

        public override void ShowToolTip()
        {
            MyToolTips toolTip = base.m_toolTip;
            int itemIdx = this.ComputeIndex(MyGuiManager.MouseCursorPosition);
            if (itemIdx != -1)
            {
                MyGuiGridItem item = this.TryGetItemAt(itemIdx);
                base.m_toolTip = (item == null) ? this.m_emptyItemToolTip : item.ToolTip;
            }
            base.ShowToolTip();
            base.m_toolTip = toolTip;
        }

        public void TrimEmptyItems()
        {
            int index = this.m_items.Count - 1;
            while ((this.m_items.Count > 0) && (this.m_items[index] == null))
            {
                this.m_items.RemoveAt(index);
                index--;
            }
            if (this.SelectedIndex != null)
            {
                int? selectedIndex = this.SelectedIndex;
                if (!this.IsValidIndex(selectedIndex.Value))
                {
                    selectedIndex = null;
                    this.SelectedIndex = selectedIndex;
                }
            }
            float num2 = (index / this.m_columnsCount) + 1;
            this.RowsCount = Math.Max(this.RowsCount, (int) num2);
        }

        private bool TryFindEmptyIndex(out int emptyIdx, int startingRow)
        {
            for (int i = startingRow * this.m_columnsCount; i < this.m_items.Count; i++)
            {
                if (this.m_items[i] == null)
                {
                    emptyIdx = i;
                    return true;
                }
            }
            emptyIdx = 0;
            return false;
        }

        public MyGuiGridItem TryGetItemAt(int itemIdx) => 
            (this.m_items.IsValidIndex<MyGuiGridItem>(itemIdx) ? this.m_items[itemIdx] : null);

        public MyGuiGridItem TryGetItemAt(int rowIdx, int colIdx) => 
            this.TryGetItemAt(this.ComputeIndex(rowIdx, colIdx));

        private void TryTriggerSingleClickEvent()
        {
            if (this.m_singleClickEvents != null)
            {
                if (this.ItemClickedWithoutDoubleClick != null)
                {
                    this.ItemClickedWithoutDoubleClick(this, this.m_singleClickEvents.Value);
                }
                this.m_singleClickEvents = null;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!base.IsMouseOver)
            {
                this.MouseOverIndex = -1;
            }
        }

        public bool EnableSelectEmptyCell { get; set; }

        public List<MyGuiGridItem> Items =>
            this.m_items;

        public Vector2 ItemStep =>
            this.m_itemStep;

        public int ColumnsCount
        {
            get => 
                this.m_columnsCount;
            set
            {
                if (this.m_columnsCount != value)
                {
                    this.m_columnsCount = value;
                    this.RefreshInternals();
                }
            }
        }

        public int RowsCount
        {
            get => 
                this.m_rowsCount;
            set
            {
                if (this.m_rowsCount != value)
                {
                    this.m_rowsCount = value;
                    this.RefreshInternals();
                }
            }
        }

        public int MaxItemCount
        {
            get => 
                this.m_maxItemCount;
            set
            {
                if (this.m_maxItemCount != value)
                {
                    this.m_maxItemCount = value;
                    this.RefreshInternals();
                }
            }
        }

        public Vector2 ItemSize { get; private set; }

        public int MouseOverIndex
        {
            get => 
                this.m_mouseOverIndex;
            private set
            {
                if (value != this.m_mouseOverIndex)
                {
                    this.m_mouseOverIndex = value;
                    if (this.MouseOverIndexChanged != null)
                    {
                        EventArgs args = new EventArgs();
                        int? rowIdx = null;
                        rowIdx = null;
                        this.PrepareEventArgs(ref args, value, rowIdx, rowIdx);
                        this.MouseOverIndexChanged(this, args);
                    }
                }
            }
        }

        public MyGuiGridItem MouseOverItem =>
            this.TryGetItemAt(this.MouseOverIndex);

        public int? SelectedIndex
        {
            get => 
                this.m_selectedIndex;
            set
            {
                try
                {
                    int? selectedIndex = this.m_selectedIndex;
                    int? nullable2 = value;
                    if (!((selectedIndex.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((selectedIndex != null) == (nullable2 != null))))
                    {
                        this.m_selectedIndex = value;
                        if ((value != null) && (this.ItemSelected != null))
                        {
                            EventArgs args;
                            this.MakeEventArgs(out args, value.Value, MySharedButtonsEnum.None);
                            this.ItemSelected(this, args);
                        }
                    }
                }
                finally
                {
                }
            }
        }

        public MyGuiGridItem SelectedItem
        {
            get
            {
                if (this.SelectedIndex == null)
                {
                    return null;
                }
                return this.TryGetItemAt(this.SelectedIndex.Value);
            }
        }

        public MyGuiControlGridStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public float ItemTextScale
        {
            get => 
                this.m_itemTextScale;
            private set
            {
                this.m_itemTextScale = value;
                this.ItemTextScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
            }
        }

        public float ItemTextScaleWithLanguage
        {
            get => 
                this.m_itemTextScaleWithLanguage;
            private set => 
                (this.m_itemTextScaleWithLanguage = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EventArgs
        {
            public int RowIndex;
            public int ColumnIndex;
            public int ItemIndex;
            public MySharedButtonsEnum Button;
        }
    }
}


namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlListbox))]
    public class MyGuiControlListbox : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlListboxStyleEnum>() + 1];
        private Vector2 m_doubleClickFirstPosition;
        private int? m_doubleClickStarted;
        private RectangleF m_itemsRectangle;
        private Item m_mouseOverItem;
        private StyleDefinition m_styleDef;
        private StyleDefinition m_customStyle;
        private bool m_useCustomStyle;
        private MyVScrollbar m_scrollBar;
        private int m_visibleRowIndexOffset;
        private int m_visibleRows;
        public readonly ObservableCollection<Item> Items;
        public List<Item> SelectedItems;
        private MyGuiControlListboxStyleEnum m_visualStyle;
        public bool MultiSelect;
        [CompilerGenerated]
        private Action<MyGuiControlListbox> ItemClicked;
        [CompilerGenerated]
        private Action<MyGuiControlListbox> ItemDoubleClicked;
        [CompilerGenerated]
        private Action<MyGuiControlListbox> ItemsSelected;
        [CompilerGenerated]
        private Action<MyGuiControlListbox> ItemMouseOver;
        private List<Item> m_StoredSelectedItems;
        private int m_StoredTopmostSelectedPosition;
        private Item m_StoredTopmostSelectedItem;
        private Item m_StoredMouseOverItem;
        private int m_StoredMouseOverPosition;
        private Item m_StoredItemOnTop;
        private float m_StoredScrollbarValue;

        public event Action<MyGuiControlListbox> ItemClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlListbox> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlListbox> a = itemClicked;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Combine(a, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemClicked, action3, a);
                    if (ReferenceEquals(itemClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlListbox> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlListbox> source = itemClicked;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Remove(source, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemClicked, action3, source);
                    if (ReferenceEquals(itemClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlListbox> ItemDoubleClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlListbox> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlListbox> a = itemDoubleClicked;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Combine(a, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemDoubleClicked, action3, a);
                    if (ReferenceEquals(itemDoubleClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlListbox> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlListbox> source = itemDoubleClicked;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Remove(source, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemDoubleClicked, action3, source);
                    if (ReferenceEquals(itemDoubleClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlListbox> ItemMouseOver
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlListbox> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<MyGuiControlListbox> a = itemMouseOver;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Combine(a, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemMouseOver, action3, a);
                    if (ReferenceEquals(itemMouseOver, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlListbox> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<MyGuiControlListbox> source = itemMouseOver;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Remove(source, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemMouseOver, action3, source);
                    if (ReferenceEquals(itemMouseOver, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlListbox> ItemsSelected
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlListbox> itemsSelected = this.ItemsSelected;
                while (true)
                {
                    Action<MyGuiControlListbox> a = itemsSelected;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Combine(a, value);
                    itemsSelected = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemsSelected, action3, a);
                    if (ReferenceEquals(itemsSelected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlListbox> itemsSelected = this.ItemsSelected;
                while (true)
                {
                    Action<MyGuiControlListbox> source = itemsSelected;
                    Action<MyGuiControlListbox> action3 = (Action<MyGuiControlListbox>) Delegate.Remove(source, value);
                    itemsSelected = Interlocked.CompareExchange<Action<MyGuiControlListbox>>(ref this.ItemsSelected, action3, source);
                    if (ReferenceEquals(itemsSelected, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlListbox()
        {
            SetupStyles();
        }

        public MyGuiControlListbox() : this(nullable, MyGuiControlListboxStyleEnum.Default)
        {
        }

        public MyGuiControlListbox(Vector2? position = new Vector2?(), MyGuiControlListboxStyleEnum visualStyle = 0) : base(position, nullable, nullable2, null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.SelectedItems = new List<Item>();
            this.m_StoredSelectedItems = new List<Item>();
            SetupStyles();
            this.m_scrollBar = new MyVScrollbar(this);
            this.m_scrollBar.ValueChanged += new Action<MyScrollbar>(this.verticalScrollBar_ValueChanged);
            this.Items = new ObservableCollection<Item>();
            this.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Items_CollectionChanged);
            this.VisualStyle = visualStyle;
            base.Name = "Listbox";
            this.MultiSelect = true;
        }

        public void Add(Item item, int? position = new int?())
        {
            item.OnVisibleChanged += new Action(this.item_OnVisibleChanged);
            if (position != null)
            {
                this.Items.Insert(position.Value, item);
            }
            else
            {
                this.Items.Add(item);
            }
        }

        public void ApplyStyle(StyleDefinition style)
        {
            this.m_useCustomStyle = true;
            this.m_customStyle = style;
            this.RefreshVisualStyle();
        }

        public void ChangeSelection(List<bool> states)
        {
            this.SelectedItems.Clear();
            int num = 0;
            foreach (Item item in this.Items)
            {
                if (num >= states.Count)
                {
                    break;
                }
                if (states[num])
                {
                    this.SelectedItems.Add(item);
                }
                num++;
            }
            if (this.ItemsSelected != null)
            {
                this.ItemsSelected(this);
            }
        }

        public void ClearItems()
        {
            this.Items.Clear();
        }

        private bool CompareItems(Item item1, Item item2, bool compareUserData, bool compareText)
        {
            if (compareUserData & compareText)
            {
                return ((item1.UserData == item2.UserData) && (item1.Text.CompareTo(item2.Text) == 0));
            }
            if (!compareUserData || (item1.UserData != item2.UserData))
            {
                return (compareText && (item1.Text.CompareTo(item2.Text) == 0));
            }
            return true;
        }

        private int ComputeIndexFromPosition(Vector2 position)
        {
            int num = ((int) ((position.Y - this.m_itemsRectangle.Position.Y) / this.ItemSize.Y)) + 1;
            int num2 = 0;
            for (int i = this.m_visibleRowIndexOffset; i < this.Items.Count; i++)
            {
                if (this.Items[i].Visible)
                {
                    num2++;
                }
                if (num2 == num)
                {
                    return i;
                }
            }
            return -1;
        }

        private float ComputeVariableItemWidth()
        {
            float num = 0.015f;
            int length = 0;
            foreach (Item item in this.Items)
            {
                if (item.Text.Length > length)
                {
                    length = item.Text.Length;
                }
            }
            return (length * num);
        }

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_itemsRectangle.Position, this.m_itemsRectangle.Size, Color.White, 1);
            this.m_scrollBar.DebugDraw();
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            this.m_styleDef.Texture.Draw(positionAbsoluteTopLeft, base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, backgroundTransitionAlpha), 1f);
            Vector2 normalizedCoord = positionAbsoluteTopLeft + new Vector2(this.m_itemsRectangle.X, this.m_itemsRectangle.Y);
            int visibleRowIndexOffset = this.m_visibleRowIndexOffset;
            Vector2 zero = Vector2.Zero;
            Vector2 vector4 = Vector2.Zero;
            if (this.ShouldDrawIconSpacing())
            {
                zero = MyGuiConstants.LISTBOX_ICON_SIZE;
                vector4 = MyGuiConstants.LISTBOX_ICON_OFFSET;
            }
            int num2 = 0;
            goto TR_0016;
        TR_0003:
            num2++;
        TR_0016:
            while (true)
            {
                if (num2 >= this.VisibleRowsCount)
                {
                    break;
                }
                int num3 = num2 + this.m_visibleRowIndexOffset;
                if (num3 >= this.Items.Count)
                {
                    break;
                }
                if (num3 >= 0)
                {
                    while (true)
                    {
                        if ((visibleRowIndexOffset < this.Items.Count) && !this.Items[visibleRowIndexOffset].Visible)
                        {
                            visibleRowIndexOffset++;
                            continue;
                        }
                        if (visibleRowIndexOffset >= this.Items.Count)
                        {
                            break;
                        }
                        Item item = this.Items[visibleRowIndexOffset];
                        visibleRowIndexOffset++;
                        if (item != null)
                        {
                            Color color = ApplyColorMaskModifiers(item.ColorMask * base.ColorMask, base.Enabled, transitionAlpha);
                            bool flag = this.SelectedItems.Contains(item) || ReferenceEquals(item, this.m_mouseOverItem);
                            if (flag)
                            {
                                Vector2 itemSize = this.ItemSize;
                                MyGuiManager.DrawSpriteBatch(this.m_styleDef.ItemTextureHighlight, normalizedCoord, itemSize, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", normalizedCoord, new Vector2(0.003f, itemSize.Y), new Color(0xe1, 230, 0xec), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                            }
                            if (!string.IsNullOrEmpty(item.Icon))
                            {
                                MyGuiManager.DrawSpriteBatch(item.Icon, normalizedCoord + vector4, zero, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                            }
                            MyGuiManager.DrawString(item.FontOverride ?? (flag ? this.m_styleDef.ItemFontHighlight : this.m_styleDef.ItemFontNormal), item.Text, (normalizedCoord + new Vector2(zero.X + (2f * vector4.X), 0.5f * this.ItemSize.Y)) + new Vector2(this.m_styleDef.TextOffset, 0f), this.TextScale, new Color?(color), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, (this.ItemSize.X - zero.X) - (5f * MyGuiConstants.LISTBOX_ICON_OFFSET.X));
                        }
                        float* singlePtr1 = (float*) ref normalizedCoord.Y;
                        singlePtr1[0] += this.ItemSize.Y;
                        goto TR_0003;
                    }
                    break;
                }
                goto TR_0003;
            }
            if (this.m_styleDef.DrawScroll)
            {
                this.m_scrollBar.Draw(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
                Vector2 positionAbsoluteTopRight = base.GetPositionAbsoluteTopRight();
                float* singlePtr2 = (float*) ref positionAbsoluteTopRight.X;
                singlePtr2[0] -= (this.m_styleDef.ScrollbarMargin.HorizontalSum + this.m_scrollBar.Size.X) + 0.0005f;
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Controls\scrollable_list_line.dds", positionAbsoluteTopRight, new Vector2(0.0012f, base.Size.Y), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            }
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlListbox objectBuilder = (MyObjectBuilder_GuiControlListbox) base.GetObjectBuilder();
            objectBuilder.VisibleRows = this.VisibleRowsCount;
            objectBuilder.VisualStyle = this.VisualStyle;
            return objectBuilder;
        }

        public float GetScrollPosition() => 
            this.m_scrollBar.Value;

        public static StyleDefinition GetVisualStyle(MyGuiControlListboxStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase captureInput = base.HandleInput();
            if (captureInput == null)
            {
                if (!base.Enabled || !base.IsMouseOver)
                {
                    return null;
                }
                if ((this.m_scrollBar != null) && this.m_scrollBar.HandleInput())
                {
                    return this;
                }
                if (this.m_styleDef.PriorityCaptureInput)
                {
                    captureInput = this;
                }
                this.HandleNewMousePress(ref captureInput);
                Vector2 vector = MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft();
                if (!this.m_itemsRectangle.Contains(vector))
                {
                    this.m_mouseOverItem = null;
                }
                else
                {
                    int idx = this.ComputeIndexFromPosition(vector);
                    this.m_mouseOverItem = this.IsValidIndex(idx) ? this.Items[idx] : null;
                    if (this.ItemMouseOver != null)
                    {
                        this.ItemMouseOver(this);
                    }
                    if (this.m_styleDef.PriorityCaptureInput)
                    {
                        captureInput = this;
                    }
                }
                if ((this.m_doubleClickStarted != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) >= 500f))
                {
                    this.m_doubleClickStarted = null;
                }
            }
            return captureInput;
        }

        private void HandleNewMousePress(ref MyGuiControlBase captureInput)
        {
            Vector2 vector = MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft();
            bool flag = this.m_itemsRectangle.Contains(vector);
            if (MyInput.Static.IsAnyNewMouseOrJoystickPressed() & flag)
            {
                int idx = this.ComputeIndexFromPosition(vector);
                if (this.IsValidIndex(idx) && this.Items[idx].Visible)
                {
                    if (this.MultiSelect && MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        if (this.SelectedItems.Contains(this.Items[idx]))
                        {
                            this.SelectedItems.Remove(this.Items[idx]);
                        }
                        else
                        {
                            this.SelectedItems.Add(this.Items[idx]);
                        }
                    }
                    else if (!this.MultiSelect || !MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        this.SelectedItems.Clear();
                        this.SelectedItems.Add(this.Items[idx]);
                    }
                    else
                    {
                        int index = 0;
                        if (this.SelectedItems.Count > 0)
                        {
                            index = this.Items.IndexOf(this.SelectedItems[this.SelectedItems.Count - 1]);
                        }
                        while (true)
                        {
                            index += (index > idx) ? -1 : 1;
                            if (this.IsValidIndex(index))
                            {
                                if (this.Items[index].Visible)
                                {
                                    if (this.SelectedItems.Contains(this.Items[index]))
                                    {
                                        this.SelectedItems.Remove(this.Items[index]);
                                    }
                                    else
                                    {
                                        this.SelectedItems.Add(this.Items[index]);
                                    }
                                }
                                if (index != idx)
                                {
                                    continue;
                                }
                            }
                            break;
                        }
                    }
                    if (this.ItemsSelected != null)
                    {
                        this.ItemsSelected(this);
                    }
                    captureInput = this;
                    if (this.ItemClicked != null)
                    {
                        this.ItemClicked(this);
                        MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    }
                }
            }
            if (MyInput.Static.IsNewPrimaryButtonPressed() & flag)
            {
                if (this.m_doubleClickStarted == null)
                {
                    int idx = this.ComputeIndexFromPosition(vector);
                    if (this.IsValidIndex(idx) && this.Items[idx].Visible)
                    {
                        this.m_doubleClickStarted = new int?(MyGuiManager.TotalTimeInMilliseconds);
                        this.m_doubleClickFirstPosition = MyGuiManager.MouseCursorPosition;
                    }
                }
                else if (((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) <= 500f) && ((this.m_doubleClickFirstPosition - MyGuiManager.MouseCursorPosition).Length() <= 0.005f))
                {
                    if (this.ItemDoubleClicked != null)
                    {
                        this.ItemDoubleClicked(this);
                    }
                    this.m_doubleClickStarted = null;
                    captureInput = this;
                }
            }
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlListbox listbox = (MyObjectBuilder_GuiControlListbox) objectBuilder;
            this.VisibleRowsCount = listbox.VisibleRows;
            this.VisualStyle = listbox.VisualStyle;
        }

        public bool IsOverScrollBar() => 
            this.m_scrollBar.IsOverCaret;

        private bool IsValidIndex(int idx) => 
            ((0 <= idx) && (idx < this.Items.Count));

        private void item_OnVisibleChanged()
        {
            this.RefreshScrollBar();
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Remove) || (e.Action == NotifyCollectionChangedAction.Replace))
            {
                foreach (object obj2 in e.OldItems)
                {
                    if (this.SelectedItems.Contains((Item) obj2))
                    {
                        this.SelectedItems.Remove((Item) obj2);
                    }
                }
                if (this.ItemsSelected != null)
                {
                    this.ItemsSelected(this);
                }
            }
            this.RefreshScrollBar();
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.m_scrollBar.HasHighlight = base.HasHighlight;
            this.m_mouseOverItem = null;
        }

        protected override void OnOriginAlignChanged()
        {
            base.OnOriginAlignChanged();
            this.RefreshInternals();
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.RefreshInternals();
        }

        public override void OnRemoving()
        {
            this.Items.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Items_CollectionChanged);
            this.Items.Clear();
            this.ItemClicked = null;
            this.ItemDoubleClicked = null;
            this.ItemsSelected = null;
            base.OnRemoving();
        }

        private void RefreshInternals()
        {
            Vector2 minSizeGui = this.m_styleDef.Texture.MinSizeGui;
            Vector2 maxSizeGui = this.m_styleDef.Texture.MaxSizeGui;
            if (this.m_styleDef.XSizeVariable)
            {
                this.ItemSize = new Vector2(this.ComputeVariableItemWidth(), this.ItemSize.Y);
            }
            if (!this.m_styleDef.DrawScroll || this.m_styleDef.XSizeVariable)
            {
                base.Size = Vector2.Clamp(new Vector2(this.m_styleDef.TextOffset + this.ItemSize.X, minSizeGui.Y + (this.ItemSize.Y * this.VisibleRowsCount)), minSizeGui, maxSizeGui);
            }
            else
            {
                base.Size = Vector2.Clamp(new Vector2(((this.m_styleDef.TextOffset + this.m_styleDef.ScrollbarMargin.HorizontalSum) + this.m_styleDef.ItemSize.X) + this.m_scrollBar.Size.X, minSizeGui.Y + (this.m_styleDef.ItemSize.Y * this.VisibleRowsCount)), minSizeGui, maxSizeGui);
            }
            this.RefreshScrollBar();
            this.m_itemsRectangle.X = this.m_styleDef.ItemsOffset.X;
            this.m_itemsRectangle.Y = this.m_styleDef.ItemsOffset.Y + this.m_styleDef.Texture.LeftTop.SizeGui.Y;
            this.m_itemsRectangle.Width = this.ItemSize.X;
            this.m_itemsRectangle.Height = this.ItemSize.Y * this.VisibleRowsCount;
        }

        private void RefreshScrollBar()
        {
            int num = 0;
            using (ObservableCollection<Item>.Enumerator enumerator = this.Items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.Visible)
                    {
                        continue;
                    }
                    num++;
                }
            }
            this.m_scrollBar.Visible = num > this.VisibleRowsCount;
            this.m_scrollBar.Init((float) num, (float) this.VisibleRowsCount);
            Vector2 vector = base.Size * new Vector2(0.5f, -0.5f);
            MyGuiBorderThickness scrollbarMargin = this.m_styleDef.ScrollbarMargin;
            Vector2 position = new Vector2(vector.X - (scrollbarMargin.Right + this.m_scrollBar.Size.X), vector.Y + scrollbarMargin.Top);
            this.m_scrollBar.Layout(position, base.Size.Y - scrollbarMargin.VerticalSum);
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = !this.m_useCustomStyle ? GetVisualStyle(this.VisualStyle) : this.m_customStyle;
            this.ItemSize = this.m_styleDef.ItemSize;
            this.TextScale = this.m_styleDef.TextScale;
            this.RefreshInternals();
        }

        public void Remove(Predicate<Item> match)
        {
            int index = this.Items.FindIndex(match);
            if (index != -1)
            {
                this.Items.RemoveAt(index);
            }
        }

        public void RestoreSituation(bool compareUserData, bool compareText)
        {
            this.SelectedItems.Clear();
            foreach (Item item in this.m_StoredSelectedItems)
            {
                foreach (Item item2 in this.Items)
                {
                    if (this.CompareItems(item, item2, compareUserData, compareText))
                    {
                        this.SelectedItems.Add(item2);
                        break;
                    }
                }
            }
            int num = -1;
            int num2 = -1;
            int num3 = 0;
            using (ObservableCollection<Item>.Enumerator enumerator2 = this.Items.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator2.MoveNext())
                    {
                        break;
                    }
                    Item current = enumerator2.Current;
                    if ((this.m_StoredMouseOverItem == null) || !this.CompareItems(current, this.m_StoredMouseOverItem, compareUserData, compareText))
                    {
                        if ((this.m_StoredTopmostSelectedItem != null) && this.CompareItems(current, this.m_StoredTopmostSelectedItem, compareUserData, compareText))
                        {
                            num = num3;
                        }
                        if ((this.m_StoredItemOnTop != null) && this.CompareItems(current, this.m_StoredItemOnTop, compareUserData, compareText))
                        {
                            num2 = num3;
                        }
                        num3++;
                        continue;
                    }
                    this.m_scrollBar.Value = (this.m_StoredScrollbarValue + num3) - this.m_StoredMouseOverPosition;
                    return;
                }
            }
            if (this.m_StoredTopmostSelectedPosition != this.m_visibleRows)
            {
                this.m_scrollBar.Value = (this.m_StoredScrollbarValue + num) - this.m_StoredTopmostSelectedPosition;
            }
            else if (num2 != -1)
            {
                this.m_scrollBar.Value = num2;
            }
            else
            {
                this.m_scrollBar.Value = this.m_StoredScrollbarValue;
            }
        }

        public void ScrollToFirstSelection()
        {
            if ((this.Items.Count != 0) && (this.SelectedItems.Count != 0))
            {
                Item objA = this.SelectedItems[0];
                int num = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.Items.Count)
                    {
                        Item objB = this.Items[num2];
                        if (!ReferenceEquals(objA, objB))
                        {
                            num2++;
                            continue;
                        }
                        num = num2;
                    }
                    if ((this.m_visibleRowIndexOffset > num) || (num >= (this.m_visibleRowIndexOffset + this.m_visibleRows)))
                    {
                        this.SetScrollPosition((float) num);
                    }
                    return;
                }
            }
        }

        public void ScrollToolbarToTop()
        {
            this.m_scrollBar.SetPage(0f);
        }

        public void SelectAllVisible()
        {
            this.SelectedItems.Clear();
            foreach (Item item in this.Items)
            {
                if (item.Visible)
                {
                    this.SelectedItems.Add(item);
                }
            }
            if (this.ItemsSelected != null)
            {
                this.ItemsSelected(this);
            }
        }

        public bool SelectByUserData(object data)
        {
            bool flag = false;
            if (this.SelectedItems == null)
            {
                this.SelectedItems = new List<Item>();
            }
            foreach (Item item in this.Items)
            {
                if (item.UserData == data)
                {
                    flag = true;
                    this.SelectedItems.Add(item);
                }
            }
            return flag;
        }

        public void SetScrollPosition(float position)
        {
            this.m_scrollBar.Value = position;
        }

        private static void SetupStyles()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition1.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition1.ItemFontNormal = "Blue";
            definition1.ItemFontHighlight = "White";
            definition1.ItemSize = new Vector2(0.25f, 0.034f);
            definition1.TextScale = 0.8f;
            definition1.TextOffset = 0.006f;
            definition1.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition1.DrawScroll = true;
            definition1.PriorityCaptureInput = false;
            definition1.XSizeVariable = false;
            MyGuiBorderThickness thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition1.ScrollbarMargin = thickness;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition2.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition2.ItemFontNormal = "Blue";
            definition2.ItemFontHighlight = "White";
            definition2.ItemSize = new Vector2(0.2535f, 0.034f);
            definition2.TextScale = 0.8f;
            definition2.TextOffset = 0.006f;
            definition2.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition2.DrawScroll = true;
            definition2.PriorityCaptureInput = false;
            definition2.XSizeVariable = false;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition2.ScrollbarMargin = thickness;
            m_styles[1] = definition2;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.Texture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition3.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition3.ItemFontNormal = "Blue";
            definition3.ItemFontHighlight = "White";
            definition3.ItemSize = new Vector2(0.25f, 0.035f);
            definition3.TextScale = 0.8f;
            definition3.TextOffset = 0.004f;
            definition3.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition3.DrawScroll = true;
            definition3.PriorityCaptureInput = true;
            definition3.XSizeVariable = true;
            thickness = new MyGuiBorderThickness {
                Left = 0f,
                Right = 0f,
                Top = 0f,
                Bottom = 0f
            };
            definition3.ScrollbarMargin = thickness;
            m_styles[2] = definition3;
            StyleDefinition definition4 = new StyleDefinition();
            definition4.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition4.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition4.ItemFontNormal = "Blue";
            definition4.ItemFontHighlight = "White";
            definition4.ItemSize = new Vector2(0.25f, 0.035f);
            definition4.TextScale = 0.8f;
            definition4.TextOffset = 0.006f;
            definition4.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition4.DrawScroll = true;
            definition4.PriorityCaptureInput = false;
            definition4.XSizeVariable = false;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition4.ScrollbarMargin = thickness;
            m_styles[3] = definition4;
            StyleDefinition definition5 = new StyleDefinition();
            definition5.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST_TOOLS_BLOCKS;
            definition5.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition5.ItemFontNormal = "Blue";
            definition5.ItemFontHighlight = "White";
            definition5.ItemSize = new Vector2(0.15f, 0.0272f);
            definition5.TextScale = 0.78f;
            definition5.TextOffset = 0.006f;
            definition5.ItemsOffset = new Vector2(6f, 6f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition5.DrawScroll = true;
            definition5.PriorityCaptureInput = false;
            definition5.XSizeVariable = false;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition5.ScrollbarMargin = thickness;
            m_styles[4] = definition5;
            StyleDefinition definition6 = new StyleDefinition();
            definition6.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition6.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition6.ItemFontNormal = "Blue";
            definition6.ItemFontHighlight = "White";
            definition6.ItemSize = new Vector2(0.21f, 0.025f);
            definition6.TextScale = 0.8f;
            definition6.TextOffset = 0.006f;
            definition6.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition6.DrawScroll = true;
            definition6.PriorityCaptureInput = false;
            definition6.XSizeVariable = false;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition6.ScrollbarMargin = thickness;
            m_styles[5] = definition6;
            StyleDefinition definition7 = new StyleDefinition();
            definition7.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition7.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition7.ItemFontNormal = "Blue";
            definition7.ItemFontHighlight = "White";
            definition7.ItemSize = new Vector2(0.231f, 0.035f);
            definition7.TextScale = 0.8f;
            definition7.TextOffset = 0.006f;
            definition7.ItemsOffset = new Vector2(6f, 2f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            definition7.DrawScroll = true;
            definition7.PriorityCaptureInput = false;
            definition7.XSizeVariable = false;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition7.ScrollbarMargin = thickness;
            m_styles[6] = definition7;
        }

        private bool ShouldDrawIconSpacing()
        {
            int visibleRowIndexOffset = this.m_visibleRowIndexOffset;
            int num2 = 0;
            goto TR_000F;
        TR_0001:
            num2++;
        TR_000F:
            while (true)
            {
                if (num2 >= this.VisibleRowsCount)
                {
                    break;
                }
                int num3 = num2 + this.m_visibleRowIndexOffset;
                if (num3 >= this.Items.Count)
                {
                    break;
                }
                if (num3 >= 0)
                {
                    while (true)
                    {
                        if ((visibleRowIndexOffset < this.Items.Count) && !this.Items[visibleRowIndexOffset].Visible)
                        {
                            visibleRowIndexOffset++;
                            continue;
                        }
                        if (visibleRowIndexOffset >= this.Items.Count)
                        {
                            break;
                        }
                        Item item = this.Items[visibleRowIndexOffset];
                        visibleRowIndexOffset++;
                        if ((item != null) && !string.IsNullOrEmpty(item.Icon))
                        {
                            return true;
                        }
                        goto TR_0001;
                    }
                    break;
                }
                goto TR_0001;
            }
            return false;
        }

        public override void ShowToolTip()
        {
            if (((this.m_mouseOverItem == null) || (this.m_mouseOverItem.ToolTip == null)) || (this.m_mouseOverItem.ToolTip.ToolTips.Count <= 0))
            {
                base.m_toolTip = null;
            }
            else
            {
                base.m_toolTip = this.m_mouseOverItem.ToolTip;
            }
            base.ShowToolTip();
        }

        public void StoreSituation()
        {
            this.m_StoredSelectedItems.Clear();
            this.m_StoredTopmostSelectedItem = null;
            this.m_StoredMouseOverItem = null;
            this.m_StoredItemOnTop = null;
            this.m_StoredTopmostSelectedPosition = this.m_visibleRows;
            foreach (Item item in this.SelectedItems)
            {
                this.m_StoredSelectedItems.Add(item);
                int index = this.Items.IndexOf(this.SelectedItems[0]);
                if ((index < this.m_StoredTopmostSelectedPosition) && (index >= this.m_visibleRowIndexOffset))
                {
                    this.m_StoredTopmostSelectedPosition = index;
                    this.m_StoredTopmostSelectedItem = item;
                }
            }
            this.m_StoredMouseOverItem = this.m_mouseOverItem;
            int num = 0;
            if (this.m_mouseOverItem != null)
            {
                foreach (Item item2 in this.Items)
                {
                    if (ReferenceEquals(this.m_mouseOverItem, item2))
                    {
                        this.m_StoredMouseOverPosition = num;
                        break;
                    }
                    num++;
                }
            }
            if (this.FirstVisibleRow < this.Items.Count)
            {
                this.m_StoredItemOnTop = this.Items[this.FirstVisibleRow];
            }
            this.m_StoredScrollbarValue = this.m_scrollBar.Value;
        }

        private void verticalScrollBar_ValueChanged(MyScrollbar scrollbar)
        {
            int num = (int) scrollbar.Value;
            int num2 = -1;
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i].Visible)
                {
                    num2++;
                }
                if (num2 == num)
                {
                    num = i;
                    break;
                }
            }
            this.m_visibleRowIndexOffset = num;
        }

        public Item MouseOverItem =>
            this.m_mouseOverItem;

        public Vector2 ItemSize { get; set; }

        public float TextScale { get; set; }

        public int VisibleRowsCount
        {
            get => 
                this.m_visibleRows;
            set
            {
                this.m_visibleRows = value;
                this.RefreshInternals();
            }
        }

        public int FirstVisibleRow
        {
            get => 
                this.m_visibleRowIndexOffset;
            set => 
                (this.m_scrollBar.Value = value);
        }

        public MyGuiControlListboxStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public Item SelectedItem
        {
            get
            {
                if (this.SelectedItems == null)
                {
                    this.SelectedItems = new List<Item>();
                }
                return ((this.SelectedItems.Count != 0) ? this.SelectedItems[this.SelectedItems.Count - 1] : null);
            }
            set
            {
                if (this.SelectedItems == null)
                {
                    this.SelectedItems = new List<Item>();
                }
                this.SelectedItems.Clear();
                this.SelectedItems.Add(value);
            }
        }

        public class Item
        {
            private bool m_visible;
            [CompilerGenerated]
            private Action OnVisibleChanged;
            public readonly string Icon;
            public readonly MyToolTips ToolTip;
            public readonly object UserData;
            public string FontOverride;
            public Vector4 ColorMask;

            public event Action OnVisibleChanged
            {
                [CompilerGenerated] add
                {
                    Action onVisibleChanged = this.OnVisibleChanged;
                    while (true)
                    {
                        Action a = onVisibleChanged;
                        Action action3 = (Action) Delegate.Combine(a, value);
                        onVisibleChanged = Interlocked.CompareExchange<Action>(ref this.OnVisibleChanged, action3, a);
                        if (ReferenceEquals(onVisibleChanged, a))
                        {
                            return;
                        }
                    }
                }
                [CompilerGenerated] remove
                {
                    Action onVisibleChanged = this.OnVisibleChanged;
                    while (true)
                    {
                        Action source = onVisibleChanged;
                        Action action3 = (Action) Delegate.Remove(source, value);
                        onVisibleChanged = Interlocked.CompareExchange<Action>(ref this.OnVisibleChanged, action3, source);
                        if (ReferenceEquals(onVisibleChanged, source))
                        {
                            return;
                        }
                    }
                }
            }

            public Item(StringBuilder text = null, string toolTip = null, string icon = null, object userData = null, string fontOverride = null)
            {
                this.ColorMask = Vector4.One;
                this.Text = new StringBuilder((text != null) ? text.ToString() : "");
                this.ToolTip = (toolTip != null) ? new MyToolTips(toolTip) : null;
                this.Icon = icon;
                this.UserData = userData;
                this.FontOverride = fontOverride;
                this.Visible = true;
            }

            public Item(ref StringBuilder text, string toolTip = null, string icon = null, object userData = null, string fontOverride = null)
            {
                this.ColorMask = Vector4.One;
                this.Text = text;
                this.ToolTip = (toolTip != null) ? new MyToolTips(toolTip) : null;
                this.Icon = icon;
                this.UserData = userData;
                this.FontOverride = fontOverride;
                this.Visible = true;
            }

            public StringBuilder Text { get; set; }

            public bool Visible
            {
                get => 
                    this.m_visible;
                set
                {
                    if (this.m_visible != value)
                    {
                        this.m_visible = value;
                        if (this.OnVisibleChanged != null)
                        {
                            this.OnVisibleChanged();
                        }
                    }
                }
            }
        }

        public class StyleDefinition
        {
            public string ItemFontHighlight;
            public string ItemFontNormal;
            public Vector2 ItemSize;
            public string ItemTextureHighlight;
            public Vector2 ItemsOffset;
            public float TextOffset;
            public bool DrawScroll;
            public bool PriorityCaptureInput;
            public bool XSizeVariable;
            public float TextScale;
            public MyGuiCompositeTexture Texture;
            public MyGuiBorderThickness ScrollbarMargin;

            public MyGuiControlListbox.StyleDefinition CloneShallow()
            {
                MyGuiControlListbox.StyleDefinition definition1 = new MyGuiControlListbox.StyleDefinition();
                definition1.ItemFontHighlight = this.ItemFontHighlight;
                definition1.ItemFontNormal = this.ItemFontNormal;
                definition1.ItemSize = new Vector2(this.ItemSize.X, this.ItemSize.Y);
                definition1.ItemTextureHighlight = this.ItemTextureHighlight;
                definition1.ItemsOffset = new Vector2(this.ItemsOffset.X, this.ItemsOffset.Y);
                definition1.TextOffset = this.TextOffset;
                definition1.DrawScroll = this.DrawScroll;
                definition1.PriorityCaptureInput = this.PriorityCaptureInput;
                definition1.XSizeVariable = this.XSizeVariable;
                definition1.TextScale = this.TextScale;
                definition1.Texture = this.Texture;
                definition1.ScrollbarMargin = this.ScrollbarMargin;
                return definition1;
            }
        }
    }
}


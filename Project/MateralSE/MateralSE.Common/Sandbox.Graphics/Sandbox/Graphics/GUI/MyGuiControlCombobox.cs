namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlCombobox))]
    public class MyGuiControlCombobox : MyGuiControlBase
    {
        private const float ITEM_HEIGHT = 0.03f;
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlComboboxStyleEnum>() + 1];
        [CompilerGenerated]
        private ItemSelectedDelegate ItemSelected;
        private bool m_isOpen;
        private bool m_scrollBarDragging;
        private List<Item> m_items;
        private Item m_selected;
        private Item m_preselectedMouseOver;
        private Item m_preselectedMouseOverPrevious;
        private int? m_preselectedKeyboardIndex;
        private int? m_preselectedKeyboardIndexPrevious;
        private int? m_mouseWheelValueLast;
        private int m_openAreaItemsCount;
        private int m_middleIndex;
        private bool m_showScrollBar;
        private float m_scrollBarCurrentPosition;
        private float m_scrollBarCurrentNonadjustedPosition;
        private float m_mouseOldPosition;
        private bool m_mousePositionReinit;
        private float m_maxScrollBarPosition;
        private float m_scrollBarEndPositionRelative;
        private int m_displayItemsStartIndex;
        private int m_displayItemsEndIndex;
        private int m_scrollBarItemOffSet;
        private float m_scrollBarHeight;
        private float m_scrollBarWidth;
        private float m_comboboxItemDeltaHeight;
        private float m_scrollRatio;
        private Vector2 m_dropDownItemSize;
        private const float ITEM_DRAW_DELTA = 0.0001f;
        private bool m_useScrollBarOffset;
        private MyGuiControlComboboxStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private RectangleF m_selectedItemArea;
        private RectangleF m_openedArea;
        private RectangleF m_openedItemArea;
        private string m_selectedItemFont;
        private MyGuiCompositeTexture m_scrollbarTexture;
        private Vector4 m_textColor;
        private float m_textScaleWithLanguage;
        private bool m_isFlipped;

        public event ItemSelectedDelegate ItemSelected
        {
            [CompilerGenerated] add
            {
                ItemSelectedDelegate itemSelected = this.ItemSelected;
                while (true)
                {
                    ItemSelectedDelegate a = itemSelected;
                    ItemSelectedDelegate delegate4 = (ItemSelectedDelegate) Delegate.Combine(a, value);
                    itemSelected = Interlocked.CompareExchange<ItemSelectedDelegate>(ref this.ItemSelected, delegate4, a);
                    if (ReferenceEquals(itemSelected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ItemSelectedDelegate itemSelected = this.ItemSelected;
                while (true)
                {
                    ItemSelectedDelegate source = itemSelected;
                    ItemSelectedDelegate delegate4 = (ItemSelectedDelegate) Delegate.Remove(source, value);
                    itemSelected = Interlocked.CompareExchange<ItemSelectedDelegate>(ref this.ItemSelected, delegate4, source);
                    if (ReferenceEquals(itemSelected, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlCombobox()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.DropDownTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST_BORDER;
            definition1.ComboboxTextureNormal = MyGuiConstants.TEXTURE_COMBOBOX_NORMAL;
            definition1.ComboboxTextureHighlight = MyGuiConstants.TEXTURE_COMBOBOX_HIGHLIGHT;
            definition1.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition1.ItemFontNormal = "Blue";
            definition1.ItemFontHighlight = "White";
            definition1.SelectedItemOffset = new Vector2(0.01f, 0.005f);
            definition1.TextScale = 0.72f;
            definition1.DropDownHighlightExtraWidth = 0.007f;
            MyGuiBorderThickness thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition1.ScrollbarMargin = thickness;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.DropDownTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition2.ComboboxTextureNormal = MyGuiConstants.TEXTURE_COMBOBOX_NORMAL;
            definition2.ComboboxTextureHighlight = MyGuiConstants.TEXTURE_COMBOBOX_HIGHLIGHT;
            definition2.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition2.ItemFontNormal = "Debug";
            definition2.ItemFontHighlight = "White";
            definition2.SelectedItemOffset = new Vector2(0.01f, 0.005f);
            definition2.TextScale = 0.72f;
            definition2.DropDownHighlightExtraWidth = 0.007f;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition2.ScrollbarMargin = thickness;
            m_styles[1] = definition2;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.DropDownTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition3.ComboboxTextureNormal = MyGuiConstants.TEXTURE_COMBOBOX_NORMAL;
            definition3.ComboboxTextureHighlight = MyGuiConstants.TEXTURE_COMBOBOX_HIGHLIGHT;
            definition3.ItemTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition3.ItemFontNormal = "Blue";
            definition3.ItemFontHighlight = "White";
            definition3.SelectedItemOffset = new Vector2(0.01f, 0.005f);
            definition3.TextScale = 0.72f;
            definition3.DropDownHighlightExtraWidth = 0.007f;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition3.ScrollbarMargin = thickness;
            m_styles[2] = definition3;
        }

        public MyGuiControlCombobox() : this(nullable, nullable, nullable2, nullable, 10, nullable, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable2)
        {
            Vector2? nullable = null;
            nullable = null;
            Vector4? nullable2 = null;
            nullable = null;
            nullable = null;
            nullable2 = null;
        }

        public MyGuiControlCombobox(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), Vector2? textOffset = new Vector2?(), int openAreaItemsCount = 10, Vector2? iconSize = new Vector2?(), bool useScrollBarOffset = false, string toolTip = null, MyGuiDrawAlignEnum originAlign = 4, Vector4? textColor = new Vector4?()) : this(position, new Vector2?((nullable != null) ? nullable.GetValueOrDefault() : (new Vector2(455f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE)), backgroundColor, toolTip, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
        {
            Vector2? nullable = size;
            base.Name = "Combobox";
            base.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            this.m_items = new List<Item>();
            this.m_isOpen = false;
            this.m_openAreaItemsCount = openAreaItemsCount;
            this.m_middleIndex = Math.Max((this.m_openAreaItemsCount / 2) - 1, 0);
            this.m_textColor = (textColor != null) ? textColor.Value : Vector4.One;
            this.m_dropDownItemSize = this.GetItemSize();
            this.m_comboboxItemDeltaHeight = this.m_dropDownItemSize.Y;
            this.m_mousePositionReinit = true;
            this.RefreshVisualStyle();
            this.InitializeScrollBarParameters();
            base.m_showToolTip = true;
            this.m_useScrollBarOffset = useScrollBarOffset;
        }

        public void AddItem(long key, string value, int? sortOrder = new int?(), string toolTip = null)
        {
            int? nullable = sortOrder;
            sortOrder = new int?((nullable != null) ? nullable.GetValueOrDefault() : this.m_items.Count);
            this.m_items.Add(new Item(key, value, sortOrder.Value, toolTip));
            this.m_items.Sort();
            this.AdjustScrollBarParameters();
            this.RefreshInternals();
        }

        public void AddItem(long key, StringBuilder value, int? sortOrder = new int?(), string toolTip = null)
        {
            int? nullable = sortOrder;
            sortOrder = new int?((nullable != null) ? nullable.GetValueOrDefault() : this.m_items.Count);
            this.m_items.Add(new Item(key, value, sortOrder.Value, toolTip));
            this.m_items.Sort();
            this.AdjustScrollBarParameters();
            this.RefreshInternals();
        }

        public void AddItem(long key, MyStringId value, int? sortOrder = new int?(), MyStringId? toolTip = new MyStringId?())
        {
            this.AddItem(key, MyTexts.Get(value), sortOrder, (toolTip != null) ? MyTexts.GetString(toolTip.Value) : null);
        }

        private void AdjustScrollBarParameters()
        {
            this.m_showScrollBar = this.m_items.Count > this.m_openAreaItemsCount;
            if (this.m_showScrollBar)
            {
                this.m_maxScrollBarPosition = this.m_scrollBarEndPositionRelative - this.m_scrollBarHeight;
                this.m_scrollBarItemOffSet = this.m_items.Count - this.m_openAreaItemsCount;
            }
        }

        public void ApplyStyle(StyleDefinition style)
        {
            if (style != null)
            {
                this.m_styleDef = style;
                this.RefreshInternals();
            }
        }

        private void Assert()
        {
        }

        private void CalculateStartAndEndDisplayItemsIndex()
        {
            this.m_scrollRatio = (this.m_scrollBarCurrentPosition == 0f) ? 0f : ((this.m_scrollBarCurrentPosition * this.m_scrollBarItemOffSet) / this.m_maxScrollBarPosition);
            this.m_displayItemsStartIndex = Math.Max(0, (int) Math.Floor((double) (this.m_scrollRatio + 0.5)));
            this.m_displayItemsEndIndex = Math.Min(this.m_items.Count, this.m_displayItemsStartIndex + this.m_openAreaItemsCount);
        }

        public override bool CheckMouseOver()
        {
            if (this.m_isOpen)
            {
                int num = this.m_showScrollBar ? this.m_openAreaItemsCount : this.m_items.Count;
                for (int i = 0; i < num; i++)
                {
                    Vector2 openItemPosition = this.GetOpenItemPosition(i);
                    MyRectangle2D openedArea = this.GetOpenedArea();
                    Vector2 vector2 = new Vector2(openItemPosition.X, Math.Max(openedArea.LeftTop.Y, openItemPosition.Y));
                    Vector2 vector3 = vector2 + new Vector2(base.Size.X, this.m_comboboxItemDeltaHeight);
                    Vector2 vector4 = MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft();
                    if (((vector4.X >= vector2.X) && ((vector4.X <= vector3.X) && (vector4.Y >= vector2.Y))) && (vector4.Y <= vector3.Y))
                    {
                        return true;
                    }
                }
            }
            return (!this.m_scrollBarDragging ? CheckMouseOver(base.Size, base.GetPositionAbsolute(), base.OriginAlign) : false);
        }

        public void ClearItems()
        {
            this.m_items.Clear();
            this.m_selected = null;
            this.m_preselectedKeyboardIndex = null;
            this.m_preselectedKeyboardIndexPrevious = null;
            this.m_preselectedMouseOver = null;
            this.m_preselectedMouseOverPrevious = null;
            this.InitializeScrollBarParameters();
        }

        public void CustomSortItems(Comparison<Item> comparison)
        {
            if (this.m_items != null)
            {
                this.m_items.Sort(comparison);
            }
        }

        private void DebugDraw()
        {
            base.BorderEnabled = true;
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            MyGuiManager.DrawBorders(positionAbsoluteTopLeft + this.m_selectedItemArea.Position, this.m_selectedItemArea.Size, Color.Cyan, 1);
            if (this.m_isOpen)
            {
                MyGuiManager.DrawBorders(positionAbsoluteTopLeft + this.m_openedArea.Position, this.m_openedArea.Size, Color.GreenYellow, 1);
                MyGuiManager.DrawBorders(positionAbsoluteTopLeft + this.m_openedItemArea.Position, this.m_openedItemArea.Size, Color.Red, 1);
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, transitionAlpha);
            if (this.m_selected != null)
            {
                this.DrawSelectedItemText(transitionAlpha);
            }
            float scrollbarInnerTexturePositionX = (base.GetPositionAbsoluteCenterLeft().X + base.Size.X) - (this.m_scrollBarWidth / 2f);
            int startIndex = 0;
            int count = this.m_items.Count;
            if (this.m_showScrollBar)
            {
                startIndex = this.m_displayItemsStartIndex;
                count = this.m_displayItemsEndIndex;
            }
            if (this.m_isOpen)
            {
                MyRectangle2D openedArea = this.GetOpenedArea();
                this.DrawOpenedAreaItems(startIndex, count, transitionAlpha);
                if (this.m_showScrollBar)
                {
                    this.DrawOpenedAreaScrollbar(scrollbarInnerTexturePositionX, openedArea, transitionAlpha);
                }
            }
        }

        private unsafe void DrawOpenedAreaItems(int startIndex, int endIndex, float transitionAlpha)
        {
            float num = (endIndex - startIndex) * (this.m_comboboxItemDeltaHeight + 0.0001f);
            Vector2 minSizeGui = this.m_styleDef.DropDownTexture.MinSizeGui;
            Vector2 size = Vector2.Clamp(new Vector2(base.Size.X, num + minSizeGui.Y), minSizeGui, this.m_styleDef.DropDownTexture.MaxSizeGui);
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            this.m_styleDef.DropDownTexture.Draw(positionAbsoluteTopLeft + this.m_openedArea.Position, size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), 1f);
            RectangleF openedItemArea = this.m_openedItemArea;
            Vector2* vectorPtr1 = (Vector2*) ref openedItemArea.Position;
            vectorPtr1[0] += positionAbsoluteTopLeft;
            float* singlePtr1 = (float*) ref openedItemArea.Position.X;
            singlePtr1[0] -= this.m_styleDef.DropDownHighlightExtraWidth;
            float* singlePtr2 = (float*) ref openedItemArea.Size.X;
            singlePtr2[0] += this.m_styleDef.DropDownHighlightExtraWidth;
            using (MyGuiManager.UsingScissorRectangle(ref openedItemArea))
            {
                Vector2 vector5 = positionAbsoluteTopLeft + this.m_openedItemArea.Position;
                int num2 = startIndex;
                while (true)
                {
                    Item item;
                    string itemFontNormal;
                    while (true)
                    {
                        if (num2 < endIndex)
                        {
                            item = this.m_items[num2];
                            itemFontNormal = this.m_styleDef.ItemFontNormal;
                            if (!ReferenceEquals(item, this.m_preselectedMouseOver))
                            {
                                if (this.m_preselectedKeyboardIndex == null)
                                {
                                    break;
                                }
                                int? preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                                int num3 = num2;
                                if (!((preselectedKeyboardIndex.GetValueOrDefault() == num3) & (preselectedKeyboardIndex != null)))
                                {
                                    break;
                                }
                            }
                            MyGuiManager.DrawSpriteBatchRoundUp(this.m_styleDef.ItemTextureHighlight, vector5 - new Vector2(this.m_styleDef.DropDownHighlightExtraWidth, 0f), this.m_selectedItemArea.Size + new Vector2(this.m_styleDef.DropDownHighlightExtraWidth, 0f), ApplyColorMaskModifiers(Vector4.One, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true);
                            itemFontNormal = this.m_styleDef.ItemFontHighlight;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    }
                    MyGuiManager.DrawString(itemFontNormal, item.Value, new Vector2(vector5.X, vector5.Y + (this.m_styleDef.DropDownHighlightExtraWidth / 2f)), this.m_textScaleWithLanguage, new Color?(ApplyColorMaskModifiers(this.m_textColor, base.Enabled, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                    float* singlePtr3 = (float*) ref vector5.Y;
                    singlePtr3[0] += 0.03f;
                    num2++;
                }
            }
        }

        private void DrawOpenedAreaScrollbar(float scrollbarInnerTexturePositionX, MyRectangle2D openedArea, float transitionAlpha)
        {
            Vector2 vector;
            MyGuiBorderThickness scrollbarMargin = this.m_styleDef.ScrollbarMargin;
            if (this.IsFlipped)
            {
                vector = base.GetPositionAbsoluteTopRight() + new Vector2(-(scrollbarMargin.Right + this.m_scrollbarTexture.MinSizeGui.X), (-openedArea.Size.Y + scrollbarMargin.Top) + this.m_scrollBarCurrentPosition);
            }
            else
            {
                vector = base.GetPositionAbsoluteBottomRight() + new Vector2(-(scrollbarMargin.Right + this.m_scrollbarTexture.MinSizeGui.X), scrollbarMargin.Top + this.m_scrollBarCurrentPosition);
            }
            this.m_scrollbarTexture.Draw(vector, this.m_scrollBarHeight - this.m_scrollbarTexture.MinSizeGui.Y, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
        }

        private unsafe void DrawSelectedItemText(float transitionAlpha)
        {
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            RectangleF selectedItemArea = this.m_selectedItemArea;
            Vector2* vectorPtr1 = (Vector2*) ref selectedItemArea.Position;
            vectorPtr1[0] += positionAbsoluteTopLeft;
            using (MyGuiManager.UsingScissorRectangle(ref selectedItemArea))
            {
                Vector2 normalizedCoord = (positionAbsoluteTopLeft + this.m_selectedItemArea.Position) + new Vector2(0f, this.m_selectedItemArea.Size.Y * 0.5f);
                MyGuiManager.DrawString(this.m_selectedItemFont, this.m_selected.Value, normalizedCoord, this.m_textScaleWithLanguage, new Color?(ApplyColorMaskModifiers(this.m_textColor, base.Enabled, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
            }
        }

        public override MyGuiControlBase GetExclusiveInputHandler() => 
            (this.m_isOpen ? this : null);

        public Item GetItemByIndex(int index)
        {
            if ((index < 0) || (index >= this.m_items.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return this.m_items[index];
        }

        public int GetItemsCount() => 
            this.m_items.Count;

        protected Vector2 GetItemSize() => 
            MyGuiConstants.COMBOBOX_MEDIUM_ELEMENT_SIZE;

        private MyRectangle2D GetOpenedArea()
        {
            MyRectangle2D rectangled;
            if (this.IsFlipped)
            {
                rectangled.LeftTop = !this.m_showScrollBar ? new Vector2(0f, -this.m_items.Count * this.m_comboboxItemDeltaHeight) : new Vector2(0f, -this.m_openAreaItemsCount * this.m_comboboxItemDeltaHeight);
                rectangled.Size = !this.m_showScrollBar ? new Vector2(this.m_dropDownItemSize.X, this.m_items.Count * this.m_comboboxItemDeltaHeight) : new Vector2(this.m_dropDownItemSize.X, this.m_openAreaItemsCount * this.m_comboboxItemDeltaHeight);
            }
            else
            {
                rectangled.LeftTop = new Vector2(0f, base.Size.Y);
                rectangled.Size = !this.m_showScrollBar ? new Vector2(this.m_dropDownItemSize.X, this.m_items.Count * this.m_comboboxItemDeltaHeight) : new Vector2(this.m_dropDownItemSize.X, this.m_openAreaItemsCount * this.m_comboboxItemDeltaHeight);
            }
            return rectangled;
        }

        private Vector2 GetOpenItemPosition(int index)
        {
            float num = (this.m_dropDownItemSize.Y / 2f) + (this.m_comboboxItemDeltaHeight * 0.5f);
            return (!this.IsFlipped ? (new Vector2(0f, 0.5f * base.Size.Y) + new Vector2(0f, num + (index * this.m_comboboxItemDeltaHeight))) : (new Vector2(0f, -0.5f * base.Size.Y) + new Vector2(0f, num - ((Math.Min(this.m_openAreaItemsCount, this.m_items.Count) - index) * this.m_comboboxItemDeltaHeight))));
        }

        public int GetSelectedIndex() => 
            ((this.m_selected != null) ? this.m_items.IndexOf(this.m_selected) : -1);

        public long GetSelectedKey() => 
            ((this.m_selected != null) ? this.m_selected.Key : -1L);

        public StringBuilder GetSelectedValue() => 
            ((this.m_selected != null) ? this.m_selected.Value : null);

        public static StyleDefinition GetVisualStyle(MyGuiControlComboboxStyleEnum style) => 
            m_styles[(int) style];

        public override unsafe MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if ((base2 == null) && base.Enabled)
            {
                if ((base.IsMouseOver && (MyInput.Static.IsNewPrimaryButtonPressed() && !this.m_isOpen)) && !this.m_scrollBarDragging)
                {
                    return this;
                }
                if ((MyInput.Static.IsNewPrimaryButtonReleased() && !this.m_scrollBarDragging) && ((base.IsMouseOver && !this.m_isOpen) || (this.IsMouseOverSelectedItem() && this.m_isOpen)))
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    this.SwitchComboboxMode();
                    base2 = this;
                }
                if (base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space)))
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    if ((this.m_preselectedKeyboardIndex != null) && (this.m_preselectedKeyboardIndex.Value < this.m_items.Count))
                    {
                        if (!this.m_isOpen)
                        {
                            this.SetScrollBarPositionByIndex(this.m_selected.Key);
                        }
                        else
                        {
                            this.SelectItemByKey(this.m_items[this.m_preselectedKeyboardIndex.Value].Key, true);
                        }
                    }
                    this.SwitchComboboxMode();
                    base2 = this;
                }
                if (this.m_isOpen)
                {
                    if (this.m_showScrollBar && MyInput.Static.IsPrimaryButtonPressed())
                    {
                        float num3;
                        float num4;
                        Vector2 positionAbsoluteCenterLeft = base.GetPositionAbsoluteCenterLeft();
                        MyRectangle2D openedArea = this.GetOpenedArea();
                        Vector2* vectorPtr1 = (Vector2*) ref openedArea.LeftTop;
                        vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
                        float negativeInfinity = (positionAbsoluteCenterLeft.X + base.Size.X) - this.m_scrollBarWidth;
                        float positiveInfinity = positionAbsoluteCenterLeft.X + base.Size.X;
                        if (this.IsFlipped)
                        {
                            num4 = (openedArea.LeftTop.Y - (base.Size.Y / 2f)) + openedArea.Size.Y;
                        }
                        else
                        {
                            num4 = (positionAbsoluteCenterLeft.Y + (base.Size.Y / 2f)) + openedArea.Size.Y;
                        }
                        if (this.m_scrollBarDragging)
                        {
                            negativeInfinity = float.NegativeInfinity;
                            positiveInfinity = float.PositiveInfinity;
                            num3 = float.NegativeInfinity;
                            num4 = float.PositiveInfinity;
                        }
                        if (((MyGuiManager.MouseCursorPosition.X >= negativeInfinity) && ((MyGuiManager.MouseCursorPosition.X <= positiveInfinity) && (MyGuiManager.MouseCursorPosition.Y >= num3))) && (MyGuiManager.MouseCursorPosition.Y <= num4))
                        {
                            float num5 = this.m_scrollBarCurrentPosition + openedArea.LeftTop.Y;
                            if ((MyGuiManager.MouseCursorPosition.Y <= num5) || (MyGuiManager.MouseCursorPosition.Y >= (num5 + this.m_scrollBarHeight)))
                            {
                                float num7 = (MyGuiManager.MouseCursorPosition.Y - openedArea.LeftTop.Y) - (this.m_scrollBarHeight / 2f);
                                this.SetScrollBarPosition(num7, true);
                            }
                            else
                            {
                                if (this.m_mousePositionReinit)
                                {
                                    this.m_mouseOldPosition = MyGuiManager.MouseCursorPosition.Y;
                                    this.m_mousePositionReinit = false;
                                }
                                float num6 = MyGuiManager.MouseCursorPosition.Y - this.m_mouseOldPosition;
                                if ((num6 > float.Epsilon) || (num6 < float.Epsilon))
                                {
                                    this.SetScrollBarPosition(this.m_scrollBarCurrentNonadjustedPosition + num6, true);
                                }
                                this.m_mouseOldPosition = MyGuiManager.MouseCursorPosition.Y;
                            }
                            this.m_scrollBarDragging = true;
                        }
                    }
                    if (MyInput.Static.IsNewPrimaryButtonReleased())
                    {
                        this.m_mouseOldPosition = MyGuiManager.MouseCursorPosition.Y;
                        this.m_mousePositionReinit = true;
                    }
                    if ((base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || MyInput.Static.IsJoystickButtonNewPressed(MyJoystickButtonsEnum.J02))) || ((!this.IsMouseOverOnOpenedArea() && !base.IsMouseOver) && MyInput.Static.IsNewPrimaryButtonReleased()))
                    {
                        MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                        this.m_isOpen = false;
                    }
                    base2 = this;
                    if (this.m_scrollBarDragging)
                    {
                        if (MyInput.Static.IsNewPrimaryButtonReleased())
                        {
                            this.m_scrollBarDragging = false;
                        }
                        base2 = this;
                    }
                    else
                    {
                        this.m_preselectedMouseOverPrevious = this.m_preselectedMouseOver;
                        this.m_preselectedMouseOver = null;
                        int displayItemsStartIndex = 0;
                        int count = this.m_items.Count;
                        float num10 = 0f;
                        if (this.m_showScrollBar)
                        {
                            displayItemsStartIndex = this.m_displayItemsStartIndex;
                            count = this.m_displayItemsEndIndex;
                            num10 = 0.025f;
                        }
                        int num11 = displayItemsStartIndex;
                        while (true)
                        {
                            if (num11 >= count)
                            {
                                if ((this.m_preselectedMouseOver != null) && !ReferenceEquals(this.m_preselectedMouseOver, this.m_preselectedMouseOverPrevious))
                                {
                                    MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
                                }
                                if (MyInput.Static.IsNewPrimaryButtonReleased() && (this.m_preselectedMouseOver != null))
                                {
                                    this.SelectItemByKey(this.m_preselectedMouseOver.Key, true);
                                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                                    this.m_isOpen = false;
                                    base2 = this;
                                }
                                if (base.HasFocus || this.IsMouseOverOnOpenedArea())
                                {
                                    if (this.m_mouseWheelValueLast == null)
                                    {
                                        this.m_mouseWheelValueLast = new int?(MyInput.Static.MouseScrollWheelValue());
                                    }
                                    int? mouseWheelValueLast = this.m_mouseWheelValueLast;
                                    if ((MyInput.Static.MouseScrollWheelValue() < mouseWheelValueLast.GetValueOrDefault()) & (mouseWheelValueLast != null))
                                    {
                                        this.HandleItemMovement(true, false, false);
                                        base2 = this;
                                    }
                                    else
                                    {
                                        mouseWheelValueLast = this.m_mouseWheelValueLast;
                                        if ((MyInput.Static.MouseScrollWheelValue() > mouseWheelValueLast.GetValueOrDefault()) & (mouseWheelValueLast != null))
                                        {
                                            this.HandleItemMovement(false, false, false);
                                            base2 = this;
                                        }
                                    }
                                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Down) || MyInput.Static.IsNewGamepadKeyDownPressed())
                                    {
                                        this.HandleItemMovement(true, false, false);
                                        base2 = this;
                                        if (MyInput.Static.IsNewGamepadKeyDownPressed())
                                        {
                                            this.SnapCursorToControl(this.m_preselectedKeyboardIndex.Value);
                                        }
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.Up) || MyInput.Static.IsNewGamepadKeyUpPressed())
                                    {
                                        this.HandleItemMovement(false, false, false);
                                        base2 = this;
                                        if (MyInput.Static.IsNewGamepadKeyUpPressed())
                                        {
                                            this.SnapCursorToControl(this.m_preselectedKeyboardIndex.Value);
                                        }
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
                                    {
                                        this.HandleItemMovement(true, true, false);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
                                    {
                                        this.HandleItemMovement(false, true, false);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.Home))
                                    {
                                        this.HandleItemMovement(true, false, true);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.End))
                                    {
                                        this.HandleItemMovement(false, false, true);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.Tab))
                                    {
                                        if (this.m_isOpen)
                                        {
                                            this.SwitchComboboxMode();
                                        }
                                        base2 = null;
                                    }
                                    this.m_mouseWheelValueLast = new int?(MyInput.Static.MouseScrollWheelValue());
                                }
                                break;
                            }
                            Vector2 openItemPosition = this.GetOpenItemPosition(num11 - this.m_displayItemsStartIndex);
                            MyRectangle2D openedArea = this.GetOpenedArea();
                            Vector2 vector3 = new Vector2(openItemPosition.X, Math.Max(openedArea.LeftTop.Y, openItemPosition.Y));
                            Vector2 vector4 = vector3 + new Vector2(base.Size.X - num10, this.m_comboboxItemDeltaHeight);
                            Vector2 vector5 = MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft();
                            if (((vector5.X >= vector3.X) && ((vector5.X <= vector4.X) && (vector5.Y >= vector3.Y))) && (vector5.Y <= vector4.Y))
                            {
                                this.m_preselectedMouseOver = this.m_items[num11];
                            }
                            num11++;
                        }
                    }
                }
            }
            return base2;
        }

        private void HandleItemMovement(bool forwardMovement, bool page = false, bool list = false)
        {
            int? preselectedKeyboardIndex;
            int? preselectedKeyboardIndexPrevious;
            this.m_preselectedKeyboardIndexPrevious = this.m_preselectedKeyboardIndex;
            int num = 0;
            if (list & forwardMovement)
            {
                this.m_preselectedKeyboardIndex = 0;
            }
            else if (list && !forwardMovement)
            {
                this.m_preselectedKeyboardIndex = new int?(this.m_items.Count - 1);
            }
            else if (page & forwardMovement)
            {
                num = (this.m_openAreaItemsCount <= this.m_items.Count) ? (this.m_openAreaItemsCount - 1) : (this.m_items.Count - 1);
            }
            else if (!page || forwardMovement)
            {
                num = !((!page && !list) & forwardMovement) ? -1 : 1;
            }
            else
            {
                num = (this.m_openAreaItemsCount <= this.m_items.Count) ? (-this.m_openAreaItemsCount + 1) : -(this.m_items.Count - 1);
            }
            if (this.m_preselectedKeyboardIndex == null)
            {
                this.m_preselectedKeyboardIndex = new int?(forwardMovement ? 0 : (this.m_items.Count - 1));
            }
            else
            {
                int? nullable1;
                preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                int num2 = num;
                if (preselectedKeyboardIndex != null)
                {
                    nullable1 = new int?(preselectedKeyboardIndex.GetValueOrDefault() + num2);
                }
                else
                {
                    preselectedKeyboardIndexPrevious = null;
                    nullable1 = preselectedKeyboardIndexPrevious;
                }
                this.m_preselectedKeyboardIndex = nullable1;
                preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                num2 = this.m_items.Count - 1;
                if ((preselectedKeyboardIndex.GetValueOrDefault() > num2) & (preselectedKeyboardIndex != null))
                {
                    this.m_preselectedKeyboardIndex = new int?(this.m_items.Count - 1);
                }
                preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                num2 = 0;
                if ((preselectedKeyboardIndex.GetValueOrDefault() < num2) & (preselectedKeyboardIndex != null))
                {
                    this.m_preselectedKeyboardIndex = 0;
                }
            }
            preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
            preselectedKeyboardIndexPrevious = this.m_preselectedKeyboardIndexPrevious;
            if (!((preselectedKeyboardIndex.GetValueOrDefault() == preselectedKeyboardIndexPrevious.GetValueOrDefault()) & ((preselectedKeyboardIndex != null) == (preselectedKeyboardIndexPrevious != null))))
            {
                MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
            }
            this.SetScrollBarPositionByIndex((long) this.m_preselectedKeyboardIndex.Value);
        }

        private void InitializeScrollBarParameters()
        {
            this.m_showScrollBar = false;
            Vector2 vector = MyGuiConstants.COMBOBOX_VSCROLLBAR_SIZE;
            this.m_scrollBarWidth = vector.X;
            this.m_scrollBarHeight = vector.Y;
            this.m_scrollBarCurrentPosition = 0f;
            this.m_scrollBarEndPositionRelative = (this.m_openAreaItemsCount * this.m_comboboxItemDeltaHeight) + this.m_styleDef.DropDownTexture.LeftBottom.SizeGui.Y;
            this.m_displayItemsStartIndex = 0;
            this.m_displayItemsEndIndex = this.m_openAreaItemsCount;
        }

        private unsafe bool IsMouseOverOnOpenedArea()
        {
            MyRectangle2D openedArea = this.GetOpenedArea();
            float* singlePtr1 = (float*) ref openedArea.Size.Y;
            singlePtr1[0] += this.m_dropDownItemSize.Y;
            Vector2 leftTop = openedArea.LeftTop;
            Vector2 vector2 = openedArea.LeftTop + openedArea.Size;
            Vector2 vector3 = MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft();
            return ((vector3.X >= leftTop.X) && ((vector3.X <= vector2.X) && ((vector3.Y >= leftTop.Y) && (vector3.Y <= vector2.Y))));
        }

        private bool IsMouseOverSelectedItem()
        {
            Vector2 vector = base.GetPositionAbsoluteCenterLeft() - new Vector2(0f, base.Size.Y / 2f);
            Vector2 vector2 = vector + base.Size;
            return ((MyGuiManager.MouseCursorPosition.X >= vector.X) && ((MyGuiManager.MouseCursorPosition.X <= vector2.X) && ((MyGuiManager.MouseCursorPosition.Y >= vector.Y) && (MyGuiManager.MouseCursorPosition.Y <= vector2.Y))));
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.RefreshInternals();
        }

        protected override void OnOriginAlignChanged()
        {
            this.RefreshInternals();
            base.OnOriginAlignChanged();
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.RefreshInternals();
        }

        private void RefreshInternals()
        {
            if (base.HasHighlight)
            {
                base.BackgroundTexture = this.m_styleDef.ComboboxTextureHighlight;
                this.m_selectedItemFont = this.m_styleDef.ItemFontHighlight;
            }
            else
            {
                base.BackgroundTexture = this.m_styleDef.ComboboxTextureNormal;
                this.m_selectedItemFont = this.m_styleDef.ItemFontNormal;
            }
            base.MinSize = base.BackgroundTexture.MinSizeGui;
            base.MaxSize = base.BackgroundTexture.MaxSizeGui;
            this.m_scrollbarTexture = base.HasHighlight ? MyGuiConstants.TEXTURE_SCROLLBAR_V_THUMB_HIGHLIGHT : MyGuiConstants.TEXTURE_SCROLLBAR_V_THUMB;
            this.m_selectedItemArea.Position = this.m_styleDef.SelectedItemOffset;
            this.m_selectedItemArea.Size = new Vector2(base.Size.X - ((this.m_scrollbarTexture.MinSizeGui.X + this.m_styleDef.ScrollbarMargin.HorizontalSum) + this.m_styleDef.SelectedItemOffset.X), 0.03f);
            MyRectangle2D openedArea = this.GetOpenedArea();
            this.m_openedArea.Position = openedArea.LeftTop;
            this.m_openedArea.Size = openedArea.Size;
            this.m_openedItemArea.Position = this.m_openedArea.Position + new Vector2(this.m_styleDef.SelectedItemOffset.X, this.m_styleDef.DropDownTexture.LeftTop.SizeGui.Y);
            this.m_openedItemArea.Size = new Vector2(this.m_selectedItemArea.Size.X, (this.m_showScrollBar ? ((float) this.m_openAreaItemsCount) : ((float) this.m_items.Count)) * this.m_selectedItemArea.Size.Y);
            this.m_textScaleWithLanguage = this.m_styleDef.TextScale * MyGuiManager.LanguageTextScale;
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.RefreshInternals();
        }

        private void RemoveItem(Item item)
        {
            if (item != null)
            {
                this.m_items.Remove(item);
                if (ReferenceEquals(this.m_selected, item))
                {
                    this.m_selected = null;
                }
            }
        }

        public void RemoveItem(long key)
        {
            Item item = this.m_items.Find(x => x.Key == key);
            this.RemoveItem(item);
        }

        public void RemoveItemByIndex(int index)
        {
            if ((index < 0) || (index >= this.m_items.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.RemoveItem(this.m_items[index]);
        }

        public void ScrollToPreSelectedItem()
        {
            if (this.m_preselectedKeyboardIndex != null)
            {
                this.m_displayItemsStartIndex = (this.m_preselectedKeyboardIndex.Value <= this.m_middleIndex) ? 0 : (this.m_preselectedKeyboardIndex.Value - this.m_middleIndex);
                this.m_displayItemsEndIndex = this.m_displayItemsStartIndex + this.m_openAreaItemsCount;
                if (this.m_displayItemsEndIndex > this.m_items.Count)
                {
                    this.m_displayItemsEndIndex = this.m_items.Count;
                    this.m_displayItemsStartIndex = this.m_displayItemsEndIndex - this.m_openAreaItemsCount;
                }
                this.SetScrollBarPosition((this.m_displayItemsStartIndex * this.m_maxScrollBarPosition) / ((float) this.m_scrollBarItemOffSet), true);
            }
        }

        public void SelectItemByIndex(int index)
        {
            if (!this.m_items.IsValidIndex<Item>(index))
            {
                this.m_selected = null;
            }
            else
            {
                this.m_selected = this.m_items[index];
                this.SetScrollBarPositionByIndex((long) index);
                if (this.ItemSelected != null)
                {
                    this.ItemSelected();
                }
            }
        }

        public void SelectItemByKey(long key, bool sendEvent = true)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                Item objB = this.m_items[i];
                long num2 = objB.Key;
                if (num2.Equals(key) && !ReferenceEquals(this.m_selected, objB))
                {
                    this.m_selected = objB;
                    this.m_preselectedKeyboardIndex = new int?(i);
                    this.SetScrollBarPositionByIndex((long) i);
                    if (sendEvent && (this.ItemSelected != null))
                    {
                        this.ItemSelected();
                    }
                    return;
                }
            }
        }

        private void SetScrollBarPosition(float value, bool calculateItemIndexes = true)
        {
            float single1 = MathHelper.Clamp(value, 0f, this.m_maxScrollBarPosition);
            value = single1;
            if (this.m_scrollBarCurrentPosition != value)
            {
                this.m_scrollBarCurrentNonadjustedPosition = value;
                this.m_scrollBarCurrentPosition = value;
                if (calculateItemIndexes)
                {
                    this.CalculateStartAndEndDisplayItemsIndex();
                }
            }
        }

        private void SetScrollBarPositionByIndex(long index)
        {
            if (this.m_showScrollBar)
            {
                this.m_scrollRatio = 0f;
                int? preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                int displayItemsEndIndex = this.m_displayItemsEndIndex;
                if ((preselectedKeyboardIndex.GetValueOrDefault() >= displayItemsEndIndex) & (preselectedKeyboardIndex != null))
                {
                    this.m_displayItemsEndIndex = Math.Max(this.m_openAreaItemsCount, this.m_preselectedKeyboardIndex.Value + 1);
                    this.m_displayItemsStartIndex = Math.Max(0, this.m_displayItemsEndIndex - this.m_openAreaItemsCount);
                    this.SetScrollBarPosition((((float) this.m_preselectedKeyboardIndex.Value) * this.m_maxScrollBarPosition) / ((float) (this.m_items.Count - 1)), false);
                }
                else
                {
                    preselectedKeyboardIndex = this.m_preselectedKeyboardIndex;
                    displayItemsEndIndex = this.m_displayItemsStartIndex;
                    if ((preselectedKeyboardIndex.GetValueOrDefault() < displayItemsEndIndex) & (preselectedKeyboardIndex != null))
                    {
                        this.m_displayItemsStartIndex = Math.Max(0, this.m_preselectedKeyboardIndex.Value);
                        this.m_displayItemsEndIndex = Math.Max(this.m_openAreaItemsCount, this.m_displayItemsStartIndex + this.m_openAreaItemsCount);
                        this.SetScrollBarPosition((((float) this.m_preselectedKeyboardIndex.Value) * this.m_maxScrollBarPosition) / ((float) (this.m_items.Count - 1)), false);
                    }
                    else if (this.m_preselectedKeyboardIndex != null)
                    {
                        this.SetScrollBarPosition((((float) this.m_preselectedKeyboardIndex.Value) * this.m_maxScrollBarPosition) / ((float) (this.m_items.Count - 1)), false);
                    }
                }
            }
        }

        public override void ShowToolTip()
        {
            MyToolTips toolTip = base.m_toolTip;
            if ((this.m_isOpen && (this.IsMouseOverOnOpenedArea() && (this.m_preselectedMouseOver != null))) && (this.m_preselectedMouseOver.ToolTip != null))
            {
                base.m_toolTip = this.m_preselectedMouseOver.ToolTip;
            }
            base.ShowToolTip();
            base.m_toolTip = toolTip;
        }

        private unsafe void SnapCursorToControl(int controlIndex)
        {
            Vector2 vector2 = this.GetOpenItemPosition(controlIndex) - this.GetOpenItemPosition(this.m_displayItemsStartIndex);
            MyRectangle2D openedArea = this.GetOpenedArea();
            Vector2 positionAbsoluteCenter = base.GetPositionAbsoluteCenter();
            float* singlePtr1 = (float*) ref positionAbsoluteCenter.Y;
            singlePtr1[0] += openedArea.LeftTop.Y;
            float* singlePtr2 = (float*) ref positionAbsoluteCenter.Y;
            singlePtr2[0] += vector2.Y;
            Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(positionAbsoluteCenter, false);
            this.m_preselectedMouseOver = this.m_items[controlIndex];
            MyInput.Static.SetMousePosition((int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y);
        }

        public void SortItemsByValueText()
        {
            if (this.m_items != null)
            {
                this.m_items.Sort((item1, item2) => item1.Value.ToString().CompareTo(item2.Value.ToString()));
            }
        }

        private void SwitchComboboxMode()
        {
            if (!this.m_scrollBarDragging)
            {
                this.m_isOpen = !this.m_isOpen;
                if (this.m_isOpen)
                {
                    if (!this.IsFlipped)
                    {
                        MyRectangle2D openedArea = this.GetOpenedArea();
                        if ((base.GetPositionAbsoluteBottomRight().Y + openedArea.Size.Y) > 1f)
                        {
                            this.IsFlipped = true;
                        }
                    }
                    else
                    {
                        MyRectangle2D openedArea = this.GetOpenedArea();
                        if ((base.GetPositionAbsoluteTopRight().Y - openedArea.LeftTop.Y) < 1f)
                        {
                            this.IsFlipped = false;
                        }
                    }
                }
            }
        }

        public Item TryGetItemByKey(long key)
        {
            using (List<Item>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Item current = enumerator.Current;
                    if (current.Key == key)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public MyGuiControlComboboxStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public bool IsFlipped
        {
            get => 
                this.m_isFlipped;
            set
            {
                if (this.m_isFlipped != value)
                {
                    this.m_isFlipped = value;
                    this.RefreshInternals();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlCombobox.<>c <>9 = new MyGuiControlCombobox.<>c();
            public static Comparison<MyGuiControlCombobox.Item> <>9__71_0;

            internal int <SortItemsByValueText>b__71_0(MyGuiControlCombobox.Item item1, MyGuiControlCombobox.Item item2) => 
                item1.Value.ToString().CompareTo(item2.Value.ToString());
        }

        public class Item : IComparable
        {
            public readonly long Key;
            public readonly int SortOrder;
            public readonly StringBuilder Value;
            public MyToolTips ToolTip;

            public Item(long key, string value, int sortOrder, string toolTip = null)
            {
                this.Key = key;
                this.SortOrder = sortOrder;
                this.Value = (value == null) ? new StringBuilder() : new StringBuilder(value.Length).Append(value);
                if (toolTip != null)
                {
                    this.ToolTip = new MyToolTips(toolTip);
                }
            }

            public Item(long key, StringBuilder value, int sortOrder, string toolTip = null)
            {
                this.Key = key;
                this.SortOrder = sortOrder;
                this.Value = (value == null) ? new StringBuilder() : new StringBuilder(value.Length).AppendStringBuilder(value);
                if (toolTip != null)
                {
                    this.ToolTip = new MyToolTips(toolTip);
                }
            }

            public int CompareTo(object compareToObject) => 
                this.SortOrder.CompareTo(((MyGuiControlCombobox.Item) compareToObject).SortOrder);
        }

        public delegate void ItemSelectedDelegate();

        public class StyleDefinition
        {
            public string ItemFontHighlight;
            public string ItemFontNormal;
            public string ItemTextureHighlight;
            public Vector2 SelectedItemOffset;
            public MyGuiCompositeTexture DropDownTexture;
            public MyGuiCompositeTexture ComboboxTextureNormal;
            public MyGuiCompositeTexture ComboboxTextureHighlight;
            public float TextScale;
            public float DropDownHighlightExtraWidth;
            public MyGuiBorderThickness ScrollbarMargin;
        }
    }
}


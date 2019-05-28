namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlList))]
    public class MyGuiControlList : MyGuiControlParent
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlListStyleEnum>() + 1];
        private MyScrollbar m_scrollBar;
        private Vector2 m_realSize;
        private bool m_showScrollbar;
        private RectangleF m_itemsRectangle;
        private MyGuiBorderThickness m_itemMargin;
        private MyGuiControlListStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        [CompilerGenerated]
        private Action<MyGuiControlList> ItemMouseOver;

        public event Action<MyGuiControlList> ItemMouseOver
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlList> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<MyGuiControlList> a = itemMouseOver;
                    Action<MyGuiControlList> action3 = (Action<MyGuiControlList>) Delegate.Combine(a, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<MyGuiControlList>>(ref this.ItemMouseOver, action3, a);
                    if (ReferenceEquals(itemMouseOver, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlList> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<MyGuiControlList> source = itemMouseOver;
                    Action<MyGuiControlList> action3 = (Action<MyGuiControlList>) Delegate.Remove(source, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<MyGuiControlList>>(ref this.ItemMouseOver, action3, source);
                    if (ReferenceEquals(itemMouseOver, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlList()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            MyGuiBorderThickness thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition1.ScrollbarMargin = thickness;
            definition1.ItemMargin = new MyGuiBorderThickness(12f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 12f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            definition1.ScrollbarEnabled = true;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.Texture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition2.ScrollbarMargin = thickness;
            definition2.ItemMargin = new MyGuiBorderThickness(12f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 12f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            definition2.ScrollbarEnabled = true;
            m_styles[1] = definition2;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.ScrollbarEnabled = true;
            m_styles[2] = definition3;
            StyleDefinition definition4 = new StyleDefinition();
            definition4.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_WBORDER_LIST;
            thickness = new MyGuiBorderThickness {
                Left = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition4.ScrollbarMargin = thickness;
            definition4.ItemMargin = new MyGuiBorderThickness(12f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 12f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            definition4.ScrollbarEnabled = true;
            m_styles[3] = definition4;
        }

        public MyGuiControlList() : this(nullable, nullable, nullable2, null, MyGuiControlListStyleEnum.Default)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlList(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string toolTip = null, MyGuiControlListStyleEnum visualStyle = 0) : base(position, size, backgroundColor, toolTip)
        {
            base.Name = "ControlList";
            Vector2? nullable = size;
            this.m_realSize = (nullable != null) ? nullable.GetValueOrDefault() : Vector2.One;
            this.m_scrollBar = new MyVScrollbar(this);
            this.m_scrollBar.ValueChanged += new Action<MyScrollbar>(this.ValueChanged);
            this.VisualStyle = visualStyle;
            this.RecalculateScrollbar();
            base.Controls.CollectionChanged += new Action<MyGuiControls>(this.OnVisibleControlsChanged);
            base.Controls.CollectionMembersVisibleChanged += new Action<MyGuiControls>(this.OnVisibleControlsChanged);
        }

        private unsafe void CalculateNewPositionsForControls(float offset)
        {
            Vector2 marginStep = this.m_itemMargin.MarginStep;
            Vector2 topLeft = (((Vector2) (-0.5f * base.Size)) + this.m_itemMargin.TopLeftOffset) - new Vector2(-1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, offset);
            foreach (MyGuiControlBase base2 in base.Controls.GetVisibleControls())
            {
                Vector2 size = base2.Size;
                size.X = this.m_itemsRectangle.Size.X;
                base2.Position = MyUtils.GetCoordAlignedFromTopLeft(topLeft, size, base2.OriginAlign);
                float* singlePtr1 = (float*) ref topLeft.Y;
                singlePtr1[0] += size.Y + marginStep.Y;
            }
        }

        private unsafe void CalculateRealSize()
        {
            Vector2 zero = Vector2.Zero;
            float y = this.m_itemMargin.MarginStep.Y;
            using (List<MyGuiControlBase>.Enumerator enumerator = base.Controls.GetVisibleControls().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Vector2? size = enumerator.Current.GetSize();
                    Vector2 vector2 = size.Value;
                    float* singlePtr1 = (float*) ref zero.Y;
                    singlePtr1[0] += vector2.Y + y;
                    Vector2* vectorPtr1 = (Vector2*) ref zero;
                    vectorPtr1->X = Math.Max(zero.X, vector2.X);
                }
            }
            float* singlePtr2 = (float*) ref zero.Y;
            singlePtr2[0] -= y;
            this.m_realSize.X = Math.Max(base.Size.X, zero.X);
            this.m_realSize.Y = Math.Max(base.Size.Y, zero.Y);
        }

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_itemsRectangle.Position, this.m_itemsRectangle.Size, Color.White, 1);
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            this.m_styleDef.Texture.Draw(positionAbsoluteTopLeft, base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, backgroundTransitionAlpha), 1f);
            RectangleF itemsRectangle = this.m_itemsRectangle;
            Vector2* vectorPtr1 = (Vector2*) ref itemsRectangle.Position;
            vectorPtr1[0] += positionAbsoluteTopLeft;
            using (MyGuiManager.UsingScissorRectangle(ref itemsRectangle))
            {
                base.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
            if (this.m_showScrollbar)
            {
                this.m_scrollBar.Draw(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
            }
            Vector2 positionAbsoluteTopRight = base.GetPositionAbsoluteTopRight();
            float* singlePtr1 = (float*) ref positionAbsoluteTopRight.X;
            singlePtr1[0] -= this.m_styleDef.ScrollbarMargin.HorizontalSum + this.m_scrollBar.Size.X;
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Controls\scrollable_list_line.dds", positionAbsoluteTopRight, new Vector2(0.001f, base.Size.Y), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
        }

        public MyGuiBorderThickness GetItemMargins() => 
            this.m_itemMargin;

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlList objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_GuiControlList;
            objectBuilder.VisualStyle = this.VisualStyle;
            return objectBuilder;
        }

        public MyScrollbar GetScrollBar() => 
            this.m_scrollBar;

        public static StyleDefinition GetVisualStyle(MyGuiControlListStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if ((!this.m_showScrollbar || (!this.m_scrollBar.Visible || !this.CheckMouseOver())) || (base2 != null))
            {
                return base2;
            }
            return (!this.m_scrollBar.HandleInput() ? base2 : this);
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlList list = builder as MyObjectBuilder_GuiControlList;
            this.VisualStyle = list.VisualStyle;
        }

        public void InitControls(IEnumerable<MyGuiControlBase> controls)
        {
            base.Controls.CollectionMembersVisibleChanged -= new Action<MyGuiControls>(this.OnVisibleControlsChanged);
            base.Controls.CollectionChanged -= new Action<MyGuiControls>(this.OnVisibleControlsChanged);
            base.Controls.Clear();
            foreach (MyGuiControlBase base2 in controls)
            {
                if (base2 != null)
                {
                    base.Controls.Add(base2);
                }
            }
            base.Controls.CollectionChanged += new Action<MyGuiControls>(this.OnVisibleControlsChanged);
            base.Controls.CollectionMembersVisibleChanged += new Action<MyGuiControls>(this.OnVisibleControlsChanged);
            this.Recalculate();
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.m_scrollBar.HasHighlight = base.HasHighlight;
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.RecalculateScrollbar();
            this.CalculateNewPositionsForControls((this.m_scrollBar != null) ? this.m_scrollBar.Value : 0f);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        private void OnVisibleControlsChanged(MyGuiControls sender)
        {
            this.Recalculate();
        }

        public void Recalculate()
        {
            Vector2 realSize = this.m_realSize;
            this.CalculateRealSize();
            this.m_itemsRectangle.Position = this.m_itemMargin.TopLeftOffset;
            this.m_itemsRectangle.Size = base.Size - (this.m_itemMargin.SizeChange + new Vector2(this.m_styleDef.ScrollbarMargin.HorizontalSum + (this.m_showScrollbar ? this.m_scrollBar.Size.X : 0f), 0f));
            this.RecalculateScrollbar();
            this.CalculateNewPositionsForControls((this.m_scrollBar != null) ? this.m_scrollBar.Value : 0f);
        }

        private void RecalculateScrollbar()
        {
            if (this.m_showScrollbar)
            {
                this.m_scrollBar.Visible = base.Size.Y < this.m_realSize.Y;
                this.m_scrollBar.Init(this.m_realSize.Y, this.m_itemsRectangle.Size.Y);
                Vector2 vector = base.Size * new Vector2(0.5f, -0.5f);
                MyGuiBorderThickness scrollbarMargin = this.m_styleDef.ScrollbarMargin;
                Vector2 position = new Vector2(vector.X - (scrollbarMargin.Right + this.m_scrollBar.Size.X), vector.Y + scrollbarMargin.Top);
                this.m_scrollBar.Layout(position, base.Size.Y - scrollbarMargin.VerticalSum);
            }
        }

        private void RefreshInternals()
        {
            this.Recalculate();
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.m_itemMargin = this.m_styleDef.ItemMargin;
            this.m_showScrollbar = this.m_styleDef.ScrollbarEnabled;
            base.MinSize = this.m_styleDef.Texture.MinSizeGui;
            base.MaxSize = this.m_styleDef.Texture.MaxSizeGui;
            this.RefreshInternals();
        }

        public void SetScrollBarPage(float page = 0f)
        {
            this.m_scrollBar.SetPage(page);
        }

        public override void ShowToolTip()
        {
            if (this.m_itemsRectangle.Contains(MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft()))
            {
                base.ShowToolTip();
            }
        }

        private void ValueChanged(MyScrollbar scrollbar)
        {
            this.CalculateNewPositionsForControls(scrollbar.Value);
        }

        public MyGuiControlListStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public class StyleDefinition
        {
            public MyGuiCompositeTexture Texture = new MyGuiCompositeTexture(null);
            public MyGuiBorderThickness ScrollbarMargin;
            public MyGuiBorderThickness ItemMargin;
            public bool ScrollbarEnabled;
        }
    }
}


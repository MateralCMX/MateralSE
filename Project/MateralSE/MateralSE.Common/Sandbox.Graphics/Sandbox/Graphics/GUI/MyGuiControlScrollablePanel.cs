namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlScrollablePanel : MyGuiControlBase, IMyGuiControlsParent, IMyGuiControlsOwner
    {
        private MyGuiControls m_controls;
        private MyVScrollbar m_scrollbarV;
        private MyHScrollbar m_scrollbarH;
        private MyGuiControlBase m_scrolledControl;
        private RectangleF m_scrolledArea;
        private MyGuiBorderThickness m_scrolledAreaPadding;
        [CompilerGenerated]
        private Action<MyGuiControlScrollablePanel> PanelScrolled;
        public Vector2 ScrollBarOffset;
        public float ScrollBarHScale;
        public float ScrollBarVScale;
        public Vector2 ContentOffset;
        public bool DrawScrollBarSeparator;

        public event Action<MyGuiControlScrollablePanel> PanelScrolled
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlScrollablePanel> panelScrolled = this.PanelScrolled;
                while (true)
                {
                    Action<MyGuiControlScrollablePanel> a = panelScrolled;
                    Action<MyGuiControlScrollablePanel> action3 = (Action<MyGuiControlScrollablePanel>) Delegate.Combine(a, value);
                    panelScrolled = Interlocked.CompareExchange<Action<MyGuiControlScrollablePanel>>(ref this.PanelScrolled, action3, a);
                    if (ReferenceEquals(panelScrolled, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlScrollablePanel> panelScrolled = this.PanelScrolled;
                while (true)
                {
                    Action<MyGuiControlScrollablePanel> source = panelScrolled;
                    Action<MyGuiControlScrollablePanel> action3 = (Action<MyGuiControlScrollablePanel>) Delegate.Remove(source, value);
                    panelScrolled = Interlocked.CompareExchange<Action<MyGuiControlScrollablePanel>>(ref this.PanelScrolled, action3, source);
                    if (ReferenceEquals(panelScrolled, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlScrollablePanel(MyGuiControlBase scrolledControl) : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.ScrollBarOffset = Vector2.Zero;
            this.ScrollBarHScale = 1f;
            this.ScrollBarVScale = 1f;
            this.ContentOffset = Vector2.Zero;
            Vector2? nullable = null;
            nullable = null;
            base.Name = "ScrollablePanel";
            this.ScrolledControl = scrolledControl;
            this.m_controls = new MyGuiControls(this);
            if (scrolledControl != null)
            {
                this.m_controls.Add(this.ScrolledControl);
            }
        }

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft(), base.Size, Color.White, 2);
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_scrolledArea.Position, this.m_scrolledArea.Size, Color.Cyan, 1);
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            Color colorMask = ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha);
            if (this.m_scrollbarV != null)
            {
                this.m_scrollbarV.ScrollBarScale = this.ScrollBarVScale;
                this.m_scrollbarV.Draw(colorMask);
                if (this.DrawScrollBarSeparator)
                {
                    Vector2 positionAbsoluteTopRight = base.GetPositionAbsoluteTopRight();
                    float* singlePtr1 = (float*) ref positionAbsoluteTopRight.X;
                    singlePtr1[0] -= this.m_scrollbarV.Size.X + 0.0021f;
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Controls\scrollable_list_line.dds", positionAbsoluteTopRight, new Vector2(0.0012f, base.Size.Y), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                }
            }
            if (this.m_scrollbarH != null)
            {
                this.m_scrollbarH.ScrollBarScale = this.ScrollBarHScale;
                this.m_scrollbarH.Draw(colorMask);
            }
        }

        protected override unsafe void DrawElements(float transitionAlpha, float backgroundTransitionAlpha)
        {
            RectangleF scrolledArea = this.m_scrolledArea;
            Vector2* vectorPtr1 = (Vector2*) ref scrolledArea.Position;
            vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
            using (MyGuiManager.UsingScissorRectangle(ref scrolledArea))
            {
                base.DrawElements(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        public unsafe void FitSizeToScrolledControl()
        {
            if (this.ScrolledControl != null)
            {
                this.m_scrolledArea.Size = this.ScrolledControl.Size;
                Vector2 vector = this.ScrolledControl.Size + this.m_scrolledAreaPadding.SizeChange;
                if (this.m_scrollbarV != null)
                {
                    float* singlePtr1 = (float*) ref vector.X;
                    singlePtr1[0] += this.m_scrollbarV.Size.X;
                }
                if (this.m_scrollbarH != null)
                {
                    float* singlePtr2 = (float*) ref vector.Y;
                    singlePtr2[0] += this.m_scrollbarH.Size.Y;
                }
                base.Size = vector;
            }
        }

        public override unsafe MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            Vector2* vectorPtr1 = (Vector2*) ref this.m_scrolledArea.Position;
            vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
            base2 = base.HandleInputElements();
            if ((this.m_scrollbarV != null) && this.m_scrollbarV.HandleInput())
            {
                base2 = base2 ?? this;
            }
            if ((this.m_scrollbarH != null) && this.m_scrollbarH.HandleInput())
            {
                base2 = base2 ?? this;
            }
            return base2;
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            if (this.m_scrollbarV != null)
            {
                this.m_scrollbarV.HasHighlight = base.HasHighlight;
            }
            if (this.m_scrollbarH != null)
            {
                this.m_scrollbarH.HasHighlight = base.HasHighlight;
            }
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.RefreshScrollbar();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshScrolledArea();
            this.RefreshScrollbar();
        }

        public void RefreshInternals()
        {
            this.RefreshScrolledArea();
            this.RefreshScrollbar();
        }

        private void RefreshScrollbar()
        {
            if (this.ScrolledControl == null)
            {
                if (this.m_scrollbarV != null)
                {
                    this.m_scrollbarV.Visible = false;
                }
                if (this.m_scrollbarH != null)
                {
                    this.m_scrollbarH.Visible = false;
                }
            }
            else
            {
                if (this.m_scrollbarV != null)
                {
                    this.m_scrollbarV.Visible = this.m_scrolledArea.Size.Y < this.ScrolledControl.Size.Y;
                    if (!this.m_scrollbarV.Visible)
                    {
                        this.m_scrollbarV.Value = 0f;
                    }
                    else
                    {
                        this.m_scrollbarV.Init(this.ScrolledControl.Size.Y, this.m_scrolledArea.Size.Y);
                        Vector2 vector = base.Size * new Vector2(0.5f, -0.5f);
                        Vector2 vector2 = new Vector2((vector.X - this.m_scrollbarV.Size.X) + this.ScrollBarOffset.X, vector.Y);
                        this.m_scrollbarV.Layout(vector2 + new Vector2(0f, this.m_scrolledAreaPadding.Top), this.m_scrolledArea.Size.Y);
                    }
                }
                if (this.m_scrollbarH != null)
                {
                    this.m_scrollbarH.Visible = this.m_scrolledArea.Size.X < this.ScrolledControl.Size.X;
                    if (!this.m_scrollbarH.Visible)
                    {
                        this.m_scrollbarH.Value = 0f;
                    }
                    else
                    {
                        this.m_scrollbarH.Init(this.ScrolledControl.Size.X, this.m_scrolledArea.Size.X);
                        Vector2 vector3 = base.Size * new Vector2(-0.5f, 0.5f);
                        Vector2 vector4 = new Vector2(vector3.X, (vector3.Y - this.m_scrollbarH.Size.Y) + this.ScrollBarOffset.Y);
                        this.m_scrollbarH.Layout(vector4 + new Vector2(this.m_scrolledAreaPadding.Left), this.m_scrolledArea.Size.X);
                    }
                }
            }
            this.RefreshScrolledControlPosition();
        }

        private unsafe void RefreshScrolledArea()
        {
            this.m_scrolledArea = new RectangleF(this.m_scrolledAreaPadding.TopLeftOffset, base.Size - this.m_scrolledAreaPadding.SizeChange);
            if (this.m_scrollbarV != null)
            {
                float* singlePtr1 = (float*) ref this.m_scrolledArea.Size.X;
                singlePtr1[0] -= this.m_scrollbarV.Size.X;
            }
            if (this.m_scrollbarH != null)
            {
                float* singlePtr2 = (float*) ref this.m_scrolledArea.Size.Y;
                singlePtr2[0] -= this.m_scrollbarH.Size.Y;
            }
            if (this.PanelScrolled != null)
            {
                this.PanelScrolled(this);
            }
        }

        private unsafe void RefreshScrolledControlPosition()
        {
            Vector2 vector = (((Vector2) (-0.5f * base.Size)) + this.m_scrolledAreaPadding.TopLeftOffset) + this.ContentOffset;
            if (this.m_scrollbarH != null)
            {
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0] -= this.m_scrollbarH.Value;
            }
            if (this.m_scrollbarV != null)
            {
                float* singlePtr2 = (float*) ref vector.Y;
                singlePtr2[0] -= this.m_scrollbarV.Value;
            }
            if (this.ScrolledControl != null)
            {
                this.ScrolledControl.Position = vector;
            }
            if (this.PanelScrolled != null)
            {
                this.PanelScrolled(this);
            }
        }

        private void scrollbar_ValueChanged(MyScrollbar scrollbar)
        {
            this.RefreshScrolledControlPosition();
        }

        private void scrolledControl_SizeChanged(MyGuiControlBase control)
        {
            this.RefreshInternals();
        }

        public void SetPageVertical(float pageNumber)
        {
            this.m_scrollbarV.SetPage(pageNumber);
        }

        public void SetVerticalScrollbarValue(float value)
        {
            if (this.m_scrollbarV != null)
            {
                this.m_scrollbarV.Value = value;
            }
        }

        public override void ShowToolTip()
        {
            if (base.IsMouseOver)
            {
                base.ShowToolTip();
            }
        }

        public float ScrollbarHSizeX =>
            ((this.m_scrollbarH != null) ? this.m_scrollbarH.Size.X : 0f);

        public float ScrollbarHSizeY =>
            ((this.m_scrollbarH != null) ? this.m_scrollbarH.Size.Y : 0f);

        public float ScrollbarVSizeX =>
            ((this.m_scrollbarV != null) ? this.m_scrollbarV.Size.X : 0f);

        public float ScrollbarVSizeY =>
            ((this.m_scrollbarV != null) ? this.m_scrollbarV.Size.Y : 0f);

        public bool ScrollbarHWheel
        {
            get => 
                ((this.m_scrollbarH != null) && this.m_scrollbarH.EnableWheelScroll);
            set
            {
                if (this.m_scrollbarH != null)
                {
                    this.m_scrollbarH.EnableWheelScroll = value;
                }
            }
        }

        public bool ScrollbarHEnabled
        {
            get => 
                (this.m_scrollbarH != null);
            set
            {
                if (value && (this.m_scrollbarH == null))
                {
                    this.m_scrollbarH = new MyHScrollbar(this);
                    this.m_scrollbarH.ValueChanged += new Action<MyScrollbar>(this.scrollbar_ValueChanged);
                }
                else if (!value)
                {
                    this.m_scrollbarH = null;
                }
            }
        }

        public bool ScrollbarVEnabled
        {
            get => 
                (this.m_scrollbarV != null);
            set
            {
                if (value && (this.m_scrollbarV == null))
                {
                    this.m_scrollbarV = new MyVScrollbar(this);
                    this.m_scrollbarV.ValueChanged += new Action<MyScrollbar>(this.scrollbar_ValueChanged);
                }
                else if (!value)
                {
                    this.m_scrollbarV = null;
                }
            }
        }

        public MyGuiControlBase ScrolledControl
        {
            get => 
                this.m_scrolledControl;
            set
            {
                if (!ReferenceEquals(this.m_scrolledControl, value))
                {
                    if (this.m_scrolledControl != null)
                    {
                        base.Elements.Remove(this.m_scrolledControl);
                        this.m_scrolledControl.SizeChanged -= new Action<MyGuiControlBase>(this.scrolledControl_SizeChanged);
                    }
                    this.m_scrolledControl = value;
                    if (this.m_scrolledControl != null)
                    {
                        base.Elements.Add(this.m_scrolledControl);
                        this.m_scrolledControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                        this.m_scrolledControl.SizeChanged += new Action<MyGuiControlBase>(this.scrolledControl_SizeChanged);
                    }
                    this.RefreshScrollbar();
                    this.RefreshScrolledControlPosition();
                }
            }
        }

        public Vector2 ScrolledAreaSize =>
            this.m_scrolledArea.Size;

        public MyGuiBorderThickness ScrolledAreaPadding
        {
            get => 
                this.m_scrolledAreaPadding;
            set
            {
                this.m_scrolledAreaPadding = value;
                this.RefreshInternals();
            }
        }

        public float ScrollbarVPosition
        {
            get => 
                ((this.m_scrollbarV == null) ? 0f : this.m_scrollbarV.Value);
            set
            {
                if (this.m_scrollbarV != null)
                {
                    this.m_scrollbarV.Value = value;
                }
            }
        }

        public MyGuiControls Controls =>
            this.m_controls;
    }
}


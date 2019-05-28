namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRageMath;

    public abstract class MyScrollbar
    {
        private bool m_hasHighlight;
        private float m_value;
        private MyGuiCompositeTexture m_normalTexture;
        private MyGuiCompositeTexture m_highlightTexture;
        protected MyGuiCompositeTexture m_backgroundTexture;
        protected MyGuiControlBase OwnerControl;
        protected Vector2 Position;
        protected Vector2 CaretSize;
        protected Vector2 CaretPageSize;
        protected float Max;
        protected float Page;
        protected StateEnum State;
        protected MyGuiCompositeTexture Texture;
        public float ScrollBarScale = 1f;
        public bool Visible;
        [CompilerGenerated]
        private Action<MyScrollbar> ValueChanged;

        public event Action<MyScrollbar> ValueChanged
        {
            [CompilerGenerated] add
            {
                Action<MyScrollbar> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<MyScrollbar> a = valueChanged;
                    Action<MyScrollbar> action3 = (Action<MyScrollbar>) Delegate.Combine(a, value);
                    valueChanged = Interlocked.CompareExchange<Action<MyScrollbar>>(ref this.ValueChanged, action3, a);
                    if (ReferenceEquals(valueChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyScrollbar> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<MyScrollbar> source = valueChanged;
                    Action<MyScrollbar> action3 = (Action<MyScrollbar>) Delegate.Remove(source, value);
                    valueChanged = Interlocked.CompareExchange<Action<MyScrollbar>>(ref this.ValueChanged, action3, source);
                    if (ReferenceEquals(valueChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyScrollbar(MyGuiControlBase control, MyGuiCompositeTexture normalTexture, MyGuiCompositeTexture highlightTexture, MyGuiCompositeTexture backgroundTexture)
        {
            this.OwnerControl = control;
            this.m_normalTexture = normalTexture;
            this.m_highlightTexture = highlightTexture;
            this.m_backgroundTexture = backgroundTexture;
            this.RefreshInternals();
        }

        protected bool CanScroll() => 
            ((this.Max > 0f) && (this.Max > this.Page));

        public void ChangeValue(float amount)
        {
            this.Value += amount;
        }

        public void DebugDraw()
        {
            MyGuiManager.DrawBorders(this.OwnerControl.GetPositionAbsoluteCenter() + this.Position, this.Size, Color.White, 1);
        }

        public abstract void Draw(Color colorMask);
        public float GetPage() => 
            (this.Value / this.Page);

        public abstract bool HandleInput();
        public void Init(float max, float page)
        {
            this.Max = max;
            this.Page = page;
        }

        public abstract void Layout(Vector2 position, float length);
        public void PageDown()
        {
            this.ChangeValue(this.Page);
        }

        public void PageUp()
        {
            this.ChangeValue(-this.Page);
        }

        protected virtual void RefreshInternals()
        {
            this.Texture = this.HasHighlight ? this.m_highlightTexture : this.m_normalTexture;
            if (this.HasHighlight)
            {
                this.Texture = this.m_highlightTexture;
            }
            else
            {
                this.Texture = this.m_normalTexture;
            }
        }

        public void SetPage(float pageNumber)
        {
            this.Value = pageNumber * this.Page;
        }

        public Vector2 Size { get; protected set; }

        public bool HasHighlight
        {
            get => 
                this.m_hasHighlight;
            set
            {
                if (this.m_hasHighlight != value)
                {
                    this.m_hasHighlight = value;
                    this.RefreshInternals();
                }
            }
        }

        public float MaxSize =>
            this.Max;

        public float PageSize =>
            this.Page;

        public float Value
        {
            get => 
                this.m_value;
            set
            {
                float single1 = MathHelper.Clamp(value, 0f, this.Max - this.Page);
                value = single1;
                if (this.m_value != value)
                {
                    this.m_value = value;
                    if (this.ValueChanged != null)
                    {
                        this.ValueChanged(this);
                    }
                }
            }
        }

        public bool IsOverCaret { get; protected set; }

        public bool IsInDomainCaret { get; protected set; }

        protected enum StateEnum
        {
            Ready,
            Drag,
            Click
        }
    }
}


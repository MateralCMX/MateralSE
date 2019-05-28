namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlRadioButton))]
    public class MyGuiControlRadioButton : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlRadioButtonStyleEnum>()];
        private bool m_selected;
        private MyGuiControlRadioButtonStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private StringBuilder m_text;
        private string m_font;
        private RectangleF m_internalArea;
        private int? m_doubleClickStarted;
        public MyGuiDrawAlignEnum TextAlignment;
        public MyGuiHighlightTexture? Icon;
        public MyGuiDrawAlignEnum IconOriginAlign;
        [CompilerGenerated]
        private Action<MyGuiControlRadioButton> SelectedChanged;
        [CompilerGenerated]
        private Action<MyGuiControlRadioButton> MouseDoubleClick;
        [CompilerGenerated]
        private Action<MyGuiControlRadioButton, bool> MouseOverChanged;
        private bool m_lastMouseOver;

        public event Action<MyGuiControlRadioButton> MouseDoubleClick
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlRadioButton> mouseDoubleClick = this.MouseDoubleClick;
                while (true)
                {
                    Action<MyGuiControlRadioButton> a = mouseDoubleClick;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Combine(a, value);
                    mouseDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.MouseDoubleClick, action3, a);
                    if (ReferenceEquals(mouseDoubleClick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlRadioButton> mouseDoubleClick = this.MouseDoubleClick;
                while (true)
                {
                    Action<MyGuiControlRadioButton> source = mouseDoubleClick;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Remove(source, value);
                    mouseDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.MouseDoubleClick, action3, source);
                    if (ReferenceEquals(mouseDoubleClick, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlRadioButton, bool> MouseOverChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlRadioButton, bool> mouseOverChanged = this.MouseOverChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButton, bool> a = mouseOverChanged;
                    Action<MyGuiControlRadioButton, bool> action3 = (Action<MyGuiControlRadioButton, bool>) Delegate.Combine(a, value);
                    mouseOverChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButton, bool>>(ref this.MouseOverChanged, action3, a);
                    if (ReferenceEquals(mouseOverChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlRadioButton, bool> mouseOverChanged = this.MouseOverChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButton, bool> source = mouseOverChanged;
                    Action<MyGuiControlRadioButton, bool> action3 = (Action<MyGuiControlRadioButton, bool>) Delegate.Remove(source, value);
                    mouseOverChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButton, bool>>(ref this.MouseOverChanged, action3, source);
                    if (ReferenceEquals(mouseOverChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlRadioButton> SelectedChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlRadioButton> selectedChanged = this.SelectedChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButton> a = selectedChanged;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Combine(a, value);
                    selectedChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.SelectedChanged, action3, a);
                    if (ReferenceEquals(selectedChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlRadioButton> selectedChanged = this.SelectedChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButton> source = selectedChanged;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Remove(source, value);
                    selectedChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.SelectedChanged, action3, source);
                    if (ReferenceEquals(selectedChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlRadioButton()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_CHARACTER;
            definition1.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_CHARACTER_HIGHLIGHT;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_GRID;
            definition2.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_GRID_HIGHLIGHT;
            m_styles[1] = definition2;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_ALL;
            definition3.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_ALL_HIGHLIGHT;
            m_styles[2] = definition3;
            StyleDefinition definition4 = new StyleDefinition();
            definition4.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_ENERGY;
            definition4.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_ENERGY_HIGHLIGHT;
            m_styles[3] = definition4;
            StyleDefinition definition5 = new StyleDefinition();
            definition5.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_STORAGE;
            definition5.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_STORAGE_HIGHLIGHT;
            m_styles[5] = definition5;
            StyleDefinition definition6 = new StyleDefinition();
            definition6.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_SYSTEM;
            definition6.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_SYSTEM_HIGHLIGHT;
            m_styles[6] = definition6;
            StyleDefinition definition7 = new StyleDefinition();
            definition7.NormalTexture = MyGuiConstants.TEXTURE_NULL;
            definition7.HighlightTexture = MyGuiConstants.TEXTURE_NULL;
            m_styles[7] = definition7;
            StyleDefinition definition8 = new StyleDefinition();
            definition8.NormalTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            definition8.HighlightTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition8.NormalFont = "Blue";
            definition8.HighlightFont = "White";
            definition8.Padding = new MyGuiBorderThickness(6f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles[8] = definition8;
            StyleDefinition definition9 = new StyleDefinition();
            definition9.NormalTexture = MyGuiConstants.TEXTURE_RECTANGLE_BUTTON_BORDER;
            definition9.HighlightTexture = MyGuiConstants.TEXTURE_RECTANGLE_BUTTON_HIGHLIGHTED_BORDER;
            definition9.NormalFont = "Blue";
            definition9.HighlightFont = "White";
            definition9.Padding = new MyGuiBorderThickness(20f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles[9] = definition9;
            StyleDefinition definition10 = new StyleDefinition();
            definition10.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_SHIP;
            definition10.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_FILTER_SHIP_HIGHLIGHT;
            m_styles[4] = definition10;
        }

        public MyGuiControlRadioButton() : this(nullable, nullable, 0, nullable2)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlRadioButton(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), int key = 0, Vector4? colorMask = new Vector4?()) : base(position, size, colorMask, null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            base.Name = "RadioButton";
            this.m_selected = false;
            this.Key = key;
            this.VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, transitionAlpha);
            Vector2 zero = Vector2.Zero;
            if ((this.Icon != null) || ((this.Text != null) && (this.Text.Length > 0)))
            {
                zero = base.GetPositionAbsoluteTopLeft();
            }
            Vector2 topLeft = zero + this.m_internalArea.Position;
            Vector2 size = this.m_internalArea.Size;
            if (this.Icon != null)
            {
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromTopLeft(topLeft, size, this.IconOriginAlign);
                MyGuiHighlightTexture texture = this.Icon.Value;
                Vector2 vector5 = Vector2.Min(texture.SizeGui, size) / texture.SizeGui;
                float num = Math.Min(vector5.X, vector5.Y);
                MyGuiManager.DrawSpriteBatch(base.HasHighlight ? texture.Highlight : texture.Normal, normalizedCoord, texture.SizeGui * num, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), this.IconOriginAlign, false, true);
            }
            if ((this.Text != null) && (this.Text.Length > 0))
            {
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromTopLeft(topLeft, this.m_internalArea.Size, this.TextAlignment);
                MyGuiManager.DrawString(this.m_font, this.Text, normalizedCoord, 0.8f * MyGuiManager.LanguageTextScale, new Color?(ApplyColorMaskModifiers(Vector4.One, base.Enabled, transitionAlpha)), this.TextAlignment, false, float.PositiveInfinity);
            }
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlRadioButton objectBuilder = (MyObjectBuilder_GuiControlRadioButton) base.GetObjectBuilder();
            objectBuilder.Key = this.Key;
            objectBuilder.VisualStyle = this.VisualStyle;
            if (this.VisualStyle == MyGuiControlRadioButtonStyleEnum.Custom)
            {
                MyGuiCustomVisualStyle style = new MyGuiCustomVisualStyle {
                    HighlightTexture = this.m_styleDef.HighlightTexture.LeftTop.Texture,
                    NormalTexture = this.m_styleDef.NormalTexture.LeftTop.Texture,
                    Size = this.m_styleDef.HighlightTexture.LeftTop.SizePx,
                    HighlightFont = this.m_styleDef.HighlightFont,
                    NormalFont = this.m_styleDef.NormalFont,
                    VerticalPadding = this.m_styleDef.Padding.VerticalSum,
                    HorizontalPadding = this.m_styleDef.Padding.HorizontalSum
                };
            }
            return objectBuilder;
        }

        public static StyleDefinition GetVisualStyle(MyGuiControlRadioButtonStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (this.m_lastMouseOver != base.IsMouseOver)
            {
                this.m_lastMouseOver = base.IsMouseOver;
                if (this.MouseOverChanged != null)
                {
                    this.MouseOverChanged(this, base.IsMouseOver);
                }
            }
            if ((base2 == null) && base.Enabled)
            {
                if (((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonReleased()) || (base.HasFocus && ((MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space)) || MyInput.Static.IsJoystickButtonNewPressed(MyJoystickButtonsEnum.J01)))) && !this.Selected)
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    this.Selected = true;
                    base2 = this;
                }
                if (base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed())
                {
                    if (this.m_doubleClickStarted == null)
                    {
                        this.m_doubleClickStarted = new int?(MyGuiManager.TotalTimeInMilliseconds);
                    }
                    else if ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) <= 500f)
                    {
                        this.MouseDoubleClick.InvokeIfNotNull<MyGuiControlRadioButton>(this);
                        this.m_doubleClickStarted = null;
                    }
                }
            }
            if ((this.m_doubleClickStarted != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) >= 500f))
            {
                this.m_doubleClickStarted = null;
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlRadioButton button = (MyObjectBuilder_GuiControlRadioButton) builder;
            this.Key = button.Key;
            if (button.VisualStyle != MyGuiControlRadioButtonStyleEnum.Custom)
            {
                this.VisualStyle = button.VisualStyle;
            }
            else if (button.CustomVisualStyle == null)
            {
                this.VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular;
            }
            else
            {
                MyGuiCustomVisualStyle style = button.CustomVisualStyle.Value;
                this.m_styleDef = new StyleDefinition();
                this.m_styleDef.HighlightFont = style.HighlightFont;
                this.m_styleDef.NormalFont = style.NormalFont;
                MyGuiSizedTexture texture = new MyGuiSizedTexture {
                    SizePx = style.Size,
                    Texture = style.HighlightTexture
                };
                MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
                texture1.LeftTop = texture;
                this.m_styleDef.HighlightTexture = texture1;
                texture = new MyGuiSizedTexture {
                    SizePx = style.Size,
                    Texture = style.NormalTexture
                };
                MyGuiCompositeTexture texture2 = new MyGuiCompositeTexture(null);
                texture2.LeftTop = texture;
                this.m_styleDef.NormalTexture = texture2;
                this.m_styleDef.Padding = new MyGuiBorderThickness(style.HorizontalPadding, style.VerticalPadding);
                this.VisualStyle = button.VisualStyle;
            }
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.RefreshInternals();
        }

        protected override void OnSizeChanged()
        {
            this.RefreshInternalArea();
            base.OnSizeChanged();
        }

        private void RefreshInternalArea()
        {
            this.m_internalArea.Position = this.m_styleDef.Padding.TopLeftOffset;
            this.m_internalArea.Size = base.Size - this.m_styleDef.Padding.SizeChange;
        }

        private void RefreshInternals()
        {
            if (base.HasHighlight)
            {
                this.m_font = this.m_styleDef.HighlightFont;
                base.BackgroundTexture = this.m_styleDef.HighlightTexture;
            }
            else
            {
                this.m_font = this.m_styleDef.NormalFont;
                base.BackgroundTexture = this.m_styleDef.NormalTexture;
            }
            base.MinSize = base.BackgroundTexture.MinSizeGui;
            base.MaxSize = base.BackgroundTexture.MaxSizeGui;
            this.RefreshInternalArea();
        }

        private void RefreshVisualStyle()
        {
            if (this.m_visualStyle != MyGuiControlRadioButtonStyleEnum.Custom)
            {
                this.m_styleDef = GetVisualStyle(this.VisualStyle);
            }
            this.RefreshInternals();
        }

        protected override bool ShouldHaveHighlight() => 
            (this.Selected || base.ShouldHaveHighlight());

        public MyGuiControlRadioButtonStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public StringBuilder Text
        {
            get => 
                this.m_text;
            set
            {
                if (value == null)
                {
                    if (this.m_text != null)
                    {
                        this.m_text = null;
                    }
                }
                else
                {
                    if (this.m_text == null)
                    {
                        this.m_text = new StringBuilder();
                    }
                    this.m_text.Clear().AppendStringBuilder(value);
                }
            }
        }

        public int Key { get; set; }

        public bool Selected
        {
            get => 
                this.m_selected;
            set
            {
                if (this.m_selected != value)
                {
                    this.m_selected = value;
                    if (value && (this.SelectedChanged != null))
                    {
                        this.SelectedChanged(this);
                    }
                }
            }
        }

        public class StyleDefinition
        {
            public MyGuiCompositeTexture NormalTexture;
            public MyGuiCompositeTexture HighlightTexture;
            public string NormalFont;
            public string HighlightFont;
            public MyGuiBorderThickness Padding;
        }
    }
}


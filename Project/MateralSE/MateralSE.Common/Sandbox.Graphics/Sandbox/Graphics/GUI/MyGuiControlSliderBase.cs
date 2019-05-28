namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlSliderBase : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlSliderStyleEnum>() + 1];
        public Action<MyGuiControlSliderBase> ValueChanged;
        private bool m_controlCaptured;
        private string m_thumbTexture;
        private MyGuiControlLabel m_label;
        private bool showLabel;
        private MyGuiCompositeTexture m_railTexture;
        private float m_labelSpaceWidth;
        private float m_debugScale;
        public float? DefaultRatio;
        private MyGuiControlSliderStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private MyGuiSliderProperties m_props;
        private float m_ratio;
        public Func<MyGuiControlSliderBase, bool> SliderClicked;
        private int m_lastTimeArrowPressed;

        static MyGuiControlSliderBase()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.RailTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL;
            definition1.RailHighlightTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL_HIGHLIGHT;
            definition1.ThumbTexture = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.RailTexture = MyGuiConstants.TEXTURE_HUE_SLIDER_RAIL;
            definition2.RailHighlightTexture = MyGuiConstants.TEXTURE_HUE_SLIDER_RAIL_HIGHLIGHT;
            definition2.ThumbTexture = MyGuiConstants.TEXTURE_HUE_SLIDER_THUMB_DEFAULT;
            m_styles[1] = definition2;
        }

        public MyGuiControlSliderBase(Vector2? position = new Vector2?(), float width = 0.29f, MyGuiSliderProperties props = null, float? defaultRatio = new float?(), Vector4? color = new Vector4?(), float labelScale = 0.8f, float labelSpaceWidth = 0f, string labelFont = "White", string toolTip = null, MyGuiControlSliderStyleEnum visualStyle = 0, MyGuiDrawAlignEnum originAlign = 4, bool showLabel = true) : base(position, nullable, colorMask, toolTip, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
        {
            this.showLabel = true;
            this.m_debugScale = 1f;
            Vector2? nullable = null;
            Vector4? colorMask = null;
            this.showLabel = showLabel;
            if (defaultRatio != null)
            {
                defaultRatio = new float?(MathHelper.Clamp(defaultRatio.Value, 0f, 1f));
            }
            if (props == null)
            {
                props = MyGuiSliderProperties.Default;
            }
            this.m_props = props;
            this.DefaultRatio = defaultRatio;
            this.m_ratio = (defaultRatio != null) ? defaultRatio.Value : 0f;
            this.m_labelSpaceWidth = labelSpaceWidth;
            nullable = null;
            nullable = null;
            colorMask = null;
            this.m_label = new MyGuiControlLabel(nullable, nullable, string.Empty, colorMask, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            if (showLabel)
            {
                base.Elements.Add(this.m_label);
            }
            this.VisualStyle = visualStyle;
            base.Size = new Vector2(width, base.Size.Y);
            this.UpdateLabel();
        }

        public void ApplyStyle(StyleDefinition style)
        {
            if (style != null)
            {
                this.m_styleDef = style;
                this.RefreshInternals();
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            this.m_railTexture.Draw(base.GetPositionAbsoluteTopLeft(), base.Size - new Vector2(this.m_labelSpaceWidth, 0f), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), this.DebugScale);
            this.DrawThumb(transitionAlpha);
            if (this.showLabel)
            {
                this.m_label.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        private void DrawThumb(float transitionAlpha)
        {
            float x = MathHelper.Lerp(this.GetStart(), this.GetEnd(), this.m_ratio);
            MyGuiManager.DrawSpriteBatch(this.m_thumbTexture, new Vector2(x, base.GetPositionAbsoluteTopLeft().Y + (base.Size.Y / 2f)), this.m_styleDef.ThumbTexture.SizeGui * ((this.DebugScale != 1f) ? (this.DebugScale * 0.5f) : this.DebugScale), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
        }

        private float GetEnd() => 
            (base.GetPositionAbsoluteTopLeft().X + (base.Size.X - (MyGuiConstants.SLIDER_INSIDE_OFFSET_X + this.m_labelSpaceWidth)));

        private float GetStart() => 
            (base.GetPositionAbsoluteTopLeft().X + MyGuiConstants.SLIDER_INSIDE_OFFSET_X);

        public static StyleDefinition GetVisualStyle(MyGuiControlSliderStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                if (!base.Enabled)
                {
                    return null;
                }
                if ((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed()) && !this.OnSliderClicked())
                {
                    this.m_controlCaptured = true;
                }
                if (MyInput.Static.IsNewPrimaryButtonReleased())
                {
                    this.m_controlCaptured = false;
                }
                if (!base.IsMouseOver)
                {
                    if (this.m_controlCaptured)
                    {
                        base2 = this;
                    }
                }
                else if (this.m_controlCaptured)
                {
                    float start = this.GetStart();
                    float end = this.GetEnd();
                    this.Ratio = (MyGuiManager.MouseCursorPosition.X - start) / (end - start);
                    base2 = this;
                }
                else if (MyInput.Static.IsNewSecondaryButtonPressed() && (this.DefaultRatio != null))
                {
                    this.Ratio = this.DefaultRatio.Value;
                    base2 = this;
                }
                if (base.HasFocus)
                {
                    if ((MyInput.Static.IsKeyPress(MyKeys.Left) || MyInput.Static.IsGamepadKeyLeftPressed()) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_lastTimeArrowPressed) > MyGuiConstants.REPEAT_PRESS_DELAY))
                    {
                        this.m_lastTimeArrowPressed = MyGuiManager.TotalTimeInMilliseconds;
                        this.Ratio -= 0.001f;
                        base2 = this;
                    }
                    if ((MyInput.Static.IsKeyPress(MyKeys.Right) || MyInput.Static.IsGamepadKeyRightPressed()) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_lastTimeArrowPressed) > MyGuiConstants.REPEAT_PRESS_DELAY))
                    {
                        this.m_lastTimeArrowPressed = MyGuiManager.TotalTimeInMilliseconds;
                        this.Ratio += 0.001f;
                        base2 = this;
                    }
                }
            }
            return base2;
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.RefreshInternals();
        }

        public override void OnRemoving()
        {
            this.SliderClicked = null;
            this.ValueChanged = null;
            base.OnRemoving();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        protected virtual bool OnSliderClicked() => 
            ((this.SliderClicked == null) ? false : this.SliderClicked(this));

        protected virtual void OnValueChange()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this);
            }
        }

        private void RefreshInternals()
        {
            if (this.m_styleDef == null)
            {
                this.m_styleDef = m_styles[0];
            }
            if (base.HasHighlight)
            {
                this.m_railTexture = this.m_styleDef.RailHighlightTexture;
                this.m_thumbTexture = this.m_styleDef.ThumbTexture.Highlight;
            }
            else
            {
                this.m_railTexture = this.m_styleDef.RailTexture;
                this.m_thumbTexture = this.m_styleDef.ThumbTexture.Normal;
            }
            base.MinSize = new Vector2(this.m_railTexture.MinSizeGui.X + this.m_labelSpaceWidth, Math.Max(this.m_railTexture.MinSizeGui.Y, this.m_label.Size.Y)) * this.DebugScale;
            base.MaxSize = new Vector2(this.m_railTexture.MaxSizeGui.X + this.m_labelSpaceWidth, Math.Max(this.m_railTexture.MaxSizeGui.Y, this.m_label.Size.Y)) * this.DebugScale;
            this.m_label.Position = new Vector2(base.Size.X * 0.5f, 0f);
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.RefreshInternals();
        }

        protected void UpdateLabel()
        {
            this.m_label.Text = this.m_props.FormatLabel(this.Value);
            this.RefreshInternals();
        }

        public MyGuiControlSliderStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public MyGuiSliderProperties Propeties
        {
            get => 
                this.m_props;
            set
            {
                this.m_props = value;
                this.Ratio = this.m_ratio;
            }
        }

        public float Ratio
        {
            get => 
                this.m_ratio;
            set
            {
                float single1 = MathHelper.Clamp(value, 0f, 1f);
                value = single1;
                if (this.m_ratio != value)
                {
                    this.m_ratio = this.m_props.RatioFilter(value);
                    this.UpdateLabel();
                    this.OnValueChange();
                }
            }
        }

        public float DebugScale
        {
            get => 
                this.m_debugScale;
            set
            {
                if (this.m_debugScale != value)
                {
                    this.m_debugScale = value;
                    this.RefreshInternals();
                }
            }
        }

        public float Value
        {
            get => 
                this.m_props.RatioToValue(this.m_ratio);
            set
            {
                float arg = this.m_props.ValueToRatio(value);
                arg = MathHelper.Clamp(this.m_props.RatioFilter(arg), 0f, 1f);
                if (arg != this.m_ratio)
                {
                    this.m_ratio = arg;
                    this.UpdateLabel();
                    this.OnValueChange();
                }
            }
        }

        public class StyleDefinition
        {
            public MyGuiCompositeTexture RailTexture;
            public MyGuiCompositeTexture RailHighlightTexture;
            public MyGuiHighlightTexture ThumbTexture;
        }
    }
}


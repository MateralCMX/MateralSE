namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlLabel))]
    public class MyGuiControlLabel : MyGuiControlBase
    {
        private StyleDefinition m_styleDefinition;
        private bool m_forceNewStringBuilder;
        private string m_font;
        private string m_text;
        private MyStringId m_textEnum;
        private float m_textScale;
        private float m_textScaleWithLanguage;
        public StringBuilder TextToDraw;
        public bool AutoEllipsis;

        public MyGuiControlLabel() : this(nullable, nullable, null, nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlLabel(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), string text = null, Vector4? colorMask = new Vector4?(), float textScale = 0.8f, string font = "Blue", MyGuiDrawAlignEnum originAlign = 1) : base(position, size, colorMask, null, null, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            base.Name = "Label";
            this.Font = font;
            if (text != null)
            {
                this.m_text = text;
                this.TextToDraw = new StringBuilder(text);
            }
            base.OriginAlign = originAlign;
            this.TextScale = textScale;
        }

        public void ApplyStyle(StyleDefinition style)
        {
            if (style != null)
            {
                this.m_styleDefinition = style;
                this.RefreshInternals();
            }
        }

        public void Autowrap(float width)
        {
            if (this.TextToDraw != null)
            {
                this.TextToDraw.Autowrap(width, this.Font, this.TextScaleWithLanguage);
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            float maxTextWidth = this.AutoEllipsis ? base.Size.X : float.PositiveInfinity;
            if (this.TextForDraw == null)
            {
                MyLog.Default.WriteLine("text shouldn't be null! MyGuiContolLabel:" + this);
            }
            else
            {
                MyGuiManager.DrawString(this.Font, this.TextForDraw, base.GetPositionAbsolute(), this.TextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha)), base.OriginAlign, false, maxTextWidth);
            }
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlLabel objectBuilder = (MyObjectBuilder_GuiControlLabel) base.GetObjectBuilder();
            objectBuilder.TextEnum = this.m_textEnum.ToString();
            objectBuilder.TextScale = this.TextScale;
            objectBuilder.Text = this.m_text;
            objectBuilder.Font = this.Font;
            return objectBuilder;
        }

        public Vector2 GetTextSize() => 
            MyGuiManager.MeasureString(this.Font, this.TextForDraw, this.TextScaleWithLanguage);

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlLabel label = (MyObjectBuilder_GuiControlLabel) objectBuilder;
            this.m_textEnum = MyStringId.GetOrCompute(label.TextEnum);
            this.TextScale = label.TextScale;
            this.m_text = string.IsNullOrWhiteSpace(label.Text) ? null : label.Text;
            this.Font = label.Font;
            this.TextToDraw = new StringBuilder();
            this.UpdateFormatParams(null);
        }

        public void PrepareForAsyncTextUpdate()
        {
            this.m_forceNewStringBuilder = true;
        }

        public void RecalculateSize()
        {
            this.RefreshInternals();
            base.Size = this.GetTextSize();
        }

        public void RefreshInternals()
        {
            if (this.m_styleDefinition != null)
            {
                this.Font = this.m_styleDefinition.Font;
                base.ColorMask = this.m_styleDefinition.ColorMask;
                this.TextScale = this.m_styleDefinition.TextScale;
            }
        }

        public void UpdateFormatParams(params object[] args)
        {
            if (this.m_text == null)
            {
                if ((this.TextToDraw == null) || this.m_forceNewStringBuilder)
                {
                    this.TextToDraw = new StringBuilder();
                }
                this.TextToDraw.Clear();
                if (args != null)
                {
                    this.TextToDraw.AppendFormat(MyTexts.GetString(this.m_textEnum), args);
                }
                else
                {
                    this.TextToDraw.Append(MyTexts.GetString(this.m_textEnum));
                }
            }
            else
            {
                if ((this.TextToDraw == null) || this.m_forceNewStringBuilder)
                {
                    this.TextToDraw = new StringBuilder();
                }
                this.TextToDraw.Clear();
                if (args != null)
                {
                    this.TextToDraw.AppendFormat(this.m_text.ToString(), args);
                }
                else
                {
                    this.TextToDraw.Append(this.m_text);
                }
            }
            this.m_forceNewStringBuilder = false;
            this.RecalculateSize();
        }

        public string Font
        {
            get => 
                this.m_font;
            set => 
                (this.m_font = value);
        }

        public string Text
        {
            get => 
                this.m_text;
            set
            {
                if (this.m_text != value)
                {
                    this.m_text = value;
                    this.UpdateFormatParams(null);
                }
            }
        }

        public MyStringId TextEnum
        {
            get => 
                this.m_textEnum;
            set
            {
                if ((this.m_textEnum != value) || (this.m_text != null))
                {
                    this.m_textEnum = value;
                    this.m_text = null;
                    this.UpdateFormatParams(null);
                }
            }
        }

        public float TextScale
        {
            get => 
                this.m_textScale;
            set
            {
                if (this.m_textScale != value)
                {
                    this.m_textScale = value;
                    this.TextScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
                    this.RecalculateSize();
                }
            }
        }

        public float TextScaleWithLanguage
        {
            get => 
                this.m_textScaleWithLanguage;
            private set => 
                (this.m_textScaleWithLanguage = value);
        }

        private StringBuilder TextForDraw =>
            (this.TextToDraw ?? MyTexts.Get(this.m_textEnum));

        public class StyleDefinition
        {
            public string Font = "Blue";
            public Vector4 ColorMask = Vector4.One;
            public float TextScale = 0.8f;
        }
    }
}


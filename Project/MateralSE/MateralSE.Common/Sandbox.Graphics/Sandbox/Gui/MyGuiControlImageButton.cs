namespace Sandbox.Gui
{
    using Sandbox;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlImageButton))]
    public class MyGuiControlImageButton : MyGuiControlBase
    {
        private StyleDefinition m_styleDefinition;
        private bool m_readyToClick;
        private bool m_readyToRightClick;
        private string m_text;
        private MyStringId m_textEnum;
        private float m_textScale;
        private float m_buttonScale;
        private bool m_activateOnMouseRelease;
        [CompilerGenerated]
        private Action<MyGuiControlImageButton> ButtonClicked;
        [CompilerGenerated]
        private Action<MyGuiControlImageButton> ButtonRightClicked;
        private StringBuilder m_drawText;
        private StringBuilder m_cornerText;
        private bool m_drawRedTextureWhenDisabled;
        private RectangleF m_internalArea;
        protected GuiSounds m_cueEnum;
        private bool m_checked;
        public bool Selected;
        private MyKeys m_boundKey;
        private bool m_allowBoundKey;
        private float m_textScaleWithLanguage;
        public MyGuiDrawAlignEnum TextAlignment;
        public string TextFont;
        public string CornerTextFont;
        public float CornerTextSize;
        public bool DrawCrossTextureWhenDisabled;
        public ButtonIcon Icon;

        public event Action<MyGuiControlImageButton> ButtonClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlImageButton> buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action<MyGuiControlImageButton> a = buttonClicked;
                    Action<MyGuiControlImageButton> action3 = (Action<MyGuiControlImageButton>) Delegate.Combine(a, value);
                    buttonClicked = Interlocked.CompareExchange<Action<MyGuiControlImageButton>>(ref this.ButtonClicked, action3, a);
                    if (ReferenceEquals(buttonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlImageButton> buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action<MyGuiControlImageButton> source = buttonClicked;
                    Action<MyGuiControlImageButton> action3 = (Action<MyGuiControlImageButton>) Delegate.Remove(source, value);
                    buttonClicked = Interlocked.CompareExchange<Action<MyGuiControlImageButton>>(ref this.ButtonClicked, action3, source);
                    if (ReferenceEquals(buttonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlImageButton> ButtonRightClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlImageButton> buttonRightClicked = this.ButtonRightClicked;
                while (true)
                {
                    Action<MyGuiControlImageButton> a = buttonRightClicked;
                    Action<MyGuiControlImageButton> action3 = (Action<MyGuiControlImageButton>) Delegate.Combine(a, value);
                    buttonRightClicked = Interlocked.CompareExchange<Action<MyGuiControlImageButton>>(ref this.ButtonRightClicked, action3, a);
                    if (ReferenceEquals(buttonRightClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlImageButton> buttonRightClicked = this.ButtonRightClicked;
                while (true)
                {
                    Action<MyGuiControlImageButton> source = buttonRightClicked;
                    Action<MyGuiControlImageButton> action3 = (Action<MyGuiControlImageButton>) Delegate.Remove(source, value);
                    buttonRightClicked = Interlocked.CompareExchange<Action<MyGuiControlImageButton>>(ref this.ButtonRightClicked, action3, source);
                    if (ReferenceEquals(buttonRightClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlImageButton() : this("Button", nullable, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, null, GuiSounds.MouseClick, 1f, nullable3, false)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlImageButton(string name = "Button", Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? colorMask = new Vector4?(), MyGuiDrawAlignEnum originAlign = 4, string toolTip = null, StringBuilder text = null, float textScale = 0.8f, MyGuiDrawAlignEnum textAlignment = 4, MyGuiControlHighlightType highlightType = 2, Action<MyGuiControlImageButton> onButtonClick = null, Action<MyGuiControlImageButton> onButtonRightClick = null, GuiSounds cueEnum = 0, float buttonScale = 1f, int? buttonIndex = new int?(), bool activateOnMouseRelease = false) : this(new Vector2?((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero), size, new Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : MyGuiConstants.BUTTON_BACKGROUND_COLOR), toolTip, null, true, true, false, highlightType, originAlign)
        {
            this.m_buttonScale = 1f;
            this.m_drawText = new StringBuilder();
            this.m_cornerText = new StringBuilder();
            this.m_drawRedTextureWhenDisabled = true;
            this.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            Vector2? nullable = position;
            Vector4? nullable2 = colorMask;
            StateDefinition definition1 = new StateDefinition();
            definition1.Texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.Active = definition1;
            StateDefinition definition3 = new StateDefinition();
            definition3.Texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL;
            definition2.Disabled = definition3;
            StateDefinition definition4 = new StateDefinition();
            definition4.Texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL;
            definition2.Normal = definition4;
            StateDefinition definition5 = new StateDefinition();
            definition5.Texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT;
            definition2.Highlight = definition5;
            StateDefinition definition6 = new StateDefinition();
            definition6.Texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT;
            definition2.ActiveHighlight = definition6;
            this.m_styleDefinition = definition2;
            this.Name = name ?? "Button";
            this.ButtonClicked = onButtonClick;
            this.ButtonRightClicked = onButtonRightClick;
            int? nullable3 = buttonIndex;
            this.Index = (nullable3 != null) ? nullable3.GetValueOrDefault() : 0;
            this.UpdateText();
            this.m_drawText.Clear().Append(text);
            this.TextScale = textScale;
            this.TextAlignment = textAlignment;
            this.m_cueEnum = cueEnum;
            this.m_activateOnMouseRelease = activateOnMouseRelease;
            this.ButtonScale = buttonScale;
            base.Size *= this.ButtonScale;
        }

        public void ApplyStyle(StyleDefinition style)
        {
            this.m_styleDefinition = style;
            this.RefreshInternals();
        }

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_internalArea.Position, this.m_internalArea.Size, Color.White, 1);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, transitionAlpha);
            if (!base.Enabled && this.DrawCrossTextureWhenDisabled)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\LockedButton.dds", base.GetPositionAbsolute(), base.Size * MyGuiConstants.LOCKBUTTON_SIZE_MODIFICATION, MyGuiConstants.DISABLED_BUTTON_COLOR, base.OriginAlign, false, true);
            }
            Vector2 topLeft = base.GetPositionAbsoluteTopLeft() + this.m_internalArea.Position;
            if (!string.IsNullOrEmpty(this.Icon.Normal))
            {
                string disabled;
                string text2;
                if (!base.Enabled)
                {
                    disabled = this.Icon.Disabled;
                }
                else if (!base.HasHighlight || !this.Checked)
                {
                    disabled = base.HasHighlight ? this.Icon.Highlight : (this.Checked ? this.Icon.Active : this.Icon.Normal);
                }
                else
                {
                    disabled = this.Icon.ActiveHighlight;
                }
                MyGuiManager.DrawSpriteBatch(text2, base.GetPositionAbsoluteCenter(), base.Size - this.m_styleDefinition.Padding.SizeChange, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            }
            Vector4 sourceColorMask = base.Enabled ? Vector4.One : MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER;
            if ((this.m_drawText.Length > 0) && (this.TextScaleWithLanguage > 0f))
            {
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromTopLeft(topLeft, this.m_internalArea.Size, this.TextAlignment);
                MyGuiManager.DrawString(this.TextFont, this.m_drawText, normalizedCoord, this.TextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(sourceColorMask, base.Enabled, transitionAlpha)), this.TextAlignment, false, float.PositiveInfinity);
            }
            if ((this.m_cornerText.Length > 0) && (this.CornerTextSize > 0f))
            {
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromTopLeft(topLeft, this.m_internalArea.Size, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                MyGuiManager.DrawString(this.CornerTextFont, this.m_cornerText, normalizedCoord, this.CornerTextSize, new Color?(ApplyColorMaskModifiers(sourceColorMask, base.Enabled, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
            }
        }

        public override string GetMouseCursorTexture() => 
            @"Textures\GUI\MouseCursor.dds";

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlImageButton objectBuilder = (MyObjectBuilder_GuiControlImageButton) base.GetObjectBuilder();
            objectBuilder.Text = this.Text;
            objectBuilder.TextEnum = this.m_textEnum.ToString();
            objectBuilder.TextScale = this.TextScale;
            objectBuilder.TextAlignment = (int) this.TextAlignment;
            objectBuilder.DrawCrossTextureWhenDisabled = this.DrawCrossTextureWhenDisabled;
            return objectBuilder;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 != null)
            {
                return base2;
            }
            else
            {
                if (this.m_activateOnMouseRelease)
                {
                    this.m_readyToClick = true;
                    this.m_readyToRightClick = true;
                }
                else
                {
                    if (base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed())
                    {
                        this.m_readyToClick = true;
                    }
                    if (!base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonReleased())
                    {
                        this.m_readyToClick = false;
                    }
                    if (base.IsMouseOver && MyInput.Static.IsNewSecondaryButtonPressed())
                    {
                        this.m_readyToRightClick = true;
                    }
                    if (!base.IsMouseOver && MyInput.Static.IsNewSecondaryButtonReleased())
                    {
                        this.m_readyToRightClick = false;
                    }
                }
                if (((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonReleased()) && this.m_readyToClick) || (base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space))))
                {
                    if (base.Enabled)
                    {
                        MyGuiSoundManager.PlaySound(this.m_cueEnum);
                        if (this.ButtonClicked != null)
                        {
                            this.ButtonClicked(this);
                        }
                    }
                    base2 = this;
                    this.m_readyToClick = false;
                    return base2;
                }
            }
            if ((base.IsMouseOver && MyInput.Static.IsNewSecondaryButtonReleased()) && this.m_readyToRightClick)
            {
                if (base.Enabled)
                {
                    MyGuiSoundManager.PlaySound(this.m_cueEnum);
                    if (this.ButtonRightClicked != null)
                    {
                        this.ButtonRightClicked(this);
                    }
                }
                base2 = this;
                this.m_readyToRightClick = false;
                return base2;
            }
            if (base.IsMouseOver && (MyInput.Static.IsPrimaryButtonPressed() || MyInput.Static.IsNewSecondaryButtonPressed()))
            {
                base2 = this;
            }
            if (((base2 == null) && (base.Enabled && (this.AllowBoundKey && (this.BoundKey != MyKeys.None)))) && MyInput.Static.IsNewKeyPressed(this.BoundKey))
            {
                MyGuiSoundManager.PlaySound(this.m_cueEnum);
                if (this.ButtonClicked != null)
                {
                    this.ButtonClicked(this);
                }
                base2 = this;
                this.m_readyToRightClick = false;
                this.m_readyToClick = false;
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlImageButton button = (MyObjectBuilder_GuiControlImageButton) objectBuilder;
            this.Text = button.Text;
            this.m_textEnum = MyStringId.GetOrCompute(button.TextEnum);
            this.TextScale = button.TextScale;
            this.TextAlignment = (MyGuiDrawAlignEnum) button.TextAlignment;
            this.DrawCrossTextureWhenDisabled = button.DrawCrossTextureWhenDisabled;
            this.UpdateText();
        }

        protected override void OnHasHighlightChanged()
        {
            this.RefreshInternals();
            base.OnHasHighlightChanged();
        }

        protected override void OnOriginAlignChanged()
        {
            base.OnOriginAlignChanged();
            this.RefreshInternals();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        protected void RaiseButtonClicked()
        {
            if (this.ButtonClicked != null)
            {
                this.ButtonClicked(this);
            }
        }

        private void RefreshInternals()
        {
            base.ColorMask = this.m_styleDefinition.BackgroundColor;
            if (!base.Enabled)
            {
                base.BackgroundTexture = this.m_styleDefinition.Disabled.Texture;
                this.TextFont = this.m_styleDefinition.Disabled.Font;
                this.CornerTextFont = this.m_styleDefinition.Disabled.CornerTextFont;
                this.CornerTextSize = this.m_styleDefinition.Disabled.CornerTextSize;
            }
            if (base.HasHighlight && this.Checked)
            {
                base.BackgroundTexture = this.m_styleDefinition.ActiveHighlight.Texture;
                this.TextFont = this.m_styleDefinition.ActiveHighlight.Font;
                this.CornerTextFont = this.m_styleDefinition.ActiveHighlight.CornerTextFont;
                this.CornerTextSize = this.m_styleDefinition.ActiveHighlight.CornerTextSize;
            }
            else if (base.HasHighlight)
            {
                base.BackgroundTexture = this.m_styleDefinition.Highlight.Texture;
                this.TextFont = this.m_styleDefinition.Highlight.Font;
                this.CornerTextFont = this.m_styleDefinition.Highlight.CornerTextFont;
                this.CornerTextSize = this.m_styleDefinition.Highlight.CornerTextSize;
            }
            else if (this.Checked)
            {
                base.BackgroundTexture = this.m_styleDefinition.Active.Texture;
                this.TextFont = this.m_styleDefinition.Active.Font;
                this.CornerTextFont = this.m_styleDefinition.Active.CornerTextFont;
                this.CornerTextSize = this.m_styleDefinition.Active.CornerTextSize;
            }
            else
            {
                base.BackgroundTexture = this.m_styleDefinition.Normal.Texture;
                this.TextFont = this.m_styleDefinition.Normal.Font;
                this.CornerTextFont = this.m_styleDefinition.Normal.CornerTextFont;
                this.CornerTextSize = this.m_styleDefinition.Normal.CornerTextSize;
            }
            Vector2 size = base.Size;
            if (base.BackgroundTexture != null)
            {
                base.MinSize = base.BackgroundTexture.MinSizeGui;
                base.MaxSize = base.BackgroundTexture.MaxSizeGui;
            }
            else
            {
                base.MinSize = Vector2.Zero;
                base.MaxSize = Vector2.PositiveInfinity;
                size = Vector2.Zero;
            }
            if ((size == Vector2.Zero) && (this.m_drawText != null))
            {
                size = MyGuiManager.MeasureString(this.TextFont, this.m_drawText, this.TextScaleWithLanguage);
            }
            MyGuiBorderThickness padding = this.m_styleDefinition.Padding;
            this.m_internalArea.Position = padding.TopLeftOffset;
            this.m_internalArea.Size = base.Size - padding.SizeChange;
            base.Size = size;
        }

        protected override bool ShouldHaveHighlight() => 
            ((base.HighlightType != MyGuiControlHighlightType.FORCED) ? base.ShouldHaveHighlight() : this.Selected);

        private void UpdateText()
        {
            if (!string.IsNullOrEmpty(this.m_text))
            {
                this.m_drawText.Clear();
                this.m_drawText.Append(this.m_text);
            }
            else
            {
                this.m_drawText.Clear();
                this.m_drawText.Append(MyTexts.GetString(this.m_textEnum));
            }
        }

        public bool Checked
        {
            get => 
                this.m_checked;
            set
            {
                this.m_checked = value;
                this.RefreshInternals();
            }
        }

        public bool ActivateOnMouseRelease
        {
            get => 
                this.m_activateOnMouseRelease;
            set => 
                (this.m_activateOnMouseRelease = value);
        }

        public MyKeys BoundKey
        {
            get => 
                this.m_boundKey;
            set => 
                (this.m_boundKey = value);
        }

        public bool AllowBoundKey
        {
            get => 
                this.m_allowBoundKey;
            set => 
                (this.m_allowBoundKey = value);
        }

        public int Index { get; private set; }

        public string Text
        {
            get => 
                this.m_text;
            set
            {
                this.m_text = value;
                this.UpdateText();
            }
        }

        public MyStringId TextEnum
        {
            get => 
                this.m_textEnum;
            set
            {
                this.m_textEnum = value;
                this.UpdateText();
            }
        }

        public string CornerText
        {
            get => 
                this.m_cornerText.ToString();
            set
            {
                this.m_cornerText.Clear();
                this.m_cornerText.Append(value);
            }
        }

        public GuiSounds CueEnum
        {
            get => 
                this.m_cueEnum;
            set => 
                (this.m_cueEnum = value);
        }

        public float TextScale
        {
            get => 
                this.m_textScale;
            set
            {
                this.m_textScale = value;
                this.TextScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
            }
        }

        protected float ButtonScale
        {
            get => 
                this.m_buttonScale;
            set => 
                (this.m_buttonScale = value);
        }

        public float TextScaleWithLanguage
        {
            get => 
                this.m_textScaleWithLanguage;
            private set => 
                (this.m_textScaleWithLanguage = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ButtonIcon
        {
            private string m_normal;
            private string m_active;
            private string m_highlight;
            private string m_activeHighlight;
            private string m_disabled;
            public string Normal
            {
                get => 
                    this.m_normal;
                set => 
                    (this.m_normal = value);
            }
            public string Active
            {
                get => 
                    (string.IsNullOrEmpty(this.m_active) ? this.Highlight : this.m_active);
                set => 
                    (this.m_active = value);
            }
            public string Highlight
            {
                get => 
                    (string.IsNullOrEmpty(this.m_highlight) ? this.m_normal : this.m_highlight);
                set => 
                    (this.m_highlight = value);
            }
            public string ActiveHighlight
            {
                get => 
                    (string.IsNullOrEmpty(this.m_activeHighlight) ? this.Highlight : this.Active);
                set => 
                    (this.m_activeHighlight = value);
            }
            public string Disabled
            {
                get => 
                    (string.IsNullOrEmpty(this.m_disabled) ? this.Normal : this.m_disabled);
                set => 
                    (this.m_disabled = value);
            }
        }

        public class StateDefinition
        {
            public MyGuiCompositeTexture Texture;
            public string Font;
            public string CornerTextFont;
            public float CornerTextSize;
        }

        public class StyleDefinition
        {
            public MyGuiControlImageButton.StateDefinition Normal;
            public MyGuiControlImageButton.StateDefinition Active;
            public MyGuiControlImageButton.StateDefinition Highlight;
            public MyGuiControlImageButton.StateDefinition ActiveHighlight;
            public MyGuiControlImageButton.StateDefinition Disabled;
            public MyGuiBorderThickness Padding;
            public Vector4 BackgroundColor = Vector4.One;
        }
    }
}


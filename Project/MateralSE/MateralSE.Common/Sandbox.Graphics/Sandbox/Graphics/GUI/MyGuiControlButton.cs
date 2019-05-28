namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlButton))]
    public class MyGuiControlButton : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles;
        private bool m_readyToClick;
        private string m_text;
        private MyStringId m_textEnum;
        private Vector2 m_textOffset;
        private float m_textScale;
        private float m_buttonScale;
        private bool m_activateOnMouseRelease;
        private float m_iconRotation;
        public bool ClickCallbackRespectsEnabledState;
        [CompilerGenerated]
        private Action<MyGuiControlButton> ButtonClicked;
        private StringBuilder m_drawText;
        private bool m_drawRedTextureWhenDisabled;
        private RectangleF m_internalArea;
        protected GuiSounds m_cueEnum;
        private bool m_checked;
        public bool Selected;
        private float m_textScaleWithLanguage;
        public MyGuiDrawAlignEnum TextAlignment;
        public string TextFont;
        public bool DrawCrossTextureWhenDisabled;
        private MyGuiControlButtonStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        public MyGuiHighlightTexture? Icon;
        public MyGuiDrawAlignEnum IconOriginAlign;
        private bool m_useCustomStyle;
        private StyleDefinition m_customStyle;

        public event Action<MyGuiControlButton> ButtonClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlButton> buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action<MyGuiControlButton> a = buttonClicked;
                    Action<MyGuiControlButton> action3 = (Action<MyGuiControlButton>) Delegate.Combine(a, value);
                    buttonClicked = Interlocked.CompareExchange<Action<MyGuiControlButton>>(ref this.ButtonClicked, action3, a);
                    if (ReferenceEquals(buttonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlButton> buttonClicked = this.ButtonClicked;
                while (true)
                {
                    Action<MyGuiControlButton> source = buttonClicked;
                    Action<MyGuiControlButton> action3 = (Action<MyGuiControlButton>) Delegate.Remove(source, value);
                    buttonClicked = Interlocked.CompareExchange<Action<MyGuiControlButton>>(ref this.ButtonClicked, action3, source);
                    if (ReferenceEquals(buttonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlButton()
        {
            MyGuiBorderThickness thickness2 = new MyGuiBorderThickness {
                Left = 7f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            MyGuiBorderThickness thickness = thickness2;
            m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlButtonStyleEnum>() + 1];
            StyleDefinition definition1 = new StyleDefinition();
            definition1.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL;
            definition1.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT;
            definition1.NormalFont = "Blue";
            definition1.HighlightFont = "White";
            definition1.Padding = thickness;
            m_styles[0] = definition1;
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.LeftTop;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.NormalTexture = texture1;
            MyGuiCompositeTexture texture2 = new MyGuiCompositeTexture(null);
            texture2.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT.LeftTop;
            definition2.HighlightTexture = texture2;
            definition2.NormalFont = "Blue";
            definition2.HighlightFont = "White";
            definition2.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * 0.75f);
            definition2.Padding = thickness;
            m_styles[1] = definition2;
            MyGuiCompositeTexture texture3 = new MyGuiCompositeTexture(null);
            texture3.Center = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_NORMAL.LeftTop;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.NormalTexture = texture3;
            MyGuiCompositeTexture texture4 = new MyGuiCompositeTexture(null);
            texture4.Center = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_HIGHLIGHT.LeftTop;
            definition3.HighlightTexture = texture4;
            definition3.NormalFont = "Blue";
            definition3.HighlightFont = "White";
            definition3.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_NORMAL.MinSizeGui * 0.75f);
            definition3.Padding = thickness;
            m_styles[13] = definition3;
            StyleDefinition definition4 = new StyleDefinition();
            definition4.NormalFont = "Blue";
            definition4.HighlightFont = "White";
            definition4.Underline = @"Textures\GUI\UnderlineHighlight.dds";
            definition4.UnderlineHighlight = @"Textures\GUI\UnderlineHighlight.dds";
            definition4.MouseOverCursor = @"Textures\GUI\MouseCursorHand.dds";
            m_styles[9] = definition4;
            StyleDefinition definition5 = new StyleDefinition();
            definition5.NormalFont = "UrlNormal";
            definition5.HighlightFont = "UrlHighlight";
            definition5.Underline = @"Textures\GUI\Underline.dds";
            definition5.UnderlineHighlight = @"Textures\GUI\UnderlineHighlight.dds";
            definition5.MouseOverCursor = @"Textures\GUI\MouseCursorHand.dds";
            m_styles[0x12] = definition5;
            StyleDefinition definition6 = new StyleDefinition();
            definition6.NormalFont = "UrlNormal";
            definition6.HighlightFont = "UrlHighlight";
            definition6.MouseOverCursor = @"Textures\GUI\MouseCursorHand.dds";
            m_styles[0x13] = definition6;
            StyleDefinition definition7 = new StyleDefinition();
            definition7.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_RED_NORMAL;
            definition7.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_RED_HIGHLIGHT;
            definition7.NormalFont = "Red";
            definition7.HighlightFont = "White";
            definition7.Padding = thickness;
            m_styles[2] = definition7;
            StyleDefinition definition8 = new StyleDefinition();
            definition8.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_CLOSE_NORMAL;
            definition8.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_CLOSE_HIGHLIGHT;
            definition8.NormalFont = "Blue";
            definition8.HighlightFont = "White";
            m_styles[3] = definition8;
            StyleDefinition definition9 = new StyleDefinition();
            definition9.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_CLOSE_BCG_NORMAL;
            definition9.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_CLOSE_BCG_HIGHLIGHT;
            definition9.NormalFont = "Blue";
            definition9.HighlightFont = "White";
            definition9.BackgroundColor = new Vector4(1f, 1f, 1f, 0.9f);
            m_styles[4] = definition9;
            StyleDefinition definition10 = new StyleDefinition();
            definition10.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_INFO_NORMAL;
            definition10.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_INFO_HIGHLIGHT;
            definition10.NormalFont = "Blue";
            definition10.HighlightFont = "White";
            m_styles[5] = definition10;
            StyleDefinition definition11 = new StyleDefinition();
            definition11.NormalTexture = MyGuiConstants.TEXTURE_INVENTORY_TRASH_NORMAL;
            definition11.HighlightTexture = MyGuiConstants.TEXTURE_INVENTORY_TRASH_HIGHLIGHT;
            definition11.NormalFont = "Blue";
            definition11.HighlightFont = "White";
            m_styles[6] = definition11;
            MyGuiCompositeTexture texture5 = new MyGuiCompositeTexture(null);
            texture5.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.LeftTop;
            StyleDefinition definition12 = new StyleDefinition();
            definition12.NormalTexture = texture5;
            MyGuiCompositeTexture texture6 = new MyGuiCompositeTexture(null);
            texture6.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT.LeftTop;
            definition12.HighlightTexture = texture6;
            definition12.NormalFont = "Blue";
            definition12.HighlightFont = "White";
            definition12.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * new Vector2(0.55f, 0.65f));
            definition12.Padding = thickness;
            m_styles[7] = definition12;
            MyGuiCompositeTexture texture7 = new MyGuiCompositeTexture(null);
            texture7.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.LeftTop;
            StyleDefinition definition13 = new StyleDefinition();
            definition13.NormalTexture = texture7;
            MyGuiCompositeTexture texture8 = new MyGuiCompositeTexture(null);
            texture8.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT.LeftTop;
            definition13.HighlightTexture = texture8;
            definition13.NormalFont = "Blue";
            definition13.HighlightFont = "White";
            definition13.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * new Vector2(0.5f, 0.8f));
            definition13.Padding = thickness;
            m_styles[8] = definition13;
            StyleDefinition definition14 = new StyleDefinition();
            definition14.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_INCREASE;
            definition14.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_INCREASE_HIGHLIGHT;
            m_styles[10] = definition14;
            StyleDefinition definition15 = new StyleDefinition();
            definition15.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_DECREASE;
            definition15.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_DECREASE_HIGHLIGHT;
            m_styles[11] = definition15;
            StyleDefinition definition16 = new StyleDefinition();
            definition16.NormalTexture = MyGuiConstants.TEXTURE_RECTANGLE_BUTTON_BORDER;
            definition16.HighlightTexture = MyGuiConstants.TEXTURE_RECTANGLE_BUTTON_HIGHLIGHTED_BORDER;
            definition16.NormalFont = "Blue";
            definition16.HighlightFont = "White";
            definition16.Padding = new MyGuiBorderThickness(5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            m_styles[12] = definition16;
            StyleDefinition definition17 = new StyleDefinition();
            definition17.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_ARROW_LEFT;
            definition17.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_ARROW_LEFT_HIGHLIGHT;
            m_styles[14] = definition17;
            StyleDefinition definition18 = new StyleDefinition();
            definition18.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_ARROW_RIGHT;
            definition18.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_ARROW_RIGHT_HIGHLIGHT;
            m_styles[15] = definition18;
            StyleDefinition definition19 = new StyleDefinition();
            definition19.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_NORMAL;
            definition19.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_HIGHLIGHT;
            m_styles[0x10] = definition19;
            StyleDefinition definition20 = new StyleDefinition();
            definition20.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_SMALL_NORMAL;
            definition20.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_SMALL_HIGHLIGHT;
            m_styles[0x11] = definition20;
            StyleDefinition definition21 = new StyleDefinition();
            definition21.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_RED_NORMAL;
            definition21.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_RED_HIGHLIGHT;
            definition21.NormalFont = "ErrorMessageBoxText";
            definition21.HighlightFont = "White";
            definition21.Padding = thickness;
            m_styles[20] = definition21;
            StyleDefinition definition22 = new StyleDefinition();
            definition22.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_BUG_NORMAL;
            definition22.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_BUG_HIGHLIGHT;
            m_styles[0x17] = definition22;
            StyleDefinition definition23 = new StyleDefinition();
            definition23.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_LIKE_NORMAL;
            definition23.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_LIKE_HIGHLIGHT;
            m_styles[0x15] = definition23;
            StyleDefinition definition24 = new StyleDefinition();
            definition24.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_ENVELOPE_NORMAL;
            definition24.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_ENVELOPE_HIGHLIGHT;
            m_styles[0x16] = definition24;
            StyleDefinition definition25 = new StyleDefinition();
            definition25.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_HELP_NORMAL;
            definition25.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_HELP_HIGHLIGHT;
            m_styles[0x18] = definition25;
            StyleDefinition definition26 = new StyleDefinition();
            definition26.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_STRIPE_LEFT_NORMAL;
            definition26.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_STRIPE_LEFT_NORMAL_HIGHLIGHT;
            definition26.NormalFont = "Blue";
            definition26.HighlightFont = "White";
            thickness2 = new MyGuiBorderThickness {
                Left = 11.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition26.Padding = thickness2;
            definition26.BackgroundColor = new Vector4(1f, 1f, 1f, 0.9f);
            m_styles[0x19] = definition26;
            StyleDefinition definition27 = new StyleDefinition();
            definition27.NormalTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_48_NORMAL;
            definition27.HighlightTexture = MyGuiConstants.TEXTURE_BUTTON_SQUARE_48_HIGHLIGHT;
            m_styles[0x1c] = definition27;
            MyGuiCompositeTexture texture9 = new MyGuiCompositeTexture(null);
            texture9.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.LeftTop;
            StyleDefinition definition28 = new StyleDefinition();
            definition28.NormalTexture = texture9;
            MyGuiCompositeTexture texture10 = new MyGuiCompositeTexture(null);
            texture10.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT.LeftTop;
            definition28.HighlightTexture = texture10;
            definition28.NormalFont = "Blue";
            definition28.HighlightFont = "White";
            definition28.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * 0.78f);
            definition28.Padding = thickness;
            m_styles[0x1a] = definition28;
            MyGuiCompositeTexture texture11 = new MyGuiCompositeTexture(null);
            texture11.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.LeftTop;
            StyleDefinition definition29 = new StyleDefinition();
            definition29.NormalTexture = texture11;
            MyGuiCompositeTexture texture12 = new MyGuiCompositeTexture(null);
            texture12.Center = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT.LeftTop;
            definition29.HighlightTexture = texture12;
            definition29.NormalFont = "Blue";
            definition29.HighlightFont = "White";
            definition29.SizeOverride = new Vector2?((MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * 0.815f) * new Vector2(1.05f, 1f));
            definition29.Padding = thickness;
            m_styles[0x1b] = definition29;
        }

        public MyGuiControlButton() : this(nullable, MyGuiControlButtonStyleEnum.Default, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlButton(Vector2? position = new Vector2?(), MyGuiControlButtonStyleEnum visualStyle = 0, Vector2? size = new Vector2?(), Vector4? colorMask = new Vector4?(), MyGuiDrawAlignEnum originAlign = 4, string toolTip = null, StringBuilder text = null, float textScale = 0.8f, MyGuiDrawAlignEnum textAlignment = 4, MyGuiControlHighlightType highlightType = 2, Action<MyGuiControlButton> onButtonClick = null, GuiSounds cueEnum = 0, float buttonScale = 1f, int? buttonIndex = new int?(), bool activateOnMouseRelease = false) : this(new Vector2?((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero), size, new Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : MyGuiConstants.BUTTON_BACKGROUND_COLOR), toolTip, null, true, true, false, highlightType, originAlign)
        {
            this.m_buttonScale = 1f;
            this.ClickCallbackRespectsEnabledState = true;
            this.m_drawText = new StringBuilder();
            this.m_drawRedTextureWhenDisabled = true;
            this.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            Vector2? nullable = position;
            Vector4? nullable2 = colorMask;
            base.Name = "Button";
            this.ButtonClicked = onButtonClick;
            int? nullable3 = buttonIndex;
            this.Index = (nullable3 != null) ? nullable3.GetValueOrDefault() : 0;
            this.UpdateText();
            this.m_drawText.Clear().Append(text);
            if (text != null)
            {
                this.Text = text.ToString();
            }
            this.TextScale = textScale;
            this.TextAlignment = textAlignment;
            this.VisualStyle = visualStyle;
            this.m_cueEnum = cueEnum;
            this.m_activateOnMouseRelease = activateOnMouseRelease;
            this.ButtonScale = buttonScale;
            base.Size *= this.ButtonScale;
        }

        private void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_internalArea.Position, this.m_internalArea.Size, Color.White, 1);
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if (base.BackgroundTexture == null)
            {
                base.MinSize = Vector2.Zero;
                base.MaxSize = Vector2.PositiveInfinity;
                Vector2? sizeOverride = this.m_styleDef.SizeOverride;
                this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : Vector2.Zero;
            }
            base.Position -= this.m_internalArea.Position / 2f;
            base.Draw(transitionAlpha, transitionAlpha);
            Vector4 one = Vector4.One;
            if (!base.Enabled && this.DrawCrossTextureWhenDisabled)
            {
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\LockedButton.dds", base.GetPositionAbsolute(), base.Size * MyGuiConstants.LOCKBUTTON_SIZE_MODIFICATION, MyGuiConstants.DISABLED_BUTTON_COLOR, base.OriginAlign, false, true);
            }
            Color color = new Color(1f, 1f, 1f, transitionAlpha);
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            Vector2 topLeft = positionAbsoluteTopLeft + this.m_internalArea.Position;
            Vector2 size = this.m_internalArea.Size;
            if (this.Icon != null)
            {
                Vector2 positionAbsoluteCenter = base.GetPositionAbsoluteCenter();
                MyGuiHighlightTexture texture = this.Icon.Value;
                Vector2 vector6 = Vector2.Min(texture.SizeGui, size) / texture.SizeGui;
                float num = Math.Min(vector6.X, vector6.Y);
                MyGuiManager.DrawSpriteBatch(base.HasHighlight ? texture.Highlight : texture.Normal, positionAbsoluteCenter, texture.SizeGui * num, color, this.IconOriginAlign, this.IconRotation, true);
            }
            if ((this.m_drawText.Length > 0) && (this.TextScaleWithLanguage > 0f))
            {
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromTopLeft(topLeft, this.m_internalArea.Size, this.TextAlignment);
                MyGuiManager.DrawString(this.TextFont, this.m_drawText, normalizedCoord, this.TextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(one, base.Enabled, transitionAlpha)), this.TextAlignment, false, float.PositiveInfinity);
            }
            if (this.m_styleDef.Underline != null)
            {
                Vector2 normalizedCoord = positionAbsoluteTopLeft;
                float* singlePtr1 = (float*) ref normalizedCoord.Y;
                singlePtr1[0] += base.Size.Y;
                Vector2 normalizedSize = new Vector2(MyGuiManager.MeasureString(this.TextFont, this.m_drawText, this.TextScaleWithLanguage).X, MyGuiManager.GetNormalizedSizeFromScreenSize(new Vector2(0f, 2f)).Y);
                MyGuiManager.DrawSpriteBatch(base.HasHighlight ? this.m_styleDef.UnderlineHighlight : this.m_styleDef.Underline, normalizedCoord, normalizedSize, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            }
            base.Position += this.m_internalArea.Position / 2f;
        }

        public override string GetMouseCursorTexture() => 
            (!base.IsMouseOver ? @"Textures\GUI\MouseCursor.dds" : this.m_styleDef.MouseOverCursor);

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlButton objectBuilder = (MyObjectBuilder_GuiControlButton) base.GetObjectBuilder();
            objectBuilder.Text = this.Text;
            objectBuilder.TextEnum = this.m_textEnum.ToString();
            objectBuilder.TextScale = this.TextScale;
            objectBuilder.TextAlignment = (int) this.TextAlignment;
            objectBuilder.DrawCrossTextureWhenDisabled = this.DrawCrossTextureWhenDisabled;
            objectBuilder.VisualStyle = this.VisualStyle;
            return objectBuilder;
        }

        public static StyleDefinition GetVisualStyle(MyGuiControlButtonStyleEnum style) => 
            m_styles[(int) style];

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
                }
                if (((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonReleased()) && this.m_readyToClick) || (base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space))))
                {
                    if (base.Enabled || !this.ClickCallbackRespectsEnabledState)
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
            if (base.IsMouseOver && MyInput.Static.IsPrimaryButtonPressed())
            {
                base2 = this;
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlButton button = (MyObjectBuilder_GuiControlButton) objectBuilder;
            this.Text = button.Text;
            this.m_textEnum = MyStringId.GetOrCompute(button.TextEnum);
            this.TextScale = button.TextScale;
            this.TextAlignment = (MyGuiDrawAlignEnum) button.TextAlignment;
            this.DrawCrossTextureWhenDisabled = button.DrawCrossTextureWhenDisabled;
            this.VisualStyle = button.VisualStyle;
            this.UpdateText();
        }

        protected override void OnHasHighlightChanged()
        {
            this.RefreshVisualStyle();
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
            Vector2? sizeOverride;
            base.ColorMask = this.m_styleDef.BackgroundColor;
            if (base.HasHighlight || this.Checked)
            {
                base.BackgroundTexture = this.m_styleDef.HighlightTexture;
                this.TextFont = this.m_styleDef.HighlightFont;
            }
            else
            {
                base.BackgroundTexture = this.m_styleDef.NormalTexture;
                this.TextFont = this.m_styleDef.NormalFont;
            }
            Vector2 size = base.Size;
            if (base.BackgroundTexture == null)
            {
                base.MinSize = Vector2.Zero;
                base.MaxSize = Vector2.PositiveInfinity;
                sizeOverride = this.m_styleDef.SizeOverride;
                size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : Vector2.Zero;
            }
            else
            {
                base.MinSize = base.BackgroundTexture.MinSizeGui;
                base.MaxSize = base.BackgroundTexture.MaxSizeGui;
                if (this.ButtonScale == 1f)
                {
                    sizeOverride = this.m_styleDef.SizeOverride;
                    size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : size;
                }
            }
            if ((size == Vector2.Zero) && (this.m_drawText != null))
            {
                size = MyGuiManager.MeasureString(this.TextFont, this.m_drawText, this.TextScaleWithLanguage);
            }
            MyGuiBorderThickness padding = this.m_styleDef.Padding;
            this.m_internalArea.Position = padding.TopLeftOffset;
            this.m_internalArea.Size = base.Size - padding.SizeChange;
            base.Size = size;
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = !this.m_useCustomStyle ? GetVisualStyle(this.VisualStyle) : this.m_customStyle;
            this.RefreshInternals();
        }

        public void SetCustomStyle(StyleDefinition buttonStyle)
        {
            this.m_useCustomStyle = true;
            this.m_customStyle = buttonStyle;
            this.RefreshVisualStyle();
        }

        protected override bool ShouldHaveHighlight() => 
            ((base.HighlightType != MyGuiControlHighlightType.CUSTOM) ? ((base.HighlightType != MyGuiControlHighlightType.FORCED) ? base.ShouldHaveHighlight() : this.Selected) : base.HasHighlight);

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

        public float IconRotation
        {
            get => 
                this.m_iconRotation;
            set
            {
                this.m_iconRotation = value;
                this.RefreshInternals();
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

        public MyGuiControlButtonStyleEnum VisualStyle
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
            public string NormalFont = "Blue";
            public string HighlightFont = "White";
            public MyGuiCompositeTexture NormalTexture;
            public MyGuiCompositeTexture HighlightTexture;
            public Vector2? SizeOverride;
            public MyGuiBorderThickness Padding;
            public Vector4 BackgroundColor = Vector4.One;
            public string Underline;
            public string UnderlineHighlight;
            public string MouseOverCursor = @"Textures\GUI\MouseCursor.dds";
        }
    }
}


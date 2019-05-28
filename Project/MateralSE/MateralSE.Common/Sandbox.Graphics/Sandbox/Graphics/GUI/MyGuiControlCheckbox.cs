namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlCheckbox))]
    public class MyGuiControlCheckbox : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlCheckboxStyleEnum>() + 1];
        public Action<MyGuiControlCheckbox> IsCheckedChanged;
        private bool m_isChecked;
        private MyGuiControlCheckboxStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private MyGuiHighlightTexture m_icon;

        static MyGuiControlCheckbox()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.NormalCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED;
            definition1.NormalUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED;
            definition1.HighlightCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED;
            definition1.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED;
            m_styles[0] = definition1;
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED.LeftTop;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.NormalCheckedTexture = texture1;
            MyGuiCompositeTexture texture2 = new MyGuiCompositeTexture(null);
            texture2.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.LeftTop;
            definition2.NormalUncheckedTexture = texture2;
            MyGuiCompositeTexture texture3 = new MyGuiCompositeTexture(null);
            texture3.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED.LeftTop;
            definition2.HighlightCheckedTexture = texture3;
            MyGuiCompositeTexture texture4 = new MyGuiCompositeTexture(null);
            texture4.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED.LeftTop;
            definition2.HighlightUncheckedTexture = texture4;
            definition2.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.MinSizeGui * 0.65f);
            m_styles[1] = definition2;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.NormalCheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_HIGHLIGHT;
            definition3.NormalUncheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_NORMAL;
            definition3.HighlightCheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_HIGHLIGHT;
            definition3.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_LEFT_NORMAL;
            m_styles[2] = definition3;
            StyleDefinition definition4 = new StyleDefinition();
            definition4.NormalCheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_RIGHT_HIGHLIGHT;
            definition4.NormalUncheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_RIGHT_NORMAL;
            definition4.HighlightCheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_RIGHT_HIGHLIGHT;
            definition4.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_SWITCHONOFF_RIGHT_NORMAL;
            m_styles[3] = definition4;
            StyleDefinition definition5 = new StyleDefinition();
            definition5.NormalCheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            definition5.NormalUncheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            definition5.HighlightCheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition5.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition5.CheckedIcon = MyGuiConstants.TEXTURE_BUTTON_ICON_REPEAT;
            definition5.UncheckedIcon = MyGuiConstants.TEXTURE_BUTTON_ICON_REPEAT_INACTIVE;
            definition5.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_ICON_REPEAT.SizeGui * 1.4f);
            m_styles[4] = definition5;
            StyleDefinition definition6 = new StyleDefinition();
            definition6.NormalCheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            definition6.NormalUncheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            definition6.HighlightCheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition6.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            definition6.CheckedIcon = MyGuiConstants.TEXTURE_BUTTON_ICON_SLAVE;
            definition6.UncheckedIcon = MyGuiConstants.TEXTURE_BUTTON_ICON_SLAVE_INACTIVE;
            definition6.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_BUTTON_ICON_SLAVE.SizeGui * 1.4f);
            m_styles[5] = definition6;
            MyGuiCompositeTexture texture5 = new MyGuiCompositeTexture(null);
            texture5.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED.LeftTop;
            StyleDefinition definition7 = new StyleDefinition();
            definition7.NormalCheckedTexture = texture5;
            MyGuiCompositeTexture texture6 = new MyGuiCompositeTexture(null);
            texture6.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.LeftTop;
            definition7.NormalUncheckedTexture = texture6;
            MyGuiCompositeTexture texture7 = new MyGuiCompositeTexture(null);
            texture7.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED.LeftTop;
            definition7.HighlightCheckedTexture = texture7;
            MyGuiCompositeTexture texture8 = new MyGuiCompositeTexture(null);
            texture8.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED.LeftTop;
            definition7.HighlightUncheckedTexture = texture8;
            definition7.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.MinSizeGui * 0.65f);
            m_styles[6] = definition7;
        }

        public MyGuiControlCheckbox(Vector2? position = new Vector2?(), Vector4? color = new Vector4?(), string toolTip = null, bool isChecked = false, MyGuiControlCheckboxStyleEnum visualStyle = 0, MyGuiDrawAlignEnum originAlign = 4) : this(new Vector2?((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero), nullable, color, str, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
        {
            Vector2? nullable = position;
            nullable = null;
            string str = toolTip;
            base.Name = "CheckBox";
            this.m_isChecked = isChecked;
            this.VisualStyle = visualStyle;
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
            base.Draw(transitionAlpha, transitionAlpha);
            string str = base.HasHighlight ? this.m_icon.Highlight : this.m_icon.Normal;
            if (!string.IsNullOrEmpty(str))
            {
                MyGuiManager.DrawSpriteBatch(str, base.GetPositionAbsoluteCenter(), this.m_icon.SizeGui, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
            }
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlCheckbox objectBuilder = (MyObjectBuilder_GuiControlCheckbox) base.GetObjectBuilder();
            objectBuilder.IsChecked = this.m_isChecked;
            objectBuilder.VisualStyle = this.VisualStyle;
            return objectBuilder;
        }

        public static StyleDefinition GetVisualStyle(MyGuiControlCheckboxStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            if (!base.IsHitTestVisible)
            {
                return null;
            }
            MyGuiControlBase base2 = base.HandleInput();
            if (((base2 == null) && base.Enabled) && ((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed()) || (base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space)))))
            {
                this.UserCheck();
                base2 = this;
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlCheckbox checkbox = (MyObjectBuilder_GuiControlCheckbox) objectBuilder;
            this.m_isChecked = checkbox.IsChecked;
            this.VisualStyle = checkbox.VisualStyle;
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.RefreshInternals();
        }

        private void RefreshInternals()
        {
            Vector2? sizeOverride;
            if (this.m_styleDef == null)
            {
                this.m_styleDef = m_styles[0];
            }
            if (this.IsChecked)
            {
                base.BackgroundTexture = !base.HasHighlight ? this.m_styleDef.NormalCheckedTexture : this.m_styleDef.HighlightCheckedTexture;
                this.m_icon = this.m_styleDef.CheckedIcon;
                sizeOverride = this.m_styleDef.SizeOverride;
                this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
            }
            else
            {
                base.BackgroundTexture = !base.HasHighlight ? this.m_styleDef.NormalUncheckedTexture : this.m_styleDef.HighlightUncheckedTexture;
                this.m_icon = this.m_styleDef.UncheckedIcon;
                sizeOverride = this.m_styleDef.SizeOverride;
                this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
            }
            base.MinSize = base.BackgroundTexture.MinSizeGui;
            base.MaxSize = base.BackgroundTexture.MaxSizeGui;
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.RefreshInternals();
        }

        private void UserCheck()
        {
            MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
            this.IsChecked = !this.IsChecked;
        }

        public bool IsChecked
        {
            get => 
                this.m_isChecked;
            set
            {
                if (this.m_isChecked != value)
                {
                    this.m_isChecked = value;
                    this.RefreshInternals();
                    if (this.IsCheckedChanged != null)
                    {
                        this.IsCheckedChanged(this);
                    }
                }
            }
        }

        public MyGuiControlCheckboxStyleEnum VisualStyle
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
            public MyGuiCompositeTexture NormalCheckedTexture;
            public MyGuiCompositeTexture NormalUncheckedTexture;
            public MyGuiCompositeTexture HighlightCheckedTexture;
            public MyGuiCompositeTexture HighlightUncheckedTexture;
            public MyGuiHighlightTexture CheckedIcon;
            public MyGuiHighlightTexture UncheckedIcon;
            public Vector2? SizeOverride;
        }
    }
}


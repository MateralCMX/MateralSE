namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlIndeterminateCheckbox))]
    public class MyGuiControlIndeterminateCheckbox : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlIndeterminateCheckboxStyleEnum>() + 1];
        public Action<MyGuiControlIndeterminateCheckbox> IsCheckedChanged;
        private CheckStateEnum m_state;
        private MyGuiControlIndeterminateCheckboxStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private MyGuiHighlightTexture m_icon;

        static MyGuiControlIndeterminateCheckbox()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.NormalCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED;
            definition1.NormalUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED;
            definition1.NormalIndeterminateTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_INDETERMINATE;
            definition1.HighlightCheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED;
            definition1.HighlightUncheckedTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED;
            definition1.HighlightIndeterminateTexture = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_INDETERMINATE;
            m_styles[0] = definition1;
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED.LeftTop;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.NormalCheckedTexture = texture1;
            MyGuiCompositeTexture texture2 = new MyGuiCompositeTexture(null);
            texture2.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.LeftTop;
            definition2.NormalUncheckedTexture = texture2;
            MyGuiCompositeTexture texture3 = new MyGuiCompositeTexture(null);
            texture3.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_INDETERMINATE.LeftTop;
            definition2.NormalIndeterminateTexture = texture3;
            MyGuiCompositeTexture texture4 = new MyGuiCompositeTexture(null);
            texture4.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED.LeftTop;
            definition2.HighlightCheckedTexture = texture4;
            MyGuiCompositeTexture texture5 = new MyGuiCompositeTexture(null);
            texture5.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED.LeftTop;
            definition2.HighlightUncheckedTexture = texture5;
            MyGuiCompositeTexture texture6 = new MyGuiCompositeTexture(null);
            texture6.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_INDETERMINATE.LeftTop;
            definition2.HighlightIndeterminateTexture = texture6;
            definition2.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.MinSizeGui * 0.65f);
            m_styles[1] = definition2;
            MyGuiCompositeTexture texture7 = new MyGuiCompositeTexture(null);
            texture7.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED.LeftTop;
            StyleDefinition definition3 = new StyleDefinition();
            definition3.NormalCheckedTexture = texture7;
            MyGuiCompositeTexture texture8 = new MyGuiCompositeTexture(null);
            texture8.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.LeftTop;
            definition3.NormalUncheckedTexture = texture8;
            MyGuiCompositeTexture texture9 = new MyGuiCompositeTexture(null);
            texture9.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_INDETERMINATE.LeftTop;
            definition3.NormalIndeterminateTexture = texture9;
            MyGuiCompositeTexture texture10 = new MyGuiCompositeTexture(null);
            texture10.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED.LeftTop;
            definition3.HighlightCheckedTexture = texture10;
            MyGuiCompositeTexture texture11 = new MyGuiCompositeTexture(null);
            texture11.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED.LeftTop;
            definition3.HighlightUncheckedTexture = texture11;
            MyGuiCompositeTexture texture12 = new MyGuiCompositeTexture(null);
            texture12.Center = MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_INDETERMINATE.LeftTop;
            definition3.HighlightIndeterminateTexture = texture12;
            definition3.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED.MinSizeGui * 0.65f);
            m_styles[2] = definition3;
        }

        public MyGuiControlIndeterminateCheckbox(Vector2? position = new Vector2?(), Vector4? color = new Vector4?(), string toolTip = null, CheckStateEnum state = 1, MyGuiControlIndeterminateCheckboxStyleEnum visualStyle = 0, MyGuiDrawAlignEnum originAlign = 4) : this(new Vector2?((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero), nullable, color, str, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
        {
            Vector2? nullable = position;
            nullable = null;
            string str = toolTip;
            base.Name = "CheckBox";
            this.m_state = state;
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
            MyObjectBuilder_GuiControlIndeterminateCheckbox objectBuilder = (MyObjectBuilder_GuiControlIndeterminateCheckbox) base.GetObjectBuilder();
            objectBuilder.State = this.m_state;
            objectBuilder.VisualStyle = this.VisualStyle;
            return objectBuilder;
        }

        public static StyleDefinition GetVisualStyle(MyGuiControlIndeterminateCheckboxStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            if (!base.Enabled)
            {
                return null;
            }
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                if ((base.IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed()) || (base.HasFocus && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space))))
                {
                    this.UserCheck(true);
                    return this;
                }
                if (base.IsMouseOver && MyInput.Static.IsNewSecondaryButtonPressed())
                {
                    this.UserCheck(false);
                    base2 = this;
                }
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlIndeterminateCheckbox checkbox = (MyObjectBuilder_GuiControlIndeterminateCheckbox) objectBuilder;
            this.m_state = checkbox.State;
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
            switch (this.State)
            {
                case CheckStateEnum.Checked:
                    base.BackgroundTexture = !base.HasHighlight ? this.m_styleDef.NormalCheckedTexture : this.m_styleDef.HighlightCheckedTexture;
                    this.m_icon = this.m_styleDef.CheckedIcon;
                    sizeOverride = this.m_styleDef.SizeOverride;
                    this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
                    break;

                case CheckStateEnum.Unchecked:
                    base.BackgroundTexture = !base.HasHighlight ? this.m_styleDef.NormalUncheckedTexture : this.m_styleDef.HighlightUncheckedTexture;
                    this.m_icon = this.m_styleDef.UncheckedIcon;
                    sizeOverride = this.m_styleDef.SizeOverride;
                    this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
                    break;

                case CheckStateEnum.Indeterminate:
                    base.BackgroundTexture = !base.HasHighlight ? this.m_styleDef.NormalIndeterminateTexture : this.m_styleDef.HighlightIndeterminateTexture;
                    this.m_icon = this.m_styleDef.IndeterimateIcon;
                    sizeOverride = this.m_styleDef.SizeOverride;
                    this.Size = (sizeOverride != null) ? sizeOverride.GetValueOrDefault() : base.BackgroundTexture.MinSizeGui;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            base.MinSize = base.BackgroundTexture.MinSizeGui;
            base.MaxSize = base.BackgroundTexture.MaxSizeGui;
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.RefreshInternals();
        }

        private void UserCheck(bool primary = true)
        {
            MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
            if (!primary)
            {
                this.State = CheckStateEnum.Unchecked;
            }
            else
            {
                switch (this.State)
                {
                    case CheckStateEnum.Checked:
                        this.State = CheckStateEnum.Indeterminate;
                        return;

                    case CheckStateEnum.Unchecked:
                        this.State = CheckStateEnum.Checked;
                        return;

                    case CheckStateEnum.Indeterminate:
                        this.State = CheckStateEnum.Unchecked;
                        return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public CheckStateEnum State
        {
            get => 
                this.m_state;
            set
            {
                if (this.m_state != value)
                {
                    this.m_state = value;
                    this.RefreshInternals();
                    if (this.IsCheckedChanged != null)
                    {
                        this.IsCheckedChanged(this);
                    }
                }
            }
        }

        public MyGuiControlIndeterminateCheckboxStyleEnum VisualStyle
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
            public MyGuiCompositeTexture NormalIndeterminateTexture;
            public MyGuiCompositeTexture HighlightCheckedTexture;
            public MyGuiCompositeTexture HighlightUncheckedTexture;
            public MyGuiCompositeTexture HighlightIndeterminateTexture;
            public MyGuiHighlightTexture CheckedIcon;
            public MyGuiHighlightTexture UncheckedIcon;
            public MyGuiHighlightTexture IndeterimateIcon;
            public Vector2? SizeOverride;
        }
    }
}


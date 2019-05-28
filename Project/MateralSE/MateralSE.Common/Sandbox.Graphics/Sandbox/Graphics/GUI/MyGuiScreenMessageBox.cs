namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenMessageBox : MyGuiScreenBase
    {
        private static readonly Style[] m_styles = new Style[MyUtils.GetMaxValueFromEnum<MyMessageBoxStyleEnum>() + 1];
        public Action<ResultEnum> ResultCallback;
        private MyStringId m_yesButtonText;
        private MyStringId m_noButtonText;
        private MyStringId m_okButtonText;
        private MyStringId m_cancelButtonText;
        private MyMessageBoxButtonsType m_buttonType;
        private MyMessageBoxStyleEnum m_type;
        private int m_timeoutInMiliseconds;
        private int m_timeoutStartedTimeInMiliseconds;
        private MyGuiControlMultilineText m_messageBoxText;
        private MyGuiControlCheckbox m_showAgainCheckBox;
        private string m_formatText;
        private StringBuilder m_formattedCache;
        private Style m_style;
        private StringBuilder m_messageText;
        private StringBuilder m_messageCaption;
        private ResultEnum m_focusedResult;

        static MyGuiScreenMessageBox()
        {
            Style style1 = new Style();
            style1.BackgroundTexture = MyGuiConstants.TEXTURE_MESSAGEBOX_BACKGROUND_INFO;
            style1.CaptionFont = "InfoMessageBoxCaption";
            style1.TextFont = "InfoMessageBoxText";
            style1.ButtonStyle = MyGuiControlButtonStyleEnum.Default;
            m_styles[0] = style1;
            Style style2 = new Style();
            style2.BackgroundTexture = MyGuiConstants.TEXTURE_MESSAGEBOX_BACKGROUND_INFO;
            style2.CaptionFont = "InfoMessageBoxCaption";
            style2.TextFont = "InfoMessageBoxText";
            style2.ButtonStyle = MyGuiControlButtonStyleEnum.Default;
            m_styles[1] = style2;
        }

        public MyGuiScreenMessageBox(MyMessageBoxStyleEnum styleEnum, MyMessageBoxButtonsType buttonType, StringBuilder messageText, StringBuilder messageCaption, MyStringId okButtonText, MyStringId cancelButtonText, MyStringId yesButtonText, MyStringId noButtonText, Action<ResultEnum> callback, int timeoutInMiliseconds, ResultEnum focusedResult, bool canHideOthers, Vector2? size, float backgroundTransition = 0f, float guiTransition = 0f) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, true, null, backgroundTransition, guiTransition)
        {
            this.InstantClose = true;
            this.m_style = m_styles[(int) styleEnum];
            this.m_focusedResult = focusedResult;
            base.m_backgroundColor = new Vector4(1f, 1f, 1f, 0.95f);
            base.m_backgroundTexture = this.m_style.BackgroundTexture.Texture;
            base.EnabledBackgroundFade = true;
            this.m_buttonType = buttonType;
            this.m_okButtonText = okButtonText;
            this.m_cancelButtonText = cancelButtonText;
            this.m_yesButtonText = yesButtonText;
            this.m_noButtonText = noButtonText;
            this.ResultCallback = callback;
            base.m_drawEvenWithoutFocus = true;
            base.CanBeHidden = false;
            this.CanHideOthers = canHideOthers;
            base.m_size = (size == null) ? new Vector2?(this.m_style.BackgroundTexture.SizeGui) : size;
            this.m_messageText = messageText;
            this.m_messageCaption = messageCaption ?? new StringBuilder();
            this.RecreateControls(true);
            if ((buttonType == MyMessageBoxButtonsType.YES_NO_TIMEOUT) || (buttonType == MyMessageBoxButtonsType.NONE_TIMEOUT))
            {
                this.m_timeoutStartedTimeInMiliseconds = MyGuiManager.TotalTimeInMilliseconds;
                this.m_timeoutInMiliseconds = timeoutInMiliseconds;
                this.m_formatText = messageText.ToString();
                this.m_formattedCache = new StringBuilder(this.m_formatText.Length);
            }
        }

        private void CallResultCallback(ResultEnum val)
        {
            if (this.ResultCallback != null)
            {
                this.ResultCallback(val);
            }
        }

        protected override void Canceling()
        {
            base.Canceling();
            switch (this.m_buttonType)
            {
                case MyMessageBoxButtonsType.NONE:
                case MyMessageBoxButtonsType.NONE_TIMEOUT:
                    break;

                case MyMessageBoxButtonsType.OK:
                    this.CallResultCallback(ResultEnum.YES);
                    return;

                case MyMessageBoxButtonsType.YES_NO:
                case MyMessageBoxButtonsType.YES_NO_TIMEOUT:
                    this.CallResultCallback(ResultEnum.NO);
                    return;

                case MyMessageBoxButtonsType.YES_NO_CANCEL:
                    this.CallResultCallback(ResultEnum.CANCEL);
                    break;

                default:
                    return;
            }
        }

        private void CloseInternal()
        {
            if (this.InstantClose)
            {
                this.CloseScreenNow();
            }
            else
            {
                this.CloseScreen();
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMessageBox";

        private MyGuiControlButton MakeButton(Vector2 position, Style config, MyStringId text, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum align)
        {
            StringBuilder builder = MyTexts.Get(text);
            Vector2? size = null;
            Vector4? colorMask = null;
            return new MyGuiControlButton(new Vector2?(position), config.ButtonStyle, size, colorMask, align, null, builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        public void OnCancelClick(MyGuiControlButton sender)
        {
            this.OnClick(ResultEnum.CANCEL);
        }

        private void OnClick(ResultEnum result)
        {
            if (this.CloseBeforeCallback)
            {
                this.CloseInternal();
                this.CallResultCallback(result);
            }
            else
            {
                this.CallResultCallback(result);
                this.CloseInternal();
            }
        }

        public void OnNoClick(MyGuiControlButton sender)
        {
            this.OnClick(ResultEnum.NO);
        }

        public void OnYesClick(MyGuiControlButton sender)
        {
            this.OnClick(ResultEnum.YES);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(this.m_style.ButtonStyle).NormalTexture.MinSizeGui;
            Vector2 vector2 = MyGuiManager.MeasureString(this.m_style.CaptionFont, this.m_messageCaption, 0.8f);
            Vector2 paddingSizeGui = this.m_style.BackgroundTexture.PaddingSizeGui;
            Vector2? size = null;
            Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(0f, ((-0.5f * base.m_size.Value.Y) + paddingSizeGui.Y) + 0.019f), size, this.m_messageCaption.ToString(), colorMask, 0.8f, this.m_style.CaptionFont, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            if (this.m_style.ButtonStyle == MyGuiControlButtonStyleEnum.Error)
            {
                list.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.691f) / 2f, (base.m_size.Value.Y / 2f) - 0.072f), base.m_size.Value.X * 0.691f, 0f, new Vector4(0.57f, 0.39f, 0.37f, 1f));
            }
            else
            {
                colorMask = null;
                list.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.691f) / 2f, (base.m_size.Value.Y / 2f) - 0.072f), base.m_size.Value.X * 0.691f, 0f, colorMask);
            }
            this.Controls.Add(list);
            StringBuilder messageText = this.m_messageText;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_messageBoxText = new MyGuiControlMultilineText(new Vector2?(Vector2.Zero), new Vector2(base.m_size.Value.X - (2f * paddingSizeGui.X), base.m_size.Value.Y - (((2f * paddingSizeGui.Y) + vector2.Y) + minSizeGui.Y)), new Vector4?(Vector4.One), this.m_style.TextFont, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, messageText, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            this.m_messageBoxText.PositionY -= 0.013f;
            this.Controls.Add(this.m_messageBoxText);
            float y = ((0.5f * base.m_size.Value.Y) - paddingSizeGui.Y) - 0.013f;
            float x = 0.01275f;
            MyGuiControlBase base2 = null;
            MyGuiControlBase base3 = null;
            MyGuiControlBase base4 = null;
            switch (this.m_buttonType)
            {
                case MyMessageBoxButtonsType.NONE:
                case MyMessageBoxButtonsType.NONE_TIMEOUT:
                    break;

                case MyMessageBoxButtonsType.OK:
                    this.Controls.Add(base2 = this.MakeButton(new Vector2(0f, y), this.m_style, this.m_okButtonText, new Action<MyGuiControlButton>(this.OnYesClick), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM));
                    break;

                case MyMessageBoxButtonsType.YES_NO:
                case MyMessageBoxButtonsType.YES_NO_TIMEOUT:
                    this.Controls.Add(base2 = this.MakeButton(new Vector2(-x, y), this.m_style, this.m_yesButtonText, new Action<MyGuiControlButton>(this.OnYesClick), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM));
                    this.Controls.Add(base3 = this.MakeButton(new Vector2(x, y), this.m_style, this.m_noButtonText, new Action<MyGuiControlButton>(this.OnNoClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM));
                    break;

                case MyMessageBoxButtonsType.YES_NO_CANCEL:
                {
                    x = 0.02f;
                    this.Controls.Add(base2 = this.MakeButton(new Vector2(-(x + (minSizeGui.X * 0.5f)), y), this.m_style, this.m_yesButtonText, new Action<MyGuiControlButton>(this.OnYesClick), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM));
                    this.Controls.Add(base3 = this.MakeButton(new Vector2(0f, y), this.m_style, this.m_noButtonText, new Action<MyGuiControlButton>(this.OnNoClick), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM));
                    this.Controls.Add(base4 = this.MakeButton(new Vector2(x + (minSizeGui.X * 0.5f), y), this.m_style, this.m_cancelButtonText, new Action<MyGuiControlButton>(this.OnCancelClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM));
                    float num3 = 0.003f;
                    base2.PositionX += num3;
                    base3.PositionX += num3;
                    base4.PositionX += num3;
                    break;
                }
                default:
                    throw new InvalidBranchException();
            }
            switch (this.m_focusedResult)
            {
                case ResultEnum.YES:
                    base.FocusedControl = base2;
                    return;

                case ResultEnum.NO:
                    base.FocusedControl = base3;
                    return;

                case ResultEnum.CANCEL:
                    base.FocusedControl = base4;
                    return;
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if ((this.m_buttonType == MyMessageBoxButtonsType.YES_NO_TIMEOUT) || (this.m_buttonType == MyMessageBoxButtonsType.NONE_TIMEOUT))
            {
                int num = MyGuiManager.TotalTimeInMilliseconds - this.m_timeoutStartedTimeInMiliseconds;
                if (num >= this.m_timeoutInMiliseconds)
                {
                    this.OnNoClick(null);
                }
                int num2 = MathHelper.Clamp((this.m_timeoutInMiliseconds - num) / 0x3e8, 0, this.m_timeoutInMiliseconds / 0x3e8);
                this.m_messageBoxText.Text = this.m_formattedCache.Clear().AppendFormat(this.m_formatText, num2.ToString());
            }
            return true;
        }

        public bool CloseBeforeCallback { get; set; }

        public bool InstantClose { get; set; }

        public bool CanHideOthers
        {
            get => 
                base.CanHideOthers;
            set => 
                (base.CanHideOthers = value);
        }

        public StringBuilder MessageText
        {
            get => 
                this.m_messageBoxText.Text;
            set => 
                (this.m_messageBoxText.Text = value);
        }

        public enum ResultEnum
        {
            YES,
            NO,
            CANCEL
        }

        public class Style
        {
            public MyGuiPaddedTexture BackgroundTexture;
            public string CaptionFont;
            public string TextFont;
            public MyGuiControlButtonStyleEnum ButtonStyle;
        }
    }
}


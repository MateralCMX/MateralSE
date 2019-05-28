namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenDialogAmount : MyGuiScreenBase
    {
        private MyGuiControlTextbox m_amountTextbox;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlButton m_increaseButton;
        private MyGuiControlButton m_decreaseButton;
        private MyGuiControlLabel m_errorLabel;
        private StringBuilder m_textBuffer;
        private MyStringId m_caption;
        private bool m_parseAsInteger;
        private float m_amountMin;
        private float m_amountMax;
        private float m_amount;
        [CompilerGenerated]
        private Action<float> OnConfirmed;

        public event Action<float> OnConfirmed
        {
            [CompilerGenerated] add
            {
                Action<float> onConfirmed = this.OnConfirmed;
                while (true)
                {
                    Action<float> a = onConfirmed;
                    Action<float> action3 = (Action<float>) Delegate.Combine(a, value);
                    onConfirmed = Interlocked.CompareExchange<Action<float>>(ref this.OnConfirmed, action3, a);
                    if (ReferenceEquals(onConfirmed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<float> onConfirmed = this.OnConfirmed;
                while (true)
                {
                    Action<float> source = onConfirmed;
                    Action<float> action3 = (Action<float>) Delegate.Remove(source, value);
                    onConfirmed = Interlocked.CompareExchange<Action<float>>(ref this.OnConfirmed, action3, source);
                    if (ReferenceEquals(onConfirmed, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenDialogAmount(float min, float max, MyStringId caption, int minMaxDecimalDigits = 3, bool parseAsInteger = false, float? defaultAmount = new float?(), float backgroundTransition = 0f, float guiTransition = 0f) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.2805344f), false, null, backgroundTransition, guiTransition)
        {
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            this.m_textBuffer = new StringBuilder();
            this.m_amountMin = min;
            this.m_amountMax = max;
            this.m_amount = (defaultAmount != null) ? defaultAmount.Value : max;
            this.m_parseAsInteger = parseAsInteger;
            this.m_caption = caption;
            this.RecreateControls(true);
        }

        private void amountTextbox_TextChanged(MyGuiControlTextbox obj)
        {
            this.m_amountTextbox.ColorMask = Vector4.One;
            this.m_errorLabel.Visible = false;
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (!this.TryParseAndStoreAmount(this.m_amountTextbox.Text))
            {
                this.m_errorLabel.Text = MyTexts.GetString(MyCommonTexts.DialogAmount_ParsingError);
                this.m_errorLabel.Visible = true;
                this.m_amountTextbox.ColorMask = Color.Red.ToVector4();
            }
            else if ((this.m_amount > this.m_amountMax) || (this.m_amount < this.m_amountMin))
            {
                this.m_errorLabel.Text = string.Format(MyTexts.GetString(MyCommonTexts.DialogAmount_RangeError), this.m_amountMin, this.m_amountMax);
                this.m_errorLabel.Visible = true;
                this.m_amountTextbox.ColorMask = Color.Red.ToVector4();
            }
            else
            {
                if (this.OnConfirmed != null)
                {
                    this.OnConfirmed(this.m_amount);
                }
                this.CloseScreen();
            }
        }

        private void decreaseButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (!this.TryParseAndStoreAmount(this.m_amountTextbox.Text))
            {
                this.m_errorLabel.Text = MyTexts.GetString(MyCommonTexts.DialogAmount_ParsingError);
                this.m_errorLabel.Visible = true;
                this.m_amountTextbox.ColorMask = Color.Red.ToVector4();
            }
            else
            {
                this.m_amount--;
                this.m_amount = MathHelper.Clamp(this.m_amount, this.m_amountMin, this.m_amountMax);
                this.RefreshAmountTextbox();
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDialogAmount";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
            {
                this.confirmButton_OnButtonClick(this.m_okButton);
            }
        }

        private void increaseButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (!this.TryParseAndStoreAmount(this.m_amountTextbox.Text))
            {
                this.m_errorLabel.Text = MyTexts.GetString(MyCommonTexts.DialogAmount_ParsingError);
                this.m_errorLabel.Visible = true;
                this.m_amountTextbox.ColorMask = Color.Red.ToVector4();
            }
            else
            {
                this.m_amount++;
                this.m_amount = MathHelper.Clamp(this.m_amount, this.m_amountMin, this.m_amountMax);
                this.RefreshAmountTextbox();
            }
        }

        public override void RecreateControls(bool contructor)
        {
            MyObjectBuilder_GuiScreen screen;
            base.RecreateControls(contructor);
            string str = MakeScreenFilepath("DialogAmount");
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_GuiScreen>(Path.Combine(MyFileSystem.ContentPath, str), out screen);
            base.Init(screen);
            Vector4? captionTextColor = null;
            base.AddCaption(this.m_caption, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            Vector2? position = null;
            position = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            position = null;
            position = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            Vector2 vector = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.071f);
            Vector2 vector2 = new Vector2(0.018f, 0f);
            this.m_okButton.Position = vector - vector2;
            this.m_cancelButton.Position = vector + vector2;
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_amountTextbox = (MyGuiControlTextbox) this.Controls.GetControlByName("AmountTextbox");
            this.m_increaseButton = (MyGuiControlButton) this.Controls.GetControlByName("IncreaseButton");
            this.m_decreaseButton = (MyGuiControlButton) this.Controls.GetControlByName("DecreaseButton");
            this.m_errorLabel = (MyGuiControlLabel) this.Controls.GetControlByName("ErrorLabel");
            this.m_errorLabel.TextScale = 0.68f;
            this.m_errorLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_errorLabel.Position = new Vector2(-0.19f, 0.008f);
            this.m_errorLabel.Visible = false;
            this.m_amountTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.amountTextbox_TextChanged);
            this.m_increaseButton.ButtonClicked += new Action<MyGuiControlButton>(this.increaseButton_OnButtonClick);
            this.m_decreaseButton.ButtonClicked += new Action<MyGuiControlButton>(this.decreaseButton_OnButtonClick);
            this.RefreshAmountTextbox();
            this.m_amountTextbox.SelectAll();
        }

        private void RefreshAmountTextbox()
        {
            this.m_textBuffer.Clear();
            if (this.m_parseAsInteger)
            {
                this.m_textBuffer.AppendInt32((int) this.m_amount);
            }
            else
            {
                this.m_textBuffer.AppendDecimalDigit(this.m_amount, 4);
            }
            this.m_amountTextbox.TextChanged -= new Action<MyGuiControlTextbox>(this.amountTextbox_TextChanged);
            this.m_amountTextbox.Text = this.m_textBuffer.ToString();
            this.m_amountTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.amountTextbox_TextChanged);
            this.m_amountTextbox.ColorMask = Vector4.One;
        }

        private bool TryParseAndStoreAmount(string text)
        {
            float num;
            if (!text.TryParseWithSuffix((NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign), CultureInfo.InvariantCulture, out num, null))
            {
                return false;
            }
            this.m_amount = this.m_parseAsInteger ? ((float) Math.Floor((double) num)) : num;
            return true;
        }
    }
}


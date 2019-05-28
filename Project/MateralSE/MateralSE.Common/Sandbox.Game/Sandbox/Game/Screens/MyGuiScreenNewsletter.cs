namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using System.Net.Mail;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenNewsletter : MyGuiScreenBase
    {
        private MyGuiControlButton m_okBtn;
        private MyGuiControlCheckbox m_hideCB;
        private MyGuiControlTextbox m_emailTBox;

        public MyGuiScreenNewsletter() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4964286f, 0.389313f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenNewsletter.ctor START");
            base.EnabledBackgroundFade = true;
            base.m_closeOnEsc = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHideOthers = true;
            base.CanBeHidden = true;
        }

        protected void BuildControls()
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionNewsletter, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyConfig.NewsletterStatus newsletterCurrentStatus = MySandboxGame.Config.NewsletterCurrentStatus;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            Vector2? size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(new Vector2(-0.195f, -0.104f) + new Vector2(0f, 0.021f)), size, MyTexts.GetString(MyCommonTexts.ScreenNewsletterSubtitle), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label.Autowrap(0.393f);
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?((new Vector2(-0.195f, -0.006f) - new Vector2(0f, 0.044f)) + new Vector2(0f, 0.021f)), size, MyTexts.GetString(MyCommonTexts.ScreenNewsletterEmailLabel), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2?((new Vector2(0.151f, 0.05f) - new Vector2(0f, 0.038f)) + new Vector2(0f, 0.021f)), size, MyTexts.GetString(MyCommonTexts.ScreenNewsletterNoInterestCheckbox), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 local1 = (base.m_size.Value / 2f) - new Vector2(0.25f, 0.03f);
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_okBtn = new MyGuiControlButton(new Vector2?((((base.m_size.Value / 2f) - (new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE)) * new Vector2(0f, 1f)) + (new Vector2(-25f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_okBtn.Enabled = false;
            this.m_okBtn.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            captionTextColor = null;
            buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?((((base.m_size.Value / 2f) - (new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE)) * new Vector2(0f, 1f)) + (new Vector2(30f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Close));
            Action<MyGuiControlCheckbox> action = checkbox => this.OnCheckedChanged();
            captionTextColor = null;
            this.m_hideCB = new MyGuiControlCheckbox(new Vector2?((new Vector2(0.184f, 0.05f) - new Vector2(0f, 0.038f)) + new Vector2(0f, 0.021f)), captionTextColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_hideCB.IsChecked = newsletterCurrentStatus == MyConfig.NewsletterStatus.NotInterested;
            this.m_hideCB.IsCheckedChanged = action;
            this.m_hideCB.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_DontAskAgain));
            size = null;
            captionTextColor = null;
            this.m_emailTBox = new MyGuiControlTextbox(size, null, 50, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_emailTBox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Email));
            this.m_emailTBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_emailTBox.Position = (label2.Position + new Vector2(label2.Size.X, 0f)) + new Vector2(0.015f, 0f);
            this.m_emailTBox.Size = new Vector2(0.374f - label2.Size.X, 0.02f);
            if ((newsletterCurrentStatus == MyConfig.NewsletterStatus.EmailConfirmed) || (newsletterCurrentStatus == MyConfig.NewsletterStatus.EmailNotConfirmed))
            {
                this.m_emailTBox.Text = "****************";
            }
            else if (newsletterCurrentStatus == MyConfig.NewsletterStatus.NotInterested)
            {
                this.m_emailTBox.Enabled = false;
            }
            this.m_emailTBox.TextChanged += new Action<MyGuiControlTextbox>(this.emailTBox_TextChanged);
            if (newsletterCurrentStatus == MyConfig.NewsletterStatus.EmailNotConfirmed)
            {
                size = null;
                captionTextColor = null;
                MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2?(this.m_emailTBox.Position + new Vector2(0.003f, 0.032f)), size, "* " + MyTexts.GetString(MyCommonTexts.ScreenNewsletterConfirmationMessage), captionTextColor, 0.48f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(label4);
            }
            this.Controls.Add(label);
            this.Controls.Add(label2);
            this.Controls.Add(this.m_emailTBox);
            this.Controls.Add(this.m_hideCB);
            this.Controls.Add(label3);
            this.Controls.Add(this.m_okBtn);
            this.Controls.Add(button);
            base.CloseButtonEnabled = true;
        }

        private void emailTBox_TextChanged(MyGuiControlTextbox obj)
        {
            this.m_okBtn.Enabled = this.IsValidEmail(obj.Text);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenNewsletter";

        private bool IsValidEmail(string email)
        {
            if (!email.Contains<char>('@') || email.Contains<char>('*'))
            {
                return false;
            }
            try
            {
                MailAddress address = new MailAddress(email);
                return (email == address.Address);
            }
            catch
            {
                return false;
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            this.RecreateControls(true);
        }

        private void OnCancelButtonClick(object sender)
        {
            this.CloseScreen();
        }

        private void OnCheckedChanged()
        {
            if (this.m_hideCB.IsChecked)
            {
                this.m_emailTBox.Enabled = false;
                this.m_okBtn.Enabled = true;
            }
            else
            {
                this.m_emailTBox.Enabled = true;
                this.m_okBtn.Enabled = this.IsValidEmail(this.m_emailTBox.Text);
            }
        }

        private void OnOkButtonClick(object sender)
        {
            if (this.m_hideCB.IsChecked)
            {
                MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.NotInterested;
                MyEShop.SendInfo(string.Empty);
            }
            else
            {
                MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.EmailNotConfirmed;
                MyEShop.SendInfo(this.m_emailTBox.Text);
            }
            MySandboxGame.Config.Save();
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
        }
    }
}


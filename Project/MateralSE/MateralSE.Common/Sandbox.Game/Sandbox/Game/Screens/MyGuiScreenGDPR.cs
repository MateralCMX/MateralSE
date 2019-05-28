namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenGDPR : MyGuiScreenBase
    {
        private MyGuiControlButton m_yesBtn;
        private MyGuiControlButton m_noBtn;
        private MyGuiControlButton m_linkBtn;

        public MyGuiScreenGDPR() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.5264286f, 0.4293893f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenGDPR.ctor START");
            base.EnabledBackgroundFade = true;
            base.m_closeOnEsc = true;
            base.CloseButtonEnabled = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHideOthers = true;
            base.CanBeHidden = true;
        }

        protected void BuildControls()
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyTexts.GetString(MySpaceTexts.AnonymousActivityTracking_Caption), captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(list2);
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(0.015f, 0.005f + 0.095f), new Vector2(0.44f, 0.45f), captionTextColor, "Blue", 0.76f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            text.AppendText(MyTexts.GetString(MySpaceTexts.AnonymousActivityTracking_Text1), "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText("\n\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText(MyTexts.GetString(MySpaceTexts.AnonymousActivityTracking_Text2), "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_linkBtn = new MyGuiControlButton(new Vector2(0f, 0.068f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Yes), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnLinkButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_linkBtn.Enabled = true;
            this.m_linkBtn.VisualStyle = MyGuiControlButtonStyleEnum.ClickableText;
            this.m_linkBtn.Text = MyTexts.GetString(MySpaceTexts.AnonymousActivityTracking_PrivacyPolicy);
            this.Controls.Add(text);
            this.Controls.Add(this.m_linkBtn);
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_yesBtn = new MyGuiControlButton(new Vector2(-0.1f, 0.17f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Yes), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnYesButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_yesBtn.Enabled = true;
            this.m_yesBtn.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.Controls.Add(this.m_yesBtn);
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_noBtn = new MyGuiControlButton(new Vector2(0.1f, 0.17f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.No), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnNoButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_noBtn.Enabled = true;
            this.m_noBtn.SetToolTip(MyTexts.GetString(MySpaceTexts.DetailScreen_Button_Close));
            this.Controls.Add(this.m_noBtn);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenGDPR";

        public override void LoadContent()
        {
            base.LoadContent();
            this.RecreateControls(true);
        }

        private void OnLinkButtonClick(object sender)
        {
            MyGuiSandbox.OpenUrl("http://mirror.keenswh.com/policy/KSWH_Privacy_Policy.pdf", UrlOpenMode.ExternalBrowser, null);
        }

        private void OnNoButtonClick(object sender)
        {
            MySandboxGame.Config.GDPRConsent = false;
            MySandboxGame.Config.Save();
            ConsentSenderGDPR.TrySendConsent();
            this.CloseScreen();
        }

        private void OnYesButtonClick(object sender)
        {
            MySandboxGame.Config.GDPRConsent = true;
            MySandboxGame.Config.Save();
            ConsentSenderGDPR.TrySendConsent();
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
        }
    }
}


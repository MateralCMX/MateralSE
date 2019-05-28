namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenTutorialsScreen : MyGuiScreenBase
    {
        private MyGuiControlButton m_okBtn;
        private MyGuiControlCheckbox m_dontShowAgainCheckbox;
        private Action m_okAction;

        public MyGuiScreenTutorialsScreen(Action okAction) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.5264286f, 0.6679389f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenWelcomeScreen.ctor START");
            this.m_okAction = okAction;
            base.EnabledBackgroundFade = true;
            base.m_closeOnEsc = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHideOthers = true;
            base.CanBeHidden = true;
        }

        protected void BuildControls()
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption("Tutorials", captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyConfig.NewsletterStatus newsletterCurrentStatus = MySandboxGame.Config.NewsletterCurrentStatus;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(list2);
            float num = 0.145f;
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(0.015f, -0.162f + num), new Vector2(0.44f, 0.45f), captionTextColor, "Blue", 0.76f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            text.AppendText("Hello Engineer!\r\n\r\n            We recommend that you view these tutorial links, which contain useful information on how to get started in Space Engineers!", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText("\n\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART1, "Introduction");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART2, "Basic Controls");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART3, "Possibilities Within The Game Modes");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART4, "Drilling, Refining, & Assembling (Survival) ");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART5, "Building Your 1st Ship (Creative)");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART10, "Survival");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART6, "Experimental Mode");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART7, "Building Your 1st Ground Vehicle (Creative)");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART8, "Steam Workshop & Blueprints");
            text.AppendText("\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendLink(MySteamConstants.URL_TUTORIAL_PART9, "Other Advice & Closing Thoughts");
            text.AppendText("\n\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.OnLinkClicked += new LinkClicked(this.OnLinkClicked);
            text.AppendText("You can always access these tutorials from the Help screen (F1 key).", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            captionTextColor = null;
            this.m_dontShowAgainCheckbox = new MyGuiControlCheckbox(new Vector2(0.08f, 0.017f + num), captionTextColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_dontShowAgainCheckbox);
            Vector2? size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(0.195f, 0.047f + num), size, "Don't show again", captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM
            };
            this.Controls.Add(label);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_okBtn = new MyGuiControlButton(new Vector2(0f, 0.155f + num), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOKButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_okBtn.Enabled = true;
            this.m_okBtn.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.Controls.Add(text);
            this.Controls.Add(this.m_okBtn);
            base.CloseButtonEnabled = true;
        }

        protected override void Canceling()
        {
            this.m_okAction();
            base.Canceling();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTutorialsScreen";

        public override void LoadContent()
        {
            base.LoadContent();
            this.RecreateControls(true);
        }

        private void OnLinkClicked(MyGuiControlBase sender, string url)
        {
            MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null);
        }

        private void OnOKButtonClick(object sender)
        {
            MySandboxGame.Config.FirstTimeTutorials = !this.m_dontShowAgainCheckbox.IsChecked;
            MySandboxGame.Config.Save();
            this.CloseScreen();
            this.m_okAction();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
        }
    }
}


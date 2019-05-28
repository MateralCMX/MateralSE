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

    public class MyGuiScreenWelcomeScreen : MyGuiScreenBase
    {
        private MyGuiControlButton m_okBtn;

        public MyGuiScreenWelcomeScreen() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.5264286f, 0.7633588f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenWelcomeScreen.ctor START");
            base.EnabledBackgroundFade = true;
            base.m_closeOnEsc = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHideOthers = true;
            base.CanBeHidden = true;
        }

        protected void BuildControls()
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionWelcomeScreen, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyConfig.NewsletterStatus newsletterCurrentStatus = MySandboxGame.Config.NewsletterCurrentStatus;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.79f, 0f, captionTextColor);
            this.Controls.Add(list2);
            float num = 0.095f;
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(0.015f, -0.162f + num), new Vector2(0.44f, 0.45f), captionTextColor, "Blue", 0.76f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            text.AppendText(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text1), "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText("\n\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text2), "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText("\n\n", "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            text.AppendText(MyTexts.GetString(MySpaceTexts.WelcomeScreen_Text3), "Blue", 0.76f, (VRageMath.Vector4) Color.White);
            captionTextColor = null;
            MyGuiControlPanel panel = new MyGuiControlPanel(new Vector2(-0.08f, 0.07f + num), new Vector2?(MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui), captionTextColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO
            };
            this.Controls.Add(panel);
            Vector2? size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(0.195f, 0.1f + num), size, MyTexts.GetString(MySpaceTexts.WelcomeScreen_SignatureTitle), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM
            };
            this.Controls.Add(label);
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(0.195f, 0.125f + num), size, MyTexts.GetString(MySpaceTexts.WelcomeScreen_Signature), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM
            };
            this.Controls.Add(label2);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_okBtn = new MyGuiControlButton(new Vector2(0f, 0.338f), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCloseButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_okBtn.Enabled = true;
            this.m_okBtn.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.Controls.Add(text);
            this.Controls.Add(this.m_okBtn);
            base.CloseButtonEnabled = true;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenWelcomeScreen";

        public override void LoadContent()
        {
            base.LoadContent();
            this.RecreateControls(true);
        }

        private void OnCloseButtonClick(object sender)
        {
            MySandboxGame.Config.WelcomScreenCurrentStatus = MyConfig.WelcomeScreenStatus.AlreadySeen;
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


namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenMotD : MyGuiScreenBase
    {
        private StringBuilder m_message;
        private MyGuiControlLabel m_caption;
        private MyGuiControlMultilineText m_messageMultiline;
        private MyGuiControlButton m_continueButton;

        public MyGuiScreenMotD(StringBuilder message) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.8f, 0.8f), true, null, 0f, 0f)
        {
            this.MessageOfTheDay = message;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMotD";

        private void onContinueClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            this.m_caption = base.AddCaption(MyTexts.GetString(MyCommonTexts.MotD_Caption), captionTextColor, captionOffset, 0.8f);
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
            text1.Position = new Vector2(0f, -0.3f);
            text1.Size = new Vector2(0.7f, 0.6f);
            text1.Font = "Blue";
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_messageMultiline = text1;
            this.m_messageMultiline.Text = MyTexts.SubstituteTexts(this.MessageOfTheDay);
            this.Controls.Add(this.m_messageMultiline);
            captionOffset = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_continueButton = new MyGuiControlButton(new Vector2(0f, 0.35f), MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.MotD_Button), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.onContinueClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_continueButton);
        }

        public StringBuilder MessageOfTheDay
        {
            get => 
                this.m_message;
            private set => 
                (this.m_message = value);
        }
    }
}


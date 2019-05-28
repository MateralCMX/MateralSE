namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenCongratulation : MyGuiScreenBase
    {
        private int m_messageId;

        public MyGuiScreenCongratulation(int messageId) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.7f, 0.54f), false, null, 0f, 0f)
        {
            this.m_messageId = messageId;
            this.RecreateControls(true);
            if (MyAudio.Static != null)
            {
                MyCueId cueId = MySoundPair.GetCueId("ArcNewItemImpact");
                MyAudio.Static.PlaySound(cueId, null, MySoundDimensions.D2, false, false);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenCongratulation";

        private void OkButtonClicked(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2? size = base.Size;
            Vector2 vector = (size != null) ? size.GetValueOrDefault() : new Vector2(1.2f, 0.5f);
            size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(0f, -0.22f), size, MyTexts.GetString(MyCommonTexts.Campaign_Congratulation_Caption), colorMask, 1.5f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(control);
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2?(Vector2.Zero), new Vector2?(vector), new VRageMath.Vector4?(Color.White.ToVector4()), "White", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            this.Controls.Add(text);
            colorMask = null;
            visibleLinesCount = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(0f, 0.22f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.Controls.Add(button);
            int messageId = this.m_messageId;
            string str3 = @"Textures\GUI\PromotedEngineer.dds";
            size = null;
            colorMask = null;
            string[] textures = new string[] { str3 };
            MyGuiControlImage image = new MyGuiControlImage(size, new Vector2(0.12f, 0.16f), colorMask, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Position = new Vector2(0f, -0.03f)
            };
            this.Controls.Add(image);
            text.Text = new StringBuilder(MyTexts.GetString(MyCommonTexts.Campaign_Congratulation_Text));
        }
    }
}


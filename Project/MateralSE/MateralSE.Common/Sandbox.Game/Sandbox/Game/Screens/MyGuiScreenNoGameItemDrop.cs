namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenNoGameItemDrop : MyGuiScreenBase
    {
        public MyGuiScreenNoGameItemDrop() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.41f, 0.4f), true, null, 0f, 0f)
        {
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenNoGameItemDrop";

        private void OnClaimButtonClick(MyGuiControlButton obj)
        {
            this.CloseScreen();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionClaimGameItem, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.74f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.74f, 0f, captionTextColor);
            this.Controls.Add(control);
            captionTextColor = null;
            string[] textures = new string[] { @"Textures\GUI\ClaimItem.png" };
            MyGuiControlImage image = new MyGuiControlImage(new Vector2(-0.15f, -0.107f), new Vector2(0.3f, 0.17f), captionTextColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                BorderEnabled = true,
                BorderSize = 2,
                BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f)
            };
            this.Controls.Add(image);
            Vector2? size = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(0f, 0.085f), size, MyTexts.GetString(MyCommonTexts.ScreenNoGameItemText), new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Font = "White"
            };
            base.Elements.Add(label);
            size = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(0f, 0.168f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Close), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnClaimButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
        }
    }
}


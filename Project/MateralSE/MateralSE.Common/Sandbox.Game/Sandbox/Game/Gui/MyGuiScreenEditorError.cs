namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenEditorError : MyGuiScreenBase
    {
        protected string m_errorText;

        public MyGuiScreenEditorError(string errorText = null) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6f, 0.7f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_errorText = "";
            this.m_errorText = errorText;
            base.CanBeHidden = false;
            base.CanHideOthers = true;
            base.m_closeOnEsc = true;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        public override bool CloseScreen() => 
            base.CloseScreen();

        public override string GetFriendlyName() => 
            "MyGuiScreenEditorError";

        private void OkButtonClicked(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MySpaceTexts.ProgrammableBlock_CodeEditor_Title, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlCompositePanel panel = new MyGuiControlCompositePanel {
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                Position = new Vector2(0f, -0.023f),
                Size = new Vector2(0.5f, 0.465f)
            };
            this.Controls.Add(panel);
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(0.005f, -0.025f), new Vector2(0.485f, 0.44f), captionTextColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                Text = new StringBuilder(this.m_errorText),
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            this.Controls.Add(text);
            captionTextColor = null;
            visibleLinesCount = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(0f, 0.277f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.Controls.Add(button);
        }
    }
}


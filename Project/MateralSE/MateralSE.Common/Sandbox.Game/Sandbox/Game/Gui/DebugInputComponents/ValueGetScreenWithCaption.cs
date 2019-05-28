namespace Sandbox.Game.GUI.DebugInputComponents
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class ValueGetScreenWithCaption : MyGuiScreenBase
    {
        private MyGuiControlTextbox m_nameTextbox;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private string m_title;
        private string m_caption;
        private ValueGetScreenAction m_acceptCallback;

        public ValueGetScreenWithCaption(string title, string caption, ValueGetScreenAction ValueAcceptedCallback) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_acceptCallback = ValueAcceptedCallback;
            this.m_title = title;
            this.m_caption = caption;
            base.m_canShareInput = false;
            base.m_isTopMostScreen = true;
            base.m_isTopScreen = true;
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (this.m_acceptCallback(this.m_nameTextbox.Text))
            {
                this.CloseScreen();
            }
        }

        public override string GetFriendlyName() => 
            "ValueGetScreenWithCaption";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsKeyPress(MyKeys.Enter))
            {
                this.confirmButton_OnButtonClick(this.m_confirmButton);
            }
            if (MyInput.Static.IsKeyPress(MyKeys.Escape))
            {
                this.cancelButton_OnButtonClick(this.m_cancelButton);
            }
        }

        public override void RecreateControls(bool contructor)
        {
            base.RecreateControls(contructor);
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.1f), size, this.m_title, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            colorMask = null;
            this.m_nameTextbox = new MyGuiControlTextbox(new Vector2(0f, 0f), this.m_caption, 0x200, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            int? buttonIndex = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Confirm), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_nameTextbox);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }

        public delegate bool ValueGetScreenAction(string valueText);
    }
}


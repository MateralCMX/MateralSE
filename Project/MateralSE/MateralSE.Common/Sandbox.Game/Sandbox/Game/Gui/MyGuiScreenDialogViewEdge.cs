namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Utils;

    internal class MyGuiScreenDialogViewEdge : MyGuiScreenBase
    {
        private MyGuiControlTextbox m_textbox;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;

        public MyGuiScreenDialogViewEdge() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
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
            MyWingedEdgeMesh.DebugDrawEdgesAdd(Convert.ToInt32(this.m_textbox.Text));
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDialogViewEdge";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        public override void RecreateControls(bool contructor)
        {
            base.RecreateControls(contructor);
            Vector2? size = null;
            Vector4? colorMask = null;
            this.Controls.Add(new MyGuiControlLabel(new Vector2(0f, -0.1f), size, "Enter the number of winged-edge mesh edge to view", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
            colorMask = null;
            this.m_textbox = new MyGuiControlTextbox(new Vector2(0.2f, 0f), null, 0x200, colorMask, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            int? buttonIndex = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_textbox);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }
    }
}


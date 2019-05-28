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

    public class Vector3GetScreenWithCaption : MyGuiScreenBase
    {
        private MyGuiControlTextbox m_nameTextbox1;
        private MyGuiControlTextbox m_nameTextbox2;
        private MyGuiControlTextbox m_nameTextbox3;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private string m_title;
        private string m_caption1;
        private string m_caption2;
        private string m_caption3;
        private Vector3GetScreenAction m_acceptCallback;

        public Vector3GetScreenWithCaption(string title, string caption1, string caption2, string caption3, Vector3GetScreenAction ValueAcceptedCallback) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_acceptCallback = ValueAcceptedCallback;
            this.m_title = title;
            this.m_caption1 = caption1;
            this.m_caption2 = caption2;
            this.m_caption3 = caption3;
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
            if (this.m_acceptCallback(this.m_nameTextbox1.Text, this.m_nameTextbox2.Text, this.m_nameTextbox3.Text))
            {
                this.CloseScreen();
            }
        }

        public override string GetFriendlyName() => 
            "Vector3GetScreenWithCaption";

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
            float num = 0f;
            float num2 = 0.04f;
            colorMask = null;
            this.m_nameTextbox1 = new MyGuiControlTextbox(new Vector2(0f - num, 0f - num2), this.m_caption1, 0x200, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            this.m_nameTextbox2 = new MyGuiControlTextbox(new Vector2(0f, 0f), this.m_caption2, 0x200, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            this.m_nameTextbox3 = new MyGuiControlTextbox(new Vector2(0f + num, 0f + num2), this.m_caption3, 0x200, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            colorMask = null;
            int? buttonIndex = null;
            this.m_confirmButton = new MyGuiControlButton(new Vector2(0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Confirm), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2(-0.21f, 0.1f), MyGuiControlButtonStyleEnum.Default, new Vector2(0.2f, 0.05f), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_nameTextbox1);
            this.Controls.Add(this.m_nameTextbox2);
            this.Controls.Add(this.m_nameTextbox3);
            this.Controls.Add(this.m_confirmButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
        }

        public delegate bool Vector3GetScreenAction(string value1, string value2, string value3);
    }
}


namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiBlueprintTextDialog : MyGuiBlueprintScreenBase
    {
        private MyGuiControlTextbox m_nameBox;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private string m_defaultName;
        private string m_caption;
        private int m_maxTextLength;
        private float m_textBoxWidth;
        private Action<string> callBack;
        private Vector2 WINDOW_SIZE;

        public MyGuiBlueprintTextDialog(Vector2 position, Action<string> callBack, string defaultName, string caption = "", int maxLenght = 20, float textBoxWidth = 0.2f) : base(position, new Vector2(0.4971429f, 0.2805344f), MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity, true)
        {
            this.WINDOW_SIZE = new Vector2(0.3f, 0.5f);
            this.m_maxTextLength = maxLenght;
            this.m_caption = caption;
            this.m_textBoxWidth = textBoxWidth;
            this.callBack = callBack;
            this.m_defaultName = defaultName;
            this.RecreateControls(true);
            base.OnEnterCallback = new Action(this.ReturnOk);
            base.CanBeHidden = true;
            base.CanHideOthers = true;
            base.CloseButtonEnabled = true;
        }

        private void CallResultCallback(string val)
        {
            if (val != null)
            {
                this.callBack(val);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiRenameDialog";

        private void OnCancel(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        private void OnOk(MyGuiControlButton button)
        {
            this.ReturnOk();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.AddCaption(this.m_caption, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, color);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            color = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.78f, 0f, color);
            this.Controls.Add(list2);
            color = null;
            this.m_nameBox = new MyGuiControlTextbox(new Vector2(0f, -0.027f), null, this.m_maxTextLength, color, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_nameBox.Text = this.m_defaultName;
            this.m_nameBox.Size = new Vector2(0.385f, 1f);
            this.Controls.Add(this.m_nameBox);
            Vector2? position = null;
            position = null;
            color = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOk), GuiSounds.MouseClick, 1f, buttonIndex, false);
            position = null;
            position = null;
            color = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancel), GuiSounds.MouseClick, 1f, buttonIndex, false);
            Vector2 vector = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.071f);
            Vector2 vector2 = new Vector2(0.018f, 0f);
            this.m_okButton.Position = vector - vector2;
            this.m_cancelButton.Position = vector + vector2;
            this.m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.m_cancelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
        }

        private void ReturnOk()
        {
            if (this.m_nameBox.Text.Length > 0)
            {
                this.CallResultCallback(this.m_nameBox.Text);
                this.CloseScreen();
            }
        }
    }
}


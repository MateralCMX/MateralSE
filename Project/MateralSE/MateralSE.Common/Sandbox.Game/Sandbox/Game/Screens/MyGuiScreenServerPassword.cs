namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenServerPassword : MyGuiScreenBase
    {
        private readonly float _padding;
        private MyGuiControlTextbox m_passwordTextbox;
        private Action<string> m_connectAction;

        public MyGuiScreenServerPassword(Action<string> connectAction) : base(new Vector2(0.5f, 0.75f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.1908397f), true, null, 0f, 0f)
        {
            this._padding = 0.02f;
            this.m_connectAction = connectAction;
            this.CreateScreen();
        }

        private void AddressEnterPressed(MyGuiControlTextbox obj)
        {
            this.ConnectButtonClick(null);
        }

        private void AddressFocusChanged(MyGuiControlBase obj, bool focused)
        {
            if (focused)
            {
                this.m_passwordTextbox.SelectAll();
                this.m_passwordTextbox.MoveCarriageToEnd();
            }
        }

        private void ConnectButtonClick(MyGuiControlButton obj)
        {
            this.CloseScreen();
            if (this.m_connectAction != null)
            {
                this.m_connectAction(this.m_passwordTextbox.Text);
            }
        }

        private void CreateScreen()
        {
            base.CanHideOthers = false;
            base.CanBeHidden = false;
            base.EnabledBackgroundFade = false;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "ServerPassword";

        public override void RecreateControls(bool constructor)
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.MultiplayerEnterPassword, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.05f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            captionTextColor = null;
            this.m_passwordTextbox = new MyGuiControlTextbox(new Vector2?(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.105f)), string.Empty, 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_passwordTextbox.Size = new Vector2(this.m_passwordTextbox.Size.X / 1.33f, this.m_passwordTextbox.Size.Y);
            this.m_passwordTextbox.PositionX += this.m_passwordTextbox.Size.X / 2f;
            this.m_passwordTextbox.EnterPressed += new Action<MyGuiControlTextbox>(this.AddressEnterPressed);
            this.m_passwordTextbox.FocusChanged += new Action<MyGuiControlBase, bool>(this.AddressFocusChanged);
            this.m_passwordTextbox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameDirectConnect_IP));
            this.m_passwordTextbox.Type = MyGuiControlTextboxType.Password;
            this.m_passwordTextbox.MoveCarriageToEnd();
            this.Controls.Add(this.m_passwordTextbox);
            Vector2? size = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(this.m_passwordTextbox.PositionX + (this.m_passwordTextbox.Size.X / 2f), this.m_passwordTextbox.PositionY + 0.007f), MyGuiControlButtonStyleEnum.ComboBoxButton, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.MultiplayerJoinConnect), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.PositionX += (button.Size.X / 2f) + (this._padding * 0.66f);
            button.ButtonClicked += new Action<MyGuiControlButton>(this.ConnectButtonClick);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGame_JoinWorld));
            this.Controls.Add(button);
        }
    }
}


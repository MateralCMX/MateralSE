namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Net;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenServerConnect : MyGuiScreenBase
    {
        private readonly float _padding;
        private MyGuiControlTextbox m_addrTextbox;
        private MyGuiControlCheckbox m_favoriteCheckbox;
        private MyGuiScreenProgress m_progressScreen;

        public MyGuiScreenServerConnect() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.269084f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this._padding = 0.02f;
            this.CreateScreen();
        }

        private void AddressEnterPressed(MyGuiControlTextbox obj)
        {
            this.ParseIPAndConnect();
        }

        private void AddressFocusChanged(MyGuiControlBase obj, bool focused)
        {
            if (focused)
            {
                this.m_addrTextbox.SelectAll();
                this.m_addrTextbox.MoveCarriageToEnd();
            }
        }

        private void CloseHandlers()
        {
            MyGameService.OnPingServerResponded -= new EventHandler<MyGameServerItem>(this.ServerResponded);
            MyGameService.OnPingServerFailedToRespond -= new EventHandler(this.ServerFailedToRespond);
        }

        private void ConnectButtonClick(MyGuiControlButton obj)
        {
            this.ParseIPAndConnect();
        }

        private void CreateScreen()
        {
            base.CanHideOthers = true;
            base.CanBeHidden = true;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "ServerConnect";

        private void ParseIPAndConnect()
        {
            try
            {
                char[] separator = new char[] { ':' };
                string[] strArray = this.m_addrTextbox.Text.Trim().Split(separator);
                ushort connPort = (strArray.Length >= 2) ? ushort.Parse(strArray[1]) : 0x6988;
                IPAddress[] hostAddresses = Dns.GetHostAddresses(strArray[0]);
                if (this.m_favoriteCheckbox.IsChecked)
                {
                    MyGameService.AddFavoriteGame(hostAddresses[0].ToIPv4NetworkOrder(), connPort, connPort);
                }
                StringBuilder text = MyTexts.Get(MyCommonTexts.DialogTextJoiningWorld);
                this.m_progressScreen = new MyGuiScreenProgress(text, new MyStringId?(MyCommonTexts.Cancel), false, true);
                MyGuiSandbox.AddScreen(this.m_progressScreen);
                this.m_progressScreen.ProgressCancelled += delegate {
                    this.CloseHandlers();
                    MySessionLoader.UnloadAndExitToMenu();
                };
                MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(this.ServerResponded);
                MyGameService.OnPingServerFailedToRespond += new EventHandler(this.ServerFailedToRespond);
                MyGameService.PingServer(hostAddresses[0].ToIPv4NetworkOrder(), connPort);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
                MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
        }

        public override void RecreateControls(bool constructor)
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.MultiplayerJoinDirectConnect, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.05f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            Vector2? size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(-new Vector2(base.m_size.Value.X * 0.385f, (base.m_size.Value.Y / 2f) - 0.116f)), size, MyTexts.GetString(MyCommonTexts.JoinGame_Favorites_Add), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(label);
            captionTextColor = null;
            this.m_favoriteCheckbox = new MyGuiControlCheckbox(new Vector2?(label.Position), captionTextColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_favoriteCheckbox.PositionX += label.Size.X + 0.01f;
            this.m_favoriteCheckbox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameDirectConnect_Favorite));
            this.Controls.Add(this.m_favoriteCheckbox);
            captionTextColor = null;
            this.m_addrTextbox = new MyGuiControlTextbox(new Vector2?(-new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.17f)), "0.0.0.0:27016", 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_addrTextbox.Size = new Vector2(this.m_addrTextbox.Size.X / 1.33f, this.m_addrTextbox.Size.Y);
            this.m_addrTextbox.PositionX += this.m_addrTextbox.Size.X / 2f;
            this.m_addrTextbox.EnterPressed += new Action<MyGuiControlTextbox>(this.AddressEnterPressed);
            this.m_addrTextbox.FocusChanged += new Action<MyGuiControlBase, bool>(this.AddressFocusChanged);
            this.m_addrTextbox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameDirectConnect_IP));
            this.m_addrTextbox.MoveCarriageToEnd();
            this.Controls.Add(this.m_addrTextbox);
            size = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(this.m_addrTextbox.PositionX + (this.m_addrTextbox.Size.X / 2f), this.m_addrTextbox.PositionY + 0.007f), MyGuiControlButtonStyleEnum.ComboBoxButton, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.MultiplayerJoinConnect), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.PositionX += (button.Size.X / 2f) + (this._padding * 0.66f);
            button.ButtonClicked += new Action<MyGuiControlButton>(this.ConnectButtonClick);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGame_JoinWorld));
            this.Controls.Add(button);
        }

        private void ServerFailedToRespond(object sender, object e)
        {
            this.CloseHandlers();
            this.m_progressScreen.CloseScreen();
            MyStringId caption = new MyStringId();
            MyGuiSandbox.Show(MyCommonTexts.MultiplaterJoin_ServerIsNotResponding, caption, MyMessageBoxStyleEnum.Error);
        }

        private void ServerResponded(object sender, MyGameServerItem serverItem)
        {
            this.CloseHandlers();
            this.m_progressScreen.CloseScreen();
            MyJoinGameHelper.JoinGame(serverItem, true);
        }
    }
}


namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Net;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenServerReconnector : MyGuiScreenBase
    {
        private string m_address;
        private ushort m_port;
        private static int WAIT = 300;
        private static int WAIT_RETRY_DELAY = 0x4b;
        private int m_counter;
        private MyReconnectionState m_state;
        private MyReconnectionState m_stateLast;
        private int m_timeToReconnect;
        private int m_timeToReconnectLastFrame;
        private MyGuiControlMultilineText m_messageBoxText;
        private MyGuiControlLabel m_reconnectingCaption;

        public MyGuiScreenServerReconnector(string address, ushort port) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_address = string.Empty;
            this.m_counter = 600;
            base.CanHideOthers = true;
            base.m_drawEvenWithoutFocus = true;
            this.m_address = address;
            this.m_port = port;
            this.m_state = MyReconnectionState.RETRY;
            this.RecreateControls(true);
        }

        private void CloseHandlers_Ping()
        {
            MyGameService.OnPingServerResponded -= new EventHandler<MyGameServerItem>(this.ServerResponded_Ping);
            MyGameService.OnPingServerFailedToRespond -= new EventHandler(this.ServerFailedToRespond_Ping);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenServerReconnector";

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum align)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder builder = MyTexts.Get(text);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, align, null, builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        public void OnCancelClick(MyGuiControlButton sender)
        {
            this.m_state = MyReconnectionState.IDLE;
            this.CloseScreen();
        }

        public static MyGuiScreenServerReconnector ReconnectToLastSession()
        {
            MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
            if (lastSession == null)
            {
                return null;
            }
            if (!lastSession.IsOnline)
            {
                return null;
            }
            if (lastSession.IsLobby)
            {
                MyJoinGameHelper.JoinGame(ulong.Parse(lastSession.ServerIP));
                return null;
            }
            MyGuiScreenServerReconnector screen = new MyGuiScreenServerReconnector(lastSession.ServerIP, (ushort) lastSession.ServerPort);
            MyGuiSandbox.AddScreen(screen);
            return screen;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_size = new Vector2(0.35f, 0.3f);
            Vector2 vector1 = new Vector2(0.1f, 0.1f);
            Vector2 vector2 = new Vector2(0.05f, 0.05f);
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(0f, -0.13f), size, MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_Caption), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(control);
            size = null;
            colorMask = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(0f, -0.07f), size, MyTexts.GetString(MyCommonTexts.MultiplayerErrorServerHasLeft), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(label2);
            size = null;
            colorMask = null;
            this.m_reconnectingCaption = new MyGuiControlLabel(new Vector2(0f, -0.01f), size, string.Format(MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_Reconnection), 0), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_reconnectingCaption);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            colorMask = null;
            list.AddHorizontal(new Vector2(-0.15f, -0.09f), 0.3f, 0f, colorMask);
            this.Controls.Add(list);
            MyGuiControlButton button = null;
            this.Controls.Add(button = this.MakeButton(new Vector2(0f, 0.12f), MyCommonTexts.Cancel, new Action<MyGuiControlButton>(this.OnCancelClick), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM));
        }

        public void ServerFailedToRespond_Ping(object sender, object e)
        {
            MyLog.Default.WriteLineAndConsole("Server failed to respond");
            this.CloseHandlers_Ping();
            this.m_reconnectingCaption.Text = MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_ServerNoResponse);
            this.m_state = MyReconnectionState.RETRY_DELAY;
            this.m_counter = WAIT_RETRY_DELAY;
        }

        public void ServerResponded_Ping(object sender, MyGameServerItem serverItem)
        {
            if (this.m_state == MyReconnectionState.WAITING)
            {
                this.m_state = MyReconnectionState.CONNECTING;
                MyLog.Default.WriteLineAndConsole("Server responded");
                this.CloseHandlers_Ping();
                MyJoinGameHelper.JoinGame(serverItem, false);
            }
        }

        public override bool Update(bool hasFocus)
        {
            base.Update(hasFocus);
            switch (this.m_state)
            {
                case MyReconnectionState.RETRY:
                    if (this.m_stateLast != this.m_state)
                    {
                        this.m_reconnectingCaption.Text = string.Format(MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_Reconnection), this.m_timeToReconnect);
                    }
                    this.m_counter--;
                    this.m_timeToReconnect = this.m_counter / 60;
                    if (this.m_timeToReconnectLastFrame != this.m_timeToReconnect)
                    {
                        this.m_reconnectingCaption.Text = string.Format(MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_Reconnection), this.m_timeToReconnect);
                    }
                    this.m_timeToReconnectLastFrame = this.m_timeToReconnect;
                    if (this.m_counter < 0)
                    {
                        this.m_counter = WAIT;
                        try
                        {
                            MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(this.ServerResponded_Ping);
                            MyGameService.OnPingServerFailedToRespond += new EventHandler(this.ServerFailedToRespond_Ping);
                            MyGameService.PingServer(Dns.GetHostAddresses(this.m_address)[0].ToIPv4NetworkOrder(), this.m_port);
                            this.m_state = MyReconnectionState.WAITING;
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine(exception);
                            MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
                        }
                    }
                    break;

                case MyReconnectionState.WAITING:
                    this.m_reconnectingCaption.Text = MyTexts.GetString(MyCommonTexts.MultiplayerReconnector_ReconnectionInProgress);
                    break;

                case MyReconnectionState.RETRY_DELAY:
                    if (this.m_counter > 0)
                    {
                        this.m_counter--;
                    }
                    else
                    {
                        this.m_state = MyReconnectionState.RETRY;
                        this.m_counter = WAIT;
                    }
                    break;

                default:
                    break;
            }
            this.m_stateLast = this.m_state;
            return true;
        }

        private enum MyReconnectionState
        {
            IDLE,
            RETRY,
            WAITING,
            CONNECTING,
            RETRY_DELAY
        }
    }
}


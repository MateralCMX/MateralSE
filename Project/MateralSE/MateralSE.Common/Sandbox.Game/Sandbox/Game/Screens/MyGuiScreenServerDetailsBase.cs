namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenServerDetailsBase : MyGuiScreenBase
    {
        protected DetailsPageEnum CurrentPage;
        protected Vector2 CurrentPosition;
        protected List<MyWorkshopItem> Mods;
        protected float Padding;
        protected Dictionary<string, float> Players;
        protected MyCachedServerItem Server;
        private MyGuiControlButton BT_Settings;
        private MyGuiControlButton BT_Mods;
        private MyGuiControlButton BT_Players;
        private MyGuiControlRotatingWheel m_loadingWheel;
        private bool serverIsFavorited;

        protected MyGuiScreenServerDetailsBase(MyCachedServerItem server) : base(0.5f, new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.9398855f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.Padding = 0.02f;
            this.Server = server;
            this.CreateScreen();
            this.TestFavoritesGameList();
        }

        private void AddAdditionalSettings(ref SortedList<string, object> result)
        {
            if (this.Settings != null)
            {
                result.Add(MyTexts.GetString(MyCommonTexts.ServerDetails_PCU_Initial), MyObjectBuilder_SessionSettings.GetInitialPCU(this.Settings));
            }
        }

        protected unsafe MyGuiControlCheckbox AddCheckbox(MyStringId text, Action<MyGuiControlCheckbox> onClick, MyStringId? tooltip = new MyStringId?())
        {
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox control = new MyGuiControlCheckbox(new Vector2?(this.CurrentPosition), color, (tooltip != null) ? MyTexts.GetString(tooltip.Value) : string.Empty, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            control.PositionX += control.Size.X / 2f;
            this.Controls.Add(control);
            if (onClick != null)
            {
                control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, onClick);
            }
            Vector2? size = null;
            color = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(text), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label.PositionX += control.Size.X + this.Padding;
            this.Controls.Add(label);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += control.Size.Y;
            return control;
        }

        protected unsafe MyGuiControlLabel AddLabel(MyStringId description)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(description), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += (control.Size.Y / 2f) + this.Padding;
            this.Controls.Add(control);
            return control;
        }

        protected unsafe MyGuiControlLabel AddLabel(MyStringId description, object value)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, $"{MyTexts.GetString(description)}: {value}", colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += (control.Size.Y / 2f) + this.Padding;
            this.Controls.Add(control);
            return control;
        }

        protected MyGuiControlMultilineText AddMultilineText(string text, float size) => 
            this.AddMultilineText(new StringBuilder(text), size);

        protected unsafe MyGuiControlMultilineText AddMultilineText(StringBuilder text, float size)
        {
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] -= this.Padding / 2f;
            float* singlePtr2 = (float*) ref this.CurrentPosition.X;
            singlePtr2[0] += 0.003f;
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText control = new MyGuiControlMultilineText(new Vector2?(this.CurrentPosition), new Vector2(base.Size.Value.X - 0.112f, size), backgroundColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            float* singlePtr3 = (float*) ref this.CurrentPosition.X;
            singlePtr3[0] -= 0.003f;
            control.Text = text;
            control.Position += control.Size / 2f;
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.Position = this.CurrentPosition;
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            panel1.Size = control.Size;
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlCompositePanel panel = panel1;
            control.Size = new Vector2(control.Size.X / 1.01f, control.Size.Y / 1.09f);
            float* singlePtr4 = (float*) ref this.CurrentPosition.Y;
            singlePtr4[0] += control.Size.Y + (this.Padding * 1.5f);
            this.Controls.Add(panel);
            this.Controls.Add(control);
            return control;
        }

        protected void AddSeparator(MyGuiControlParent parent, Vector2 localPos, float size = 1f)
        {
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList {
                Size = new Vector2(1f, 0.01f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
            };
            VRageMath.Vector4? color = null;
            control.AddHorizontal(Vector2.Zero, size, 0f, color);
            control.Position = new Vector2(localPos.X, localPos.Y - 0.02f);
            control.Alpha = 0.4f;
            parent.Controls.Add(control);
        }

        private IMyAsyncResult BeginModResultAction() => 
            new LoadModsResult(this.Server);

        private IMyAsyncResult BeginPlayerResultAction() => 
            new LoadPlayersResult(this.Server);

        private void CloseButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        private void CloseFavoritesRequest()
        {
            MyGameService.OnFavoritesServerListResponded -= new EventHandler<int>(this.OnFavoritesServerListResponded);
            MyGameService.OnFavoritesServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnFavoritesServersCompleteResponse);
            MyGameService.CancelFavoritesServersRequest();
            this.m_loadingWheel.Visible = false;
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

        protected unsafe void DrawButtons()
        {
            float x = this.CurrentPosition.X;
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            this.BT_Settings = new MyGuiControlButton(new Vector2?(this.CurrentPosition), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ServerDetails_Settings), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.BT_Settings.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerDetails_Settings));
            this.BT_Settings.PositionX += this.BT_Settings.Size.X / 2f;
            float* singlePtr1 = (float*) ref this.CurrentPosition.X;
            singlePtr1[0] += this.BT_Settings.Size.X + (this.Padding / 4f);
            this.BT_Settings.ButtonClicked += new Action<MyGuiControlButton>(this.SettingButtonClick);
            this.Controls.Add(this.BT_Settings);
            size = null;
            colorMask = null;
            buttonIndex = null;
            this.BT_Mods = new MyGuiControlButton(new Vector2?(this.CurrentPosition), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.WorldSettings_Mods), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.BT_Mods.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerDetails_Mods));
            this.BT_Mods.PositionX += this.BT_Mods.Size.X / 2f;
            float* singlePtr2 = (float*) ref this.CurrentPosition.X;
            singlePtr2[0] += this.BT_Mods.Size.X + (this.Padding / 4f);
            this.BT_Mods.ButtonClicked += new Action<MyGuiControlButton>(this.ModsButtonClick);
            this.Controls.Add(this.BT_Mods);
            size = null;
            colorMask = null;
            buttonIndex = null;
            this.BT_Players = new MyGuiControlButton(new Vector2?(this.CurrentPosition), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenCaptionPlayers), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.BT_Players.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerDetails_Players));
            this.BT_Players.PositionX += this.BT_Players.Size.X / 2f;
            float* singlePtr3 = (float*) ref this.CurrentPosition.X;
            singlePtr3[0] += this.BT_Players.Size.X + (this.Padding / 4f);
            this.BT_Players.ButtonClicked += new Action<MyGuiControlButton>(this.PlayersButtonClick);
            this.Controls.Add(this.BT_Players);
            this.CurrentPosition.X = x;
            float* singlePtr4 = (float*) ref this.CurrentPosition.Y;
            singlePtr4[0] += this.BT_Settings.Size.Y + (this.Padding / 2f);
        }

        private unsafe void DrawMods()
        {
            Vector2? size;
            VRageMath.Vector4? nullable2;
            if ((this.Mods != null) && (this.Mods.Count > 0))
            {
                double byteSize = this.Mods.Sum<MyWorkshopItem>(m => (long) m.Size);
                this.AddLabel(MyCommonTexts.ServerDetails_ModDownloadSize, byteSize.ToString("0.") + " " + MyUtils.FormatByteSizePrefix(ref byteSize) + "B");
            }
            this.AddLabel(MyCommonTexts.WorldSettings_Mods, null);
            if (this.Mods == null)
            {
                size = null;
                nullable2 = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.ServerDetails_ModError), nullable2, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else if (this.Mods.Count == 0)
            {
                size = null;
                nullable2 = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.ServerDetails_NoMods), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                this.Mods.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase));
                MyGuiControlParent scrolledControl = new MyGuiControlParent();
                MyGuiControlScrollablePanel control = new MyGuiControlScrollablePanel(scrolledControl) {
                    ScrollbarVEnabled = true,
                    Position = this.CurrentPosition,
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
                };
                size = base.Size;
                control.Size = new Vector2(base.Size.Value.X - 0.112f, ((size.Value.Y / 2f) - this.CurrentPosition.Y) - 0.145f);
                control.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
                control.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
                this.Controls.Add(control);
                size = null;
                size = null;
                nullable2 = null;
                int? buttonIndex = null;
                MyGuiControlButton button = new MyGuiControlButton(size, MyGuiControlButtonStyleEnum.Close, size, nullable2, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
                scrolledControl.Size = new Vector2(control.Size.X, (this.Mods.Count * ((button.Size.Y / 2f) + this.Padding)) + (button.Size.Y / 2f));
                Vector2 vector = new Vector2(-control.Size.X / 2f, -scrolledControl.Size.Y / 2f);
                foreach (MyWorkshopItem item in this.Mods)
                {
                    size = null;
                    nullable2 = null;
                    buttonIndex = null;
                    MyGuiControlButton button2 = new MyGuiControlButton(new Vector2?(vector), MyGuiControlButtonStyleEnum.ClickableText, size, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, new StringBuilder(item.Title), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                        UserData = item.Id
                    };
                    int num2 = Math.Min(item.Description.Length, 0x80);
                    int index = item.Description.IndexOf("\n");
                    if (index > 0)
                    {
                        num2 = Math.Min(num2, index - 1);
                    }
                    button2.SetToolTip(item.Description.Substring(0, num2));
                    button2.ButtonClicked += new Action<MyGuiControlButton>(this.ModURLClick);
                    float* singlePtr1 = (float*) ref vector.Y;
                    singlePtr1[0] += (button2.Size.Y / 2f) + this.Padding;
                    scrolledControl.Controls.Add(button2);
                }
            }
        }

        private unsafe void DrawPlayers()
        {
            Vector2? size;
            VRageMath.Vector4? nullable2;
            MyGuiControlLabel label = this.AddLabel(MyCommonTexts.ScreenCaptionPlayers, null);
            if (this.Players == null)
            {
                size = null;
                nullable2 = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.ServerDetails_PlayerError), nullable2, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else if (this.Players.Count == 0)
            {
                size = null;
                nullable2 = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2?(this.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.ServerDetails_ServerEmpty), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                MyGuiControlParent scrolledControl = new MyGuiControlParent();
                MyGuiControlScrollablePanel control = new MyGuiControlScrollablePanel(scrolledControl) {
                    ScrollbarVEnabled = true,
                    Position = this.CurrentPosition,
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
                };
                size = base.Size;
                control.Size = new Vector2(base.Size.Value.X - 0.112f, ((size.Value.Y / 2f) - this.CurrentPosition.Y) - 0.145f);
                control.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
                control.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
                this.Controls.Add(control);
                scrolledControl.Size = new Vector2(control.Size.X, (this.Players.Count * ((label.Size.Y / 2f) + this.Padding)) + (label.Size.Y / 2f));
                Vector2 vector = new Vector2(-control.Size.X / 2f, (-scrolledControl.Size.Y / 2f) + (label.Size.Y / 2f));
                foreach (KeyValuePair<string, float> pair in this.Players)
                {
                    StringBuilder output = new StringBuilder(pair.Key);
                    output.Append(": ");
                    MyValueFormatter.AppendTimeInBestUnit((float) ((int) pair.Value), output);
                    size = null;
                    nullable2 = null;
                    MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(vector), size, output.ToString(), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                    float* singlePtr1 = (float*) ref vector.Y;
                    singlePtr1[0] += (label2.Size.Y / 2f) + this.Padding;
                    scrolledControl.Controls.Add(label2);
                }
            }
        }

        protected abstract void DrawSettings();
        private void EndModResultAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            LoadModsResult result2 = (LoadModsResult) result;
            this.Mods = result2.ServerMods;
            screen.CloseScreen();
            this.m_loadingWheel.Visible = false;
            this.RecreateControls(false);
        }

        private void EndPlayerResultAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            LoadPlayersResult result2 = (LoadPlayersResult) result;
            this.Players = result2.Players;
            screen.CloseScreen();
            this.m_loadingWheel.Visible = false;
            this.RecreateControls(false);
        }

        private void FavoriteButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGameServerItem server = this.Server.Server;
            MyGameService.AddFavoriteGame(server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.NetAdr.Port, (ushort) server.NetAdr.Port);
            this.serverIsFavorited = true;
            this.RecreateControls(false);
        }

        public override string GetFriendlyName() => 
            "ServerDetails";

        private void JoinButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        protected SortedList<string, object> LoadSessionSettings(VRage.Game.Game game)
        {
            if (this.Settings == null)
            {
                return null;
            }
            SortedList<string, object> result = new SortedList<string, object>();
            foreach (FieldInfo info in typeof(MyObjectBuilder_SessionSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                GameRelationAttribute customAttribute = info.GetCustomAttribute<GameRelationAttribute>();
                if ((customAttribute != null) && ((customAttribute.RelatedTo == VRage.Game.Game.Shared) || (customAttribute.RelatedTo == game)))
                {
                    DisplayAttribute attribute2 = info.GetCustomAttribute<DisplayAttribute>();
                    if ((attribute2 != null) && !string.IsNullOrEmpty(attribute2.Name))
                    {
                        string keyString = "ServerDetails_" + info.Name;
                        if (MyTexts.GetString(keyString) != keyString)
                        {
                            result.Add(keyString, info.GetValue(this.Settings));
                        }
                    }
                }
            }
            this.AddAdditionalSettings(ref result);
            return result;
        }

        private void ModsButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = DetailsPageEnum.Mods;
            if ((this.Server.Mods != null) && (this.Server.Mods.Count > 0))
            {
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.BeginModResultAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.EndModResultAction), null));
            }
            else if ((this.Server.Mods == null) || (this.Server.Mods.Count != 0))
            {
                this.RecreateControls(false);
            }
            else
            {
                this.Mods = new List<MyWorkshopItem>();
                this.RecreateControls(false);
            }
        }

        private void ModURLClick(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrl(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, (ulong) button.UserData), UrlOpenMode.SteamOrExternalWithConfirm, null);
        }

        private void OnFavoritesServerListResponded(object sender, int server)
        {
            MyCachedServerItem item = new MyCachedServerItem(MyGameService.GetFavoritesServerDetails(server));
            MyCachedServerItem item2 = this.Server;
            if ((item.Server.NetAdr.Address.ToString() == item2.Server.NetAdr.Address.ToString()) && (item.Server.NetAdr.Port.ToString() == item2.Server.NetAdr.Port.ToString()))
            {
                this.serverIsFavorited = true;
                this.RecreateControls(false);
                this.m_loadingWheel.Visible = false;
            }
        }

        private void OnFavoritesServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseFavoritesRequest();
        }

        private void ParseIPAndConnect()
        {
            try
            {
                ushort port = ushort.Parse(this.Server.Server.NetAdr.Port.ToString());
                MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(MySandboxGame.Static.ServerResponded);
                MyGameService.OnPingServerFailedToRespond += new EventHandler(MySandboxGame.Static.ServerFailedToRespond);
                MyGameService.PingServer(Dns.GetHostAddresses(this.Server.Server.NetAdr.Address.MapToIPv4().ToString())[0].ToIPv4NetworkOrder(), port);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
                MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
        }

        private void PlayersButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = DetailsPageEnum.Players;
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.BeginPlayerResultAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.EndPlayerResultAction), null));
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            int? nullable3;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.JoinGame_ServerDetails, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(list2);
            MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list3.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.15f), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(list3);
            MyGuiControlSeparatorList list4 = new MyGuiControlSeparatorList();
            float num = 0.303f;
            if (this.Server.ExperimentalMode)
            {
                num = 0.34f;
            }
            captionTextColor = null;
            list4.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - num), base.m_size.Value.X * 0.835f, 0f, captionTextColor);
            this.Controls.Add(list4);
            this.CurrentPosition = new Vector2(0f, 0f) - new Vector2(((base.m_size.Value.X * 0.835f) / 2f) - 0.003f, (base.m_size.Value.Y / 2f) - 0.116f);
            this.DrawButtons();
            Vector2? textureResolution = null;
            this.m_loadingWheel = new MyGuiControlRotatingWheel(new Vector2?(this.BT_Players.Position + new Vector2(0.137f, -0.004f)), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, textureResolution, 1.5f);
            this.Controls.Add(this.m_loadingWheel);
            this.m_loadingWheel.Visible = false;
            if (!this.serverIsFavorited)
            {
                textureResolution = null;
                captionTextColor = null;
                nullable3 = null;
                MyGuiControlButton button2 = new MyGuiControlButton(new Vector2?(new Vector2(0f, 0f) - new Vector2(-0.003f, (-base.m_size.Value.Y / 2f) + 0.071f)), MyGuiControlButtonStyleEnum.Default, textureResolution, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ServerDetails_AddFavorite), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false);
                button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerDetails_AddFavorite));
                button2.ButtonClicked += new Action<MyGuiControlButton>(this.FavoriteButtonClick);
                this.Controls.Add(button2);
            }
            else
            {
                textureResolution = null;
                captionTextColor = null;
                nullable3 = null;
                MyGuiControlButton button3 = new MyGuiControlButton(new Vector2?(new Vector2(0f, 0f) - new Vector2(-0.003f, (-base.m_size.Value.Y / 2f) + 0.071f)), MyGuiControlButtonStyleEnum.Default, textureResolution, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ServerDetails_RemoveFavorite), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false);
                button3.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerDetails_RemoveFavorite));
                button3.ButtonClicked += new Action<MyGuiControlButton>(this.UnFavoriteButtonClick);
                this.Controls.Add(button3);
            }
            textureResolution = null;
            captionTextColor = null;
            nullable3 = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?(new Vector2(0f, 0f) - new Vector2(0.18f, (-base.m_size.Value.Y / 2f) + 0.071f)), MyGuiControlButtonStyleEnum.Default, textureResolution, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.JoinGame_Title), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, nullable3, false);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGame_JoinWorld));
            button.ButtonClicked += new Action<MyGuiControlButton>(this.ConnectButtonClick);
            this.Controls.Add(button);
            float* singlePtr1 = (float*) ref this.CurrentPosition.Y;
            singlePtr1[0] += 0.012f;
            this.AddLabel(MyCommonTexts.ServerDetails_Server, this.Server.Server.Name);
            this.AddLabel(MyCommonTexts.ServerDetails_Map, this.Server.Server.Map);
            this.AddLabel(MyCommonTexts.ServerDetails_Version, new MyVersion((int) this.Server.Server.GetGameTagByPrefixUlong("version")).FormattedText.ToString().Replace("_", "."));
            this.AddLabel(MyCommonTexts.ServerDetails_IPAddress, this.Server.Server.NetAdr);
            if (this.Server.ExperimentalMode)
            {
                this.AddLabel(MyCommonTexts.ServerIsExperimental);
            }
            float* singlePtr2 = (float*) ref this.CurrentPosition.Y;
            singlePtr2[0] += 0.028f;
            switch (this.CurrentPage)
            {
                case DetailsPageEnum.Settings:
                    base.FocusedControl = this.BT_Settings;
                    this.BT_Settings.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.BT_Settings.HasHighlight = true;
                    this.BT_Settings.Selected = true;
                    this.DrawSettings();
                    return;

                case DetailsPageEnum.Mods:
                    base.FocusedControl = this.BT_Mods;
                    this.BT_Mods.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.BT_Mods.HasHighlight = true;
                    this.BT_Mods.Selected = true;
                    this.DrawMods();
                    return;

                case DetailsPageEnum.Players:
                    base.FocusedControl = this.BT_Players;
                    this.BT_Players.HighlightType = MyGuiControlHighlightType.FORCED;
                    this.BT_Players.HasHighlight = true;
                    this.BT_Players.Selected = true;
                    this.DrawPlayers();
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        private void SettingButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CurrentPage = DetailsPageEnum.Settings;
            this.RecreateControls(false);
        }

        private void TestFavoritesGameList()
        {
            MyGameService.OnFavoritesServerListResponded += new EventHandler<int>(this.OnFavoritesServerListResponded);
            MyGameService.OnFavoritesServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnFavoritesServersCompleteResponse);
            MyGameService.RequestFavoritesServerList($"gamedir:{MyPerGameSettings.SteamGameServerGameDir};secure:1");
            this.m_loadingWheel.Visible = true;
        }

        private void UnFavoriteButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGameServerItem server = this.Server.Server;
            MyGameService.RemoveFavoriteGame(server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.NetAdr.Port, (ushort) server.NetAdr.Port);
            MyGuiScreenJoinGame firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenJoinGame>();
            if (firstScreenOfType.m_selectedPage.Name == "PageFavoritesPanel")
            {
                firstScreenOfType.RemoveFavoriteServer(this.Server);
            }
            this.serverIsFavorited = false;
            this.RecreateControls(false);
        }

        protected MyObjectBuilder_SessionSettings Settings =>
            this.Server.Settings;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenServerDetailsBase.<>c <>9 = new MyGuiScreenServerDetailsBase.<>c();
            public static Func<MyWorkshopItem, long> <>9__17_1;
            public static Comparison<MyWorkshopItem> <>9__17_0;

            internal int <DrawMods>b__17_0(MyWorkshopItem a, MyWorkshopItem b) => 
                string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase);

            internal long <DrawMods>b__17_1(MyWorkshopItem m) => 
                ((long) m.Size);
        }

        protected enum DetailsPageEnum
        {
            Settings,
            Mods,
            Players
        }

        private class LoadModsResult : IMyAsyncResult
        {
            public LoadModsResult(MyCachedServerItem server)
            {
                this.ServerMods = new List<MyWorkshopItem>();
                this.Task = Parallel.Start(delegate {
                    if (MyGameService.IsOnline && ((server.Mods != null) && (server.Mods.Count > 0)))
                    {
                        MyWorkshop.GetItemsBlockingUGC(server.Mods, this.ServerMods);
                    }
                });
            }

            public bool IsCompleted =>
                this.Task.IsComplete;

            public ParallelTasks.Task Task { get; private set; }

            public List<MyWorkshopItem> ServerMods { get; private set; }
        }

        private class LoadPlayersResult : IMyAsyncResult
        {
            public LoadPlayersResult(MyCachedServerItem server)
            {
                MyGameService.GetPlayerDetails(server.Server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.Server.NetAdr.Port, new PlayerDetailsResponse(this.LoadCompleted), () => this.LoadCompleted(null));
            }

            private void LoadCompleted(Dictionary<string, float> players)
            {
                this.Players = players;
                this.IsCompleted = true;
            }

            public Dictionary<string, float> Players { get; private set; }

            public bool IsCompleted { get; private set; }

            public ParallelTasks.Task Task { get; private set; }
        }
    }
}


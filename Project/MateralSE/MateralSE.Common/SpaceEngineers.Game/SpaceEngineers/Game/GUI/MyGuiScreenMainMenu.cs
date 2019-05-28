namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyGuiScreenMainMenu : MyGuiScreenMainMenuBase
    {
        private static MyStringHash Deluxe = MyStringHash.GetOrCompute("Deluxe");
        private static MyStringHash DecoBlockDlc = MyStringHash.GetOrCompute("Decoratives");
        private static MyStringHash SkinSale = MyStringHash.GetOrCompute("SkinSale");
        private static MyStringHash GhostSkin = MyStringHash.GetOrCompute("GhostSkin");
        private static MyStringHash PromotedEngineer = MyStringHash.GetOrCompute("PromotedEngineer");
        private Dictionary<MyStringHash, MyBannerInfo> m_banners;
        private Dictionary<MyStringHash, MyBadge> m_badges;
        private MyGuiControlBase[] m_bannerControls;
        private Task m_activeEventsTask;
        private bool m_downloadedActiveEventsOK;
        private readonly int DLC_UPDATE_INTERVAL;
        private MyGuiControlNews m_newsControl;
        private MyGuiControlElementGroup m_elementGroup;
        private int m_currentDLCcounter;
        private bool isStartMenu;
        private const int CONTROLS_PER_BANNER = 3;
        public MyGuiControlButton m_exitGameButton;
        public MyGuiControlImageButton m_lastClickedBanner;
        private bool m_canChangeBannerHighlight;

        public MyGuiScreenMainMenu() : this(false)
        {
        }

        public MyGuiScreenMainMenu(bool pauseGame) : base(pauseGame)
        {
            MyBannerInfo info1 = new MyBannerInfo();
            info1.Active = true;
            info1.Status = MyBannerStatus.Offline;
            info1.HighlightTexture = @"Textures\GUI\DLCs\Deluxe\DeluxeBannerHighlight.dds";
            info1.Texture = @"Textures\GUI\DLCs\Deluxe\DeluxeBanner.dds";
            info1.URL = MyDLCs.MyDLC.DeluxeEdition.URL;
            info1.DLCId = new uint?(MyDLCs.MyDLC.DeluxeEdition.AppId);
            info1.ToolTip = MySpaceTexts.ScreenMainMenu_DeluxeLine1;
            info1.Line1 = MySpaceTexts.ScreenMainMenu_DeluxeLine1;
            info1.Line2 = MySpaceTexts.ScreenMainMenu_DeluxeLine2;
            Dictionary<MyStringHash, MyBannerInfo> dictionary1 = new Dictionary<MyStringHash, MyBannerInfo>();
            dictionary1.Add(Deluxe, info1);
            MyBannerInfo info2 = new MyBannerInfo();
            info2.Active = false;
            info2.Status = MyBannerStatus.Offline;
            info2.HighlightTexture = @"Textures\GUI\SkinSaleHighlight.png";
            info2.Texture = @"Textures\GUI\SkinSale.png";
            info2.URL = MyPerGameSettings.SkinSaleUrl;
            info2.ToolTip = MySpaceTexts.ScreenMainMenu_SkinSaleLine1;
            info2.Line1 = MySpaceTexts.ScreenMainMenu_SkinSaleLine1;
            info2.Line2 = MySpaceTexts.ScreenMainMenu_SkinSaleLine2;
            dictionary1.Add(SkinSale, info2);
            MyBannerInfo info3 = new MyBannerInfo();
            info3.Active = false;
            info3.Status = MyBannerStatus.Offline;
            info3.HighlightTexture = @"Textures\GUI\GhostSkinHighlight.dds";
            info3.Texture = @"Textures\GUI\GhostSkin.dds";
            info3.URL = "http://store.steampowered.com/itemstore/244850/detail/359/";
            info3.ToolTip = MySpaceTexts.ScreenMainMenu_GhostSkinLine1;
            info3.Line1 = MySpaceTexts.ScreenMainMenu_GhostSkinLine1;
            info3.Line2 = MySpaceTexts.ScreenMainMenu_GhostSkinLine2;
            dictionary1.Add(GhostSkin, info3);
            MyBannerInfo info4 = new MyBannerInfo();
            info4.Active = true;
            info4.Status = MyBannerStatus.Offline;
            info4.HighlightTexture = @"Textures\GUI\DLCs\Decorative\BannerHighlight.DDS";
            info4.Texture = @"Textures\GUI\DLCs\Decorative\Banner.DDS";
            info4.URL = MyDLCs.MyDLC.DecorativeBlocks.URL;
            info4.DLCId = new uint?(MyDLCs.MyDLC.DecorativeBlocks.AppId);
            info4.ToolTip = MySpaceTexts.ScreenMainMenu_DecoBlockDlcLine1;
            info4.Line1 = MySpaceTexts.ScreenMainMenu_DecoBlockDlcLine1;
            info4.Line2 = MySpaceTexts.ScreenMainMenu_DecoBlockDlcLine2;
            dictionary1.Add(DecoBlockDlc, info4);
            this.m_banners = dictionary1;
            MyBadge badge1 = new MyBadge();
            badge1.Status = MyBannerStatus.Offline;
            badge1.Texture = MyDLCs.MyDLC.DeluxeEdition.Badge;
            badge1.DLCId = MyDLCs.MyDLC.DeluxeEdition.AppId;
            badge1.AchievementName = "";
            Dictionary<MyStringHash, MyBadge> dictionary2 = new Dictionary<MyStringHash, MyBadge>();
            dictionary2.Add(Deluxe, badge1);
            MyBadge badge2 = new MyBadge();
            badge2.Status = MyBannerStatus.Offline;
            badge2.Texture = @"Textures\GUI\PromotedEngineer.dds";
            badge2.DLCId = 0;
            badge2.AchievementName = "Promoted_engineer";
            dictionary2.Add(PromotedEngineer, badge2);
            MyBadge badge3 = new MyBadge();
            badge3.Status = MyBannerStatus.Offline;
            badge3.Texture = MyDLCs.MyDLC.DecorativeBlocks.Badge;
            badge3.DLCId = MyDLCs.MyDLC.DecorativeBlocks.AppId;
            badge3.AchievementName = "";
            dictionary2.Add(DecoBlockDlc, badge3);
            this.m_badges = dictionary2;
            this.DLC_UPDATE_INTERVAL = 0x1388;
            this.isStartMenu = true;
            this.m_canChangeBannerHighlight = true;
            if (!pauseGame && (MyGuiScreenGamePlay.Static == null))
            {
                this.AddIntroScreen();
            }
            MyGuiSandbox.DrawGameLogoHandler = new Action<float, Vector2>(this.DrawGameLogo);
        }

        private void AddIntroScreen()
        {
            if (MyFakes.ENABLE_MENU_VIDEO_BACKGROUND)
            {
                MyGuiSandbox.AddScreen(MyGuiScreenIntroVideo.CreateBackgroundScreen());
            }
        }

        private void banner_HightlightChanged(MyGuiControlBase obj)
        {
            if (this.m_canChangeBannerHighlight)
            {
                this.m_canChangeBannerHighlight = false;
                if (obj.UserData != null)
                {
                    int userData = (int) obj.UserData;
                    bool hasHighlight = obj.HasHighlight;
                    MyGuiControlBase objA = this.m_bannerControls[userData * 3];
                    if (!ReferenceEquals(objA, obj))
                    {
                        objA.HighlightType = hasHighlight ? MyGuiControlHighlightType.CUSTOM : MyGuiControlHighlightType.WHEN_CURSOR_OVER;
                        objA.HasHighlight = hasHighlight;
                    }
                    MyGuiControlBase base3 = this.m_bannerControls[(userData * 3) + 1];
                    if (!ReferenceEquals(base3, obj))
                    {
                        base3.HighlightType = hasHighlight ? MyGuiControlHighlightType.CUSTOM : MyGuiControlHighlightType.WHEN_CURSOR_OVER;
                        base3.HasHighlight = hasHighlight;
                    }
                    MyGuiControlBase base4 = this.m_bannerControls[(userData * 3) + 2];
                    if (!ReferenceEquals(base4, obj))
                    {
                        base4.HighlightType = hasHighlight ? MyGuiControlHighlightType.CUSTOM : MyGuiControlHighlightType.WHEN_CURSOR_OVER;
                        base4.HasHighlight = hasHighlight;
                    }
                }
                this.m_canChangeBannerHighlight = true;
            }
        }

        private void CreateBanners()
        {
            float num = 10f;
            this.m_bannerControls = new MyGuiControlBase[this.m_banners.Count * 3];
            int num2 = 0;
            foreach (MyBannerInfo info in this.m_banners.Values)
            {
                num2++;
                int num3 = num2;
                if (info.Active && (info.Status != MyBannerStatus.Installed))
                {
                    MyGuiControlImageButton.StateDefinition definition1 = new MyGuiControlImageButton.StateDefinition();
                    definition1.Texture = new MyGuiCompositeTexture(info.HighlightTexture);
                    MyGuiControlImageButton.StyleDefinition style = new MyGuiControlImageButton.StyleDefinition();
                    style.Highlight = definition1;
                    MyGuiControlImageButton.StateDefinition definition3 = new MyGuiControlImageButton.StateDefinition();
                    definition3.Texture = new MyGuiCompositeTexture(info.HighlightTexture);
                    style.ActiveHighlight = definition3;
                    MyGuiControlImageButton.StateDefinition definition4 = new MyGuiControlImageButton.StateDefinition();
                    definition4.Texture = new MyGuiCompositeTexture(info.Texture);
                    style.Normal = definition4;
                    string url = info.URL;
                    Vector2 vector = new Vector2(0.2375f, 0.13f);
                    VRageMath.Vector4? colorMask = null;
                    int? buttonIndex = null;
                    MyGuiControlImageButton control = new MyGuiControlImageButton("Button", new Vector2?(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), new Vector2?(vector), colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, x => MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null), null, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                        BackgroundTexture = new MyGuiCompositeTexture(info.Texture)
                    };
                    control.ApplyStyle(style);
                    control.CanHaveFocus = false;
                    control.SetToolTip(info.ToolTip);
                    control.HightlightChanged += new Action<MyGuiControlBase>(this.banner_HightlightChanged);
                    control.UserData = num3;
                    control.ButtonClicked += new Action<MyGuiControlImageButton>(this.MenuRefocusImageButton);
                    this.Controls.Add(control);
                    this.m_bannerControls[num3 * 3] = control;
                    MyStringId? tooltip = null;
                    MyGuiControlButton button2 = base.MakeButton((MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)) + (new Vector2(0f, 29f) / MyGuiConstants.GUI_OPTIMAL_SIZE), MySpaceTexts.Blank, x => MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null), tooltip);
                    button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                    button2.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                    button2.Text = MyTexts.GetString(info.Line1);
                    button2.Alpha = 1f;
                    button2.VisualStyle = MyGuiControlButtonStyleEnum.UrlTextNoLine;
                    button2.Size = new Vector2(0.22f, 0.13f);
                    button2.HightlightChanged += new Action<MyGuiControlBase>(this.banner_HightlightChanged);
                    button2.SetToolTip(info.ToolTip);
                    button2.UserData = num3;
                    button2.CanHaveFocus = false;
                    this.Controls.Add(button2);
                    this.m_bannerControls[(num3 * 3) + 1] = button2;
                    tooltip = null;
                    MyGuiControlButton button3 = base.MakeButton((MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)) + (new Vector2(0f, 54f) / MyGuiConstants.GUI_OPTIMAL_SIZE), MySpaceTexts.Blank, x => MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null), tooltip);
                    button3.Text = string.Format(MyTexts.GetString(info.Line2), MySession.Platform);
                    button3.Alpha = 1f;
                    button3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                    button3.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                    button3.VisualStyle = MyGuiControlButtonStyleEnum.UrlTextNoLine;
                    button3.Size = new Vector2(0.22f, 0.13f);
                    button3.HightlightChanged += new Action<MyGuiControlBase>(this.banner_HightlightChanged);
                    button3.SetToolTip(info.ToolTip);
                    button3.UserData = num3;
                    button3.CanHaveFocus = false;
                    this.Controls.Add(button3);
                    this.m_bannerControls[(num3 * 3) + 2] = button3;
                    Vector2 vector2 = new Vector2(0.004f, 0f);
                    Vector2 vector3 = new Vector2(0.22f, 0f);
                    colorMask = null;
                    MyGuiControlPanel panel = new MyGuiControlPanel(new Vector2?(((MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)) + vector3) - vector2), new Vector2(0.22f, 0.13f), colorMask, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM) {
                        BackgroundTexture = MyGuiConstants.TEXTURE_NEWS_BACKGROUND_BlueLine
                    };
                    this.Controls.Add(panel);
                    num -= 3.5f;
                }
            }
        }

        private MyGuiControlBase CreateImageTooltip(string path, string text)
        {
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            parent1.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            parent1.ColorMask = (VRageMath.Vector4) MyGuiConstants.THEMED_GUI_BACKGROUND_COLOR;
            MyGuiControlParent parent = parent1;
            parent.CanHaveFocus = false;
            parent.HighlightType = MyGuiControlHighlightType.NEVER;
            parent.BorderEnabled = true;
            Vector2 vector = new Vector2(0.005f, 0.002f);
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(new Vector2?(Vector2.Zero), size, text, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlLabel control = label1;
            control.CanHaveFocus = false;
            control.HighlightType = MyGuiControlHighlightType.NEVER;
            colorMask = null;
            MyGuiControlImage image1 = new MyGuiControlImage(new Vector2?(Vector2.Zero), new Vector2(0.175625f, 0.1317188f), colorMask, null, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            MyGuiControlImage image = image1;
            image.SetTexture(path);
            image.CanHaveFocus = false;
            image.HighlightType = MyGuiControlHighlightType.NEVER;
            parent.Size = new Vector2(Math.Max(control.Size.X, image.Size.X) + (vector.X * 2f), (control.Size.Y + image.Size.Y) + (vector.Y * 4f));
            parent.Controls.Add(image);
            parent.Controls.Add(control);
            control.Position = (-parent.Size / 2f) + vector;
            image.Position = new Vector2(0f, (parent.Size.Y / 2f) - vector.Y);
            return parent;
        }

        public void DownloadActiveEvents()
        {
            if ((this.m_activeEventsTask == null) || this.m_activeEventsTask.IsCompleted)
            {
                this.m_activeEventsTask = Task.Run(() => this.DownloadActiveEventsAsync()).ContinueWith(task => this.DownloadActiveEventsCompleted());
            }
        }

        private void DownloadActiveEventsAsync()
        {
            try
            {
                WebClient client1 = new WebClient();
                client1.Proxy = null;
                using (StringReader reader = new StringReader(client1.DownloadString(new Uri(MyPerGameSettings.EventsUrl))))
                {
                    string[] separator = new string[] { System.Environment.NewLine };
                    string[] strArray = reader.ReadToEnd().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    int index = 0;
                    while (true)
                    {
                        if (index >= strArray.Length)
                        {
                            this.m_downloadedActiveEventsOK = true;
                            break;
                        }
                        string str = strArray[index];
                        if (!str.StartsWith("//") && this.m_banners.ContainsKey(MyStringHash.GetOrCompute(str)))
                        {
                            this.m_banners[MyStringHash.GetOrCompute(str)].Active = true;
                        }
                        index++;
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error while downloading events: " + exception.ToString());
            }
        }

        private void DownloadActiveEventsCompleted()
        {
            if (this.m_downloadedActiveEventsOK)
            {
                MySandboxGame.Static.Invoke(() => this.RecreateBanners(), "DownloadActiveEventsCompleted");
            }
        }

        private unsafe void DrawGameLogo(float transitionAlpha, Vector2 position)
        {
            MyGuiSandbox.DrawGameLogo(transitionAlpha, position);
            float* singlePtr1 = (float*) ref position.X;
            singlePtr1[0] += 0.46875f;
            Vector2 size = new Vector2(0.16f, 0.2133333f);
            size *= 0.7f;
            foreach (KeyValuePair<MyStringHash, MyBadge> pair in this.m_badges)
            {
                if (pair.Value.Status == MyBannerStatus.Installed)
                {
                    MyGuiSandbox.DrawBadge(pair.Value.Texture, transitionAlpha, position, size);
                    float* singlePtr2 = (float*) ref position.X;
                    singlePtr2[0] += size.X;
                }
            }
        }

        private string GetThumbnail(MyObjectBuilder_LastSession session)
        {
            if (session == null)
            {
                return null;
            }
            string path = session.Path;
            if (Directory.Exists(path + MyGuiScreenLoadSandbox.CONST_BACKUP))
            {
                string[] directories = Directory.GetDirectories(path + MyGuiScreenLoadSandbox.CONST_BACKUP);
                if (directories.Any<string>())
                {
                    string str3 = directories.Last<string>() + MyGuiScreenLoadSandbox.CONST_THUMB;
                    if (System.IO.File.Exists(str3) && (new FileInfo(str3).Length > 0L))
                    {
                        return (Directory.GetDirectories(path + MyGuiScreenLoadSandbox.CONST_BACKUP).Last<string>() + MyGuiScreenLoadSandbox.CONST_THUMB);
                    }
                }
            }
            string str2 = path + MyGuiScreenLoadSandbox.CONST_THUMB;
            if (!System.IO.File.Exists(str2) || (new FileInfo(str2).Length <= 0L))
            {
                return null;
            }
            return (path + MyGuiScreenLoadSandbox.CONST_THUMB);
        }

        private void joinGameScreen_Closed(MyGuiScreenBase source)
        {
            if (source.Cancelled)
            {
                base.State = MyGuiScreenState.OPENING;
                source.Closed -= new MyGuiScreenBase.ScreenHandler(this.joinGameScreen_Closed);
            }
        }

        private void m_elementGroup_SelectedChanged(MyGuiControlElementGroup obj)
        {
            foreach (MyGuiControlBase base2 in this.m_elementGroup)
            {
                if (base2.HasFocus && !ReferenceEquals(obj.SelectedElement, base2))
                {
                    base.FocusedControl = obj.SelectedElement;
                    break;
                }
            }
        }

        private void MenuRefocusImageButton(MyGuiControlImageButton sender)
        {
            this.m_lastClickedBanner = sender;
        }

        private void OnClickBack(MyGuiControlButton obj)
        {
            this.isStartMenu = true;
            this.RecreateControls(false);
        }

        private void OnClickExitToWindows(MyGuiControlButton sender)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnExitToWindowsMessageBoxCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnClickInventory(MyGuiControlButton obj)
        {
            if (!MyGameService.IsActive)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SteamIsOfflinePleaseRestart), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            else if (MySession.Static != null)
            {
                object[] args = new object[2];
                args[0] = false;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(args));
            }
            else
            {
                MyGuiScreenLoadInventory inventory = MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(Array.Empty<object>());
                inventory.OnLoadingAction = (Action) Delegate.Combine(inventory.OnLoadingAction, delegate {
                    MySessionLoader.LoadInventoryScene();
                    MySandboxGame.IsUpdateReady = true;
                    inventory.Initialize(false, null);
                });
                MyGuiSandbox.AddScreen(new MyGuiScreenLoading(inventory, null));
            }
        }

        private void OnClickLoad(MyGuiControlBase sender)
        {
            this.RunWithTutorialCheck(() => MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox()));
        }

        private void OnClickNewGame(MyGuiControlButton sender)
        {
            this.RunWithTutorialCheck(() => MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNewGame>(Array.Empty<object>())));
        }

        private void OnClickOptions(MyGuiControlButton sender)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenOptionsSpace>(Array.Empty<object>()));
        }

        private void OnClickPlayers(MyGuiControlButton obj)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenPlayers>(Array.Empty<object>()));
        }

        private void OnClickRecommend(MyGuiControlButton sender)
        {
            if (MyFakes.XBOX_PREVIEW)
            {
                MyGuiSandbox.Show(MyCommonTexts.MessageBoxTextErrorFeatureNotAvailableYet, MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.MessageBoxCaptionRecommend);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextRecommend), MySession.Platform), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClickRecommendOK), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnClickRecommendOK(MyGuiScreenMessageBox.ResultEnum result)
        {
            MyGuiSandbox.OpenUrl(MySteamConstants.URL_RECOMMEND_GAME, UrlOpenMode.SteamOrExternal, null);
        }

        private void OnClickReportBug(MyGuiControlButton obj)
        {
            MyGuiSandbox.OpenUrl(MyPerGameSettings.BugReportUrl, UrlOpenMode.SteamOrExternalWithConfirm, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextOpenBrowser, "forums.keenswh.com"));
        }

        private void OnClickSaveAs(MyGuiControlButton sender)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenSaveAs(MySession.Static.Name));
        }

        private void OnClickSaveWorld(MyGuiControlButton sender)
        {
            MyGuiScreenMessageBox box;
            MyStringId? nullable;
            Vector2? nullable2;
            base.CanBeHidden = false;
            if (MyAsyncSaving.InProgress)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                box = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextSavingInProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
            }
            else
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                box = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextDoYouWantToSaveYourProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnSaveWorldMessageBoxCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
            }
            box.SkipTransition = true;
            box.InstantClose = false;
            MyGuiSandbox.AddScreen(box);
        }

        private void OnContinueGameClicked(MyGuiControlButton myGuiControlButton)
        {
            this.RunWithTutorialCheck(delegate {
                MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
                if (lastSession != null)
                {
                    if (!lastSession.IsOnline)
                    {
                        MySessionLoader.LoadLastSession();
                    }
                    else if (lastSession.IsLobby)
                    {
                        MyJoinGameHelper.JoinGame(ulong.Parse(lastSession.ServerIP));
                    }
                    else
                    {
                        try
                        {
                            ushort serverPort = (ushort) lastSession.ServerPort;
                            MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(MySandboxGame.Static.ServerResponded);
                            MyGameService.OnPingServerFailedToRespond += new EventHandler(MySandboxGame.Static.ServerFailedToRespond);
                            MyGameService.PingServer(Dns.GetHostAddresses(lastSession.ServerIP)[0].ToIPv4NetworkOrder(), serverPort);
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine(exception);
                            MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
                        }
                    }
                }
            });
        }

        private void OnCustomGameClicked(MyGuiControlButton myGuiControlButton)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenWorldSettings>(Array.Empty<object>()));
        }

        private void OnExitToMainMenuClick(MyGuiControlButton sender)
        {
            MyGuiScreenMessageBox box;
            MyStringId? nullable;
            Vector2? nullable2;
            base.CanBeHidden = false;
            if (!Sync.IsServer)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                box = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAnyWorldBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnExitToMainMenuFromCampaignMessageBoxCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
            }
            else if (!MySession.Static.Settings.EnableSaving || !Sync.IsServer)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                box = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextCampaignBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnExitToMainMenuFromCampaignMessageBoxCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
            }
            else
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                box = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.MessageBoxTextSaveChangesBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnExitToMainMenuMessageBoxCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
            }
            box.SkipTransition = true;
            box.InstantClose = false;
            MyGuiSandbox.AddScreen(box);
        }

        private void OnExitToMainMenuFromCampaignMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn != MyGuiScreenMessageBox.ResultEnum.YES)
            {
                base.CanBeHidden = true;
            }
            else
            {
                MyAudio.Static.Mute = true;
                MyAudio.Static.StopMusic();
                MySessionLoader.UnloadAndExitToMenu();
            }
        }

        private void OnExitToMainMenuMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            switch (callbackReturn)
            {
                case MyGuiScreenMessageBox.ResultEnum.YES:
                    MyAudio.Static.Mute = true;
                    MyAudio.Static.StopMusic();
                    MyAsyncSaving.Start(delegate {
                        MySandboxGame.Static.OnScreenshotTaken += new EventHandler(this.UnloadAndExitAfterScreeshotWasTaken);
                    }, null, true);
                    return;

                case MyGuiScreenMessageBox.ResultEnum.NO:
                    MyAudio.Static.Mute = true;
                    MyAudio.Static.StopMusic();
                    MySessionLoader.UnloadAndExitToMenu();
                    return;

                case MyGuiScreenMessageBox.ResultEnum.CANCEL:
                    base.CanBeHidden = true;
                    return;
            }
        }

        private void OnExitToWindowsMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.OnLogoutProgressClosed();
            }
            else if ((this.m_exitGameButton != null) && this.m_exitGameButton.Visible)
            {
                base.FocusedControl = this.m_exitGameButton;
                this.m_exitGameButton.Selected = true;
            }
        }

        private void OnJoinWorld(MyGuiControlButton sender)
        {
            this.RunWithTutorialCheck(delegate {
                if (MyGameService.IsOnline)
                {
                    MyGuiScreenJoinGame screen = new MyGuiScreenJoinGame();
                    screen.Closed += new MyGuiScreenBase.ScreenHandler(this.joinGameScreen_Closed);
                    MyGuiSandbox.AddScreen(screen);
                }
                else
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.SteamIsOfflinePleaseRestart), MySession.Platform), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            });
        }

        private void OnLogoutProgressClosed()
        {
            MySandboxGame.Log.WriteLine("Application closed by user");
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.ReportGameQuit("Exit to Windows");
            }
            MyScreenManager.CloseAllScreensNowExcept(null);
            MySandboxGame.ExitThreadSafe();
        }

        private void OnPlayClicked(MyGuiControlButton obj)
        {
            this.isStartMenu = false;
            this.RecreateControls(false);
        }

        private void OnSaveWorldMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyAsyncSaving.Start(null, null, false);
            }
            else
            {
                base.CanBeHidden = true;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
        }

        private void RecreateBanners()
        {
            this.RemoveBanners();
            this.CreateBanners();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            MyStringId? nullable;
            base.RecreateControls(constructor);
            this.m_elementGroup = new MyGuiControlElementGroup();
            this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_SelectedChanged);
            Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
            Vector2 vector2 = (MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, 0x36, 0x36) + new Vector2(minSizeGui.X / 2f, 0f)) + (new Vector2(15f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            float* singlePtr1 = (float*) ref vector2.Y;
            singlePtr1[0] += 0.043f;
            Vector2 vector1 = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) + new Vector2(-minSizeGui.X / 2f, 0f);
            if (MyGuiScreenGamePlay.Static != null)
            {
                int num2;
                MyGuiControlButton button4;
                MyAnalyticsHelper.ReportActivityStart(null, "show_main_menu", string.Empty, "gui", string.Empty, true);
                base.EnabledBackgroundFade = true;
                nullable = null;
                MyGuiControlButton button2 = base.MakeButton(vector2 - ((num2 = (Sync.MultiplayerActive ? 6 : 5) - 1) * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonSave, new Action<MyGuiControlButton>(this.OnClickSaveWorld), nullable);
                nullable = null;
                MyGuiControlButton button3 = base.MakeButton(vector2 - (--num2 * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.LoadScreenButtonSaveAs, new Action<MyGuiControlButton>(this.OnClickSaveAs), nullable);
                if (!Sync.IsServer || !MySession.Static.Settings.EnableSaving)
                {
                    MyStringId text = !Sync.IsServer ? MyCommonTexts.NotificationClientCannotSave : MyCommonTexts.NotificationSavingDisabled;
                    button2.Enabled = false;
                    button2.ShowTooltipWhenDisabled = true;
                    button2.SetToolTip(text);
                    button3.Enabled = false;
                    button3.ShowTooltipWhenDisabled = true;
                    button3.SetToolTip(text);
                }
                this.Controls.Add(button2);
                this.m_elementGroup.Add(button2);
                this.Controls.Add(button3);
                this.m_elementGroup.Add(button3);
                if (Sync.MultiplayerActive)
                {
                    nullable = null;
                    button4 = base.MakeButton(vector2 - (--num2 * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonPlayers, new Action<MyGuiControlButton>(this.OnClickPlayers), nullable);
                    this.Controls.Add(button4);
                    this.m_elementGroup.Add(button4);
                }
                nullable = null;
                button4 = base.MakeButton(vector2 - (--num2 * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonOptions, new Action<MyGuiControlButton>(this.OnClickOptions), nullable);
                this.Controls.Add(button4);
                this.m_elementGroup.Add(button4);
                nullable = null;
                this.m_exitGameButton = base.MakeButton(vector2 - (--num2 * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonExitToMainMenu, new Action<MyGuiControlButton>(this.OnExitToMainMenuClick), nullable);
                this.Controls.Add(this.m_exitGameButton);
                this.m_elementGroup.Add(this.m_exitGameButton);
            }
            else
            {
                base.EnabledBackgroundFade = false;
                MyGuiControlButton button = null;
                int num = MyPerGameSettings.MultiplayerEnabled ? 7 : 6;
                MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
                if (lastSession == null)
                {
                    num--;
                }
                else
                {
                    string str;
                    num--;
                    nullable = null;
                    button = base.MakeButton((vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)) - (MyGuiConstants.MENU_BUTTONS_POSITION_DELTA / 2f), MyCommonTexts.ScreenMenuButtonContinueGame, new Action<MyGuiControlButton>(this.OnContinueGameClicked), nullable);
                    this.Controls.Add(button);
                    this.m_elementGroup.Add(button);
                    MyToolTips toolTip = new MyToolTips();
                    string thumbnail = this.GetThumbnail(lastSession);
                    if (!lastSession.IsOnline)
                    {
                        str = $"{MyTexts.GetString(MyCommonTexts.ToolTipContinueGame)}{System.Environment.NewLine}{lastSession.GameName}";
                    }
                    else if (lastSession.IsLobby)
                    {
                        str = $"{MyTexts.GetString(MyCommonTexts.ToolTipContinueGame)}{System.Environment.NewLine}{lastSession.GameName} - {lastSession.ServerIP}";
                    }
                    else
                    {
                        str = $"{MyTexts.GetString(MyCommonTexts.ToolTipContinueGame)}{System.Environment.NewLine}{lastSession.GameName} - {lastSession.ServerIP}:{lastSession.ServerPort}";
                    }
                    if (thumbnail == null)
                    {
                        toolTip.AddToolTip(str, 0.7f, "Blue");
                    }
                    else
                    {
                        List<string> texturesToLoad = new List<string>();
                        texturesToLoad.Add(thumbnail);
                        MyRenderProxy.PreloadTextures(texturesToLoad, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
                        toolTip.AddToolTip(str, 0.7f, "Blue");
                        toolTip.RecalculateOnChange = false;
                        MyGuiControlBase base2 = this.CreateImageTooltip(thumbnail, str);
                        base2.Visible = false;
                        this.Controls.Add(base2);
                        toolTip.TooltipControl = base2;
                    }
                    button.SetToolTip(toolTip);
                }
                button = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonCampaign, new Action<MyGuiControlButton>(this.OnClickNewGame), new MyStringId?(MyCommonTexts.ToolTipNewGame));
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                num = (num - 1) - 1;
                button = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonLoadGame, new Action<MyGuiControlButton>(this.OnClickLoad), new MyStringId?(MyCommonTexts.ToolTipLoadGame));
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                if (MyPerGameSettings.MultiplayerEnabled)
                {
                    num--;
                    button = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonJoinGame, new Action<MyGuiControlButton>(this.OnJoinWorld), new MyStringId?(MyCommonTexts.ToolTipJoinGame));
                    this.Controls.Add(button);
                    this.m_elementGroup.Add(button);
                }
                num--;
                button = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonOptions, new Action<MyGuiControlButton>(this.OnClickOptions), new MyStringId?(MyCommonTexts.ToolTipOptions));
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                if (MyFakes.ENABLE_MAIN_MENU_INVENTORY_SCENE)
                {
                    num--;
                    button = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonInventory, new Action<MyGuiControlButton>(this.OnClickInventory), new MyStringId?(MyCommonTexts.ScreenMenuButtonInventoryTooltip));
                    this.Controls.Add(button);
                    this.m_elementGroup.Add(button);
                }
                num--;
                this.m_exitGameButton = base.MakeButton(vector2 - (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), MyCommonTexts.ScreenMenuButtonExitToWindows, new Action<MyGuiControlButton>(this.OnClickExitToWindows), new MyStringId?(MyCommonTexts.ToolTipExitToWindows));
                this.Controls.Add(this.m_exitGameButton);
                this.m_elementGroup.Add(this.m_exitGameButton);
            }
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlPanel control = new MyGuiControlPanel(new Vector2?(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 0x31, 0x52)), new Vector2?(MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui), backgroundColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO
            };
            this.Controls.Add(control);
            if (!MyFakes.SHOW_BANNERS)
            {
                this.RefreshDLC();
                this.RefreshGameLogo();
            }
            this.CreateBanners();
            MyGuiControlNews news1 = new MyGuiControlNews();
            news1.Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36) - (5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA);
            news1.Size = new Vector2(0.4f, 0.28f);
            news1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_newsControl = news1;
            this.Controls.Add(this.m_newsControl);
            base.CheckLowMemSwitchToLow();
            this.DownloadActiveEvents();
        }

        private bool RefreshDLC()
        {
            bool flag = false;
            foreach (KeyValuePair<MyStringHash, MyBannerInfo> pair in this.m_banners)
            {
                MyBannerInfo info = pair.Value;
                if (info.Active)
                {
                    MyBannerStatus status = info.Status;
                    if (MyFakes.SHOW_BANNERS)
                    {
                        info.Status = MyBannerStatus.NotInstalled;
                    }
                    else if (!MyGameService.IsActive || (info.DLCId == null))
                    {
                        info.Status = MyBannerStatus.Offline;
                    }
                    else
                    {
                        info.Status = MyGameService.IsDlcInstalled(info.DLCId.Value) ? MyBannerStatus.Installed : MyBannerStatus.NotInstalled;
                    }
                    flag |= status != info.Status;
                }
            }
            return flag;
        }

        private void RefreshGameLogo()
        {
            foreach (KeyValuePair<MyStringHash, MyBadge> pair in this.m_badges)
            {
                if (!MyGameService.IsActive)
                {
                    pair.Value.Status = MyBannerStatus.Offline;
                    continue;
                }
                if ((pair.Value.DLCId != 0) && MyGameService.IsDlcInstalled(pair.Value.DLCId))
                {
                    pair.Value.Status = MyBannerStatus.Installed;
                    continue;
                }
                if (string.IsNullOrEmpty(pair.Value.AchievementName) || !MyGameService.IsAchieved(pair.Value.AchievementName))
                {
                    pair.Value.Status = MyBannerStatus.NotInstalled;
                }
                else
                {
                    pair.Value.Status = MyBannerStatus.Installed;
                }
            }
        }

        private void RemoveBanners()
        {
            foreach (MyGuiControlBase base2 in this.m_bannerControls)
            {
                if (base2 != null)
                {
                    this.Controls.Remove(base2);
                }
            }
        }

        private void RunWithTutorialCheck(Action afterTutorial)
        {
            if (MySandboxGame.Config.FirstTimeTutorials)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenTutorialsScreen(afterTutorial));
            }
            else
            {
                afterTutorial();
            }
        }

        private void UnloadAndExitAfterScreeshotWasTaken(object sender, System.EventArgs e)
        {
            MySandboxGame.Static.OnScreenshotTaken -= new EventHandler(this.UnloadAndExitAfterScreeshotWasTaken);
            MySessionLoader.UnloadAndExitToMenu();
        }

        public override bool Update(bool hasFocus)
        {
            base.Update(hasFocus);
            this.m_currentDLCcounter += 0x10;
            if (this.m_currentDLCcounter > this.DLC_UPDATE_INTERVAL)
            {
                this.m_currentDLCcounter = 0;
                if (this.RefreshDLC())
                {
                    this.RecreateControls(false);
                }
                this.RefreshGameLogo();
            }
            if ((ReferenceEquals(MyGuiScreenGamePlay.Static, null) & hasFocus) && MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                if (this.isStartMenu)
                {
                    this.OnClickExitToWindows(null);
                }
                else
                {
                    this.OnClickBack(null);
                }
            }
            if (hasFocus && (this.m_lastClickedBanner != null))
            {
                base.FocusedControl = null;
                this.m_lastClickedBanner = null;
            }
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenMainMenu.<>c <>9 = new MyGuiScreenMainMenu.<>c();
            public static Action <>9__37_0;
            public static Action <>9__45_0;
            public static Action <>9__46_0;

            internal void <OnClickLoad>b__46_0()
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox());
            }

            internal void <OnClickNewGame>b__45_0()
            {
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNewGame>(Array.Empty<object>()));
            }

            internal void <OnContinueGameClicked>b__37_0()
            {
                MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
                if (lastSession != null)
                {
                    if (!lastSession.IsOnline)
                    {
                        MySessionLoader.LoadLastSession();
                    }
                    else if (lastSession.IsLobby)
                    {
                        MyJoinGameHelper.JoinGame(ulong.Parse(lastSession.ServerIP));
                    }
                    else
                    {
                        try
                        {
                            ushort serverPort = (ushort) lastSession.ServerPort;
                            MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(MySandboxGame.Static.ServerResponded);
                            MyGameService.OnPingServerFailedToRespond += new EventHandler(MySandboxGame.Static.ServerFailedToRespond);
                            MyGameService.PingServer(Dns.GetHostAddresses(lastSession.ServerIP)[0].ToIPv4NetworkOrder(), serverPort);
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine(exception);
                            MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
                        }
                    }
                }
            }
        }

        private class MyBadge
        {
            public MyGuiScreenMainMenu.MyBannerStatus Status;
            public uint DLCId;
            public string AchievementName;
            public string Texture;
        }

        private class MyBannerInfo
        {
            public bool Active;
            public MyGuiScreenMainMenu.MyBannerStatus Status;
            public string HighlightTexture;
            public string Texture;
            public string URL;
            public uint? DLCId;
            public MyStringId ToolTip;
            public MyStringId Line1;
            public MyStringId Line2;
        }

        private enum MyBannerStatus
        {
            Offline,
            Installed,
            NotInstalled
        }
    }
}


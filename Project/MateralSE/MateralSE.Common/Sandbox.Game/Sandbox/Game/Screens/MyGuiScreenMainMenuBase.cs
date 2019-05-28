namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Gui.DebugInputComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenMainMenuBase : MyGuiScreenBase
    {
        protected const float TEXT_LINE_HEIGHT = 0.014f;
        protected const int INITIAL_TRANSITION_TIME = 0x5dc;
        protected bool m_pauseGame;
        protected bool m_musicPlayed;
        private static bool m_firstLoadup = true;
        private List<MyStringId> m_warningNotifications;
        private static readonly StringBuilder BUILD_DATE = new StringBuilder("Build: " + MySandboxGame.BuildDateTime.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture));
        private static readonly StringBuilder APP_VERSION = MyFinalBuildConstants.APP_VERSION_STRING;
        private static readonly StringBuilder STEAM_INACTIVE = new StringBuilder("STEAM NOT AVAILABLE");
        private static readonly StringBuilder NOT_OBFUSCATED = new StringBuilder("NOT OBFUSCATED");
        private static readonly StringBuilder NON_OFFICIAL = new StringBuilder(" NON-OFFICIAL");
        private static readonly StringBuilder PROFILING = new StringBuilder(" PROFILING");
        private static readonly StringBuilder PLATFORM = new StringBuilder(System.Environment.Is64BitProcess ? " 64-bit" : " 32-bit");
        private static StringBuilder BranchName = new StringBuilder(50);

        public MyGuiScreenMainMenuBase(bool pauseGame = false) : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            this.m_warningNotifications = new List<MyStringId>();
            if (!MyScreenManager.IsScreenOfTypeOpen(typeof(MyGuiScreenGamePlay)))
            {
                base.m_closeOnEsc = false;
            }
            else
            {
                this.m_pauseGame = pauseGame;
                if (this.m_pauseGame && !Sync.MultiplayerActive)
                {
                    MySandboxGame.PausePush();
                }
            }
            base.m_drawEvenWithoutFocus = false;
            this.DrawBuildInformation = true;
        }

        protected void CheckLowMemSwitchToLow()
        {
            if (MySandboxGame.Config.LowMemSwitchToLow == MyConfig.LowMemSwitch.TRIGGERED)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.LowMemSwitchToLowQuestion), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result != MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.USER_SAID_NO;
                        MySandboxGame.Config.Save();
                    }
                    else
                    {
                        MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.ARMED;
                        MySandboxGame.Config.SetToLowQuality();
                        MySandboxGame.Config.Save();
                        if (MySpaceAnalytics.Instance != null)
                        {
                            MySpaceAnalytics.Instance.ReportGameQuit("Exit to Windows");
                        }
                        MyScreenManager.CloseAllScreensNowExcept(null);
                        MySandboxGame.ExitThreadSafe();
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public override bool CloseScreen()
        {
            if (this.m_pauseGame && !Sync.MultiplayerActive)
            {
                MySandboxGame.PausePop();
            }
            m_firstLoadup = false;
            this.m_musicPlayed = false;
            MyAnalyticsHelper.ReportActivityEnd(null, "show_main_menu");
            return base.CloseScreen();
        }

        public override void CloseScreenNow()
        {
            m_firstLoadup = false;
            base.CloseScreenNow();
        }

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            if ((MySandboxGame.Config.EnablePerformanceWarnings && MySandboxGame.Config.ExperimentalMode) && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode))
            {
                this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode);
            }
            MyGuiSandbox.DrawGameLogoHandler(base.m_transitionAlpha, MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 0x2c, 0x44));
            this.DrawPerformanceWarning();
            if (this.DrawBuildInformation)
            {
                this.DrawObfuscationStatus();
                this.DrawSteamStatus();
                this.DrawAppVersion();
            }
            return true;
        }

        private unsafe void DrawAppVersion()
        {
            Vector2 zero = Vector2.Zero;
            Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 8, 8);
            if (!string.IsNullOrEmpty(MyGameService.BranchName))
            {
                BranchName.Clear();
                BranchName.Append(" ");
                BranchName.Append(MyGameService.BranchName);
                zero = MyGuiManager.MeasureString("BuildInfoHighlight", BranchName, 0.6f);
                MyGuiManager.DrawString("BuildInfoHighlight", BranchName, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * base.m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
                float* singlePtr1 = (float*) ref normalizedCoord.X;
                singlePtr1[0] -= zero.X;
            }
            MyGuiManager.DrawString("BuildInfo", MyFinalBuildConstants.APP_VERSION_STRING_DOTS, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * base.m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
        }

        private unsafe void DrawBuildDate()
        {
            Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36);
            float* singlePtr1 = (float*) ref normalizedCoord.Y;
            singlePtr1[0] -= 0f;
            MyGuiManager.DrawString("BuildInfo", BUILD_DATE, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * base.m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
        }

        private unsafe void DrawObfuscationStatus()
        {
            if (MyPerGameSettings.ShowObfuscationStatus && !MyObfuscation.Enabled)
            {
                Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36);
                float* singlePtr1 = (float*) ref normalizedCoord.Y;
                singlePtr1[0] -= 0.042f;
                MyGuiManager.DrawString("BuildInfoHighlight", NOT_OBFUSCATED, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * base.m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
            }
        }

        private void DrawPerformanceWarning()
        {
            if (this.m_warningNotifications.Count != 0)
            {
                Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 4, 0x2a) - new Vector2(MyGuiConstants.TEXTURE_HUD_BG_PERFORMANCE.SizeGui.X / 1.5f, 0f);
                MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_HUD_BG_PERFORMANCE;
                MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, texture.SizeGui / 1.5f, Color.White, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, true);
                Color? colorMask = null;
                MyGuiManager.DrawString("White", new StringBuilder(MyTexts.GetString(this.m_warningNotifications[0])), normalizedCoord + new Vector2(0.09f, -0.011f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                StringBuilder stringBuilder = new StringBuilder();
                colorMask = null;
                MyGuiManager.DrawString("White", stringBuilder.AppendFormat(MyCommonTexts.PerformanceWarningCombination, MyGuiSandbox.GetKeyName(MyControlsSpace.HELP_SCREEN)), normalizedCoord + new Vector2(0.09f, 0.011f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                stringBuilder.Clear();
                colorMask = null;
                MyGuiManager.DrawString("White", stringBuilder.AppendFormat("({0})", this.m_warningNotifications.Count), normalizedCoord + new Vector2(0.177f, -0.023f), 0.55f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                this.m_warningNotifications.RemoveAt(0);
            }
        }

        private unsafe void DrawSteamStatus()
        {
            if (!MyGameService.IsActive)
            {
                Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 0x36, 0x36);
                float* singlePtr1 = (float*) ref normalizedCoord.Y;
                singlePtr1[0] -= 0.028f;
                MyGuiManager.DrawString("BuildInfo", STEAM_INACTIVE, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * base.m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMainMenu";

        public override int GetTransitionOpeningTime() => 
            (!m_firstLoadup ? base.GetTransitionOpeningTime() : 0x5dc);

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELP_SCREEN))
            {
                if (!MyInput.Static.IsAnyShiftKeyPressed() || (MyPerGameSettings.GUI.PerformanceWarningScreen == null))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HelpScreen, Array.Empty<object>()));
                }
                else
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PerformanceWarningScreen, Array.Empty<object>()));
                }
            }
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply) && MyInput.Static.IsAnyShiftKeyPressed())
                {
                    GC.Collect();
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.M))
                {
                    this.RecreateControls(false);
                }
            }
        }

        public override bool HideScreen()
        {
            m_firstLoadup = false;
            return base.HideScreen();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            this.RecreateControls(true);
        }

        protected MyGuiControlButton MakeButton(Vector2 position, MyStringId text, Action<MyGuiControlButton> onClick, MyStringId? tooltip = new MyStringId?())
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.StripeLeft, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            if (tooltip != null)
            {
                button.SetToolTip(MyTexts.GetString(tooltip.Value));
            }
            return button;
        }

        public override bool RegisterClicks() => 
            true;

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if (!this.m_musicPlayed)
            {
                if (MyGuiScreenGamePlay.Static == null)
                {
                    MyAudio.Static.PlayMusic(MyPerGameSettings.MainMenuTrack, 0);
                }
                this.m_musicPlayed = true;
            }
            if (MyReloadTestComponent.Enabled && (base.State == MyGuiScreenState.OPENED))
            {
                MyReloadTestComponent.DoReload();
            }
            return true;
        }

        public bool DrawBuildInformation { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenMainMenuBase.<>c <>9 = new MyGuiScreenMainMenuBase.<>c();
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__35_0;

            internal void <CheckLowMemSwitchToLow>b__35_0(MyGuiScreenMessageBox.ResultEnum result)
            {
                if (result != MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.USER_SAID_NO;
                    MySandboxGame.Config.Save();
                }
                else
                {
                    MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.ARMED;
                    MySandboxGame.Config.SetToLowQuality();
                    MySandboxGame.Config.Save();
                    if (MySpaceAnalytics.Instance != null)
                    {
                        MySpaceAnalytics.Instance.ReportGameQuit("Exit to Windows");
                    }
                    MyScreenManager.CloseAllScreensNowExcept(null);
                    MySandboxGame.ExitThreadSafe();
                }
            }
        }
    }
}


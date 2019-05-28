namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Ansel;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.Plugins;
    using VRage.Utils;
    using VRageMath;

    public static class MyGuiSandbox
    {
        public static Regex urlRgx = new Regex(@"^(http|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?$");
        internal static IMyGuiSandbox Gui = new MyNullGui();
        private static Dictionary<System.Type, System.Type> m_createdScreenTypes = new Dictionary<System.Type, System.Type>();
        public static int TotalGamePlayTimeInMilliseconds;
        public static Action<object> GuiControlCreated;
        public static Action<object> GuiControlRemoved;
        private static Regex[] WWW_WHITELIST = new Regex[] { new Regex("^(http[s]{0,1}://){0,1}[^/]*youtube.com/.*", RegexOptions.IgnoreCase), new Regex("^(http[s]{0,1}://){0,1}[^/]*youtu.be/.*", RegexOptions.IgnoreCase), new Regex("^(http[s]{0,1}://){0,1}[^/]*steamcommunity.com/.*", RegexOptions.IgnoreCase), new Regex("^(http[s]{0,1}://){0,1}[^/]*forum[s]{0,1}.keenswh.com/.*", RegexOptions.IgnoreCase) };

        public static void AddScreen(MyGuiScreenBase screen)
        {
            Gui.AddScreen(screen);
            if (GuiControlCreated != null)
            {
                GuiControlCreated(screen);
            }
            screen.Closed += delegate (MyGuiScreenBase x) {
                if (GuiControlRemoved != null)
                {
                    GuiControlRemoved(x);
                }
            };
            if (MyAPIGateway.GuiControlCreated != null)
            {
                MyAPIGateway.GuiControlCreated(screen);
            }
        }

        public static bool Ansel_IsSpectatorEnabled() => 
            MyGuiScreenGamePlay.SpectatorEnabled;

        public static void Ansel_WarningMessage(bool pauseAllowed, bool spectatorEnabled)
        {
            if (!pauseAllowed || !spectatorEnabled)
            {
                StringBuilder messageText = new StringBuilder();
                if (!pauseAllowed)
                {
                    messageText.Append(MyTexts.Get(MyCommonTexts.MessageBoxTextAnselCannotPauseOnlineGame));
                    messageText.AppendLine("");
                }
                if (!spectatorEnabled)
                {
                    messageText.Append(MyTexts.Get(MyCommonTexts.MessageBoxTextAnselSpectatorDisabled));
                    messageText.AppendLine("");
                }
                messageText.Append(MyTexts.Get(MyCommonTexts.MessageBoxTextAnselTimeout));
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                AddScreen(CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.NONE_TIMEOUT, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, null, 0xfa0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public static void BackToIntroLogos(Action afterLogosAction)
        {
            Gui.BackToIntroLogos(afterLogosAction);
        }

        public static void BackToMainMenu()
        {
            Gui.BackToMainMenu();
        }

        private static void ChooseScreenType<T>(ref System.Type createdType, Assembly[] assemblies) where T: MyGuiScreenBase
        {
            if (assemblies != null)
            {
                foreach (Assembly assembly in assemblies)
                {
                    ChooseScreenType<T>(ref createdType, assembly);
                }
            }
        }

        private static void ChooseScreenType<T>(ref System.Type createdType, Assembly assembly) where T: MyGuiScreenBase
        {
            if (assembly != null)
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    if (typeof(T).IsAssignableFrom(type))
                    {
                        createdType = type;
                        return;
                    }
                }
            }
        }

        public static MyGuiScreenMessageBox CreateMessageBox(MyMessageBoxStyleEnum styleEnum = 0, MyMessageBoxButtonsType buttonType = 1, StringBuilder messageText = null, StringBuilder messageCaption = null, MyStringId? okButtonText = new MyStringId?(), MyStringId? cancelButtonText = new MyStringId?(), MyStringId? yesButtonText = new MyStringId?(), MyStringId? noButtonText = new MyStringId?(), Action<MyGuiScreenMessageBox.ResultEnum> callback = null, int timeoutInMiliseconds = 0, MyGuiScreenMessageBox.ResultEnum focusedResult = 0, bool canHideOthers = true, Vector2? size = new Vector2?())
        {
            MyStringId? nullable = okButtonText;
            nullable = cancelButtonText;
            nullable = yesButtonText;
            nullable = noButtonText;
            return new MyGuiScreenMessageBox(styleEnum, buttonType, messageText, messageCaption, (nullable != null) ? nullable.GetValueOrDefault() : MyCommonTexts.Ok, (nullable != null) ? nullable.GetValueOrDefault() : MyCommonTexts.Cancel, (nullable != null) ? nullable.GetValueOrDefault() : MyCommonTexts.Yes, (nullable != null) ? nullable.GetValueOrDefault() : MyCommonTexts.No, callback, timeoutInMiliseconds, focusedResult, canHideOthers, size, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity);
        }

        public static T CreateScreen<T>(params object[] args) where T: MyGuiScreenBase
        {
            System.Type type = null;
            if (!m_createdScreenTypes.TryGetValue(typeof(T), out type))
            {
                System.Type type2 = typeof(T);
                type = type2;
                ChooseScreenType<T>(ref type, MyPlugins.GameAssembly);
                ChooseScreenType<T>(ref type, MyPlugins.SandboxAssembly);
                ChooseScreenType<T>(ref type, MyPlugins.UserAssemblies);
                m_createdScreenTypes[type2] = type;
            }
            return (Activator.CreateInstance(type, args) as T);
        }

        public static MyGuiScreenBase CreateScreen(System.Type screenType, params object[] args) => 
            (Activator.CreateInstance(screenType, args) as MyGuiScreenBase);

        public static void Draw()
        {
            Gui.Draw();
        }

        public static void DrawBadge(string texture, float transitionAlpha, Vector2 position, Vector2 size)
        {
            Gui.DrawBadge(texture, transitionAlpha, position, size);
        }

        public static void DrawGameLogo(float transitionAlpha, Vector2 position)
        {
            Gui.DrawGameLogo(transitionAlpha, position);
        }

        public static float GetDefaultTextScaleWithLanguage() => 
            Gui.GetDefaultTextScaleWithLanguage();

        public static string GetKeyName(MyStringId control)
        {
            MyControl gameControl = MyInput.Static.GetGameControl(control);
            return ((gameControl == null) ? "" : gameControl.GetControlButtonName(MyGuiInputDeviceEnum.Keyboard));
        }

        public static void HandleInput()
        {
            Gui.HandleInput();
        }

        public static void HandleInputAfterSimulation()
        {
            Gui.HandleInputAfterSimulation();
        }

        public static void InsertScreen(MyGuiScreenBase screen, int index)
        {
            Gui.InsertScreen(screen, index);
            if (GuiControlCreated != null)
            {
                GuiControlCreated(screen);
            }
            screen.Closed += delegate (MyGuiScreenBase x) {
                if (GuiControlRemoved != null)
                {
                    GuiControlRemoved(x);
                }
            };
            if (MyAPIGateway.GuiControlCreated != null)
            {
                MyAPIGateway.GuiControlCreated(screen);
            }
        }

        public static bool IsDebugScreenEnabled() => 
            Gui.IsDebugScreenEnabled();

        public static bool IsUrlValid(string url) => 
            urlRgx.IsMatch(url);

        public static bool IsUrlWhitelisted(string wwwLink)
        {
            Regex[] regexArray = WWW_WHITELIST;
            for (int i = 0; i < regexArray.Length; i++)
            {
                if (regexArray[i].IsMatch(wwwLink))
                {
                    return true;
                }
            }
            return false;
        }

        public static void LoadContent()
        {
            Gui.LoadContent();
        }

        public static void LoadData(bool nullGui)
        {
            MyAnsel.WarningMessageDelegate = (MyAnsel.DelegatePauseNotAllowedWarning) Delegate.Combine(MyAnsel.WarningMessageDelegate, new MyAnsel.DelegatePauseNotAllowedWarning(MyGuiSandbox.Ansel_WarningMessage));
            MyAnsel.IsSpectatorEnabledDelegate = (MyAnsel.DelegateIsSpectatorEnabled) Delegate.Combine(MyAnsel.IsSpectatorEnabledDelegate, new MyAnsel.DelegateIsSpectatorEnabled(MyGuiSandbox.Ansel_IsSpectatorEnabled));
            if (!nullGui)
            {
                Gui = new MyDX9Gui();
            }
            Gui.LoadData();
        }

        public static void OpenExternalBrowser(string url)
        {
            if (!MyBrowserHelper.OpenInternetBrowser(url))
            {
                StringBuilder messageText = MyTexts.Get(MyCommonTexts.TitleFailedToStartInternetBrowser);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                AddScreen(CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, messageText, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public static void OpenUrl(string url, UrlOpenMode openMode, StringBuilder confirmMessage = null)
        {
            bool flag = (openMode & UrlOpenMode.ExternalBrowser) != 0;
            bool flag2 = (openMode & UrlOpenMode.ConfirmExternal) != 0;
            bool flag3 = ((openMode & UrlOpenMode.SteamOverlay) != 0) && Gui.OpenSteamOverlay(url);
            if (MyFakes.XBOX_PREVIEW)
            {
                Show(MyCommonTexts.MessageBoxTextErrorFeatureNotAvailableYet, MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
            else if (!flag3 & flag)
            {
                if (flag2)
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    AddScreen(CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, confirmMessage ?? new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextOpenBrowser, url), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                        if (retval == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            OpenExternalBrowser(url);
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                else
                {
                    OpenExternalBrowser(url);
                }
            }
        }

        public static void OpenUrlWithFallback(string url, string urlFriendlyName, bool useWhitelist = false)
        {
            if (useWhitelist && !IsUrlWhitelisted(url))
            {
                MySandboxGame.Log.WriteLine("URL NOT ALLOWED: " + url);
            }
            else
            {
                StringBuilder confirmMessage = new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextOpenUrlOverlayNotEnabled), urlFriendlyName, MySession.Platform);
                OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, confirmMessage);
            }
        }

        public static void RemoveScreen(MyGuiScreenBase screen)
        {
            Gui.RemoveScreen(screen);
            if (GuiControlRemoved != null)
            {
                GuiControlRemoved(screen);
            }
        }

        public static void SetMouseCursorVisibility(bool visible, bool changePosition = true)
        {
            Gui.SetMouseCursorVisibility(visible, changePosition);
        }

        public static void Show(StringBuilder text, MyStringId caption = new MyStringId(), MyMessageBoxStyleEnum type = 0)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            AddScreen(CreateMessageBox(type, MyMessageBoxButtonsType.OK, text, MyTexts.Get(caption), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void Show(MyStringId text, MyStringId caption = new MyStringId(), MyMessageBoxStyleEnum type = 0)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            AddScreen(CreateMessageBox(type, MyMessageBoxButtonsType.OK, MyTexts.Get(text), MyTexts.Get(caption), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void ShowModErrors()
        {
            Gui.ShowModErrors();
        }

        public static void SwitchDebugScreensEnabled()
        {
            Gui.SwitchDebugScreensEnabled();
        }

        public static void TakeScreenshot(int width, int height, string saveToPath = null, bool ignoreSprites = false, bool showNotification = true)
        {
            Gui.TakeScreenshot(width, height, saveToPath, ignoreSprites, showNotification);
        }

        public static void UnloadContent()
        {
            Gui.UnloadContent();
        }

        public static void Update(int totalTimeInMS)
        {
            Gui.Update(totalTimeInMS);
        }

        public static Vector2 MouseCursorPosition =>
            Gui.MouseCursorPosition;

        public static Action<float, Vector2> DrawGameLogoHandler
        {
            get => 
                Gui.DrawGameLogoHandler;
            set => 
                (Gui.DrawGameLogoHandler = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiSandbox.<>c <>9 = new MyGuiSandbox.<>c();
            public static MyGuiScreenBase.ScreenHandler <>9__30_0;
            public static MyGuiScreenBase.ScreenHandler <>9__31_0;

            internal void <AddScreen>b__30_0(MyGuiScreenBase x)
            {
                if (MyGuiSandbox.GuiControlRemoved != null)
                {
                    MyGuiSandbox.GuiControlRemoved(x);
                }
            }

            internal void <InsertScreen>b__31_0(MyGuiScreenBase x)
            {
                if (MyGuiSandbox.GuiControlRemoved != null)
                {
                    MyGuiSandbox.GuiControlRemoved(x);
                }
            }
        }
    }
}


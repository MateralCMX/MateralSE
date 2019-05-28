namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.AppCode;
    using Sandbox.Common;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.GUI.DebugInputComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Input;
    using VRage.Serialization;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;

    public class MyDX9Gui : IMyGuiSandbox
    {
        public static int TotalGamePlayTimeInMilliseconds;
        private static MyGuiScreenDebugBase m_currentDebugScreen;
        private MyGuiScreenMessageBox m_currentModErrorsMessageBox;
        private MyGuiScreenDebugBase m_currentStatisticsScreen;
        private bool m_debugScreensEnabled = true;
        private StringBuilder m_debugText = new StringBuilder();
        public string GameLogoTexture = @"Textures\GUI\GameLogoLarge.dds";
        private Vector2 m_gameLogoSize = (new Vector2(0.43875f, 0.1975f) * 0.8f);
        internal List<MyDebugComponent> UserDebugInputComponents = new List<MyDebugComponent>();
        private Vector2 m_oldVisPos;
        private Vector2 m_oldNonVisPos;
        private bool m_oldMouseVisibilityState;
        private bool m_wasInputToNonFocusedScreens;
        private StringBuilder m_inputSharingText;
        private StringBuilder m_renderOverloadedText = new StringBuilder("WARNING: Render is overloaded, optimize your scene!");
        private bool m_shapeRenderingMessageBoxShown;
        private List<System.Type> m_pausingScreenTypes;
        private bool m_cameraControllerMovementAllowed;
        private static bool m_lookAroundEnabled;

        public MyDX9Gui()
        {
            MySandboxGame.Log.WriteLine("MyGuiManager()");
            this.DrawGameLogoHandler = new Action<float, Vector2>(this.DrawGameLogo);
            this.m_inputSharingText = !MyFakes.ALT_AS_DEBUG_KEY ? new StringBuilder("WARNING: Sharing input enabled (release Scroll Lock to disable it)") : new StringBuilder("WARNING: Sharing input enabled (release ALT to disable it)");
            MyGuiScreenBase.EnableSlowTransitionAnimations = MyFakes.ENABLE_SLOW_WINDOW_TRANSITION_ANIMATIONS;
            this.UserDebugInputComponents.Add(new MyGlobalInputComponent());
            this.UserDebugInputComponents.Add(new MyCharacterInputComponent());
            this.UserDebugInputComponents.Add(new MyOndraInputComponent());
            this.UserDebugInputComponents.Add(new MyPetaInputComponent());
            this.UserDebugInputComponents.Add(new MyMartinInputComponent());
            this.UserDebugInputComponents.Add(new MyTomasInputComponent());
            this.UserDebugInputComponents.Add(new MyTestersInputComponent());
            this.UserDebugInputComponents.Add(new MyHonzaInputComponent());
            this.UserDebugInputComponents.Add(new MyCestmirDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyAlexDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyMichalDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyAsteroidsDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyRendererStatsComponent());
            this.UserDebugInputComponents.Add(new MyPlanetsDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyRenderDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyComponentsDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyVRDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyResearchDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyVisualScriptingDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyAIDebugInputComponent());
            this.UserDebugInputComponents.Add(new MyAlesDebugInputComponent());
            this.LoadDebugInputsFromConfig();
        }

        private void AddCloseHandler(MyGuiScreenBase previousScreen, MyGuiScreenBase logoScreen, Action afterLogosAction)
        {
            previousScreen.Closed += delegate (MyGuiScreenBase screen) {
                if (!screen.Cancelled)
                {
                    this.AddScreen(logoScreen);
                }
                else
                {
                    afterLogosAction();
                }
            };
        }

        public void AddScreen(MyGuiScreenBase screen)
        {
            MyScreenManager.AddScreen(screen);
        }

        public void BackToIntroLogos(Action afterLogosAction)
        {
            MyScreenManager.CloseAllScreensNowExcept(null);
            LogoItem item = new LogoItem {
                Screen = typeof(MyGuiScreenIntroVideo)
            };
            item.Args = new string[] { @"Videos\KSH.wmv" };
            LogoItem[] itemArray1 = new LogoItem[3];
            itemArray1[0] = item;
            item = new LogoItem {
                Screen = typeof(MyGuiScreenLogo)
            };
            item.Args = new string[] { @"Textures\Logo\vrage_logo_2_0_white.dds" };
            itemArray1[1] = item;
            item = new LogoItem {
                Screen = typeof(MyGuiScreenLogo)
            };
            item.Args = new string[] { @"Textures\Logo\se.dds" };
            itemArray1[2] = item;
            MyGuiScreenBase previousScreen = null;
            LogoItem[] itemArray = itemArray1;
            int index = 0;
            while (index < itemArray.Length)
            {
                LogoItem item2 = itemArray[index];
                List<string> list = new List<string>();
                string[] args = item2.Args;
                int num2 = 0;
                while (true)
                {
                    if (num2 >= args.Length)
                    {
                        if (list.Count != 0)
                        {
                            object[] objArray1 = new object[] { list.ToArray() };
                            MyGuiScreenBase logoScreen = (MyGuiScreenBase) Activator.CreateInstance(item2.Screen, objArray1);
                            if (previousScreen != null)
                            {
                                this.AddCloseHandler(previousScreen, logoScreen, afterLogosAction);
                            }
                            else
                            {
                                this.AddScreen(logoScreen);
                            }
                            previousScreen = logoScreen;
                        }
                        index++;
                        break;
                    }
                    string str = args[num2];
                    if (MyFileSystem.FileExists(Path.Combine(MyFileSystem.ContentPath, str)))
                    {
                        list.Add(str);
                    }
                    num2++;
                }
            }
            if (previousScreen != null)
            {
                previousScreen.Closed += screen => afterLogosAction();
            }
            else
            {
                afterLogosAction();
            }
        }

        public void BackToMainMenu()
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.MainMenu, Array.Empty<object>()));
        }

        public void Draw()
        {
            MyScreenManager.Draw();
            this.m_debugText.Clear();
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS && (MySandboxGame.Config.DebugComponentsInfo != MyDebugComponent.MyDebugComponentInfoState.NoInfo))
            {
                float y = 0f;
                int num2 = 0;
                bool flag2 = false;
                MyDebugComponent.ResetFrame();
                foreach (MyDebugComponent component in this.UserDebugInputComponents)
                {
                    if (component.Enabled)
                    {
                        if (y == 0f)
                        {
                            this.m_debugText.AppendLine("Debug input:");
                            this.m_debugText.AppendLine();
                            y += 0.063f;
                        }
                        this.m_debugText.ConcatFormat<string, int>("{0} (Ctrl + numPad{1})", this.UserDebugInputComponents[num2].GetName(), num2, null);
                        this.m_debugText.AppendLine();
                        y += 0.0265f;
                        if (MySession.Static != null)
                        {
                            component.DispatchUpdate();
                        }
                        component.Draw();
                        flag2 = true;
                    }
                    num2++;
                }
                if (flag2)
                {
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Controls\rectangle_dark_center.dds", new Vector2(MyGuiManager.GetMaxMouseCoord().X, 0f), new Vector2(MyGuiManager.MeasureString("White", this.m_debugText, 1f).X + 0.012f, y), new Color(0, 0, 0, 130), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false, true);
                    MyGuiManager.DrawString("White", this.m_debugText, new Vector2(MyGuiManager.GetMaxMouseCoord().X - 0.01f, 0f), 1f, new Color?(Color.White), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                }
            }
            bool visible = MyVideoSettingsManager.IsHardwareCursorUsed();
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (((screenWithFocus == null) || !screenWithFocus.GetDrawMouseCursor()) && (!MyScreenManager.InputToNonFocusedScreens || (MyScreenManager.GetScreensCount() <= 1)))
            {
                if (visible && (screenWithFocus != null))
                {
                    this.SetMouseCursorVisibility(screenWithFocus.GetDrawMouseCursor(), true);
                }
            }
            else
            {
                this.SetMouseCursorVisibility(visible, false);
                if (!visible || MyFakes.FORCE_SOFTWARE_MOUSE_DRAW)
                {
                    this.DrawMouseCursor(this.GetMouseOverTexture(screenWithFocus));
                }
            }
        }

        public void DrawBadge(string texture, float transitionAlpha, Vector2 position, Vector2 size)
        {
            Color color = Color.White * transitionAlpha;
            MyGuiManager.DrawSpriteBatch(texture, position, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
        }

        public void DrawGameLogo(float transitionAlpha, Vector2 position)
        {
            Color color = Color.White * transitionAlpha;
            MyGuiManager.DrawSpriteBatch(this.GameLogoTexture, position, new Vector2(0.43875f, 0.1975f), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
        }

        private void DrawMouseCursor(string mouseCursorTexture)
        {
            if (mouseCursorTexture != null)
            {
                MyGuiManager.GetNormalizedSize(new Vector2(64f), 1f);
                Vector2? originNormalized = null;
                MyGuiManager.DrawSpriteBatch(mouseCursorTexture, this.MouseCursorPosition, 1f, new Color(MyGuiConstants.MOUSE_CURSOR_COLOR), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0f, originNormalized, true);
            }
        }

        private void EnableSoundsBasedOnWindowFocus()
        {
            if ((MySandboxGame.Static.WindowHandle != GetForegroundWindow()) || (MyScreenManager.GetScreenWithFocus() == null))
            {
                MyAudio.Static.Mute = true;
            }
            else
            {
                MyAudio.Static.Mute = false;
            }
        }

        private void F12Handling()
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F12))
            {
                if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
                {
                    this.ShowDeveloperDebugScreen();
                }
                else if (m_currentDebugScreen is MyGuiScreenDebugDeveloper)
                {
                    this.RemoveScreen(m_currentDebugScreen);
                    m_currentDebugScreen = null;
                }
            }
            MyScreenManager.InputToNonFocusedScreens = !MyFakes.ALT_AS_DEBUG_KEY ? (MyInput.Static.IsKeyPress(MyKeys.ScrollLock) && !MyInput.Static.IsKeyPress(MyKeys.Tab)) : (MyInput.Static.IsAnyAltKeyPressed() && !MyInput.Static.IsKeyPress(MyKeys.Tab));
            if (MyScreenManager.InputToNonFocusedScreens != this.m_wasInputToNonFocusedScreens)
            {
                if (MyScreenManager.InputToNonFocusedScreens && (m_currentDebugScreen != null))
                {
                    this.SetMouseCursorVisibility(MyScreenManager.InputToNonFocusedScreens, true);
                }
                this.m_wasInputToNonFocusedScreens = MyScreenManager.InputToNonFocusedScreens;
            }
        }

        public float GetDefaultTextScaleWithLanguage() => 
            (0.8f * MyGuiManager.LanguageTextScale);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        private string GetMouseOverTexture(MyGuiScreenBase screen)
        {
            if (screen != null)
            {
                MyGuiControlBase mouseOverControl = screen.GetMouseOverControl();
                if (mouseOverControl != null)
                {
                    string mouseCursorTexture = mouseOverControl.GetMouseCursorTexture();
                    return (mouseCursorTexture ?? MyGuiManager.GetMouseCursorTexture());
                }
            }
            return MyGuiManager.GetMouseCursorTexture();
        }

        public Vector2 GetNormalizedCoordsAndPreserveOriginalSize(int width, int height) => 
            new Vector2(((float) width) / ((float) MySandboxGame.ScreenSize.X), ((float) height) / ((float) MySandboxGame.ScreenSize.Y));

        private bool HandleDebugInput()
        {
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                int num = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < 10)
                    {
                        if (!MyInput.Static.IsNewKeyPressed((MyKeys) ((byte) (0x60 + num2))))
                        {
                            num2++;
                            continue;
                        }
                        num = num2;
                        if (MyInput.Static.IsAnyAltKeyPressed())
                        {
                            num += 10;
                        }
                    }
                    if ((num <= -1) || (num >= this.UserDebugInputComponents.Count))
                    {
                        break;
                    }
                    MyDebugComponent local1 = this.UserDebugInputComponents[num];
                    local1.Enabled = !local1.Enabled;
                    this.SaveDebugInputsToConfig();
                    return false;
                }
            }
            bool flag = false;
            foreach (MyDebugComponent component in this.UserDebugInputComponents)
            {
                if (component.Enabled && !MyInput.Static.IsAnyAltKeyPressed())
                {
                    flag = component.HandleInput() | flag;
                }
                if (flag)
                {
                    break;
                }
            }
            return flag;
        }

        public void HandleInput()
        {
            try
            {
                if (!MySandboxGame.Static.PauseInput)
                {
                    if (MyInput.Static.IsAnyAltKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.F4))
                    {
                        if (MySpaceAnalytics.Instance != null)
                        {
                            if (MySession.Static != null)
                            {
                                MySpaceAnalytics.Instance.ReportGameplayEnd();
                            }
                            MySpaceAnalytics.Instance.ReportGameQuit("Alt+F4");
                        }
                        MySandboxGame.ExitThreadSafe();
                    }
                    else
                    {
                        Vector2? nullable;
                        if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SCREENSHOT))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                            nullable = null;
                            this.TakeScreenshot(null, false, nullable, true);
                        }
                        bool flag = MyInput.Static.IsNewKeyPressed(MyKeys.F12);
                        if (((MyInput.Static.IsNewKeyPressed(MyKeys.F2) | flag) && MyInput.Static.IsAnyShiftKeyPressed()) && MyInput.Static.IsAnyAltKeyPressed())
                        {
                            StringBuilder builder;
                            MyStringId? nullable2;
                            if ((MySession.Static == null) || !MySession.Static.CreativeMode)
                            {
                                builder = new StringBuilder("MODDING HELPER KEYS");
                                nullable2 = null;
                                nullable2 = null;
                                nullable2 = null;
                                nullable2 = null;
                                nullable = null;
                                this.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Use of helper key combinations for modders is only allowed in creative mode."), builder, nullable2, nullable2, nullable2, nullable2, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable));
                            }
                            else if (flag)
                            {
                                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = !MyDebugDrawSettings.DEBUG_DRAW_PHYSICS;
                                if (!this.m_shapeRenderingMessageBoxShown)
                                {
                                    this.m_shapeRenderingMessageBoxShown = true;
                                    builder = new StringBuilder("PHYSICS SHAPES");
                                    nullable2 = null;
                                    nullable2 = null;
                                    nullable2 = null;
                                    nullable2 = null;
                                    nullable = null;
                                    this.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Enabled physics shapes rendering. This feature is for modders only and is not part of the gameplay."), builder, nullable2, nullable2, nullable2, nullable2, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable));
                                }
                            }
                        }
                        else
                        {
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.H) && MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                MyGeneralStats.ToggleProfiler();
                            }
                            if ((MyInput.Static.IsNewKeyPressed(MyKeys.F11) && MyInput.Static.IsAnyShiftKeyPressed()) && !MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.SwitchTimingScreen();
                            }
                            if (MyFakes.ENABLE_MISSION_SCREEN && MyInput.Static.IsNewKeyPressed(MyKeys.U))
                            {
                                nullable = null;
                                nullable = null;
                                MyScreenManager.AddScreen(new MyGuiScreenMission(null, null, null, null, null, null, nullable, nullable, false, true, false, MyMissionScreenStyleEnum.BLUE));
                            }
                            if ((!MyInput.Static.ENABLE_DEVELOPER_KEYS && Sync.MultiplayerActive) && (m_currentDebugScreen is MyGuiScreenDebugOfficial))
                            {
                                this.RemoveScreen(m_currentDebugScreen);
                                m_currentDebugScreen = null;
                            }
                            bool flag2 = false;
                            if (((MySession.Static != null) && MySession.Static.CreativeMode) || MyInput.Static.ENABLE_DEVELOPER_KEYS)
                            {
                                this.F12Handling();
                            }
                            if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
                            {
                                if ((MyInput.Static.IsNewKeyPressed(MyKeys.F11) && !MyInput.Static.IsAnyShiftKeyPressed()) && MyInput.Static.IsAnyCtrlKeyPressed())
                                {
                                    this.SwitchStatisticsScreen();
                                }
                                if ((MyInput.Static.IsAnyShiftKeyPressed() && (MyInput.Static.IsAnyAltKeyPressed() && MyInput.Static.IsAnyCtrlKeyPressed())) && MyInput.Static.IsNewKeyPressed(MyKeys.Home))
                                {
                                    throw new InvalidOperationException("Controlled crash");
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Pause) && MyInput.Static.IsAnyShiftKeyPressed())
                                {
                                    GC.Collect(GC.MaxGeneration);
                                }
                                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.F2))
                                {
                                    if (MyInput.Static.IsKeyPress(MyKeys.LeftAlt) && MyInput.Static.IsKeyPress(MyKeys.LeftShift))
                                    {
                                        MyDefinitionManager.Static.ReloadParticles();
                                    }
                                    else if (MyInput.Static.IsKeyPress(MyKeys.LeftShift))
                                    {
                                        MyDefinitionManager.Static.ReloadDecalMaterials();
                                        MyRenderProxy.ReloadTextures();
                                    }
                                    else if (MyInput.Static.IsKeyPress(MyKeys.LeftAlt))
                                    {
                                        MyRenderProxy.ReloadModels();
                                    }
                                    else
                                    {
                                        MyRenderProxy.ReloadEffects();
                                    }
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.F3) && MyInput.Static.IsKeyPress(MyKeys.LeftShift))
                                {
                                    WinApi.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                                }
                                flag2 = this.HandleDebugInput();
                            }
                            if (!flag2)
                            {
                                MyScreenManager.HandleInput();
                            }
                        }
                    }
                }
            }
            finally
            {
            }
        }

        public void HandleInputAfterSimulation()
        {
            if (MySession.Static != null)
            {
                int num1;
                int num2;
                if (!ReferenceEquals(MyScreenManager.GetScreenWithFocus(), MyGuiScreenGamePlay.Static) || (MyGuiScreenGamePlay.Static == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !MyScreenManager.InputToNonFocusedScreens;
                }
                bool flag = (bool) num1;
                bool flag2 = MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND) || ((MySession.Static.ControlledEntity != null) && MySession.Static.ControlledEntity.PrimaryLookaround);
                bool flag3 = (MySession.Static.ControlledEntity != null) && (!flag && (this.m_cameraControllerMovementAllowed != flag));
                if (((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.SpectatorDelta)) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.SpectatorFixed))
                {
                    num2 = 1;
                }
                else
                {
                    num2 = (int) (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.SpectatorOrbit);
                }
                bool flag4 = (bool) num2;
                bool flag5 = flag4;
                bool flag6 = (MyScreenManager.GetScreenWithFocus() is MyGuiScreenDebugBase) && !MyInput.Static.IsAnyAltKeyPressed();
                MySession.Static.GetCameraControllerEnum();
                float roll = MyInput.Static.GetRoll();
                Vector2 rotation = MyInput.Static.GetRotation();
                Vector3 positionDelta = MyInput.Static.GetPositionDelta();
                MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
                if ((MySandboxGame.IsPaused && (screenWithFocus is MyGuiScreenGamePlay)) && !MyScreenManager.InputToNonFocusedScreens)
                {
                    if (!flag4 && !flag5)
                    {
                        return;
                    }
                    if (!flag4)
                    {
                        positionDelta = Vector3.Zero;
                    }
                    if (!flag5 | flag6)
                    {
                        roll = 0f;
                        rotation = Vector2.Zero;
                    }
                }
                else if (!flag2)
                {
                    if (((MySession.Static.CameraController is MySpectatorCameraController) && (MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.ConstantDelta)) && flag)
                    {
                        MySpectatorCameraController.Static.MoveAndRotate(positionDelta, rotation, roll);
                    }
                }
                else
                {
                    if (flag)
                    {
                        MySession.Static.CameraController.Rotate(rotation, roll);
                        if (!m_lookAroundEnabled & flag3)
                        {
                            MySession.Static.ControlledEntity.MoveAndRotateStopped();
                        }
                    }
                    if (flag3)
                    {
                        MySession.Static.CameraController.RotateStopped();
                    }
                }
                MyScreenManager.HandleInputAfterSimulation();
                if (flag3)
                {
                    MySession.Static.ControlledEntity.MoveAndRotateStopped();
                }
                this.m_cameraControllerMovementAllowed = flag;
                m_lookAroundEnabled = flag2;
            }
        }

        public void HandleRenderProfilerInput()
        {
            MyRenderProfiler.HandleInput();
        }

        public void InsertScreen(MyGuiScreenBase screen, int index)
        {
            MyScreenManager.InsertScreen(screen, index);
        }

        public bool IsDebugScreenEnabled() => 
            this.m_debugScreensEnabled;

        public void LoadContent()
        {
            MySandboxGame.Log.WriteLine("MyGuiManager.LoadContent() - START");
            MySandboxGame.Log.IncreaseIndent();
            MyGuiManager.SetMouseCursorTexture(@"Textures\GUI\MouseCursor.dds");
            MyGuiManager.LoadContent();
            MyGuiManager.CurrentLanguage = MySandboxGame.Config.Language;
            MyScreenManager.LoadContent();
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiManager.LoadContent() - END");
        }

        public void LoadData()
        {
            MyScreenManager.LoadData();
            MyGuiManager.LoadData();
            MyLanguage.CurrentLanguage = MySandboxGame.Config.Language;
            if (MyFakes.SHOW_AUDIO_DEV_SCREEN)
            {
                MyGuiScreenDebugAudio screen = new MyGuiScreenDebugAudio();
                this.AddScreen(screen);
            }
        }

        private void LoadDebugInputsFromConfig()
        {
            foreach (KeyValuePair<string, MyConfig.MyDebugInputData> pair in MySandboxGame.Config.DebugInputComponents.Dictionary)
            {
                for (int i = 0; i < this.UserDebugInputComponents.Count; i++)
                {
                    if (this.UserDebugInputComponents[i].GetName() == pair.Key)
                    {
                        this.UserDebugInputComponents[i].Enabled = pair.Value.Enabled;
                        try
                        {
                            this.UserDebugInputComponents[i].InputData = pair.Value.Data;
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public bool OpenSteamOverlay(string url)
        {
            if (!MyGameService.IsOverlayEnabled)
            {
                return false;
            }
            MyGameService.OpenOverlayUrl(url);
            return true;
        }

        public void RemoveScreen(MyGuiScreenBase screen)
        {
            MyScreenManager.RemoveScreen(screen);
        }

        private void SaveDebugInputsToConfig()
        {
            MySandboxGame.Config.DebugInputComponents.Dictionary.Clear();
            SerializableDictionary<string, MyConfig.MyDebugInputData> debugInputComponents = MySandboxGame.Config.DebugInputComponents;
            foreach (MyDebugComponent component in this.UserDebugInputComponents)
            {
                MyConfig.MyDebugInputData data;
                string name = component.GetName();
                debugInputComponents.Dictionary.TryGetValue(name, out data);
                data.Enabled = component.Enabled;
                data.Data = component.InputData;
                debugInputComponents[name] = data;
            }
            MySandboxGame.Config.Save();
        }

        public void SetMouseCursorVisibility(bool visible, bool changePosition = true)
        {
            if (this.m_oldMouseVisibilityState && (visible != this.m_oldMouseVisibilityState))
            {
                this.m_oldVisPos = MyInput.Static.GetMousePosition();
                this.m_oldMouseVisibilityState = visible;
            }
            if (!this.m_oldMouseVisibilityState && (visible != this.m_oldMouseVisibilityState))
            {
                this.m_oldNonVisPos = MyInput.Static.GetMousePosition();
                this.m_oldMouseVisibilityState = visible;
                if (changePosition)
                {
                    MyInput.Static.SetMousePosition((int) this.m_oldVisPos.X, (int) this.m_oldVisPos.Y);
                }
            }
            MySandboxGame.Static.SetMouseVisible(visible);
        }

        private void ShowDeveloperDebugScreen()
        {
            if (!(m_currentDebugScreen is MyGuiScreenDebugOfficial) && !(m_currentDebugScreen is MyGuiScreenDebugDeveloper))
            {
                if (m_currentDebugScreen != null)
                {
                    this.RemoveScreen(m_currentDebugScreen);
                }
                MyGuiScreenDebugDeveloper developer = new MyGuiScreenDebugDeveloper();
                this.AddScreen(m_currentDebugScreen = developer);
                m_currentDebugScreen.Closed += screen => (m_currentDebugScreen = null);
            }
        }

        private static void ShowModDebugScreen()
        {
            if (m_currentDebugScreen == null)
            {
                MyScreenManager.AddScreen(m_currentDebugScreen = new MyGuiScreenDebugOfficial());
                m_currentDebugScreen.Closed += screen => (m_currentDebugScreen = null);
            }
            else if (m_currentDebugScreen is MyGuiScreenDebugOfficial)
            {
                m_currentDebugScreen.RecreateControls(false);
            }
        }

        public void ShowModErrors()
        {
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS || !Sync.MultiplayerActive)
            {
                ShowModDebugScreen();
            }
            else
            {
                this.ShowModErrorsMessageBox();
            }
        }

        private void ShowModErrorsMessageBox()
        {
            ListReader<MyDefinitionErrors.Error> errors = MyDefinitionErrors.GetErrors();
            if (this.m_currentModErrorsMessageBox != null)
            {
                this.RemoveScreen(this.m_currentModErrorsMessageBox);
            }
            StringBuilder messageText = MyTexts.Get(MyCommonTexts.MessageBoxErrorModLoadingFailure);
            messageText.Append("\n");
            foreach (MyDefinitionErrors.Error error in errors)
            {
                if (error.Severity != TErrorSeverity.Critical)
                {
                    continue;
                }
                if (error.ModName != null)
                {
                    messageText.Append("\n");
                    messageText.Append(error.ModName);
                }
            }
            messageText.Append("\n");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            this.m_currentModErrorsMessageBox = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            this.AddScreen(this.m_currentModErrorsMessageBox);
        }

        public void SwitchDebugScreensEnabled()
        {
            this.m_debugScreensEnabled = !this.m_debugScreensEnabled;
        }

        private void SwitchInputScreen()
        {
            if (this.m_currentStatisticsScreen is MyGuiScreenDebugInput)
            {
                this.RemoveScreen(this.m_currentStatisticsScreen);
                this.m_currentStatisticsScreen = null;
            }
            else
            {
                MyGuiScreenDebugBase base2;
                if (this.m_currentStatisticsScreen != null)
                {
                    this.RemoveScreen(this.m_currentStatisticsScreen);
                }
                this.m_currentStatisticsScreen = base2 = new MyGuiScreenDebugInput();
                this.AddScreen(base2);
            }
        }

        public static void SwitchModDebugScreen()
        {
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS || !Sync.MultiplayerActive)
            {
                if (m_currentDebugScreen == null)
                {
                    ShowModDebugScreen();
                }
                else if (m_currentDebugScreen is MyGuiScreenDebugOfficial)
                {
                    m_currentDebugScreen.CloseScreen();
                    m_currentDebugScreen = null;
                }
            }
        }

        private void SwitchStatisticsScreen()
        {
            if (this.m_currentStatisticsScreen is MyGuiScreenDebugStatistics)
            {
                this.RemoveScreen(this.m_currentStatisticsScreen);
                this.m_currentStatisticsScreen = null;
            }
            else
            {
                MyGuiScreenDebugBase base2;
                if (this.m_currentStatisticsScreen != null)
                {
                    this.RemoveScreen(this.m_currentStatisticsScreen);
                }
                this.m_currentStatisticsScreen = base2 = new MyGuiScreenDebugStatistics();
                this.AddScreen(base2);
            }
        }

        private void SwitchTimingScreen()
        {
            if (!(this.m_currentStatisticsScreen is MyGuiScreenDebugTiming))
            {
                MyGuiScreenDebugBase base2;
                if (this.m_currentStatisticsScreen != null)
                {
                    this.RemoveScreen(this.m_currentStatisticsScreen);
                }
                this.m_currentStatisticsScreen = base2 = new MyGuiScreenDebugTiming();
                this.AddScreen(base2);
            }
            else if (MyRenderProxy.DrawRenderStats == MyRenderProxy.MyStatsState.SimpleTimingStats)
            {
                MyRenderProxy.DrawRenderStats = MyRenderProxy.MyStatsState.ComplexTimingStats;
            }
            else
            {
                MyRenderProxy.DrawRenderStats = MyRenderProxy.MyStatsState.MoveNext;
            }
        }

        public void TakeScreenshot(string saveToPath = null, bool ignoreSprites = false, Vector2? sizeMultiplier = new Vector2?(), bool showNotification = true)
        {
            if (sizeMultiplier == null)
            {
                sizeMultiplier = new Vector2(MySandboxGame.Config.ScreenshotSizeMultiplier);
            }
            MyRenderProxy.TakeScreenshot(sizeMultiplier.Value, saveToPath, false, ignoreSprites, showNotification);
        }

        public void TakeScreenshot(int width, int height, string saveToPath = null, bool ignoreSprites = false, bool showNotification = true)
        {
            this.TakeScreenshot(saveToPath, ignoreSprites, new Vector2?(new Vector2((float) width, (float) height) / MySandboxGame.ScreenSize), showNotification);
        }

        public void UnloadContent()
        {
            MyScreenManager.UnloadContent();
        }

        public void Update(int totalTimeInMS)
        {
            int num1;
            int num2;
            this.HandleRenderProfilerInput();
            TotalGamePlayTimeInMilliseconds = totalTimeInMS;
            MyScreenManager.Update(totalTimeInMS);
            MyScreenManager.GetScreenWithFocus();
            if (!MySandboxGame.Static.IsActive)
            {
                num1 = 0;
            }
            else if ((MyExternalAppBase.Static != null) || (MySandboxGame.Static.WindowHandle != GetForegroundWindow()))
            {
                num1 = (MyExternalAppBase.Static == null) ? 0 : ((int) !MyExternalAppBase.IsEditorActive);
            }
            else
            {
                num1 = 1;
            }
            bool gameFocused = (bool) num2;
            if (MyRenderProxy.DrawRenderStats == MyRenderProxy.MyStatsState.Last)
            {
                this.RemoveScreen(this.m_currentStatisticsScreen);
                this.m_currentStatisticsScreen = null;
            }
            MyInput.Static.Update(gameFocused);
            MyGuiManager.Update(totalTimeInMS);
            MyGuiManager.MouseCursorPosition = this.MouseCursorPosition;
            MyGuiManager.TotalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
        }

        public Action<float, Vector2> DrawGameLogoHandler { get; set; }

        public Vector2 MouseCursorPosition =>
            MyGuiManager.GetNormalizedMousePosition(MyInput.Static.GetMousePosition(), MyInput.Static.GetMouseAreaSize());

        public static bool LookaroundEnabled =>
            m_lookAroundEnabled;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDX9Gui.<>c <>9 = new MyDX9Gui.<>c();
            public static MyGuiScreenBase.ScreenHandler <>9__40_0;
            public static MyGuiScreenBase.ScreenHandler <>9__43_0;

            internal void <ShowDeveloperDebugScreen>b__43_0(MyGuiScreenBase screen)
            {
                MyDX9Gui.m_currentDebugScreen = null;
            }

            internal void <ShowModDebugScreen>b__40_0(MyGuiScreenBase screen)
            {
                MyDX9Gui.m_currentDebugScreen = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LogoItem
        {
            public System.Type Screen;
            public string[] Args;
        }
    }
}


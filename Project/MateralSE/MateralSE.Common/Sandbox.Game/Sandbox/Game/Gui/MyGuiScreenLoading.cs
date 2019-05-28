namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.AppCode;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenLoading : MyGuiScreenBase
    {
        public static readonly int STREAMING_TIMEOUT = 900;
        public static MyGuiScreenLoading Static;
        private MyGuiScreenBase m_screenToLoad;
        private readonly MyGuiScreenGamePlay m_screenToUnload;
        private string m_backgroundScreenTexture;
        private string m_backgroundTextureFromConstructor;
        private string m_customTextFromConstructor;
        private string m_rotatingWheelTexture;
        private string m_gameLogoTexture;
        private MyLoadingScreenText m_currentText;
        private MyGuiControlMultilineText m_multiTextControl;
        private StringBuilder m_authorWithDash;
        private MyGuiControlRotatingWheel m_wheel;
        private bool m_exceptionDuringLoad;
        public static string LastBackgroundTexture;
        [CompilerGenerated]
        private Action OnScreenLoadingFinished;
        public Action OnLoadingXMLAllowed;
        public static int m_currentTextIdx = 0;
        private volatile bool m_loadInDrawFinished;
        private bool m_loadFinished;
        private bool m_isStreamed;
        private int m_streamingTimeout;
        private string m_font;
        private static long lastEnvWorkingSet = 0L;
        private static long lastGc = 0L;
        private static long lastVid = 0L;

        public event Action OnScreenLoadingFinished
        {
            [CompilerGenerated] add
            {
                Action onScreenLoadingFinished = this.OnScreenLoadingFinished;
                while (true)
                {
                    Action a = onScreenLoadingFinished;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onScreenLoadingFinished = Interlocked.CompareExchange<Action>(ref this.OnScreenLoadingFinished, action3, a);
                    if (ReferenceEquals(onScreenLoadingFinished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onScreenLoadingFinished = this.OnScreenLoadingFinished;
                while (true)
                {
                    Action source = onScreenLoadingFinished;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onScreenLoadingFinished = Interlocked.CompareExchange<Action>(ref this.OnScreenLoadingFinished, action3, source);
                    if (ReferenceEquals(onScreenLoadingFinished, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenLoading(MyGuiScreenBase screenToLoad, MyGuiScreenGamePlay screenToUnload) : this(screenToLoad, screenToUnload, null, null)
        {
        }

        public MyGuiScreenLoading(MyGuiScreenBase screenToLoad, MyGuiScreenGamePlay screenToUnload, string textureFromConstructor, string customText = null) : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            this.m_font = "LoadingScreen";
            base.CanBeHidden = false;
            base.m_isTopMostScreen = true;
            MyLoadingPerformance.Instance.StartTiming();
            Static = this;
            this.m_screenToLoad = screenToLoad;
            this.m_screenToUnload = screenToUnload;
            base.m_closeOnEsc = false;
            base.DrawMouseCursor = false;
            this.m_loadInDrawFinished = false;
            base.m_drawEvenWithoutFocus = true;
            this.m_currentText = MyLoadingScreenText.GetRandomText();
            base.m_isFirstForUnload = true;
            MyGuiSandbox.SetMouseCursorVisibility(false, true);
            this.m_rotatingWheelTexture = @"Textures\GUI\screens\screen_loading_wheel_loading_screen.dds";
            this.m_backgroundTextureFromConstructor = textureFromConstructor;
            this.m_customTextFromConstructor = customText;
            this.m_loadFinished = false;
            if (this.m_screenToLoad != null)
            {
                int num1;
                MySandboxGame.IsUpdateReady = false;
                if (!Sync.IsServer || Game.IsDedicated)
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) (MyExternalAppBase.Static != null);
                }
                MySandboxGame.AreClipmapsReady = (bool) num1;
                MySandboxGame.RenderTasksFinished = Game.IsDedicated || (MyExternalAppBase.Static != null);
            }
            this.m_authorWithDash = new StringBuilder();
            this.RecreateControls(true);
            MyInput.Static.EnableInput(false);
            if ((Sync.IsServer || Game.IsDedicated) || (MyMultiplayer.Static == null))
            {
                this.m_isStreamed = true;
            }
            else
            {
                MyMultiplayer.Static.LocalRespawnRequested += new Action(this.OnLocalRespawnRequested);
            }
        }

        public override bool Draw()
        {
            this.DrawInternal();
            return base.Draw();
        }

        private unsafe void DrawInternal()
        {
            Rectangle rectangle2;
            Color color = new Color(0xff, 0xff, 0xff, 250);
            Color* colorPtr1 = (Color*) ref color;
            colorPtr1.A = (byte) (color.A * base.m_transitionAlpha);
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", fullscreenRectangle, Color.Black, true);
            MyGuiManager.GetSafeHeightFullScreenPictureSize(MyGuiConstants.LOADING_BACKGROUND_TEXTURE_REAL_SIZE, out rectangle2);
            MyGuiManager.DrawSpriteBatch(this.m_backgroundScreenTexture, rectangle2, new Color(new VRageMath.Vector4(1f, 1f, 1f, base.m_transitionAlpha)), true);
            MyGuiManager.DrawSpriteBatch(@"Textures\Gui\Screens\screen_background_fade.dds", rectangle2, new Color(new VRageMath.Vector4(1f, 1f, 1f, base.m_transitionAlpha)), true);
            MyGuiSandbox.DrawGameLogoHandler(base.m_transitionAlpha, MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 0x2c, 0x44));
            LastBackgroundTexture = this.m_backgroundScreenTexture;
            MyGuiManager.DrawString(this.m_font, MyTexts.Get(MyCommonTexts.LoadingPleaseWaitUppercase), MyGuiConstants.LOADING_PLEASE_WAIT_POSITION, MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.1f, new Color(MyGuiConstants.LOADING_PLEASE_WAIT_COLOR * base.m_transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
            if (string.IsNullOrEmpty(this.m_customTextFromConstructor))
            {
                Vector2 textSize = this.m_multiTextControl.TextSize;
                Vector2 normalizedCoord = this.m_multiTextControl.GetPositionAbsoluteBottomLeft() + new Vector2(((this.m_multiTextControl.Size.X - textSize.X) * 0.5f) + 0.025f, 0.025f);
                Color? colorMask = null;
                MyGuiManager.DrawString(this.m_font, this.m_authorWithDash, normalizedCoord, MyGuiSandbox.GetDefaultTextScaleWithLanguage(), colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            }
            this.m_multiTextControl.Draw(1f, 1f);
        }

        public bool DrawLoading()
        {
            MyTimeSpan? updateTimestamp = null;
            MyRenderProxy.AfterUpdate(updateTimestamp);
            MyRenderProxy.BeforeUpdate();
            this.DrawInternal();
            updateTimestamp = null;
            MyRenderProxy.AfterUpdate(updateTimestamp);
            MyRenderProxy.BeforeUpdate();
            return base.Draw();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenLoading";

        private static string GetRandomBackgroundTexture()
        {
            string str = MyUtils.GetRandomInt(MyPerGameSettings.GUI.LoadingScreenIndexRange.X, MyPerGameSettings.GUI.LoadingScreenIndexRange.Y + 1).ToString().PadLeft(3, '0');
            return (@"Textures\GUI\Screens\loading_background_" + str + ".dds");
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                {
                    this.m_currentText = MyLoadingScreenText.GetScreenText(++m_currentTextIdx);
                    this.RefreshText();
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                {
                    this.m_currentText = MyLoadingScreenText.GetScreenText(--m_currentTextIdx);
                    this.RefreshText();
                }
            }
        }

        public override void LoadContent()
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenLoading.LoadContent - START");
            MySandboxGame.Log.IncreaseIndent();
            this.m_backgroundScreenTexture = this.m_backgroundTextureFromConstructor ?? GetRandomBackgroundTexture();
            this.m_gameLogoTexture = @"Textures\GUI\GameLogoLarge.dds";
            if (this.m_screenToUnload != null)
            {
                this.m_screenToUnload.IsLoaded = false;
                this.m_screenToUnload.CloseScreenNow();
            }
            base.LoadContent();
            MyRenderProxy.LimitMaxQueueSize = true;
            if (((this.m_screenToLoad == null) || this.m_loadInDrawFinished) || !this.m_loadFinished)
            {
                this.m_loadFinished = false;
            }
            else
            {
                this.m_screenToLoad.State = MyGuiScreenState.OPENING;
                this.m_screenToLoad.LoadContent();
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiScreenLoading.LoadContent - END");
        }

        private void MyMultiplayer_PendingReplicablesDone()
        {
            this.m_isStreamed = true;
            if (MySession.Static.VoxelMaps.Instances.Count > 0)
            {
                MySandboxGame.AreClipmapsReady = false;
            }
            MyMultiplayer.Static.PendingReplicablesDone -= new Action(this.MyMultiplayer_PendingReplicablesDone);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyInput.Static.EnableInput(true);
            MyAudio.Static.Mute = false;
        }

        private unsafe void OnLoadException(Exception e, StringBuilder errorText, float heightMultiplier = 1f)
        {
            MySandboxGame.Log.WriteLine("ERROR: Loading screen failed");
            MySandboxGame.Log.WriteLine(e);
            this.UnloadOnException(true);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, errorText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            Vector2 vector = screen.Size.Value;
            float* singlePtr1 = (float*) ref vector.Y;
            singlePtr1[0] *= heightMultiplier;
            screen.Size = new Vector2?(vector);
            screen.RecreateControls(false);
            MyGuiSandbox.AddScreen(screen);
        }

        private void OnLocalRespawnRequested()
        {
            (MyMultiplayer.Static as MyMultiplayerClientBase).RequestBatchConfirmation();
            MyMultiplayer.Static.PendingReplicablesDone += new Action(this.MyMultiplayer_PendingReplicablesDone);
            MyMultiplayer.Static.LocalRespawnRequested -= new Action(this.OnLocalRespawnRequested);
            this.m_streamingTimeout = 0;
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = MyGuiManager.MeasureString(this.m_font, MyTexts.Get(MyCommonTexts.LoadingPleaseWaitUppercase), 1.1f);
            Vector2? textureResolution = null;
            this.m_wheel = new MyGuiControlRotatingWheel(new Vector2?(MyGuiConstants.LOADING_PLEASE_WAIT_POSITION - new Vector2(0f, 0.09f + vector.Y)), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, this.m_rotatingWheelTexture, false, MyPerGameSettings.GUI.MultipleSpinningWheels, textureResolution, 1.5f);
            StringBuilder contents = string.IsNullOrEmpty(this.m_customTextFromConstructor) ? new StringBuilder(this.m_currentText.ToString()) : new StringBuilder(this.m_customTextFromConstructor);
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_multiTextControl = new MyGuiControlMultilineText(new Vector2?(Vector2.One * 0.5f), new Vector2(0.9f, 0.2f), new VRageMath.Vector4?(VRageMath.Vector4.One), this.m_font, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, contents, false, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            this.m_multiTextControl.BorderEnabled = false;
            this.m_multiTextControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            this.m_multiTextControl.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            this.Controls.Add(this.m_wheel);
            this.RefreshText();
        }

        private void RefreshText()
        {
            if (string.IsNullOrEmpty(this.m_customTextFromConstructor))
            {
                this.m_multiTextControl.TextEnum = MyStringId.GetOrCompute(this.m_currentText.ToString());
                if (this.m_currentText is MyLoadingScreenQuote)
                {
                    this.m_authorWithDash.Clear().Append("- ").AppendStringBuilder(MyTexts.Get((this.m_currentText as MyLoadingScreenQuote).Author)).Append(" -");
                }
            }
        }

        private void RunLoad()
        {
            this.m_exceptionDuringLoad = false;
            try
            {
                this.m_screenToLoad.RunLoadingAction();
            }
            catch (MyLoadingNeedXMLException exception)
            {
                this.m_exceptionDuringLoad = true;
                if (this.OnLoadingXMLAllowed == null)
                {
                    this.OnLoadException(exception, new StringBuilder(exception.Message), 1.5f);
                }
                else
                {
                    this.UnloadOnException(false);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.LoadingNeedsXML), MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo), okButtonText, okButtonText, okButtonText, okButtonText, <p0> => this.OnLoadingXMLAllowed(), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
            catch (MyLoadingException exception2)
            {
                this.OnLoadException(exception2, new StringBuilder(exception2.Message), 1.5f);
                this.m_exceptionDuringLoad = true;
            }
            catch (Exception exception3)
            {
                this.OnLoadException(exception3, MyTexts.Get(MyCommonTexts.WorldFileIsCorruptedAndCouldNotBeLoaded), 1f);
                this.m_exceptionDuringLoad = true;
            }
        }

        public override void UnloadContent()
        {
            if (this.m_backgroundScreenTexture != null)
            {
                MyRenderProxy.UnloadTexture(this.m_backgroundScreenTexture);
            }
            if (this.m_backgroundTextureFromConstructor != null)
            {
                MyRenderProxy.UnloadTexture(this.m_backgroundTextureFromConstructor);
            }
            if (this.m_backgroundScreenTexture != null)
            {
                MyRenderProxy.UnloadTexture(this.m_rotatingWheelTexture);
            }
            if (((this.m_screenToLoad != null) && !this.m_loadFinished) && this.m_loadInDrawFinished)
            {
                this.m_screenToLoad.UnloadContent();
                this.m_screenToLoad.UnloadData();
                this.m_screenToLoad = null;
            }
            if ((this.m_screenToLoad != null) && !this.m_loadInDrawFinished)
            {
                this.m_screenToLoad.UnloadContent();
            }
            MyRenderProxy.LimitMaxQueueSize = false;
            base.UnloadContent();
            Static = null;
        }

        private void UnloadOnException(bool exitToMainMenu)
        {
            this.m_screenToLoad = null;
            if (MyGuiScreenGamePlay.Static != null)
            {
                MyGuiScreenGamePlay.Static.UnloadData();
                MyGuiScreenGamePlay.Static.UnloadContent();
            }
            MySandboxGame.IsUpdateReady = true;
            MySandboxGame.AreClipmapsReady = true;
            MySandboxGame.RenderTasksFinished = true;
            try
            {
                if (exitToMainMenu)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
                else
                {
                    MySessionLoader.Unload();
                }
            }
            catch (Exception exception)
            {
                MySession.Static = null;
                MySandboxGame.Log.WriteLine("ERROR: failed unload after exception in loading !");
                MySandboxGame.Log.WriteLine(exception);
            }
        }

        public override bool Update(bool hasFocus)
        {
            int num1;
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if ((base.State == MyGuiScreenState.OPENED) && !this.m_loadFinished)
            {
                MyHud.ScreenEffects.FadeScreen(0f, 0f);
                MyAudio.Static.Mute = true;
                MyAudio.Static.StopMusic();
                MyAudio.Static.ChangeGlobalVolume(0f, 0f);
                this.DrawLoading();
                if (this.m_screenToLoad != null)
                {
                    MySandboxGame.Log.WriteLine("RunLoadingAction - START");
                    this.RunLoad();
                    MySandboxGame.Log.WriteLine("RunLoadingAction - END");
                }
                if (this.m_screenToLoad != null)
                {
                    MyScreenManager.AddScreenNow(this.m_screenToLoad);
                    this.m_screenToLoad.Update(false);
                }
                this.m_screenToLoad = null;
                this.m_loadFinished = true;
                this.m_wheel.ManualRotationUpdate = true;
            }
            this.m_streamingTimeout++;
            if ((Sync.IsServer || (Game.IsDedicated || ((MyMultiplayer.Static == null) || !MyFakes.ENABLE_WAIT_UNTIL_MULTIPLAYER_READY))) || this.m_isStreamed)
            {
                num1 = 1;
            }
            else
            {
                num1 = !MyFakes.LOADING_STREAMING_TIMEOUT_ENABLED ? 0 : ((int) (this.m_streamingTimeout >= STREAMING_TIMEOUT));
            }
            bool flag = (bool) num1;
            if (((this.m_loadFinished && MySandboxGame.IsGameReady) & flag) && MySandboxGame.AreClipmapsReady)
            {
                MyHud.ScreenEffects.FadeScreen(1f, 5f);
                if (!this.m_exceptionDuringLoad && (this.OnScreenLoadingFinished != null))
                {
                    this.OnScreenLoadingFinished();
                    this.OnScreenLoadingFinished = null;
                }
                this.CloseScreenNow();
                this.DrawLoading();
            }
            return true;
        }
    }
}


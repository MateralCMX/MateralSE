namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyGuiScreenIntroVideo : MyGuiScreenBase
    {
        private uint m_videoID;
        private bool m_playbackStarted;
        private string[] m_videos;
        private string m_currentVideo;
        private List<Subtitle> m_subtitles;
        private int m_currentSubtitleIndex;
        private float m_volume;
        private int m_transitionTime;
        private Vector4 m_colorMultiplier;
        private static readonly string m_videoOverlay = @"Textures\GUI\Screens\main_menu_overlay.dds";
        private bool m_loop;
        private bool m_videoOverlayEnabled;

        public MyGuiScreenIntroVideo(string[] videos) : this(videos, false, false, true, 1f, true, 300)
        {
            MyRenderProxy.Settings.RenderThreadHighPriority = true;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        public MyGuiScreenIntroVideo(string[] videos, bool loop, bool videoOverlayEnabled, bool canHaveFocus, float volume, bool closeOnEsc, int transitionTime) : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            this.m_videoID = uint.MaxValue;
            this.m_currentVideo = "";
            this.m_subtitles = new List<Subtitle>();
            this.m_volume = 1f;
            this.m_transitionTime = 300;
            this.m_colorMultiplier = Vector4.One;
            this.m_loop = true;
            this.m_videoOverlayEnabled = true;
            base.DrawMouseCursor = false;
            base.CanHaveFocus = canHaveFocus;
            base.m_closeOnEsc = closeOnEsc;
            base.m_drawEvenWithoutFocus = true;
            this.m_videos = videos;
            this.m_videoOverlayEnabled = videoOverlayEnabled;
            this.m_loop = loop;
            this.m_volume = volume;
            this.m_transitionTime = transitionTime;
        }

        private static void AddCloseEvent(Action onVideoFinished, MyGuiScreenIntroVideo result)
        {
            result.Closed += screen => onVideoFinished();
        }

        public override bool CloseScreen()
        {
            MyRenderProxy.Settings.RenderThreadHighPriority = false;
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            bool flag1 = base.CloseScreen();
            if (flag1)
            {
                this.CloseVideo();
            }
            return flag1;
        }

        public override void CloseScreenNow()
        {
            if (base.State != MyGuiScreenState.CLOSED)
            {
                this.UnloadContent();
            }
            MyRenderProxy.Settings.RenderThreadHighPriority = false;
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            base.CloseScreenNow();
        }

        private void CloseVideo()
        {
            if (this.m_videoID != uint.MaxValue)
            {
                MyRenderProxy.CloseVideo(this.m_videoID);
                this.m_videoID = uint.MaxValue;
            }
        }

        public static MyGuiScreenIntroVideo CreateBackgroundScreen() => 
            new MyGuiScreenIntroVideo(MyPerGameSettings.GUI.MainMenuBackgroundVideos, true, true, false, 0f, false, 0x5dc);

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            if (MyRenderProxy.IsVideoValid(this.m_videoID))
            {
                MyRenderProxy.UpdateVideo(this.m_videoID);
                Vector4 vector = this.m_colorMultiplier * base.m_transitionAlpha;
                MyRenderProxy.DrawVideo(this.m_videoID, MyGuiManager.GetSafeFullscreenRectangle(), new Color(vector), MyVideoRectangleFitMode.AutoFit);
            }
            if (this.m_videoOverlayEnabled)
            {
                this.DrawVideoOverlay();
            }
            return true;
        }

        private void DrawVideoOverlay()
        {
            MyGuiManager.DrawSpriteBatch(m_videoOverlay, MyGuiManager.GetSafeFullscreenRectangle(), Color.White * base.m_transitionAlpha, true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenIntroVideo";

        public override int GetTransitionClosingTime() => 
            this.m_transitionTime;

        public override int GetTransitionOpeningTime() => 
            this.m_transitionTime;

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsNewLeftMousePressed() || (MyInput.Static.IsNewRightMousePressed() || MyInput.Static.IsNewKeyPressed(MyKeys.Space))) || MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
            {
                this.Canceling();
            }
        }

        public override void LoadContent()
        {
            this.m_playbackStarted = false;
            this.LoadRandomVideo();
            base.LoadContent();
        }

        private void LoadRandomVideo()
        {
            int randomInt = MyUtils.GetRandomInt(0, this.m_videos.Length);
            if (this.m_videos.Length != 0)
            {
                this.m_currentVideo = this.m_videos[randomInt];
            }
        }

        private void Loop()
        {
            this.m_currentSubtitleIndex = 0;
            this.LoadRandomVideo();
            this.TryPlayVideo();
        }

        private void TryPlayVideo()
        {
            if (MyFakes.ENABLE_VIDEO_PLAYER)
            {
                this.CloseVideo();
                string path = Path.Combine(MyFileSystem.ContentPath, this.m_currentVideo);
                if (File.Exists(path))
                {
                    this.m_videoID = MyRenderProxy.PlayVideo(path, this.m_volume);
                }
            }
        }

        public override void UnloadContent()
        {
            this.CloseVideo();
            this.m_currentVideo = "";
            base.UnloadContent();
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if (!this.m_playbackStarted)
            {
                this.TryPlayVideo();
                this.m_playbackStarted = true;
            }
            else
            {
                if (MyRenderProxy.IsVideoValid(this.m_videoID) && (MyRenderProxy.GetVideoState(this.m_videoID) != VideoState.Playing))
                {
                    if (this.m_loop)
                    {
                        this.Loop();
                    }
                    else
                    {
                        this.CloseScreen();
                    }
                }
                if ((base.State == MyGuiScreenState.CLOSING) && MyRenderProxy.IsVideoValid(this.m_videoID))
                {
                    MyRenderProxy.SetVideoVolume(this.m_videoID, base.m_transitionAlpha);
                }
            }
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Subtitle
        {
            public TimeSpan StartTime;
            public TimeSpan Length;
            public StringBuilder Text;
            public Subtitle(int startMs, int lengthMs, MyStringId textEnum)
            {
                this.StartTime = TimeSpan.FromMilliseconds((double) startMs);
                this.Length = TimeSpan.FromMilliseconds((double) lengthMs);
                this.Text = MyTexts.Get(textEnum);
            }
        }
    }
}


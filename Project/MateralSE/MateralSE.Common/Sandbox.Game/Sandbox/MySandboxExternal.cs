namespace Sandbox
{
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using VRageRender;
    using VRageRender.ExternalApp;
    using VRageRender.Messages;

    public class MySandboxExternal : MySandboxGame
    {
        public readonly IExternalApp ExternalApp;
        private MyRenderDeviceSettings m_currentSettings;
        private Control m_control;

        public MySandboxExternal(IExternalApp externalApp, string[] commandlineArgs, IntPtr windowHandle) : base(commandlineArgs)
        {
            base.WindowHandle = windowHandle;
            this.ExternalApp = externalApp;
            this.m_control = Control.FromHandle(windowHandle);
        }

        protected override void AfterDraw()
        {
            base.AfterDraw();
            if (base.GameRenderComponent.RenderThread != null)
            {
                Size clientSize = this.m_control.ClientSize;
                if ((((this.m_currentSettings.BackBufferWidth != clientSize.Width) || (this.m_currentSettings.BackBufferHeight != clientSize.Height)) && (clientSize.Height > 0)) && (clientSize.Width > 0))
                {
                    MyRenderDeviceSettings settings = new MyRenderDeviceSettings {
                        AdapterOrdinal = this.m_currentSettings.AdapterOrdinal,
                        RefreshRate = this.m_currentSettings.RefreshRate,
                        VSync = this.m_currentSettings.VSync,
                        WindowMode = this.m_currentSettings.WindowMode,
                        BackBufferHeight = clientSize.Height,
                        BackBufferWidth = clientSize.Width
                    };
                    this.SwitchSettings(settings);
                }
                base.GameRenderComponent.RenderThread.TickSync();
            }
        }

        protected override void CheckGraphicsCard(MyRenderMessageVideoAdaptersResponse msgVideoAdapters)
        {
            base.CheckGraphicsCard(msgVideoAdapters);
            MyPerformanceSettings defaults = new MyPerformanceSettings();
            MyRenderSettings1 settings3 = new MyRenderSettings1 {
                AnisotropicFiltering = MyTextureAnisoFiltering.NONE,
                AntialiasingMode = MyAntialiasingMode.FXAA,
                ShadowQuality = MyShadowsQuality.MEDIUM,
                AmbientOcclusionEnabled = true,
                TextureQuality = MyTextureQuality.MEDIUM,
                ModelQuality = MyRenderQualityEnum.NORMAL,
                VoxelQuality = MyRenderQualityEnum.NORMAL,
                GrassDrawDistance = 160f,
                GrassDensityFactor = 1f,
                HqDepth = true,
                VoxelShaderQuality = MyRenderQualityEnum.NORMAL,
                AlphaMaskedShaderQuality = MyRenderQualityEnum.NORMAL,
                AtmosphereShaderQuality = MyRenderQualityEnum.NORMAL,
                DistanceFade = 100f
            };
            defaults.RenderSettings = settings3;
            defaults.EnableDamageEffects = true;
            MyVideoSettingsManager.UpdateRenderSettingsFromConfig(ref defaults);
        }

        protected override void StartRenderComponent(MyRenderDeviceSettings? settings)
        {
            base.DrawThread = Thread.CurrentThread;
            MyRenderWindow window = new MyRenderWindow {
                Control = Control.FromHandle(base.WindowHandle)
            };
            window.TopLevelForm = (Form) window.Control.TopLevelControl;
            window.TopLevelForm.Tag = window;
            base.m_bufferedInputSource = window;
            window.TopLevelForm.KeyPress += new KeyPressEventHandler(this.TopLevelForm_KeyPress);
            MySandboxGame.m_windowCreatedEvent.Set();
            window.TopLevelForm.FormClosed += (o, e) => ExitThreadSafe();
            Action showCursor = delegate {
            };
            Action hideCursor = delegate {
            };
            base.m_setMouseVisible = delegate (bool b) {
                MyGameRenderComponent gameRenderComponent = this.GameRenderComponent;
                if (gameRenderComponent != null)
                {
                    MyRenderThread renderThread = gameRenderComponent.RenderThread;
                    if (renderThread != null)
                    {
                        renderThread.Invoke(b ? showCursor : hideCursor);
                    }
                }
            };
            if (settings == null)
            {
                settings = new MyRenderDeviceSettings(0, MyWindowModeEnum.Window, window.Control.ClientSize.Width, window.Control.ClientSize.Height, 0, false, false, false);
            }
            base.GameRenderComponent.StartSync(base.m_gameTimer, window, settings, MyRenderQualityEnum.NORMAL, MyPerGameSettings.MaxFrameRate);
            base.GameRenderComponent.RenderThread.SizeChanged += new SizeChangedHandler(this.RenderThread_SizeChanged);
            base.GameRenderComponent.RenderThread.BeforeDraw += new Action(this.RenderThread_BeforeDraw);
            MyViewport viewport = new MyViewport(0f, 0f, (float) window.Control.ClientSize.Width, (float) window.Control.ClientSize.Height);
            base.RenderThread_SizeChanged(window.Control.ClientSize.Width, window.Control.ClientSize.Height, viewport);
        }

        public override void SwitchSettings(MyRenderDeviceSettings settings)
        {
            this.m_currentSettings = settings;
            this.m_currentSettings.WindowMode = MyWindowModeEnum.Window;
            base.SwitchSettings(this.m_currentSettings);
        }

        private void TopLevelForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            ((MyRenderWindow) ((Form) sender).Tag).AddChar(e.KeyChar);
        }

        protected override void Update()
        {
            base.Update();
            this.ExternalApp.Update();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySandboxExternal.<>c <>9 = new MySandboxExternal.<>c();
            public static FormClosedEventHandler <>9__5_0;
            public static Action <>9__5_1;
            public static Action <>9__5_2;

            internal void <StartRenderComponent>b__5_0(object o, FormClosedEventArgs e)
            {
                MySandboxGame.ExitThreadSafe();
            }

            internal void <StartRenderComponent>b__5_1()
            {
            }

            internal void <StartRenderComponent>b__5_2()
            {
            }
        }
    }
}


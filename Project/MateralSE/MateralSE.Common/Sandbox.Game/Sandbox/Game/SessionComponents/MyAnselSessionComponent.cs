namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Ansel;
    using VRage.Game.Components;
    using VRage.Input;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyAnselSessionComponent : MySessionComponentBase
    {
        private MyScreenTool m_screenTool;
        private bool m_isSessionRunning;
        private MyNullInput m_nullInput = new MyNullInput();
        private bool m_prevHeadRenderingEnabled;
        private IMyInput m_prevInput;

        public override void Draw()
        {
            base.Draw();
            bool isAnselSessionRunning = MyAnsel.IsAnselSessionRunning;
            if (isAnselSessionRunning != this.m_isSessionRunning)
            {
                if (!this.m_isSessionRunning)
                {
                    this.SessionStarted();
                }
                else
                {
                    this.SessionEnded();
                    this.m_screenTool.Restore();
                }
                this.m_isSessionRunning = isAnselSessionRunning;
            }
            if (isAnselSessionRunning)
            {
                if (MyAnsel.IsOverlayEnabled)
                {
                    this.m_screenTool.Restore();
                }
                else
                {
                    this.m_screenTool.Hide();
                }
            }
        }

        public override void LoadData()
        {
            this.m_screenTool.Init();
            MyAnsel.IsGamePausable = !Sync.MultiplayerActive;
            MyAnsel.IsAnselSessionEnabled = true;
            if (!MyFakes.ENABLE_ANSEL_IN_MULTIPLAYER)
            {
                MyAnsel.IsAnselSessionEnabled = !Sync.MultiplayerActive;
            }
        }

        private void SessionEnded()
        {
            if (MyAnsel.IsGamePausable)
            {
                MySandboxGame.PausePop();
            }
            MyInput.Static = this.m_prevInput;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                localCharacter.EnableHead(this.m_prevHeadRenderingEnabled);
            }
            MyRenderProxy.SetFrameTimeStep(0f);
        }

        private void SessionStarted()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                this.m_prevHeadRenderingEnabled = localCharacter.HeadRenderingEnabled;
                localCharacter.EnableHead(true);
            }
            this.m_prevInput = MyInput.Static;
            MyInput.Static = this.m_nullInput;
            if (MyAnsel.IsGamePausable)
            {
                MySandboxGame.PausePush();
            }
            MyRenderProxy.SetFrameTimeStep(-1f);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyAnsel.IsAnselSessionEnabled = false;
            if (this.m_isSessionRunning)
            {
                MyAnsel.StopSession();
                this.SessionEnded();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyScreenTool
        {
            private bool m_isHidden;
            private MyAnselGuiScreen m_anselGuiScreen;
            private bool m_prevMinimalHud;
            public void Init()
            {
                this.m_anselGuiScreen = new MyAnselGuiScreen();
            }

            public void Hide()
            {
                if (!this.m_isHidden)
                {
                    this.m_prevMinimalHud = MyHud.MinimalHud;
                    MyHud.MinimalHud = true;
                    MyGuiSandbox.AddScreen(this.m_anselGuiScreen);
                    this.m_isHidden = true;
                }
            }

            public void Restore()
            {
                if (this.m_isHidden)
                {
                    MyGuiSandbox.RemoveScreen(this.m_anselGuiScreen);
                    MyHud.MinimalHud = this.m_prevMinimalHud;
                    this.m_isHidden = false;
                }
            }
        }
    }
}


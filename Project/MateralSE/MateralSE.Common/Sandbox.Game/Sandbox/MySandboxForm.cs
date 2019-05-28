namespace Sandbox
{
    using Sandbox.Engine.Platform;
    using Sandbox.Graphics.GUI.IME;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;
    using VRageRender.ExternalApp;

    internal class MySandboxForm : GameWindowForm, IMyRenderWindow
    {
        private bool m_showCursor = true;
        private bool m_isCursorVisible = true;
        private bool m_captureMouse = true;

        public void BeforeDraw()
        {
            this.UpdateClip();
        }

        private static void ClearClip()
        {
            Cursor.Clip = System.Drawing.Rectangle.Empty;
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.ClientSize = new Size(0x11c, 0x106);
            base.Name = "MySandboxForm";
            base.ResumeLayout(false);
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!this.IsActive)
            {
                this.IsActive = true;
                if (!this.ShowCursor)
                {
                    this.IsCursorVisible = false;
                }
            }
            base.OnActivated(e);
        }

        public void OnDeactivate()
        {
            base.TopMost = false;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (this.IsActive)
            {
                this.IsActive = false;
                ClearClip();
                if (!this.IsCursorVisible)
                {
                    this.IsCursorVisible = true;
                }
            }
            base.OnDeactivate(e);
        }

        public void OnModeChanged(MyWindowModeEnum mode, int width, int height, VRageMath.Rectangle desktopBounds)
        {
            Thread.Sleep(0x3e8);
            switch (mode)
            {
                case MyWindowModeEnum.Window:
                    base.FormBorderStyle = FormBorderStyle.FixedSingle;
                    base.TopMost = false;
                    base.ClientSize = new Size(width, height);
                    base.Location = new System.Drawing.Point(desktopBounds.X + ((desktopBounds.Width - base.ClientSize.Width) / 2), desktopBounds.Y + ((desktopBounds.Height - base.ClientSize.Height) / 2));
                    break;

                case MyWindowModeEnum.FullscreenWindow:
                    base.FormBorderStyle = FormBorderStyle.None;
                    base.TopMost = true;
                    base.SizeGripStyle = SizeGripStyle.Hide;
                    base.ClientSize = new Size(desktopBounds.Width, desktopBounds.Height);
                    base.Location = new System.Drawing.Point(desktopBounds.Left, desktopBounds.Top);
                    break;

                case MyWindowModeEnum.Fullscreen:
                    base.TopMost = true;
                    base.FormBorderStyle = FormBorderStyle.None;
                    break;

                default:
                    break;
            }
            MySandboxGame.Static.UpdateMouseCapture();
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            ClearClip();
            base.OnResizeBegin(e);
        }

        private void SetClip()
        {
            Cursor.Clip = base.RectangleToScreen(base.ClientRectangle);
        }

        public void SetMouseCapture(bool capture)
        {
            this.m_captureMouse = capture;
            this.UpdateClip();
        }

        public void UpdateClip()
        {
            if (!base.IsDisposed)
            {
                MySandboxGame.GameWindowHandle = base.Handle;
                Control control = FromHandle(WinApi.GetForegroundWindow());
                bool flag = false;
                if (control != null)
                {
                    flag = !control.TopLevelControl.InvokeRequired && (base.Handle == control.TopLevelControl.Handle);
                }
                if (flag && (this.m_captureMouse || !this.m_isCursorVisible))
                {
                    this.SetClip();
                }
                else
                {
                    ClearClip();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = true;
            int msg = m.Msg;
            if (msg == 0x51)
            {
                if (MyImeProcessor.Instance != null)
                {
                    MyImeProcessor.Instance.LanguageChanged();
                }
            }
            else
            {
                if ((msg - 0x10d) > 2)
                {
                    switch (msg)
                    {
                        case 0x281:
                        case 0x282:
                        case 0x283:
                        case 0x284:
                        case 0x285:
                        case 0x286:
                        case 0x288:
                        case 0x290:
                        case 0x291:
                            break;

                        default:
                            goto TR_0002;
                    }
                }
                flag = false;
            }
        TR_0002:
            if (flag)
            {
                base.WndProc(ref m);
            }
            MyMessageLoop.AddMessage(ref m);
        }

        private bool IsCursorVisible
        {
            get => 
                this.m_isCursorVisible;
            set
            {
                if (!this.m_isCursorVisible & value)
                {
                    while (WinApi.ShowCursor(true) < 0)
                    {
                    }
                    this.m_isCursorVisible = value;
                }
                else if (this.m_isCursorVisible && !value)
                {
                    while (true)
                    {
                        if (WinApi.ShowCursor(false) < 0)
                        {
                            this.m_isCursorVisible = value;
                            break;
                        }
                    }
                }
            }
        }

        public bool IsActive { get; private set; }

        public bool ShowCursor
        {
            get => 
                this.m_showCursor;
            set
            {
                if (this.m_showCursor != value)
                {
                    this.m_showCursor = value;
                    this.IsCursorVisible = value;
                }
            }
        }

        public bool DrawEnabled =>
            (base.WindowState != FormWindowState.Minimized);
    }
}


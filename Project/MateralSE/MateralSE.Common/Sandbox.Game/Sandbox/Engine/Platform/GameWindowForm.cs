namespace Sandbox.Engine.Platform
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Graphics.GUI.IME;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using VRage;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;
    using VRageRender.ExternalApp;

    internal class GameWindowForm : Form, IMessageFilter, IMyBufferedInputSource
    {
        private bool allowUserResizing;
        private MyGuiControlIme m_ImeControl;
        private MouseEventArgs m_emptyMouseEventArgs;
        private FastResourceLock m_bufferedCharsLock;
        private List<char> m_bufferedChars;
        private Vector2 m_mousePosition;

        public GameWindowForm() : this("VRage")
        {
        }

        public GameWindowForm(string text)
        {
            this.m_emptyMouseEventArgs = new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0);
            this.m_bufferedCharsLock = new FastResourceLock();
            this.m_bufferedChars = new List<char>();
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BypassedMessages = new HashSet<int>();
            if (MyFakes.ENABLE_IME && (MySandboxGame.Config.Language == MyLanguagesEnum.ChineseChina))
            {
                this.InitializeIME();
            }
            else
            {
                this.BypassedMessages.Add(0x282);
                this.BypassedMessages.Add(6);
            }
            this.BypassedMessages.Add(0x100);
            this.BypassedMessages.Add(0x101);
            this.BypassedMessages.Add(0x102);
            this.BypassedMessages.Add(0x103);
            this.BypassedMessages.Add(260);
            this.BypassedMessages.Add(0x105);
            this.BypassedMessages.Add(0x106);
            this.BypassedMessages.Add(0x107);
            this.BypassedMessages.Add(0x20a);
            this.BypassedMessages.Add(0x200);
            this.BypassedMessages.Add(0x201);
            this.BypassedMessages.Add(0x202);
            this.BypassedMessages.Add(0x203);
            this.BypassedMessages.Add(0x204);
            this.BypassedMessages.Add(0x205);
            this.BypassedMessages.Add(0x206);
            this.BypassedMessages.Add(0x207);
            this.BypassedMessages.Add(520);
            this.BypassedMessages.Add(0x209);
            this.BypassedMessages.Add(0x20d);
            this.BypassedMessages.Add(0x20b);
            this.BypassedMessages.Add(0x20c);
            this.BypassedMessages.Add(20);
            this.BypassedMessages.Add(0x18);
            this.BypassedMessages.Add(7);
            this.BypassedMessages.Add(8);
        }

        public void AddChar(char ch)
        {
            this.m_bufferedChars.Add(ch);
        }

        private void FocusIme()
        {
            if (this.m_ImeControl != null)
            {
                base.ActiveControl = this.m_ImeControl;
                Action method = () => this.m_ImeControl.Focus();
                this.m_ImeControl.Invoke(method);
            }
        }

        private void InitializeIME()
        {
            MyImeProcessor.CreateInstance();
            base.ImeMode = ImeMode.On;
            this.m_ImeControl = new MyGuiControlIme();
            base.Controls.Add(this.m_ImeControl);
            this.m_ImeControl.ActivateInputListening();
            this.m_ImeControl.Size = new Size(0, 10);
            this.m_ImeControl.AutoFocusing = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            MessageFilterHook.RemoveMessageFilter(base.Handle, this);
        }

        protected override void OnLoad(EventArgs e)
        {
            MessageFilterHook.AddMessageFilter(base.Handle, this);
            base.OnLoad(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x200)
            {
                return false;
            }
            if (m.Msg == 0x102)
            {
                return false;
            }
            if (m.Msg != 260)
            {
                if (m.Msg == 0x105)
                {
                    return true;
                }
                if (m.Msg == 0x106)
                {
                    return true;
                }
                if (m.Msg == 0x107)
                {
                    return true;
                }
                if (m.Msg == 0x101)
                {
                    return true;
                }
                if (m.Msg == 0x100)
                {
                    return true;
                }
                if (m.Msg == 0xa4)
                {
                    return true;
                }
                if (m.Msg == 6)
                {
                    MyRenderProxy.HandleFocusMessage((m.WParam != IntPtr.Zero) ? MyWindowFocusMessage.Activate : MyWindowFocusMessage.Deactivate);
                }
                if (m.Msg == 7)
                {
                    MyRenderProxy.HandleFocusMessage(MyWindowFocusMessage.SetFocus);
                }
                if (!this.BypassedMessages.Contains(m.Msg))
                {
                    return false;
                }
                if (m.Msg == 6)
                {
                    if (m.WParam == IntPtr.Zero)
                    {
                        this.OnDeactivate(EventArgs.Empty);
                    }
                    else
                    {
                        this.OnActivated(EventArgs.Empty);
                    }
                }
                if (m.Msg == 0x200)
                {
                    this.OnMouseMove(this.m_emptyMouseEventArgs);
                }
                m.Result = WinApi.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
            return true;
        }

        void IMyBufferedInputSource.AddChar(char ch)
        {
            using (this.m_bufferedCharsLock.AcquireExclusiveUsing())
            {
                this.m_bufferedChars.Add(ch);
            }
        }

        void IMyBufferedInputSource.SwapBufferedTextInput(ref List<char> swappedBuffer)
        {
            swappedBuffer.Clear();
            using (this.m_bufferedCharsLock.AcquireExclusiveUsing())
            {
                List<char> list = swappedBuffer;
                swappedBuffer = this.m_bufferedChars;
                this.m_bufferedChars = list;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg != 260)
            {
                if (m.Msg != 0x102)
                {
                    if (m.Msg == 0x200)
                    {
                        this.m_mousePosition.X = (short) ((long) m.LParam);
                        this.m_mousePosition.Y = (short) (((long) m.LParam) >> 0x10);
                    }
                    base.WndProc(ref m);
                }
                else
                {
                    char wParam = (char) ((int) m.WParam);
                    using (this.m_bufferedCharsLock.AcquireExclusiveUsing())
                    {
                        this.m_bufferedChars.Add(wParam);
                    }
                }
            }
        }

        public HashSet<int> BypassedMessages { get; private set; }

        internal bool AllowUserResizing
        {
            get => 
                this.allowUserResizing;
            set
            {
                if (this.allowUserResizing != value)
                {
                    this.allowUserResizing = value;
                    base.MaximizeBox = this.allowUserResizing;
                    this.FormBorderStyle = this.allowUserResizing ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
                }
            }
        }

        Vector2 IMyBufferedInputSource.MousePosition =>
            this.m_mousePosition;

        Vector2 IMyBufferedInputSource.MouseAreaSize =>
            new Vector2((float) base.ClientSize.Width, (float) base.ClientSize.Height);
    }
}


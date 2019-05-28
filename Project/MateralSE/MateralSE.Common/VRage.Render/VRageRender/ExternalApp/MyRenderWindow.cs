namespace VRageRender.ExternalApp
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using VRage;
    using VRageMath;
    using VRageRender;

    public class MyRenderWindow : IMyRenderWindow, IMyBufferedInputSource
    {
        public System.Windows.Forms.Control Control;
        public Form TopLevelForm;
        private readonly FastResourceLock m_bufferedCharsLock = new FastResourceLock();
        private List<char> m_bufferedChars = new List<char>();

        public void AddChar(char ch)
        {
            this.m_bufferedChars.Add(ch);
        }

        public void BeforeDraw()
        {
        }

        public void OnDeactivate()
        {
        }

        public void OnModeChanged(MyWindowModeEnum mode, int width, int height, VRageMath.Rectangle desktopBounds)
        {
        }

        public void SetMouseCapture(bool capture)
        {
            if (capture)
            {
                Cursor.Clip = this.Control.RectangleToScreen(this.Control.ClientRectangle);
                Cursor.Hide();
            }
            else
            {
                Cursor.Clip = new System.Drawing.Rectangle(0, 0, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                Cursor.Show();
            }
        }

        void IMyBufferedInputSource.AddChar(char ch)
        {
            this.AddChar(ch);
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

        public bool DrawEnabled =>
            true;

        public IntPtr Handle =>
            this.Control.Handle;

        Vector2 IMyBufferedInputSource.MousePosition =>
            new Vector2();

        Vector2 IMyBufferedInputSource.MouseAreaSize =>
            new Vector2((float) this.Control.ClientSize.Width, (float) this.Control.ClientSize.Height);
    }
}


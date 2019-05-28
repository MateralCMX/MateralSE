namespace VRage.Input
{
    using SharpDX;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Windows.Forms;

    public static class MyWindowsMouse
    {
        private static int m_currentWheel;
        private static IntPtr m_windowHandle;

        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern unsafe bool ClientToScreen(void* handle, POINT* lpPoint);
        private static int GET_WHEEL_DELTA_WPARAM(IntPtr wParam) => 
            ((short) HIWORD(wParam));

        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int keyCode);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern unsafe int GetCursorPos(POINT* point);
        public static unsafe void GetPosition(out int x, out int y)
        {
            POINT point;
            GetCursorPos(&point);
            if (m_windowHandle != IntPtr.Zero)
            {
                ScreenToClient(m_windowHandle.ToPointer(), &point);
            }
            x = point.X;
            y = point.Y;
        }

        private static ushort HIWORD(IntPtr dwValue) => 
            ((ushort) ((((long) dwValue) >> 0x10) & 0xffffL));

        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern unsafe int ScreenToClient(void* handle, POINT* point);
        [DllImport("user32.dll")]
        private static extern IntPtr SetCapture(IntPtr hWnd);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        public static void SetMouseCapture(IntPtr window)
        {
            SetCapture(window);
        }

        public static unsafe void SetPosition(int x, int y)
        {
            POINT lpPoint = new POINT(x, y);
            if (m_windowHandle != IntPtr.Zero)
            {
                ClientToScreen(m_windowHandle.ToPointer(), &lpPoint);
            }
            SetCursorPos(lpPoint.X, lpPoint.Y);
        }

        public static void SetWindow(IntPtr windowHandle)
        {
            m_windowHandle = windowHandle;
            MessageFilterHook.AddMessageFilter(windowHandle, new MouseMessageFilter());
        }

        public class MouseMessageFilter : IMessageFilter
        {
            private const int WmMouseWheel = 0x20a;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0x20a)
                {
                    int num = MyWindowsMouse.GET_WHEEL_DELTA_WPARAM(m.WParam);
                    MyWindowsMouse.m_currentWheel += num;
                }
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=8), NativeCppClass]
        private struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
    }
}


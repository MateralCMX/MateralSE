namespace VRage.Input
{
    using SharpDX;
    using SharpDX.DirectInput;
    using System;
    using VRage.Utils;

    public static class MyDirectInput
    {
        private static SharpDX.DirectInput.DirectInput m_directInput;
        private static Mouse m_mouse;
        private static MouseState m_mouseState = new MouseState();

        public static void Close()
        {
            if (m_mouse != null)
            {
                m_mouse.Dispose();
                m_mouse = null;
            }
            if (m_directInput != null)
            {
                m_directInput.Dispose();
                m_directInput = null;
            }
        }

        public static MyMouseState GetMouseState()
        {
            if (m_mouse == null)
            {
                return new MyMouseState();
            }
            MyMouseState state = new MyMouseState();
            if (m_mouse.TryAcquire().Success)
            {
                try
                {
                    m_mouse.GetCurrentState(ref m_mouseState);
                    m_mouse.Poll();
                    state = new MyMouseState {
                        X = m_mouseState.X,
                        Y = m_mouseState.Y,
                        LeftButton = m_mouseState.Buttons[0],
                        RightButton = m_mouseState.Buttons[1],
                        MiddleButton = m_mouseState.Buttons[2],
                        XButton1 = m_mouseState.Buttons[3],
                        XButton2 = m_mouseState.Buttons[4],
                        ScrollWheelValue = m_mouseState.Z
                    };
                }
                catch (SharpDXException)
                {
                }
            }
            return state;
        }

        public static void Initialize(IntPtr handle)
        {
            try
            {
                m_directInput = new SharpDX.DirectInput.DirectInput();
                m_mouse = new Mouse(m_directInput);
                try
                {
                    m_mouse.SetCooperativeLevel(handle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
                }
                catch
                {
                    MyLog.Default.WriteLine("WARNING: DirectInput SetCooperativeLevel failed");
                }
            }
            catch (SharpDXException exception)
            {
                MyLog.Default.WriteLine("DirectInput initialization error: " + exception);
            }
        }

        public static SharpDX.DirectInput.DirectInput DirectInput =>
            m_directInput;
    }
}


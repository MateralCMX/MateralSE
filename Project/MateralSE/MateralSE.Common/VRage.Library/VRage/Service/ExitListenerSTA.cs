namespace VRage.Service
{
    using System;
    using VRage.Win32;

    public static class ExitListenerSTA
    {
        public static  event ApplicationExitHandler OnExit;

        public static void Listen()
        {
            WinApi.MSG msg;
            if (WinApi.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                WinApi.TranslateMessage(ref msg);
                WinApi.DispatchMessage(ref msg);
                if (msg.message == 0x10)
                {
                    Raise();
                }
            }
        }

        private static bool Raise()
        {
            bool stopListening = true;
            ApplicationExitHandler onExit = m_onExit;
            if (onExit != null)
            {
                onExit(ref stopListening);
            }
            return stopListening;
        }
    }
}


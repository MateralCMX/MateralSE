namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyMessageBox
    {
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern uint MessageBox(IntPtr hWndle, string text, string caption, int buttons);
        public static void Show(string caption, string text)
        {
            MessageBox(IntPtr.Zero, text, caption, 0);
        }
    }
}


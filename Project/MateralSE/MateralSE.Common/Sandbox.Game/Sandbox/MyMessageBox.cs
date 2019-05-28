namespace Sandbox
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyMessageBox
    {
        public static MessageBoxResult Show(IntPtr hWnd, string text, string caption, MessageBoxOptions options) => 
            Show(hWnd, text, caption, (int) options);

        [DllImport("user32.dll", EntryPoint="MessageBox", CharSet=CharSet.Auto)]
        private static extern MessageBoxResult Show(IntPtr hWnd, string text, string caption, int options);
    }
}


namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public static class MyClipboardHelper
    {
        public static void SetClipboard(string text)
        {
            Thread thread1 = new Thread(delegate {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetText(text);
                }
                catch (ExternalException)
                {
                }
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            thread1.Join();
        }
    }
}


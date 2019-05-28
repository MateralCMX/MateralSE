namespace VRage.Library.Utils
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    public static class MyBrowserHelper
    {
        public const string IE_PROCESS = "IExplore.exe";

        public static bool OpenInternetBrowser(string url)
        {
            try
            {
                try
                {
                    Process.Start(url);
                }
                catch (Win32Exception)
                {
                    Process.Start(new ProcessStartInfo("IExplore.exe", url));
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}


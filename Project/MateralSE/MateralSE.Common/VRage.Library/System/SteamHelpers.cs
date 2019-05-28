namespace System
{
    using System.IO;
    using System.Linq;

    public static class SteamHelpers
    {
        public static bool IsAppManifestPresent(string path, uint appId)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                return (IsSteamPath(path) && Directory.GetFiles(info.Parent.Parent.FullName).Contains<string>(("AppManifest_" + appId + ".acf"), StringComparer.InvariantCultureIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSteamPath(string path)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                return (info.Parent.Name.Equals("Common", StringComparison.InvariantCultureIgnoreCase) && info.Parent.Parent.Name.Equals("SteamApps", StringComparison.InvariantCultureIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}


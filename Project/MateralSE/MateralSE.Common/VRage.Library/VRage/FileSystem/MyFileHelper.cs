namespace VRage.FileSystem
{
    using System;
    using System.IO;

    public class MyFileHelper
    {
        public static bool CanWrite(string path)
        {
            bool flag;
            if (!File.Exists(path))
            {
                return true;
            }
            try
            {
                using (File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    flag = true;
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}


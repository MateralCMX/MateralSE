namespace Sandbox.Game
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBasicGameInfo
    {
        public int? GameVersion;
        public string GameName;
        public string GameNameSafe;
        public string ApplicationName;
        public string GameAcronym;
        public string MinimumRequirementsWeb;
        public string SplashScreenImage;
        public bool CheckIsSetup()
        {
            bool flag = true;
            FieldInfo[] fields = this.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                bool flag2 = fields[i].GetValue(this) != null;
                flag &= flag2;
            }
            return flag;
        }
    }
}


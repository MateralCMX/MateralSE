namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderDeviceSettings : IEquatable<MyRenderDeviceSettings>
    {
        public int AdapterOrdinal;
        public int NewAdapterOrdinal;
        public bool DisableWindowedModeForOldDriver;
        public MyWindowModeEnum WindowMode;
        public int BackBufferWidth;
        public int BackBufferHeight;
        public int RefreshRate;
        public bool VSync;
        public bool DebugDrawOnly;
        public bool UseStereoRendering;
        public bool SettingsMandatory;
        public MyRenderDeviceSettings(int adapter, MyWindowModeEnum windowMode, int width, int height, int refreshRate, bool vsync, bool useStereoRendering, bool settingsMandatory)
        {
            this.AdapterOrdinal = adapter;
            this.NewAdapterOrdinal = adapter;
            this.DisableWindowedModeForOldDriver = false;
            this.WindowMode = windowMode;
            this.BackBufferWidth = width;
            this.BackBufferHeight = height;
            this.RefreshRate = refreshRate;
            this.VSync = vsync;
            this.UseStereoRendering = useStereoRendering;
            this.SettingsMandatory = settingsMandatory;
            this.DebugDrawOnly = false;
        }

        bool IEquatable<MyRenderDeviceSettings>.Equals(MyRenderDeviceSettings other) => 
            this.Equals(ref other);

        public bool Equals(ref MyRenderDeviceSettings other) => 
            ((this.AdapterOrdinal == other.AdapterOrdinal) && ((this.WindowMode == other.WindowMode) && ((this.BackBufferWidth == other.BackBufferWidth) && ((this.BackBufferHeight == other.BackBufferHeight) && ((this.RefreshRate == other.RefreshRate) && ((this.VSync == other.VSync) && ((this.UseStereoRendering == other.UseStereoRendering) && (this.SettingsMandatory == other.SettingsMandatory))))))));

        public override string ToString()
        {
            string str = "MyRenderDeviceSettings: {\n";
            object[] objArray1 = new object[] { str, "AdapterOrdinal: ", this.AdapterOrdinal, "\n" };
            str = string.Concat(objArray1);
            object[] objArray2 = new object[] { str, "NewAdapterOrdinal: ", this.NewAdapterOrdinal, "\n" };
            str = string.Concat(objArray2);
            object[] objArray3 = new object[] { str, "WindowMode: ", this.WindowMode, "\n" };
            str = string.Concat(objArray3);
            object[] objArray4 = new object[] { str, "BackBufferWidth: ", this.BackBufferWidth, "\n" };
            str = string.Concat(objArray4);
            object[] objArray5 = new object[] { str, "BackBufferHeight: ", this.BackBufferHeight, "\n" };
            str = string.Concat(objArray5);
            object[] objArray6 = new object[] { str, "RefreshRate: ", this.RefreshRate, "\n" };
            return (((((string.Concat(objArray6) + "VSync: " + this.VSync.ToString() + "\n") + "DebugDrawOnly: " + this.DebugDrawOnly.ToString() + "\n") + "UseStereoRendering: " + this.UseStereoRendering.ToString() + "\n") + "SettingsMandatory: " + this.SettingsMandatory.ToString() + "\n") + "}");
        }
    }
}


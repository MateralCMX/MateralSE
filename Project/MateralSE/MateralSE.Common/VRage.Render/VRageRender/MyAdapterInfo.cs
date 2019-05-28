namespace VRageRender
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("DeviceName: '{Name}', Description: '{Description}'")]
    public struct MyAdapterInfo
    {
        public string Name;
        public string DeviceName;
        public MyDisplayMode[] SupportedDisplayModes;
        public string OutputName;
        public string Description;
        public int AdapterDeviceId;
        public int OutputId;
        public Rectangle DesktopBounds;
        public Vector2I DesktopResolution;
        public int MaxTextureSize;
        public bool Has512MBRam;
        public bool IsDx11Supported;
        public int Priority;
        public bool IsOutputAttached;
        public ulong VRAM;
        public ulong SVRAM;
        public bool MultithreadedRenderingSupported;
        public VendorIds VendorId;
        public int DeviceId;
        public string DriverVersion;
        public string DriverDate;
        public bool DriverUpdateNecessary;
        public string DriverUpdateLink;
        public bool IsNvidiaNotebookGpu;
        public bool AftermathSupported;
        public MyRenderQualityEnum Quality;
        public bool Mobile;
        public void LogInfo(Action<string> lineWriter)
        {
            lineWriter("Adapter: " + this.Name);
            lineWriter("VendorId: " + this.VendorId);
            lineWriter("DeviceId: " + this.DeviceId);
            lineWriter("Description: " + this.Description);
        }

        public override string ToString() => 
            $"DeviceName: '{this.Name}', Description: '{this.Description}'";

        public MyDisplayMode? GetDisplayMode(int width, int height, int refreshRate)
        {
            foreach (MyDisplayMode mode in this.SupportedDisplayModes)
            {
                if (((mode.Width == width) && (mode.Height == height)) && ((refreshRate == 0) || (Math.Abs((float) (mode.RefreshRateF - (((float) refreshRate) / 1000f))) < 0.1f)))
                {
                    return new MyDisplayMode?(mode);
                }
            }
            return null;
        }
    }
}


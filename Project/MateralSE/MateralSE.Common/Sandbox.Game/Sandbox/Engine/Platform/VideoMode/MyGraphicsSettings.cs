namespace Sandbox.Engine.Platform.VideoMode
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGraphicsSettings
    {
        public MyPerformanceSettings PerformanceSettings;
        public float FieldOfView;
        public bool PostProcessingEnabled;
        public MyStringId GraphicsRenderer;
        public float FlaresIntensity;
        public override bool Equals(object other)
        {
            MyGraphicsSettings settings = (MyGraphicsSettings) other;
            return this.Equals(ref settings);
        }

        public bool Equals(ref MyGraphicsSettings other) => 
            ((this.FieldOfView == other.FieldOfView) && ((this.PostProcessingEnabled == other.PostProcessingEnabled) && ((this.FlaresIntensity == other.FlaresIntensity) && ((this.GraphicsRenderer == other.GraphicsRenderer) && this.PerformanceSettings.Equals(other.PerformanceSettings)))));
    }
}


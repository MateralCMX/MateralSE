namespace Sandbox.Engine.Platform.VideoMode
{
    using System;
    using System.Runtime.InteropServices;
    using VRageRender;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPerformanceSettings
    {
        public MyRenderSettings1 RenderSettings;
        public bool EnableDamageEffects;
        public override bool Equals(object other)
        {
            MyPerformanceSettings settings = (MyPerformanceSettings) other;
            return this.Equals(ref settings);
        }

        private bool Equals(ref MyPerformanceSettings other) => 
            ((this.EnableDamageEffects == other.EnableDamageEffects) && this.RenderSettings.Equals(ref other.RenderSettings));
    }
}


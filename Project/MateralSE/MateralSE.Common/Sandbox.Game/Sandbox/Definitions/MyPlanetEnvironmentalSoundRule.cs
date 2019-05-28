namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPlanetEnvironmentalSoundRule
    {
        public SymmetricSerializableRange Latitude;
        public SerializableRange Height;
        public SerializableRange SunAngleFromZenith;
        public MyStringHash EnvironmentSound;
        public bool Check(float angleFromEquator, float height, float sunAngleFromZenith) => 
            (this.Latitude.ValueBetween(angleFromEquator) && (this.Height.ValueBetween(height) && this.SunAngleFromZenith.ValueBetween(sunAngleFromZenith)));
    }
}


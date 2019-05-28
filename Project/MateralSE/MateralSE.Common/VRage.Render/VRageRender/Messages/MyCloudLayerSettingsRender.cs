namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCloudLayerSettingsRender
    {
        public uint ID;
        public uint AtmosphereID;
        public string DebugName;
        public string Model;
        public List<string> Textures;
        public Vector3D CenterPoint;
        public double Altitude;
        public double MinScaledAltitude;
        public bool ScalingEnabled;
        public Vector3D RotationAxis;
        public float AngularVelocity;
        public float InitialRotation;
        public double FadeOutRelativeAltitudeStart;
        public double FadeOutRelativeAltitudeEnd;
        public float ApplyFogRelativeDistance;
        public double MaxPlanetHillRadius;
        public Vector4 Color;
        public bool FadeIn;
    }
}


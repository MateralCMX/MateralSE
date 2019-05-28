namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRageMath;
    using VRageRender.Messages;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPlanetInitArguments
    {
        public string StorageName;
        public int Seed;
        public IMyStorage Storage;
        public Vector3D PositionMinCorner;
        public float Radius;
        public float AtmosphereRadius;
        public float MaxRadius;
        public float MinRadius;
        public bool HasAtmosphere;
        public Vector3 AtmosphereWavelengths;
        public float GravityFalloff;
        public bool MarkAreaEmpty;
        public MyAtmosphereSettings AtmosphereSettings;
        public float SurfaceGravity;
        public bool AddGps;
        public bool SpherizeWithDistance;
        public MyPlanetGeneratorDefinition Generator;
        public bool UserCreated;
        public bool InitializeComponents;
        public bool FadeIn;
        public override string ToString()
        {
            object[] objArray1 = new object[0x24];
            objArray1[0] = "Planet init arguments: \nStorage name: ";
            objArray1[1] = this.StorageName ?? "<null>";
            object[] local2 = objArray1;
            local2[2] = "\n Storage: ";
            local2[3] = (this.Storage != null) ? this.Storage.ToString() : "<null>";
            object[] local3 = local2;
            local3[4] = "\n PositionMinCorner: ";
            local3[5] = this.PositionMinCorner;
            local3[6] = "\n Radius: ";
            local3[7] = this.Radius;
            local3[8] = "\n AtmosphereRadius: ";
            local3[9] = this.AtmosphereRadius;
            local3[10] = "\n MaxRadius: ";
            local3[11] = this.MaxRadius;
            local3[12] = "\n MinRadius: ";
            local3[13] = this.MinRadius;
            local3[14] = "\n HasAtmosphere: ";
            local3[15] = this.HasAtmosphere.ToString();
            local3[0x10] = "\n AtmosphereWavelengths: ";
            local3[0x11] = this.AtmosphereWavelengths;
            local3[0x12] = "\n GravityFalloff: ";
            local3[0x13] = this.GravityFalloff;
            local3[20] = "\n MarkAreaEmpty: ";
            local3[0x15] = this.MarkAreaEmpty.ToString();
            local3[0x16] = "\n AtmosphereSettings: ";
            local3[0x17] = this.AtmosphereSettings.ToString();
            local3[0x18] = "\n SurfaceGravity: ";
            local3[0x19] = this.SurfaceGravity;
            local3[0x1a] = "\n AddGps: ";
            local3[0x1b] = this.AddGps.ToString();
            local3[0x1c] = "\n SpherizeWithDistance: ";
            local3[0x1d] = this.SpherizeWithDistance.ToString();
            local3[30] = "\n Generator: ";
            local3[0x1f] = (this.Generator != null) ? this.Generator.ToString() : "<null>";
            object[] local4 = local3;
            local4[0x20] = "\n UserCreated: ";
            local4[0x21] = this.UserCreated.ToString();
            local4[0x22] = "\n InitializeComponents: ";
            local4[0x23] = this.InitializeComponents.ToString();
            return string.Concat(local4);
        }
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Planet : MyObjectBuilder_VoxelMap
    {
        [ProtoMember(0x20)]
        public float Radius;
        [ProtoMember(0x23)]
        public bool HasAtmosphere;
        [ProtoMember(0x26)]
        public float AtmosphereRadius;
        [ProtoMember(0x29)]
        public float MinimumSurfaceRadius;
        [ProtoMember(0x2c)]
        public float MaximumHillRadius;
        [ProtoMember(0x2f)]
        public Vector3 AtmosphereWavelengths;
        [ProtoMember(50), XmlArrayItem("Sector"), Nullable]
        public SavedSector[] SavedEnviromentSectors;
        [ProtoMember(0x37)]
        public float GravityFalloff;
        [ProtoMember(0x3a)]
        public bool MarkAreaEmpty;
        [ProtoMember(0x3d), Nullable]
        public MyAtmosphereSettings? AtmosphereSettings;
        [ProtoMember(0x41)]
        public float SurfaceGravity = 1f;
        [ProtoMember(0x44)]
        public bool SpawnsFlora;
        [ProtoMember(0x47)]
        public bool ShowGPS;
        [ProtoMember(0x4a)]
        public bool SpherizeWithDistance = true;
        [ProtoMember(0x4d), Nullable]
        public string PlanetGenerator = "";
        [ProtoMember(0x51)]
        public int Seed;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct SavedSector
        {
            [ProtoMember(20)]
            public Vector3S IdPos;
            [ProtoMember(0x17)]
            public Vector3B IdDir;
            [ProtoMember(0x1a), XmlElement("Item"), Nullable]
            public HashSet<int> RemovedItems;
        }
    }
}


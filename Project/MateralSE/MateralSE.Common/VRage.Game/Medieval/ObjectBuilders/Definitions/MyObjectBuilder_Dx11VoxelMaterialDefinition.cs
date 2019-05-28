namespace Medieval.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Dx11VoxelMaterialDefinition : MyObjectBuilder_VoxelMaterialDefinition
    {
        [ProtoMember(15), ModdableContentFile("dds")]
        public string ColorMetalXZnY;
        [ProtoMember(0x13), ModdableContentFile("dds")]
        public string ColorMetalY;
        [ProtoMember(0x17), ModdableContentFile("dds")]
        public string NormalGlossXZnY;
        [ProtoMember(0x1b), ModdableContentFile("dds")]
        public string NormalGlossY;
        [ProtoMember(0x1f), ModdableContentFile("dds")]
        public string ExtXZnY;
        [ProtoMember(0x23), ModdableContentFile("dds")]
        public string ExtY;
        [ProtoMember(0x27), ModdableContentFile("dds")]
        public string ColorMetalXZnYFar1;
        [ProtoMember(0x2b), ModdableContentFile("dds")]
        public string ColorMetalYFar1;
        [ProtoMember(0x2f), ModdableContentFile("dds")]
        public string NormalGlossXZnYFar1;
        [ProtoMember(0x33), ModdableContentFile("dds")]
        public string NormalGlossYFar1;
        [ProtoMember(0x37)]
        public float Scale = 8f;
        [ProtoMember(0x3a)]
        public float ScaleFar1 = 8f;
        [ProtoMember(0x3d), ModdableContentFile("dds")]
        public string ExtXZnYFar1;
        [ProtoMember(0x41), ModdableContentFile("dds")]
        public string ExtYFar1;
        [ProtoMember(0x45), ModdableContentFile("dds")]
        public string FoliageTextureArray1;
        [ProtoMember(0x49), ModdableContentFile("dds")]
        public string FoliageTextureArray2;
        [ProtoMember(0x4d), ModdableContentFile("dds"), XmlArrayItem("Color")]
        public string[] FoliageColorTextureArray;
        [ProtoMember(0x52), ModdableContentFile("dds"), XmlArrayItem("Normal")]
        public string[] FoliageNormalTextureArray;
        [ProtoMember(0x57)]
        public float FoliageDensity;
        [ProtoMember(90)]
        public Vector2 FoliageScale = Vector2.One;
        [ProtoMember(0x5d)]
        public float FoliageRandomRescaleMult;
        [ProtoMember(0x60)]
        public int FoliageType;
        [ProtoMember(0x63)]
        public byte BiomeValueMin;
        [ProtoMember(0x66)]
        public byte BiomeValueMax;
        [ProtoMember(0x69), ModdableContentFile("dds")]
        public string ColorMetalXZnYFar2;
        [ProtoMember(0x6d), ModdableContentFile("dds")]
        public string ColorMetalYFar2;
        [ProtoMember(0x71), ModdableContentFile("dds")]
        public string NormalGlossXZnYFar2;
        [ProtoMember(0x75), ModdableContentFile("dds")]
        public string NormalGlossYFar2;
        [ProtoMember(0x79), ModdableContentFile("dds")]
        public string ExtXZnYFar2;
        [ProtoMember(0x7d), ModdableContentFile("dds")]
        public string ExtYFar2;
        [ProtoMember(0x81)]
        public float InitialScale = 2f;
        [ProtoMember(0x84)]
        public float ScaleMultiplier = 4f;
        [ProtoMember(0x87)]
        public float InitialDistance = 5f;
        [ProtoMember(0x8a)]
        public float DistanceMultiplier = 4f;
        [ProtoMember(0x8d)]
        public float TilingScale = 32f;
        [ProtoMember(0x90)]
        public float Far1Distance;
        [ProtoMember(0x93)]
        public float Far2Distance;
        [ProtoMember(150)]
        public float Far3Distance;
        [ProtoMember(0x99)]
        public float Far1Scale = 400f;
        [ProtoMember(0x9c)]
        public float Far2Scale = 2000f;
        [ProtoMember(0x9f)]
        public float Far3Scale = 7000f;
        [ProtoMember(0xa2)]
        public Vector4 Far3Color = ((Vector4) Color.Black);
        [ProtoMember(0xa5)]
        public float ExtDetailScale;
        [ProtoMember(0xa8, IsRequired=false)]
        public TilingSetup SimpleTilingSetup;
    }
}


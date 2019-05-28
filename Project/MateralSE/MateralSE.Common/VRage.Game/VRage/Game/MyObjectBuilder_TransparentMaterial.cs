namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TransparentMaterial : MyObjectBuilder_Base
    {
        [ProtoMember(13)]
        public string Name;
        [ProtoMember(0x10)]
        public bool AlphaMistingEnable;
        [ProtoMember(0x13), DefaultValue(1)]
        public float AlphaMistingStart = 1f;
        [ProtoMember(0x16), DefaultValue(4)]
        public float AlphaMistingEnd = 4f;
        [ProtoMember(0x19), DefaultValue(1)]
        public float AlphaSaturation = 1f;
        [ProtoMember(0x1c)]
        public bool CanBeAffectedByOtherLights;
        [ProtoMember(0x1f)]
        public float Emissivity;
        [ProtoMember(0x22)]
        public bool IgnoreDepth;
        [ProtoMember(0x25), DefaultValue(true)]
        public bool NeedSort = true;
        [ProtoMember(40)]
        public float SoftParticleDistanceScale;
        [ProtoMember(0x2b)]
        public string Texture;
        [ProtoMember(0x2e)]
        public bool UseAtlas;
        [ProtoMember(0x31)]
        public Vector2 UVOffset = new Vector2(0f, 0f);
        [ProtoMember(0x34)]
        public Vector2 UVSize = new Vector2(1f, 1f);
        [ProtoMember(0x37)]
        public bool Reflection;
    }
}


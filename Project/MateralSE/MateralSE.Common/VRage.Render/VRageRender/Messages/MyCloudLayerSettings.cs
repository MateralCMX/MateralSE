namespace VRageRender.Messages
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRageMath;

    [ProtoContract]
    public class MyCloudLayerSettings
    {
        [ProtoMember(12)]
        public string Model;
        [ProtoMember(15), XmlArrayItem("Texture")]
        public List<string> Textures;
        [ProtoMember(0x12)]
        public float RelativeAltitude;
        [ProtoMember(0x15)]
        public Vector3D RotationAxis;
        [ProtoMember(0x17)]
        public float AngularVelocity;
        [ProtoMember(0x19)]
        public float InitialRotation;
        [ProtoMember(0x1c)]
        public bool ScalingEnabled;
        [ProtoMember(30)]
        public float FadeOutRelativeAltitudeStart;
        [ProtoMember(0x20)]
        public float FadeOutRelativeAltitudeEnd;
        [ProtoMember(0x22)]
        public float ApplyFogRelativeDistance;
        [ProtoMember(0x25)]
        public Vector4 Color = Vector4.One;
    }
}


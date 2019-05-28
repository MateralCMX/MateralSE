namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ModelComponentDefinition : MyObjectBuilder_ComponentDefinitionBase
    {
        [ProtoMember(14)]
        public Vector3 Size;
        [ProtoMember(0x11)]
        public float Mass;
        [ProtoMember(20), DefaultValue((string) null)]
        public float? Volume;
        [ProtoMember(0x17), ModdableContentFile("mwm")]
        public string Model = @"Models\Components\Sphere.mwm";
    }
}


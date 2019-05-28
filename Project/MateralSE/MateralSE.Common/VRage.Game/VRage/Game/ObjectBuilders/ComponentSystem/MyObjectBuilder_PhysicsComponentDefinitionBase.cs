namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PhysicsComponentDefinitionBase : MyObjectBuilder_ComponentDefinitionBase
    {
        [ProtoMember(0x1d), DefaultValue(0)]
        public MyMassPropertiesComputationType MassPropertiesComputation;
        [ProtoMember(0x20), DefaultValue(0)]
        public RigidBodyFlag RigidBodyFlags;
        [ProtoMember(0x23), DefaultValue((string) null)]
        public string CollisionLayer;
        [ProtoMember(0x26), DefaultValue((string) null)]
        public float? LinearDamping;
        [ProtoMember(0x29), DefaultValue((string) null)]
        public float? AngularDamping;
        [ProtoMember(0x2c)]
        public bool ForceActivate;
        [ProtoMember(0x2f)]
        public MyUpdateFlags UpdateFlags;
        [ProtoMember(50)]
        public bool Serialize;

        public enum MyMassPropertiesComputationType
        {
            None,
            Box,
            Sphere,
            Capsule,
            Cylinder
        }

        [Flags]
        public enum MyUpdateFlags
        {
            Gravity = 1
        }
    }
}


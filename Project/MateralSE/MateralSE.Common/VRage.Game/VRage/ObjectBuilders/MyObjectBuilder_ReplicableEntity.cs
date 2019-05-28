namespace VRage.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using VRage;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_ReplicableEntity : MyObjectBuilder_EntityBase
    {
        public SerializableVector3 LinearVelocity;
        public SerializableVector3 AngularVelocity;
        public float Mass = 5f;
    }
}


namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_StaticEnvironmentModule : MyObjectBuilder_EnvironmentModuleBase
    {
        [ProtoMember(14)]
        public HashSet<int> DisabledItems = new HashSet<int>();
        [Nullable, ProtoMember(0x12)]
        public List<SerializableOrientedBoundingBoxD> Boxes = new List<SerializableOrientedBoundingBoxD>();
        [ProtoMember(0x15)]
        public int MinScanned = 15;
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGenerator : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x3e)]
        public HashSet<EmptyArea> MarkedAreas = new HashSet<EmptyArea>();
        [ProtoMember(0x41)]
        public HashSet<EmptyArea> DeletedAreas = new HashSet<EmptyArea>();
        [ProtoMember(0x44)]
        public HashSet<MyObjectSeedParams> ExistingObjectsSeeds = new HashSet<MyObjectSeedParams>();
    }
}


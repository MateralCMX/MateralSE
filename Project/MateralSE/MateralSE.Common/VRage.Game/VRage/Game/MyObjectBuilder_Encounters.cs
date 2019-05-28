namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Encounters : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x44)]
        public HashSet<MyEncounterId> SavedEncounters;
    }
}


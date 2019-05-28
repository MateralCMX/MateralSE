namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GlobalEvents : MyObjectBuilder_Base
    {
        [ProtoMember(12)]
        public List<MyObjectBuilder_GlobalEventBase> Events = new List<MyObjectBuilder_GlobalEventBase>();
    }
}


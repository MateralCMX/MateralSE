namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BlockGroup : MyObjectBuilder_Base
    {
        [ProtoMember(13)]
        public string Name;
        [ProtoMember(0x10)]
        public List<Vector3I> Blocks = new List<Vector3I>();
    }
}


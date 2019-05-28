namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FunctionalBlock : MyObjectBuilder_TerminalBlock
    {
        [ProtoMember(11)]
        public bool Enabled = true;
    }
}


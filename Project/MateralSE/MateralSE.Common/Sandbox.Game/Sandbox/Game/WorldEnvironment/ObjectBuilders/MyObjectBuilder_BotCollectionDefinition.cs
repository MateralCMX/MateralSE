namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, XmlType("VR.EI.BotCollection")]
    public class MyObjectBuilder_BotCollectionDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x17), XmlElement("Bot")]
        public BotDefEntry[] Bots;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct BotDefEntry
        {
            [ProtoMember(15)]
            public SerializableDefinitionId Id;
            [ProtoMember(0x12), XmlAttribute("Probability")]
            public float Probability;
        }
    }
}


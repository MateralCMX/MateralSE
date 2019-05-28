namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AIComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x19)]
        public List<BotData> BotBrains = new List<BotData>();

        public bool ShouldSerializeBotBrains() => 
            ((this.BotBrains != null) && (this.BotBrains.Count > 0));

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct BotData
        {
            [ProtoMember(0x11)]
            public int PlayerHandle;
            [ProtoMember(20), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_Bot>))]
            public MyObjectBuilder_Bot BotBrain;
        }
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_GlobalChatHistory : MyObjectBuilder_Base
    {
        [ProtoMember(0x34), XmlArrayItem("GCI")]
        public List<MyObjectBuilder_GlobalChatItem> Chat;
    }
}


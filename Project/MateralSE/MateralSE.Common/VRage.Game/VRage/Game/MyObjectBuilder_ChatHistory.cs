namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_ChatHistory : MyObjectBuilder_Base
    {
        [ProtoMember(13)]
        public long IdentityId;
        [ProtoMember(15)]
        public List<MyObjectBuilder_PlayerChatHistory> PlayerChatHistory;
        [ProtoMember(0x11)]
        public MyObjectBuilder_GlobalChatHistory GlobalChatHistory;
    }
}


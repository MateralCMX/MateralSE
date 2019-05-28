namespace VRage.Game.ObjectBuilders.AI.Bot
{
    using ProtoBuf;
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_HumanoidBot : MyObjectBuilder_AgentBot
    {
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_GoodAIControlHandTool : MyObjectBuilder_HandToolBase
    {
    }
}


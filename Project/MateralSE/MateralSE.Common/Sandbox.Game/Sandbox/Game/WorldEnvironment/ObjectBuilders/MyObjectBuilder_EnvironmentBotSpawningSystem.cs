namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_EnvironmentBotSpawningSystem : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(11)]
        public int TimeSinceLastEventInMs;
    }
}


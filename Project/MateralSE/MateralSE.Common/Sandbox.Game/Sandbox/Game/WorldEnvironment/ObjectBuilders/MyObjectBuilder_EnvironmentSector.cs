namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_EnvironmentSector : MyObjectBuilder_Base
    {
        [ProtoMember(10)]
        public long SectorId;
    }
}


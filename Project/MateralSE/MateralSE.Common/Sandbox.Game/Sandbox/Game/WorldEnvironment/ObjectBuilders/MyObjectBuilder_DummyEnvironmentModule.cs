namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_DummyEnvironmentModule : MyObjectBuilder_EnvironmentModuleBase
    {
        [ProtoMember(10, IsRequired=false)]
        public HashSet<int> DisabledItems = new HashSet<int>();
    }
}


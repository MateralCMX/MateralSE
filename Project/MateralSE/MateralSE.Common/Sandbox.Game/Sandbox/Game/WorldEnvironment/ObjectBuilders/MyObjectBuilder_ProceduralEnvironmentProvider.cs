namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_ProceduralEnvironmentProvider : MyObjectBuilder_EnvironmentDataProvider
    {
        [ProtoMember(13), XmlElement("Sector")]
        public List<MyObjectBuilder_ProceduralEnvironmentSector> Sectors = new List<MyObjectBuilder_ProceduralEnvironmentSector>();
    }
}


namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_EnvironmentDataProvider : MyObjectBuilder_Base
    {
        [ProtoMember(12), XmlAttribute("Face")]
        public Base6Directions.Direction Face;
    }
}


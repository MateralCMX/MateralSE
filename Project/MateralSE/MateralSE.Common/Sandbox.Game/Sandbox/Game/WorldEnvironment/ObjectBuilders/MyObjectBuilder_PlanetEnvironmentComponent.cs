namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_PlanetEnvironmentComponent : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(0x11), XmlElement("Provider", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EnvironmentDataProvider>)), DynamicNullableObjectBuilderItem(false)]
        public MyObjectBuilder_EnvironmentDataProvider[] DataProviders = new MyObjectBuilder_EnvironmentDataProvider[0];
        [ProtoMember(0x22), XmlArrayItem("Sector"), Nullable]
        public List<ObstructingBox> SectorObstructions = new List<ObstructingBox>();

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct ObstructingBox
        {
            [ProtoMember(0x1a)]
            public long SectorId;
            [ProtoMember(30)]
            public List<SerializableOrientedBoundingBoxD> ObstructingBoxes;
        }
    }
}


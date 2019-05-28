namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CutsceneSessionComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(14), XmlArrayItem("Cutscene")]
        public Cutscene[] Cutscenes;
        [XmlArrayItem("WaypointName")]
        public List<string> VoxelPrecachingWaypointNames = new List<string>();
    }
}


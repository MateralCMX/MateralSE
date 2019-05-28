namespace VRage.ObjectBuilders.Definitions.Components
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VoxelMesherComponentDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(14), XmlArrayItem("Step")]
        public List<MyObjectBuilder_VoxelPostprocessing> PostprocessingSteps = new List<MyObjectBuilder_VoxelPostprocessing>();
    }
}


namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [XmlType("VR.EI.VoxelMapCollection"), MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_VoxelMapCollectionDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlElement("Storage")]
        public VoxelMapStorage[] StorageDefs;
        public string Modifier;

        [StructLayout(LayoutKind.Sequential)]
        public struct VoxelMapStorage
        {
            [XmlAttribute("Storage")]
            public string Storage;
            [XmlAttribute("Probability")]
            public float Probability;
        }
    }
}


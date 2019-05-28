namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [XmlType("VR.ProceduralWorldEnvironment"), MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_ProceduralWorldEnvironment : MyObjectBuilder_WorldEnvironmentBase
    {
        [XmlArrayItem("Item")]
        public MyEnvironmentItemTypeDefinition[] ItemTypes;
        [XmlArrayItem("Mapping")]
        public MyProceduralEnvironmentMapping[] EnvironmentMappings;
        public MyProceduralScanningMethod ScanningMethod;
    }
}


namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;

    [XmlType("VR.ProceduralEnvironmentModule")]
    public class MyObjectBuilder_ProceduralEnvironmentModuleDefinition : MyObjectBuilder_DefinitionBase
    {
        public string QualifiedTypeName;
    }
}


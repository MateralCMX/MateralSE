namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;

    [XmlType("VR.EnvironmentModuleProxy")]
    public class MyObjectBuilder_EnvironmentModuleProxyDefinition : MyObjectBuilder_DefinitionBase
    {
        public string QualifiedTypeName;
    }
}


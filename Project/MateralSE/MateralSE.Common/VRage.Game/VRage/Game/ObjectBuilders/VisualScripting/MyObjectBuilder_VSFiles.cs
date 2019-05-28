namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Campaign;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VSFiles : MyObjectBuilder_Base
    {
        public MyObjectBuilder_VisualScript VisualScript;
        public MyObjectBuilder_VisualLevelScript LevelScript;
        public MyObjectBuilder_Campaign Campaign;
        public MyObjectBuilder_ScriptSM StateMachine;
    }
}


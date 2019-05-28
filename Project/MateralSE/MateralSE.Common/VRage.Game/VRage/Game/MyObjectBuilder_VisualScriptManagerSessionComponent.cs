namespace VRage.Game
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VisualScriptManagerSessionComponent : MyObjectBuilder_SessionComponent
    {
        public bool FirstRun = true;
        [XmlArray("LevelScriptFiles", IsNullable=true), XmlArrayItem("FilePath")]
        public string[] LevelScriptFiles;
        [XmlArray("StateMachines", IsNullable=true), XmlArrayItem("FilePath")]
        public string[] StateMachines;
        [DefaultValue((string) null)]
        public MyObjectBuilder_ScriptStateMachineManager ScriptStateMachineManager;
        [DefaultValue((string) null)]
        public MyObjectBuilder_Questlog Questlog;
    }
}


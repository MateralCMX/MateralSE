namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CommentScriptNode : MyObjectBuilder_ScriptNode
    {
        public string CommentText = "Insert Comment...";
        public SerializableVector2 CommentSize = new SerializableVector2(50f, 20f);
    }
}


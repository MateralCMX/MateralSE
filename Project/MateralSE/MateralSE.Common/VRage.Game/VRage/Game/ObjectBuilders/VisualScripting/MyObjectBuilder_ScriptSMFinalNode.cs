﻿namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ScriptSMFinalNode : MyObjectBuilder_ScriptSMNode
    {
    }
}

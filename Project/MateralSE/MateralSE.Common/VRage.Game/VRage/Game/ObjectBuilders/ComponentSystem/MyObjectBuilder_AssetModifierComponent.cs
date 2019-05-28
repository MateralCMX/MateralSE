namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AssetModifierComponent : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(14), XmlArrayItem("AssetModifier"), Serialize(MyObjectFlags.DefaultZero)]
        public List<SerializableDefinitionId> AssetModifiers = new List<SerializableDefinitionId>();
    }
}


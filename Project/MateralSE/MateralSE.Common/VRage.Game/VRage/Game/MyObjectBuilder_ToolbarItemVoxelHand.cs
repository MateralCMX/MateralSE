namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ToolbarItemVoxelHand : MyObjectBuilder_ToolbarItemDefinition
    {
        public bool ShouldSerializedefId() => 
            false;

        public SerializableDefinitionId defId
        {
            get => 
                base.DefinitionId;
            set => 
                (base.DefinitionId = value);
        }
    }
}


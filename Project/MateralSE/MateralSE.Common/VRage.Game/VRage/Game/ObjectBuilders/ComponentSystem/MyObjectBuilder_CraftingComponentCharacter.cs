namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers"), Obsolete("This is here only for backwards compatibility, the component has renamed from Character to Basic!")]
    public class MyObjectBuilder_CraftingComponentCharacter : MyObjectBuilder_CraftingComponentBase
    {
    }
}


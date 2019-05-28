namespace VRage.Game.ObjectBuilders.Components
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_EntityOwnershipComponent : MyObjectBuilder_ComponentBase
    {
        public long OwnerId;
        public MyOwnershipShareModeEnum ShareMode;
    }
}


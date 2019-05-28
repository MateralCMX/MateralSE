namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionComponentSafeZones : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x1f)]
        public MySafeZoneAction AllowedActions = MySafeZoneAction.All;
    }
}


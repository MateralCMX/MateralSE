namespace VRage.Game.ObjectBuilders.Campaign
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CampaignSMNode : MyObjectBuilder_Base
    {
        public string Name;
        public string SaveFilePath;
        public SerializableVector2 Location;
    }
}


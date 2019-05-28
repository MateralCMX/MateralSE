namespace VRage.Game.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CampaignSessionComponent : MyObjectBuilder_SessionComponent
    {
        public string CampaignName;
        public string ActiveState;
        public bool IsVanilla;
        public MyObjectBuilder_Checkpoint.ModItem Mod;
        public string LocalModFolder;
        public string CurrentOutcome;
    }
}


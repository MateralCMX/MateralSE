namespace VRage.Game.ObjectBuilders.Campaign
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Campaign : MyObjectBuilder_Base
    {
        public MyObjectBuilder_CampaignSM StateMachine;
        [XmlArrayItem("Path")]
        public List<string> LocalizationPaths = new List<string>();
        [XmlArrayItem("Language")]
        public List<string> LocalizationLanguages = new List<string>();
        public string DefaultLocalizationLanguage;
        public string DescriptionLocalizationFile;
        public string Name;
        public string Description;
        public string ImagePath;
        public bool IsMultiplayer;
        public string Difficulty;
        public string Author;
        [XmlIgnore]
        public bool IsVanilla = true;
        [XmlIgnore]
        public bool IsLocalMod = true;
        [XmlIgnore]
        public string ModFolderPath;
        [XmlIgnore]
        public ulong PublishedFileId;
        [XmlIgnore]
        public bool IsDebug;
    }
}


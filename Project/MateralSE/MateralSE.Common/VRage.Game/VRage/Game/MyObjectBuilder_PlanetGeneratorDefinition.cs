namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.ObjectBuilders.Definitions.Components;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("PlanetGeneratorDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PlanetGeneratorDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x22d)]
        public MyPlanetMaps? PlanetMaps;
        [ProtoMember(560)]
        public bool? HasAtmosphere;
        [ProtoMember(0x233), XmlArrayItem("CloudLayer")]
        public List<MyCloudLayerSettings> CloudLayers;
        [ProtoMember(0x237)]
        public SerializableRange? HillParams;
        [ProtoMember(570)]
        public float? GravityFalloffPower;
        [ProtoMember(0x23d)]
        public SerializableRange? MaterialsMaxDepth;
        [ProtoMember(0x240)]
        public SerializableRange? MaterialsMinDepth;
        [ProtoMember(0x243)]
        public MyAtmosphereColorShift HostileAtmosphereColorShift;
        [ProtoMember(0x246), XmlArrayItem("Material")]
        public MyPlanetMaterialDefinition[] CustomMaterialTable;
        [ProtoMember(0x24a), XmlArrayItem("Distortion")]
        public MyPlanetDistortionDefinition[] DistortionTable;
        [ProtoMember(590)]
        public MyPlanetMaterialDefinition DefaultSurfaceMaterial;
        [ProtoMember(0x251)]
        public MyPlanetMaterialDefinition DefaultSubSurfaceMaterial;
        [ProtoMember(0x254)]
        public float? SurfaceGravity;
        [ProtoMember(0x257)]
        public MyPlanetAtmosphere Atmosphere;
        [ProtoMember(0x25a)]
        public MyAtmosphereSettings? AtmosphereSettings;
        [ProtoMember(0x25d)]
        public string FolderName;
        [ProtoMember(0x260)]
        public MyPlanetMaterialGroup[] ComplexMaterials;
        [ProtoMember(0x263), XmlArrayItem("SoundRule")]
        public MySerializablePlanetEnvironmentalSoundRule[] SoundRules;
        [ProtoMember(0x267), XmlArrayItem("MusicCategory")]
        public List<MyMusicCategory> MusicCategories;
        [ProtoMember(0x26b), XmlArrayItem("Ore")]
        public MyPlanetOreMapping[] OreMappings;
        [ProtoMember(0x26f), XmlArrayItem("Item")]
        public PlanetEnvironmentItemMapping[] EnvironmentItems;
        [ProtoMember(0x273)]
        public MyPlanetMaterialBlendSettings? MaterialBlending;
        [ProtoMember(630)]
        public MyPlanetSurfaceDetail SurfaceDetail;
        [ProtoMember(0x279)]
        public MyPlanetAnimalSpawnInfo AnimalSpawnInfo;
        [ProtoMember(0x27c)]
        public MyPlanetAnimalSpawnInfo NightAnimalSpawnInfo;
        public float? SectorDensity;
        [ProtoMember(0x281)]
        public string InheritFrom;
        public SerializableDefinitionId? Environment;
        [XmlElement(typeof(MyAbstractXmlSerializer<MyObjectBuilder_PlanetMapProvider>))]
        public MyObjectBuilder_PlanetMapProvider MapProvider;
        [ProtoMember(0x289)]
        public MyObjectBuilder_VoxelMesherComponentDefinition MesherPostprocessing;
        [ProtoMember(0x28c)]
        public float MinimumSurfaceLayerDepth = 4f;
    }
}


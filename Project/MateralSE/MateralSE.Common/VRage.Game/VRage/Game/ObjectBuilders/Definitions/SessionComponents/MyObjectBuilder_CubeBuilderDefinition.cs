namespace VRage.Game.ObjectBuilders.Definitions.SessionComponents
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CubeBuilderDefinition : MyObjectBuilder_SessionComponentDefinition
    {
        public float DefaultBlockBuildingDistance = 20f;
        public float MaxBlockBuildingDistance = 20f;
        public float MinBlockBuildingDistance = 1f;
        public double BuildingDistSmallSurvivalCharacter = 5.0;
        public double BuildingDistLargeSurvivalCharacter = 10.0;
        public double BuildingDistSmallSurvivalShip = 12.5;
        public double BuildingDistLargeSurvivalShip = 12.5;
        public MyPlacementSettings BuildingSettings;
    }
}


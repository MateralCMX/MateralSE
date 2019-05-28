namespace VRage.Game.Definitions.SessionComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;

    [MyDefinitionType(typeof(MyObjectBuilder_CubeBuilderDefinition), (Type) null)]
    public class MyCubeBuilderDefinition : MySessionComponentDefinition
    {
        public float DefaultBlockBuildingDistance;
        public float MaxBlockBuildingDistance;
        public float MinBlockBuildingDistance;
        public double BuildingDistSmallSurvivalCharacter;
        public double BuildingDistLargeSurvivalCharacter;
        public double BuildingDistSmallSurvivalShip;
        public double BuildingDistLargeSurvivalShip;
        public MyPlacementSettings BuildingSettings;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CubeBuilderDefinition definition = (MyObjectBuilder_CubeBuilderDefinition) builder;
            this.DefaultBlockBuildingDistance = definition.DefaultBlockBuildingDistance;
            this.MaxBlockBuildingDistance = definition.MaxBlockBuildingDistance;
            this.MinBlockBuildingDistance = definition.MinBlockBuildingDistance;
            this.BuildingDistSmallSurvivalCharacter = definition.BuildingDistSmallSurvivalCharacter;
            this.BuildingDistLargeSurvivalCharacter = definition.BuildingDistLargeSurvivalCharacter;
            this.BuildingDistSmallSurvivalShip = definition.BuildingDistSmallSurvivalShip;
            this.BuildingDistLargeSurvivalShip = definition.BuildingDistLargeSurvivalShip;
            this.BuildingSettings = definition.BuildingSettings;
        }
    }
}


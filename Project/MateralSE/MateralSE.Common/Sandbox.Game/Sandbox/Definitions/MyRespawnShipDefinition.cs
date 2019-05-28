namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_RespawnShipDefinition), (Type) null)]
    public class MyRespawnShipDefinition : MyDefinitionBase
    {
        public int Cooldown;
        public MyPrefabDefinition Prefab;
        public bool UseForSpace;
        public float MinimalAirDensity;
        public bool UseForPlanetsWithAtmosphere;
        public bool UseForPlanetsWithoutAtmosphere;
        public float PlanetDeployAltitude;
        public Vector3 InitialLinearVelocity;
        public Vector3 InitialAngularVelocity;
        public bool SpawnWithDefaultItems;
        public string HelpTextLocalizationId;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            float valueOrDefault;
            base.Init(builder);
            MyObjectBuilder_RespawnShipDefinition definition = (MyObjectBuilder_RespawnShipDefinition) builder;
            this.Cooldown = definition.CooldownSeconds;
            this.Prefab = MyDefinitionManager.Static.GetPrefabDefinition(definition.Prefab);
            this.UseForSpace = definition.UseForSpace;
            this.MinimalAirDensity = definition.MinimalAirDensity;
            this.SpawnWithDefaultItems = definition.SpawnWithDefaultItems;
            this.InitialLinearVelocity = (Vector3) definition.InitialLinearVelocity;
            this.InitialAngularVelocity = (Vector3) definition.InitialAngularVelocity;
            this.UseForPlanetsWithAtmosphere = definition.UseForPlanetsWithAtmosphere;
            this.UseForPlanetsWithoutAtmosphere = definition.UseForPlanetsWithoutAtmosphere;
            float? planetDeployAltitude = definition.PlanetDeployAltitude;
            if (planetDeployAltitude != null)
            {
                valueOrDefault = planetDeployAltitude.GetValueOrDefault();
            }
            else
            {
                int num1;
                if (definition.UseForPlanetsWithAtmosphere || definition.UseForPlanetsWithoutAtmosphere)
                {
                    num1 = 0x7d0;
                }
                else
                {
                    num1 = 10;
                }
                valueOrDefault = num1;
            }
            this.PlanetDeployAltitude = valueOrDefault;
            this.HelpTextLocalizationId = definition.HelpTextLocalizationId;
        }
    }
}


namespace SpaceEngineers.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.World;
    using System;
    using VRage.Game;

    [StartingStateType(typeof(MyObjectBuilder_WorldGeneratorPlayerStartingState_RespawnShip))]
    public class MyRespawnShipState : MyWorldGeneratorStartingStateBase
    {
        private string m_respawnShipId;

        public override MyObjectBuilder_WorldGeneratorPlayerStartingState GetObjectBuilder()
        {
            MyObjectBuilder_WorldGeneratorPlayerStartingState_RespawnShip objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorPlayerStartingState_RespawnShip;
            objectBuilder.RespawnShip = this.m_respawnShipId;
            return objectBuilder;
        }

        public override Vector3D? GetStartingLocation() => 
            null;

        public override void Init(MyObjectBuilder_WorldGeneratorPlayerStartingState builder)
        {
            base.Init(builder);
            MyObjectBuilder_WorldGeneratorPlayerStartingState_RespawnShip ship = builder as MyObjectBuilder_WorldGeneratorPlayerStartingState_RespawnShip;
            this.m_respawnShipId = ship.RespawnShip;
        }

        public override void SetupCharacter(MyWorldGenerator.Args generatorArgs)
        {
            string respawnShipId = this.m_respawnShipId;
            if (!MyDefinitionManager.Static.HasRespawnShip(this.m_respawnShipId))
            {
                respawnShipId = MyDefinitionManager.Static.GetFirstRespawnShip();
            }
            if (MySession.Static.LocalHumanPlayer != null)
            {
                this.CreateAndSetPlayerFaction();
                Color? color = null;
                long? planetId = null;
                MySpaceRespawnComponent.Static.SpawnAtShip(MySession.Static.LocalHumanPlayer, respawnShipId, null, null, color, planetId);
            }
        }
    }
}


namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemCreateGrid))]
    public class MyToolbarItemCreateGrid : MyToolbarItemDefinition
    {
        private static MyStringHash CreateSmallShip = MyStringHash.GetOrCompute("CreateSmallShip");
        private static MyStringHash CreateLargeShip = MyStringHash.GetOrCompute("CreateLargeShip");
        private static MyStringHash CreateStation = MyStringHash.GetOrCompute("CreateStation");

        public override bool Activate()
        {
            if (base.Definition.Id.SubtypeId == CreateStation)
            {
                this.CreateGrid(MyCubeSize.Large, true);
            }
            return false;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || (type == MyToolbarType.Spectator));

        private void CreateGrid(MyCubeSize cubeSize, bool isStatic)
        {
            if (!MyEntities.MemoryLimitReachedReport && !MySandboxGame.IsPaused)
            {
                MySessionComponentVoxelHand.Static.Enabled = false;
                MyCubeBuilder.Static.StartStaticGridPlacement(cubeSize, isStatic);
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter != null)
                {
                    MyDefinitionId weaponDefinition = new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer));
                    localCharacter.SwitchToWeapon(weaponDefinition);
                }
            }
        }

        public override bool Init(MyObjectBuilder_ToolbarItem objBuilder)
        {
            base.Init(objBuilder);
            base.WantsToBeSelected = false;
            base.WantsToBeActivated = true;
            base.ActivateOnClick = true;
            return true;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L) => 
            MyToolbarItem.ChangeInfo.None;
    }
}


namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemPrefabThrower))]
    internal class MyToolbarItemPrefabThrower : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            if (base.Definition == null)
            {
                return false;
            }
            MySessionComponentThrower.Static.Enabled = MyFakes.ENABLE_PREFAB_THROWER;
            MySessionComponentThrower.Static.CurrentDefinition = (MyPrefabThrowerDefinition) base.Definition;
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                controlledEntity.SwitchToWeapon((MyToolbarItemWeapon) null);
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || (type == MyToolbarType.Spectator));

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            return base.Init(data);
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            int num1;
            MyPrefabThrowerDefinition definition = MySessionComponentThrower.Static.Enabled ? MySessionComponentThrower.Static.CurrentDefinition : null;
            if (!MySessionComponentThrower.Static.Enabled || (definition == null))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (definition.Id.SubtypeId == (base.Definition as MyPrefabThrowerDefinition).Id.SubtypeId);
            }
            this.WantsToBeSelected = (bool) num1;
            return MyToolbarItem.ChangeInfo.None;
        }
    }
}


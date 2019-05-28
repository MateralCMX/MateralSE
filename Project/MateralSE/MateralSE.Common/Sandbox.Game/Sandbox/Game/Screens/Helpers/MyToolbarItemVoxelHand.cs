namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemVoxelHand))]
    public class MyToolbarItemVoxelHand : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            if (base.Definition == null)
            {
                return false;
            }
            if (!MySessionComponentVoxelHand.Static.TrySetBrush(base.Definition.Id.SubtypeName))
            {
                return false;
            }
            bool flag = MySession.Static.CreativeMode || MySession.Static.IsUserAdmin(Sync.MyId);
            if (flag)
            {
                MySession.Static.GameFocusManager.Clear();
            }
            MySessionComponentVoxelHand.Static.Enabled = flag;
            if (!MySessionComponentVoxelHand.Static.Enabled)
            {
                return false;
            }
            MySessionComponentVoxelHand.Static.CurrentDefinition = base.Definition as MyVoxelHandDefinition;
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                controlledEntity.SwitchToWeapon((MyToolbarItemWeapon) null);
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || (type == MyToolbarType.Spectator));

        public override bool Init(MyObjectBuilder_ToolbarItem objBuilder)
        {
            base.Init(objBuilder);
            base.WantsToBeSelected = false;
            base.ActivateOnClick = false;
            return true;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            if (MySessionComponentVoxelHand.Static != null)
            {
                int num1;
                MyVoxelHandDefinition definition = MySessionComponentVoxelHand.Static.Enabled ? MySessionComponentVoxelHand.Static.CurrentDefinition : null;
                if (!MySessionComponentVoxelHand.Static.Enabled || (definition == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (definition.Id.SubtypeId == (base.Definition as MyVoxelHandDefinition).Id.SubtypeId);
                }
                this.WantsToBeSelected = (bool) num1;
            }
            return MyToolbarItem.ChangeInfo.None;
        }
    }
}


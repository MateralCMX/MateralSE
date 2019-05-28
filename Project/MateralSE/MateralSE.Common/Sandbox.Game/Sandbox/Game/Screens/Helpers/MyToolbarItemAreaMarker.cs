namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemAreaMarker))]
    public class MyToolbarItemAreaMarker : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            if (!MyFakes.ENABLE_BARBARIANS || !MyPerGameSettings.EnableAi)
            {
                return false;
            }
            if (base.Definition == null)
            {
                return false;
            }
            MyPlaceAreas.Static.AreaMarkerDefinition = base.Definition as MyAreaMarkerDefinition;
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                controlledEntity.SwitchToWeapon((MyToolbarItemWeapon) null);
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || (type == MyToolbarType.Spectator));

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            MyObjectBuilder_ToolbarItem objectBuilder = base.GetObjectBuilder();
            MyObjectBuilder_ToolbarItemAreaMarker marker = objectBuilder as MyObjectBuilder_ToolbarItemAreaMarker;
            return ((marker != null) ? marker : objectBuilder);
        }

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.Init(data);
            base.ActivateOnClick = false;
            return true;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            MyAreaMarkerDefinition areaMarkerDefinition = MyPlaceAreas.Static.AreaMarkerDefinition;
            this.WantsToBeSelected = (areaMarkerDefinition != null) && (areaMarkerDefinition.Id.SubtypeId == (base.Definition as MyAreaMarkerDefinition).Id.SubtypeId);
            return MyToolbarItem.ChangeInfo.None;
        }
    }
}


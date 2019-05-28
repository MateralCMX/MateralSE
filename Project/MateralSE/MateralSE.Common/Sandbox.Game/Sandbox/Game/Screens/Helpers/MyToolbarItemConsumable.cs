namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemConsumable))]
    public class MyToolbarItemConsumable : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            MyFixedPoint a = (this.Inventory != null) ? this.Inventory.GetItemAmount(base.Definition.Id, MyItemFlags.None, false) : 0;
            if (a > 0)
            {
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                a = MyFixedPoint.Min(a, 1);
                if (((controlledEntity != null) && (controlledEntity.StatComp != null)) && (a > 0))
                {
                    this.Inventory.ConsumeItem(base.Definition.Id, a, controlledEntity.EntityId);
                }
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            (type == MyToolbarType.Character);

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            if (base.Definition == null)
            {
                return null;
            }
            MyObjectBuilder_ToolbarItemConsumable consumable1 = (MyObjectBuilder_ToolbarItemConsumable) MyToolbarItemFactory.CreateObjectBuilder(this);
            consumable1.DefinitionId = (SerializableDefinitionId) base.Definition.Id;
            return consumable1;
        }

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            base.WantsToBeActivated = false;
            return base.Init(data);
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            bool newEnabled = (this.Inventory != null) ? (this.Inventory.GetItemAmount(base.Definition.Id, MyItemFlags.None, false) > 0) : false;
            return base.SetEnabled(newEnabled);
        }

        public MyInventory Inventory
        {
            get
            {
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                return ((controlledEntity != null) ? controlledEntity.GetInventory(0) : null);
            }
        }
    }
}


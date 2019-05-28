namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemUsable))]
    public class MyToolbarItemUsable : MyToolbarItemDefinition
    {
        private MyFixedPoint m_lastAmount = 0;

        public override bool Activate()
        {
            MyFixedPoint a = (this.Inventory != null) ? this.Inventory.GetItemAmount(base.Definition.Id, MyItemFlags.None, false) : 0;
            if (a > 0)
            {
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                a = MyFixedPoint.Min(a, 1);
                if ((controlledEntity != null) && (a > 0))
                {
                    this.Inventory.ConsumeItem(base.Definition.Id, a, controlledEntity.EntityId);
                }
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            (type == MyToolbarType.Character);

        public override void FillGridItem(MyGuiGridItem gridItem)
        {
            if (this.m_lastAmount > 0)
            {
                gridItem.AddText($"{this.m_lastAmount}x", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
            else
            {
                gridItem.ClearText(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
        }

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            if (base.Definition == null)
            {
                return null;
            }
            MyObjectBuilder_ToolbarItemUsable usable1 = (MyObjectBuilder_ToolbarItemUsable) MyToolbarItemFactory.CreateObjectBuilder(this);
            usable1.DefinitionId = (SerializableDefinitionId) base.Definition.Id;
            return usable1;
        }

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            base.WantsToBeActivated = false;
            return base.Init(data);
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            MyToolbarItem.ChangeInfo none = MyToolbarItem.ChangeInfo.None;
            if (localCharacter != null)
            {
                MyInventory inventory = localCharacter.GetInventory(0);
                MyFixedPoint point = (inventory != null) ? inventory.GetItemAmount(base.Definition.Id, MyItemFlags.None, false) : 0;
                if (this.m_lastAmount != point)
                {
                    this.m_lastAmount = point;
                    none |= MyToolbarItem.ChangeInfo.IconText;
                }
            }
            bool newEnabled = this.m_lastAmount > 0;
            return (none | base.SetEnabled(newEnabled));
        }

        public MyInventory Inventory
        {
            get
            {
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                return ((controlledEntity != null) ? controlledEntity.GetInventory(0) : null);
            }
        }

        public MyFixedPoint Amount =>
            this.m_lastAmount;
    }
}


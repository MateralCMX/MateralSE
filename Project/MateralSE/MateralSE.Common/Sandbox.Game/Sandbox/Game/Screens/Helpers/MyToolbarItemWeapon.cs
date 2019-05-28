namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemWeapon))]
    public class MyToolbarItemWeapon : MyToolbarItemDefinition
    {
        protected int m_lastAmmoCount = -1;
        protected bool m_needsWeaponSwitching = true;
        protected string m_lastTextValue = string.Empty;

        public override bool Activate()
        {
            if (base.Definition == null)
            {
                return false;
            }
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                if (!this.m_needsWeaponSwitching)
                {
                    controlledEntity.SwitchAmmoMagazine();
                }
                else
                {
                    controlledEntity.SwitchToWeapon(this);
                    base.WantsToBeActivated = true;
                }
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            true;

        public override bool Equals(object obj)
        {
            bool flag = base.Equals(obj);
            if (flag && !(obj is MyToolbarItemWeapon))
            {
                flag = false;
            }
            return flag;
        }

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder() => 
            ((MyObjectBuilder_ToolbarItemWeapon) base.GetObjectBuilder());

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            return base.Init(data);
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            int num1;
            bool flag = false;
            bool flag2 = false;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                num1 = (localCharacter.FindWeaponItemByDefinition(base.Definition.Id) != null) ? 1 : ((int) !localCharacter.WeaponTakesBuilderFromInventory(new MyDefinitionId?(base.Definition.Id)));
            }
            else
            {
                num1 = 0;
            }
            bool flag3 = (bool) num1;
            MyToolbarItem.ChangeInfo none = MyToolbarItem.ChangeInfo.None;
            if (flag3)
            {
                IMyGunObject<MyDeviceBase> currentWeapon = localCharacter.CurrentWeapon;
                if (currentWeapon != null)
                {
                    flag = MyDefinitionManager.Static.GetPhysicalItemForHandItem(currentWeapon.DefinitionId).Id == base.Definition.Id;
                }
                if (localCharacter.LeftHandItem != null)
                {
                    flag |= ReferenceEquals(base.Definition, localCharacter.LeftHandItem.PhysicalItemDefinition);
                }
                if (flag && (currentWeapon != null))
                {
                    MyWeaponItemDefinition physicalItemForHandItem = MyDefinitionManager.Static.GetPhysicalItemForHandItem(currentWeapon.DefinitionId) as MyWeaponItemDefinition;
                    if ((physicalItemForHandItem != null) && physicalItemForHandItem.ShowAmmoCount)
                    {
                        int ammunitionAmount = localCharacter.CurrentWeapon.GetAmmunitionAmount();
                        if (this.m_lastAmmoCount != ammunitionAmount)
                        {
                            this.m_lastAmmoCount = ammunitionAmount;
                            base.IconText.Clear().AppendInt32(ammunitionAmount);
                            none |= MyToolbarItem.ChangeInfo.IconText;
                        }
                    }
                }
            }
            MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
            if ((controlledEntity != null) && (controlledEntity.GridSelectionSystem.WeaponSystem != null))
            {
                flag2 = controlledEntity.GridSelectionSystem.WeaponSystem.HasGunsOfId(base.Definition.Id);
                if (flag2 && (controlledEntity.GridSelectionSystem.WeaponSystem.GetGun(base.Definition.Id).GunBase is MyGunBase))
                {
                    int number = 0;
                    foreach (IMyGunObject<MyDeviceBase> obj3 in controlledEntity.GridSelectionSystem.WeaponSystem.GetGunsById(base.Definition.Id))
                    {
                        number += obj3.GetAmmunitionAmount();
                    }
                    if (number != this.m_lastAmmoCount)
                    {
                        this.m_lastAmmoCount = number;
                        base.IconText.Clear().AppendInt32(number);
                        none |= MyToolbarItem.ChangeInfo.IconText;
                    }
                }
                MyDefinitionId? gunId = controlledEntity.GridSelectionSystem.GetGunId();
                MyDefinitionId id = base.Definition.Id;
                flag = (gunId != null) ? ((gunId != null) ? (gunId.GetValueOrDefault() == id) : true) : false;
            }
            none |= base.SetEnabled(flag3 | flag2);
            base.WantsToBeSelected = flag;
            this.m_needsWeaponSwitching = !flag;
            if (this.m_lastTextValue != base.IconText.ToString())
            {
                none |= MyToolbarItem.ChangeInfo.IconText;
            }
            this.m_lastTextValue = base.IconText.ToString();
            return none;
        }

        public int AmmoCount =>
            this.m_lastAmmoCount;
    }
}


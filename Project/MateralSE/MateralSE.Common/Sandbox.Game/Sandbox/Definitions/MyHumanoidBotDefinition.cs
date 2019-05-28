namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_HumanoidBotDefinition), (Type) null)]
    public class MyHumanoidBotDefinition : MyAgentDefinition
    {
        public MyDefinitionId StartingWeaponDefinitionId;
        public List<MyDefinitionId> InventoryItems;

        public override void AddItems(MyCharacter character)
        {
            MyWeaponDefinition definition;
            base.AddItems(character);
            MyObjectBuilder_PhysicalGunObject objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalGunObject>(this.StartingWeaponDefinitionId.SubtypeName);
            if (character.WeaponTakesBuilderFromInventory(new MyDefinitionId?(this.StartingWeaponDefinitionId)))
            {
                character.GetInventory(0).AddItems(1, objectBuilder);
            }
            foreach (MyDefinitionId id in this.InventoryItems)
            {
                objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalGunObject>(id.SubtypeName);
                character.GetInventory(0).AddItems(1, objectBuilder);
            }
            character.SwitchToWeapon(this.StartingWeaponDefinitionId);
            MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), this.StartingWeaponDefinitionId.SubtypeName);
            if (MyDefinitionManager.Static.TryGetWeaponDefinition(defId, out definition) && definition.HasAmmoMagazines())
            {
                MyObjectBuilder_AmmoMagazine magazine = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_AmmoMagazine>(definition.AmmoMagazinesId[0].SubtypeName);
                character.GetInventory(0).AddItems(3, magazine);
            }
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_HumanoidBotDefinition definition = builder as MyObjectBuilder_HumanoidBotDefinition;
            if ((definition.StartingItem != null) && !string.IsNullOrWhiteSpace(definition.StartingItem.Subtype))
            {
                this.StartingWeaponDefinitionId = new MyDefinitionId(definition.StartingItem.Type, definition.StartingItem.Subtype);
            }
            this.InventoryItems = new List<MyDefinitionId>();
            if (definition.InventoryItems != null)
            {
                foreach (MyObjectBuilder_HumanoidBotDefinition.Item item in definition.InventoryItems)
                {
                    this.InventoryItems.Add(new MyDefinitionId(item.Type, item.Subtype));
                }
            }
        }
    }
}


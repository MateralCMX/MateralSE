namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;

    [MyComponentBuilder(typeof(MyObjectBuilder_CraftingComponentInteractive), true)]
    public class MyCraftingComponentInteractive : MyCraftingComponentBase, IMyEventProxy, IMyEventOwner
    {
        private int m_lastUpdateTime;
        private bool m_productionEnabled;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private VRage.ModAPI.IMyEntity m_lastEntityInteraction;

        protected override void AddProducedItemToInventory(MyBlueprintDefinitionBase definition, MyFixedPoint amountMult)
        {
            if (Sync.IsServer)
            {
                MyInventory inventory = null;
                MyInventory inventory2 = (base.Entity as MyEntity).GetInventory(0);
                if (this.m_lastEntityInteraction == null)
                {
                    if (inventory2 == null)
                    {
                        return;
                    }
                    foreach (MyBlueprintDefinitionBase.Item item3 in definition.Results)
                    {
                        MyFixedPoint amount = item3.Amount * amountMult;
                        IMyInventoryItem item = base.CreateInventoryItem(item3.Id, amount);
                        inventory2.Add(item, item.Amount);
                    }
                }
                else
                {
                    inventory = (this.m_lastEntityInteraction as MyEntity).GetInventory(0);
                    if (inventory != null)
                    {
                        foreach (MyBlueprintDefinitionBase.Item item in definition.Results)
                        {
                            MyFixedPoint amount = item.Amount * amountMult;
                            IMyInventoryItem item2 = base.CreateInventoryItem(item.Id, amount);
                            if (!inventory.Add(item2, item2.Amount))
                            {
                                inventory2.Add(item2, item2.Amount);
                            }
                        }
                    }
                }
                this.m_lastEntityInteraction = null;
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyCraftingComponentInteractiveDefinition definition2 = definition as MyCraftingComponentInteractiveDefinition;
            if (definition2 != null)
            {
                this.ActionSound = new MySoundPair(definition2.ActionSound, true);
                base.m_craftingSpeedMultiplier = definition2.CraftingSpeedMultiplier;
                foreach (string str in definition2.AvailableBlueprintClasses)
                {
                    MyBlueprintClassDefinition blueprintClass = MyDefinitionManager.Static.GetBlueprintClass(str);
                    base.m_blueprintClasses.Add(blueprintClass);
                }
            }
        }

        public override bool IsSerialized() => 
            true;

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false) => 
            (base.Serialize(false) as MyObjectBuilder_CraftingComponentInteractive);

        public void SetLastEntityInteraction(VRage.ModAPI.IMyEntity entity)
        {
            this.m_lastEntityInteraction = entity;
        }

        protected override void StartProduction_Implementation()
        {
            base.StartProduction_Implementation();
            MyEntity entity = base.Entity as MyEntity;
            if (entity != null)
            {
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
            this.m_productionEnabled = true;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        protected override void StopProduction_Implementation()
        {
            base.StopOperating_Implementation();
            this.m_productionEnabled = false;
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            base.m_elapsedTimeMs = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (base.IsProductionDone || !this.CanOperate)
            {
                this.StopProduction_Implementation();
            }
            else
            {
                this.UpdateProduction_Implementation();
                if (!base.IsProducing)
                {
                    this.StopProduction_Implementation();
                }
            }
        }

        public override void UpdateCurrentItemStatus(float statusDelta)
        {
            if (base.IsProducing)
            {
                MyCraftingComponentBase.MyBlueprintToProduce itemToProduce = base.GetItemToProduce(base.m_currentItem);
                if (itemToProduce != null)
                {
                    MyBlueprintDefinitionBase blueprint = itemToProduce.Blueprint;
                    base.m_currentItemStatus = Math.Min((float) 1f, (float) (base.m_currentItemStatus + ((statusDelta * base.m_craftingSpeedMultiplier) / (blueprint.BaseProductionTimeInSeconds * 1000f))));
                }
            }
        }

        protected override void UpdateProduction_Implementation()
        {
            if (this.m_productionEnabled)
            {
                if (base.IsProducing)
                {
                    base.UpdateCurrentItem();
                    this.UpdateProductionSound();
                }
                else if (!base.IsProductionDone)
                {
                    base.SelectItemToProduction();
                    if (base.m_currentItem != -1)
                    {
                        base.UpdateCurrentItem();
                        this.UpdateProductionSound();
                    }
                }
                if (!base.IsProducing && (this.m_soundEmitter != null))
                {
                    this.m_soundEmitter.StopSound(true, true);
                }
            }
        }

        private void UpdateProductionSound()
        {
            if (this.m_soundEmitter == null)
            {
                this.m_soundEmitter = new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f);
            }
            if (base.m_currentItemStatus >= 1f)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
            else
            {
                bool? nullable;
                MyCraftingComponentBase.MyBlueprintToProduce currentItemInProduction = base.GetCurrentItemInProduction();
                if ((currentItemInProduction == null) || (currentItemInProduction.Blueprint.ProgressBarSoundCue == null))
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySingleSound(this.ActionSound, false, false, false, nullable);
                }
                else
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySingleSound(MySoundPair.GetCueId(currentItemInProduction.Blueprint.ProgressBarSoundCue), false, false, nullable);
                }
            }
        }

        public MySoundPair ActionSound { get; set; }

        public override string ComponentTypeDebugString =>
            "Interactive crafting component";

        public override string DisplayNameText =>
            (base.Entity as MyEntity).DisplayNameText;

        public override bool RequiresItemsToOperate =>
            false;

        public override bool CanOperate =>
            true;
    }
}


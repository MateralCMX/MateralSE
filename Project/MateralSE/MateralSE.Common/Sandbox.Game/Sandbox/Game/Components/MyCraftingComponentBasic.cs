namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;

    [MyComponentBuilder(typeof(MyObjectBuilder_CraftingComponentBasic), true), MyComponentBuilder(typeof(MyObjectBuilder_CraftingComponentCharacter), false)]
    public class MyCraftingComponentBasic : MyCraftingComponentBase, IMyEventProxy, IMyEventOwner
    {
        private int m_lastUpdateTime;
        private MyEntity3DSoundEmitter m_soundEmitter;

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyCraftingComponentBasicDefinition definition2 = definition as MyCraftingComponentBasicDefinition;
            if (definition2 != null)
            {
                this.ActionSound = new MySoundPair(definition2.ActionSound, true);
                base.m_craftingSpeedMultiplier = definition2.CraftingSpeedMultiplier;
                foreach (string str in definition2.AvailableBlueprintClasses)
                {
                    MyBlueprintClassDefinition blueprintClass = MyDefinitionManager.Static.GetBlueprintClass(str);
                    if (blueprintClass != null)
                    {
                        base.m_blueprintClasses.Add(blueprintClass);
                    }
                }
            }
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            MyEntity entity = base.Entity as MyEntity;
            if (entity != null)
            {
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false) => 
            (base.Serialize(false) as MyObjectBuilder_CraftingComponentBasic);

        protected override void StartProduction_Implementation()
        {
            base.StartProduction_Implementation();
            MyEntity entity = base.Entity as MyEntity;
            if (entity != null)
            {
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        protected override void StopProduction_Implementation()
        {
            base.StopOperating_Implementation();
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

        protected override void UpdateProduction_Implementation()
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
            "Character crafting component";

        public override string DisplayNameText =>
            (base.Entity as MyEntity).DisplayNameText;

        public override bool RequiresItemsToOperate =>
            false;

        public override bool CanOperate =>
            true;
    }
}


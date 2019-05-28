namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage.Game.Components;
    using VRage.ModAPI;

    public abstract class MyCharacterComponent : MyEntityComponentBase
    {
        private bool m_needsUpdateAfterSimulation;
        private bool m_needsUpdateSimulation;
        private bool m_needsUpdateAfterSimulation10;
        private bool m_needsUpdateBeforeSimulation100;
        private bool m_needsUpdateBeforeSimulation;

        protected MyCharacterComponent()
        {
        }

        public virtual void OnCharacterDead()
        {
        }

        public virtual void Simulate()
        {
        }

        public virtual void UpdateAfterSimulation()
        {
        }

        public virtual void UpdateAfterSimulation10()
        {
        }

        public virtual void UpdateBeforeSimulation()
        {
        }

        public virtual void UpdateBeforeSimulation100()
        {
        }

        public bool NeedsUpdateAfterSimulation
        {
            get => 
                this.m_needsUpdateAfterSimulation;
            set
            {
                this.m_needsUpdateAfterSimulation = value;
                IMyEntity entity = base.Entity;
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public bool NeedsUpdateSimulation
        {
            get => 
                this.m_needsUpdateSimulation;
            set
            {
                this.m_needsUpdateSimulation = value;
                IMyEntity entity = base.Entity;
                entity.NeedsUpdate |= MyEntityUpdateEnum.SIMULATE;
            }
        }

        public bool NeedsUpdateAfterSimulation10
        {
            get => 
                this.m_needsUpdateAfterSimulation10;
            set
            {
                this.m_needsUpdateAfterSimulation10 = value;
                IMyEntity entity = base.Entity;
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        public bool NeedsUpdateBeforeSimulation100
        {
            get => 
                this.m_needsUpdateBeforeSimulation100;
            set
            {
                this.m_needsUpdateBeforeSimulation100 = value;
                IMyEntity entity = base.Entity;
                entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
        }

        public bool NeedsUpdateBeforeSimulation
        {
            get => 
                this.m_needsUpdateBeforeSimulation;
            set
            {
                this.m_needsUpdateBeforeSimulation = value;
                IMyEntity entity = base.Entity;
                entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public MyCharacter Character =>
            ((MyCharacter) base.Entity);

        public override string ComponentTypeDebugString =>
            "Character Component";
    }
}


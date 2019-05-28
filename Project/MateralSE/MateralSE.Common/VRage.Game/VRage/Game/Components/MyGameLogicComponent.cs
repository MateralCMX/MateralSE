namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    public abstract class MyGameLogicComponent : MyEntityComponentBase, IMyGameLogicComponent
    {
        private MyEntityUpdateEnum m_needsUpdate;
        private bool m_entityUpdate;

        protected MyGameLogicComponent()
        {
        }

        public virtual void Close()
        {
        }

        public virtual MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public virtual void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
        }

        public virtual void MarkForClose()
        {
        }

        public virtual void UpdateAfterSimulation()
        {
        }

        public virtual void UpdateAfterSimulation10()
        {
        }

        public virtual void UpdateAfterSimulation100()
        {
        }

        public virtual void UpdateBeforeSimulation()
        {
        }

        public virtual void UpdateBeforeSimulation10()
        {
        }

        public virtual void UpdateBeforeSimulation100()
        {
        }

        public virtual void UpdateOnceBeforeFrame()
        {
        }

        public virtual void UpdatingStopped()
        {
        }

        void IMyGameLogicComponent.Close()
        {
            MyGameLogic.UnregisterForUpdate(this);
            this.Close();
        }

        void IMyGameLogicComponent.RegisterForUpdate()
        {
            MyGameLogic.RegisterForUpdate(this);
        }

        void IMyGameLogicComponent.UnregisterForUpdate()
        {
            MyGameLogic.UnregisterForUpdate(this);
        }

        void IMyGameLogicComponent.UpdateAfterSimulation(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateAfterSimulation();
            }
        }

        void IMyGameLogicComponent.UpdateAfterSimulation10(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateAfterSimulation10();
            }
        }

        void IMyGameLogicComponent.UpdateAfterSimulation100(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateAfterSimulation100();
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateBeforeSimulation();
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation10(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateBeforeSimulation10();
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation100(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateBeforeSimulation100();
            }
        }

        void IMyGameLogicComponent.UpdateOnceBeforeFrame(bool entityUpdate)
        {
            if (entityUpdate == this.m_entityUpdate)
            {
                this.UpdateOnceBeforeFrame();
            }
        }

        bool IMyGameLogicComponent.EntityUpdate
        {
            get => 
                this.m_entityUpdate;
            set => 
                (this.m_entityUpdate = value);
        }

        public MyEntityUpdateEnum NeedsUpdate
        {
            get
            {
                if (!this.m_entityUpdate)
                {
                    return this.m_needsUpdate;
                }
                MyEntityUpdateEnum nONE = MyEntityUpdateEnum.NONE;
                if ((base.Entity.Flags & EntityFlags.NeedsUpdate) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_FRAME;
                }
                if ((base.Entity.Flags & EntityFlags.NeedsUpdate10) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                }
                if ((base.Entity.Flags & EntityFlags.NeedsUpdate100) != 0)
                {
                    nONE |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                }
                if ((base.Entity.Flags & EntityFlags.NeedsUpdateBeforeNextFrame) != 0)
                {
                    nONE |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                return nONE;
            }
            set
            {
                if (value != this.NeedsUpdate)
                {
                    if (!this.m_entityUpdate)
                    {
                        if (base.Entity.InScene)
                        {
                            MyGameLogic.ChangeUpdate(this, value, false);
                        }
                        this.m_needsUpdate = value;
                    }
                    else
                    {
                        if (base.Entity.InScene)
                        {
                            MyAPIGatewayShortcuts.UnregisterEntityUpdate(base.Entity, false);
                        }
                        IMyEntity entity = base.Entity;
                        entity.Flags &= ~EntityFlags.NeedsUpdateBeforeNextFrame;
                        IMyEntity entity2 = base.Entity;
                        entity2.Flags &= ~EntityFlags.NeedsUpdate;
                        IMyEntity entity3 = base.Entity;
                        entity3.Flags &= ~EntityFlags.NeedsUpdate10;
                        IMyEntity entity4 = base.Entity;
                        entity4.Flags &= ~EntityFlags.NeedsUpdate100;
                        if ((value & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            IMyEntity entity5 = base.Entity;
                            entity5.Flags |= EntityFlags.NeedsUpdateBeforeNextFrame;
                        }
                        if ((value & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            IMyEntity entity6 = base.Entity;
                            entity6.Flags |= EntityFlags.NeedsUpdate;
                        }
                        if ((value & MyEntityUpdateEnum.EACH_10TH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            IMyEntity entity7 = base.Entity;
                            entity7.Flags |= EntityFlags.NeedsUpdate10;
                        }
                        if ((value & MyEntityUpdateEnum.EACH_100TH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            IMyEntity entity8 = base.Entity;
                            entity8.Flags |= EntityFlags.NeedsUpdate100;
                        }
                        if (base.Entity.InScene)
                        {
                            MyAPIGatewayShortcuts.RegisterEntityUpdate(base.Entity);
                        }
                    }
                }
            }
        }

        [XmlIgnore]
        public bool Closed { get; protected set; }

        [XmlIgnore]
        public bool MarkedForClose { get; protected set; }

        public override string ComponentTypeDebugString =>
            "Game Logic";
    }
}


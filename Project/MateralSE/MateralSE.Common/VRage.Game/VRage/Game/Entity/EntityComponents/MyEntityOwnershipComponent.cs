namespace VRage.Game.Entity.EntityComponents
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyComponentType(typeof(MyEntityOwnershipComponent)), MyComponentBuilder(typeof(MyObjectBuilder_EntityOwnershipComponent), true)]
    public class MyEntityOwnershipComponent : MyEntityComponentBase
    {
        private long m_ownerId;
        private MyOwnershipShareModeEnum m_shareMode = MyOwnershipShareModeEnum.All;
        public Action<long, long> OwnerChanged;
        public Action<MyOwnershipShareModeEnum> ShareModeChanged;

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_EntityOwnershipComponent component = builder as MyObjectBuilder_EntityOwnershipComponent;
            this.m_ownerId = component.OwnerId;
            this.m_shareMode = component.ShareMode;
        }

        public override bool IsSerialized() => 
            true;

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_EntityOwnershipComponent component1 = base.Serialize(false) as MyObjectBuilder_EntityOwnershipComponent;
            component1.OwnerId = this.m_ownerId;
            component1.ShareMode = this.m_shareMode;
            return component1;
        }

        public long OwnerId
        {
            get => 
                this.m_ownerId;
            set
            {
                if ((this.m_ownerId != value) && (this.OwnerChanged != null))
                {
                    this.OwnerChanged(this.m_ownerId, value);
                }
                this.m_ownerId = value;
            }
        }

        public MyOwnershipShareModeEnum ShareMode
        {
            get => 
                this.m_shareMode;
            set
            {
                if ((this.m_shareMode != value) && (this.ShareModeChanged != null))
                {
                    this.ShareModeChanged(value);
                }
                this.m_shareMode = value;
            }
        }

        public override string ComponentTypeDebugString =>
            base.GetType().Name;
    }
}


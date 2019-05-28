namespace VRage.Network
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;

    public abstract class MyClientStateBase
    {
        public MyTimeSpan ClientTimeStamp;
        private short m_ping;

        protected MyClientStateBase()
        {
        }

        public abstract void ResetControlledEntityControls();
        public abstract void Serialize(BitStream stream, bool outOfOrder);
        public abstract void Update();

        public Endpoint EndpointId { get; set; }

        public int PlayerSerialId { get; set; }

        public virtual Vector3D? Position { get; protected set; }

        public short Ping
        {
            get => 
                this.m_ping;
            set => 
                (this.m_ping = value);
        }

        public abstract IMyReplicable ControlledReplicable { get; }

        public abstract IMyReplicable CharacterReplicable { get; }

        public bool IsControllingCharacter { get; protected set; }

        public bool IsControllingJetpack { get; protected set; }

        public bool IsControllingGrid { get; protected set; }
    }
}


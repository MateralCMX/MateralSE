namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GunBase : MyObjectBuilder_DeviceBase
    {
        private SerializableDictionary<string, int> m_remainingAmmos;
        [ProtoMember(0x34), DefaultValue(0)]
        public int RemainingAmmo;
        [ProtoMember(0x37), DefaultValue("")]
        public string CurrentAmmoMagazineName = "";
        [ProtoMember(0x3a)]
        public List<RemainingAmmoIns> RemainingAmmosList = new List<RemainingAmmoIns>();
        [ProtoMember(0x3d)]
        public long LastShootTime;

        public bool ShouldSerializeRemainingAmmos() => 
            false;

        [NoSerialize]
        public SerializableDictionary<string, int> RemainingAmmos
        {
            get => 
                this.m_remainingAmmos;
            set
            {
                this.m_remainingAmmos = value;
                if (this.RemainingAmmosList == null)
                {
                    this.RemainingAmmosList = new List<RemainingAmmoIns>();
                }
                foreach (KeyValuePair<string, int> pair in value.Dictionary)
                {
                    RemainingAmmoIns item = new RemainingAmmoIns {
                        SubtypeName = pair.Key,
                        Amount = pair.Value
                    };
                    this.RemainingAmmosList.Add(item);
                }
            }
        }

        [ProtoContract]
        public class RemainingAmmoIns
        {
            [ProtoMember(0x13), XmlAttribute, Nullable]
            public string SubtypeName;
            [ProtoMember(0x18), XmlAttribute]
            public int Amount;
        }
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, "EventDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GlobalEventDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x26)]
        public long? MinActivationTimeMs;
        [ProtoMember(0x2a)]
        public long? MaxActivationTimeMs;
        [ProtoMember(0x2e)]
        public long? FirstActivationTimeMs;

        public bool ShouldSerializeFirstActivationTime() => 
            (this.FirstActivationTimeMs != null);

        public bool ShouldSerializeMaxActivationTime() => 
            (this.MaxActivationTimeMs != null);

        public bool ShouldSerializeMinActivationTime() => 
            (this.MinActivationTimeMs != null);

        [ProtoMember(0x1f)]
        private MyGlobalEventTypeEnum EventType
        {
            get => 
                MyGlobalEventTypeEnum.InvalidEventType;
            set
            {
            }
        }
    }
}


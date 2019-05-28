namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_TriggerAggregate : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(14), DefaultValue((string) null), DynamicObjectBuilderItem(false), Serialize(MyObjectFlags.DefaultZero), XmlElement("AreaTriggers", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_TriggerBase>))]
        public List<MyObjectBuilder_TriggerBase> AreaTriggers;
    }
}


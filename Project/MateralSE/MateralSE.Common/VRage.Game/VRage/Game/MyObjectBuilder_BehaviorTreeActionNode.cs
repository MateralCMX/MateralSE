namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BehaviorTreeActionNode : MyObjectBuilder_BehaviorTreeNode
    {
        [ProtoMember(0x62)]
        public string ActionName;
        [ProtoMember(0x65), XmlArrayItem("Parameter", Type=typeof(MyAbstractXmlSerializer<TypeValue>))]
        public TypeValue[] Parameters;

        [ProtoContract, XmlType("BoolType")]
        public class BoolType : MyObjectBuilder_BehaviorTreeActionNode.TypeValue
        {
            [XmlAttribute, ProtoMember(0x4b)]
            public bool BoolValue;

            public override object GetValue() => 
                this.BoolValue;
        }

        [ProtoContract, XmlType("FloatType")]
        public class FloatType : MyObjectBuilder_BehaviorTreeActionNode.TypeValue
        {
            [XmlAttribute, ProtoMember(0x3d)]
            public float FloatValue;

            public override object GetValue() => 
                this.FloatValue;
        }

        [ProtoContract, XmlType("IntType")]
        public class IntType : MyObjectBuilder_BehaviorTreeActionNode.TypeValue
        {
            [XmlAttribute, ProtoMember(0x22)]
            public int IntValue;

            public override object GetValue() => 
                this.IntValue;
        }

        [ProtoContract, XmlType("MemType")]
        public class MemType : MyObjectBuilder_BehaviorTreeActionNode.TypeValue
        {
            [XmlAttribute, ProtoMember(0x59)]
            public string MemName;

            public override object GetValue() => 
                this.MemName;
        }

        [ProtoContract, XmlType("StringType")]
        public class StringType : MyObjectBuilder_BehaviorTreeActionNode.TypeValue
        {
            [XmlAttribute, ProtoMember(0x2f)]
            public string StringValue;

            public override object GetValue() => 
                this.StringValue;
        }

        [ProtoContract, XmlType("TypeValue")]
        public abstract class TypeValue
        {
            protected TypeValue()
            {
            }

            public abstract object GetValue();
        }
    }
}


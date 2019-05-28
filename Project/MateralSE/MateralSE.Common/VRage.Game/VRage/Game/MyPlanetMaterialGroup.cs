namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [XmlType("MaterialGroup"), ProtoContract]
    public class MyPlanetMaterialGroup : ICloneable
    {
        [ProtoMember(0xc1), XmlAttribute(AttributeName="Value")]
        public byte Value;
        [ProtoMember(0xc5), XmlAttribute(AttributeName="Name")]
        public string Name = "Default";
        [ProtoMember(0xc9), XmlElement("Rule")]
        public MyPlanetMaterialPlacementRule[] MaterialRules;

        public object Clone()
        {
            MyPlanetMaterialGroup group = new MyPlanetMaterialGroup {
                Value = this.Value,
                Name = this.Name,
                MaterialRules = new MyPlanetMaterialPlacementRule[this.MaterialRules.Length]
            };
            for (int i = 0; i < this.MaterialRules.Length; i++)
            {
                group.MaterialRules[i] = this.MaterialRules[i].Clone() as MyPlanetMaterialPlacementRule;
            }
            return group;
        }
    }
}


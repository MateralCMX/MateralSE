namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Ore : MyObjectBuilder_PhysicalObject
    {
        [Nullable, XmlIgnore]
        public MyStringHash? MaterialTypeName;
        [XmlIgnore, NoSerialize]
        private string m_materialName;

        public override MyObjectBuilder_Base Clone()
        {
            MyObjectBuilder_Ore ore1 = MyObjectBuilderSerializer.Clone(this) as MyObjectBuilder_Ore;
            ore1.MaterialTypeName = this.MaterialTypeName;
            return ore1;
        }

        public string GetMaterialName()
        {
            if (!string.IsNullOrEmpty(this.m_materialName))
            {
                return this.m_materialName;
            }
            if (this.MaterialTypeName != null)
            {
                return this.MaterialTypeName.Value.String;
            }
            return string.Empty;
        }

        public bool HasMaterialName() => 
            (!string.IsNullOrEmpty(this.m_materialName) || ((this.MaterialTypeName != null) && (this.MaterialTypeName.Value.GetHashCode() != 0)));

        [NoSerialize, ProtoMember(20, IsRequired=false)]
        public string MaterialNameString
        {
            get
            {
                if ((this.MaterialTypeName == null) || (this.MaterialTypeName.Value.GetHashCode() == 0))
                {
                    return this.m_materialName;
                }
                return this.MaterialTypeName.Value.String;
            }
            set => 
                (this.m_materialName = value);
        }
    }
}


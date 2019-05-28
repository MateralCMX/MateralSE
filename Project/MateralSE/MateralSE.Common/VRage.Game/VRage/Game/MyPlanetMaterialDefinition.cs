namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyPlanetMaterialDefinition : ICloneable
    {
        [ProtoMember(0x21), XmlAttribute(AttributeName="Material")]
        public string Material;
        [ProtoMember(0x25), XmlAttribute(AttributeName="Value")]
        public byte Value;
        [ProtoMember(0x29), XmlAttribute(AttributeName="MaxDepth")]
        public float MaxDepth = 1f;
        [ProtoMember(0x2d), XmlArrayItem("Layer")]
        public MyPlanetMaterialLayer[] Layers;

        public object Clone() => 
            new MyPlanetMaterialDefinition { 
                Material = this.Material,
                Value = this.Value,
                MaxDepth = this.MaxDepth,
                Layers = (this.Layers == null) ? null : (this.Layers.Clone() as MyPlanetMaterialLayer[])
            };

        public virtual bool IsRule =>
            false;

        public bool HasLayers =>
            ((this.Layers != null) && (this.Layers.Length != 0));

        public string FirstOrDefault =>
            ((this.Material == null) ? (!this.HasLayers ? null : this.Layers[0].Material) : this.Material);
    }
}


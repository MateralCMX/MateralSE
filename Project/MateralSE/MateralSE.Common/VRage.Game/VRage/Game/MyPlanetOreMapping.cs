namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyPlanetOreMapping
    {
        [ProtoMember(220), XmlAttribute(AttributeName="Value")]
        public byte Value;
        [ProtoMember(0xe0), XmlAttribute(AttributeName="Type")]
        public string Type;
        [ProtoMember(0xe4), XmlAttribute(AttributeName="Start")]
        public float Start = 5f;
        [ProtoMember(0xe8), XmlAttribute(AttributeName="Depth")]
        public float Depth = 10f;
        [ProtoMember(0xec), XmlIgnore]
        public ColorDefinitionRGBA? ColorShift;
        private float? m_colorInfluence;

        public override bool Equals(object obj) => 
            ((obj != null) ? (!ReferenceEquals(this, obj) ? (!(obj.GetType() != base.GetType()) ? this.Equals((MyPlanetOreMapping) obj) : false) : true) : false);

        protected bool Equals(MyPlanetOreMapping other) => 
            (this.Value == other.Value);

        public override int GetHashCode() => 
            this.Value.GetHashCode();

        [ProtoMember(240), XmlAttribute("TargetColor")]
        public string TargetColor
        {
            get => 
                this.ColorShift?.Value.Hex;
            set => 
                (this.ColorShift = new ColorDefinitionRGBA(value));
        }

        [ProtoMember(0xf6), XmlAttribute("ColorInfluence")]
        public float ColorInfluence
        {
            get
            {
                float? colorInfluence = this.m_colorInfluence;
                return ((colorInfluence != null) ? colorInfluence.GetValueOrDefault() : 0f);
            }
            set => 
                (this.m_colorInfluence = new float?(value));
        }
    }
}


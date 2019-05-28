namespace VRage.Game.ObjectBuilders.Animation
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationSMVariable : MyObjectBuilder_Base
    {
        public override string ToString() => 
            (this.Name + "=" + this.Value);

        [ProtoMember(12), Description("Name of target variable."), ReadOnly(true)]
        public string Name { get; set; }

        [ProtoMember(0x11), Description("Float value to setup.")]
        public float Value { get; set; }

        [Browsable(false)]
        public MyStringHash SubtypeId =>
            base.SubtypeId;

        [Browsable(false)]
        public string SubtypeName
        {
            get => 
                base.SubtypeName;
            set => 
                (base.SubtypeName = value);
        }

        [Browsable(false)]
        public MyObjectBuilderType TypeId =>
            base.TypeId;
    }
}


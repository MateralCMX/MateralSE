namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public abstract class MyObjectBuilder_GuiControlBase : MyObjectBuilder_Base
    {
        [ProtoMember(13)]
        public Vector2 Position;
        [ProtoMember(0x10)]
        public Vector2 Size;
        [ProtoMember(0x13)]
        public string Name;
        [ProtoMember(0x16)]
        public Vector4 BackgroundColor = Vector4.One;
        [ProtoMember(0x19)]
        public string ControlTexture;
        [ProtoMember(0x1c)]
        public MyGuiDrawAlignEnum OriginAlign;

        protected MyObjectBuilder_GuiControlBase()
        {
        }

        public bool ShouldSerializeControlAlign() => 
            false;

        public int ControlAlign
        {
            get => 
                ((int) this.OriginAlign);
            set => 
                (this.OriginAlign = (MyGuiDrawAlignEnum) value);
        }
    }
}


namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CompoundCubeBlock : MyObjectBuilder_CubeBlock
    {
        [ProtoMember(15), DynamicItem(typeof(MyObjectBuilderDynamicSerializer), true), XmlArrayItem("MyObjectBuilder_CubeBlock", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CubeBlock>))]
        public MyObjectBuilder_CubeBlock[] Blocks;
        [ProtoMember(20), Serialize(MyObjectFlags.DefaultZero)]
        public ushort[] BlockIds;

        public override void Remap(IMyRemapHelper remapHelper)
        {
            base.Remap(remapHelper);
            MyObjectBuilder_CubeBlock[] blocks = this.Blocks;
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Remap(remapHelper);
            }
        }
    }
}


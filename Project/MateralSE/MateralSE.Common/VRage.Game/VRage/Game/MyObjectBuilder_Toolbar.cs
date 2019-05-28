namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Toolbar : MyObjectBuilder_Base
    {
        [ProtoMember(0x30)]
        public MyToolbarType ToolbarType;
        [ProtoMember(0x33), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public int? SelectedSlot;
        [ProtoMember(0x37), Serialize(MyObjectFlags.DefaultZero)]
        public List<Slot> Slots = new List<Slot>();
        [ProtoMember(0x3d), DefaultValue((string) null), NoSerialize]
        public List<Vector3> ColorMaskHSVList;

        public void Remap(IMyRemapHelper remapHelper)
        {
            if (this.Slots != null)
            {
                using (List<Slot>.Enumerator enumerator = this.Slots.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Data.Remap(remapHelper);
                    }
                }
            }
        }

        public bool ShouldSerializeColorMaskHSVList() => 
            false;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct Slot
        {
            [ProtoMember(0x24)]
            public int Index;
            [ProtoMember(0x27)]
            public string Item;
            [ProtoMember(0x2a), DynamicObjectBuilder(false), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ToolbarItem>))]
            public MyObjectBuilder_ToolbarItem Data;
        }
    }
}


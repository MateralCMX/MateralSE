namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CargoContainer : MyObjectBuilder_TerminalBlock
    {
        [ProtoMember(13), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_Inventory Inventory;
        [ProtoMember(0x11), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public string ContainerType;

        public override void SetupForProjector()
        {
            base.SetupForProjector();
            if (this.Inventory != null)
            {
                this.Inventory.Clear();
            }
            if (base.ComponentContainer != null)
            {
                MyObjectBuilder_ComponentContainer.ComponentData data = base.ComponentContainer.Components.Find(s => s.Component.TypeId == typeof(MyObjectBuilder_Inventory));
                if (data != null)
                {
                    (data.Component as MyObjectBuilder_Inventory).Clear();
                }
            }
        }

        public bool ShouldSerializeContainerType() => 
            (this.ContainerType != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyObjectBuilder_CargoContainer.<>c <>9 = new MyObjectBuilder_CargoContainer.<>c();
            public static Predicate<MyObjectBuilder_ComponentContainer.ComponentData> <>9__4_0;

            internal bool <SetupForProjector>b__4_0(MyObjectBuilder_ComponentContainer.ComponentData s) => 
                (s.Component.TypeId == typeof(MyObjectBuilder_Inventory));
        }
    }
}


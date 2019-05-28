namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public abstract class MyObjectBuilder_ToolbarItem : MyObjectBuilder_Base
    {
        protected MyObjectBuilder_ToolbarItem()
        {
        }

        public virtual void Remap(IMyRemapHelper remapHelper)
        {
        }
    }
}


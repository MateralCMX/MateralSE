namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_ProceduralEnvironmentSector : MyObjectBuilder_EnvironmentSector
    {
        [ProtoMember(0x1a)]
        public Module[] SavedModules;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct Module
        {
            [ProtoMember(0x11)]
            public SerializableDefinitionId ModuleId;
            [ProtoMember(20), Serialize(MyObjectFlags.Dynamic, typeof(MyObjectBuilderDynamicSerializer)), XmlElement(typeof(MyAbstractXmlSerializer<MyObjectBuilder_EnvironmentModuleBase>))]
            public MyObjectBuilder_EnvironmentModuleBase Builder;
        }
    }
}


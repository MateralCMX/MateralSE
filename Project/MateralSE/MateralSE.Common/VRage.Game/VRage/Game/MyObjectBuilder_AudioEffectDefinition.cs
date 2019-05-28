namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Data.Audio;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AudioEffectDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("Sound"), ProtoMember(0x27)]
        public List<SoundList> Sounds;
        [ProtoMember(0x2a), DefaultValue(0)]
        public int OutputSound;

        [ProtoContract]
        public class SoundEffect
        {
            [ProtoMember(0x18)]
            public string VolumeCurve;
            [ProtoMember(0x1a)]
            public float Duration;
            [ProtoMember(0x1c), DefaultValue(4)]
            public MyAudioEffect.FilterType Filter = MyAudioEffect.FilterType.None;
            [ProtoMember(30), DefaultValue((float) 1f)]
            public float Frequency = 1f;
            [ProtoMember(0x20), DefaultValue(false)]
            public bool StopAfter;
            [ProtoMember(0x22), DefaultValue((float) 1f)]
            public float Q = 1f;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct SoundList
        {
            [ProtoMember(0x12)]
            public List<MyObjectBuilder_AudioEffectDefinition.SoundEffect> SoundEffects;
        }
    }
}


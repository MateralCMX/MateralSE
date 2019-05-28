namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data.Audio;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [ProtoContract, XmlType("Sound"), MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public sealed class MyObjectBuilder_AudioDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlIgnore]
        public MySoundData SoundData = new MySoundData();

        [ProtoMember(0x18)]
        public string Category
        {
            get => 
                this.SoundData.Category.ToString();
            set => 
                (this.SoundData.Category = MyStringId.GetOrCompute(value));
        }

        [ProtoMember(0x1f), DefaultValue(3)]
        public MyCurveType VolumeCurve
        {
            get => 
                this.SoundData.VolumeCurve;
            set => 
                (this.SoundData.VolumeCurve = value);
        }

        [ProtoMember(0x26)]
        public float MaxDistance
        {
            get => 
                this.SoundData.MaxDistance;
            set => 
                (this.SoundData.MaxDistance = value);
        }

        [ProtoMember(0x2d)]
        public float UpdateDistance
        {
            get => 
                this.SoundData.UpdateDistance;
            set => 
                (this.SoundData.UpdateDistance = value);
        }

        [ProtoMember(0x34), DefaultValue((float) 1f)]
        public float Volume
        {
            get => 
                this.SoundData.Volume;
            set => 
                (this.SoundData.Volume = value);
        }

        [ProtoMember(0x3b), DefaultValue((float) 0f)]
        public float VolumeVariation
        {
            get => 
                this.SoundData.VolumeVariation;
            set => 
                (this.SoundData.VolumeVariation = value);
        }

        [ProtoMember(0x42), DefaultValue((float) 0f)]
        public float PitchVariation
        {
            get => 
                this.SoundData.PitchVariation;
            set => 
                (this.SoundData.PitchVariation = value);
        }

        [ProtoMember(0x49), DefaultValue((float) 0f)]
        public float Pitch
        {
            get => 
                this.SoundData.Pitch;
            set => 
                (this.SoundData.Pitch = value);
        }

        [ProtoMember(80), DefaultValue(-1)]
        public int PreventSynchronization
        {
            get => 
                this.SoundData.PreventSynchronization;
            set => 
                (this.SoundData.PreventSynchronization = value);
        }

        [ProtoMember(0x57)]
        public string DynamicMusicCategory
        {
            get => 
                this.SoundData.DynamicMusicCategory.ToString();
            set => 
                (this.SoundData.DynamicMusicCategory = MyStringId.GetOrCompute(value));
        }

        [ProtoMember(0x5e)]
        public int DynamicMusicAmount
        {
            get => 
                this.SoundData.DynamicMusicAmount;
            set => 
                (this.SoundData.DynamicMusicAmount = value);
        }

        [ProtoMember(0x65), DefaultValue(true)]
        public bool ModifiableByHelmetFilters
        {
            get => 
                this.SoundData.ModifiableByHelmetFilters;
            set => 
                (this.SoundData.ModifiableByHelmetFilters = value);
        }

        [ProtoMember(0x6c), DefaultValue(false)]
        public bool AlwaysUseOneMode
        {
            get => 
                this.SoundData.AlwaysUseOneMode;
            set => 
                (this.SoundData.AlwaysUseOneMode = value);
        }

        [ProtoMember(0x73), DefaultValue(true)]
        public bool CanBeSilencedByVoid
        {
            get => 
                this.SoundData.CanBeSilencedByVoid;
            set => 
                (this.SoundData.CanBeSilencedByVoid = value);
        }

        [ProtoMember(0x7a), DefaultValue(false)]
        public bool StreamSound
        {
            get => 
                this.SoundData.StreamSound;
            set => 
                (this.SoundData.StreamSound = value);
        }

        [ProtoMember(0x81), DefaultValue(false)]
        public bool DisablePitchEffects
        {
            get => 
                this.SoundData.DisablePitchEffects;
            set => 
                (this.SoundData.DisablePitchEffects = value);
        }

        [ProtoMember(0x88), DefaultValue(0)]
        public int SoundLimit
        {
            get => 
                this.SoundData.SoundLimit;
            set => 
                (this.SoundData.SoundLimit = value);
        }

        [ProtoMember(0x8f), DefaultValue(false)]
        public bool Loopable
        {
            get => 
                this.SoundData.Loopable;
            set => 
                (this.SoundData.Loopable = value);
        }

        [ProtoMember(150)]
        public string Alternative2D
        {
            get => 
                this.SoundData.Alternative2D;
            set => 
                (this.SoundData.Alternative2D = value);
        }

        [ProtoMember(0x9d), DefaultValue(false)]
        public bool UseOcclusion
        {
            get => 
                this.SoundData.UseOcclusion;
            set => 
                (this.SoundData.UseOcclusion = value);
        }

        [ProtoMember(0xa4)]
        public List<MyAudioWave> Waves
        {
            get => 
                this.SoundData.Waves;
            set => 
                (this.SoundData.Waves = value);
        }

        [ProtoMember(0xab)]
        public List<DistantSound> DistantSounds
        {
            get => 
                this.SoundData.DistantSounds;
            set => 
                (this.SoundData.DistantSounds = value);
        }

        [ProtoMember(0xb2), DefaultValue("")]
        public string TransitionCategory
        {
            get => 
                this.SoundData.MusicTrack.TransitionCategory.ToString();
            set => 
                (this.SoundData.MusicTrack.TransitionCategory = MyStringId.GetOrCompute(value));
        }

        [ProtoMember(0xb9), DefaultValue("")]
        public string MusicCategory
        {
            get => 
                this.SoundData.MusicTrack.MusicCategory.ToString();
            set => 
                (this.SoundData.MusicTrack.MusicCategory = MyStringId.GetOrCompute(value));
        }

        [ProtoMember(0xc0), DefaultValue("")]
        public string RealisticFilter
        {
            get => 
                this.SoundData.RealisticFilter.String;
            set => 
                (this.SoundData.RealisticFilter = MyStringHash.GetOrCompute(value));
        }

        [ProtoMember(0xc7), DefaultValue((float) 1f)]
        public float RealisticVolumeChange
        {
            get => 
                this.SoundData.RealisticVolumeChange;
            set => 
                (this.SoundData.RealisticVolumeChange = value);
        }
    }
}


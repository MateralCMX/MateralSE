namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipSoundsDefinition), (Type) null)]
    public class MyShipSoundsDefinition : MyDefinitionBase
    {
        public float MinWeight = 3000f;
        public bool AllowSmallGrid = true;
        public bool AllowLargeGrid = true;
        public Dictionary<ShipSystemSoundsEnum, MySoundPair> Sounds = new Dictionary<ShipSystemSoundsEnum, MySoundPair>();
        public List<MyTuple<float, float>> ThrusterVolumes = new List<MyTuple<float, float>>();
        public List<MyTuple<float, float>> EngineVolumes = new List<MyTuple<float, float>>();
        public List<MyTuple<float, float>> WheelsVolumes = new List<MyTuple<float, float>>();
        public float EnginePitchRangeInSemitones = 4f;
        public float EnginePitchRangeInSemitones_h = -2f;
        public float WheelsPitchRangeInSemitones = 4f;
        public float WheelsPitchRangeInSemitones_h = -2f;
        public float ThrusterPitchRangeInSemitones = 4f;
        public float ThrusterPitchRangeInSemitones_h = -2f;
        public float EngineTimeToTurnOn = 4f;
        public float EngineTimeToTurnOff = 3f;
        public float WheelsLowerThrusterVolumeBy = 0.33f;
        public float WheelsFullSpeed = 32f;
        public float WheelsSpeedCompensation = 3f;
        public float ThrusterCompositionMinVolume = 0.4f;
        public float ThrusterCompositionMinVolume_c = 0.6666666f;
        public float ThrusterCompositionChangeSpeed = 0.025f;
        public float SpeedUpSoundChangeVolumeTo = 1f;
        public float SpeedDownSoundChangeVolumeTo = 1f;
        public float SpeedUpDownChangeSpeed = 0.2f;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipSoundsDefinition definition = builder as MyObjectBuilder_ShipSoundsDefinition;
            this.MinWeight = definition.MinWeight;
            this.AllowSmallGrid = definition.AllowSmallGrid;
            this.AllowLargeGrid = definition.AllowLargeGrid;
            this.EnginePitchRangeInSemitones = definition.EnginePitchRangeInSemitones;
            this.EnginePitchRangeInSemitones_h = definition.EnginePitchRangeInSemitones * -0.5f;
            this.EngineTimeToTurnOn = definition.EngineTimeToTurnOn;
            this.EngineTimeToTurnOff = definition.EngineTimeToTurnOff;
            this.WheelsLowerThrusterVolumeBy = definition.WheelsLowerThrusterVolumeBy;
            this.WheelsFullSpeed = definition.WheelsFullSpeed;
            this.ThrusterCompositionMinVolume = definition.ThrusterCompositionMinVolume;
            this.ThrusterCompositionMinVolume_c = definition.ThrusterCompositionMinVolume / (1f - definition.ThrusterCompositionMinVolume);
            this.ThrusterCompositionChangeSpeed = definition.ThrusterCompositionChangeSpeed;
            this.SpeedDownSoundChangeVolumeTo = definition.SpeedDownSoundChangeVolumeTo;
            this.SpeedUpSoundChangeVolumeTo = definition.SpeedUpSoundChangeVolumeTo;
            this.SpeedUpDownChangeSpeed = definition.SpeedUpDownChangeSpeed * 0.01666667f;
            foreach (ShipSound sound in definition.Sounds)
            {
                if (sound.SoundName.Length == 0)
                {
                    continue;
                }
                MySoundPair objA = new MySoundPair(sound.SoundName, true);
                if (!ReferenceEquals(objA, MySoundPair.Empty))
                {
                    this.Sounds.Add(sound.SoundType, objA);
                }
            }
            List<MyTuple<float, float>> list = new List<MyTuple<float, float>>();
            foreach (ShipSoundVolumePair pair2 in definition.ThrusterVolumes)
            {
                list.Add(new MyTuple<float, float>(Math.Max(0f, pair2.Speed), Math.Max(0f, pair2.Volume)));
            }
            this.ThrusterVolumes = (from o in list
                orderby o.Item1
                select o).ToList<MyTuple<float, float>>();
            List<MyTuple<float, float>> list2 = new List<MyTuple<float, float>>();
            foreach (ShipSoundVolumePair pair3 in definition.EngineVolumes)
            {
                list2.Add(new MyTuple<float, float>(Math.Max(0f, pair3.Speed), Math.Max(0f, pair3.Volume)));
            }
            this.EngineVolumes = (from o in list2
                orderby o.Item1
                select o).ToList<MyTuple<float, float>>();
            List<MyTuple<float, float>> list3 = new List<MyTuple<float, float>>();
            foreach (ShipSoundVolumePair pair4 in definition.WheelsVolumes)
            {
                list3.Add(new MyTuple<float, float>(Math.Max(0f, pair4.Speed), Math.Max(0f, pair4.Volume)));
            }
            this.WheelsVolumes = (from o in list3
                orderby o.Item1
                select o).ToList<MyTuple<float, float>>();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipSoundsDefinition.<>c <>9 = new MyShipSoundsDefinition.<>c();
            public static Func<MyTuple<float, float>, float> <>9__24_0;
            public static Func<MyTuple<float, float>, float> <>9__24_1;
            public static Func<MyTuple<float, float>, float> <>9__24_2;

            internal float <Init>b__24_0(MyTuple<float, float> o) => 
                o.Item1;

            internal float <Init>b__24_1(MyTuple<float, float> o) => 
                o.Item1;

            internal float <Init>b__24_2(MyTuple<float, float> o) => 
                o.Item1;
        }
    }
}


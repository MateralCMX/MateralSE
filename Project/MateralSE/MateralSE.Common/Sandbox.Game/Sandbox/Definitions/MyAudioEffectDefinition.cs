namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AudioEffectDefinition), (Type) null)]
    public class MyAudioEffectDefinition : MyDefinitionBase
    {
        public MyAudioEffect Effect = new MyAudioEffect();

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AudioEffectDefinition definition = builder as MyObjectBuilder_AudioEffectDefinition;
            this.Effect.EffectId = base.Id.SubtypeId;
            using (List<MyObjectBuilder_AudioEffectDefinition.SoundList>.Enumerator enumerator = definition.Sounds.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    List<MyAudioEffect.SoundEffect> item = new List<MyAudioEffect.SoundEffect>();
                    foreach (MyObjectBuilder_AudioEffectDefinition.SoundEffect effect in enumerator.Current.SoundEffects)
                    {
                        MyCurveDefinition definition2;
                        MyAudioEffect.SoundEffect effect2 = new MyAudioEffect.SoundEffect {
                            VolumeCurve = MyDefinitionManager.Static.TryGetDefinition<MyCurveDefinition>(new MyDefinitionId(typeof(MyObjectBuilder_CurveDefinition), effect.VolumeCurve), out definition2) ? definition2.Curve : null,
                            Duration = effect.Duration,
                            Filter = effect.Filter,
                            Frequency = (float) (2.0 * Math.Sin((3.14 * effect.Frequency) / 44100.0)),
                            OneOverQ = 1f / effect.Q,
                            StopAfter = effect.StopAfter
                        };
                        item.Add(effect2);
                    }
                    this.Effect.SoundsEffects.Add(item);
                }
            }
            if (definition.OutputSound == 0)
            {
                this.Effect.ResultEmitterIdx = this.Effect.SoundsEffects.Count - 1;
            }
            else
            {
                this.Effect.ResultEmitterIdx = definition.OutputSound - 1;
            }
        }
    }
}


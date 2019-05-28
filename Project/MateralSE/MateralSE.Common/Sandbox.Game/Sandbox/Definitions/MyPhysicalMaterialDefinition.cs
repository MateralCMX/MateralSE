namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PhysicalMaterialDefinition), (Type) null)]
    public class MyPhysicalMaterialDefinition : MyDefinitionBase
    {
        public float Density;
        public float HorisontalTransmissionMultiplier;
        public float HorisontalFragility;
        public float SupportMultiplier;
        public float CollisionMultiplier;
        public Dictionary<MyStringId, Dictionary<MyStringHash, CollisionProperty>> CollisionProperties = new Dictionary<MyStringId, Dictionary<MyStringHash, CollisionProperty>>(MyStringId.Comparer);
        public Dictionary<MyStringId, MySoundPair> GeneralSounds = new Dictionary<MyStringId, MySoundPair>(MyStringId.Comparer);
        public MyStringHash InheritFrom = MyStringHash.NullOrEmpty;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PhysicalMaterialDefinition definition = builder as MyObjectBuilder_PhysicalMaterialDefinition;
            if (definition != null)
            {
                this.Density = definition.Density;
                this.HorisontalTransmissionMultiplier = definition.HorisontalTransmissionMultiplier;
                this.HorisontalFragility = definition.HorisontalFragility;
                this.SupportMultiplier = definition.SupportMultiplier;
                this.CollisionMultiplier = definition.CollisionMultiplier;
            }
            MyObjectBuilder_MaterialPropertiesDefinition definition2 = builder as MyObjectBuilder_MaterialPropertiesDefinition;
            if (definition2 != null)
            {
                this.InheritFrom = MyStringHash.GetOrCompute(definition2.InheritFrom);
                foreach (MyObjectBuilder_MaterialPropertiesDefinition.ContactProperty property in definition2.ContactProperties)
                {
                    MyStringId orCompute = MyStringId.GetOrCompute(property.Type);
                    if (!this.CollisionProperties.ContainsKey(orCompute))
                    {
                        this.CollisionProperties[orCompute] = new Dictionary<MyStringHash, CollisionProperty>(MyStringHash.Comparer);
                    }
                    MyStringHash hash = MyStringHash.GetOrCompute(property.Material);
                    this.CollisionProperties[orCompute][hash] = new CollisionProperty(property.SoundCue, property.ParticleEffect, property.AlternativeImpactSounds);
                }
                foreach (MyObjectBuilder_MaterialPropertiesDefinition.GeneralProperty property2 in definition2.GeneralProperties)
                {
                    this.GeneralSounds[MyStringId.GetOrCompute(property2.Type)] = new MySoundPair(property2.SoundCue, true);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollisionProperty
        {
            public MySoundPair Sound;
            public string ParticleEffect;
            public List<MyPhysicalMaterialDefinition.ImpactSounds> ImpactSoundCues;
            public CollisionProperty(string soundCue, string particleEffectName, List<AlternativeImpactSounds> impactsounds)
            {
                this.Sound = new MySoundPair(soundCue, true);
                this.ParticleEffect = particleEffectName;
                if ((impactsounds == null) || (impactsounds.Count == 0))
                {
                    this.ImpactSoundCues = null;
                }
                else
                {
                    this.ImpactSoundCues = new List<MyPhysicalMaterialDefinition.ImpactSounds>();
                    foreach (AlternativeImpactSounds sounds in impactsounds)
                    {
                        this.ImpactSoundCues.Add(new MyPhysicalMaterialDefinition.ImpactSounds(sounds.mass, sounds.soundCue, sounds.minVelocity, sounds.maxVolumeVelocity));
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ImpactSounds
        {
            public float Mass;
            public MySoundPair SoundCue;
            public float minVelocity;
            public float maxVolumeVelocity;
            public ImpactSounds(float mass, string soundCue, float minVelocity, float maxVolumeVelocity)
            {
                this.Mass = mass;
                this.SoundCue = new MySoundPair(soundCue, true);
                this.minVelocity = minVelocity;
                this.maxVolumeVelocity = maxVolumeVelocity;
            }
        }
    }
}


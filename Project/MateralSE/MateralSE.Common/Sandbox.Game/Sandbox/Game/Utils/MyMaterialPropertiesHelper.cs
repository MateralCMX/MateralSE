namespace Sandbox.Game.Utils
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyMaterialPropertiesHelper : MySessionComponentBase
    {
        public static MyMaterialPropertiesHelper Static;
        private Dictionary<MyStringId, Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>>> MaterialDictionary = new Dictionary<MyStringId, Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>>>(MyStringId.Comparer);
        private HashSet<MyStringHash> m_loaded = new HashSet<MyStringHash>(MyStringHash.Comparer);

        public MySoundPair GetCollisionCue(MyStringId type, MyStringHash materialType1, MyStringHash materialType2)
        {
            Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>> dictionary;
            Dictionary<MyStringHash, MaterialProperties> dictionary2;
            MaterialProperties properties;
            if ((!this.MaterialDictionary.TryGetValue(type, out dictionary) || !dictionary.TryGetValue(materialType1, out dictionary2)) || !dictionary2.TryGetValue(materialType2, out properties))
            {
                return MySoundPair.Empty;
            }
            return properties.Sound;
        }

        public MySoundPair GetCollisionCueWithMass(MyStringId type, MyStringHash materialType1, MyStringHash materialType2, ref float volume, float? mass = new float?(), float velocity = 0f)
        {
            Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>> dictionary;
            Dictionary<MyStringHash, MaterialProperties> dictionary2;
            MaterialProperties properties;
            if ((!this.MaterialDictionary.TryGetValue(type, out dictionary) || !dictionary.TryGetValue(materialType1, out dictionary2)) || !dictionary2.TryGetValue(materialType2, out properties))
            {
                return MySoundPair.Empty;
            }
            if (((mass == null) || (properties.ImpactSoundCues == null)) || (properties.ImpactSoundCues.Count == 0))
            {
                return properties.Sound;
            }
            int num = -1;
            float num2 = -1f;
            for (int i = 0; i < properties.ImpactSoundCues.Count; i++)
            {
                float? nullable = mass;
                float num4 = properties.ImpactSoundCues[i].Mass;
                if ((((nullable.GetValueOrDefault() >= num4) & (nullable != null)) && (properties.ImpactSoundCues[i].Mass > num2)) && (velocity >= properties.ImpactSoundCues[i].minVelocity))
                {
                    num = i;
                    num2 = properties.ImpactSoundCues[i].Mass;
                }
            }
            if (num < 0)
            {
                return properties.Sound;
            }
            volume = 0.25f + (0.75f * MyMath.Clamp((velocity - properties.ImpactSoundCues[num].minVelocity) / (properties.ImpactSoundCues[num].maxVolumeVelocity - properties.ImpactSoundCues[num].minVelocity), 0f, 1f));
            return properties.ImpactSoundCues[num].SoundCue;
        }

        public string GetCollisionEffect(MyStringId type, MyStringHash materialType1, MyStringHash materialType2)
        {
            string particleEffectName = null;
            Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>> dictionary;
            Dictionary<MyStringHash, MaterialProperties> dictionary2;
            MaterialProperties properties;
            if ((this.MaterialDictionary.TryGetValue(type, out dictionary) && dictionary.TryGetValue(materialType1, out dictionary2)) && dictionary2.TryGetValue(materialType2, out properties))
            {
                particleEffectName = properties.ParticleEffectName;
            }
            return particleEffectName;
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
            foreach (MyPhysicalMaterialDefinition definition in MyDefinitionManager.Static.GetPhysicalMaterialDefinitions())
            {
                this.LoadMaterialProperties(definition);
            }
            foreach (MyPhysicalMaterialDefinition definition2 in MyDefinitionManager.Static.GetPhysicalMaterialDefinitions())
            {
                this.LoadMaterialSoundsInheritance(definition2);
            }
        }

        private void LoadMaterialProperties(MyPhysicalMaterialDefinition material)
        {
            MyStringHash subtypeId = material.Id.SubtypeId;
            foreach (KeyValuePair<MyStringId, Dictionary<MyStringHash, MyPhysicalMaterialDefinition.CollisionProperty>> pair in material.CollisionProperties)
            {
                MyStringId key = pair.Key;
                if (!this.MaterialDictionary.ContainsKey(key))
                {
                    this.MaterialDictionary[key] = new Dictionary<MyStringHash, Dictionary<MyStringHash, MaterialProperties>>(MyStringHash.Comparer);
                }
                if (!this.MaterialDictionary[key].ContainsKey(subtypeId))
                {
                    this.MaterialDictionary[key][subtypeId] = new Dictionary<MyStringHash, MaterialProperties>(MyStringHash.Comparer);
                }
                foreach (KeyValuePair<MyStringHash, MyPhysicalMaterialDefinition.CollisionProperty> pair2 in pair.Value)
                {
                    this.MaterialDictionary[key][subtypeId][pair2.Key] = new MaterialProperties(pair2.Value.Sound, pair2.Value.ParticleEffect, pair2.Value.ImpactSoundCues);
                    if (!this.MaterialDictionary[key].ContainsKey(pair2.Key))
                    {
                        this.MaterialDictionary[key][pair2.Key] = new Dictionary<MyStringHash, MaterialProperties>(MyStringHash.Comparer);
                    }
                    if (!this.MaterialDictionary[key][pair2.Key].ContainsKey(subtypeId))
                    {
                        this.MaterialDictionary[key][pair2.Key][subtypeId] = new MaterialProperties(pair2.Value.Sound, pair2.Value.ParticleEffect, pair2.Value.ImpactSoundCues);
                    }
                }
            }
        }

        private void LoadMaterialSoundsInheritance(MyPhysicalMaterialDefinition material)
        {
            MyStringHash subtypeId = material.Id.SubtypeId;
            if (this.m_loaded.Add(subtypeId) && (material.InheritFrom != MyStringHash.NullOrEmpty))
            {
                MyPhysicalMaterialDefinition definition;
                if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalMaterialDefinition>(new MyDefinitionId(typeof(MyObjectBuilder_PhysicalMaterialDefinition), material.InheritFrom), out definition))
                {
                    if (!this.m_loaded.Contains(material.InheritFrom))
                    {
                        this.LoadMaterialSoundsInheritance(definition);
                    }
                    foreach (KeyValuePair<MyStringId, MySoundPair> pair in definition.GeneralSounds)
                    {
                        material.GeneralSounds[pair.Key] = pair.Value;
                    }
                }
                foreach (MyStringId id in this.MaterialDictionary.Keys)
                {
                    if (!this.MaterialDictionary[id].ContainsKey(subtypeId))
                    {
                        this.MaterialDictionary[id][subtypeId] = new Dictionary<MyStringHash, MaterialProperties>(MyStringHash.Comparer);
                    }
                    MaterialProperties? nullable = null;
                    if (this.MaterialDictionary[id].ContainsKey(material.InheritFrom))
                    {
                        foreach (KeyValuePair<MyStringHash, MaterialProperties> pair2 in this.MaterialDictionary[id][material.InheritFrom])
                        {
                            if (pair2.Key == material.InheritFrom)
                            {
                                nullable = new MaterialProperties?(pair2.Value);
                                continue;
                            }
                            if (!this.MaterialDictionary[id][subtypeId].ContainsKey(pair2.Key))
                            {
                                this.MaterialDictionary[id][subtypeId][pair2.Key] = pair2.Value;
                                this.MaterialDictionary[id][pair2.Key][subtypeId] = pair2.Value;
                                continue;
                            }
                            if (!this.MaterialDictionary[id][pair2.Key].ContainsKey(subtypeId))
                            {
                                this.MaterialDictionary[id][pair2.Key][subtypeId] = pair2.Value;
                            }
                        }
                        if (nullable != null)
                        {
                            this.MaterialDictionary[id][subtypeId][subtypeId] = nullable.Value;
                            this.MaterialDictionary[id][subtypeId][material.InheritFrom] = nullable.Value;
                            this.MaterialDictionary[id][material.InheritFrom][subtypeId] = nullable.Value;
                        }
                    }
                }
            }
        }

        public bool TryCreateCollisionEffect(MyStringId type, Vector3D position, Vector3 normal, MyStringHash material1, MyStringHash material2, IMyEntity entity)
        {
            MyParticleEffect effect;
            string effectName = this.GetCollisionEffect(type, material1, material2);
            if (effectName == null)
            {
                return false;
            }
            MatrixD worldMatrix = MatrixD.CreateWorld(position, normal, Vector3.CalculatePerpendicularVector(normal));
            if (entity == null)
            {
                return MyParticlesManager.TryCreateParticleEffect(effectName, worldMatrix, out effect);
            }
            MyEntity entity2 = entity as MyEntity;
            worldMatrix *= entity2.PositionComp.WorldMatrixNormalizedInv;
            return MyParticlesManager.TryCreateParticleEffect(effectName, ref worldMatrix, ref position, entity2.Render.RenderObjectIDs[0], out effect);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public static class CollisionType
        {
            public static MyStringId Start = MyStringId.GetOrCompute("Start");
            public static MyStringId Hit = MyStringId.GetOrCompute("Hit");
            public static MyStringId Walk = MyStringId.GetOrCompute("Walk");
            public static MyStringId Run = MyStringId.GetOrCompute("Run");
            public static MyStringId Sprint = MyStringId.GetOrCompute("Sprint");
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MaterialProperties
        {
            public MySoundPair Sound;
            public string ParticleEffectName;
            public List<MyPhysicalMaterialDefinition.ImpactSounds> ImpactSoundCues;
            public MaterialProperties(MySoundPair soundCue, string particleEffectName, List<MyPhysicalMaterialDefinition.ImpactSounds> impactSounds)
            {
                this.Sound = soundCue;
                this.ParticleEffectName = particleEffectName;
                this.ImpactSoundCues = impactSounds;
            }
        }
    }
}


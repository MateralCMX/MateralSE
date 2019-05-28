namespace Sandbox.Game.ParticleEffects
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;

    public class MyCubeBlockEffect
    {
        public readonly int EffectId;
        private CubeBlockEffectBase m_effectDefinition;
        public bool CanBeDeleted;
        private List<MyCubeBlockParticleEffect> m_particleEffects;
        private MyEntity m_entity;

        public MyCubeBlockEffect(int EffectId, CubeBlockEffectBase effectDefinition, MyEntity block)
        {
            this.EffectId = EffectId;
            this.m_entity = block;
            this.m_effectDefinition = effectDefinition;
            this.m_particleEffects = new List<MyCubeBlockParticleEffect>();
            if (this.m_effectDefinition.ParticleEffects != null)
            {
                for (int i = 0; i < this.m_effectDefinition.ParticleEffects.Length; i++)
                {
                    this.m_particleEffects.Add(new MyCubeBlockParticleEffect(this.m_effectDefinition.ParticleEffects[i], this.m_entity));
                }
            }
        }

        public void Stop()
        {
            for (int i = 0; i < this.m_particleEffects.Count; i++)
            {
                this.m_particleEffects[i].Stop();
            }
            this.m_particleEffects.Clear();
        }

        public void Update()
        {
            for (int i = 0; i < this.m_particleEffects.Count; i++)
            {
                if (!this.m_particleEffects[i].CanBeDeleted)
                {
                    this.m_particleEffects[i].Update();
                }
                else
                {
                    this.m_particleEffects[i].Stop();
                    this.m_particleEffects.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}


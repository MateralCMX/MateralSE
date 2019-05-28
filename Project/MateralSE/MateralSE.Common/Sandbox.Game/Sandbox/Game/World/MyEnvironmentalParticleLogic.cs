namespace Sandbox.Game.World
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using VRage.Game.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEnvironmentalParticleLogicType(typeof(MyObjectBuilder_EnvironmentalParticleLogic), true)]
    public class MyEnvironmentalParticleLogic
    {
        protected float m_particleDensity;
        protected float m_particleSpawnDistance;
        protected float m_particleDespawnDistance;
        private int m_maxParticles = 0x80;
        protected List<MyEnvironmentalParticle> m_nonActiveParticles;
        protected List<MyEnvironmentalParticle> m_activeParticles;
        protected List<int> m_particlesToRemove = new List<int>();

        protected void DeactivateAll()
        {
            foreach (MyEnvironmentalParticle particle in this.m_activeParticles)
            {
                this.m_nonActiveParticles.Add(particle);
                particle.Deactivate();
            }
            this.m_activeParticles.Clear();
        }

        protected bool Despawn(MyEnvironmentalParticle particle)
        {
            if (particle != null)
            {
                using (List<MyEnvironmentalParticle>.Enumerator enumerator = this.m_activeParticles.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyEnvironmentalParticle current = enumerator.Current;
                        if (ReferenceEquals(particle, current))
                        {
                            this.m_activeParticles.Remove(particle);
                            particle.Deactivate();
                            this.m_nonActiveParticles.Add(particle);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual void Draw()
        {
        }

        public virtual void Init(MyObjectBuilder_EnvironmentalParticleLogic builder)
        {
            this.m_particleDensity = builder.Density;
            this.m_particleSpawnDistance = builder.MaxSpawnDistance;
            this.m_particleDespawnDistance = builder.DespawnDistance;
            this.m_maxParticles = builder.MaxParticles;
            this.m_nonActiveParticles = new List<MyEnvironmentalParticle>(this.m_maxParticles);
            this.m_activeParticles = new List<MyEnvironmentalParticle>(this.m_maxParticles);
            string material = builder.Material;
            Vector4 particleColor = builder.ParticleColor;
            MyObjectBuilder_EnvironmentalParticleLogicSpace space = builder as MyObjectBuilder_EnvironmentalParticleLogicSpace;
            if (space != null)
            {
                material = space.MaterialPlanet;
                particleColor = space.ParticleColorPlanet;
            }
            for (int i = 0; i < this.m_maxParticles; i++)
            {
                this.m_nonActiveParticles.Add(new MyEnvironmentalParticle(builder.Material, material, builder.ParticleColor, particleColor, builder.MaxLifeTime));
            }
        }

        public virtual void Simulate()
        {
        }

        protected MyEnvironmentalParticle Spawn(Vector3 position)
        {
            int count = this.m_nonActiveParticles.Count;
            if (count <= 0)
            {
                return null;
            }
            MyEnvironmentalParticle item = this.m_nonActiveParticles[count - 1];
            this.m_activeParticles.Add(item);
            this.m_nonActiveParticles.RemoveAtFast<MyEnvironmentalParticle>(count - 1);
            item.Activate(position);
            return item;
        }

        public virtual void UpdateAfterSimulation()
        {
            for (int i = 0; i < this.m_activeParticles.Count; i++)
            {
                MyEnvironmentalParticle particle = this.m_activeParticles[i];
                if ((((MySandboxGame.TotalGamePlayTimeInMilliseconds - particle.BirthTime) >= particle.LifeTime) || ((particle.Position - MySector.MainCamera.Position).Length() > this.m_particleDespawnDistance)) || !particle.Active)
                {
                    this.m_particlesToRemove.Add(i);
                }
            }
            for (int j = this.m_particlesToRemove.Count - 1; j >= 0; j--)
            {
                int index = this.m_particlesToRemove[j];
                this.m_nonActiveParticles.Add(this.m_activeParticles[index]);
                this.m_activeParticles[index].Deactivate();
                this.m_activeParticles.RemoveAt(index);
            }
            this.m_particlesToRemove.Clear();
        }

        public virtual void UpdateBeforeSimulation()
        {
        }

        protected float ParticleDensity =>
            this.m_particleDensity;

        protected float ParticleSpawnDistance =>
            this.m_particleSpawnDistance;

        protected float ParticleDespawnDistance =>
            this.m_particleDespawnDistance;

        public class MyEnvironmentalParticle
        {
            private Vector3 m_position;
            private MyStringId m_material;
            private Vector4 m_color;
            private MyStringId m_materialPlanet;
            private Vector4 m_colorPlanet;
            private int m_birthTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            private int m_lifeTime;
            private bool m_active;
            public object UserData;

            public MyEnvironmentalParticle(string material, string materialPlanet, Vector4 color, Vector4 colorPlanet, int lifeTime)
            {
                this.m_material = (material != null) ? MyStringId.GetOrCompute(material) : MyTransparentMaterials.ErrorMaterial.Id;
                this.m_materialPlanet = (materialPlanet != null) ? MyStringId.GetOrCompute(materialPlanet) : MyTransparentMaterials.ErrorMaterial.Id;
                this.m_color = color;
                this.m_colorPlanet = colorPlanet;
                this.m_position = new Vector3();
                this.m_lifeTime = lifeTime;
                this.Deactivate();
            }

            public void Activate(Vector3 position)
            {
                this.m_birthTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_position = position;
                this.m_active = true;
            }

            public void Deactivate()
            {
                this.m_active = false;
            }

            public Vector3 Position
            {
                get => 
                    this.m_position;
                set => 
                    (this.m_position = value);
            }

            public MyStringId Material =>
                this.m_material;

            public Vector4 Color =>
                this.m_color;

            public MyStringId MaterialPlanet =>
                this.m_materialPlanet;

            public Vector4 ColorPlanet =>
                this.m_colorPlanet;

            public int BirthTime =>
                this.m_birthTime;

            public int LifeTime =>
                this.m_lifeTime;

            public bool Active =>
                this.m_active;
        }
    }
}


namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation | MyUpdateOrder.BeforeSimulation)]
    internal class MyEnvironmentalParticles : MySessionComponentBase
    {
        private List<MyEnvironmentalParticleLogic> m_particleHandlers = new List<MyEnvironmentalParticleLogic>();

        public override void Draw()
        {
            base.Draw();
            using (List<MyEnvironmentalParticleLogic>.Enumerator enumerator = this.m_particleHandlers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Draw();
                }
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            if (MySector.EnvironmentDefinition != null)
            {
                foreach (MyObjectBuilder_EnvironmentDefinition.EnvironmentalParticleSettings settings in MySector.EnvironmentDefinition.EnvironmentalParticles)
                {
                    MyObjectBuilder_EnvironmentalParticleLogic builder = MyObjectBuilderSerializer.CreateNewObject(settings.Id) as MyObjectBuilder_EnvironmentalParticleLogic;
                    if (builder != null)
                    {
                        builder.Density = settings.Density;
                        builder.DespawnDistance = settings.DespawnDistance;
                        builder.ParticleColor = settings.Color;
                        builder.ParticleColorPlanet = settings.ColorPlanet;
                        builder.MaxSpawnDistance = settings.MaxSpawnDistance;
                        builder.Material = settings.Material;
                        builder.MaterialPlanet = settings.MaterialPlanet;
                        builder.MaxLifeTime = settings.MaxLifeTime;
                        builder.MaxParticles = settings.MaxParticles;
                        MyEnvironmentalParticleLogic item = MyEnvironmentalParticleLogicFactory.CreateEnvironmentalParticleLogic(builder);
                        item.Init(builder);
                        this.m_particleHandlers.Add(item);
                    }
                }
            }
        }

        public override void Simulate()
        {
            base.Simulate();
            if (!MyParticlesManager.Paused)
            {
                using (List<MyEnvironmentalParticleLogic>.Enumerator enumerator = this.m_particleHandlers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Simulate();
                    }
                }
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (!MyParticlesManager.Paused)
            {
                using (List<MyEnvironmentalParticleLogic>.Enumerator enumerator = this.m_particleHandlers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateAfterSimulation();
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (!MyParticlesManager.Paused)
            {
                using (List<MyEnvironmentalParticleLogic>.Enumerator enumerator = this.m_particleHandlers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateBeforeSimulation();
                    }
                }
            }
        }
    }
}


namespace SpaceEngineers.Game.EntityComponents.GameLogic
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRageMath;

    public class MySharedWindComponent : MyEntityComponentBase
    {
        private float m_windSpeed = -1f;
        private MyWindTurbine m_updatingTurbine;
        private readonly HashSet<MyWindTurbine> m_windTurbines = new HashSet<MyWindTurbine>();

        private float ComputeWindSpeed()
        {
            MyCubeGrid entity = this.Entity;
            if ((entity.IsPreview || (entity.Physics == null)) || !MyFixedGrids.IsRooted(entity))
            {
                return 0f;
            }
            Vector3D centerOfMassWorld = entity.Physics.CenterOfMassWorld;
            MyPlanet closestPlanet = MyPlanets.Static.GetClosestPlanet(centerOfMassWorld);
            if ((closestPlanet == null) || (closestPlanet.PositionComp.WorldAABB.Contains(centerOfMassWorld) == ContainmentType.Disjoint))
            {
                return 0f;
            }
            return closestPlanet.GetWindSpeed(centerOfMassWorld);
        }

        public void Register(MyWindTurbine windTurbine)
        {
            this.m_windTurbines.Add(windTurbine);
            if (this.UpdatingTurbine == null)
            {
                this.UpdatingTurbine = windTurbine;
            }
        }

        public void Unregister(MyWindTurbine windTurbine)
        {
            this.m_windTurbines.Remove(windTurbine);
            if (ReferenceEquals(this.UpdatingTurbine, windTurbine))
            {
                if (this.m_windTurbines.Count == 0)
                {
                    this.UpdatingTurbine = null;
                    this.Entity.Components.Remove(typeof(MySharedWindComponent), this);
                }
                else
                {
                    this.UpdatingTurbine = this.m_windTurbines.FirstElement<MyWindTurbine>();
                }
            }
        }

        public void Update10()
        {
            using (HashSet<MyWindTurbine>.Enumerator enumerator = this.m_windTurbines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateNextRay();
                }
            }
        }

        public void UpdateWindSpeed()
        {
            this.WindSpeed = this.ComputeWindSpeed();
        }

        public MyCubeGrid Entity =>
            ((MyCubeGrid) base.Entity);

        public Vector3D GravityNormal { get; private set; }

        public float WindSpeed
        {
            get => 
                ((this.m_windSpeed < 0f) ? 0f : this.m_windSpeed);
            private set
            {
                if (this.m_windSpeed != value)
                {
                    if ((value == 0f) != !(this.m_windSpeed != 0f))
                    {
                        MyWindTurbine updatingTurbine = this.UpdatingTurbine;
                        updatingTurbine.NeedsUpdate ^= MyEntityUpdateEnum.EACH_10TH_FRAME;
                    }
                    this.m_windSpeed = value;
                    MyGridPhysics physics = this.Entity.Physics;
                    Vector3D worldPoint = (physics != null) ? physics.CenterOfMassWorld : this.Entity.PositionComp.GetPosition();
                    this.GravityNormal = Vector3.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(worldPoint));
                    using (HashSet<MyWindTurbine>.Enumerator enumerator = this.m_windTurbines.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.OnEnvironmentChanged();
                        }
                    }
                }
            }
        }

        public bool IsEnabled =>
            (this.WindSpeed > 0f);

        private MyWindTurbine UpdatingTurbine
        {
            get => 
                this.m_updatingTurbine;
            set
            {
                if (this.m_updatingTurbine != null)
                {
                    this.m_updatingTurbine.NeedsUpdate &= ~(MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME);
                }
                this.m_updatingTurbine = value;
                if (this.m_updatingTurbine != null)
                {
                    MyEntityUpdateEnum enum2 = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
                    if (this.IsEnabled)
                    {
                        enum2 |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                    }
                    this.m_updatingTurbine.NeedsUpdate |= enum2;
                }
            }
        }

        public override string ComponentTypeDebugString =>
            base.GetType().Name;
    }
}


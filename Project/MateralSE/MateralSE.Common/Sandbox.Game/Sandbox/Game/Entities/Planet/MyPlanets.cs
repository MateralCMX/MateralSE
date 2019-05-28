namespace Sandbox.Game.Entities.Planet
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 500)]
    public class MyPlanets : MySessionComponentBase
    {
        [CompilerGenerated]
        private Action<MyPlanet> OnPlanetAdded;
        [CompilerGenerated]
        private Action<MyPlanet> OnPlanetRemoved;
        private readonly List<MyPlanet> m_planets = new List<MyPlanet>();
        public readonly List<BoundingBoxD> m_planetAABBsCache = new List<BoundingBoxD>();

        public event Action<MyPlanet> OnPlanetAdded
        {
            [CompilerGenerated] add
            {
                Action<MyPlanet> onPlanetAdded = this.OnPlanetAdded;
                while (true)
                {
                    Action<MyPlanet> a = onPlanetAdded;
                    Action<MyPlanet> action3 = (Action<MyPlanet>) Delegate.Combine(a, value);
                    onPlanetAdded = Interlocked.CompareExchange<Action<MyPlanet>>(ref this.OnPlanetAdded, action3, a);
                    if (ReferenceEquals(onPlanetAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlanet> onPlanetAdded = this.OnPlanetAdded;
                while (true)
                {
                    Action<MyPlanet> source = onPlanetAdded;
                    Action<MyPlanet> action3 = (Action<MyPlanet>) Delegate.Remove(source, value);
                    onPlanetAdded = Interlocked.CompareExchange<Action<MyPlanet>>(ref this.OnPlanetAdded, action3, source);
                    if (ReferenceEquals(onPlanetAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPlanet> OnPlanetRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyPlanet> onPlanetRemoved = this.OnPlanetRemoved;
                while (true)
                {
                    Action<MyPlanet> a = onPlanetRemoved;
                    Action<MyPlanet> action3 = (Action<MyPlanet>) Delegate.Combine(a, value);
                    onPlanetRemoved = Interlocked.CompareExchange<Action<MyPlanet>>(ref this.OnPlanetRemoved, action3, a);
                    if (ReferenceEquals(onPlanetRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlanet> onPlanetRemoved = this.OnPlanetRemoved;
                while (true)
                {
                    Action<MyPlanet> source = onPlanetRemoved;
                    Action<MyPlanet> action3 = (Action<MyPlanet>) Delegate.Remove(source, value);
                    onPlanetRemoved = Interlocked.CompareExchange<Action<MyPlanet>>(ref this.OnPlanetRemoved, action3, source);
                    if (ReferenceEquals(onPlanetRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyPlanet GetClosestPlanet(Vector3D position)
        {
            List<MyPlanet> planets = this.m_planets;
            return ((planets.Count != 0) ? planets.MinBy<MyPlanet>(x => ((float) (Vector3D.DistanceSquared(x.PositionComp.GetPosition(), position) / 1000.0))) : null);
        }

        public ListReader<BoundingBoxD> GetPlanetAABBs()
        {
            if (this.m_planetAABBsCache.Count == 0)
            {
                foreach (MyPlanet planet in this.m_planets)
                {
                    this.m_planetAABBsCache.Add(planet.PositionComp.WorldAABB);
                }
            }
            return this.m_planetAABBsCache;
        }

        public static List<MyPlanet> GetPlanets() => 
            ((Static != null) ? Static.m_planets : null);

        public override void LoadData()
        {
            Static = this;
            base.LoadData();
        }

        public static void Register(MyPlanet myPlanet)
        {
            Static.m_planets.Add(myPlanet);
            Static.m_planetAABBsCache.Clear();
            Static.OnPlanetAdded.InvokeIfNotNull<MyPlanet>(myPlanet);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public static void UnRegister(MyPlanet myPlanet)
        {
            Static.m_planets.Remove(myPlanet);
            Static.m_planetAABBsCache.Clear();
            Static.OnPlanetRemoved.InvokeIfNotNull<MyPlanet>(myPlanet);
        }

        public static MyPlanets Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }
    }
}


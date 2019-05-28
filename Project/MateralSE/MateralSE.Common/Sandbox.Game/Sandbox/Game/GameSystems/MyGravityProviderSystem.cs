namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Library.Collections;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x29a)]
    public class MyGravityProviderSystem : MySessionComponentBase
    {
        public const float G = 9.81f;
        private static Dictionary<IMyGravityProvider, int> m_proxyIdMap = new Dictionary<IMyGravityProvider, int>();
        private static MyDynamicAABBTreeD m_artificialGravityGenerators = new MyDynamicAABBTreeD(Vector3D.One * 10.0, 10.0);
        private static ConcurrentCachingList<IMyGravityProvider> m_naturalGravityGenerators = new ConcurrentCachingList<IMyGravityProvider>();
        [ThreadStatic]
        private static GravityCollector m_gravityCollector;

        public static void AddGravityGenerator(IMyGravityProvider gravityGenerator)
        {
            if (!m_proxyIdMap.ContainsKey(gravityGenerator))
            {
                BoundingBoxD xd;
                gravityGenerator.GetProxyAABB(out xd);
                int num = m_artificialGravityGenerators.AddProxy(ref xd, gravityGenerator, 0, true);
                m_proxyIdMap.Add(gravityGenerator, num);
            }
        }

        public static void AddNaturalGravityProvider(IMyGravityProvider gravityGenerator)
        {
            m_naturalGravityGenerators.Add(gravityGenerator);
        }

        public static Vector3 CalculateArtificialGravityInPoint(Vector3D worldPoint, float gravityMultiplier = 1f)
        {
            if (gravityMultiplier == 0f)
            {
                return Vector3.Zero;
            }
            if (m_gravityCollector == null)
            {
                m_gravityCollector = new GravityCollector();
            }
            m_gravityCollector.Gravity = Vector3.Zero;
            m_gravityCollector.Collect(m_artificialGravityGenerators, ref worldPoint);
            return (m_gravityCollector.Gravity * gravityMultiplier);
        }

        public static float CalculateArtificialGravityStrengthMultiplier(float naturalGravityMultiplier) => 
            MathHelper.Clamp((float) (1f - (naturalGravityMultiplier * 2f)), (float) 0f, (float) 1f);

        public static float CalculateHighestNaturalGravityMultiplierInPoint(Vector3D worldPoint)
        {
            float num = 0f;
            m_naturalGravityGenerators.ApplyChanges();
            foreach (IMyGravityProvider provider in m_naturalGravityGenerators)
            {
                if (!provider.IsPositionInRange(worldPoint))
                {
                    continue;
                }
                float gravityMultiplier = provider.GetGravityMultiplier(worldPoint);
                if (gravityMultiplier > num)
                {
                    num = gravityMultiplier;
                }
            }
            return num;
        }

        public static Vector3 CalculateNaturalGravityInPoint(Vector3D worldPoint)
        {
            float num;
            return CalculateNaturalGravityInPoint(worldPoint, out num);
        }

        public static Vector3 CalculateNaturalGravityInPoint(Vector3D worldPoint, out float naturalGravityMultiplier)
        {
            naturalGravityMultiplier = 0f;
            Vector3 zero = Vector3.Zero;
            m_naturalGravityGenerators.ApplyChanges();
            foreach (IMyGravityProvider provider in m_naturalGravityGenerators)
            {
                if (provider.IsPositionInRange(worldPoint))
                {
                    Vector3 worldGravity = provider.GetWorldGravity(worldPoint);
                    float gravityMultiplier = provider.GetGravityMultiplier(worldPoint);
                    if (gravityMultiplier > naturalGravityMultiplier)
                    {
                        naturalGravityMultiplier = gravityMultiplier;
                    }
                    zero += worldGravity;
                }
            }
            return zero;
        }

        public static Vector3 CalculateTotalGravityInPoint(Vector3D worldPoint) => 
            CalculateTotalGravityInPoint(worldPoint, true);

        public static Vector3 CalculateTotalGravityInPoint(Vector3D worldPoint, bool clearVectors)
        {
            float num;
            Vector3 vector2 = CalculateNaturalGravityInPoint(worldPoint, out num);
            return (vector2 + CalculateArtificialGravityInPoint(worldPoint, CalculateArtificialGravityStrengthMultiplier(num)));
        }

        public static bool DoesTrajectoryIntersectNaturalGravity(Vector3D start, Vector3D end, double raySize = 0.0)
        {
            Vector3D vectord = start - end;
            if (Vector3D.IsZero(vectord))
            {
                return IsPositionInNaturalGravity(start, raySize);
            }
            Ray ray = new Ray((Vector3) start, Vector3.Normalize(vectord));
            double num1 = MathHelper.Max(raySize, 0.0);
            raySize = num1;
            m_naturalGravityGenerators.ApplyChanges();
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, IMyGravityProvider, List<IMyGravityProvider>.Enumerator> enumerator = m_naturalGravityGenerators.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IMyGravityProvider current = enumerator.Current;
                    if (current != null)
                    {
                        MySphericalNaturalGravityComponent component = current as MySphericalNaturalGravityComponent;
                        if (component != null)
                        {
                            BoundingSphereD ed = new BoundingSphereD(component.Position, component.GravityLimit + raySize);
                            if (ray.Intersects((BoundingSphere) ed) != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static double GetStrongestNaturalGravityWell(Vector3D worldPosition, out IMyGravityProvider nearestProvider)
        {
            double minValue = double.MinValue;
            nearestProvider = null;
            m_naturalGravityGenerators.ApplyChanges();
            foreach (IMyGravityProvider provider in m_naturalGravityGenerators)
            {
                float num2 = provider.GetWorldGravity(worldPosition).Length();
                if (num2 > minValue)
                {
                    minValue = num2;
                    nearestProvider = provider;
                }
            }
            return minValue;
        }

        public static bool IsGravityReady() => 
            !m_artificialGravityGenerators.IsRootNull();

        public static bool IsPositionInNaturalGravity(Vector3D position, double sphereSize = 0.0)
        {
            double num1 = MathHelper.Max(sphereSize, 0.0);
            sphereSize = num1;
            m_naturalGravityGenerators.ApplyChanges();
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, IMyGravityProvider, List<IMyGravityProvider>.Enumerator> enumerator = m_naturalGravityGenerators.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IMyGravityProvider current = enumerator.Current;
                    if ((current != null) && current.IsPositionInRange(position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void OnGravityGeneratorMoved(IMyGravityProvider gravityGenerator, ref Vector3 velocity)
        {
            int num;
            if (m_proxyIdMap.TryGetValue(gravityGenerator, out num))
            {
                BoundingBoxD xd;
                gravityGenerator.GetProxyAABB(out xd);
                m_artificialGravityGenerators.MoveProxy(num, ref xd, velocity);
            }
        }

        public static void RemoveGravityGenerator(IMyGravityProvider gravityGenerator)
        {
            int num;
            if (m_proxyIdMap.TryGetValue(gravityGenerator, out num))
            {
                m_artificialGravityGenerators.RemoveProxy(num);
                m_proxyIdMap.Remove(gravityGenerator);
            }
        }

        public static void RemoveNaturalGravityProvider(IMyGravityProvider gravityGenerator)
        {
            m_naturalGravityGenerators.Remove(gravityGenerator, false);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            m_naturalGravityGenerators.ApplyChanges();
            if (m_proxyIdMap.Count <= 0)
            {
                int count = m_naturalGravityGenerators.Count;
            }
            m_proxyIdMap.Clear();
            m_artificialGravityGenerators.Clear();
            m_naturalGravityGenerators.ClearImmediate();
        }

        private class GravityCollector
        {
            public Vector3 Gravity;
            private readonly Func<int, bool> CollectAction;
            private Vector3D WorldPoint;
            private MyDynamicAABBTreeD Tree;

            public GravityCollector()
            {
                this.CollectAction = new Func<int, bool>(this.CollectCallback);
            }

            public void Collect(MyDynamicAABBTreeD tree, ref Vector3D worldPoint)
            {
                this.Tree = tree;
                this.WorldPoint = worldPoint;
                tree.QueryPoint(this.CollectAction, ref worldPoint);
            }

            private bool CollectCallback(int proxyId)
            {
                IMyGravityProvider userData = this.Tree.GetUserData<IMyGravityProvider>(proxyId);
                if (userData.IsWorking && userData.IsPositionInRange(this.WorldPoint))
                {
                    this.Gravity += userData.GetWorldGravity(this.WorldPoint);
                }
                return true;
            }
        }
    }
}


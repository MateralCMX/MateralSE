namespace Sandbox.ModAPI.Physics
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.ModAPI;
    using VRage.Scripting;
    using VRage.Utils;
    using VRageMath;

    internal class MyPhysics : IMyPhysics
    {
        public static readonly Sandbox.ModAPI.Physics.MyPhysics Static = new Sandbox.ModAPI.Physics.MyPhysics();
        private MyConcurrentPool<List<Sandbox.Engine.Physics.MyPhysics.HitInfo>> m_collectorsPool;

        public MyPhysics()
        {
            this.m_collectorsPool = new MyConcurrentPool<List<Sandbox.Engine.Physics.MyPhysics.HitInfo>>(10, x => x.Clear(), 0x2710, null);
        }

        private void AssertMainThread()
        {
            if (!ReferenceEquals(MyUtils.MainThread, Thread.CurrentThread))
            {
                MyModWatchdog.ReportIncorrectBehaviour(MyCommonTexts.ModRuleViolation_PhysicsParallelAccess);
            }
        }

        public void CastRayParallel(ref Vector3D from, ref Vector3D to, int raycastCollisionFilter, Action<IHitInfo> callback)
        {
            Sandbox.Engine.Physics.MyPhysics.CastRayParallel(ref from, ref to, raycastCollisionFilter, x => callback((IHitInfo) x));
        }

        public void CastRayParallel(ref Vector3D from, ref Vector3D to, List<IHitInfo> toList, int raycastCollisionFilter, Action<List<IHitInfo>> callback)
        {
            Sandbox.Engine.Physics.MyPhysics.CastRayParallel(ref from, ref to, this.m_collectorsPool.Get(), raycastCollisionFilter, delegate (List<Sandbox.Engine.Physics.MyPhysics.HitInfo> hits) {
                foreach (Sandbox.Engine.Physics.MyPhysics.HitInfo info in hits)
                {
                    toList.Add(info);
                }
                this.m_collectorsPool.Return(hits);
                callback(toList);
            });
        }

        bool IMyPhysics.CastLongRay(Vector3D from, Vector3D to, out IHitInfo hitInfo, bool any)
        {
            this.AssertMainThread();
            Sandbox.Engine.Physics.MyPhysics.HitInfo? nullable = Sandbox.Engine.Physics.MyPhysics.CastLongRay(from, to, any);
            if (nullable != null)
            {
                hitInfo = (IHitInfo) nullable;
                return true;
            }
            hitInfo = null;
            return false;
        }

        bool IMyPhysics.CastRay(Vector3D from, Vector3D to, out IHitInfo hitInfo, int raycastFilterLayer)
        {
            this.AssertMainThread();
            Sandbox.Engine.Physics.MyPhysics.HitInfo? nullable = Sandbox.Engine.Physics.MyPhysics.CastRay(from, to, raycastFilterLayer);
            if (nullable != null)
            {
                hitInfo = (IHitInfo) nullable;
                return true;
            }
            hitInfo = null;
            return false;
        }

        void IMyPhysics.CastRay(Vector3D from, Vector3D to, List<IHitInfo> toList, int raycastFilterLayer)
        {
            this.AssertMainThread();
            List<Sandbox.Engine.Physics.MyPhysics.HitInfo> list = this.m_collectorsPool.Get();
            toList.Clear();
            Sandbox.Engine.Physics.MyPhysics.CastRay(from, to, list, raycastFilterLayer);
            foreach (Sandbox.Engine.Physics.MyPhysics.HitInfo info in list)
            {
                toList.Add(info);
            }
            this.m_collectorsPool.Return(list);
        }

        bool IMyPhysics.CastRay(Vector3D from, Vector3D to, out IHitInfo hitInfo, uint raycastCollisionFilter, bool ignoreConvexShape)
        {
            Sandbox.Engine.Physics.MyPhysics.HitInfo info;
            this.AssertMainThread();
            bool flag1 = Sandbox.Engine.Physics.MyPhysics.CastRay(from, to, out info, raycastCollisionFilter, ignoreConvexShape);
            hitInfo = info;
            return flag1;
        }

        void IMyPhysics.EnsurePhysicsSpace(BoundingBoxD aabb)
        {
            this.AssertMainThread();
            Sandbox.Engine.Physics.MyPhysics.EnsurePhysicsSpace(aabb);
        }

        int IMyPhysics.GetCollisionLayer(string strLayer) => 
            Sandbox.Engine.Physics.MyPhysics.GetCollisionLayer(strLayer);

        int IMyPhysics.StepsLastSecond =>
            Sandbox.Engine.Physics.MyPhysics.StepsLastSecond;

        float IMyPhysics.SimulationRatio =>
            Sandbox.Engine.Physics.MyPhysics.SimulationRatio;

        float IMyPhysics.ServerSimulationRatio =>
            Sync.ServerSimulationRatio;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Sandbox.ModAPI.Physics.MyPhysics.<>c <>9 = new Sandbox.ModAPI.Physics.MyPhysics.<>c();
            public static Action<List<Sandbox.Engine.Physics.MyPhysics.HitInfo>> <>9__18_0;

            internal void <.ctor>b__18_0(List<Sandbox.Engine.Physics.MyPhysics.HitInfo> x)
            {
                x.Clear();
            }
        }
    }
}


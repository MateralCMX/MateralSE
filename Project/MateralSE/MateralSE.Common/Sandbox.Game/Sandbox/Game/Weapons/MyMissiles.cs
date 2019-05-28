namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyMissiles : MySessionComponentBase
    {
        private static readonly Dictionary<MissileId, Queue<MyMissile>> m_missiles = new Dictionary<MissileId, Queue<MyMissile>>();
        private static readonly MyDynamicAABBTreeD m_missileTree = new MyDynamicAABBTreeD(Vector3D.One * 10.0, 10.0);

        public static void Add(MyObjectBuilder_Missile builder)
        {
            Queue<MyMissile> queue;
            MissileId key = new MissileId {
                AmmoMagazineId = builder.AmmoMagazineId,
                WeaponDefinitionId = builder.WeaponDefinitionId
            };
            if (!m_missiles.TryGetValue(key, out queue) || (queue.Count <= 0))
            {
                Vector3D? relativeOffset = null;
                MyEntities.CreateFromObjectBuilderParallel(builder, true, delegate (MyEntity x) {
                    MyMissile missile = x as MyMissile;
                    missile.m_pruningProxyId = -1;
                    RegisterMissile(missile);
                }, null, null, relativeOffset, false, false);
            }
            else
            {
                MyMissile entity = queue.Dequeue();
                entity.UpdateData(builder);
                entity.m_pruningProxyId = -1;
                MyEntities.Add(entity, true);
                RegisterMissile(entity);
            }
        }

        public static void GetAllMissilesInSphere(ref BoundingSphereD sphere, List<MyEntity> result)
        {
            m_missileTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
        }

        public override void LoadData()
        {
        }

        public static void OnMissileMoved(MyMissile missile, ref Vector3 velocity)
        {
            if (missile.m_pruningProxyId != -1)
            {
                BoundingBoxD xd;
                BoundingSphereD sphere = new BoundingSphereD(missile.PositionComp.GetPosition(), 1.0);
                BoundingBoxD.CreateFromSphere(ref sphere, out xd);
                m_missileTree.MoveProxy(missile.m_pruningProxyId, ref xd, velocity);
            }
        }

        private static void RegisterMissile(MyMissile missile)
        {
            if (missile.m_pruningProxyId == -1)
            {
                BoundingBoxD xd;
                BoundingSphereD sphere = new BoundingSphereD(missile.PositionComp.GetPosition(), 1.0);
                BoundingBoxD.CreateFromSphere(ref sphere, out xd);
                missile.m_pruningProxyId = m_missileTree.AddProxy(ref xd, missile, 0, true);
            }
        }

        public static void Remove(long entityId)
        {
            MyMissile entityById = MyEntities.GetEntityById(entityId, false) as MyMissile;
            if (entityById != null)
            {
                Return(entityById);
            }
        }

        public static void Return(MyMissile missile)
        {
            if (missile.InScene)
            {
                Queue<MyMissile> queue;
                MissileId key = new MissileId {
                    AmmoMagazineId = missile.AmmoMagazineId,
                    WeaponDefinitionId = missile.WeaponDefinitionId
                };
                if (!m_missiles.TryGetValue(key, out queue))
                {
                    queue = new Queue<MyMissile>();
                    m_missiles.Add(key, queue);
                }
                queue.Enqueue(missile);
                MyEntities.Remove(missile);
                UnregisterMissile(missile);
            }
        }

        protected override void UnloadData()
        {
            foreach (KeyValuePair<MissileId, Queue<MyMissile>> pair in m_missiles)
            {
                while (pair.Value.Count > 0)
                {
                    pair.Value.Dequeue().Close();
                }
            }
            m_missiles.Clear();
            m_missileTree.Clear();
        }

        private static void UnregisterMissile(MyMissile missile)
        {
            if (missile.m_pruningProxyId != -1)
            {
                m_missileTree.RemoveProxy(missile.m_pruningProxyId);
                missile.m_pruningProxyId = -1;
            }
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyPhysics) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMissiles.<>c <>9 = new MyMissiles.<>c();
            public static Action<MyEntity> <>9__7_0;

            internal void <Add>b__7_0(MyEntity x)
            {
                MyMissile missile = x as MyMissile;
                missile.m_pruningProxyId = -1;
                MyMissiles.RegisterMissile(missile);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MissileId
        {
            public SerializableDefinitionId WeaponDefinitionId;
            public SerializableDefinitionId AmmoMagazineId;
        }
    }
}


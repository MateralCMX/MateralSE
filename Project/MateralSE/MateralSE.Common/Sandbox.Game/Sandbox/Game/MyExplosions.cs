namespace Sandbox.Game
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Generics;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyExplosions : MySessionComponentBase
    {
        private static MyObjectsPool<MyExplosion> m_explosions = null;
        private static List<MyExplosionInfo> m_explosionBuffer1 = new List<MyExplosionInfo>();
        private static List<MyExplosionInfo> m_explosionBuffer2 = new List<MyExplosionInfo>();
        private static List<MyExplosionInfo> m_explosionsRead = m_explosionBuffer1;
        private static List<MyExplosionInfo> m_explosionsWrite = m_explosionBuffer2;
        private static List<MyExplosion> m_exploded = new List<MyExplosion>();
        private static HashSet<long> m_activeEntityKickbacks = new HashSet<long>();
        private static SortedDictionary<long, long> m_activeEntityKickbacksByTime = new SortedDictionary<long, long>();

        public static void AddExplosion(ref MyExplosionInfo explosionInfo, bool updateSync = true)
        {
            if (MySessionComponentSafeZones.IsActionAllowed(BoundingBoxD.CreateFromSphere(explosionInfo.ExplosionSphere), MySafeZoneAction.Damage, 0L))
            {
                if (Sync.IsServer & updateSync)
                {
                    MyExplosionInfoSimplified simplified = new MyExplosionInfoSimplified {
                        Damage = explosionInfo.Damage,
                        Center = explosionInfo.ExplosionSphere.Center,
                        Radius = (float) explosionInfo.ExplosionSphere.Radius,
                        Type = explosionInfo.ExplosionType,
                        Flags = explosionInfo.ExplosionFlags,
                        VoxelCenter = explosionInfo.VoxelExplosionCenter,
                        ParticleScale = explosionInfo.ParticleScale,
                        Velocity = explosionInfo.Velocity
                    };
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseStaticEvent<MyExplosionInfoSimplified>(s => new Action<MyExplosionInfoSimplified>(MyExplosions.ProxyExplosionRequest), simplified, targetEndpoint, new Vector3D?(explosionInfo.ExplosionSphere.Center));
                }
                m_explosionsWrite.Add(explosionInfo);
            }
        }

        public override void Draw()
        {
            using (HashSet<MyExplosion>.Enumerator enumerator = m_explosions.Active.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDraw();
                }
            }
        }

        public override void LoadData()
        {
            MySandboxGame.Log.WriteLine("MyExplosions.LoadData() - START");
            MySandboxGame.Log.IncreaseIndent();
            if (m_explosions == null)
            {
                m_explosions = new MyObjectsPool<MyExplosion>(0x400, null);
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyExplosions.LoadData() - END");
        }

        [Event(null, 0xa7), Reliable, ServerInvoked, BroadcastExcept]
        private static void ProxyExplosionRequest(MyExplosionInfoSimplified explosionInfo)
        {
            if (MySession.Static.Ready && !MyEventContext.Current.IsLocallyInvoked)
            {
                MyExplosionInfo info2 = new MyExplosionInfo {
                    PlayerDamage = 0f,
                    Damage = explosionInfo.Damage,
                    ExplosionType = explosionInfo.Type,
                    ExplosionSphere = new BoundingSphere((Vector3) explosionInfo.Center, explosionInfo.Radius),
                    LifespanMiliseconds = 700,
                    HitEntity = null,
                    ParticleScale = explosionInfo.ParticleScale,
                    OwnerEntity = null,
                    Direction = new Vector3?(Vector3.Forward),
                    VoxelExplosionCenter = explosionInfo.VoxelCenter,
                    ExplosionFlags = explosionInfo.Flags,
                    VoxelCutoutScale = 1f,
                    PlaySound = true,
                    ObjectsRemoveDelayInMiliseconds = 40,
                    Velocity = explosionInfo.Velocity
                };
                AddExplosion(ref info2, false);
            }
        }

        public static bool ShouldUseMassScaleForEntity(MyEntity entity)
        {
            long entityId = entity.EntityId;
            if (m_activeEntityKickbacks.Contains(entityId))
            {
                return false;
            }
            long key = (MySession.Static.ElapsedGameTime + TimeSpan.FromSeconds(2.0)).Ticks + (entityId % ((long) 100));
            while (m_activeEntityKickbacksByTime.ContainsKey(key))
            {
                key += 1L;
            }
            m_activeEntityKickbacks.Add(entityId);
            m_activeEntityKickbacksByTime.Add(key, entityId);
            return true;
        }

        private void SwapBuffers()
        {
            if (ReferenceEquals(m_explosionBuffer1, m_explosionsRead))
            {
                m_explosionsWrite = m_explosionBuffer1;
                m_explosionsRead = m_explosionBuffer2;
            }
            else
            {
                m_explosionsWrite = m_explosionBuffer2;
                m_explosionsRead = m_explosionBuffer1;
            }
        }

        protected override void UnloadData()
        {
            if ((m_explosions != null) && (m_explosions.ActiveCount > 0))
            {
                foreach (MyExplosion explosion in m_explosions.Active)
                {
                    if (explosion != null)
                    {
                        explosion.Close();
                    }
                }
                m_explosions.DeallocateAll();
            }
            m_explosionsRead.Clear();
            m_explosionsWrite.Clear();
            m_activeEntityKickbacks.Clear();
            m_activeEntityKickbacksByTime.Clear();
        }

        public override void UpdateBeforeSimulation()
        {
            this.SwapBuffers();
            this.UpdateEntityKickbacks();
            foreach (MyExplosionInfo info in m_explosionsRead)
            {
                MyExplosion item = null;
                m_explosions.AllocateOrCreate(out item);
                if (item != null)
                {
                    item.Start(info);
                }
            }
            m_explosionsRead.Clear();
            foreach (MyExplosion explosion2 in m_explosions.Active)
            {
                if (explosion2.Update())
                {
                    m_exploded.Add(explosion2);
                    continue;
                }
                m_exploded.Add(explosion2);
                m_explosions.MarkForDeallocate(explosion2);
            }
            foreach (MyExplosion local1 in m_exploded)
            {
                local1.ApplyVolumetricDamageToGrid();
                local1.Clear();
            }
            m_exploded.Clear();
            m_explosions.DeallocateAllMarked();
            MyDebris.Static.UpdateBeforeSimulation();
        }

        private void UpdateEntityKickbacks()
        {
            long ticks = MySession.Static.ElapsedGameTime.Ticks;
            while (m_activeEntityKickbacksByTime.Count != 0)
            {
                using (SortedDictionary<long, long>.Enumerator enumerator = m_activeEntityKickbacksByTime.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        continue;
                    }
                    KeyValuePair<long, long> current = enumerator.Current;
                    if (current.Key <= ticks)
                    {
                        long item = current.Value;
                        m_activeEntityKickbacks.Remove(item);
                        m_activeEntityKickbacksByTime.Remove(current.Key);
                        continue;
                    }
                }
                return;
            }
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyLights) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyExplosions.<>c <>9 = new MyExplosions.<>c();
            public static Func<IMyEventOwner, Action<MyExplosionInfoSimplified>> <>9__13_0;

            internal Action<MyExplosionInfoSimplified> <AddExplosion>b__13_0(IMyEventOwner s) => 
                new Action<MyExplosionInfoSimplified>(MyExplosions.ProxyExplosionRequest);
        }
    }
}


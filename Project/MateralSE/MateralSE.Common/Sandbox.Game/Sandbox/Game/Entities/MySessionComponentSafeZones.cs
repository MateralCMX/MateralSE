namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x7d0, typeof(MyObjectBuilder_SessionComponentSafeZones), (Type) null)]
    public class MySessionComponentSafeZones : MySessionComponentBase
    {
        private static MyConcurrentList<MySafeZone> m_safeZones = new MyConcurrentList<MySafeZone>();
        [CompilerGenerated]
        private static EventHandler OnAddSafeZone;
        [CompilerGenerated]
        private static EventHandler OnRemoveSafeZone;
        public static MySafeZoneAction AllowedActions = MySafeZoneAction.All;
        private static HashSet<MyEntity> m_entitiesToForget = new HashSet<MyEntity>();
        private static HashSet<MyEntity> m_recentlyAddedEntities = new HashSet<MyEntity>();
        private static HashSet<MyEntity> m_recentlyRemovedEntities = new HashSet<MyEntity>();
        private const int FRAMES_TO_REMOVE_RECENT = 100;
        private int m_recentCounter;

        public static  event EventHandler OnAddSafeZone
        {
            [CompilerGenerated] add
            {
                EventHandler onAddSafeZone = OnAddSafeZone;
                while (true)
                {
                    EventHandler a = onAddSafeZone;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onAddSafeZone = Interlocked.CompareExchange<EventHandler>(ref OnAddSafeZone, handler3, a);
                    if (ReferenceEquals(onAddSafeZone, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onAddSafeZone = OnAddSafeZone;
                while (true)
                {
                    EventHandler source = onAddSafeZone;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onAddSafeZone = Interlocked.CompareExchange<EventHandler>(ref OnAddSafeZone, handler3, source);
                    if (ReferenceEquals(onAddSafeZone, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event EventHandler OnRemoveSafeZone
        {
            [CompilerGenerated] add
            {
                EventHandler onRemoveSafeZone = OnRemoveSafeZone;
                while (true)
                {
                    EventHandler a = onRemoveSafeZone;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onRemoveSafeZone = Interlocked.CompareExchange<EventHandler>(ref OnRemoveSafeZone, handler3, a);
                    if (ReferenceEquals(onRemoveSafeZone, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onRemoveSafeZone = OnRemoveSafeZone;
                while (true)
                {
                    EventHandler source = onRemoveSafeZone;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onRemoveSafeZone = Interlocked.CompareExchange<EventHandler>(ref OnRemoveSafeZone, handler3, source);
                    if (ReferenceEquals(onRemoveSafeZone, source))
                    {
                        return;
                    }
                }
            }
        }

        public static void AddSafeZone(MySafeZone safeZone)
        {
            m_safeZones.Add(safeZone);
            if (OnAddSafeZone != null)
            {
                OnAddSafeZone(safeZone, null);
            }
        }

        [Event(null, 0x6d), Reliable, Server]
        public static void CreateSafeZone_Implementation(Vector3D position)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyObjectBuilder_SafeZone objectBuilder = new MyObjectBuilder_SafeZone();
                objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(position, Vector3.Forward, Vector3.Up);
                objectBuilder.Radius = 100f;
                objectBuilder.PersistentFlags = MyPersistentEntityFlags2.InScene;
                MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false);
            }
        }

        [Event(null, 0x86), Reliable, Server]
        public static void DeleteSafeZone_Implementation(long entityId)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyEntity entity = null;
                if (MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    entity.Close();
                }
            }
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SessionComponentSafeZones zones1 = new MyObjectBuilder_SessionComponentSafeZones();
            zones1.AllowedActions = AllowedActions;
            return zones1;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            AllowedActions = (sessionComponent as MyObjectBuilder_SessionComponentSafeZones).AllowedActions;
        }

        public static bool IsActionAllowed(MyEntity entity, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            if (!AllowedActions.HasFlag(action))
            {
                return false;
            }
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MySafeZone, List<MySafeZone>.Enumerator> enumerator = m_safeZones.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!enumerator.Current.IsActionAllowed(entity, action, sourceEntityId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsActionAllowed(BoundingBoxD aabb, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            if (!AllowedActions.HasFlag(action))
            {
                return false;
            }
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MySafeZone, List<MySafeZone>.Enumerator> enumerator = m_safeZones.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!enumerator.Current.IsActionAllowed(aabb, action, sourceEntityId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsActionAllowed(Vector3D point, MySafeZoneAction action, long sourceEntityId = 0L)
        {
            if (!AllowedActions.HasFlag(action))
            {
                return false;
            }
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MySafeZone, List<MySafeZone>.Enumerator> enumerator = m_safeZones.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!enumerator.Current.IsActionAllowed(point, action, sourceEntityId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsRecentlyAddedOrRemoved(MyEntity obj) => 
            (m_recentlyAddedEntities.Contains(obj) || m_recentlyRemovedEntities.Contains(obj));

        public override void LoadData()
        {
            base.LoadData();
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityAdd);
                MyEntities.OnEntityRemove += new Action<MyEntity>(this.MyEntities_OnEntityRemove);
                MyEntities.OnEntityDelete += new Action<MyEntity>(this.MyEntities_OnEntityDelete);
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity obj)
        {
            if ((obj.Physics != null) && obj.Physics.IsStatic)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MySafeZone, List<MySafeZone>.Enumerator> enumerator = m_safeZones.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InsertEntity(obj);
                    }
                }
            }
            m_recentlyAddedEntities.Add(obj);
            this.m_recentCounter = 100;
        }

        private void MyEntities_OnEntityDelete(MyEntity obj)
        {
            m_entitiesToForget.Add(obj);
        }

        private void MyEntities_OnEntityRemove(MyEntity obj)
        {
            if ((obj.Physics != null) && obj.Physics.IsStatic)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MySafeZone, List<MySafeZone>.Enumerator> enumerator = m_safeZones.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveEntityInternal(obj, true);
                    }
                }
            }
            m_recentlyRemovedEntities.Add(obj);
            this.m_recentCounter = 100;
        }

        public static void RemoveSafeZone(MySafeZone safeZone)
        {
            m_safeZones.Remove(safeZone);
            if (OnRemoveSafeZone != null)
            {
                OnRemoveSafeZone(safeZone, null);
            }
        }

        public static void RequestCreateSafeZone(Vector3D position)
        {
            if (MySession.Static.IsUserAdmin(Sync.MyId))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? nullable = null;
                MyMultiplayer.RaiseStaticEvent<Vector3D>(x => new Action<Vector3D>(MySessionComponentSafeZones.CreateSafeZone_Implementation), position, targetEndpoint, nullable);
            }
        }

        public static void RequestDeleteSafeZone(long entityId)
        {
            if (MySession.Static.IsUserAdmin(Sync.MyId))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MySessionComponentSafeZones.DeleteSafeZone_Implementation), entityId, targetEndpoint, position);
            }
        }

        public static void RequestUpdateGlobalSafeZone()
        {
            if (MySession.Static.IsUserAdmin(Sync.MyId))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MySafeZoneAction>(x => new Action<MySafeZoneAction>(MySessionComponentSafeZones.UpdateGlobalSafeZone_Implementation), AllowedActions, targetEndpoint, position);
            }
            else if (!MyEventContext.Current.IsLocallyInvoked)
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        public static void RequestUpdateSafeZone(MyObjectBuilder_SafeZone ob)
        {
            if (MySession.Static.IsUserAdmin(Sync.MyId))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyObjectBuilder_SafeZone>(x => new Action<MyObjectBuilder_SafeZone>(MySessionComponentSafeZones.UpdateSafeZone_Implementation), ob, targetEndpoint, position);
            }
        }

        protected override void UnloadData()
        {
            m_safeZones.Clear();
            m_entitiesToForget.Clear();
            m_recentlyAddedEntities.Clear();
            m_recentlyRemovedEntities.Clear();
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_recentCounter > 0)
            {
                this.m_recentCounter--;
                if (this.m_recentCounter == 0)
                {
                    m_entitiesToForget.Clear();
                    m_recentlyAddedEntities.Clear();
                    m_recentlyRemovedEntities.Clear();
                }
            }
            if (m_entitiesToForget.Count > 0)
            {
                foreach (MyEntity entity in m_entitiesToForget)
                {
                    m_recentlyAddedEntities.Remove(entity);
                    m_recentlyRemovedEntities.Remove(entity);
                }
                m_entitiesToForget.Clear();
            }
        }

        [Event(null, 0xbc), Reliable, Server, Broadcast]
        public static void UpdateGlobalSafeZone_Implementation(MySafeZoneAction allowedActions)
        {
            if (!MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                AllowedActions = allowedActions;
            }
        }

        [Event(null, 170), Reliable, Server, Broadcast]
        public static void UpdateSafeZone_Implementation(MyObjectBuilder_SafeZone ob)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                MyEventContext.ValidationFailed();
            }
            else
            {
                MySafeZone entity = null;
                if (MyEntities.TryGetEntityById<MySafeZone>(ob.EntityId, out entity, false))
                {
                    entity.InitInternal(ob, true);
                }
            }
        }

        public override bool IsRequiredByGame =>
            true;

        public static ListReader<MySafeZone> SafeZones =>
            m_safeZones.List;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentSafeZones.<>c <>9 = new MySessionComponentSafeZones.<>c();
            public static Func<IMyEventOwner, Action<Vector3D>> <>9__21_0;
            public static Func<IMyEventOwner, Action<long>> <>9__23_0;
            public static Func<IMyEventOwner, Action<MyObjectBuilder_SafeZone>> <>9__25_0;
            public static Func<IMyEventOwner, Action<MySafeZoneAction>> <>9__26_0;

            internal Action<Vector3D> <RequestCreateSafeZone>b__21_0(IMyEventOwner x) => 
                new Action<Vector3D>(MySessionComponentSafeZones.CreateSafeZone_Implementation);

            internal Action<long> <RequestDeleteSafeZone>b__23_0(IMyEventOwner x) => 
                new Action<long>(MySessionComponentSafeZones.DeleteSafeZone_Implementation);

            internal Action<MySafeZoneAction> <RequestUpdateGlobalSafeZone>b__26_0(IMyEventOwner x) => 
                new Action<MySafeZoneAction>(MySessionComponentSafeZones.UpdateGlobalSafeZone_Implementation);

            internal Action<MyObjectBuilder_SafeZone> <RequestUpdateSafeZone>b__25_0(IMyEventOwner x) => 
                new Action<MyObjectBuilder_SafeZone>(MySessionComponentSafeZones.UpdateSafeZone_Implementation);
        }
    }
}


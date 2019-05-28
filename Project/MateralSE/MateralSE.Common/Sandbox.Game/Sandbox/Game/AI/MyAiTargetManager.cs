namespace Sandbox.Game.AI
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner, PreloadRequired, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyAiTargetManager : MySessionComponentBase
    {
        private HashSet<MyAiTargetBase> m_aiTargets = new HashSet<MyAiTargetBase>();
        private static Dictionary<KeyValuePair<long, long>, ReservedEntityData> m_reservedEntities;
        private static Dictionary<string, Dictionary<long, ReservedAreaData>> m_reservedAreas;
        private static Queue<KeyValuePair<long, long>> m_removeReservedEntities;
        private static Queue<KeyValuePair<string, long>> m_removeReservedAreas;
        private static long AreaReservationCounter;
        public static MyAiTargetManager Static;
        [CompilerGenerated]
        private static ReservationHandler OnReservationResult;
        [CompilerGenerated]
        private static AreaReservationHandler OnAreaReservationResult;

        public static  event AreaReservationHandler OnAreaReservationResult
        {
            [CompilerGenerated] add
            {
                AreaReservationHandler onAreaReservationResult = OnAreaReservationResult;
                while (true)
                {
                    AreaReservationHandler a = onAreaReservationResult;
                    AreaReservationHandler handler3 = (AreaReservationHandler) Delegate.Combine(a, value);
                    onAreaReservationResult = Interlocked.CompareExchange<AreaReservationHandler>(ref OnAreaReservationResult, handler3, a);
                    if (ReferenceEquals(onAreaReservationResult, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                AreaReservationHandler onAreaReservationResult = OnAreaReservationResult;
                while (true)
                {
                    AreaReservationHandler source = onAreaReservationResult;
                    AreaReservationHandler handler3 = (AreaReservationHandler) Delegate.Remove(source, value);
                    onAreaReservationResult = Interlocked.CompareExchange<AreaReservationHandler>(ref OnAreaReservationResult, handler3, source);
                    if (ReferenceEquals(onAreaReservationResult, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event ReservationHandler OnReservationResult
        {
            [CompilerGenerated] add
            {
                ReservationHandler onReservationResult = OnReservationResult;
                while (true)
                {
                    ReservationHandler a = onReservationResult;
                    ReservationHandler handler3 = (ReservationHandler) Delegate.Combine(a, value);
                    onReservationResult = Interlocked.CompareExchange<ReservationHandler>(ref OnReservationResult, handler3, a);
                    if (ReferenceEquals(onReservationResult, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ReservationHandler onReservationResult = OnReservationResult;
                while (true)
                {
                    ReservationHandler source = onReservationResult;
                    ReservationHandler handler3 = (ReservationHandler) Delegate.Remove(source, value);
                    onReservationResult = Interlocked.CompareExchange<ReservationHandler>(ref OnReservationResult, handler3, source);
                    if (ReferenceEquals(onReservationResult, source))
                    {
                        return;
                    }
                }
            }
        }

        public static void AddAiTarget(MyAiTargetBase aiTarget)
        {
            if (Static != null)
            {
                Static.m_aiTargets.Add(aiTarget);
            }
        }

        public bool IsEntityReserved(long entityId) => 
            this.IsEntityReserved(entityId, 0L);

        public bool IsEntityReserved(long entityId, long localId)
        {
            ReservedEntityData data;
            return (Sync.IsServer ? m_reservedEntities.TryGetValue(new KeyValuePair<long, long>(entityId, localId), out data) : false);
        }

        public bool IsInReservedArea(string areaName, Vector3D position)
        {
            Dictionary<long, ReservedAreaData> dictionary = null;
            if (m_reservedAreas.TryGetValue(areaName, out dictionary))
            {
                using (Dictionary<long, ReservedAreaData>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        ReservedAreaData current = enumerator.Current;
                        Vector3D vectord = current.WorldPosition - position;
                        if (vectord.LengthSquared() < (current.Radius * current.Radius))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void LoadData()
        {
            Static = this;
            m_reservedEntities = new Dictionary<KeyValuePair<long, long>, ReservedEntityData>();
            m_removeReservedEntities = new Queue<KeyValuePair<long, long>>();
            m_removeReservedAreas = new Queue<KeyValuePair<string, long>>();
            m_reservedAreas = new Dictionary<string, Dictionary<long, ReservedAreaData>>();
            MyEntities.OnEntityRemove += new Action<MyEntity>(this.OnEntityRemoved);
        }

        private void OnEntityRemoved(MyEntity entity)
        {
            foreach (MyAiTargetBase base2 in this.m_aiTargets)
            {
                if (ReferenceEquals(base2.TargetEntity, entity))
                {
                    base2.UnsetTarget();
                }
            }
        }

        [Event(null, 0x1a9), Reliable, Broadcast]
        private static void OnReserveAreaAllSuccess(long id, string reservationName, Vector3D position, float radius)
        {
            if (!m_reservedAreas.ContainsKey(reservationName))
            {
                m_reservedAreas[reservationName] = new Dictionary<long, ReservedAreaData>();
            }
            ReservedAreaData data = new ReservedAreaData {
                WorldPosition = position,
                Radius = radius
            };
            m_reservedAreas[reservationName].Add(id, data);
        }

        [Event(null, 0x1b2), Reliable, Broadcast]
        private static void OnReserveAreaCancel(string reservationName, long id)
        {
            Dictionary<long, ReservedAreaData> dictionary;
            if (m_reservedAreas.TryGetValue(reservationName, out dictionary))
            {
                dictionary.Remove(id);
            }
        }

        [Event(null, 410), Reliable, Client]
        private static void OnReserveAreaFailure(Vector3D position, float radius, int senderSerialId)
        {
            if (OnAreaReservationResult != null)
            {
                ReservedAreaData entityData = new ReservedAreaData {
                    WorldPosition = position,
                    Radius = radius,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnAreaReservationResult(ref entityData, false);
            }
        }

        [Event(null, 340), Reliable, Server]
        private static void OnReserveAreaRequest(string reservationName, Vector3D position, float radius, long reservationTimeMs, int senderSerialId)
        {
            EndpointId sender;
            Vector3D? nullable;
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                sender = new EndpointId(Sync.MyId);
            }
            else
            {
                sender = MyEventContext.Current.Sender;
            }
            if (!m_reservedAreas.ContainsKey(reservationName))
            {
                m_reservedAreas.Add(reservationName, new Dictionary<long, ReservedAreaData>());
            }
            Dictionary<long, ReservedAreaData> dictionary = m_reservedAreas[reservationName];
            bool flag = false;
            MyPlayer.PlayerId id2 = new MyPlayer.PlayerId(sender.Value, senderSerialId);
            foreach (KeyValuePair<long, ReservedAreaData> pair in dictionary)
            {
                ReservedAreaData data = pair.Value;
                Vector3D vectord = data.WorldPosition - position;
                if (vectord.LengthSquared() <= (data.Radius * data.Radius))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    OnReserveAreaFailure(position, radius, senderSerialId);
                }
                else
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<Vector3D, float, int>(s => new Action<Vector3D, float, int>(MyAiTargetManager.OnReserveAreaFailure), position, radius, senderSerialId, sender, nullable);
                }
            }
            else
            {
                AreaReservationCounter += 1L;
                ReservedAreaData data2 = new ReservedAreaData {
                    WorldPosition = position,
                    Radius = radius,
                    ReservationTimer = MySandboxGame.Static.TotalTime + MyTimeSpan.FromMilliseconds((double) reservationTimeMs),
                    ReserverId = id2
                };
                dictionary[AreaReservationCounter] = data2;
                EndpointId targetEndpoint = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<long, string, Vector3D, float>(s => new Action<long, string, Vector3D, float>(MyAiTargetManager.OnReserveAreaAllSuccess), AreaReservationCounter, reservationName, position, radius, targetEndpoint, nullable);
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    OnReserveAreaSuccess(position, radius, senderSerialId);
                }
                else
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<Vector3D, float, int>(s => new Action<Vector3D, float, int>(MyAiTargetManager.OnReserveAreaSuccess), position, radius, senderSerialId, sender, nullable);
                }
            }
        }

        [Event(null, 0x18b), Reliable, Client]
        private static void OnReserveAreaSuccess(Vector3D position, float radius, int senderSerialId)
        {
            if (OnAreaReservationResult != null)
            {
                ReservedAreaData entityData = new ReservedAreaData {
                    WorldPosition = position,
                    Radius = radius,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnAreaReservationResult(ref entityData, true);
            }
        }

        [Event(null, 0x93), Reliable, Client]
        private static void OnReserveEntityFailure(long entityId, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.ENTITY,
                    EntityId = entityId,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, false);
            }
        }

        [Event(null, 0x5e), Reliable, Server]
        private static void OnReserveEntityRequest(long entityId, long reservationTimeMs, int senderSerialId)
        {
            EndpointId sender;
            ReservedEntityData data;
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                sender = new EndpointId(Sync.MyId);
            }
            else
            {
                sender = MyEventContext.Current.Sender;
            }
            bool flag = true;
            KeyValuePair<long, long> key = new KeyValuePair<long, long>(entityId, 0L);
            if (!m_reservedEntities.TryGetValue(key, out data))
            {
                ReservedEntityData data2 = new ReservedEntityData {
                    EntityId = entityId,
                    ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L),
                    ReserverId = new MyPlayer.PlayerId(sender.Value, senderSerialId)
                };
                m_reservedEntities.Add(key, data2);
            }
            else if (data.ReserverId == new MyPlayer.PlayerId(sender.Value, senderSerialId))
            {
                data.ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L);
            }
            else
            {
                flag = false;
            }
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                if (flag)
                {
                    OnReserveEntitySuccess(entityId, senderSerialId);
                }
                else
                {
                    OnReserveEntityFailure(entityId, senderSerialId);
                }
            }
            else
            {
                Vector3D? nullable;
                if (flag)
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MyAiTargetManager.OnReserveEntitySuccess), entityId, senderSerialId, sender, nullable);
                }
                else
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MyAiTargetManager.OnReserveEntityFailure), entityId, senderSerialId, sender, nullable);
                }
            }
        }

        [Event(null, 0x89), Reliable, Client]
        private static void OnReserveEntitySuccess(long entityId, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.ENTITY,
                    EntityId = entityId,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, true);
            }
        }

        [Event(null, 0xd9), Reliable, Client]
        private static void OnReserveEnvironmentItemFailure(long entityId, int localId, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.ENVIRONMENT_ITEM,
                    EntityId = entityId,
                    LocalId = localId,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, false);
            }
        }

        [Event(null, 0x9d), Reliable, Server]
        private static void OnReserveEnvironmentItemRequest(long entityId, int localId, long reservationTimeMs, int senderSerialId)
        {
            EndpointId sender;
            ReservedEntityData data;
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                sender = new EndpointId(Sync.MyId);
            }
            else
            {
                sender = MyEventContext.Current.Sender;
            }
            bool flag = true;
            KeyValuePair<long, long> key = new KeyValuePair<long, long>(entityId, (long) localId);
            if (!m_reservedEntities.TryGetValue(key, out data))
            {
                ReservedEntityData data2 = new ReservedEntityData {
                    EntityId = entityId,
                    LocalId = localId,
                    ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L),
                    ReserverId = new MyPlayer.PlayerId(sender.Value, senderSerialId)
                };
                m_reservedEntities.Add(key, data2);
            }
            else if (data.ReserverId == new MyPlayer.PlayerId(sender.Value, senderSerialId))
            {
                data.ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L);
            }
            else
            {
                flag = false;
            }
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                if (flag)
                {
                    OnReserveEnvironmentItemSuccess(entityId, localId, senderSerialId);
                }
                else
                {
                    OnReserveEnvironmentItemFailure(entityId, localId, senderSerialId);
                }
            }
            else
            {
                Vector3D? nullable;
                if (flag)
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<long, int, int>(s => new Action<long, int, int>(MyAiTargetManager.OnReserveEnvironmentItemSuccess), entityId, localId, senderSerialId, sender, nullable);
                }
                else
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<long, int, int>(s => new Action<long, int, int>(MyAiTargetManager.OnReserveEnvironmentItemFailure), entityId, localId, senderSerialId, sender, nullable);
                }
            }
        }

        [Event(null, 0xc9), Reliable, Client]
        private static void OnReserveEnvironmentItemSuccess(long entityId, int localId, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.ENVIRONMENT_ITEM,
                    EntityId = entityId,
                    LocalId = localId,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, true);
            }
        }

        [Event(null, 300), Reliable, Client]
        private static void OnReserveVoxelPositionFailure(long entityId, Vector3I voxelPosition, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.VOXEL,
                    EntityId = entityId,
                    GridPos = voxelPosition,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, false);
            }
        }

        [Event(null, 0xe9), Reliable, Server]
        private static void OnReserveVoxelPositionRequest(long entityId, Vector3I voxelPosition, long reservationTimeMs, int senderSerialId)
        {
            EndpointId sender;
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                sender = new EndpointId(Sync.MyId);
            }
            else
            {
                sender = MyEventContext.Current.Sender;
            }
            bool flag = true;
            MyVoxelBase result = null;
            if (MySession.Static.VoxelMaps.Instances.TryGetValue(entityId, out result))
            {
                ReservedEntityData data;
                Vector3I vectori = result.StorageMax - result.StorageMin;
                KeyValuePair<long, long> key = new KeyValuePair<long, long>(entityId, (long) ((voxelPosition.X + (voxelPosition.Y * vectori.X)) + ((voxelPosition.Z * vectori.X) * vectori.Y)));
                if (!m_reservedEntities.TryGetValue(key, out data))
                {
                    ReservedEntityData data2 = new ReservedEntityData {
                        EntityId = entityId,
                        GridPos = voxelPosition,
                        ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L),
                        ReserverId = new MyPlayer.PlayerId(sender.Value, senderSerialId)
                    };
                    m_reservedEntities.Add(key, data2);
                }
                else if (data.ReserverId == new MyPlayer.PlayerId(sender.Value, senderSerialId))
                {
                    data.ReservationTimer = Stopwatch.GetTimestamp() + ((Stopwatch.Frequency * reservationTimeMs) / 0x3e8L);
                }
                else
                {
                    flag = false;
                }
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    if (flag)
                    {
                        OnReserveVoxelPositionSuccess(entityId, voxelPosition, senderSerialId);
                    }
                    else
                    {
                        OnReserveVoxelPositionFailure(entityId, voxelPosition, senderSerialId);
                    }
                }
                else
                {
                    Vector3D? nullable;
                    if (flag)
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent<long, Vector3I, int>(s => new Action<long, Vector3I, int>(MyAiTargetManager.OnReserveVoxelPositionSuccess), entityId, voxelPosition, senderSerialId, sender, nullable);
                    }
                    else
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent<long, Vector3I, int>(s => new Action<long, Vector3I, int>(MyAiTargetManager.OnReserveVoxelPositionFailure), entityId, voxelPosition, senderSerialId, sender, nullable);
                    }
                }
            }
        }

        [Event(null, 0x11c), Reliable, Client]
        private static void OnReserveVoxelPositionSuccess(long entityId, Vector3I voxelPosition, int senderSerialId)
        {
            if (OnReservationResult != null)
            {
                ReservedEntityData entityData = new ReservedEntityData {
                    Type = MyReservedEntityType.VOXEL,
                    EntityId = entityId,
                    GridPos = voxelPosition,
                    ReserverId = new MyPlayer.PlayerId(0L, senderSerialId)
                };
                OnReservationResult(ref entityData, true);
            }
        }

        public static void RemoveAiTarget(MyAiTargetBase aiTarget)
        {
            if (Static != null)
            {
                Static.m_aiTargets.Remove(aiTarget);
            }
        }

        public void RequestAreaReservation(string reservationName, Vector3D position, float radius, long reservationTimeMs, int senderSerialId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<string, Vector3D, float, long, int>(s => new Action<string, Vector3D, float, long, int>(MyAiTargetManager.OnReserveAreaRequest), reservationName, position, radius, reservationTimeMs, senderSerialId, targetEndpoint, nullable);
        }

        public void RequestEntityReservation(long entityId, long reservationTimeMs, int senderSerialId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, long, int>(s => new Action<long, long, int>(MyAiTargetManager.OnReserveEntityRequest), entityId, reservationTimeMs, senderSerialId, targetEndpoint, position);
        }

        public void RequestEnvironmentItemReservation(long entityId, int localId, long reservationTimeMs, int senderSerialId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, long, int>(s => new Action<long, int, long, int>(MyAiTargetManager.OnReserveEnvironmentItemRequest), entityId, localId, reservationTimeMs, senderSerialId, targetEndpoint, position);
        }

        public void RequestVoxelPositionReservation(long entityId, Vector3I voxelPosition, long reservationTimeMs, int senderSerialId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3I, long, int>(s => new Action<long, Vector3I, long, int>(MyAiTargetManager.OnReserveVoxelPositionRequest), entityId, voxelPosition, reservationTimeMs, senderSerialId, targetEndpoint, position);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_aiTargets.Clear();
            MyEntities.OnEntityRemove -= new Action<MyEntity>(this.OnEntityRemoved);
            Static = null;
        }

        public void UnreserveEntity(long entityId)
        {
            this.UnreserveEntity(entityId, 0L);
        }

        public void UnreserveEntity(long entityId, long localId)
        {
            if (Sync.IsServer)
            {
                m_reservedEntities.Remove(new KeyValuePair<long, long>(entityId, localId));
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (Sync.IsServer)
            {
                foreach (KeyValuePair<KeyValuePair<long, long>, ReservedEntityData> pair in m_reservedEntities)
                {
                    if (Stopwatch.GetTimestamp() > pair.Value.ReservationTimer)
                    {
                        m_removeReservedEntities.Enqueue(pair.Key);
                    }
                }
                foreach (KeyValuePair<long, long> pair2 in m_removeReservedEntities)
                {
                    m_reservedEntities.Remove(pair2);
                }
                m_removeReservedEntities.Clear();
                foreach (KeyValuePair<string, Dictionary<long, ReservedAreaData>> pair3 in m_reservedAreas)
                {
                    foreach (KeyValuePair<long, ReservedAreaData> pair4 in pair3.Value)
                    {
                        if (MySandboxGame.Static.TotalTime > pair4.Value.ReservationTimer)
                        {
                            m_removeReservedAreas.Enqueue(new KeyValuePair<string, long>(pair3.Key, pair4.Key));
                        }
                    }
                }
                foreach (KeyValuePair<string, long> pair5 in m_removeReservedAreas)
                {
                    m_reservedAreas[pair5.Key].Remove(pair5.Value);
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<string, long>(s => new Action<string, long>(MyAiTargetManager.OnReserveAreaCancel), pair5.Key, pair5.Value, targetEndpoint, position);
                }
                m_removeReservedAreas.Clear();
            }
        }

        public override bool IsRequiredByGame =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAiTargetManager.<>c <>9 = new MyAiTargetManager.<>c();
            public static Func<IMyEventOwner, Action<long, int>> <>9__21_0;
            public static Func<IMyEventOwner, Action<long, int>> <>9__21_1;
            public static Func<IMyEventOwner, Action<long, int, int>> <>9__24_0;
            public static Func<IMyEventOwner, Action<long, int, int>> <>9__24_1;
            public static Func<IMyEventOwner, Action<long, Vector3I, int>> <>9__27_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, int>> <>9__27_1;
            public static Func<IMyEventOwner, Action<long, long, int>> <>9__30_0;
            public static Func<IMyEventOwner, Action<long, int, long, int>> <>9__31_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, long, int>> <>9__32_0;
            public static Func<IMyEventOwner, Action<string, Vector3D, float, long, int>> <>9__33_0;
            public static Func<IMyEventOwner, Action<long, string, Vector3D, float>> <>9__34_0;
            public static Func<IMyEventOwner, Action<Vector3D, float, int>> <>9__34_1;
            public static Func<IMyEventOwner, Action<Vector3D, float, int>> <>9__34_2;
            public static Func<IMyEventOwner, Action<string, long>> <>9__43_0;

            internal Action<long, string, Vector3D, float> <OnReserveAreaRequest>b__34_0(IMyEventOwner s) => 
                new Action<long, string, Vector3D, float>(MyAiTargetManager.OnReserveAreaAllSuccess);

            internal Action<Vector3D, float, int> <OnReserveAreaRequest>b__34_1(IMyEventOwner s) => 
                new Action<Vector3D, float, int>(MyAiTargetManager.OnReserveAreaSuccess);

            internal Action<Vector3D, float, int> <OnReserveAreaRequest>b__34_2(IMyEventOwner s) => 
                new Action<Vector3D, float, int>(MyAiTargetManager.OnReserveAreaFailure);

            internal Action<long, int> <OnReserveEntityRequest>b__21_0(IMyEventOwner s) => 
                new Action<long, int>(MyAiTargetManager.OnReserveEntitySuccess);

            internal Action<long, int> <OnReserveEntityRequest>b__21_1(IMyEventOwner s) => 
                new Action<long, int>(MyAiTargetManager.OnReserveEntityFailure);

            internal Action<long, int, int> <OnReserveEnvironmentItemRequest>b__24_0(IMyEventOwner s) => 
                new Action<long, int, int>(MyAiTargetManager.OnReserveEnvironmentItemSuccess);

            internal Action<long, int, int> <OnReserveEnvironmentItemRequest>b__24_1(IMyEventOwner s) => 
                new Action<long, int, int>(MyAiTargetManager.OnReserveEnvironmentItemFailure);

            internal Action<long, Vector3I, int> <OnReserveVoxelPositionRequest>b__27_0(IMyEventOwner s) => 
                new Action<long, Vector3I, int>(MyAiTargetManager.OnReserveVoxelPositionSuccess);

            internal Action<long, Vector3I, int> <OnReserveVoxelPositionRequest>b__27_1(IMyEventOwner s) => 
                new Action<long, Vector3I, int>(MyAiTargetManager.OnReserveVoxelPositionFailure);

            internal Action<string, Vector3D, float, long, int> <RequestAreaReservation>b__33_0(IMyEventOwner s) => 
                new Action<string, Vector3D, float, long, int>(MyAiTargetManager.OnReserveAreaRequest);

            internal Action<long, long, int> <RequestEntityReservation>b__30_0(IMyEventOwner s) => 
                new Action<long, long, int>(MyAiTargetManager.OnReserveEntityRequest);

            internal Action<long, int, long, int> <RequestEnvironmentItemReservation>b__31_0(IMyEventOwner s) => 
                new Action<long, int, long, int>(MyAiTargetManager.OnReserveEnvironmentItemRequest);

            internal Action<long, Vector3I, long, int> <RequestVoxelPositionReservation>b__32_0(IMyEventOwner s) => 
                new Action<long, Vector3I, long, int>(MyAiTargetManager.OnReserveVoxelPositionRequest);

            internal Action<string, long> <UpdateAfterSimulation>b__43_0(IMyEventOwner s) => 
                new Action<string, long>(MyAiTargetManager.OnReserveAreaCancel);
        }

        public delegate void AreaReservationHandler(ref MyAiTargetManager.ReservedAreaData entityData, bool success);

        public delegate void ReservationHandler(ref MyAiTargetManager.ReservedEntityData entityData, bool success);

        [StructLayout(LayoutKind.Sequential)]
        public struct ReservedAreaData
        {
            public Vector3D WorldPosition;
            public float Radius;
            public MyTimeSpan ReservationTimer;
            public MyPlayer.PlayerId ReserverId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ReservedEntityData
        {
            public MyReservedEntityType Type;
            public long EntityId;
            public int LocalId;
            public Vector3I GridPos;
            public long ReservationTimer;
            public MyPlayer.PlayerId ReserverId;
        }
    }
}


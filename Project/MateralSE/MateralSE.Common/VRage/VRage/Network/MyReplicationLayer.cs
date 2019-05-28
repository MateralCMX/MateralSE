namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Library;
    using VRage.Library.Algorithms;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Serialization;
    using VRage.Utils;

    public abstract class MyReplicationLayer : MyReplicationLayerBase, IDisposable, INetObjectResolver
    {
        private readonly SequenceIdGenerator m_networkIdGenerator = SequenceIdGenerator.CreateWithStopwatch(TimeSpan.FromSeconds(1800.0), 0x186a0);
        protected readonly bool m_isNetworkAuthority;
        private readonly Dictionary<NetworkId, IMyNetObject> m_networkIDToObject = new Dictionary<NetworkId, IMyNetObject>();
        private readonly Dictionary<IMyNetObject, NetworkId> m_objectToNetworkID = new Dictionary<IMyNetObject, NetworkId>();
        private readonly Dictionary<IMyEventProxy, IMyProxyTarget> m_proxyToTarget = new Dictionary<IMyEventProxy, IMyProxyTarget>();
        private readonly Dictionary<Type, VRage.Boxed<NetworkObjectStat>> m_networkObjectStats = new Dictionary<Type, VRage.Boxed<NetworkObjectStat>>();
        protected MyReplicablesBase m_replicables;
        private const int TIMESTAMP_CORRECTION_MINIMUM = 10;
        private const float SMOOTH_TIMESTAMP_CORRECTION_AMPLITUDE = 1f;
        public float PingSmoothFactor = 3f;
        private readonly FastResourceLock m_networkObjectLock = new FastResourceLock();
        protected long SyncFrameCounter;
        private readonly Thread m_mainThread;
        private readonly Queue<VRage.Network.CallSite> m_lastFiveSites = new Queue<VRage.Network.CallSite>(5);

        protected MyReplicationLayer(bool isNetworkAuthority, EndpointId localEndpoint, Thread mainThread)
        {
            this.m_mainThread = mainThread;
            this.TimestampCorrectionMinimum = 10;
            this.SmoothCorrectionAmplitude = 1f;
            this.m_isNetworkAuthority = isNetworkAuthority;
            base.SetLocalEndpoint(localEndpoint);
        }

        protected virtual unsafe void AddNetworkObject(NetworkId networkID, IMyNetObject obj)
        {
            using (this.m_networkObjectLock.AcquireExclusiveUsing())
            {
                IMyNetObject obj2;
                if (this.m_networkIDToObject.TryGetValue(networkID, out obj2))
                {
                    if ((obj != null) && (obj2 != null))
                    {
                        string[] textArray1 = new string[] { "Replicated object already exists adding : ", obj.ToString(), " existing : ", obj2.ToString(), " id : ", networkID.ToString() };
                        MyLog.Default.WriteLine(string.Concat(textArray1));
                    }
                }
                else
                {
                    VRage.Boxed<NetworkObjectStat> boxed;
                    this.m_networkIDToObject.Add(networkID, obj);
                    this.m_objectToNetworkID.Add(obj, networkID);
                    Type key = obj.GetType();
                    if (!this.m_networkObjectStats.TryGetValue(key, out boxed))
                    {
                        NetworkObjectStat stat = new NetworkObjectStat {
                            Group = GetNetworkObjectGroup(obj)
                        };
                        boxed = new VRage.Boxed<NetworkObjectStat>(stat);
                        this.m_networkObjectStats[key] = boxed;
                    }
                    int* numPtr1 = (int*) ref boxed.BoxedValue.Count;
                    numPtr1[0]++;
                    IMyProxyTarget target = obj as IMyProxyTarget;
                    if (((target != null) && (target.Target != null)) && !this.m_proxyToTarget.ContainsKey(target.Target))
                    {
                        this.m_proxyToTarget.Add(target.Target, target);
                    }
                }
            }
        }

        protected void AddNetworkObjectServer(IMyNetObject obj)
        {
            NetworkId networkID = new NetworkId(this.m_networkIdGenerator.NextId());
            this.AddNetworkObject(networkID, obj);
        }

        public override void AdvanceSyncTime()
        {
            this.SyncFrameCounter += 1L;
        }

        [Conditional("DEBUG")]
        protected void CheckThread()
        {
        }

        public abstract MyPacketStatistics ClearClientStatistics();
        public abstract MyPacketStatistics ClearServerStatistics();
        public abstract void Disconnect();
        protected abstract bool DispatchBlockingEvent(IPacketData stream, VRage.Network.CallSite site, EndpointId recipient, IMyNetObject eventInstance, Vector3D? position, IMyNetObject blockedNetObject);
        protected abstract bool DispatchEvent(IPacketData stream, VRage.Network.CallSite site, EndpointId recipient, IMyNetObject eventInstance, Vector3D? position);
        protected sealed override void DispatchEvent<T1, T2, T3, T4, T5, T6, T7, T8>(VRage.Network.CallSite callSite, EndpointId recipient, Vector3D? position, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5, ref T6 arg6, ref T7 arg7, ref T8 arg8) where T1: IMyEventOwner where T8: IMyEventOwner
        {
            IMyNetObject proxyTarget;
            NetworkId networkIdByObject;
            uint num = callSite.Id;
            if (callSite.MethodInfo.IsStatic)
            {
                proxyTarget = null;
                networkIdByObject = NetworkId.Invalid;
            }
            else
            {
                if (((T1) arg1) == null)
                {
                    throw new InvalidOperationException("First argument (the instance on which is event invoked) cannot be null for non-static events");
                }
                if (!(((T1) arg1) is IMyEventProxy))
                {
                    if (!(((T1) arg1) is IMyNetObject))
                    {
                        throw new InvalidOperationException("Instance events may be called only on IMyNetObject or IMyEventProxy");
                    }
                    proxyTarget = (IMyNetObject) ((T1) arg1);
                    networkIdByObject = this.GetNetworkIdByObject(proxyTarget);
                }
                else
                {
                    proxyTarget = this.GetProxyTarget((IMyEventProxy) ((T1) arg1));
                    if (proxyTarget == null)
                    {
                        string msg = "Raising event on object which is not recognized by replication: " + ((T1) arg1);
                        MyLog.Default.WriteLine(msg);
                        return;
                    }
                    num += (uint) base.m_typeTable.Get(proxyTarget.GetType()).EventTable.Count;
                    networkIdByObject = this.GetNetworkIdByObject(proxyTarget);
                }
            }
            NetworkId invalid = NetworkId.Invalid;
            IMyNetObject proxyTarget = null;
            if ((((T8) arg8) is IMyEventProxy) && callSite.IsBlocking)
            {
                proxyTarget = this.GetProxyTarget((IMyEventProxy) ((T8) arg8));
                invalid = this.GetNetworkIdByObject(proxyTarget);
            }
            else
            {
                if ((((T8) arg8) is IMyEventProxy) && !callSite.IsBlocking)
                {
                    throw new InvalidOperationException("Rising blocking event but event itself does not have Blocking attribute");
                }
                if (!(((T8) arg8) is IMyEventProxy) && callSite.IsBlocking)
                {
                    throw new InvalidOperationException("Event contain Blocking attribute but blocked event proxy is not set or raised event is not blocking one");
                }
            }
            CallSite<T1, T2, T3, T4, T5, T6, T7> site = (CallSite<T1, T2, T3, T4, T5, T6, T7>) callSite;
            MyPacketDataBitStreamBase bitStreamPacketData = this.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteNetworkId(networkIdByObject);
            bitStreamPacketData.Stream.WriteNetworkId(invalid);
            bitStreamPacketData.Stream.WriteUInt16((ushort) num, 0x10);
            bitStreamPacketData.Stream.WriteBool(position != null);
            if (position != null)
            {
                bitStreamPacketData.Stream.Write(position.Value);
            }
            using (MySerializerNetObject.Using(this))
            {
                site.Serializer(arg1, bitStreamPacketData.Stream, ref arg2, ref arg3, ref arg4, ref arg5, ref arg6, ref arg7);
            }
            bitStreamPacketData.Stream.Terminate();
            if (invalid.IsInvalid ? this.DispatchEvent(bitStreamPacketData, callSite, recipient, proxyTarget, position) : this.DispatchBlockingEvent(bitStreamPacketData, callSite, recipient, proxyTarget, position, proxyTarget))
            {
                base.InvokeLocally<T1, T2, T3, T4, T5, T6, T7>(site, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }

        public virtual void Dispose()
        {
            using (this.m_networkObjectLock.AcquireExclusiveUsing())
            {
                this.m_networkObjectStats.Clear();
                this.m_networkIDToObject.Clear();
                this.m_objectToNetworkID.Clear();
                this.m_proxyToTarget.Clear();
            }
        }

        protected abstract MyPacketDataBitStreamBase GetBitStreamPacketData();
        public virtual MyClientStateBase GetClientData(Endpoint endpointId) => 
            null;

        public virtual string GetMultiplayerStat() => 
            ("Multiplayer Statistics:" + MyEnvironment.NewLine);

        public NetworkId GetNetworkIdByObject(IMyNetObject obj)
        {
            if (obj == null)
            {
                return NetworkId.Invalid;
            }
            using (this.m_networkObjectLock.AcquireSharedUsing())
            {
                return this.m_objectToNetworkID.GetValueOrDefault<IMyNetObject, NetworkId>(obj, NetworkId.Invalid);
            }
        }

        private static NetworkObjectGroup GetNetworkObjectGroup(IMyNetObject obj)
        {
            NetworkObjectGroup none = NetworkObjectGroup.None;
            if (obj is IMyReplicable)
            {
                none |= NetworkObjectGroup.Replicable;
            }
            if (obj is IMyStateGroup)
            {
                none |= NetworkObjectGroup.StatGroup;
            }
            if (none == NetworkObjectGroup.None)
            {
                none |= NetworkObjectGroup.Unknown;
            }
            return none;
        }

        protected IMyNetObject GetObjectByNetworkId(NetworkId id)
        {
            using (this.m_networkObjectLock.AcquireSharedUsing())
            {
                return this.m_networkIDToObject.GetValueOrDefault<NetworkId, IMyNetObject>(id);
            }
        }

        public IMyProxyTarget GetProxyTarget(IMyEventProxy proxy)
        {
            using (this.m_networkObjectLock.AcquireSharedUsing())
            {
                return this.m_proxyToTarget.GetValueOrDefault<IMyEventProxy, IMyProxyTarget>(proxy);
            }
        }

        public abstract MyTimeSpan GetSimulationUpdateTime();
        protected Type GetTypeByTypeId(TypeId typeId) => 
            base.m_typeTable.Get(typeId).Type;

        protected TypeId GetTypeIdByType(Type type) => 
            base.m_typeTable.Get(type).TypeId;

        public bool Invoke(VRage.Network.CallSite callSite, BitStream stream, object obj, EndpointId source, MyClientStateBase clientState, bool validate)
        {
            bool flag;
            using (MySerializerNetObject.Using(this))
            {
                using (MyEventContext.Set(source, clientState, false))
                {
                    int num1;
                    int num2;
                    if (!callSite.Invoke(stream, obj, validate))
                    {
                        num1 = 0;
                    }
                    else if (validate)
                    {
                        num1 = (int) !MyEventContext.Current.HasValidationFailed;
                    }
                    else
                    {
                        num1 = 1;
                    }
                    flag = (bool) num2;
                }
            }
            return flag;
        }

        public bool IsTypeReplicated(Type type)
        {
            MySynchronizedTypeInfo info;
            return (base.m_typeTable.TryGet(type, out info) && info.IsReplicated);
        }

        public void OnEvent(MyPacket packet)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.GetBitStreamPacketData();
            bitStreamPacketData.Stream.ResetRead(packet.BitStream, true);
            this.ProcessEvent(bitStreamPacketData, packet.Sender.Id);
            packet.Return();
        }

        protected abstract void OnEvent(MyPacketDataBitStreamBase data, VRage.Network.CallSite site, object obj, IMyNetObject sendAs, Vector3D? position, EndpointId source);
        protected virtual void OnEvent(MyPacketDataBitStreamBase data, NetworkId networkId, NetworkId blockedNetId, uint eventId, EndpointId sender, Vector3D? position)
        {
            VRage.Network.CallSite site;
            IMyNetObject objectByNetworkId;
            object target;
            try
            {
                if (!networkId.IsInvalid)
                {
                    objectByNetworkId = this.GetObjectByNetworkId(networkId);
                    if (objectByNetworkId == null)
                    {
                        return;
                    }
                    else if (objectByNetworkId.IsValid)
                    {
                        MySynchronizedTypeInfo info = base.m_typeTable.Get(objectByNetworkId.GetType());
                        int count = info.EventTable.Count;
                        if (eventId < count)
                        {
                            target = objectByNetworkId;
                            site = info.EventTable.Get(eventId);
                        }
                        else
                        {
                            target = ((IMyProxyTarget) objectByNetworkId).Target;
                            site = base.m_typeTable.Get(target.GetType()).EventTable.Get(eventId - ((uint) count));
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    site = base.m_typeTable.StaticEventTable.Get(eventId);
                    objectByNetworkId = null;
                    target = null;
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Static: " + !networkId.IsInvalid.ToString());
                MyLog.Default.WriteLine("EventId: " + eventId);
                MyLog.Default.WriteLine("Last five sites:");
                foreach (VRage.Network.CallSite site2 in this.m_lastFiveSites)
                {
                    MyLog.Default.WriteLine(site2.ToString());
                }
                throw exception;
            }
            if (this.m_lastFiveSites.Count >= 5)
            {
                this.m_lastFiveSites.Dequeue();
            }
            this.m_lastFiveSites.Enqueue(site);
            this.OnEvent(data, site, target, objectByNetworkId, position, sender);
        }

        private void ProcessEvent(MyPacketDataBitStreamBase data, EndpointId sender)
        {
            NetworkId networkId = data.Stream.ReadNetworkId();
            NetworkId blockedNetId = data.Stream.ReadNetworkId();
            uint eventId = data.Stream.ReadUInt16(0x10);
            Vector3D? position = null;
            if (data.Stream.ReadBool())
            {
                position = new Vector3D?(data.Stream.ReadVector3D());
            }
            this.OnEvent(data, networkId, blockedNetId, eventId, sender, position);
        }

        public void RefreshReplicableHierarchy(IMyReplicable replicable)
        {
            this.m_replicables.Refresh(replicable);
        }

        protected NetworkId RemoveNetworkedObject(IMyNetObject obj)
        {
            using (this.m_networkObjectLock.AcquireExclusiveUsing())
            {
                NetworkId id;
                if (this.m_objectToNetworkID.TryGetValue(obj, out id))
                {
                    this.RemoveNetworkedObjectInternal(id, obj);
                }
                return id;
            }
        }

        protected virtual unsafe void RemoveNetworkedObjectInternal(NetworkId networkID, IMyNetObject obj)
        {
            this.m_objectToNetworkID.Remove(obj);
            this.m_networkIDToObject.Remove(networkID);
            IMyProxyTarget target = obj as IMyProxyTarget;
            if ((target != null) && (target.Target != null))
            {
                this.m_proxyToTarget.Remove(target.Target);
            }
            int* numPtr1 = (int*) ref this.m_networkObjectStats[obj.GetType()].BoxedValue.Count;
            numPtr1[0]--;
            this.m_networkIdGenerator.Return(networkID.Value);
        }

        private void ReportObjects(string name, NetworkObjectGroup group)
        {
            using (this.m_networkObjectLock.AcquireSharedUsing())
            {
                int num = 0;
                MyStatsGraph.Begin(name, 0x7fffffff, "ReportObjects", 0x116, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
                foreach (KeyValuePair<Type, VRage.Boxed<NetworkObjectStat>> pair in this.m_networkObjectStats)
                {
                    VRage.Boxed<NetworkObjectStat> boxed = pair.Value;
                    if ((boxed.BoxedValue.Count > 0) && boxed.BoxedValue.Group.HasFlag(group))
                    {
                        num += boxed.BoxedValue.Count;
                        MyStatsGraph.Begin(pair.Key.Name, 0x7fffffff, "ReportObjects", 0x11d, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
                        MyStatsGraph.End(new float?((float) boxed.BoxedValue.Count), 0f, "", "{0:.} x", "", "ReportObjects", 0x11e, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
                    }
                }
                MyStatsGraph.End(new float?((float) num), 0f, "", "{0:.} x", "", "ReportObjects", 0x121, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
            }
        }

        public void ReportReplicatedObjects()
        {
            MyStatsGraph.ProfileAdvanced(true);
            MyStatsGraph.Begin("ReportObjects", 0x7fffffff, "ReportReplicatedObjects", 0xfd, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
            this.ReportObjects("Replicable objects", NetworkObjectGroup.Replicable);
            this.ReportObjects("State groups", NetworkObjectGroup.StatGroup);
            this.ReportObjects("Unknown net objects", NetworkObjectGroup.Unknown);
            float? bytesTransfered = null;
            MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "ReportReplicatedObjects", 0x101, @"E:\Repo1\Sources\VRage\Replication\MyReplicationLayer.cs");
            MyStatsGraph.ProfileAdvanced(false);
        }

        public abstract void SendUpdate();
        internal void SerializeTypeTable(BitStream stream)
        {
            base.m_typeTable.Serialize(stream);
        }

        public virtual void SetPriorityMultiplier(EndpointId id, float priority)
        {
        }

        public abstract void Simulate();
        public bool TryGetNetworkIdByObject(IMyNetObject obj, out NetworkId networkId)
        {
            if (obj == null)
            {
                networkId = NetworkId.Invalid;
                return false;
            }
            using (this.m_networkObjectLock.AcquireSharedUsing())
            {
                return this.m_objectToNetworkID.TryGetValue(obj, out networkId);
            }
        }

        public abstract void UpdateAfter();
        public abstract void UpdateBefore();
        public abstract void UpdateClientStateGroups();
        public abstract void UpdateStatisticsData(int outgoing, int incoming, int tamperred, float gcMemory, float processMemory);
        void INetObjectResolver.Resolve<T>(BitStream stream, ref T obj) where T: class, IMyNetObject
        {
            if (stream.Reading)
            {
                obj = (T) this.GetObjectByNetworkId(stream.ReadNetworkId());
            }
            else
            {
                NetworkId id;
                stream.WriteNetworkId(this.TryGetNetworkIdByObject((T) obj, out id) ? id : NetworkId.Invalid);
            }
        }

        public bool UseSmoothPing { get; set; }

        public bool UseSmoothCorrection { get; set; }

        public float SmoothCorrectionAmplitude { get; set; }

        public int TimestampCorrectionMinimum { get; set; }

        [Flags]
        private enum NetworkObjectGroup
        {
            None = 0,
            Replicable = 1,
            StatGroup = 2,
            Unknown = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NetworkObjectStat
        {
            public int Count;
            public MyReplicationLayer.NetworkObjectGroup Group;
        }
    }
}


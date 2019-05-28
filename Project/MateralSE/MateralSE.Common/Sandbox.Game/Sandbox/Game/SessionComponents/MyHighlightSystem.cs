namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Network;
    using VRageRender;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyHighlightSystem : MySessionComponentBase
    {
        private static MyHighlightSystem m_static;
        private static int m_exclusiveKeyCounter = 10;
        private readonly Dictionary<int, long> m_exclusiveKeysToIds = new Dictionary<int, long>();
        private readonly HashSet<long> m_highlightedIds = new HashSet<long>();
        private readonly MyHudSelectedObject m_highlightCalculationHelper = new MyHudSelectedObject();
        private readonly List<uint> m_subPartIndicies = new List<uint>();
        private readonly HashSet<uint> m_highlightOverlappingIds = new HashSet<uint>();
        [CompilerGenerated]
        private Action<MyHighlightData> HighlightRejected;
        [CompilerGenerated]
        private Action<MyHighlightData> HighlightAccepted;
        [CompilerGenerated]
        private Action<MyHighlightData, int> ExclusiveHighlightRejected;
        [CompilerGenerated]
        private Action<MyHighlightData, int> ExclusiveHighlightAccepted;
        private StringBuilder m_highlightAttributeBuilder = new StringBuilder();

        public event Action<MyHighlightData, int> ExclusiveHighlightAccepted
        {
            [CompilerGenerated] add
            {
                Action<MyHighlightData, int> exclusiveHighlightAccepted = this.ExclusiveHighlightAccepted;
                while (true)
                {
                    Action<MyHighlightData, int> a = exclusiveHighlightAccepted;
                    Action<MyHighlightData, int> action3 = (Action<MyHighlightData, int>) Delegate.Combine(a, value);
                    exclusiveHighlightAccepted = Interlocked.CompareExchange<Action<MyHighlightData, int>>(ref this.ExclusiveHighlightAccepted, action3, a);
                    if (ReferenceEquals(exclusiveHighlightAccepted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyHighlightData, int> exclusiveHighlightAccepted = this.ExclusiveHighlightAccepted;
                while (true)
                {
                    Action<MyHighlightData, int> source = exclusiveHighlightAccepted;
                    Action<MyHighlightData, int> action3 = (Action<MyHighlightData, int>) Delegate.Remove(source, value);
                    exclusiveHighlightAccepted = Interlocked.CompareExchange<Action<MyHighlightData, int>>(ref this.ExclusiveHighlightAccepted, action3, source);
                    if (ReferenceEquals(exclusiveHighlightAccepted, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyHighlightData, int> ExclusiveHighlightRejected
        {
            [CompilerGenerated] add
            {
                Action<MyHighlightData, int> exclusiveHighlightRejected = this.ExclusiveHighlightRejected;
                while (true)
                {
                    Action<MyHighlightData, int> a = exclusiveHighlightRejected;
                    Action<MyHighlightData, int> action3 = (Action<MyHighlightData, int>) Delegate.Combine(a, value);
                    exclusiveHighlightRejected = Interlocked.CompareExchange<Action<MyHighlightData, int>>(ref this.ExclusiveHighlightRejected, action3, a);
                    if (ReferenceEquals(exclusiveHighlightRejected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyHighlightData, int> exclusiveHighlightRejected = this.ExclusiveHighlightRejected;
                while (true)
                {
                    Action<MyHighlightData, int> source = exclusiveHighlightRejected;
                    Action<MyHighlightData, int> action3 = (Action<MyHighlightData, int>) Delegate.Remove(source, value);
                    exclusiveHighlightRejected = Interlocked.CompareExchange<Action<MyHighlightData, int>>(ref this.ExclusiveHighlightRejected, action3, source);
                    if (ReferenceEquals(exclusiveHighlightRejected, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyHighlightData> HighlightAccepted
        {
            [CompilerGenerated] add
            {
                Action<MyHighlightData> highlightAccepted = this.HighlightAccepted;
                while (true)
                {
                    Action<MyHighlightData> a = highlightAccepted;
                    Action<MyHighlightData> action3 = (Action<MyHighlightData>) Delegate.Combine(a, value);
                    highlightAccepted = Interlocked.CompareExchange<Action<MyHighlightData>>(ref this.HighlightAccepted, action3, a);
                    if (ReferenceEquals(highlightAccepted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyHighlightData> highlightAccepted = this.HighlightAccepted;
                while (true)
                {
                    Action<MyHighlightData> source = highlightAccepted;
                    Action<MyHighlightData> action3 = (Action<MyHighlightData>) Delegate.Remove(source, value);
                    highlightAccepted = Interlocked.CompareExchange<Action<MyHighlightData>>(ref this.HighlightAccepted, action3, source);
                    if (ReferenceEquals(highlightAccepted, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyHighlightData> HighlightRejected
        {
            [CompilerGenerated] add
            {
                Action<MyHighlightData> highlightRejected = this.HighlightRejected;
                while (true)
                {
                    Action<MyHighlightData> a = highlightRejected;
                    Action<MyHighlightData> action3 = (Action<MyHighlightData>) Delegate.Combine(a, value);
                    highlightRejected = Interlocked.CompareExchange<Action<MyHighlightData>>(ref this.HighlightRejected, action3, a);
                    if (ReferenceEquals(highlightRejected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyHighlightData> highlightRejected = this.HighlightRejected;
                while (true)
                {
                    Action<MyHighlightData> source = highlightRejected;
                    Action<MyHighlightData> action3 = (Action<MyHighlightData>) Delegate.Remove(source, value);
                    highlightRejected = Interlocked.CompareExchange<Action<MyHighlightData>>(ref this.HighlightRejected, action3, source);
                    if (ReferenceEquals(highlightRejected, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyHighlightSystem()
        {
            m_static = this;
        }

        public void AddHighlightOverlappingModel(uint modelRenderId)
        {
            if ((modelRenderId != uint.MaxValue) && !this.m_highlightOverlappingIds.Contains(modelRenderId))
            {
                this.m_highlightOverlappingIds.Add(modelRenderId);
                MyRenderProxy.UpdateHighlightOverlappingModel(modelRenderId, true);
            }
        }

        private void CollectSubPartIndicies(MyEntity currentEntity)
        {
            if ((currentEntity.Subparts != null) && (currentEntity.Render != null))
            {
                foreach (MyEntitySubpart subpart in currentEntity.Subparts.Values)
                {
                    this.CollectSubPartIndicies(subpart);
                    this.m_subPartIndicies.AddRange(subpart.Render.RenderObjectIDs);
                }
            }
        }

        private void HighlightUseObject(IMyUseObject useObject, MyHighlightData data)
        {
            this.m_highlightCalculationHelper.HighlightAttribute = null;
            if (useObject.Dummy != null)
            {
                object obj2;
                useObject.Dummy.CustomData.TryGetValue("highlight", out obj2);
                string str = obj2 as string;
                if (str == null)
                {
                    return;
                }
                if (data.SubPartNames == null)
                {
                    this.m_highlightCalculationHelper.HighlightAttribute = str;
                }
                else
                {
                    this.m_highlightAttributeBuilder.Clear();
                    char[] separator = new char[] { ';' };
                    string[] strArray = data.SubPartNames.Split(separator);
                    int index = 0;
                    while (true)
                    {
                        if (index >= strArray.Length)
                        {
                            if (this.m_highlightAttributeBuilder.Length > 0)
                            {
                                this.m_highlightAttributeBuilder.TrimEnd(1);
                            }
                            this.m_highlightCalculationHelper.HighlightAttribute = this.m_highlightAttributeBuilder.ToString();
                            break;
                        }
                        string str2 = strArray[index];
                        if (str.Contains(str2))
                        {
                            this.m_highlightAttributeBuilder.Append(str2).Append(';');
                        }
                        index++;
                    }
                }
                if (string.IsNullOrEmpty(this.m_highlightCalculationHelper.HighlightAttribute))
                {
                    return;
                }
            }
            this.m_highlightCalculationHelper.Highlight(useObject);
            MyRenderProxy.UpdateModelHighlight(this.m_highlightCalculationHelper.InteractiveObject.RenderObjectID, this.m_highlightCalculationHelper.SectionNames, this.m_highlightCalculationHelper.SubpartIndices, data.OutlineColor, (float) data.Thickness, (float) data.PulseTimeInFrames, this.m_highlightCalculationHelper.InteractiveObject.InstanceID);
        }

        public bool IsHighlighted(long entityId) => 
            this.m_highlightedIds.Contains(entityId);

        public bool IsReserved(long entityId) => 
            this.m_exclusiveKeysToIds.ContainsValue(entityId);

        private void MakeLocalHighlightChange(MyHighlightData data)
        {
            MyEntity entity;
            if (data.Thickness > -1)
            {
                this.m_highlightedIds.Add(data.EntityId);
            }
            else
            {
                this.m_highlightedIds.Remove(data.EntityId);
            }
            if (MyEntities.TryGetEntityById(data.EntityId, out entity, false))
            {
                if (!data.IgnoreUseObjectData)
                {
                    IMyUseObject useObject = entity as IMyUseObject;
                    MyUseObjectsComponentBase base2 = entity.Components.Get<MyUseObjectsComponentBase>();
                    if ((useObject != null) || (base2 != null))
                    {
                        if (base2 == null)
                        {
                            this.HighlightUseObject(useObject, data);
                            if (this.HighlightAccepted != null)
                            {
                                this.HighlightAccepted(data);
                            }
                            return;
                        }
                        List<IMyUseObject> objects = new List<IMyUseObject>();
                        base2.GetInteractiveObjects<IMyUseObject>(objects);
                        int num = 0;
                        while (true)
                        {
                            if (num >= objects.Count)
                            {
                                if (objects.Count <= 0)
                                {
                                    break;
                                }
                                if (this.HighlightAccepted != null)
                                {
                                    this.HighlightAccepted(data);
                                }
                                return;
                            }
                            this.HighlightUseObject(objects[num], data);
                            num++;
                        }
                    }
                }
                this.m_subPartIndicies.Clear();
                this.CollectSubPartIndicies(entity);
                MyRenderProxy.UpdateModelHighlight(entity.Render.GetRenderObjectID(), null, this.m_subPartIndicies.ToArray(), data.OutlineColor, (float) data.Thickness, (float) data.PulseTimeInFrames, -1);
                if (this.HighlightAccepted != null)
                {
                    this.HighlightAccepted(data);
                }
            }
        }

        private void NotifyExclusiveHighlightAccepted(MyHighlightData data, int exclusiveKey)
        {
            if (this.ExclusiveHighlightAccepted != null)
            {
                this.ExclusiveHighlightAccepted(data, exclusiveKey);
                foreach (Delegate delegate2 in this.ExclusiveHighlightAccepted.GetInvocationList())
                {
                    this.ExclusiveHighlightAccepted -= ((Action<MyHighlightData, int>) delegate2);
                }
            }
        }

        private void NotifyExclusiveHighlightRejected(MyHighlightData data, int exclusiveKey)
        {
            if (this.ExclusiveHighlightRejected != null)
            {
                this.ExclusiveHighlightRejected(data, exclusiveKey);
                foreach (Delegate delegate2 in this.ExclusiveHighlightRejected.GetInvocationList())
                {
                    this.ExclusiveHighlightRejected -= ((Action<MyHighlightData, int>) delegate2);
                }
            }
        }

        private void NotifyHighlightAccepted(MyHighlightData data)
        {
            if (this.HighlightAccepted != null)
            {
                this.HighlightAccepted(data);
                foreach (Delegate delegate2 in this.HighlightAccepted.GetInvocationList())
                {
                    this.HighlightAccepted -= ((Action<MyHighlightData>) delegate2);
                }
            }
        }

        [Event(null, 0x1b9), Reliable, Client]
        private static void OnHighlightOnClient(HighlightMsg msg)
        {
            long num;
            Vector3D? nullable;
            if (m_static.m_exclusiveKeysToIds.ContainsValue(msg.Data.EntityId) && (!m_static.m_exclusiveKeysToIds.TryGetValue(msg.ExclusiveKey, out num) || (num != msg.Data.EntityId)))
            {
                if (m_static.HighlightRejected != null)
                {
                    m_static.HighlightRejected(msg.Data);
                }
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<HighlightMsg>(s => new Action<HighlightMsg>(MyHighlightSystem.OnRequestRejected), msg, MyEventContext.Current.Sender, nullable);
            }
            else
            {
                m_static.MakeLocalHighlightChange(msg.Data);
                if (msg.IsExclusive)
                {
                    bool flag = msg.Data.Thickness > -1;
                    if (msg.ExclusiveKey == -1)
                    {
                        m_exclusiveKeyCounter++;
                        msg.ExclusiveKey = m_exclusiveKeyCounter;
                        if (flag && !m_static.m_exclusiveKeysToIds.ContainsKey(msg.ExclusiveKey))
                        {
                            m_static.m_exclusiveKeysToIds.Add(msg.ExclusiveKey, msg.Data.EntityId);
                        }
                    }
                    if (!flag)
                    {
                        m_static.m_exclusiveKeysToIds.Remove(msg.ExclusiveKey);
                    }
                }
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<HighlightMsg>(s => new Action<HighlightMsg>(MyHighlightSystem.OnRequestAccepted), msg, MyEventContext.Current.Sender, nullable);
            }
        }

        [Event(null, 0x1f6), Reliable, Server]
        private static void OnRequestAccepted(HighlightMsg msg)
        {
            if (msg.IsExclusive)
            {
                m_static.NotifyExclusiveHighlightAccepted(msg.Data, msg.ExclusiveKey);
            }
            else
            {
                m_static.NotifyHighlightAccepted(msg.Data);
            }
        }

        [Event(null, 0x1e7), Reliable, Server]
        private static void OnRequestRejected(HighlightMsg msg)
        {
            if (msg.IsExclusive)
            {
                m_static.NotifyExclusiveHighlightRejected(msg.Data, msg.ExclusiveKey);
            }
            else if (m_static.HighlightRejected != null)
            {
                m_static.HighlightRejected(msg.Data);
            }
        }

        private void ProcessRequest(MyHighlightData data, int exclusiveKey, bool isExclusive)
        {
            if (data.PlayerId == -1L)
            {
                data.PlayerId = MySession.Static.LocalPlayerId;
            }
            if (((MyMultiplayer.Static == null) || MyMultiplayer.Static.IsServer) && (data.PlayerId != MySession.Static.LocalPlayerId))
            {
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(data.PlayerId, out id))
                {
                    HighlightMsg msg = new HighlightMsg {
                        Data = data,
                        ExclusiveKey = exclusiveKey,
                        IsExclusive = isExclusive
                    };
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<HighlightMsg>(s => new Action<HighlightMsg>(MyHighlightSystem.OnHighlightOnClient), msg, new EndpointId(id.SteamId), position);
                }
            }
            else
            {
                long num;
                bool flag = data.Thickness > -1;
                if (this.m_exclusiveKeysToIds.ContainsValue(data.EntityId) && (!this.m_exclusiveKeysToIds.TryGetValue(exclusiveKey, out num) || (num != data.EntityId)))
                {
                    if (this.HighlightRejected != null)
                    {
                        this.HighlightRejected(data);
                    }
                }
                else if (!isExclusive)
                {
                    this.MakeLocalHighlightChange(data);
                    if (this.HighlightAccepted != null)
                    {
                        this.HighlightAccepted(data);
                    }
                }
                else
                {
                    if (exclusiveKey == -1)
                    {
                        m_exclusiveKeyCounter++;
                        exclusiveKey = m_exclusiveKeyCounter;
                    }
                    if (!flag)
                    {
                        this.m_exclusiveKeysToIds.Remove(exclusiveKey);
                    }
                    else if (!this.m_exclusiveKeysToIds.ContainsKey(exclusiveKey))
                    {
                        this.m_exclusiveKeysToIds.Add(exclusiveKey, data.EntityId);
                    }
                    this.MakeLocalHighlightChange(data);
                    if (this.ExclusiveHighlightAccepted != null)
                    {
                        this.ExclusiveHighlightAccepted(data, exclusiveKey);
                    }
                }
            }
        }

        public void RemoveHighlightOverlappingModel(uint modelRenderId)
        {
            if ((modelRenderId != uint.MaxValue) && this.m_highlightOverlappingIds.Contains(modelRenderId))
            {
                this.m_highlightOverlappingIds.Remove(modelRenderId);
                MyRenderProxy.UpdateHighlightOverlappingModel(modelRenderId, false);
            }
        }

        public void RequestHighlightChange(MyHighlightData data)
        {
            this.ProcessRequest(data, -1, false);
        }

        public void RequestHighlightChangeExclusive(MyHighlightData data, int exclusiveKey = -1)
        {
            this.ProcessRequest(data, exclusiveKey, true);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            m_static = null;
        }

        public HashSetReader<uint> HighlightOverlappingRenderIds =>
            new HashSetReader<uint>(this.m_highlightOverlappingIds);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyHighlightSystem.<>c <>9 = new MyHighlightSystem.<>c();
            public static Func<IMyEventOwner, Action<MyHighlightSystem.HighlightMsg>> <>9__31_0;
            public static Func<IMyEventOwner, Action<MyHighlightSystem.HighlightMsg>> <>9__36_1;
            public static Func<IMyEventOwner, Action<MyHighlightSystem.HighlightMsg>> <>9__36_0;

            internal Action<MyHighlightSystem.HighlightMsg> <OnHighlightOnClient>b__36_0(IMyEventOwner s) => 
                new Action<MyHighlightSystem.HighlightMsg>(MyHighlightSystem.OnRequestAccepted);

            internal Action<MyHighlightSystem.HighlightMsg> <OnHighlightOnClient>b__36_1(IMyEventOwner s) => 
                new Action<MyHighlightSystem.HighlightMsg>(MyHighlightSystem.OnRequestRejected);

            internal Action<MyHighlightSystem.HighlightMsg> <ProcessRequest>b__31_0(IMyEventOwner s) => 
                new Action<MyHighlightSystem.HighlightMsg>(MyHighlightSystem.OnHighlightOnClient);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HighlightMsg
        {
            public MyHighlightSystem.MyHighlightData Data;
            public int ExclusiveKey;
            public bool IsExclusive;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyHighlightData
        {
            public long EntityId;
            public Color? OutlineColor;
            public int Thickness;
            public ulong PulseTimeInFrames;
            public long PlayerId;
            public bool IgnoreUseObjectData;
            public string SubPartNames;
            public MyHighlightData(long entityId = 0L, int thickness = -1, ulong pulseTimeInFrames = 0UL, Color? outlineColor = new Color?(), bool ignoreUseObjectData = false, long playerId = -1L, string subPartNames = null)
            {
                this.EntityId = entityId;
                this.Thickness = thickness;
                this.OutlineColor = outlineColor;
                this.PulseTimeInFrames = pulseTimeInFrames;
                this.PlayerId = playerId;
                this.IgnoreUseObjectData = ignoreUseObjectData;
                this.SubPartNames = subPartNames;
            }
        }
    }
}


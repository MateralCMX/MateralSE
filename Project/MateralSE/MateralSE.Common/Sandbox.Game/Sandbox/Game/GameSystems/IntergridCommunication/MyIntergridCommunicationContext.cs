namespace Sandbox.Game.GameSystems.IntergridCommunication
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyIntergridCommunicationContext : IMyIntergridCommunicationSystem
    {
        private HashSet<MyMessageListener> m_pendingCallbacks;
        private HashSet<BroadcastListener> m_broadcastListeners;
        private LRUCache<long, ConnectionData> m_connectionCache = new LRUCache<long, ConnectionData>(10, null);
        private static HashSet<MyDataBroadcaster> m_broadcasters;

        public MyIntergridCommunicationContext(MyProgrammableBlock programmableBlock)
        {
            this.ProgrammableBlock = programmableBlock;
            this.ProgrammableBlock.OnClosing += new Action<MyEntity>(this.ProgrammableBlock_OnClosing);
            this.UnicastListener = new Sandbox.Game.GameSystems.IntergridCommunication.UnicastListener(this);
        }

        public void DisposeBroadcastListener(BroadcastListener broadcastListener, bool keepIfHavingPendingMessages)
        {
            if (broadcastListener.IsActive)
            {
                broadcastListener.IsActive = false;
                broadcastListener.DisableMessageCallback();
                Context.UnregisterBroadcastListener(broadcastListener);
            }
            if (!keepIfHavingPendingMessages || !broadcastListener.HasPendingMessage)
            {
                this.m_broadcastListeners.Remove(broadcastListener);
            }
        }

        public void DisposeContext()
        {
            this.UnicastListener.DisableMessageCallback();
            if (this.m_broadcastListeners != null)
            {
                while (this.m_broadcastListeners.Count > 0)
                {
                    this.DisposeBroadcastListener(this.m_broadcastListeners.FirstElement<BroadcastListener>(), false);
                }
            }
            if ((this.m_pendingCallbacks != null) && (this.m_pendingCallbacks.Count != 0))
            {
                while (this.m_pendingCallbacks.Count > 0)
                {
                    this.UnregisterFromCallback(this.m_pendingCallbacks.FirstElement<MyMessageListener>());
                }
            }
            this.ProgrammableBlock.OnClosing -= new Action<MyEntity>(this.ProgrammableBlock_OnClosing);
            this.ProgrammableBlock = null;
        }

        private TransmissionDistance? EvaluateConnectionTo(MyIntergridCommunicationContext targetContext, ConnectionData connectionData)
        {
            MyProgrammableBlock programmableBlock = this.ProgrammableBlock;
            MyProgrammableBlock block2 = targetContext.ProgrammableBlock;
            if (!programmableBlock.GetUserRelationToOwner(block2.OwnerId).IsFriendly())
            {
                connectionData.ReleaseBroadcaster();
                return null;
            }
            MyCubeGrid cubeGrid = programmableBlock.CubeGrid;
            MyCubeGrid objB = block2.CubeGrid;
            if (ReferenceEquals(cubeGrid, objB) || MyCubeGridGroups.Static.Mechanical.HasSameGroup(cubeGrid, objB))
            {
                return 0;
            }
            if (MyCubeGridGroups.Static.Logical.HasSameGroup(cubeGrid, objB))
            {
                return 1;
            }
            MyDataBroadcaster target = null;
            if (((connectionData.Broadcaster != null) && (connectionData.Broadcaster.TryGetTarget(out target) && !target.Closed)) && Context.ConnectionProvider(block2, target, programmableBlock.OwnerId))
            {
                return 2;
            }
            using (MyUtils.ReuseCollection<MyDataBroadcaster>(ref m_broadcasters))
            {
                Context.BroadcasterProvider(programmableBlock.CubeGrid, m_broadcasters, programmableBlock.OwnerId);
                using (HashSet<MyDataBroadcaster>.Enumerator enumerator = m_broadcasters.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyDataBroadcaster current = enumerator.Current;
                        if (Context.ConnectionProvider(block2, current, programmableBlock.OwnerId))
                        {
                            if (connectionData.Broadcaster == null)
                            {
                                connectionData.Broadcaster = new WeakReference<MyDataBroadcaster>(current);
                            }
                            else
                            {
                                connectionData.Broadcaster.SetTarget(current);
                            }
                            return 2;
                        }
                    }
                }
            }
            connectionData.ReleaseBroadcaster();
            return null;
        }

        public long GetAddressOfThisContext() => 
            this.ProgrammableBlock.EntityId;

        public void InvokeSinglePendingCallback()
        {
            this.m_pendingCallbacks.FirstElement<MyMessageListener>().InvokeCallback();
        }

        public bool IsConnectedTo(MyIntergridCommunicationContext targetContext, TransmissionDistance transmissionDistance)
        {
            long addressOfThisContext = targetContext.GetAddressOfThisContext();
            ConnectionData data = this.m_connectionCache.Read(addressOfThisContext);
            if (data == null)
            {
                data = new ConnectionData();
                this.m_connectionCache.Write(addressOfThisContext, data);
            }
            int gameplayFrameCounter = MySession.Static.GameplayFrameCounter;
            if (gameplayFrameCounter >= data.ValidTill)
            {
                TransmissionDistance? nullable2 = this.EvaluateConnectionTo(targetContext, data);
                data.ConnectionType = (nullable2 != null) ? nullable2.GetValueOrDefault() : ~TransmissionDistance.CurrentConstruct;
                data.ValidTill = gameplayFrameCounter + ConnectionData.ValidDurationFrames;
            }
            return ((data.ConnectionType != ~TransmissionDistance.CurrentConstruct) && (data.ConnectionType <= transmissionDistance));
        }

        private void ProgrammableBlock_OnClosing(MyEntity block)
        {
            MyIGCSystemSessionComponent.Static.EvictContextFor((MyProgrammableBlock) block);
        }

        public void RegisterForCallback(MyMessageListener messageListener)
        {
            if (this.m_pendingCallbacks == null)
            {
                this.m_pendingCallbacks = new HashSet<MyMessageListener>();
            }
            if (this.m_pendingCallbacks.Count == 0)
            {
                Context.RegisterContextWithPendingCallbacks(this);
            }
            this.m_pendingCallbacks.Add(messageListener);
        }

        void IMyIntergridCommunicationSystem.DisableBroadcastListener(IMyBroadcastListener broadcastListener)
        {
            BroadcastListener item = broadcastListener as BroadcastListener;
            if ((item == null) || !ReferenceEquals(item.Context, this))
            {
                throw new ArgumentException("broadcastListener");
            }
            if (this.m_broadcastListeners.Contains(item))
            {
                this.DisposeBroadcastListener(item, true);
            }
        }

        void IMyIntergridCommunicationSystem.GetBroadcastListeners(List<IMyBroadcastListener> broadcastListeners, Func<IMyBroadcastListener, bool> collect)
        {
            if (this.m_broadcastListeners != null)
            {
                foreach (BroadcastListener listener in this.m_broadcastListeners)
                {
                    if ((collect == null) || collect(listener))
                    {
                        broadcastListeners.Add(listener);
                    }
                }
            }
        }

        bool IMyIntergridCommunicationSystem.IsEndpointReachable(long address, TransmissionDistance transmissionDistance)
        {
            MyIntergridCommunicationContext contextForPB = Context.GetContextForPB(address);
            if (contextForPB == null)
            {
                return false;
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_IGC)
            {
                Vector3D from = this.ProgrammableBlock.WorldMatrix.Translation;
                Vector3D to = MyEntities.GetEntityById(address, false).WorldMatrix.Translation;
                MyIGCSystemSessionComponent.Static.AddDebugDraw(delegate {
                    Color? colorTo = null;
                    MyRenderProxy.DebugDrawArrow3D(from, to, Color.Red, colorTo, false, 0.1, null, 0.5f, false);
                });
            }
            return this.IsConnectedTo(contextForPB, transmissionDistance);
        }

        IMyBroadcastListener IMyIntergridCommunicationSystem.RegisterBroadcastListener(string tag)
        {
            if (this.m_broadcastListeners == null)
            {
                this.m_broadcastListeners = new HashSet<BroadcastListener>();
            }
            BroadcastListener item = null;
            foreach (BroadcastListener listener2 in this.m_broadcastListeners)
            {
                if (listener2.Tag == tag)
                {
                    item = listener2;
                    break;
                }
            }
            if (item == null)
            {
                item = new BroadcastListener(this, tag);
                this.m_broadcastListeners.Add(item);
            }
            if (!item.IsActive)
            {
                item.IsActive = true;
                Context.RegisterBroadcastListener(item);
            }
            return item;
        }

        void IMyIntergridCommunicationSystem.SendBroadcastMessage<TData>(string tag, TData data, TransmissionDistance transmissionDistance)
        {
            MyIGCSystemSessionComponent.Message message = MyIGCSystemSessionComponent.Message.FromBroadcast(MyIGCSystemSessionComponent.BoxMessage<TData>(data), tag, transmissionDistance, this);
            Context.EnqueueMessage(message);
        }

        bool IMyIntergridCommunicationSystem.SendUnicastMessage<TData>(long addressee, string tag, TData data)
        {
            MyIntergridCommunicationContext contextForPB = Context.GetContextForPB(addressee);
            if ((contextForPB == null) || ReferenceEquals(contextForPB, this))
            {
                return false;
            }
            if (!this.IsConnectedTo(contextForPB, TransmissionDistance.AntennaRelay))
            {
                return false;
            }
            object obj2 = MyIGCSystemSessionComponent.BoxMessage<TData>(data);
            Context.EnqueueMessage(MyIGCSystemSessionComponent.Message.FromUnicast(obj2, tag, this, contextForPB));
            return true;
        }

        public void UnregisterFromCallback(MyMessageListener messageListener)
        {
            this.m_pendingCallbacks.Remove(messageListener);
            if (this.m_pendingCallbacks.Count == 0)
            {
                Context.UnregisterContextWithPendingCallbacks(this);
            }
        }

        private static MyIGCSystemSessionComponent Context =>
            MyIGCSystemSessionComponent.Static;

        public bool IsActive =>
            (this.ProgrammableBlock != null);

        public Sandbox.Game.GameSystems.IntergridCommunication.UnicastListener UnicastListener { get; private set; }

        public MyProgrammableBlock ProgrammableBlock { get; private set; }

        long IMyIntergridCommunicationSystem.Me =>
            this.GetAddressOfThisContext();

        IMyUnicastListener IMyIntergridCommunicationSystem.UnicastListener =>
            this.UnicastListener;

        private class ConnectionData
        {
            public static int ValidDurationFrames = 1;
            public int ValidTill;
            public TransmissionDistance ConnectionType;
            public WeakReference<MyDataBroadcaster> Broadcaster;

            public void ReleaseBroadcaster()
            {
                if (this.Broadcaster != null)
                {
                    this.Broadcaster.SetTarget(null);
                }
            }
        }
    }
}


namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Network;
    using VRage.Utils;

    public class MyEventsBuffer : IDisposable
    {
        private readonly Stack<MyBufferedEvent> m_eventPool;
        private readonly Stack<Queue<MyBufferedEvent>> m_listPool;
        private readonly Dictionary<NetworkId, MyObjectEventsBuffer> m_buffer = new Dictionary<NetworkId, MyObjectEventsBuffer>(0x10);
        private readonly Thread m_mainThread;

        public MyEventsBuffer(Thread mainThread, int eventCapacity = 0x20)
        {
            this.m_mainThread = mainThread;
            this.m_listPool = new Stack<Queue<MyBufferedEvent>>(0x10);
            for (int i = 0; i < 0x10; i++)
            {
                this.m_listPool.Push(new Queue<MyBufferedEvent>(0x10));
            }
            this.m_eventPool = new Stack<MyBufferedEvent>(eventCapacity);
            for (int j = 0; j < eventCapacity; j++)
            {
                this.m_eventPool.Push(new MyBufferedEvent());
            }
        }

        [Conditional("DEBUG")]
        private void CheckThread()
        {
        }

        public bool ContainsEvents(NetworkId netId)
        {
            MyObjectEventsBuffer buffer;
            return (this.m_buffer.TryGetValue(netId, out buffer) && (buffer.Events.Count > 0));
        }

        public void Dispose()
        {
            this.m_eventPool.Clear();
            foreach (KeyValuePair<NetworkId, MyObjectEventsBuffer> pair in this.m_buffer)
            {
                foreach (MyBufferedEvent event2 in pair.Value.Events)
                {
                    if (event2.Data != null)
                    {
                        event2.Data.Return();
                    }
                }
            }
            this.m_buffer.Clear();
        }

        public void EnqueueBarrier(NetworkId targetObjectId, NetworkId blockingObjectId)
        {
            MyObjectEventsBuffer buffer;
            MyBufferedEvent item = this.ObtainEvent();
            item.TargetObjectId = targetObjectId;
            item.BlockingObjectId = blockingObjectId;
            item.IsBarrier = true;
            if (!this.m_buffer.TryGetValue(targetObjectId, out buffer))
            {
                buffer = new MyObjectEventsBuffer {
                    Events = this.ObtainList()
                };
                this.m_buffer.Add(targetObjectId, buffer);
            }
            buffer.IsProcessing = false;
            buffer.Events.Enqueue(item);
        }

        public void EnqueueEvent(MyPacketDataBitStreamBase data, NetworkId targetObjectId, NetworkId blockingObjectId, uint eventId, EndpointId sender, Vector3D? position)
        {
            MyObjectEventsBuffer buffer;
            MyBufferedEvent item = this.ObtainEvent();
            item.Data = data;
            item.TargetObjectId = targetObjectId;
            item.BlockingObjectId = blockingObjectId;
            item.EventId = eventId;
            item.Sender = sender;
            item.IsBarrier = false;
            item.Position = position;
            if (!this.m_buffer.TryGetValue(targetObjectId, out buffer))
            {
                buffer = new MyObjectEventsBuffer {
                    Events = this.ObtainList()
                };
                this.m_buffer.Add(targetObjectId, buffer);
            }
            buffer.IsProcessing = false;
            buffer.Events.Enqueue(item);
        }

        public string GetEventsBufferStat()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Pending Events Buffer:");
            foreach (KeyValuePair<NetworkId, MyObjectEventsBuffer> pair in this.m_buffer)
            {
                object[] objArray1 = new object[] { "    NetworkId: ", pair.Key, ", EventsCount: ", pair.Value.Events.Count };
                string str = string.Concat(objArray1);
                builder.AppendLine(str);
            }
            return builder.ToString();
        }

        private MyBufferedEvent ObtainEvent() => 
            ((this.m_eventPool.Count <= 0) ? new MyBufferedEvent() : this.m_eventPool.Pop());

        private Queue<MyBufferedEvent> ObtainList() => 
            ((this.m_listPool.Count <= 0) ? new Queue<MyBufferedEvent>(0x10) : this.m_listPool.Pop());

        private bool ProcessBarrierEvent(NetworkId targetObjectId, MyBufferedEvent eventToProcess, Handler eventHandler, IsBlockedHandler isBlockedHandler) => 
            (!isBlockedHandler(eventToProcess.TargetObjectId, eventToProcess.BlockingObjectId) ? this.ProcessEvents(eventToProcess.BlockingObjectId, eventHandler, isBlockedHandler, targetObjectId) : false);

        private bool ProcessBlockingEvent(NetworkId targetObjectId, MyBufferedEvent eventToProcess, NetworkId caller, Handler eventHandler, IsBlockedHandler isBlockedHandler, ref Queue<NetworkId> postProcessQueue)
        {
            if (isBlockedHandler(eventToProcess.TargetObjectId, eventToProcess.BlockingObjectId))
            {
                return false;
            }
            if (!this.TryLiftBarrier(eventToProcess.BlockingObjectId))
            {
                return this.ProcessEvents(eventToProcess.BlockingObjectId, eventHandler, isBlockedHandler, targetObjectId);
            }
            eventHandler(eventToProcess.Data, eventToProcess.TargetObjectId, eventToProcess.BlockingObjectId, eventToProcess.EventId, eventToProcess.Sender, eventToProcess.Position);
            eventToProcess.Data = null;
            if (eventToProcess.BlockingObjectId.IsValid && !eventToProcess.BlockingObjectId.Equals(caller))
            {
                postProcessQueue.Enqueue(eventToProcess.BlockingObjectId);
            }
            return true;
        }

        public bool ProcessEvents(NetworkId targetObjectId, Handler eventHandler, IsBlockedHandler isBlockedHandler, NetworkId caller)
        {
            MyObjectEventsBuffer buffer;
            bool flag = false;
            Queue<NetworkId> postProcessQueue = new Queue<NetworkId>();
            if (!this.m_buffer.TryGetValue(targetObjectId, out buffer))
            {
                return false;
            }
            if (buffer.IsProcessing)
            {
                return false;
            }
            buffer.IsProcessing = true;
            buffer.IsProcessing = false;
            if (!this.ProcessEventsBuffer(buffer, targetObjectId, eventHandler, isBlockedHandler, caller, ref postProcessQueue))
            {
                return false;
            }
            if (buffer.Events.Count == 0)
            {
                this.ReturnList(buffer.Events);
                buffer.Events = null;
                flag = true;
            }
            if (flag)
            {
                this.m_buffer.Remove(targetObjectId);
            }
            while (postProcessQueue.Count > 0)
            {
                NetworkId id = postProcessQueue.Dequeue();
                this.ProcessEvents(id, eventHandler, isBlockedHandler, targetObjectId);
            }
            return true;
        }

        private bool ProcessEventsBuffer(MyObjectEventsBuffer eventsBuffer, NetworkId targetObjectId, Handler eventHandler, IsBlockedHandler isBlockedHandler, NetworkId caller, ref Queue<NetworkId> postProcessQueue)
        {
            while (eventsBuffer.Events.Count > 0)
            {
                bool flag = true;
                MyBufferedEvent eventToProcess = eventsBuffer.Events.Peek();
                if (eventToProcess.Data != null)
                {
                    int bitPosition = eventToProcess.Data.Stream.BitPosition;
                }
                if (eventToProcess.IsBarrier)
                {
                    flag = this.ProcessBarrierEvent(targetObjectId, eventToProcess, eventHandler, isBlockedHandler);
                }
                else
                {
                    if (eventToProcess.BlockingObjectId.IsValid)
                    {
                        flag = this.ProcessBlockingEvent(targetObjectId, eventToProcess, caller, eventHandler, isBlockedHandler, ref postProcessQueue);
                    }
                    else
                    {
                        eventHandler(eventToProcess.Data, eventToProcess.TargetObjectId, eventToProcess.BlockingObjectId, eventToProcess.EventId, eventToProcess.Sender, eventToProcess.Position);
                        eventToProcess.Data = null;
                    }
                    if (flag)
                    {
                        eventsBuffer.Events.Dequeue();
                        if ((eventToProcess.Data != null) && !eventToProcess.Data.Stream.CheckTerminator())
                        {
                            MyLog.Default.WriteLine("RPC: Invalid stream terminator");
                        }
                        this.ReturnEvent(eventToProcess);
                    }
                }
                if (!flag)
                {
                    eventsBuffer.IsProcessing = false;
                    return false;
                }
            }
            return true;
        }

        public void RemoveEvents(NetworkId objectInstance)
        {
            MyObjectEventsBuffer buffer;
            if (this.m_buffer.TryGetValue(objectInstance, out buffer))
            {
                foreach (MyBufferedEvent event2 in buffer.Events)
                {
                    this.ReturnEvent(event2);
                }
                buffer.Events.Clear();
                this.ReturnList(buffer.Events);
                buffer.Events = null;
            }
            this.m_buffer.Remove(objectInstance);
        }

        private void ReturnEvent(MyBufferedEvent evnt)
        {
            if (evnt.Data != null)
            {
                evnt.Data.Return();
            }
            evnt.Data = null;
            this.m_eventPool.Push(evnt);
        }

        private void ReturnList(Queue<MyBufferedEvent> list)
        {
            this.m_listPool.Push(list);
        }

        private bool TryLiftBarrier(NetworkId targetObjectId)
        {
            MyObjectEventsBuffer buffer;
            if (this.m_buffer.TryGetValue(targetObjectId, out buffer))
            {
                MyBufferedEvent evnt = buffer.Events.Peek();
                if (evnt.IsBarrier && evnt.TargetObjectId.Equals(targetObjectId))
                {
                    buffer.Events.Dequeue();
                    this.ReturnEvent(evnt);
                    return true;
                }
            }
            return false;
        }

        public delegate void Handler(MyPacketDataBitStreamBase data, NetworkId objectInstance, NetworkId blockedNetId, uint eventId, EndpointId sender, Vector3D? position);

        public delegate bool IsBlockedHandler(NetworkId objectInstance, NetworkId blockedNetId);

        private class MyBufferedEvent
        {
            public MyPacketDataBitStreamBase Data;
            public NetworkId TargetObjectId;
            public NetworkId BlockingObjectId;
            public uint EventId;
            public EndpointId Sender;
            public bool IsBarrier;
            public Vector3D? Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyObjectEventsBuffer
        {
            public Queue<MyEventsBuffer.MyBufferedEvent> Events;
            public bool IsProcessing;
        }
    }
}


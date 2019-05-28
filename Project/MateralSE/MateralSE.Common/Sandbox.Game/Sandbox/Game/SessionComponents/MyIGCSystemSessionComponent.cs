namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.IntergridCommunication;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Library.Collections;
    using VRageMath;
    using VRageMath.PackedVector;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0x29a, typeof(MyObjectBuilder_MyIGCSystemSessionComponent), (Type) null)]
    internal class MyIGCSystemSessionComponent : MySessionComponentBase
    {
        private static MyIGCSystemSessionComponent m_static;
        private MySwapList<Message> m_messagesForNextTick = new MySwapList<Message>();
        private Queue<MyTuple<int, Action>> m_debugDrawQueue;
        private Dictionary<long, MyIntergridCommunicationContext> m_perPBCommContexts = new Dictionary<long, MyIntergridCommunicationContext>();
        private CachingHashSet<MyIntergridCommunicationContext> m_contextsWithPendingCallbacks = new CachingHashSet<MyIntergridCommunicationContext>();
        private Dictionary<string, CachingHashSet<BroadcastListener>> m_activeBroadcastListeners = new Dictionary<string, CachingHashSet<BroadcastListener>>();
        private List<long> m_idsToInitialize;

        public void AddDebugDraw(Action action)
        {
            if (this.m_debugDrawQueue == null)
            {
                this.m_debugDrawQueue = new Queue<MyTuple<int, Action>>();
            }
            this.m_debugDrawQueue.Enqueue(MyTuple.Create<int, Action>(MySession.Static.GameplayFrameCounter + 30, action));
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            if (this.m_idsToInitialize != null)
            {
                using (List<long>.Enumerator enumerator = this.m_idsToInitialize.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyProgrammableBlock entityById = (MyProgrammableBlock) MyEntities.GetEntityById(enumerator.Current, false);
                        if (entityById != null)
                        {
                            this.GetOrMakeContextFor(entityById);
                        }
                    }
                }
            }
        }

        public static object BoxMessage<TMessage>(TMessage message)
        {
            if (!MessageTypeChecker<TMessage>.IsAllowed)
            {
                throw new Exception("Message type " + typeof(TMessage) + " is not allowed!");
            }
            return message;
        }

        public void EnqueueMessage(Message message)
        {
            this.m_messagesForNextTick.Add(message);
        }

        public void EvictContextFor(MyProgrammableBlock block)
        {
            long entityId = block.EntityId;
            MyIntergridCommunicationContext contextForPB = this.GetContextForPB(entityId);
            if (contextForPB != null)
            {
                contextForPB.DisposeContext();
                this.m_perPBCommContexts.Remove(entityId);
                this.GetOrMakeContextFor(block);
            }
        }

        public MyIntergridCommunicationContext GetContextForPB(long programmableBlockId)
        {
            MyIntergridCommunicationContext context;
            this.m_perPBCommContexts.TryGetValue(programmableBlockId, out context);
            return context;
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_MyIGCSystemSessionComponent objectBuilder = (MyObjectBuilder_MyIGCSystemSessionComponent) base.GetObjectBuilder();
            objectBuilder.ActiveProgrammableBlocks = new List<long>(this.m_perPBCommContexts.Count);
            foreach (long num in this.m_perPBCommContexts.Keys)
            {
                objectBuilder.ActiveProgrammableBlocks.Add(num);
            }
            return objectBuilder;
        }

        public MyIntergridCommunicationContext GetOrMakeContextFor(MyProgrammableBlock block)
        {
            MyIntergridCommunicationContext context;
            long entityId = block.EntityId;
            if (!this.m_perPBCommContexts.TryGetValue(entityId, out context))
            {
                context = new MyIntergridCommunicationContext(block);
                this.m_perPBCommContexts.Add(entityId, context);
            }
            return context;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_MyIGCSystemSessionComponent component = (MyObjectBuilder_MyIGCSystemSessionComponent) sessionComponent;
            this.m_idsToInitialize = component.ActiveProgrammableBlocks;
        }

        public override void LoadData()
        {
            base.LoadData();
            m_static = this;
            this.BroadcasterProvider = new Action<MyCubeGrid, HashSet<MyDataBroadcaster>, long>(MyAntennaSystem.GetCubeGridGroupBroadcasters);
            this.ConnectionProvider = (target, source, rightsCheckedIdentity) => MyAntennaSystem.Static.CheckConnection(target, source, rightsCheckedIdentity, false);
        }

        public void RegisterBroadcastListener(BroadcastListener listener)
        {
            CachingHashSet<BroadcastListener> set;
            if (!this.m_activeBroadcastListeners.TryGetValue(listener.Tag, out set))
            {
                set = new CachingHashSet<BroadcastListener>();
                this.m_activeBroadcastListeners.Add(listener.Tag, set);
            }
            set.Add(listener);
        }

        public void RegisterContextWithPendingCallbacks(MyIntergridCommunicationContext context)
        {
            this.m_contextsWithPendingCallbacks.Add(context);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            using (Dictionary<long, MyIntergridCommunicationContext>.ValueCollection.Enumerator enumerator = this.m_perPBCommContexts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DisposeContext();
                }
            }
            this.m_debugDrawQueue = null;
            this.m_perPBCommContexts = null;
            this.ConnectionProvider = null;
            this.BroadcasterProvider = null;
            this.m_contextsWithPendingCallbacks = null;
            m_static = null;
        }

        public void UnregisterBroadcastListener(BroadcastListener listener)
        {
            this.m_activeBroadcastListeners[listener.Tag].Remove(listener, false);
        }

        public void UnregisterContextWithPendingCallbacks(MyIntergridCommunicationContext context)
        {
            this.m_contextsWithPendingCallbacks.Remove(context, false);
        }

        public override void UpdateBeforeSimulation()
        {
            // Invalid method body.
        }

        public static MyIGCSystemSessionComponent Static =>
            m_static;

        public Action<MyCubeGrid, HashSet<MyDataBroadcaster>, long> BroadcasterProvider { get; private set; }

        public Func<MyProgrammableBlock, MyDataBroadcaster, long, bool> ConnectionProvider { get; private set; }

        public override bool IsRequiredByGame =>
            true;

        public override Type[] Dependencies =>
            new Type[] { typeof(MyAntennaSystem) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyIGCSystemSessionComponent.<>c <>9 = new MyIGCSystemSessionComponent.<>c();
            public static Func<MyProgrammableBlock, MyDataBroadcaster, long, bool> <>9__30_0;

            internal bool <LoadData>b__30_0(MyProgrammableBlock target, MyDataBroadcaster source, long rightsCheckedIdentity) => 
                MyAntennaSystem.Static.CheckConnection(target, source, rightsCheckedIdentity, false);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public readonly string Tag;
            public readonly object Data;
            public readonly Sandbox.ModAPI.Ingame.TransmissionDistance TransmissionDistance;
            public readonly MyIntergridCommunicationContext Source;
            public readonly MyIntergridCommunicationContext UnicastDestination;
            public bool IsUnicast =>
                (this.UnicastDestination != null);
            private Message(object data, string tag, MyIntergridCommunicationContext source, MyIntergridCommunicationContext unicastDestination, Sandbox.ModAPI.Ingame.TransmissionDistance transmissionDistance)
            {
                this.Tag = tag;
                this.Data = data;
                this.Source = source;
                this.UnicastDestination = unicastDestination;
                this.TransmissionDistance = transmissionDistance;
            }

            public static MyIGCSystemSessionComponent.Message FromBroadcast(object data, string broadcastTag, Sandbox.ModAPI.Ingame.TransmissionDistance transmissionDistance, MyIntergridCommunicationContext source) => 
                new MyIGCSystemSessionComponent.Message(data, broadcastTag, source, null, transmissionDistance);

            public static MyIGCSystemSessionComponent.Message FromUnicast(object data, string unicastTag, MyIntergridCommunicationContext source, MyIntergridCommunicationContext unicastDestination) => 
                new MyIGCSystemSessionComponent.Message(data, unicastTag, source, unicastDestination, Sandbox.ModAPI.Ingame.TransmissionDistance.AntennaRelay);
        }

        private static class MessageTypeChecker<TMessageType>
        {
            public static readonly bool IsAllowed;

            static MessageTypeChecker()
            {
                MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsAllowed = MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsTypeAllowed(typeof(TMessageType), 0x19);
            }

            private static bool IsImmutableCollection(Type type) => 
                ((type == typeof(System.Collections.Immutable.ImmutableArray<>)) || ((type == typeof(ImmutableList<>)) || ((type == typeof(ImmutableQueue<>)) || ((type == typeof(ImmutableStack<>)) || ((type == typeof(ImmutableHashSet<>)) || ((type == typeof(ImmutableSortedSet<>)) || ((type == typeof(ImmutableDictionary<,>)) || (type == typeof(ImmutableSortedDictionary<,>)))))))));

            private static bool IsMyTuple(Type type, int genericArgs)
            {
                switch (genericArgs)
                {
                    case 1:
                        return (type == typeof(MyTuple<>));

                    case 2:
                        return (type == typeof(MyTuple<,>));

                    case 3:
                        return (type == typeof(MyTuple<,,>));

                    case 4:
                        return (type == typeof(MyTuple<,,,>));

                    case 5:
                        return (type == typeof(MyTuple<,,,,>));

                    case 6:
                        return (type == typeof(MyTuple<,,,,,>));
                }
                return false;
            }

            private static bool IsPrimitiveOfSafeStruct(Type type) => 
                (!type.IsPrimitive ? ((type == typeof(string)) || ((type == typeof(Ray)) || ((type == typeof(RayD)) || ((type == typeof(Line)) || ((type == typeof(LineD)) || ((type == typeof(Color)) || ((type == typeof(Plane)) || ((type == typeof(VRageMath.Point)) || ((type == typeof(PlaneD)) || ((type == typeof(MyQuad)) || ((type == typeof(Matrix)) || ((type == typeof(MatrixD)) || ((type == typeof(MatrixI)) || ((type == typeof(MyQuadD)) || ((type == typeof(Capsule)) || ((type == typeof(Vector2)) || ((type == typeof(Vector3)) || ((type == typeof(Vector4)) || ((type == typeof(CapsuleD)) || ((type == typeof(Vector2D)) || ((type == typeof(Vector2B)) || ((type == typeof(Vector3L)) || ((type == typeof(Vector4D)) || ((type == typeof(Vector3D)) || ((type == typeof(MyShort4)) || ((type == typeof(MyBounds)) || ((type == typeof(Vector3B)) || ((type == typeof(Vector3S)) || ((type == typeof(Vector2I)) || ((type == typeof(Vector4I)) || ((type == typeof(CubeFace)) || ((type == typeof(Vector3I)) || ((type == typeof(Matrix3x3)) || ((type == typeof(MyUShort4)) || ((type == typeof(Rectangle)) || ((type == typeof(Quaternion)) || ((type == typeof(RectangleF)) || ((type == typeof(BoundingBox)) || ((type == typeof(QuaternionD)) || ((type == typeof(MyTransform)) || ((type == typeof(BoundingBox2)) || ((type == typeof(BoundingBoxI)) || ((type == typeof(BoundingBoxD)) || ((type == typeof(MyTransformD)) || ((type == typeof(Vector3UByte)) || ((type == typeof(CurveTangent)) || ((type == typeof(Vector4UByte)) || ((type == typeof(BoundingBox2I)) || ((type == typeof(BoundingBox2D)) || ((type == typeof(Vector3Ushort)) || ((type == typeof(CurveLoopType)) || ((type == typeof(BoundingSphere)) || ((type == typeof(BoundingSphereD)) || ((type == typeof(ContainmentType)) || ((type == typeof(CurveContinuity)) || ((type == typeof(MyBlockOrientation)) || ((type == typeof(Base6Directions.Axis)) || ((type == typeof(MyOrientedBoundingBox)) || ((type == typeof(PlaneIntersectionType)) || ((type == typeof(MyOrientedBoundingBoxD)) || ((type == typeof(Vector3I_RangeIterator)) || ((type == typeof(Base6Directions.Direction)) || ((type == typeof(Base27Directions.Direction)) || ((type == typeof(CompressedPositionOrientation)) || ((type == typeof(Base6Directions.DirectionFlags)) || ((type == typeof(HalfVector3)) || ((type == typeof(HalfVector2)) || (type == typeof(HalfVector4))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) : true);

            private static bool IsTypeAllowed(Type type, int recursion)
            {
                if (recursion <= 0)
                {
                    return false;
                }
                if (!MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsPrimitiveOfSafeStruct(type))
                {
                    if (!type.IsGenericType)
                    {
                        return false;
                    }
                    Type[] genericArguments = type.GetGenericArguments();
                    Type genericTypeDefinition = type.GetGenericTypeDefinition();
                    if (!MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsMyTuple(genericTypeDefinition, genericArguments.Length) && !MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsImmutableCollection(genericTypeDefinition))
                    {
                        return false;
                    }
                    Type[] typeArray2 = genericArguments;
                    for (int i = 0; i < typeArray2.Length; i++)
                    {
                        if (!MyIGCSystemSessionComponent.MessageTypeChecker<TMessageType>.IsTypeAllowed(typeArray2[i], recursion - 1))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}


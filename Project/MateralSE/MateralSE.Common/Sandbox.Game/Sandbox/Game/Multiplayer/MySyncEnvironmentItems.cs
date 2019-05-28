namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.EnvironmentItems;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    public static class MySyncEnvironmentItems
    {
        public static Action<MyEntity, int> OnRemoveEnvironmentItem;

        [Event(null, 0x58), Reliable, Broadcast]
        private static void OnBatchAddItemMessage(long entityId, Vector3D position, MyStringHash subtypeId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.BatchAddItem(position, subtypeId, false);
            }
        }

        [Event(null, 0x68), Reliable, Broadcast]
        private static void OnBatchModifyItemMessage(long entityId, int localId, MyStringHash subtypeId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.BatchModifyItem(localId, subtypeId, false);
            }
        }

        [Event(null, 120), Reliable, Broadcast]
        private static void OnBatchRemoveItemMessage(long entityId, int localId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.BatchRemoveItem(localId, false);
            }
        }

        [Event(null, 0x48), Reliable, Broadcast]
        private static void OnBeginBatchAddMessage(long entityId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.BeginBatch(false);
            }
        }

        [Event(null, 0x88), Reliable, Broadcast]
        private static void OnEndBatchAddMessage(long entityId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.EndBatch(false);
            }
        }

        [Event(null, 0x33), Reliable, Broadcast]
        private static void OnModifyModelMessage(long entityId, int instanceId, MyStringHash subtypeId)
        {
            MyEnvironmentItems items;
            if (MyEntities.TryGetEntityById<MyEnvironmentItems>(entityId, out items, false))
            {
                items.ModifyItemModel(instanceId, subtypeId, true, false);
            }
            else
            {
                bool flag1 = MyFakes.ENABLE_FLORA_COMPONENT_DEBUG;
            }
        }

        [Event(null, 0x1d), Reliable, Server, BroadcastExcept]
        private static void OnRemoveEnvironmentItemMessage(long entityId, int itemInstanceId)
        {
            MyEntity entity;
            if (!MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                bool flag1 = MyFakes.ENABLE_FLORA_COMPONENT_DEBUG;
            }
            else if (OnRemoveEnvironmentItem != null)
            {
                OnRemoveEnvironmentItem(entity, itemInstanceId);
            }
        }

        public static void RemoveEnvironmentItem(long entityId, int itemInstanceId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MySyncEnvironmentItems.OnRemoveEnvironmentItemMessage), entityId, itemInstanceId, targetEndpoint, position);
        }

        public static void SendBatchAddItemMessage(long entityId, Vector3D position, MyStringHash subtypeId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3D, MyStringHash>(s => new Action<long, Vector3D, MyStringHash>(MySyncEnvironmentItems.OnBatchAddItemMessage), entityId, position, subtypeId, targetEndpoint, nullable);
        }

        public static void SendBatchModifyItemMessage(long entityId, int localId, MyStringHash subtypeId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, MyStringHash>(s => new Action<long, int, MyStringHash>(MySyncEnvironmentItems.OnBatchModifyItemMessage), entityId, localId, subtypeId, targetEndpoint, position);
        }

        public static void SendBatchRemoveItemMessage(long entityId, int localId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MySyncEnvironmentItems.OnBatchRemoveItemMessage), entityId, localId, targetEndpoint, position);
        }

        public static void SendBeginBatchAddMessage(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySyncEnvironmentItems.OnBeginBatchAddMessage), entityId, targetEndpoint, position);
        }

        public static void SendEndBatchAddMessage(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySyncEnvironmentItems.OnEndBatchAddMessage), entityId, targetEndpoint, position);
        }

        public static void SendModifyModelMessage(long entityId, int instanceId, MyStringHash subtypeId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, MyStringHash>(s => new Action<long, int, MyStringHash>(MySyncEnvironmentItems.OnModifyModelMessage), entityId, instanceId, subtypeId, targetEndpoint, position);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySyncEnvironmentItems.<>c <>9 = new MySyncEnvironmentItems.<>c();
            public static Func<IMyEventOwner, Action<long, int>> <>9__1_0;
            public static Func<IMyEventOwner, Action<long, int, MyStringHash>> <>9__3_0;
            public static Func<IMyEventOwner, Action<long>> <>9__5_0;
            public static Func<IMyEventOwner, Action<long, Vector3D, MyStringHash>> <>9__7_0;
            public static Func<IMyEventOwner, Action<long, int, MyStringHash>> <>9__9_0;
            public static Func<IMyEventOwner, Action<long, int>> <>9__11_0;
            public static Func<IMyEventOwner, Action<long>> <>9__13_0;

            internal Action<long, int> <RemoveEnvironmentItem>b__1_0(IMyEventOwner s) => 
                new Action<long, int>(MySyncEnvironmentItems.OnRemoveEnvironmentItemMessage);

            internal Action<long, Vector3D, MyStringHash> <SendBatchAddItemMessage>b__7_0(IMyEventOwner s) => 
                new Action<long, Vector3D, MyStringHash>(MySyncEnvironmentItems.OnBatchAddItemMessage);

            internal Action<long, int, MyStringHash> <SendBatchModifyItemMessage>b__9_0(IMyEventOwner s) => 
                new Action<long, int, MyStringHash>(MySyncEnvironmentItems.OnBatchModifyItemMessage);

            internal Action<long, int> <SendBatchRemoveItemMessage>b__11_0(IMyEventOwner s) => 
                new Action<long, int>(MySyncEnvironmentItems.OnBatchRemoveItemMessage);

            internal Action<long> <SendBeginBatchAddMessage>b__5_0(IMyEventOwner s) => 
                new Action<long>(MySyncEnvironmentItems.OnBeginBatchAddMessage);

            internal Action<long> <SendEndBatchAddMessage>b__13_0(IMyEventOwner s) => 
                new Action<long>(MySyncEnvironmentItems.OnEndBatchAddMessage);

            internal Action<long, int, MyStringHash> <SendModifyModelMessage>b__3_0(IMyEventOwner s) => 
                new Action<long, int, MyStringHash>(MySyncEnvironmentItems.OnModifyModelMessage);
        }
    }
}


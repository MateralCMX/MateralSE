namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [StaticEventOwner]
    public class MyToolBarCollection
    {
        private Dictionary<MyPlayer.PlayerId, MyToolbar> m_playerToolbars = new Dictionary<MyPlayer.PlayerId, MyToolbar>();

        public bool AddPlayerToolbar(MyPlayer.PlayerId pid, MyToolbar toolbar)
        {
            MyToolbar toolbar2;
            if (toolbar == null)
            {
                return false;
            }
            if (this.m_playerToolbars.TryGetValue(pid, out toolbar2))
            {
                return false;
            }
            this.m_playerToolbars.Add(pid, toolbar);
            return true;
        }

        public bool ContainsToolbar(MyPlayer.PlayerId pid) => 
            this.m_playerToolbars.ContainsKey(pid);

        private void CreateDefaultToolbar(MyPlayer.PlayerId playerId)
        {
            if (!this.ContainsToolbar(playerId))
            {
                MyToolbar toolbar = new MyToolbar(MyToolbarType.Character, 9, 9);
                toolbar.Init(MySession.Static.Scenario.DefaultToolbar, null, true);
                this.AddPlayerToolbar(playerId, toolbar);
            }
        }

        private static ulong GetSenderIdSafe() => 
            (!MyEventContext.Current.IsLocallyInvoked ? MyEventContext.Current.Sender.Value : Sync.MyId);

        public void LoadToolbars(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (checkpoint.AllPlayersData != null)
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> pair in checkpoint.AllPlayersData.Dictionary)
                {
                    MyPlayer.PlayerId pid = new MyPlayer.PlayerId(pair.Key.ClientId, pair.Key.SerialId);
                    MyToolbar toolbar = new MyToolbar(MyToolbarType.Character, 9, 9);
                    toolbar.Init(pair.Value.Toolbar, null, true);
                    this.AddPlayerToolbar(pid, toolbar);
                }
            }
        }

        [Event(null, 80), Reliable, Server]
        private static void OnChangeSlotBuilderItemRequest(int playerSerialId, int index, [Serialize(MyObjectFlags.Dynamic, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_ToolbarItem itemBuilder)
        {
            MyPlayer.PlayerId pid = new MyPlayer.PlayerId(GetSenderIdSafe(), playerSerialId);
            if (MySession.Static.Toolbars.ContainsToolbar(pid))
            {
                MyToolbarItem item = MyToolbarItemFactory.CreateToolbarItem(itemBuilder);
                MyToolbar toolbar = MySession.Static.Toolbars.TryGetPlayerToolbar(pid);
                if (toolbar != null)
                {
                    toolbar.SetItemAtIndex(index, item);
                }
            }
        }

        [Event(null, 0x36), Reliable, Server]
        private static void OnChangeSlotItemRequest(int playerSerialId, int index, DefinitionIdBlit defId)
        {
            MyPlayer.PlayerId pid = new MyPlayer.PlayerId(GetSenderIdSafe(), playerSerialId);
            if (MySession.Static.Toolbars.ContainsToolbar(pid))
            {
                MyDefinitionBase base2;
                MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>((MyDefinitionId) defId, out base2);
                if (base2 != null)
                {
                    MyToolbarItem item = MyToolbarItemFactory.CreateToolbarItem(MyToolbarItemFactory.ObjectBuilderFromDefinition(base2));
                    MyToolbar toolbar = MySession.Static.Toolbars.TryGetPlayerToolbar(pid);
                    if (toolbar != null)
                    {
                        toolbar.SetItemAtIndex(index, item);
                    }
                }
            }
        }

        [Event(null, 0x22), Reliable, Server]
        private static void OnClearSlotRequest(int playerSerialId, int index)
        {
            MyPlayer.PlayerId pid = new MyPlayer.PlayerId(GetSenderIdSafe(), playerSerialId);
            if (MySession.Static.Toolbars.ContainsToolbar(pid))
            {
                MySession.Static.Toolbars.TryGetPlayerToolbar(pid).SetItemAtIndex(index, null);
            }
        }

        [Event(null, 0x65), Reliable, Server]
        private static void OnNewToolbarRequest(int playerSerialId)
        {
            MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(GetSenderIdSafe(), playerSerialId);
            MySession.Static.Toolbars.CreateDefaultToolbar(playerId);
        }

        public bool RemovePlayerToolbar(MyPlayer.PlayerId pid) => 
            this.m_playerToolbars.Remove(pid);

        public static void RequestChangeSlotItem(MyPlayer.PlayerId pid, int index, MyDefinitionId defId)
        {
            DefinitionIdBlit blit = new DefinitionIdBlit();
            blit = defId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int, int, DefinitionIdBlit>(s => new Action<int, int, DefinitionIdBlit>(MyToolBarCollection.OnChangeSlotItemRequest), pid.SerialId, index, blit, targetEndpoint, position);
        }

        public static void RequestChangeSlotItem(MyPlayer.PlayerId pid, int index, MyObjectBuilder_ToolbarItem itemBuilder)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int, int, MyObjectBuilder_ToolbarItem>(s => new Action<int, int, MyObjectBuilder_ToolbarItem>(MyToolBarCollection.OnChangeSlotBuilderItemRequest), pid.SerialId, index, itemBuilder, targetEndpoint, position);
        }

        public static void RequestClearSlot(MyPlayer.PlayerId pid, int index)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int, int>(s => new Action<int, int>(MyToolBarCollection.OnClearSlotRequest), pid.SerialId, index, targetEndpoint, position);
        }

        public static void RequestCreateToolbar(MyPlayer.PlayerId pid)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(MyToolBarCollection.OnNewToolbarRequest), pid.SerialId, targetEndpoint, position);
        }

        public void SaveToolbars(MyObjectBuilder_Checkpoint checkpoint)
        {
            Dictionary<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> dictionary = checkpoint.AllPlayersData.Dictionary;
            foreach (KeyValuePair<MyPlayer.PlayerId, MyToolbar> pair in this.m_playerToolbars)
            {
                MyObjectBuilder_Checkpoint.PlayerId key = new MyObjectBuilder_Checkpoint.PlayerId(pair.Key.SteamId) {
                    SerialId = pair.Key.SerialId
                };
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key].Toolbar = pair.Value.GetObjectBuilder();
                }
            }
        }

        public MyToolbar TryGetPlayerToolbar(MyPlayer.PlayerId pid)
        {
            MyToolbar toolbar;
            this.m_playerToolbars.TryGetValue(pid, out toolbar);
            return toolbar;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyToolBarCollection.<>c <>9 = new MyToolBarCollection.<>c();
            public static Func<IMyEventOwner, Action<int, int>> <>9__0_0;
            public static Func<IMyEventOwner, Action<int, int, DefinitionIdBlit>> <>9__2_0;
            public static Func<IMyEventOwner, Action<int, int, MyObjectBuilder_ToolbarItem>> <>9__4_0;
            public static Func<IMyEventOwner, Action<int>> <>9__6_0;

            internal Action<int, int, DefinitionIdBlit> <RequestChangeSlotItem>b__2_0(IMyEventOwner s) => 
                new Action<int, int, DefinitionIdBlit>(MyToolBarCollection.OnChangeSlotItemRequest);

            internal Action<int, int, MyObjectBuilder_ToolbarItem> <RequestChangeSlotItem>b__4_0(IMyEventOwner s) => 
                new Action<int, int, MyObjectBuilder_ToolbarItem>(MyToolBarCollection.OnChangeSlotBuilderItemRequest);

            internal Action<int, int> <RequestClearSlot>b__0_0(IMyEventOwner s) => 
                new Action<int, int>(MyToolBarCollection.OnClearSlotRequest);

            internal Action<int> <RequestCreateToolbar>b__6_0(IMyEventOwner s) => 
                new Action<int>(MyToolBarCollection.OnNewToolbarRequest);
        }
    }
}


namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Network;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x7d0, typeof(MyObjectBuilder_MySessionComponentDLC), (Type) null)]
    public class MySessionComponentDLC : MySessionComponentBase
    {
        private HashSet<uint> m_availableDLCs;
        private Dictionary<ulong, HashSet<uint>> m_clientAvailableDLCs;

        public MyDLCs.MyDLC GetFirstMissingDefinitionDLC(MyDefinitionBase definition, ulong steamId)
        {
            if (definition.DLCs != null)
            {
                foreach (string str in definition.DLCs)
                {
                    if (!this.HasDLC(str, steamId))
                    {
                        MyDLCs.MyDLC ydlc;
                        MyDLCs.TryGetDLC(str, out ydlc);
                        return ydlc;
                    }
                }
            }
            return null;
        }

        private bool HasClientDLC(uint DLCId, ulong steamId)
        {
            HashSet<uint> set;
            if (!this.m_clientAvailableDLCs.TryGetValue(steamId, out set))
            {
                this.UpdateClientDLC(steamId);
                set = this.m_clientAvailableDLCs[steamId];
            }
            return set.Contains(DLCId);
        }

        public bool HasDefinitionDLC(MyDefinitionBase definition, ulong steamId)
        {
            if (definition.DLCs != null)
            {
                foreach (string str in definition.DLCs)
                {
                    if (!this.HasDLC(str, steamId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool HasDefinitionDLC(MyDefinitionId definitionId, ulong steamId)
        {
            MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(definitionId);
            return this.HasDefinitionDLC(definition, steamId);
        }

        private bool HasDLC(string DLCName, ulong steamId)
        {
            MyDLCs.MyDLC ydlc;
            MyDLCs.MyDLC ydlc2;
            return (!MyFakes.OWN_ALL_DLCS ? ((steamId != 0) ? ((steamId != Sync.MyId) ? (Sync.IsServer && (MyDLCs.TryGetDLC(DLCName, out ydlc2) && this.HasClientDLC(ydlc2.AppId, steamId))) : (!Sync.IsDedicated ? (MyDLCs.TryGetDLC(DLCName, out ydlc) && this.m_availableDLCs.Contains(ydlc.AppId)) : false)) : false) : true);
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            this.m_availableDLCs = new HashSet<uint>();
            if (!Sync.IsDedicated)
            {
                this.UpdateLocalPlayerDLC();
                MyGameService.OnDLCInstalled += new Action<uint>(this.OnDLCInstalled);
            }
            if ((MyMultiplayer.Static != null) && Sync.IsServer)
            {
                this.m_clientAvailableDLCs = new Dictionary<ulong, HashSet<uint>>();
                MyMultiplayer.Static.ClientJoined += new Action<ulong>(this.UpdateClientDLC);
            }
        }

        private void OnDLCInstalled(uint dlcId)
        {
            this.m_availableDLCs.Add(dlcId);
            if (!Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent(x => new Action(MySessionComponentDLC.RequestUpdateClientDLC), targetEndpoint, position);
            }
        }

        [Event(null, 0x53), Reliable, Server]
        public static void RequestUpdateClientDLC()
        {
            MySession.Static.GetComponent<MySessionComponentDLC>().UpdateClientDLC(MyEventContext.Current.Sender.Value);
        }

        protected override void UnloadData()
        {
            if (!Sync.IsDedicated)
            {
                MyGameService.OnDLCInstalled -= new Action<uint>(this.OnDLCInstalled);
            }
            if ((MyMultiplayer.Static != null) && Sync.IsServer)
            {
                MyMultiplayer.Static.ClientJoined -= new Action<ulong>(this.UpdateClientDLC);
            }
            base.UnloadData();
        }

        private void UpdateClientDLC(ulong steamId)
        {
            HashSet<uint> set;
            if (!this.m_clientAvailableDLCs.TryGetValue(steamId, out set))
            {
                set = new HashSet<uint>();
                this.m_clientAvailableDLCs.Add(steamId, set);
            }
            foreach (uint num in MyDLCs.DLCs.Keys)
            {
                if (!Sync.IsDedicated || MyGameService.GameServer.UserHasLicenseForApp(steamId, num))
                {
                    set.Add(num);
                    if (num == MyFakes.SWITCH_DLC_FROM)
                    {
                        set.Add(MyFakes.SWITCH_DLC_TO);
                    }
                }
            }
        }

        private void UpdateLocalPlayerDLC()
        {
            int dLCCount = MyGameService.GetDLCCount();
            for (int i = 0; i < dLCCount; i++)
            {
                uint num3;
                bool flag;
                string str;
                MyGameService.GetDLCDataByIndex(i, out num3, out flag, out str, 0x80);
                if (MyGameService.IsDlcInstalled(num3))
                {
                    this.m_availableDLCs.Add(num3);
                    if (num3 == MyFakes.SWITCH_DLC_FROM)
                    {
                        this.m_availableDLCs.Add(MyFakes.SWITCH_DLC_TO);
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentDLC.<>c <>9 = new MySessionComponentDLC.<>c();
            public static Func<IMyEventOwner, Action> <>9__12_0;

            internal Action <OnDLCInstalled>b__12_0(IMyEventOwner x) => 
                new Action(MySessionComponentDLC.RequestUpdateClientDLC);
        }
    }
}


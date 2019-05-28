namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage;
    using VRage.Sync;

    public class MyGridRadioSystem
    {
        private HashSet<MyDataBroadcaster> m_broadcasters = new HashSet<MyDataBroadcaster>();
        private HashSet<MyDataReceiver> m_receivers = new HashSet<MyDataReceiver>();
        private HashSet<MyRadioBroadcaster> m_radioBroadcasters = new HashSet<MyRadioBroadcaster>();
        private HashSet<MyRadioReceiver> m_radioReceivers = new HashSet<MyRadioReceiver>();
        private HashSet<MyLaserBroadcaster> m_laserBroadcasters = new HashSet<MyLaserBroadcaster>();
        private HashSet<MyLaserReceiver> m_laserReceivers = new HashSet<MyLaserReceiver>();
        public bool IsClosing;
        private MyMultipleEnabledEnum m_antennasBroadcasterEnabled = MyMultipleEnabledEnum.NoObjects;
        private bool m_antennasBroadcasterEnabledNeedsRefresh;
        private MyCubeGrid m_grid;

        public MyGridRadioSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
        }

        private void broadcaster_EnabledChanged()
        {
            this.m_antennasBroadcasterEnabledNeedsRefresh = true;
        }

        public void BroadcasterStateChanged(MyMultipleEnabledEnum enabledState)
        {
            this.m_antennasBroadcasterEnabled = enabledState;
            bool flag = enabledState == MyMultipleEnabledEnum.AllEnabled;
            if (Sync.IsServer)
            {
                foreach (MyRadioBroadcaster broadcaster in this.m_radioBroadcasters)
                {
                    if (broadcaster.Entity is MyRadioAntenna)
                    {
                        (broadcaster.Entity as MyRadioAntenna).EnableBroadcasting.Value = flag;
                    }
                }
            }
            this.m_antennasBroadcasterEnabledNeedsRefresh = false;
        }

        private void RefreshAntennasBroadcasterEnabled()
        {
            this.m_antennasBroadcasterEnabledNeedsRefresh = false;
            bool flag = true;
            bool flag2 = true;
            using (HashSet<MyRadioBroadcaster>.Enumerator enumerator = this.m_radioBroadcasters.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyRadioBroadcaster current = enumerator.Current;
                    if (current.Entity is MyRadioAntenna)
                    {
                        flag = flag && current.Enabled;
                        flag2 = flag2 && !current.Enabled;
                        if (!flag && !flag2)
                        {
                            this.m_antennasBroadcasterEnabled = MyMultipleEnabledEnum.Mixed;
                            return;
                        }
                    }
                }
            }
            this.AntennasBroadcasterEnabled = flag ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
        }

        public void Register(MyDataBroadcaster broadcaster)
        {
            this.m_broadcasters.Add(broadcaster);
            MyLaserBroadcaster item = broadcaster as MyLaserBroadcaster;
            if (item != null)
            {
                this.m_laserBroadcasters.Add(item);
            }
            else
            {
                MyRadioBroadcaster broadcaster3 = broadcaster as MyRadioBroadcaster;
                if ((broadcaster3 != null) && (broadcaster.Entity is MyRadioAntenna))
                {
                    this.m_radioBroadcasters.Add(broadcaster3);
                    (broadcaster.Entity as MyRadioAntenna).EnableBroadcasting.ValueChanged += obj => this.broadcaster_EnabledChanged();
                    if (this.m_radioBroadcasters.Count == 1)
                    {
                        this.m_antennasBroadcasterEnabled = broadcaster3.Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
                    }
                    else if (((this.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.AllEnabled) && !broadcaster3.Enabled) || ((this.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.AllDisabled) && broadcaster3.Enabled))
                    {
                        this.m_antennasBroadcasterEnabled = MyMultipleEnabledEnum.Mixed;
                    }
                }
            }
        }

        public void Register(MyDataReceiver reciever)
        {
            this.m_receivers.Add(reciever);
            MyLaserReceiver item = reciever as MyLaserReceiver;
            if (item != null)
            {
                this.m_laserReceivers.Add(item);
            }
            else
            {
                MyRadioReceiver receiver2 = reciever as MyRadioReceiver;
                if (receiver2 != null)
                {
                    this.m_radioReceivers.Add(receiver2);
                }
            }
        }

        public void Unregister(MyDataBroadcaster broadcaster)
        {
            this.m_broadcasters.Remove(broadcaster);
            MyLaserBroadcaster item = broadcaster as MyLaserBroadcaster;
            if (item != null)
            {
                this.m_laserBroadcasters.Remove(item);
            }
            else
            {
                MyRadioBroadcaster broadcaster3 = broadcaster as MyRadioBroadcaster;
                if ((broadcaster3 != null) && (broadcaster.Entity is MyRadioAntenna))
                {
                    this.m_radioBroadcasters.Remove(broadcaster3);
                    (broadcaster.Entity as MyRadioAntenna).EnableBroadcasting.ValueChanged -= obj => this.broadcaster_EnabledChanged();
                    if (this.m_radioBroadcasters.Count == 0)
                    {
                        this.m_antennasBroadcasterEnabled = MyMultipleEnabledEnum.NoObjects;
                    }
                    else if (this.m_radioBroadcasters.Count == 1)
                    {
                        this.AntennasBroadcasterEnabled = this.m_radioBroadcasters.First<MyRadioBroadcaster>().Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
                    }
                    else if (this.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.Mixed)
                    {
                        this.m_antennasBroadcasterEnabledNeedsRefresh = true;
                    }
                }
            }
        }

        public void Unregister(MyDataReceiver reciever)
        {
            this.m_receivers.Remove(reciever);
            MyLaserReceiver item = reciever as MyLaserReceiver;
            if (item != null)
            {
                this.m_laserReceivers.Remove(item);
            }
            else
            {
                MyRadioReceiver receiver2 = reciever as MyRadioReceiver;
                if (receiver2 != null)
                {
                    this.m_radioReceivers.Remove(receiver2);
                }
            }
        }

        public void UpdateRemoteControlInfo()
        {
            using (HashSet<MyDataBroadcaster>.Enumerator enumerator = this.m_broadcasters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateRemoteControlInfo();
                }
            }
        }

        public MyMultipleEnabledEnum AntennasBroadcasterEnabled
        {
            get
            {
                if (this.m_antennasBroadcasterEnabledNeedsRefresh)
                {
                    this.RefreshAntennasBroadcasterEnabled();
                }
                return this.m_antennasBroadcasterEnabled;
            }
            set
            {
                if (((this.m_antennasBroadcasterEnabled != value) && (this.m_antennasBroadcasterEnabled != MyMultipleEnabledEnum.NoObjects)) && !this.IsClosing)
                {
                    this.BroadcasterStateChanged(value);
                }
            }
        }

        public HashSet<MyDataBroadcaster> Broadcasters =>
            this.m_broadcasters;

        public HashSet<MyDataReceiver> Receivers =>
            this.m_receivers;

        public HashSet<MyLaserBroadcaster> LaserBroadcasters =>
            this.m_laserBroadcasters;

        public HashSet<MyLaserReceiver> LaserReceivers =>
            this.m_laserReceivers;
    }
}


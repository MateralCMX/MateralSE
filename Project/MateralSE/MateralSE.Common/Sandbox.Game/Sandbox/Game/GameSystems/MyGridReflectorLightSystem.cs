namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage;

    public class MyGridReflectorLightSystem
    {
        private HashSet<MyReflectorLight> m_reflectors = new HashSet<MyReflectorLight>();
        public bool IsClosing;
        private MyMultipleEnabledEnum m_reflectorsEnabled = MyMultipleEnabledEnum.NoObjects;
        private bool m_reflectorsEnabledNeedsRefresh;
        private MyCubeGrid m_grid;

        public MyGridReflectorLightSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
        }

        private void reflector_EnabledChanged(MyTerminalBlock obj)
        {
            this.m_reflectorsEnabledNeedsRefresh = true;
        }

        public void ReflectorStateChanged(MyMultipleEnabledEnum enabledState)
        {
            this.m_reflectorsEnabled = enabledState;
            if (Sync.IsServer)
            {
                bool flag = enabledState == MyMultipleEnabledEnum.AllEnabled;
                foreach (MyReflectorLight local1 in this.m_reflectors)
                {
                    local1.EnabledChanged -= new Action<MyTerminalBlock>(this.reflector_EnabledChanged);
                    local1.Enabled = flag;
                    local1.EnabledChanged += new Action<MyTerminalBlock>(this.reflector_EnabledChanged);
                }
                this.m_reflectorsEnabledNeedsRefresh = false;
            }
        }

        private void RefreshReflectorsEnabled()
        {
            this.m_reflectorsEnabledNeedsRefresh = false;
            if (Sync.IsServer)
            {
                bool flag = true;
                bool flag2 = true;
                using (HashSet<MyReflectorLight>.Enumerator enumerator = this.m_reflectors.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyReflectorLight current = enumerator.Current;
                        flag = flag && current.Enabled;
                        flag2 = flag2 && !current.Enabled;
                        if (!flag && !flag2)
                        {
                            this.m_reflectorsEnabled = MyMultipleEnabledEnum.Mixed;
                            return;
                        }
                    }
                }
                this.ReflectorsEnabled = flag ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
            }
        }

        public void Register(MyReflectorLight reflector)
        {
            this.m_reflectors.Add(reflector);
            reflector.EnabledChanged += new Action<MyTerminalBlock>(this.reflector_EnabledChanged);
            if (this.m_reflectors.Count == 1)
            {
                this.m_reflectorsEnabled = reflector.Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
            }
            else if (((this.ReflectorsEnabled == MyMultipleEnabledEnum.AllEnabled) && !reflector.Enabled) || ((this.ReflectorsEnabled == MyMultipleEnabledEnum.AllDisabled) && reflector.Enabled))
            {
                this.m_reflectorsEnabled = MyMultipleEnabledEnum.Mixed;
            }
        }

        public void Unregister(MyReflectorLight reflector)
        {
            this.m_reflectors.Remove(reflector);
            reflector.EnabledChanged -= new Action<MyTerminalBlock>(this.reflector_EnabledChanged);
            if (this.m_reflectors.Count == 0)
            {
                this.m_reflectorsEnabled = MyMultipleEnabledEnum.NoObjects;
            }
            else if (this.m_reflectors.Count == 1)
            {
                this.m_reflectorsEnabled = this.m_reflectors.First<MyReflectorLight>().Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
            }
            else if (this.ReflectorsEnabled == MyMultipleEnabledEnum.Mixed)
            {
                this.m_reflectorsEnabledNeedsRefresh = true;
            }
        }

        public int ReflectorCount =>
            this.m_reflectors.Count;

        public MyMultipleEnabledEnum ReflectorsEnabled
        {
            get
            {
                if (this.m_reflectorsEnabledNeedsRefresh)
                {
                    this.RefreshReflectorsEnabled();
                }
                return this.m_reflectorsEnabled;
            }
            set
            {
                if (((this.m_reflectorsEnabled != value) && (this.m_reflectorsEnabled != MyMultipleEnabledEnum.NoObjects)) && !this.IsClosing)
                {
                    this.m_grid.SendReflectorState(value);
                }
            }
        }
    }
}


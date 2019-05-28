namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.Localization;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage;
    using VRage.ModAPI;
    using VRage.Utils;

    public class MyGridLandingSystem
    {
        private static readonly int GEAR_MODE_COUNT = (MyUtils.GetMaxValueFromEnum<LandingGearMode>() + 1);
        private static readonly List<IMyLandingGear> m_gearTmpList = new List<IMyLandingGear>();
        private HashSet<IMyLandingGear>[] m_gearStates = new HashSet<IMyLandingGear>[GEAR_MODE_COUNT];
        private LockModeChangedHandler m_onStateChanged;
        public MyStringId HudMessage = MyStringId.NullOrEmpty;

        public MyGridLandingSystem()
        {
            for (int i = 0; i < GEAR_MODE_COUNT; i++)
            {
                this.m_gearStates[i] = new HashSet<IMyLandingGear>();
            }
            this.m_onStateChanged = new LockModeChangedHandler(this.StateChanged);
        }

        public List<IMyEntity> GetAttachedEntities()
        {
            List<IMyEntity> list = new List<IMyEntity>();
            using (HashSet<IMyLandingGear>.Enumerator enumerator = this.m_gearStates[2].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyEntity attachedEntity = enumerator.Current.GetAttachedEntity();
                    if (attachedEntity != null)
                    {
                        list.Add(attachedEntity);
                    }
                }
            }
            return list;
        }

        public void Register(IMyLandingGear gear)
        {
            gear.LockModeChanged += this.m_onStateChanged;
            this.m_gearStates[(int) gear.LockMode].Add(gear);
        }

        private void StateChanged(IMyLandingGear gear, LandingGearMode oldMode)
        {
            if ((oldMode == LandingGearMode.ReadyToLock) && (gear.LockMode == LandingGearMode.Locked))
            {
                this.HudMessage = MySpaceTexts.NotificationLandingGearSwitchLocked;
            }
            else if ((oldMode != LandingGearMode.Locked) || (gear.LockMode != LandingGearMode.Unlocked))
            {
                this.HudMessage = MyStringId.NullOrEmpty;
            }
            else
            {
                this.HudMessage = MySpaceTexts.NotificationLandingGearSwitchUnlocked;
            }
            this.m_gearStates[(int) oldMode].Remove(gear);
            this.m_gearStates[(int) gear.LockMode].Add(gear);
        }

        public void Switch()
        {
            if ((this.Locked == MyMultipleEnabledEnum.AllEnabled) || (this.Locked == MyMultipleEnabledEnum.Mixed))
            {
                this.Switch(false);
            }
            else if (this.Locked == MyMultipleEnabledEnum.AllDisabled)
            {
                this.Switch(true);
            }
        }

        public void Switch(bool enabled)
        {
            int index = enabled ? 1 : 2;
            bool flag = !enabled && (this.m_gearStates[2].Count > 0);
            foreach (IMyLandingGear gear in this.m_gearStates[index])
            {
                m_gearTmpList.Add(gear);
            }
            if (enabled)
            {
                foreach (IMyLandingGear gear2 in this.m_gearStates[0])
                {
                    m_gearTmpList.Add(gear2);
                }
            }
            using (List<IMyLandingGear>.Enumerator enumerator2 = m_gearTmpList.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.RequestLock(enabled);
                }
            }
            m_gearTmpList.Clear();
            if (flag)
            {
                HashSet<IMyLandingGear>[] gearStates = this.m_gearStates;
                for (int i = 0; i < gearStates.Length; i++)
                {
                    foreach (IMyLandingGear gear3 in gearStates[i])
                    {
                        if (gear3.AutoLock)
                        {
                            gear3.ResetAutolock();
                        }
                    }
                }
            }
        }

        public void Unregister(IMyLandingGear gear)
        {
            this.m_gearStates[(int) gear.LockMode].Remove(gear);
            gear.LockModeChanged -= this.m_onStateChanged;
        }

        public MyMultipleEnabledEnum Locked
        {
            get
            {
                int totalGearCount = this.TotalGearCount;
                return ((totalGearCount != 0) ? ((totalGearCount != this[LandingGearMode.Locked]) ? ((totalGearCount != (this[LandingGearMode.ReadyToLock] + this[LandingGearMode.Unlocked])) ? MyMultipleEnabledEnum.Mixed : MyMultipleEnabledEnum.AllDisabled) : MyMultipleEnabledEnum.AllEnabled) : MyMultipleEnabledEnum.NoObjects);
            }
        }

        public int TotalGearCount
        {
            get
            {
                int num = 0;
                for (int i = 0; i < GEAR_MODE_COUNT; i++)
                {
                    num += this.m_gearStates[i].Count;
                }
                return num;
            }
        }

        public int this[LandingGearMode mode] =>
            this.m_gearStates[(int) mode].Count;
    }
}


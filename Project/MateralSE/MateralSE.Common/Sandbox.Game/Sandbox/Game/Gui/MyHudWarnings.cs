namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game.Components;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class MyHudWarnings : MySessionComponentBase
    {
        public static readonly int FRAMES_BETWEEN_UPDATE = 30;
        public static bool EnableWarnings = true;
        private static List<MyHudWarningGroup> m_hudWarnings = new List<MyHudWarningGroup>();
        private static List<MyGuiSounds> m_soundQueue = new List<MyGuiSounds>();
        private static IMySourceVoice m_sound;
        private static int m_lastSoundPlayed = 0;
        private int m_updateCounter;

        public static void Add(MyHudWarningGroup hudWarningGroup)
        {
            m_hudWarnings.Add(hudWarningGroup);
        }

        private static bool EnergyCritWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            if (MySession.Static.ControlledEntity.Entity is MyCharacter)
            {
                if (!IsEnergyUnderTreshold(0.1f))
                {
                    return false;
                }
                cue = MyGuiSounds.HudVocEnergyCrit;
                if (((MySession.Static.LocalCharacter == null) || ((MySession.Static.LocalCharacter.OxygenComponent == null) || !MySession.Static.LocalCharacter.OxygenComponent.NeedsOxygenFromSuit)) || !MySession.Static.Settings.EnableOxygen)
                {
                    text = MySpaceTexts.NotificationSuitEnergyCritical;
                }
                else
                {
                    text = MySpaceTexts.NotificationSuitEnergyCriticalNoDamage;
                }
                goto TR_0002;
            }
            else
            {
                if (!(MySession.Static.ControlledEntity.Entity is MyCockpit))
                {
                    return false;
                }
                if (!IsEnergyUnderTreshold(0.05f))
                {
                    return false;
                }
                MyCockpit entity = (MyCockpit) MySession.Static.ControlledEntity.Entity;
                bool flag = false;
                List<MyCubeGrid> groupNodes = MyCubeGridGroups.Static.Logical.GetGroupNodes(entity.CubeGrid);
                if (groupNodes == null)
                {
                    goto TR_000C;
                }
                else if (groupNodes.Count != 0)
                {
                    using (List<MyCubeGrid>.Enumerator enumerator = groupNodes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.NumberOfReactors > 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                    cue = !entity.CubeGrid.IsStatic ? MyGuiSounds.HudVocShipFuelCrit : MyGuiSounds.HudVocStationFuelCrit;
                    if (((MySession.Static.LocalCharacter == null) || ((MySession.Static.LocalCharacter.OxygenComponent == null) || !MySession.Static.LocalCharacter.OxygenComponent.NeedsOxygenFromSuit)) || !MySession.Static.Settings.EnableOxygen)
                    {
                        text = MySpaceTexts.NotificationShipEnergyCritical;
                    }
                    else
                    {
                        text = MySpaceTexts.NotificationShipEnergyCriticalNoDamage;
                    }
                }
                else
                {
                    goto TR_000C;
                }
                goto TR_0002;
            }
            goto TR_000C;
        TR_0002:
            return true;
        TR_000C:
            return false;
        }

        private static bool EnergyLowWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            if (MySession.Static.ControlledEntity.Entity is MyCharacter)
            {
                if (!IsEnergyUnderTreshold(0.25f))
                {
                    return false;
                }
                cue = MyGuiSounds.HudVocEnergyLow;
                if (((MySession.Static.LocalCharacter == null) || ((MySession.Static.LocalCharacter.OxygenComponent == null) || !MySession.Static.LocalCharacter.OxygenComponent.NeedsOxygenFromSuit)) || !MySession.Static.Settings.EnableOxygen)
                {
                    text = MySpaceTexts.NotificationSuitEnergyLow;
                }
                else
                {
                    text = MySpaceTexts.NotificationSuitEnergyLowNoDamage;
                }
                goto TR_0002;
            }
            else
            {
                if (!(MySession.Static.ControlledEntity.Entity is MyCockpit))
                {
                    return false;
                }
                if (!IsEnergyUnderTreshold(0.125f))
                {
                    return false;
                }
                MyCockpit entity = (MyCockpit) MySession.Static.ControlledEntity.Entity;
                bool flag = false;
                List<MyCubeGrid> groupNodes = MyCubeGridGroups.Static.Logical.GetGroupNodes(entity.CubeGrid);
                if (groupNodes == null)
                {
                    goto TR_000C;
                }
                else if (groupNodes.Count != 0)
                {
                    using (List<MyCubeGrid>.Enumerator enumerator = groupNodes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.NumberOfReactors > 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                    cue = !entity.CubeGrid.IsStatic ? MyGuiSounds.HudVocShipFuelLow : MyGuiSounds.HudVocStationFuelLow;
                    if (((MySession.Static.LocalCharacter == null) || ((MySession.Static.LocalCharacter.OxygenComponent == null) || !MySession.Static.LocalCharacter.OxygenComponent.NeedsOxygenFromSuit)) || !MySession.Static.Settings.EnableOxygen)
                    {
                        text = MySpaceTexts.NotificationShipEnergyLow;
                    }
                    else
                    {
                        text = MySpaceTexts.NotificationShipEnergyLowNoDamage;
                    }
                }
                else
                {
                    goto TR_000C;
                }
                goto TR_0002;
            }
            goto TR_000C;
        TR_0002:
            return true;
        TR_000C:
            return false;
        }

        private static bool EnergyNoWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (!IsEnergyUnderTreshold(0f))
            {
                return false;
            }
            if (MySession.Static.ControlledEntity.Entity is MyCharacter)
            {
                cue = MyGuiSounds.HudVocEnergyNo;
                text = MySpaceTexts.NotificationEnergyNo;
            }
            else
            {
                if (!(MySession.Static.ControlledEntity.Entity is MyCockpit))
                {
                    return false;
                }
                MyCockpit entity = (MyCockpit) MySession.Static.ControlledEntity.Entity;
                bool flag = false;
                List<MyCubeGrid> groupNodes = MyCubeGridGroups.Static.Logical.GetGroupNodes(entity.CubeGrid);
                if ((groupNodes == null) || (groupNodes.Count == 0))
                {
                    return false;
                }
                using (List<MyCubeGrid>.Enumerator enumerator = groupNodes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.NumberOfReactors > 0)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    return false;
                }
                cue = !entity.CubeGrid.IsStatic ? MyGuiSounds.HudVocShipFuelNo : MyGuiSounds.HudVocStationFuelNo;
                text = MySpaceTexts.NotificationFuelNo;
            }
            return true;
        }

        public static void EnqueueSound(MyGuiSounds sound)
        {
            if (MyGuiAudio.HudWarnings)
            {
                if (((m_sound != null) && m_sound.IsPlaying) || ((MySandboxGame.TotalGamePlayTimeInMilliseconds - m_lastSoundPlayed) <= 0x1388))
                {
                    m_soundQueue.Add(sound);
                }
                else
                {
                    m_sound = MyGuiAudio.PlaySound(sound);
                    m_lastSoundPlayed = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
            }
        }

        private static bool FuelCritWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            MyCharacter entity = MySession.Static.ControlledEntity.Entity as MyCharacter;
            if ((entity == null) || (entity.JetpackComp == null))
            {
                return false;
            }
            if (!IsFuelUnderThreshold(0.05f))
            {
                return false;
            }
            cue = MyGuiSounds.HudVocFuelCrit;
            text = MySpaceTexts.NotificationSuitFuelCritical;
            return true;
        }

        private static bool FuelLowWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            MyCharacter entity = MySession.Static.ControlledEntity.Entity as MyCharacter;
            if ((entity == null) || (entity.JetpackComp == null))
            {
                return false;
            }
            if (!IsFuelUnderThreshold(0.1f))
            {
                return false;
            }
            cue = MyGuiSounds.HudVocFuelLow;
            text = MySpaceTexts.NotificationSuitFuelLow;
            return true;
        }

        private static bool HealthCritWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (!HealthWarningMethod(MyCharacterStatComponent.HEALTH_RATIO_CRITICAL))
            {
                return false;
            }
            cue = MyGuiSounds.HudVocHealthCritical;
            text = MySpaceTexts.NotificationHealthCritical;
            return true;
        }

        private static bool HealthLowWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.None;
            text = MySpaceTexts.Blank;
            if (!HealthWarningMethod(MyCharacterStatComponent.HEALTH_RATIO_LOW))
            {
                return false;
            }
            cue = MyGuiSounds.HudVocHealthLow;
            text = MySpaceTexts.NotificationHealthLow;
            return true;
        }

        private static bool HealthWarningMethod(float treshold) => 
            ((MySession.Static.LocalCharacter != null) && ((MySession.Static.LocalCharacter.StatComp != null) && ((MySession.Static.LocalCharacter.StatComp.HealthRatio < treshold) && !MySession.Static.LocalCharacter.IsDead)));

        private static bool IsEnergyUnderTreshold(float treshold)
        {
            if (MySession.Static.CreativeMode || (MySession.Static.ControlledEntity == null))
            {
                return false;
            }
            if ((MySession.Static.ControlledEntity.Entity is MyCharacter) || (MySession.Static.ControlledEntity == null))
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                return ((localCharacter != null) ? ((localCharacter.SuitBattery.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId) <= 0f) ? (((localCharacter.SuitBattery.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId) / 1E-05f) <= treshold) && !localCharacter.IsDead) : false) : false);
            }
            if (!(MySession.Static.ControlledEntity.Entity is MyCockpit))
            {
                return false;
            }
            MyCubeGrid cubeGrid = (MySession.Static.ControlledEntity.Entity as MyCockpit).CubeGrid;
            if ((cubeGrid.GridSystems == null) || (cubeGrid.GridSystems.ResourceDistributor == null))
            {
                return false;
            }
            MyMultipleEnabledEnum enum2 = cubeGrid.GridSystems.ResourceDistributor.SourcesEnabledByType(MyResourceDistributorComponent.ElectricityId);
            return ((MyHud.ShipInfo.FuelRemainingTime <= treshold) && ((enum2 != MyMultipleEnabledEnum.AllDisabled) && (enum2 != MyMultipleEnabledEnum.NoObjects)));
        }

        private static bool IsFuelUnderThreshold(float treshold)
        {
            if (MySession.Static.CreativeMode || (MySession.Static.ControlledEntity == null))
            {
                return false;
            }
            if (!(MySession.Static.ControlledEntity.Entity is MyCharacter))
            {
                return false;
            }
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            return ((localCharacter != null) ? (localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId) < treshold) : false);
        }

        public override void LoadData()
        {
            base.LoadData();
            if (!Game.IsDedicated)
            {
                MyHudWarning item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.HealthLowWarningMethod), 1, 0x493e0, 0, 0x9c4);
                List<MyHudWarning> hudWarnings = new List<MyHudWarning>();
                hudWarnings.Add(item);
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.HealthCritWarningMethod), 0, 0x493e0, 0, 0x1388);
                hudWarnings.Add(item);
                Add(new MyHudWarningGroup(hudWarnings, false));
                hudWarnings.Clear();
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.FuelLowWarningMethod), 1, 0x493e0, 0, 0x9c4);
                hudWarnings.Add(item);
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.FuelCritWarningMethod), 0, 0x493e0, 0, 0x1388);
                hudWarnings.Add(item);
                Add(new MyHudWarningGroup(hudWarnings, false));
                hudWarnings.Clear();
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.EnergyLowWarningMethod), 2, 0x493e0, 0, 0x9c4);
                hudWarnings.Add(item);
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.EnergyCritWarningMethod), 1, 0x493e0, 0, 0x1388);
                hudWarnings.Add(item);
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.EnergyNoWarningMethod), 0, 0x493e0, 0, 0x1388);
                hudWarnings.Add(item);
                Add(new MyHudWarningGroup(hudWarnings, false));
                hudWarnings.Clear();
                item = new MyHudWarning(new MyWarningDetectionMethod(MyHudWarnings.MeteorInboundWarningMethod), 0, 0x927c0, 0, 0x1388);
                hudWarnings.Add(item);
                Add(new MyHudWarningGroup(hudWarnings, false));
            }
        }

        private static bool MeteorInboundWarningMethod(out MyGuiSounds cue, out MyStringId text)
        {
            cue = MyGuiSounds.HudVocMeteorInbound;
            text = MySpaceTexts.NotificationMeteorInbound;
            if ((MyMeteorShower.CurrentTarget == null) || (MySession.Static.ControlledEntity == null))
            {
                return false;
            }
            return (Vector3.Distance((Vector3) MyMeteorShower.CurrentTarget.Value.Center, (Vector3) MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition()) < ((2.0 * MyMeteorShower.CurrentTarget.Value.Radius) + 500.0));
        }

        public static void Remove(MyHudWarningGroup hudWarningGroup)
        {
            m_hudWarnings.Remove(hudWarningGroup);
        }

        public static void RemoveSound(MyGuiSounds cueEnum)
        {
            if (((m_sound != null) && (m_sound.CueEnum == MyGuiAudio.GetCue(cueEnum))) && !m_sound.IsPlaying)
            {
                m_sound.Stop(false);
                m_sound = null;
            }
            m_soundQueue.RemoveAll(cue => cue == cueEnum);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            using (List<MyHudWarningGroup>.Enumerator enumerator = m_hudWarnings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Clear();
                }
            }
            m_hudWarnings.Clear();
            m_soundQueue.Clear();
            if (m_sound != null)
            {
                m_sound.Stop(true);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (Game.IsDedicated)
            {
                m_hudWarnings.Clear();
                m_soundQueue.Clear();
            }
            else
            {
                this.m_updateCounter++;
                if ((this.m_updateCounter % FRAMES_BETWEEN_UPDATE) == 0)
                {
                    using (List<MyHudWarningGroup>.Enumerator enumerator = m_hudWarnings.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Update();
                        }
                    }
                    if ((m_soundQueue.Count > 0) && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - m_lastSoundPlayed) > 0x1388))
                    {
                        m_lastSoundPlayed = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                        m_sound = MyGuiAudio.PlaySound(m_soundQueue[0]);
                        m_soundQueue.RemoveAt(0);
                    }
                }
            }
        }
    }
}


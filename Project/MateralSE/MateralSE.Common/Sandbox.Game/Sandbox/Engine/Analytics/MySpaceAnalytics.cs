namespace Sandbox.Engine.Analytics
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.SessionComponents;
    using VRage.Game.VisualScripting.Missions;
    using VRage.Library.Utils;
    using VRage.Utils;

    public sealed class MySpaceAnalytics : MyAnalyticsHelper
    {
        private static readonly object m_singletonGuard = new object();
        private static MyAnalyticsHelper m_instance = null;
        private int m_lastCampaignProgressionTime;
        private float m_lastMinuteUpdate;
        private bool m_registeredEventsInVisualScripting = true;
        private string m_startedTutorialName = string.Empty;

        private MySpaceAnalytics()
        {
        }

        protected override Dictionary<string, object> GetGameplayEndData()
        {
            int isVanilla;
            Dictionary<string, object> gameplayEndData = base.GetGameplayEndData();
            bool flag = (MyCampaignManager.Static != null) ? (MyCampaignManager.Static.ActiveCampaign != null) : false;
            gameplayEndData["is_campaign_mission"] = flag;
            if (!flag || (MyCampaignManager.Static.ActiveCampaign == null))
            {
                isVanilla = 0;
            }
            else
            {
                isVanilla = (int) MyCampaignManager.Static.ActiveCampaign.IsVanilla;
            }
            "is_official_campaign"[(string) gameplayEndData] = (bool) isVanilla;
            int num = 0;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if ((localCharacter != null) && (localCharacter.Toolbar != null))
            {
                for (int i = 0; i < localCharacter.Toolbar.ItemCount; i++)
                {
                    if (localCharacter.Toolbar.GetItemAtIndex(i) != null)
                    {
                        num++;
                    }
                }
            }
            gameplayEndData["toolbar_used_slots"] = (uint) num;
            gameplayEndData["toolbar_page_switches"] = MySession.Static.ToolbarPageSwitches;
            gameplayEndData["total_blocks_created"] = MySession.Static.TotalBlocksCreated;
            gameplayEndData["total_damage_dealt"] = MySession.Static.TotalDamageDealt;
            gameplayEndData["total_amount_mined"] = MySession.Static.AmountMined;
            gameplayEndData["time_piloting_big_ships"] = (uint) MySession.Static.TimePilotingBigShip.TotalSeconds;
            gameplayEndData["time_piloting_small_ships"] = (uint) MySession.Static.TimePilotingSmallShip.TotalSeconds;
            gameplayEndData["time_on_foot_all"] = (uint) MySession.Static.TimeOnFoot.TotalSeconds;
            gameplayEndData["time_using_jetpack"] = (uint) MySession.Static.TimeOnJetpack.TotalSeconds;
            gameplayEndData["time_on_foot_stations"] = (uint) MySession.Static.TimeOnStation.TotalSeconds;
            gameplayEndData["time_on_foot_ships"] = (uint) MySession.Static.TimeOnShips.TotalSeconds;
            gameplayEndData["time_on_foot_asteroids"] = (uint) MySession.Static.TimeOnAsteroids.TotalSeconds;
            gameplayEndData["time_on_foot_planets"] = (uint) MySession.Static.TimeOnPlanets.TotalSeconds;
            gameplayEndData["time_in_ship_builder_mode"] = (uint) MySession.Static.TimeInBuilderMode.TotalSeconds;
            gameplayEndData["total_blocks_created_from_ship"] = MySession.Static.TotalBlocksCreatedFromShips;
            return gameplayEndData;
        }

        protected override Dictionary<string, object> GetGameplayStartData(MyGameEntryEnum entry, MyObjectBuilder_SessionSettings settings)
        {
            int isVanilla;
            int num2;
            int count;
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            bool flag = (MyCampaignManager.Static != null) ? (MyCampaignManager.Static.ActiveCampaign != null) : false;
            Dictionary<string, object> gameplayStartData = base.GetGameplayStartData(entry, settings);
            gameplayStartData["is_campaign_mission"] = flag;
            Dictionary<string, object> dictionary1 = gameplayStartData;
            if (!flag || (MyCampaignManager.Static.ActiveCampaign == null))
            {
                isVanilla = 0;
            }
            else
            {
                isVanilla = (int) MyCampaignManager.Static.ActiveCampaign.IsVanilla;
            }
            dictionary1[(string) dictionary1] = (bool) isVanilla;
            if ((component == null) || (component.RunningLevelScriptNames == null))
            {
                num2 = 0;
            }
            else
            {
                num2 = component.RunningLevelScriptNames.Count<string>();
            }
            "is_official_campaign"["level_script_count"] = num2;
            if (((component == null) || (component.SMManager == null)) || (component.SMManager.MachineDefinitions == null))
            {
                count = 0;
            }
            else
            {
                count = component.SMManager.MachineDefinitions.Count;
            }
            "is_official_campaign"["state_machine_count"] = count;
            this.m_lastCampaignProgressionTime = 0;
            string local2 = "is_official_campaign";
            local2["voxel_support"] = settings.StationVoxelSupport;
            local2["destructible_blocks"] = settings.DestructibleBlocks;
            local2["destructible_voxels"] = settings.EnableVoxelDestruction;
            local2["jetpack"] = settings.EnableJetpack;
            local2["hostility"] = settings.EnvironmentHostility.ToString();
            local2["drones"] = settings.EnableDrones;
            local2["wolfs"] = settings.EnableWolfs;
            local2["spiders"] = settings.EnableSpiders;
            local2["encounters"] = settings.EnableEncounters;
            local2["oxygen"] = settings.EnableOxygen;
            local2["pressurization"] = settings.EnableOxygenPressurization;
            local2["realistic_sounds"] = settings.RealisticSound;
            local2["tool_shake"] = settings.EnableToolShake;
            local2["multiplier_inventory"] = settings.InventorySizeMultiplier;
            local2["multiplier_welding_speed"] = settings.WelderSpeedMultiplier;
            local2["multiplier_grinding_speed"] = settings.GrinderSpeedMultiplier;
            local2["multiplier_refinery_speed"] = settings.RefinerySpeedMultiplier;
            local2["multiplier_assembler_speed"] = settings.AssemblerSpeedMultiplier;
            local2["multiplier_assembler_efficiency"] = settings.AssemblerEfficiencyMultiplier;
            local2["max_floating_objects"] = settings.MaxFloatingObjects;
            return (Dictionary<string, object>) local2;
        }

        protected override Dictionary<string, object> GetSessionData()
        {
            Dictionary<string, object> sessionData = base.GetSessionData();
            sessionData["audio_hud_warnings"] = MySandboxGame.Config.HudWarnings;
            sessionData["speed_based_ship_sounds"] = MySandboxGame.Config.ShipSoundsAreBasedOnSpeed;
            return sessionData;
        }

        private void obj_CursorStateChanged(MyVSStateMachineNode arg1, MyVSStateMachineNode arg2)
        {
            if (!arg1.PassThrough)
            {
                this.ReportCampaignProgression(arg1.Name);
            }
        }

        protected override void RegisterEventsInVisualScripting()
        {
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            if ((component != null) && (component.SMManager != null))
            {
                component.SMManager.StateMachineStarted += new Action<MyVSStateMachine>(this.SMManager_StateMachineStarted);
                foreach (MyVSStateMachine machine in component.SMManager.RunningMachines)
                {
                    this.SMManager_StateMachineStarted(machine);
                }
            }
        }

        [Conditional("WINDOWS")]
        public void ReportCampaignProgression(string completedState)
        {
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && ((MySession.Static != null) && ((MyCampaignManager.Static != null) && (MyCampaignManager.Static.IsCampaignRunning && (MyCampaignManager.Static.ActiveCampaign != null))))) && MyCampaignManager.Static.ActiveCampaign.IsVanilla)
            {
                try
                {
                    int timeFromLastCompletion = ((int) Math.Round(MySession.Static.ElapsedPlayTime.TotalSeconds)) - this.m_lastCampaignProgressionTime;
                    this.m_lastCampaignProgressionTime = (int) Math.Round(MySession.Static.ElapsedPlayTime.TotalSeconds);
                    MyAnalyticsManager.Instance.ReportCampaignProgression(MyCampaignManager.Static.ActiveCampaignName, MySession.Static.Name, completedState, timeFromLastCompletion);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public void ReportScenarioStart(string scenarioName)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                Dictionary<string, object> eventData = base.CopyDefaultSessionData();
                eventData["name"] = scenarioName;
                ReportEvent(MyAnalyticsProgressionStatus.ScenarioStart, eventData, 0.0);
            }
        }

        public override void ReportToolbarSwitch(int page)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                try
                {
                    MyAnalyticsManager.Instance.ReportToolbarPageSwitch(page);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public void ReportTutorialEnd()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                Dictionary<string, object> eventData = base.CopyDefaultSessionData();
                eventData["name"] = this.m_startedTutorialName;
                ReportEvent(MyAnalyticsProgressionStatus.TutorialEnd, eventData, 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public void ReportTutorialScreen(string initiatedFrom)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                Dictionary<string, object> eventData = base.CopyDefaultSessionData();
                eventData["name"] = this.m_startedTutorialName;
                eventData["source"] = initiatedFrom;
                ReportEvent(MyAnalyticsProgressionStatus.TutorialClick, eventData, 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public void ReportTutorialStart(string tutorialName)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_startedTutorialName = tutorialName;
                Dictionary<string, object> eventData = base.CopyDefaultSessionData();
                eventData["name"] = this.m_startedTutorialName;
                ReportEvent(MyAnalyticsProgressionStatus.TutorialStart, eventData, 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public void ReportTutorialStep(string stepName, int stepNumber)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                Dictionary<string, object> eventData = base.CopyDefaultSessionData();
                eventData["name"] = this.m_startedTutorialName;
                eventData["step_name"] = stepName;
                eventData["step_number"] = stepNumber;
                ReportEvent(MyAnalyticsProgressionStatus.TutorialStep, eventData, 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public void SetEntry(MyGameEntryEnum entry)
        {
            base.m_entry = entry;
        }

        private void SMManager_StateMachineStarted(MyVSStateMachine obj)
        {
            obj.CursorStateChanged += new Action<MyVSStateMachineNode, MyVSStateMachineNode>(this.obj_CursorStateChanged);
        }

        public void Update(MyTimeSpan updateTime)
        {
            if ((updateTime.Seconds - 60.0) >= this.m_lastMinuteUpdate)
            {
                this.m_lastMinuteUpdate = (float) updateTime.Seconds;
                ReportServerStatus();
            }
            if (!this.m_registeredEventsInVisualScripting && (Instance != null))
            {
                this.RegisterEventsInVisualScripting();
                this.m_registeredEventsInVisualScripting = true;
            }
        }

        public static MySpaceAnalytics Instance
        {
            get
            {
                object singletonGuard = m_singletonGuard;
                lock (singletonGuard)
                {
                    if (m_instance != null)
                    {
                        if (!(m_instance is MySpaceAnalytics))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        m_instance = new MySpaceAnalytics();
                    }
                }
                return (m_instance as MySpaceAnalytics);
            }
        }
    }
}


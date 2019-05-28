namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Triggers;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MySessionComponentMissionTriggers : MySessionComponentBase
    {
        protected bool m_someoneWon;
        private int m_updateCount;

        public MySessionComponentMissionTriggers()
        {
            this.MissionTriggers = new Dictionary<MyPlayer.PlayerId, MyMissionTriggers>();
            Static = this;
        }

        public static bool CanRespawn(MyPlayer.PlayerId Id)
        {
            MyMissionTriggers triggers;
            return (!MySession.Static.Settings.ScenarioEditMode ? (Static.MissionTriggers.TryGetValue(Id, out triggers) ? !triggers.Lost : true) : true);
        }

        public MyObjectBuilder_SessionComponentMission GetObjectBuilder()
        {
            MyObjectBuilder_SessionComponentMission mission = new MyObjectBuilder_SessionComponentMission();
            if (this.MissionTriggers != null)
            {
                foreach (KeyValuePair<MyPlayer.PlayerId, MyMissionTriggers> pair in this.MissionTriggers)
                {
                    mission.Triggers.Dictionary.Add(new MyObjectBuilder_SessionComponentMission.pair(pair.Key.SteamId, pair.Key.SerialId), pair.Value.GetObjectBuilder());
                }
            }
            return mission;
        }

        public static StringBuilder GetProgress(MyPlayer player)
        {
            MyMissionTriggers triggers;
            if (!Static.MissionTriggers.TryGetValue(player.Id, out triggers))
            {
                triggers = Static.TryCreateFromDefault(player.Id, false);
            }
            return triggers.GetProgress();
        }

        private bool IsLocal(MyPlayer.PlayerId Id) => 
            (!Sandbox.Engine.Platform.Game.IsDedicated && ((MySession.Static.LocalHumanPlayer != null) && (Id == MySession.Static.LocalHumanPlayer.Id)));

        public void Load(MyObjectBuilder_SessionComponentMission obj)
        {
            this.MissionTriggers.Clear();
            if ((obj != null) && (obj.Triggers != null))
            {
                foreach (KeyValuePair<MyObjectBuilder_SessionComponentMission.pair, MyObjectBuilder_MissionTriggers> pair in obj.Triggers.Dictionary)
                {
                    MyPlayer.PlayerId key = new MyPlayer.PlayerId(pair.Key.stm, pair.Key.ser);
                    MyMissionTriggers triggers = new MyMissionTriggers(pair.Value);
                    this.MissionTriggers.Add(key, triggers);
                }
            }
        }

        public static void PlayerDied(MyPlayer player)
        {
            RaiseSignal(player, Signal.PLAYER_DIED);
        }

        public static void RaiseSignal(MyPlayer player, Signal signal)
        {
            MyMissionTriggers triggers;
            if (!Static.MissionTriggers.TryGetValue(player.Id, out triggers))
            {
                triggers = Static.TryCreateFromDefault(player.Id, false);
            }
            triggers.RaiseSignal(player.Id, signal);
            if (Static.IsLocal(player.Id))
            {
                Static.UpdateLocal(player);
            }
        }

        public void SetLost(MyPlayer.PlayerId Id, int index)
        {
            MyMissionTriggers triggers;
            if (this.MissionTriggers.TryGetValue(Id, out triggers))
            {
                triggers.SetLost(index);
            }
        }

        public void SetWon(MyPlayer.PlayerId Id, int index)
        {
            MyMissionTriggers triggers;
            if (this.MissionTriggers.TryGetValue(Id, out triggers))
            {
                triggers.SetWon(index);
            }
        }

        public MyMissionTriggers TryCreateFromDefault(MyPlayer.PlayerId newId, bool overwrite = false)
        {
            MyMissionTriggers triggers;
            if (overwrite)
            {
                this.MissionTriggers.Remove(newId);
            }
            else if (this.MissionTriggers.TryGetValue(newId, out triggers))
            {
                return triggers;
            }
            MyMissionTriggers triggers2 = new MyMissionTriggers();
            this.MissionTriggers.Add(newId, triggers2);
            this.MissionTriggers.TryGetValue(MyMissionTriggers.DefaultPlayerId, out triggers);
            if (triggers == null)
            {
                triggers = new MyMissionTriggers();
                Static.MissionTriggers.Add(MyMissionTriggers.DefaultPlayerId, triggers);
            }
            triggers2.CopyTriggersFrom(triggers);
            this.m_someoneWon = false;
            return triggers2;
        }

        public bool Update(MyPlayer player, MyEntity entity)
        {
            MyMissionTriggers triggers;
            if (!Sync.IsServer)
            {
                return false;
            }
            if (!this.MissionTriggers.TryGetValue(player.Id, out triggers))
            {
                triggers = this.TryCreateFromDefault(player.Id, false);
            }
            triggers.UpdateWin(player, entity);
            if (!triggers.Won)
            {
                triggers.UpdateLose(player, entity);
            }
            else
            {
                this.m_someoneWon = true;
                MySpaceAnalytics.Instance.ReportTutorialEnd();
            }
            return triggers.Lost;
        }

        public override void UpdateBeforeSimulation()
        {
            if (((MySession.Static.IsScenario || MySession.Static.Settings.ScenarioEditMode) && (MyScenarioSystem.Static != null)) && (MyScenarioSystem.Static.GameState >= MyScenarioSystem.MyState.Running))
            {
                this.m_updateCount++;
                if ((this.m_updateCount % 10) == 0)
                {
                    this.UpdateLocal();
                    if (Sync.IsServer)
                    {
                        int num = 0;
                        int num2 = 0;
                        foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                        {
                            MyEntity entity = null;
                            if (((player.Controller != null) && (player.Controller.ControlledEntity != null)) && (player.Controller.ControlledEntity.Entity != null))
                            {
                                entity = player.Controller.ControlledEntity.Entity;
                            }
                            if (this.Update(player, entity))
                            {
                                num++;
                            }
                            num2++;
                        }
                        if (((num + 1) == num2) && (num > 0))
                        {
                            using (IEnumerator<MyPlayer> enumerator = MySession.Static.Players.GetOnlinePlayers().GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    RaiseSignal(enumerator.Current, Signal.ALL_OTHERS_LOST);
                                }
                            }
                        }
                        if (this.m_someoneWon)
                        {
                            foreach (KeyValuePair<MyPlayer.PlayerId, MyMissionTriggers> pair in this.MissionTriggers)
                            {
                                pair.Value.RaiseSignal(pair.Key, Signal.OTHER_WON);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateLocal()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (MySession.Static.LocalHumanPlayer != null))
            {
                this.UpdateLocal(MySession.Static.LocalHumanPlayer);
            }
        }

        private void UpdateLocal(MyPlayer player)
        {
            MyEntity me = null;
            if (((player.Controller != null) && (player.Controller.ControlledEntity != null)) && (player.Controller.ControlledEntity.Entity != null))
            {
                me = player.Controller.ControlledEntity.Entity;
            }
            this.UpdateLocal(player, me);
        }

        private void UpdateLocal(MyPlayer player, MyEntity me)
        {
            MyMissionTriggers triggers;
            if (!this.MissionTriggers.TryGetValue(player.Id, out triggers))
            {
                triggers = this.TryCreateFromDefault(player.Id, false);
            }
            else
            {
                triggers.DisplayMsg();
                triggers.DisplayHints(player, me);
            }
        }

        public static MySessionComponentMissionTriggers Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public Dictionary<MyPlayer.PlayerId, MyMissionTriggers> MissionTriggers { get; private set; }
    }
}


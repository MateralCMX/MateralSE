namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Library;
    using VRage.Network;

    [StaticEventOwner]
    public class MyMissionTriggers
    {
        public static readonly MyPlayer.PlayerId DefaultPlayerId = new MyPlayer.PlayerId(0L, 0);
        private List<MyTrigger> m_winTriggers;
        private List<MyTrigger> m_loseTriggers;
        private IMyHudNotification m_notification;
        private StringBuilder m_progress;

        public MyMissionTriggers()
        {
            this.m_winTriggers = new List<MyTrigger>();
            this.m_loseTriggers = new List<MyTrigger>();
            this.m_progress = new StringBuilder();
        }

        public MyMissionTriggers(MyObjectBuilder_MissionTriggers builder)
        {
            this.m_winTriggers = new List<MyTrigger>();
            this.m_loseTriggers = new List<MyTrigger>();
            this.m_progress = new StringBuilder();
            this.Init(builder);
        }

        public void CopyTriggersFrom(MyMissionTriggers source)
        {
            List<MyTrigger>.Enumerator enumerator;
            this.m_winTriggers.Clear();
            using (enumerator = source.m_winTriggers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyTrigger item = (MyTrigger) enumerator.Current.Clone();
                    item.IsTrue = false;
                    this.m_winTriggers.Add(item);
                }
            }
            this.m_loseTriggers.Clear();
            using (enumerator = source.m_loseTriggers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyTrigger item = (MyTrigger) enumerator.Current.Clone();
                    item.IsTrue = false;
                    this.m_loseTriggers.Add(item);
                }
            }
            this.Won = false;
            this.Lost = false;
            this.Message = null;
            this.HideNotification();
        }

        public void DisplayHints(MyPlayer player, MyEntity me)
        {
            for (int i = 0; i < this.m_winTriggers.Count; i++)
            {
                this.m_winTriggers[i].DisplayHints(player, me);
            }
            for (int j = 0; j < this.m_loseTriggers.Count; j++)
            {
                this.m_loseTriggers[j].DisplayHints(player, me);
            }
        }

        public bool DisplayMsg()
        {
            if ((this.Message == null) || (this.m_notification != null))
            {
                return false;
            }
            this.m_notification = MyAPIGateway.Utilities.CreateNotification(this.Message, 0, this.IsMsgWinning ? "Green" : "Red");
            this.m_notification.Show();
            return true;
        }

        protected void DoEnd()
        {
            if (!MySession.Static.Settings.ScenarioEditMode)
            {
                Sync.Players.RespawnComponent.CloseRespawnScreen();
            }
            MyScenarioSystem.Static.GameState = MyScenarioSystem.MyState.Ending;
        }

        public virtual MyObjectBuilder_MissionTriggers GetObjectBuilder()
        {
            MyObjectBuilder_MissionTriggers triggers = new MyObjectBuilder_MissionTriggers();
            foreach (MyTrigger trigger in this.m_winTriggers)
            {
                triggers.WinTriggers.Add(trigger.GetObjectBuilder());
            }
            foreach (MyTrigger trigger2 in this.m_loseTriggers)
            {
                triggers.LoseTriggers.Add(trigger2.GetObjectBuilder());
            }
            triggers.message = this.Message;
            triggers.Won = this.Won;
            triggers.Lost = this.Lost;
            return triggers;
        }

        public StringBuilder GetProgress()
        {
            StringBuilder progress;
            this.m_progress.Clear().Append(MyTexts.Get(MySpaceTexts.ScenarioProgressWinConditions)).Append(MyEnvironment.NewLine);
            for (int i = 0; i < this.m_winTriggers.Count; i++)
            {
                progress = this.m_winTriggers[i].GetProgress();
                if (progress != null)
                {
                    this.m_progress.Append(progress).Append(MyEnvironment.NewLine);
                }
            }
            this.m_progress.Append(MyEnvironment.NewLine).Append(MyTexts.Get(MySpaceTexts.ScenarioProgressLoseConditions)).Append(MyEnvironment.NewLine);
            for (int j = 0; j < this.m_loseTriggers.Count; j++)
            {
                progress = this.m_loseTriggers[j].GetProgress();
                if (progress != null)
                {
                    this.m_progress.Append(progress).Append(MyEnvironment.NewLine);
                }
            }
            return this.m_progress;
        }

        public void HideNotification()
        {
            if (this.m_notification != null)
            {
                this.m_notification.Hide();
                this.m_notification = null;
            }
        }

        public void Init(MyObjectBuilder_MissionTriggers builder)
        {
            foreach (MyObjectBuilder_Trigger trigger in builder.WinTriggers)
            {
                this.m_winTriggers.Add(TriggerFactory.CreateInstance(trigger));
            }
            foreach (MyObjectBuilder_Trigger trigger2 in builder.LoseTriggers)
            {
                this.m_loseTriggers.Add(TriggerFactory.CreateInstance(trigger2));
            }
            this.Message = builder.message;
            this.Won = builder.Won;
            this.Lost = builder.Lost;
            if (this.Won)
            {
                this.IsMsgWinning = true;
            }
        }

        [Event(null, 0x119), Reliable, Broadcast]
        private static void OnPlayerLost(MyPlayer.PlayerId id, int triggerIndex)
        {
            MySessionComponentMissionTriggers.Static.SetLost(id, triggerIndex);
        }

        [Event(null, 0x113), Reliable, Broadcast]
        private static void OnPlayerWon(MyPlayer.PlayerId id, int triggerIndex)
        {
            MySessionComponentMissionTriggers.Static.SetWon(id, triggerIndex);
        }

        public bool RaiseSignal(MyPlayer.PlayerId Id, Signal signal)
        {
            if (this.Won || this.Lost)
            {
                return true;
            }
            if ((signal - 1) <= Signal.ALL_OTHERS_LOST)
            {
                int triggerIndex = 0;
                while (true)
                {
                    if (triggerIndex >= this.m_winTriggers.Count)
                    {
                        for (int i = 0; i < this.m_loseTriggers.Count; i++)
                        {
                            MyTrigger trigger2 = this.m_loseTriggers[i];
                            if (trigger2.IsTrue || trigger2.RaiseSignal(signal))
                            {
                                SetPlayerLost(Id, i);
                                return true;
                            }
                        }
                        break;
                    }
                    MyTrigger trigger = this.m_winTriggers[triggerIndex];
                    if (trigger.IsTrue || trigger.RaiseSignal(signal))
                    {
                        SetPlayerWon(Id, triggerIndex);
                        return true;
                    }
                    triggerIndex++;
                }
            }
            return false;
        }

        public void SetLost(int triggerIndex)
        {
            this.Lost = true;
            this.m_loseTriggers[triggerIndex].SetTrue();
            if (this.Message == null)
            {
                this.Message = this.m_loseTriggers[triggerIndex].Message;
                this.IsMsgWinning = false;
            }
            this.DoEnd();
        }

        private static void SetPlayerLost(MyPlayer.PlayerId id, int triggerIndex)
        {
            MySessionComponentMissionTriggers.Static.SetLost(id, triggerIndex);
            if (Sync.MultiplayerActive && MySession.Static.IsScenario)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<MyPlayer.PlayerId, int>(s => new Action<MyPlayer.PlayerId, int>(MyMissionTriggers.OnPlayerLost), id, triggerIndex, targetEndpoint, position);
            }
        }

        private static void SetPlayerWon(MyPlayer.PlayerId id, int triggerIndex)
        {
            MySessionComponentMissionTriggers.Static.SetWon(id, triggerIndex);
            if (Sync.MultiplayerActive && MySession.Static.IsScenario)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<MyPlayer.PlayerId, int>(s => new Action<MyPlayer.PlayerId, int>(MyMissionTriggers.OnPlayerWon), id, triggerIndex, targetEndpoint, position);
            }
        }

        public void SetWon(int triggerIndex)
        {
            this.Won = true;
            this.m_winTriggers[triggerIndex].SetTrue();
            if (this.Message == null)
            {
                this.Message = this.m_winTriggers[triggerIndex].Message;
                this.IsMsgWinning = true;
            }
            this.DoEnd();
        }

        public bool UpdateLose(MyPlayer player, MyEntity me)
        {
            if (this.Won || this.Lost)
            {
                return true;
            }
            for (int i = 0; i < this.m_loseTriggers.Count; i++)
            {
                MyTrigger trigger = this.m_loseTriggers[i];
                if (trigger.IsTrue || trigger.Update(player, me))
                {
                    SetPlayerLost(player.Id, i);
                    return true;
                }
            }
            return false;
        }

        public bool UpdateWin(MyPlayer player, MyEntity me)
        {
            if (this.Won || this.Lost)
            {
                return true;
            }
            for (int i = 0; i < this.m_winTriggers.Count; i++)
            {
                MyTrigger trigger = this.m_winTriggers[i];
                if (trigger.IsTrue || trigger.Update(player, me))
                {
                    SetPlayerWon(player.Id, i);
                    return true;
                }
            }
            return false;
        }

        public bool Won { get; protected set; }

        public bool Lost { get; protected set; }

        public string Message { get; protected set; }

        public bool IsMsgWinning { get; protected set; }

        public List<MyTrigger> WinTriggers =>
            this.m_winTriggers;

        public List<MyTrigger> LoseTriggers =>
            this.m_loseTriggers;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMissionTriggers.<>c <>9 = new MyMissionTriggers.<>c();
            public static Func<IMyEventOwner, Action<MyPlayer.PlayerId, int>> <>9__40_0;
            public static Func<IMyEventOwner, Action<MyPlayer.PlayerId, int>> <>9__41_0;

            internal Action<MyPlayer.PlayerId, int> <SetPlayerLost>b__41_0(IMyEventOwner s) => 
                new Action<MyPlayer.PlayerId, int>(MyMissionTriggers.OnPlayerLost);

            internal Action<MyPlayer.PlayerId, int> <SetPlayerWon>b__40_0(IMyEventOwner s) => 
                new Action<MyPlayer.PlayerId, int>(MyMissionTriggers.OnPlayerWon);
        }
    }
}


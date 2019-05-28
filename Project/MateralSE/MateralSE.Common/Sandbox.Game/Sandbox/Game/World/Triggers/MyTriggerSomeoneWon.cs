namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Triggers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library;
    using VRage.Utils;

    [TriggerType(typeof(MyObjectBuilder_TriggerSomeoneWon))]
    public class MyTriggerSomeoneWon : MyTrigger, ICloneable
    {
        private StringBuilder m_progress;

        public MyTriggerSomeoneWon()
        {
            this.m_progress = new StringBuilder();
        }

        public MyTriggerSomeoneWon(MyTriggerSomeoneWon trg) : base(trg)
        {
            this.m_progress = new StringBuilder();
        }

        public override object Clone() => 
            new MyTriggerSomeoneWon(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerSomeoneWon(this));
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionSomeoneWon;

        public override StringBuilder GetProgress()
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if (onlinePlayers.Count == 1)
            {
                return null;
            }
            this.m_progress.Clear().Append(MyTexts.Get(MySpaceTexts.ScenarioProgressSomeoneWon));
            foreach (MyPlayer player in onlinePlayers)
            {
                MyMissionTriggers triggers;
                if (ReferenceEquals(player, MySession.Static.LocalHumanPlayer))
                {
                    continue;
                }
                if (MySessionComponentMissionTriggers.Static.MissionTriggers.TryGetValue(player.Id, out triggers) && (!triggers.Lost && !triggers.Won))
                {
                    this.m_progress.Append(MyEnvironment.NewLine).Append("   ").Append(player.DisplayName);
                }
            }
            return this.m_progress;
        }

        public override bool RaiseSignal(Signal signal)
        {
            if (signal == Signal.OTHER_WON)
            {
                base.m_IsTrue = true;
            }
            return this.IsTrue;
        }
    }
}


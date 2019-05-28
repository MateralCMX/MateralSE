namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Triggers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;

    [TriggerType(typeof(MyObjectBuilder_TriggerTimeLimit))]
    internal class MyTriggerTimeLimit : MyTrigger, ICloneable
    {
        private int m_limitInMinutes;
        private TimeSpan m_limit;
        private int m_lastSeconds;
        private StringBuilder m_progress;

        public MyTriggerTimeLimit()
        {
            this.m_limitInMinutes = 30;
            this.m_limit = new TimeSpan(0, 30, 0);
            this.m_progress = new StringBuilder();
        }

        public MyTriggerTimeLimit(MyTriggerTimeLimit trg) : base(trg)
        {
            this.m_limitInMinutes = 30;
            this.m_limit = new TimeSpan(0, 30, 0);
            this.m_progress = new StringBuilder();
            this.LimitInMinutes = trg.LimitInMinutes;
        }

        public override object Clone() => 
            new MyTriggerTimeLimit(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerTimeLimit(this));
        }

        public override void DisplayHints(MyPlayer player, MyEntity me)
        {
            if (MySession.Static.IsScenario && (MyScenarioSystem.Static.ServerStartGameTime != DateTime.MaxValue))
            {
                TimeSpan span = this.m_limit - (DateTime.UtcNow - MyScenarioSystem.Static.ServerStartGameTime);
                int seconds = span.Seconds;
                if (this.m_lastSeconds != seconds)
                {
                    this.m_lastSeconds = seconds;
                    MyHud.ScenarioInfo.TimeLeftMin = (int) span.TotalMinutes;
                    MyHud.ScenarioInfo.TimeLeftSec = seconds;
                }
            }
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionTimeLimit;

        public override MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_TriggerTimeLimit objectBuilder = (MyObjectBuilder_TriggerTimeLimit) base.GetObjectBuilder();
            objectBuilder.Limit = this.LimitInMinutes;
            return objectBuilder;
        }

        public override StringBuilder GetProgress()
        {
            this.m_progress.Clear().AppendFormat(MyTexts.GetString(MySpaceTexts.ScenarioProgressTimeLimit), this.LimitInMinutes);
            return this.m_progress;
        }

        public override void Init(MyObjectBuilder_Trigger ob)
        {
            base.Init(ob);
            this.LimitInMinutes = ((MyObjectBuilder_TriggerTimeLimit) ob).Limit;
        }

        public override bool Update(MyPlayer player, MyEntity me)
        {
            if (MySession.Static.IsScenario && (this.m_limit <= (DateTime.UtcNow - MyScenarioSystem.Static.ServerStartGameTime)))
            {
                base.m_IsTrue = true;
            }
            return this.IsTrue;
        }

        public int LimitInMinutes
        {
            get => 
                this.m_limitInMinutes;
            set
            {
                this.m_limitInMinutes = value;
                this.m_limit = new TimeSpan(0, value, 0);
            }
        }
    }
}


namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Triggers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;

    [TriggerType(typeof(MyObjectBuilder_TriggerNoSpawn))]
    internal class MyTriggerNoSpawn : MyTrigger, ICloneable
    {
        private bool m_isDead;
        private bool m_couldRespawn;
        private DateTime m_begin;
        private int m_limitInSeconds;
        private TimeSpan m_limit;
        private int m_lastSeconds;
        private StringBuilder m_guiText;
        private StringBuilder m_progress;

        public MyTriggerNoSpawn()
        {
            this.m_couldRespawn = true;
            this.m_limitInSeconds = 60;
            this.m_limit = new TimeSpan(0, 1, 0);
            this.m_guiText = new StringBuilder();
            this.m_progress = new StringBuilder();
        }

        public MyTriggerNoSpawn(MyTriggerNoSpawn trg) : base(trg)
        {
            this.m_couldRespawn = true;
            this.m_limitInSeconds = 60;
            this.m_limit = new TimeSpan(0, 1, 0);
            this.m_guiText = new StringBuilder();
            this.m_progress = new StringBuilder();
            this.LimitInSeconds = trg.LimitInSeconds;
        }

        public override object Clone() => 
            new MyTriggerNoSpawn(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerNoSpawn(this));
        }

        public override void DisplayHints(MyPlayer player, MyEntity me)
        {
            if (MySession.Static.IsScenario)
            {
                if (base.m_IsTrue)
                {
                    this.m_begin = DateTime.MinValue;
                }
                else
                {
                    if (!Sync.Players.RespawnComponent.IsInRespawnScreen())
                    {
                        this.m_begin = DateTime.MinValue;
                    }
                    else if (this.m_begin == DateTime.MinValue)
                    {
                        this.m_begin = DateTime.UtcNow;
                    }
                    if (this.m_begin != DateTime.MinValue)
                    {
                        TimeSpan span = this.m_limit - (DateTime.UtcNow - this.m_begin);
                        int seconds = span.Seconds;
                        if (this.m_lastSeconds != seconds)
                        {
                            this.m_lastSeconds = seconds;
                            this.m_guiText.Clear().AppendFormat(MyTexts.GetString(MySpaceTexts.ScreenMedicals_NoRespawnPlace), (int) span.TotalMinutes, seconds);
                            Sync.Players.RespawnComponent.SetNoRespawnText(this.m_guiText, (int) span.TotalSeconds);
                        }
                    }
                }
            }
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionNoSpawn;

        public override MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_TriggerNoSpawn objectBuilder = (MyObjectBuilder_TriggerNoSpawn) base.GetObjectBuilder();
            objectBuilder.Limit = this.LimitInSeconds;
            return objectBuilder;
        }

        public override StringBuilder GetProgress()
        {
            this.m_progress.Clear().AppendFormat(MySpaceTexts.ScenarioProgressNoSpawn, this.LimitInSeconds);
            return this.m_progress;
        }

        public override void Init(MyObjectBuilder_Trigger ob)
        {
            base.Init(ob);
            this.LimitInSeconds = ((MyObjectBuilder_TriggerNoSpawn) ob).Limit;
        }

        public override bool Update(MyPlayer player, MyEntity me)
        {
            if (!player.Identity.IsDead)
            {
                this.m_begin = DateTime.MinValue;
            }
            else
            {
                if (this.m_begin == DateTime.MinValue)
                {
                    this.m_begin = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - this.m_begin) > this.m_limit)
                {
                    base.m_IsTrue = true;
                }
            }
            return this.IsTrue;
        }

        public int LimitInSeconds
        {
            get => 
                this.m_limitInSeconds;
            set
            {
                this.m_limitInSeconds = value;
                this.m_limit = new TimeSpan(0, 0, value);
            }
        }
    }
}


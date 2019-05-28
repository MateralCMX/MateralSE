namespace Sandbox.Game.World.Triggers
{
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
    using VRage.Library;
    using VRage.Utils;

    [TriggerType(typeof(MyObjectBuilder_TriggerLives))]
    internal class MyTriggerLives : MyTrigger, ICloneable
    {
        public int LivesLeft;
        private StringBuilder m_progress;

        public MyTriggerLives()
        {
            this.LivesLeft = 1;
            this.m_progress = new StringBuilder();
        }

        public MyTriggerLives(MyTriggerLives trg) : base(trg)
        {
            this.LivesLeft = 1;
            this.m_progress = new StringBuilder();
            this.LivesLeft = trg.LivesLeft;
        }

        public override object Clone() => 
            new MyTriggerLives(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerLives(this));
        }

        public override void DisplayHints(MyPlayer player, MyEntity me)
        {
            if (MySession.Static.IsScenario)
            {
                MyHud.ScenarioInfo.LivesLeft = this.LivesLeft;
            }
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionLives;

        public override MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_TriggerLives objectBuilder = (MyObjectBuilder_TriggerLives) base.GetObjectBuilder();
            objectBuilder.Lives = this.LivesLeft;
            return objectBuilder;
        }

        public override StringBuilder GetProgress()
        {
            this.m_progress.Clear().AppendFormat(MySpaceTexts.ScenarioProgressLimitedLives, this.LivesLeft).Append(MyEnvironment.NewLine);
            return this.m_progress;
        }

        public override void Init(MyObjectBuilder_Trigger ob)
        {
            base.Init(ob);
            this.LivesLeft = ((MyObjectBuilder_TriggerLives) ob).Lives;
        }

        public override bool RaiseSignal(Signal signal)
        {
            if (signal == Signal.PLAYER_DIED)
            {
                this.LivesLeft--;
                if (this.LivesLeft <= 0)
                {
                    base.m_IsTrue = true;
                }
            }
            return this.IsTrue;
        }

        public override bool IsTrue
        {
            get => 
                base.m_IsTrue;
            set
            {
                base.m_IsTrue = value;
                if (value)
                {
                    this.LivesLeft = 0;
                }
            }
        }
    }
}


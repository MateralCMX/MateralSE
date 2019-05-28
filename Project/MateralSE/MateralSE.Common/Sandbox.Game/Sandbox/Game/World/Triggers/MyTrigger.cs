namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;

    public class MyTrigger : ICloneable
    {
        protected bool m_IsTrue;
        public string Message;
        public string WwwLink;
        public string NextMission;

        public MyTrigger()
        {
        }

        public MyTrigger(MyTrigger trg)
        {
            this.m_IsTrue = trg.m_IsTrue;
            if (trg.Message != null)
            {
                this.Message = string.Copy(trg.Message);
            }
            if (trg.WwwLink != null)
            {
                this.WwwLink = string.Copy(trg.WwwLink);
            }
            if (trg.NextMission != null)
            {
                this.NextMission = string.Copy(trg.NextMission);
            }
        }

        public virtual object Clone() => 
            new MyTrigger(this);

        public virtual void DisplayGUI()
        {
        }

        public virtual void DisplayHints(MyPlayer player, MyEntity me)
        {
        }

        public static MyStringId GetCaption() => 
            MyCommonTexts.MessageBoxCaptionError;

        public virtual MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_Trigger trigger1 = TriggerFactory.CreateObjectBuilder(this);
            trigger1.IsTrue = this.m_IsTrue;
            trigger1.Message = this.Message;
            trigger1.WwwLink = this.WwwLink;
            trigger1.NextMission = this.NextMission;
            return trigger1;
        }

        public virtual StringBuilder GetProgress() => 
            null;

        public virtual void Init(MyObjectBuilder_Trigger ob)
        {
            this.m_IsTrue = ob.IsTrue;
            this.Message = ob.Message;
            this.WwwLink = ob.WwwLink;
            this.NextMission = ob.NextMission;
        }

        public virtual bool RaiseSignal(Signal signal) => 
            this.IsTrue;

        public void SetTrue()
        {
            this.IsTrue = true;
            if ((this.WwwLink != null) && (this.WwwLink.Length > 0))
            {
                MyGuiSandbox.OpenUrlWithFallback(this.WwwLink, "Scenario info", true);
            }
            if (((this.NextMission != null) && (this.NextMission.Length > 0)) && MySession.Static.IsScenario)
            {
                MyScenarioSystem.LoadNextScenario(this.NextMission);
            }
        }

        public virtual bool Update(MyPlayer player, MyEntity me) => 
            this.IsTrue;

        public virtual bool IsTrue
        {
            get => 
                this.m_IsTrue;
            set => 
                (this.m_IsTrue = value);
        }
    }
}


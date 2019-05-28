namespace Sandbox.Game.World.Triggers
{
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
    using VRageMath;

    [TriggerType(typeof(MyObjectBuilder_TriggerPositionLeft))]
    public class MyTriggerPositionLeft : MyTrigger, ICloneable
    {
        public Vector3D TargetPos;
        protected double m_maxDistance2;
        private StringBuilder m_progress;

        public MyTriggerPositionLeft()
        {
            this.TargetPos = new Vector3D(0.0, 0.0, 0.0);
            this.m_maxDistance2 = 10000.0;
            this.m_progress = new StringBuilder();
        }

        public MyTriggerPositionLeft(MyTriggerPositionLeft pos) : base(pos)
        {
            this.TargetPos = new Vector3D(0.0, 0.0, 0.0);
            this.m_maxDistance2 = 10000.0;
            this.m_progress = new StringBuilder();
            this.TargetPos = pos.TargetPos;
            this.m_maxDistance2 = pos.m_maxDistance2;
        }

        public override object Clone() => 
            new MyTriggerPositionLeft(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerPositionLeft(this));
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionPositionLeft;

        public override MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_TriggerPositionLeft objectBuilder = (MyObjectBuilder_TriggerPositionLeft) base.GetObjectBuilder();
            objectBuilder.Pos = this.TargetPos;
            objectBuilder.Distance2 = this.m_maxDistance2;
            return objectBuilder;
        }

        public override StringBuilder GetProgress()
        {
            object[] args = new object[] { this.TargetPos.X, this.TargetPos.Y, this.TargetPos.Z, Math.Sqrt(this.m_maxDistance2) };
            this.m_progress.Clear().AppendFormat(MyTexts.GetString(MySpaceTexts.ScenarioProgressPositionLeft), args);
            return this.m_progress;
        }

        public override void Init(MyObjectBuilder_Trigger ob)
        {
            base.Init(ob);
            this.TargetPos = ((MyObjectBuilder_TriggerPositionLeft) ob).Pos;
            this.m_maxDistance2 = ((MyObjectBuilder_TriggerPositionLeft) ob).Distance2;
        }

        public override bool Update(MyPlayer player, MyEntity me)
        {
            if ((me != null) && (Vector3D.DistanceSquared(me.PositionComp.GetPosition(), this.TargetPos) > this.m_maxDistance2))
            {
                base.m_IsTrue = true;
            }
            return this.IsTrue;
        }

        public double Radius
        {
            get => 
                Math.Sqrt(this.m_maxDistance2);
            set => 
                (this.m_maxDistance2 = value * value);
        }
    }
}


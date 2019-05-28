namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Definitions;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyEventType(typeof(MyObjectBuilder_GlobalEventBase), true), MyEventType(typeof(MyObjectBuilder_GlobalEventDefinition), false)]
    public class MyGlobalEventBase : IComparable
    {
        public int CompareTo(object obj)
        {
            if (!(obj is MyGlobalEventBase))
            {
                return 0;
            }
            TimeSpan span = this.ActivationTime - (obj as MyGlobalEventBase).ActivationTime;
            return ((span.Ticks != 0) ? ((span.Ticks < 0L) ? -1 : 1) : (RuntimeHelpers.GetHashCode(this) - RuntimeHelpers.GetHashCode(obj)));
        }

        public virtual MyObjectBuilder_GlobalEventBase GetObjectBuilder()
        {
            MyObjectBuilder_GlobalEventBase base1 = MyObjectBuilderSerializer.CreateNewObject(this.Definition.Id.TypeId, this.Definition.Id.SubtypeName) as MyObjectBuilder_GlobalEventBase;
            base1.ActivationTimeMs = this.ActivationTime.Ticks / 0x2710L;
            base1.Enabled = this.Enabled;
            return base1;
        }

        public virtual void Init(MyObjectBuilder_GlobalEventBase ob)
        {
            this.Definition = MyDefinitionManager.Static.GetEventDefinition(ob.GetId());
            this.Action = MyGlobalEventFactory.GetEventHandler(ob.GetId());
            this.ActivationTime = TimeSpan.FromMilliseconds((double) ob.ActivationTimeMs);
            this.Enabled = ob.Enabled;
            this.RemoveAfterHandlerExit = false;
        }

        public virtual void InitFromDefinition(MyGlobalEventDefinition definition)
        {
            this.Definition = definition;
            this.Action = MyGlobalEventFactory.GetEventHandler(this.Definition.Id);
            if (this.Definition.FirstActivationTime != null)
            {
                this.ActivationTime = this.Definition.FirstActivationTime.Value;
            }
            else
            {
                this.RecalculateActivationTime();
            }
            this.Enabled = true;
            this.RemoveAfterHandlerExit = false;
        }

        public void RecalculateActivationTime()
        {
            TimeSpan? minActivationTime = this.Definition.MinActivationTime;
            TimeSpan? maxActivationTime = this.Definition.MaxActivationTime;
            this.ActivationTime = !(((minActivationTime != null) == (maxActivationTime != null)) ? ((minActivationTime != null) ? (minActivationTime.GetValueOrDefault() == maxActivationTime.GetValueOrDefault()) : true) : false) ? MyUtils.GetRandomTimeSpan(this.Definition.MinActivationTime.Value, this.Definition.MaxActivationTime.Value) : this.Definition.MinActivationTime.Value;
            MySandboxGame.Log.WriteLine("MyGlobalEvent.RecalculateActivationTime:");
            MySandboxGame.Log.WriteLine("Next activation in " + this.ActivationTime.ToString());
        }

        public void SetActivationTime(TimeSpan time)
        {
            this.ActivationTime = time;
        }

        public bool IsOneTime =>
            (this.Definition.MinActivationTime == null);

        public bool IsPeriodic =>
            !this.IsOneTime;

        public bool IsInPast =>
            (this.ActivationTime.Ticks <= 0L);

        public bool IsInFuture =>
            (this.ActivationTime.Ticks > 0L);

        public bool IsHandlerValid =>
            (this.Action != null);

        public MyGlobalEventDefinition Definition { get; private set; }

        public MethodInfo Action { get; private set; }

        public TimeSpan ActivationTime { get; private set; }

        public bool Enabled { get; set; }

        public bool RemoveAfterHandlerExit { get; set; }
    }
}


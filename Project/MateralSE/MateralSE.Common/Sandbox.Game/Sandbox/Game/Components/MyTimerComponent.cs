namespace Sandbox.Game.Components
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Systems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyComponentType(typeof(MyTimerComponent)), MyComponentBuilder(typeof(MyObjectBuilder_TimerComponent), true)]
    public class MyTimerComponent : MyEntityComponentBase
    {
        public bool Repeat;
        public float TimeToEvent;
        public Action<MyEntityComponentContainer> EventToTrigger;
        private float m_setTimeMin;
        private float m_originTimeMin;
        public bool TimerEnabled = true;
        public bool RemoveEntityOnTimer;
        private bool m_resetOrigin;

        public void ClearEvent()
        {
            this.EventToTrigger = null;
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase baseBuilder)
        {
            MyObjectBuilder_TimerComponent component = baseBuilder as MyObjectBuilder_TimerComponent;
            this.Repeat = component.Repeat;
            this.TimeToEvent = component.TimeToEvent;
            this.m_setTimeMin = component.SetTimeMinutes;
            this.TimerEnabled = component.TimerEnabled;
            this.RemoveEntityOnTimer = component.RemoveEntityOnTimer;
            if (this.RemoveEntityOnTimer && Sync.IsServer)
            {
                this.EventToTrigger = GetRemoveEntityOnTimerEvent();
            }
        }

        private static Action<MyEntityComponentContainer> GetRemoveEntityOnTimerEvent() => 
            delegate (MyEntityComponentContainer container) {
                if (!container.Entity.MarkedForClose)
                {
                    container.Entity.Close();
                }
            };

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyTimerComponentDefinition definition2 = definition as MyTimerComponentDefinition;
            if (definition2 != null)
            {
                this.TimerEnabled = definition2.TimeToRemoveMin > 0f;
                this.m_setTimeMin = definition2.TimeToRemoveMin;
                this.TimeToEvent = this.m_setTimeMin;
                this.RemoveEntityOnTimer = definition2.TimeToRemoveMin > 0f;
                if (this.RemoveEntityOnTimer && Sync.IsServer)
                {
                    this.EventToTrigger = GetRemoveEntityOnTimerEvent();
                }
            }
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (this.TimerEnabled)
            {
                this.m_resetOrigin = true;
            }
            MyTimerComponentSystem.Static.Register(this);
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (MyTimerComponentSystem.Static != null)
            {
                MyTimerComponentSystem.Static.Unregister(this);
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_TimerComponent component1 = MyComponentFactory.CreateObjectBuilder(this) as MyObjectBuilder_TimerComponent;
            component1.Repeat = this.Repeat;
            component1.TimeToEvent = this.TimeToEvent;
            component1.SetTimeMinutes = this.m_setTimeMin;
            component1.TimerEnabled = this.TimerEnabled;
            component1.RemoveEntityOnTimer = this.RemoveEntityOnTimer;
            return component1;
        }

        public void SetRemoveEntityTimer(float timeMin)
        {
            this.RemoveEntityOnTimer = true;
            this.SetTimer(timeMin, GetRemoveEntityOnTimerEvent(), true, false);
        }

        public void SetTimer(float timeMin, Action<MyEntityComponentContainer> triggerEvent, bool start = true, bool repeat = false)
        {
            this.TimeToEvent = -1f;
            this.m_setTimeMin = timeMin;
            this.Repeat = repeat;
            this.EventToTrigger = triggerEvent;
            this.TimerEnabled = false;
            if (start)
            {
                this.StartTiming();
            }
        }

        private void StartTiming()
        {
            this.TimeToEvent = this.m_setTimeMin;
            this.TimerEnabled = true;
            this.m_originTimeMin = (float) MySession.Static.ElapsedGameTime.TotalMinutes;
        }

        public void Update()
        {
            if (this.TimerEnabled)
            {
                float totalMinutes = (float) MySession.Static.ElapsedGameTime.TotalMinutes;
                if (this.m_resetOrigin)
                {
                    this.m_originTimeMin = (totalMinutes - this.m_setTimeMin) + this.TimeToEvent;
                    this.m_resetOrigin = false;
                }
                this.TimeToEvent = (this.m_originTimeMin + this.m_setTimeMin) - totalMinutes;
                if (this.TimeToEvent <= 0f)
                {
                    if (this.EventToTrigger != null)
                    {
                        this.EventToTrigger(base.Container);
                    }
                    if (this.Repeat)
                    {
                        this.m_originTimeMin = (float) MySession.Static.ElapsedGameTime.TotalMinutes;
                    }
                    else
                    {
                        this.TimerEnabled = false;
                    }
                }
            }
        }

        public override string ComponentTypeDebugString =>
            "Timer";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTimerComponent.<>c <>9 = new MyTimerComponent.<>c();
            public static Action<MyEntityComponentContainer> <>9__21_0;

            internal void <GetRemoveEntityOnTimerEvent>b__21_0(MyEntityComponentContainer container)
            {
                if (!container.Entity.MarkedForClose)
                {
                    container.Entity.Close();
                }
            }
        }
    }
}


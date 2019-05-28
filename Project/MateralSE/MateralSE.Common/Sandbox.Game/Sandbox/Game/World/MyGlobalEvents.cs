namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyGlobalEvents : MySessionComponentBase
    {
        private static SortedSet<MyGlobalEventBase> m_globalEvents = new SortedSet<MyGlobalEventBase>();
        private int m_elapsedTimeInMilliseconds;
        private int m_previousTime;
        private static readonly int GLOBAL_EVENT_UPDATE_RATIO_IN_MS = 0x7d0;
        private static Predicate<MyGlobalEventBase> m_removalPredicate = new Predicate<MyGlobalEventBase>(MyGlobalEvents.RemovalPredicate);
        private static MyDefinitionId m_defIdToRemove;

        public static void AddGlobalEvent(MyGlobalEventBase globalEvent)
        {
            m_globalEvents.Add(globalEvent);
        }

        private void AddGlobalEventToEventLog(MyGlobalEventBase globalEvent)
        {
            MySandboxGame.Log.WriteLine("MyGlobalEvents.StartGlobalEvent: " + globalEvent.Definition.Id.ToString());
        }

        public override void BeforeStart()
        {
            this.m_previousTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        internal static void DisableEvents()
        {
            using (SortedSet<MyGlobalEventBase>.Enumerator enumerator = m_globalEvents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = false;
                }
            }
        }

        public override void Draw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_EVENTS)
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 500f), "Upcoming events:", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                StringBuilder builder = new StringBuilder();
                float y = 530f;
                foreach (MyGlobalEventBase base2 in m_globalEvents)
                {
                    int totalHours = (int) base2.ActivationTime.TotalHours;
                    int minutes = base2.ActivationTime.Minutes;
                    TimeSpan activationTime = base2.ActivationTime;
                    int seconds = activationTime.Seconds;
                    builder.Clear();
                    builder.AppendFormat("{0}:{1:D2}:{2:D2}", totalHours, minutes, seconds);
                    builder.AppendFormat(" {0}: {1}", base2.Enabled ? "ENABLED" : "--OFF--", base2.Definition.DisplayNameString ?? base2.Definition.Id.SubtypeName);
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, y), builder.ToString(), base2.Enabled ? Color.White : Color.Gray, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    y += 20f;
                }
            }
        }

        public static void EnableEvents()
        {
            using (SortedSet<MyGlobalEventBase>.Enumerator enumerator = m_globalEvents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = true;
                }
            }
        }

        public static MyGlobalEventBase GetEventById(MyDefinitionId defId)
        {
            using (SortedSet<MyGlobalEventBase>.Enumerator enumerator = m_globalEvents.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGlobalEventBase current = enumerator.Current;
                    if (current.Definition.Id == defId)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static MyObjectBuilder_GlobalEvents GetObjectBuilder()
        {
            MyObjectBuilder_GlobalEvents events = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GlobalEvents>();
            foreach (MyGlobalEventBase base2 in m_globalEvents)
            {
                events.Events.Add(base2.GetObjectBuilder());
            }
            return events;
        }

        public void Init(MyObjectBuilder_GlobalEvents objectBuilder)
        {
            foreach (MyObjectBuilder_GlobalEventBase base2 in objectBuilder.Events)
            {
                m_globalEvents.Add(MyGlobalEventFactory.CreateEvent(base2));
            }
        }

        public override void LoadData()
        {
            m_globalEvents.Clear();
            base.LoadData();
        }

        public static void LoadEvents(MyObjectBuilder_GlobalEvents eventsBuilder)
        {
            if (eventsBuilder != null)
            {
                using (List<MyObjectBuilder_GlobalEventBase>.Enumerator enumerator = eventsBuilder.Events.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyGlobalEventBase item = MyGlobalEventFactory.CreateEvent(enumerator.Current);
                        if ((item != null) && item.IsHandlerValid)
                        {
                            m_globalEvents.Add(item);
                        }
                    }
                }
            }
        }

        private static bool RemovalPredicate(MyGlobalEventBase globalEvent) => 
            (globalEvent.Definition.Id == m_defIdToRemove);

        public static void RemoveEventsById(MyDefinitionId defIdToRemove)
        {
            m_defIdToRemove = defIdToRemove;
            m_globalEvents.RemoveWhere(m_removalPredicate);
        }

        public static void RemoveGlobalEvent(MyGlobalEventBase globalEvent)
        {
            m_globalEvents.Remove(globalEvent);
        }

        public static void RescheduleEvent(MyGlobalEventBase globalEvent, TimeSpan time)
        {
            m_globalEvents.Remove(globalEvent);
            globalEvent.SetActivationTime(time);
            m_globalEvents.Add(globalEvent);
        }

        private void StartGlobalEvent(MyGlobalEventBase globalEvent)
        {
            this.AddGlobalEventToEventLog(globalEvent);
            if (globalEvent.IsHandlerValid)
            {
                object[] parameters = new object[] { globalEvent };
                globalEvent.Action.Invoke(this, parameters);
            }
        }

        protected override void UnloadData()
        {
            m_globalEvents.Clear();
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            if (Sync.IsServer)
            {
                this.m_elapsedTimeInMilliseconds += MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_previousTime;
                this.m_previousTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                if (this.m_elapsedTimeInMilliseconds >= GLOBAL_EVENT_UPDATE_RATIO_IN_MS)
                {
                    foreach (MyGlobalEventBase local1 in m_globalEvents)
                    {
                        TimeSpan activationTime = local1.ActivationTime;
                        local1.SetActivationTime(TimeSpan.FromTicks(activationTime.Ticks - (this.m_elapsedTimeInMilliseconds * 0x2710L)));
                    }
                    for (MyGlobalEventBase base2 = m_globalEvents.FirstOrDefault<MyGlobalEventBase>(); (base2 != null) && base2.IsInPast; base2 = m_globalEvents.FirstOrDefault<MyGlobalEventBase>())
                    {
                        m_globalEvents.Remove(base2);
                        if (base2.Enabled)
                        {
                            this.StartGlobalEvent(base2);
                        }
                        if (base2.IsPeriodic)
                        {
                            if (base2.RemoveAfterHandlerExit)
                            {
                                m_globalEvents.Remove(base2);
                            }
                            else if (!m_globalEvents.Contains(base2))
                            {
                                base2.RecalculateActivationTime();
                                AddGlobalEvent(base2);
                            }
                        }
                    }
                    this.m_elapsedTimeInMilliseconds = 0;
                }
            }
        }

        public static bool EventsEmpty =>
            (m_globalEvents.Count == 0);
    }
}


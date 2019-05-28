namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MySessionComponentTriggerSystem : MySessionComponentBase
    {
        private readonly Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>> m_triggers = new Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>();
        private readonly FastResourceLock m_dictionaryLock = new FastResourceLock();
        public static MySessionComponentTriggerSystem Static;

        public void AddTrigger(MyTriggerComponent trigger)
        {
            if (!this.Contains(trigger))
            {
                using (this.m_dictionaryLock.AcquireExclusiveUsing())
                {
                    CachingHashSet<MyTriggerComponent> set;
                    if (this.m_triggers.TryGetValue((MyEntity) trigger.Entity, out set))
                    {
                        set.Add(trigger);
                    }
                    else
                    {
                        CachingHashSet<MyTriggerComponent> set1 = new CachingHashSet<MyTriggerComponent>();
                        set1.Add(trigger);
                        this.m_triggers[(MyEntity) trigger.Entity] = set1;
                    }
                }
            }
        }

        public bool Contains(MyTriggerComponent trigger)
        {
            using (this.m_dictionaryLock.AcquireSharedUsing())
            {
                using (Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>.ValueCollection.Enumerator enumerator = this.m_triggers.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.Contains(trigger))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void Draw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER)
            {
                using (this.m_dictionaryLock.AcquireSharedUsing())
                {
                    using (Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>.ValueCollection.Enumerator enumerator = this.m_triggers.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            using (HashSet<MyTriggerComponent>.Enumerator enumerator2 = enumerator.Current.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    enumerator2.Current.DebugDraw();
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<MyTriggerComponent> GetAllTriggers()
        {
            List<MyTriggerComponent> list = new List<MyTriggerComponent>();
            using (this.m_dictionaryLock.AcquireSharedUsing())
            {
                using (Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>.ValueCollection.Enumerator enumerator = this.m_triggers.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MyTriggerComponent component in enumerator.Current)
                        {
                            list.Add(component);
                        }
                    }
                }
            }
            return list;
        }

        public List<MyTriggerComponent> GetIntersectingTriggers(Vector3D position)
        {
            List<MyTriggerComponent> list = new List<MyTriggerComponent>();
            using (this.m_dictionaryLock.AcquireSharedUsing())
            {
                using (Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>.ValueCollection.Enumerator enumerator = this.m_triggers.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MyTriggerComponent component in enumerator.Current)
                        {
                            if (component.Contains(position))
                            {
                                list.Add(component);
                            }
                        }
                    }
                }
            }
            return list;
        }

        public MyEntity GetTriggersEntity(string triggerName, out MyTriggerComponent foundTrigger)
        {
            foundTrigger = null;
            using (Dictionary<MyEntity, CachingHashSet<MyTriggerComponent>>.Enumerator enumerator = this.m_triggers.GetEnumerator())
            {
                MyEntity entity;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        KeyValuePair<MyEntity, CachingHashSet<MyTriggerComponent>> current = enumerator.Current;
                        HashSet<MyTriggerComponent>.Enumerator enumerator2 = current.Value.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                MyTriggerComponent component = enumerator2.Current;
                                MyAreaTriggerComponent component2 = component as MyAreaTriggerComponent;
                                if ((component2 != null) && (component2.Name == triggerName))
                                {
                                    foundTrigger = component;
                                    return current.Key;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return entity;
            }
        TR_0000:
            return null;
        }

        public bool IsAnyTriggerActive(MyEntity entity)
        {
            using (this.m_dictionaryLock.AcquireSharedUsing())
            {
                bool flag;
                if (!this.m_triggers.ContainsKey(entity))
                {
                    goto TR_0000;
                }
                else
                {
                    using (HashSet<MyTriggerComponent>.Enumerator enumerator = this.m_triggers[entity].GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            if (enumerator.Current.Enabled)
                            {
                                return true;
                            }
                        }
                    }
                    flag = this.m_triggers[entity].Count == 0;
                }
                return flag;
            }
        TR_0000:
            return true;
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
        }

        public static void RemoveTrigger(MyEntity entity, MyTriggerComponent trigger)
        {
            if (Static != null)
            {
                Static.RemoveTriggerInternal(entity, trigger);
            }
        }

        private void RemoveTriggerInternal(MyEntity entity, MyTriggerComponent trigger)
        {
            using (this.m_dictionaryLock.AcquireExclusiveUsing())
            {
                CachingHashSet<MyTriggerComponent> set;
                if (this.m_triggers.TryGetValue(entity, out set))
                {
                    set.Remove(trigger, false);
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            using (this.m_dictionaryLock.AcquireSharedUsing())
            {
                foreach (CachingHashSet<MyTriggerComponent> local1 in this.m_triggers.Values)
                {
                    local1.ApplyChanges();
                    using (HashSet<MyTriggerComponent>.Enumerator enumerator2 = local1.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.Update();
                        }
                    }
                }
            }
        }
    }
}


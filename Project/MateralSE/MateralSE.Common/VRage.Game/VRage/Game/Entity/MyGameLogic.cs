namespace VRage.Game.Entity
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.ModAPI;

    public static class MyGameLogic
    {
        private static CachingList<MyGameLogicComponent> m_componentsForUpdateOnce = new CachingList<MyGameLogicComponent>();
        private static MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent> m_componentsForUpdate = new MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent>(1);
        private static MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent> m_componentsForUpdate10 = new MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent>(10);
        private static MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent> m_componentsForUpdate100 = new MyDistributedUpdater<CachingList<MyGameLogicComponent>, MyGameLogicComponent>(100);

        public static void ChangeUpdate(MyGameLogicComponent component, MyEntityUpdateEnum newUpdate, bool immediate = false)
        {
            if (!((IMyGameLogicComponent) component).EntityUpdate)
            {
                MyEntityUpdateEnum needsUpdate = component.NeedsUpdate;
                if (needsUpdate != newUpdate)
                {
                    if ((needsUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if ((newUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            m_componentsForUpdateOnce.Add(component);
                        }
                    }
                    else if ((newUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if (immediate)
                        {
                            m_componentsForUpdateOnce.ApplyChanges();
                        }
                        m_componentsForUpdateOnce.Remove(component, immediate);
                    }
                    if ((needsUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if ((newUpdate & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            m_componentsForUpdate.List.Add(component);
                        }
                    }
                    else if ((newUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if (immediate)
                        {
                            m_componentsForUpdate.List.ApplyChanges();
                        }
                        m_componentsForUpdate.List.Remove(component, immediate);
                    }
                    if ((needsUpdate & MyEntityUpdateEnum.EACH_10TH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if ((newUpdate & MyEntityUpdateEnum.EACH_10TH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            m_componentsForUpdate10.List.Add(component);
                        }
                    }
                    else if ((newUpdate & MyEntityUpdateEnum.EACH_10TH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if (immediate)
                        {
                            m_componentsForUpdate10.List.ApplyChanges();
                        }
                        m_componentsForUpdate10.List.Remove(component, immediate);
                    }
                    if ((needsUpdate & MyEntityUpdateEnum.EACH_100TH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if ((newUpdate & MyEntityUpdateEnum.EACH_100TH_FRAME) != MyEntityUpdateEnum.NONE)
                        {
                            m_componentsForUpdate100.List.Add(component);
                        }
                    }
                    else if ((newUpdate & MyEntityUpdateEnum.EACH_100TH_FRAME) == MyEntityUpdateEnum.NONE)
                    {
                        if (immediate)
                        {
                            m_componentsForUpdate100.List.ApplyChanges();
                        }
                        m_componentsForUpdate100.List.Remove(component, immediate);
                    }
                }
            }
        }

        public static void RegisterForUpdate(MyGameLogicComponent component)
        {
            if (!((IMyGameLogicComponent) component).EntityUpdate)
            {
                MyEntityUpdateEnum needsUpdate = component.NeedsUpdate;
                if ((needsUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
                {
                    m_componentsForUpdateOnce.Add(component);
                }
                MyEntityUpdateEnum local1 = needsUpdate;
                if ((local1 & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE)
                {
                    m_componentsForUpdate.List.Add(component);
                }
                MyEntityUpdateEnum local2 = local1;
                if ((local2 & MyEntityUpdateEnum.EACH_10TH_FRAME) != MyEntityUpdateEnum.NONE)
                {
                    m_componentsForUpdate10.List.Add(component);
                }
                if ((local2 & MyEntityUpdateEnum.EACH_100TH_FRAME) != MyEntityUpdateEnum.NONE)
                {
                    m_componentsForUpdate100.List.Add(component);
                }
            }
        }

        public static void UnregisterForUpdate(MyGameLogicComponent component)
        {
            MyEntityUpdateEnum needsUpdate = component.NeedsUpdate;
            if ((needsUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
            {
                m_componentsForUpdateOnce.Remove(component, false);
            }
            MyEntityUpdateEnum local1 = needsUpdate;
            if ((local1 & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE)
            {
                m_componentsForUpdate.List.Remove(component, false);
            }
            MyEntityUpdateEnum local2 = local1;
            if ((local2 & MyEntityUpdateEnum.EACH_10TH_FRAME) != MyEntityUpdateEnum.NONE)
            {
                m_componentsForUpdate10.List.Remove(component, false);
            }
            if ((local2 & MyEntityUpdateEnum.EACH_100TH_FRAME) != MyEntityUpdateEnum.NONE)
            {
                m_componentsForUpdate100.List.Remove(component, false);
            }
        }

        public static void UpdateAfterSimulation()
        {
            m_componentsForUpdate.List.ApplyChanges();
            m_componentsForUpdate.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation(false);
                }
            });
            m_componentsForUpdate10.List.ApplyChanges();
            m_componentsForUpdate10.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation10(false);
                }
            });
            m_componentsForUpdate100.List.ApplyChanges();
            m_componentsForUpdate100.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation100(false);
                }
            });
        }

        public static void UpdateBeforeSimulation()
        {
            UpdateOnceBeforeFrame();
            m_componentsForUpdate.List.ApplyChanges();
            m_componentsForUpdate.Update();
            m_componentsForUpdate.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation(false);
                }
            });
            m_componentsForUpdate10.List.ApplyChanges();
            m_componentsForUpdate10.Update();
            m_componentsForUpdate10.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation10(false);
                }
            });
            m_componentsForUpdate100.List.ApplyChanges();
            m_componentsForUpdate100.Update();
            m_componentsForUpdate100.Iterate(delegate (MyGameLogicComponent c) {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation100(false);
                }
            });
        }

        public static void UpdateOnceBeforeFrame()
        {
            m_componentsForUpdateOnce.ApplyChanges();
            foreach (MyGameLogicComponent component in m_componentsForUpdateOnce)
            {
                component.NeedsUpdate &= ~MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                if (!component.MarkedForClose && !component.Closed)
                {
                    ((IMyGameLogicComponent) component).UpdateOnceBeforeFrame(false);
                }
            }
        }

        public static void UpdatingStopped()
        {
            foreach (MyGameLogicComponent component in m_componentsForUpdate.List)
            {
                if (component.MarkedForClose)
                {
                    continue;
                }
                if (!component.Closed)
                {
                    component.UpdatingStopped();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGameLogic.<>c <>9 = new MyGameLogic.<>c();
            public static Action<MyGameLogicComponent> <>9__8_0;
            public static Action<MyGameLogicComponent> <>9__8_1;
            public static Action<MyGameLogicComponent> <>9__8_2;
            public static Action<MyGameLogicComponent> <>9__9_0;
            public static Action<MyGameLogicComponent> <>9__9_1;
            public static Action<MyGameLogicComponent> <>9__9_2;

            internal void <UpdateAfterSimulation>b__9_0(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation(false);
                }
            }

            internal void <UpdateAfterSimulation>b__9_1(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation10(false);
                }
            }

            internal void <UpdateAfterSimulation>b__9_2(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateAfterSimulation100(false);
                }
            }

            internal void <UpdateBeforeSimulation>b__8_0(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation(false);
                }
            }

            internal void <UpdateBeforeSimulation>b__8_1(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation10(false);
                }
            }

            internal void <UpdateBeforeSimulation>b__8_2(MyGameLogicComponent c)
            {
                if (!c.MarkedForClose && !c.Closed)
                {
                    ((IMyGameLogicComponent) c).UpdateBeforeSimulation100(false);
                }
            }
        }
    }
}


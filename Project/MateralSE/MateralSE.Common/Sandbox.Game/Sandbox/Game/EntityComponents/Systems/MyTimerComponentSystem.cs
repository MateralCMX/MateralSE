namespace Sandbox.Game.EntityComponents.Systems
{
    using Sandbox.Game.Components;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyTimerComponentSystem : MySessionComponentBase
    {
        private const int UPDATE_FRAME = 100;
        public static MyTimerComponentSystem Static;
        private List<MyTimerComponent> m_timerComponents = new List<MyTimerComponent>();
        private int m_frameCounter;

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
        }

        public void Register(MyTimerComponent timerComponent)
        {
            this.m_timerComponents.Add(timerComponent);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public void Unregister(MyTimerComponent timerComponent)
        {
            this.m_timerComponents.Remove(timerComponent);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            int num = this.m_frameCounter + 1;
            this.m_frameCounter = num;
            if ((num % 100) == 0)
            {
                this.m_frameCounter = 0;
                this.UpdateTimerComponents();
            }
        }

        private void UpdateTimerComponents()
        {
            using (List<MyTimerComponent>.Enumerator enumerator = this.m_timerComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
        }
    }
}


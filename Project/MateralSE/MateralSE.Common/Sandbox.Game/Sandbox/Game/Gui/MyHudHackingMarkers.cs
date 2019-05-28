namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Game.Gui;

    public class MyHudHackingMarkers
    {
        private Dictionary<long, MyHudEntityParams> m_markerEntities = new Dictionary<long, MyHudEntityParams>();
        private Dictionary<long, float> m_blinkingTimes = new Dictionary<long, float>();
        private List<long> m_removeList = new List<long>();

        public MyHudHackingMarkers()
        {
            this.Visible = true;
        }

        public void Clear()
        {
            this.m_markerEntities.Clear();
            this.m_blinkingTimes.Clear();
        }

        internal void RegisterMarker(long entityId, MyHudEntityParams hudParams)
        {
            this.m_markerEntities[entityId] = hudParams;
            this.m_blinkingTimes[entityId] = hudParams.BlinkingTime;
        }

        internal void RegisterMarker(MyEntity entity, MyHudEntityParams hudParams)
        {
            this.RegisterMarker(entity.EntityId, hudParams);
        }

        internal void UnregisterMarker(long entityId)
        {
            this.m_markerEntities.Remove(entityId);
            this.m_blinkingTimes.Remove(entityId);
        }

        internal void UnregisterMarker(MyEntity entity)
        {
            this.UnregisterMarker(entity.EntityId);
        }

        internal void UpdateMarkers()
        {
            this.m_removeList.Clear();
            foreach (KeyValuePair<long, MyHudEntityParams> pair in this.m_markerEntities)
            {
                if (this.m_blinkingTimes[pair.Key] <= 0.01666667f)
                {
                    this.m_removeList.Add(pair.Key);
                    continue;
                }
                Dictionary<long, float> blinkingTimes = this.m_blinkingTimes;
                long key = pair.Key;
                blinkingTimes[key] -= 0.01666667f;
            }
            foreach (long num2 in this.m_removeList)
            {
                this.UnregisterMarker(num2);
            }
            this.m_removeList.Clear();
        }

        public bool Visible { get; set; }

        internal Dictionary<long, MyHudEntityParams> MarkerEntities =>
            this.m_markerEntities;
    }
}


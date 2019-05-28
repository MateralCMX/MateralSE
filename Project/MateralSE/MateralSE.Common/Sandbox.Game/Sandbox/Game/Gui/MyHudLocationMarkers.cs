namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Game.Gui;

    public class MyHudLocationMarkers
    {
        private SortedList<long, MyHudEntityParams> m_markerEntities = new SortedList<long, MyHudEntityParams>();

        public MyHudLocationMarkers()
        {
            this.Visible = true;
        }

        public void Clear()
        {
            this.m_markerEntities.Clear();
        }

        public void RegisterMarker(long entityId, MyHudEntityParams hudParams)
        {
            this.m_markerEntities[entityId] = hudParams;
        }

        public void RegisterMarker(MyEntity entity, MyHudEntityParams hudParams)
        {
            if (hudParams.Entity == null)
            {
                hudParams.Entity = entity;
            }
            this.RegisterMarker(entity.EntityId, hudParams);
        }

        public void UnregisterMarker(long entityId)
        {
            this.m_markerEntities.Remove(entityId);
        }

        public void UnregisterMarker(MyEntity entity)
        {
            this.UnregisterMarker(entity.EntityId);
        }

        public bool Visible { get; set; }

        public SortedList<long, MyHudEntityParams> MarkerEntities =>
            this.m_markerEntities;
    }
}


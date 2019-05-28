namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyHudOreMarkers : IEnumerable<MyEntityOreDeposit>, IEnumerable
    {
        private readonly HashSet<MyEntityOreDeposit> m_markers = new HashSet<MyEntityOreDeposit>(MyEntityOreDeposit.Comparer);

        public MyHudOreMarkers()
        {
            this.Visible = true;
        }

        internal void Clear()
        {
            this.m_markers.Clear();
        }

        public HashSet<MyEntityOreDeposit>.Enumerator GetEnumerator() => 
            this.m_markers.GetEnumerator();

        internal void RegisterMarker(MyEntityOreDeposit deposit)
        {
            this.m_markers.Add(deposit);
        }

        IEnumerator<MyEntityOreDeposit> IEnumerable<MyEntityOreDeposit>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        internal void UnregisterMarker(MyEntityOreDeposit deposit)
        {
            this.m_markers.Remove(deposit);
        }

        public bool Visible { get; set; }
    }
}


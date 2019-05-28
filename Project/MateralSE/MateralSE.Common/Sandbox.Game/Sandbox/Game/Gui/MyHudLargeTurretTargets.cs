namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Game.Gui;

    public class MyHudLargeTurretTargets
    {
        private Dictionary<MyEntity, MyHudEntityParams> m_markers = new Dictionary<MyEntity, MyHudEntityParams>();

        public MyHudLargeTurretTargets()
        {
            this.Visible = true;
        }

        internal void RegisterMarker(MyEntity target, MyHudEntityParams hudParams)
        {
            this.m_markers[target] = hudParams;
        }

        internal void UnregisterMarker(MyEntity target)
        {
            this.m_markers.Remove(target);
        }

        public bool Visible { get; set; }

        internal Dictionary<MyEntity, MyHudEntityParams> Targets =>
            this.m_markers;
    }
}


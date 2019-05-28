namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public class MyHudGpsMarkers
    {
        private List<MyGps> m_Inss = new List<MyGps>();
        private DistanceFromCameraComparer m_distFromCamComparer = new DistanceFromCameraComparer();

        public MyHudGpsMarkers()
        {
            this.Visible = true;
        }

        public void Clear()
        {
            this.m_Inss.Clear();
        }

        public void RegisterMarker(MyGps ins)
        {
            if (!this.m_Inss.Contains(ins))
            {
                this.m_Inss.Add(ins);
            }
        }

        internal void Sort()
        {
            this.Sort(this.m_distFromCamComparer);
        }

        internal void Sort(DistanceFromCameraComparer distComparer)
        {
            this.m_Inss.Sort(distComparer);
        }

        public void UnregisterMarker(MyGps ins)
        {
            this.m_Inss.Remove(ins);
        }

        public bool Visible { get; set; }

        internal List<MyGps> MarkerEntities =>
            this.m_Inss;

        public class DistanceFromCameraComparer : IComparer<MyGps>
        {
            public int Compare(MyGps first, MyGps second) => 
                Vector3D.DistanceSquared(MySector.MainCamera.Position, second.Coords).CompareTo(Vector3D.DistanceSquared(MySector.MainCamera.Position, first.Coords));
        }
    }
}


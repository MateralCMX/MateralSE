namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyHierarchyComponent<TYPE> : MyHierarchyComponentBase
    {
        public Action<BoundingBoxD, List<TYPE>> QueryAABBImpl;
        public Action<BoundingSphereD, List<TYPE>> QuerySphereImpl;
        public Action<LineD, List<MyLineSegmentOverlapResult<TYPE>>> QueryLineImpl;

        public void QueryAABB(ref BoundingBoxD aabb, List<TYPE> result)
        {
            if (((base.Entity != null) && !base.Entity.MarkedForClose) && (this.QueryAABBImpl != null))
            {
                this.QueryAABBImpl(aabb, result);
            }
        }

        public void QueryLine(ref LineD line, List<MyLineSegmentOverlapResult<TYPE>> result)
        {
            if (!base.Entity.MarkedForClose && (this.QueryLineImpl != null))
            {
                this.QueryLineImpl(line, result);
            }
        }

        public void QuerySphere(ref BoundingSphereD sphere, List<TYPE> result)
        {
            if (!base.Entity.MarkedForClose && (this.QuerySphereImpl != null))
            {
                this.QuerySphereImpl(sphere, result);
            }
        }
    }
}


namespace Sandbox.Engine.Models
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    internal class MyModelOctree : IMyTriangePruningStructure
    {
        private MyModel m_model;
        private MyModelOctreeNode m_rootNode;

        private MyModelOctree()
        {
        }

        public MyModelOctree(MyModel model)
        {
            this.m_model = model;
            this.m_rootNode = new MyModelOctreeNode(model.BoundingBox);
            for (int i = 0; i < this.m_model.Triangles.Length; i++)
            {
                this.m_rootNode.AddTriangle(model, i, 0);
            }
            this.m_rootNode.OptimizeChilds();
        }

        public void Close()
        {
        }

        public bool GetIntersectionWithAABB(IMyEntity physObject, ref BoundingBoxD aabb) => 
            false;

        public MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(IMyEntity physObject, ref LineD line, IntersectionFlags flags)
        {
            BoundingSphereD worldVolume = physObject.WorldVolume;
            if (!MyUtils.IsLineIntersectingBoundingSphere(ref line, ref worldVolume))
            {
                return null;
            }
            MatrixD worldMatrixNormalizedInv = physObject.GetWorldMatrixNormalizedInv();
            return this.GetIntersectionWithLine(physObject, ref line, ref worldMatrixNormalizedInv, flags);
        }

        public MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(IMyEntity physObject, ref LineD line, ref MatrixD customInvMatrix, IntersectionFlags flags)
        {
            Line line2 = new Line((Vector3) Vector3D.Transform(line.From, ref customInvMatrix), (Vector3) Vector3D.Transform(line.To, ref customInvMatrix), true);
            double? minDistanceUntilNow = null;
            return this.m_rootNode.GetIntersectionWithLine(physObject, this.m_model, ref line2, minDistanceUntilNow, flags);
        }

        public bool GetIntersectionWithSphere(ref BoundingSphere sphere) => 
            this.m_rootNode.GetIntersectionWithSphere(this.m_model, ref sphere);

        public bool GetIntersectionWithSphere(IMyEntity physObject, ref BoundingSphereD sphere)
        {
            MatrixD worldMatrixNormalizedInv = physObject.GetWorldMatrixNormalizedInv();
            BoundingSphere sphere2 = new BoundingSphere((Vector3) Vector3D.Transform(sphere.Center, ref worldMatrixNormalizedInv), (float) sphere.Radius);
            return this.m_rootNode.GetIntersectionWithSphere(this.m_model, ref sphere2);
        }

        public void GetTrianglesIntersectingAABB(ref BoundingBox box, List<MyTriangle_Vertex_Normal> retTriangles, int maxNeighbourTriangles)
        {
        }

        public void GetTrianglesIntersectingLine(IMyEntity entity, ref LineD line, IntersectionFlags flags, List<MyIntersectionResultLineTriangleEx> result)
        {
            MatrixD worldMatrixNormalizedInv = entity.GetWorldMatrixNormalizedInv();
            this.GetTrianglesIntersectingLine(entity, ref line, ref worldMatrixNormalizedInv, flags, result);
        }

        public void GetTrianglesIntersectingLine(IMyEntity entity, ref LineD line, ref MatrixD customInvMatrix, IntersectionFlags flags, List<MyIntersectionResultLineTriangleEx> result)
        {
        }

        public void GetTrianglesIntersectingSphere(ref BoundingSphere sphere, Vector3? referenceNormalVector, float? maxAngle, List<MyTriangle_Vertex_Normals> retTriangles, int maxNeighbourTriangles)
        {
            this.m_rootNode.GetTrianglesIntersectingSphere(this.m_model, ref sphere, referenceNormalVector, maxAngle, retTriangles, maxNeighbourTriangles);
        }

        public int Size =>
            0;
    }
}


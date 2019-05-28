namespace Sandbox.Engine.Models
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    internal class MyModelOctreeNode
    {
        private const int OCTREE_CHILDS_COUNT = 8;
        private const int MAX_RECURSIVE_LEVEL = 8;
        private const float CHILD_BOUNDING_BOX_EXPAND = 0.3f;
        private List<MyModelOctreeNode> m_childs;
        private BoundingBox m_boundingBox;
        private BoundingBox m_realBoundingBox;
        private List<int> m_triangleIndices;

        private MyModelOctreeNode()
        {
        }

        public MyModelOctreeNode(BoundingBox boundingBox)
        {
            this.m_childs = new List<MyModelOctreeNode>(8);
            for (int i = 0; i < 8; i++)
            {
                this.m_childs.Add(null);
            }
            this.m_boundingBox = boundingBox;
            this.m_realBoundingBox = BoundingBox.CreateInvalid();
            this.m_triangleIndices = new List<int>();
        }

        public void AddTriangle(MyModel model, int triangleIndex, int recursiveLevel)
        {
            BoundingBox boundingBox = new BoundingBox();
            model.GetTriangleBoundingBox(triangleIndex, ref boundingBox);
            if (recursiveLevel != 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    BoundingBox childBoundingBox = this.GetChildBoundingBox(this.m_boundingBox, i);
                    if (childBoundingBox.Contains(boundingBox) == ContainmentType.Contains)
                    {
                        if (this.m_childs[i] == null)
                        {
                            this.m_childs[i] = new MyModelOctreeNode(childBoundingBox);
                        }
                        this.m_childs[i].AddTriangle(model, triangleIndex, recursiveLevel + 1);
                        this.m_realBoundingBox = this.m_realBoundingBox.Include(ref boundingBox.Min);
                        this.m_realBoundingBox = this.m_realBoundingBox.Include(ref boundingBox.Max);
                        return;
                    }
                }
            }
            this.m_triangleIndices.Add(triangleIndex);
            this.m_realBoundingBox = this.m_realBoundingBox.Include(ref boundingBox.Min);
            this.m_realBoundingBox = this.m_realBoundingBox.Include(ref boundingBox.Max);
        }

        private unsafe BoundingBox GetChildBoundingBox(BoundingBox parentBoundingBox, int childIndex)
        {
            Vector3 vector;
            switch (childIndex)
            {
                case 0:
                    vector = new Vector3(0f, 0f, 0f);
                    break;

                case 1:
                    vector = new Vector3(1f, 0f, 0f);
                    break;

                case 2:
                    vector = new Vector3(1f, 0f, 1f);
                    break;

                case 3:
                    vector = new Vector3(0f, 0f, 1f);
                    break;

                case 4:
                    vector = new Vector3(0f, 1f, 0f);
                    break;

                case 5:
                    vector = new Vector3(1f, 1f, 0f);
                    break;

                case 6:
                    vector = new Vector3(1f, 1f, 1f);
                    break;

                case 7:
                    vector = new Vector3(0f, 1f, 1f);
                    break;

                default:
                    throw new InvalidBranchException();
            }
            Vector3 vector2 = (parentBoundingBox.Max - parentBoundingBox.Min) / 2f;
            BoundingBox box = new BoundingBox {
                Min = parentBoundingBox.Min + (vector * vector2)
            };
            BoundingBox* boxPtr1 = (BoundingBox*) ref box;
            boxPtr1->Max = box.Min + vector2;
            Vector3* vectorPtr1 = (Vector3*) ref box.Min;
            vectorPtr1[0] -= vector2 * 0.3f;
            Vector3* vectorPtr2 = (Vector3*) ref box.Max;
            vectorPtr2[0] += vector2 * 0.3f;
            BoundingBox* boxPtr2 = (BoundingBox*) ref box;
            boxPtr2->Min = Vector3.Max(box.Min, parentBoundingBox.Min);
            BoundingBox* boxPtr3 = (BoundingBox*) ref box;
            boxPtr3->Max = Vector3.Min(box.Max, parentBoundingBox.Max);
            return box;
        }

        public MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(IMyEntity physObject, MyModel model, ref Line line, double? minDistanceUntilNow, IntersectionFlags flags)
        {
            MyIntersectionResultLineTriangle? nullable = this.GetIntersectionWithLineRecursive(model, ref line, minDistanceUntilNow);
            if (nullable != null)
            {
                return new MyIntersectionResultLineTriangleEx(nullable.Value, physObject, ref line);
            }
            return null;
        }

        private MyIntersectionResultLineTriangle? GetIntersectionWithLineRecursive(MyModel model, ref Line line, double? minDistanceUntilNow)
        {
            double? nullable4;
            double? nullable1;
            float? lineBoundingBoxIntersection = MyUtils.GetLineBoundingBoxIntersection(ref line, ref this.m_boundingBox);
            if (lineBoundingBoxIntersection != null)
            {
                nullable1 = new double?((double) lineBoundingBoxIntersection.GetValueOrDefault());
            }
            else
            {
                nullable4 = null;
                nullable1 = nullable4;
            }
            double? nullable = nullable1;
            if (nullable != null)
            {
                if (minDistanceUntilNow != null)
                {
                    nullable4 = minDistanceUntilNow;
                    double num = nullable.Value;
                    if ((nullable4.GetValueOrDefault() < num) & (nullable4 != null))
                    {
                        goto TR_0000;
                    }
                }
                MyIntersectionResultLineTriangle? a = null;
                BoundingBox boundingBox = new BoundingBox();
                BoundingBox box = BoundingBox.CreateInvalid().Include(line.From).Include(line.To);
                for (int i = 0; i < this.m_triangleIndices.Count; i++)
                {
                    int triangleIndex = this.m_triangleIndices[i];
                    model.GetTriangleBoundingBox(triangleIndex, ref boundingBox);
                    if (boundingBox.Intersects(ref box))
                    {
                        MyTriangle_Vertices vertices;
                        MyTriangleVertexIndices indices = model.Triangles[triangleIndex];
                        vertices.Vertex0 = model.GetVertex(indices.I0);
                        vertices.Vertex1 = model.GetVertex(indices.I2);
                        vertices.Vertex2 = model.GetVertex(indices.I1);
                        float? lineTriangleIntersection = MyUtils.GetLineTriangleIntersection(ref line, ref vertices);
                        if ((lineTriangleIntersection != null) && ((a == null) || (lineTriangleIntersection.Value < a.Value.Distance)))
                        {
                            Vector3 normalVectorFromTriangle = MyUtils.GetNormalVectorFromTriangle(ref vertices);
                            a = new MyIntersectionResultLineTriangle(triangleIndex, ref vertices, ref normalVectorFromTriangle, lineTriangleIntersection.Value);
                        }
                    }
                }
                if (this.m_childs != null)
                {
                    for (int j = 0; j < this.m_childs.Count; j++)
                    {
                        double? nullable8;
                        if (a != null)
                        {
                            nullable8 = new double?((double) a.Value.Distance);
                        }
                        else
                        {
                            nullable4 = null;
                            nullable8 = nullable4;
                        }
                        MyIntersectionResultLineTriangle? b = this.m_childs[j].GetIntersectionWithLineRecursive(model, ref line, nullable8);
                        a = MyIntersectionResultLineTriangle.GetCloserIntersection(ref a, ref b);
                    }
                }
                return a;
            }
        TR_0000:
            return null;
        }

        public bool GetIntersectionWithSphere(MyModel model, ref BoundingSphere sphere)
        {
            if (this.m_boundingBox.Intersects(ref sphere))
            {
                BoundingBox boundingBox = new BoundingBox();
                for (int i = 0; i < this.m_triangleIndices.Count; i++)
                {
                    int triangleIndex = this.m_triangleIndices[i];
                    model.GetTriangleBoundingBox(triangleIndex, ref boundingBox);
                    if (boundingBox.Intersects(ref sphere))
                    {
                        MyTriangle_Vertices vertices;
                        MyTriangleVertexIndices indices = model.Triangles[triangleIndex];
                        vertices.Vertex0 = model.GetVertex(indices.I0);
                        vertices.Vertex1 = model.GetVertex(indices.I2);
                        vertices.Vertex2 = model.GetVertex(indices.I1);
                        Plane trianglePlane = new Plane(vertices.Vertex0, vertices.Vertex1, vertices.Vertex2);
                        if (MyUtils.GetSphereTriangleIntersection(ref sphere, ref trianglePlane, ref vertices) != null)
                        {
                            return true;
                        }
                    }
                }
                if (this.m_childs != null)
                {
                    for (int j = 0; j < this.m_childs.Count; j++)
                    {
                        if (this.m_childs[j].GetIntersectionWithSphere(model, ref sphere))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void GetTrianglesIntersectingSphere(MyModel model, ref BoundingSphere sphere, Vector3? referenceNormalVector, float? maxAngle, List<MyTriangle_Vertex_Normals> retTriangles, int maxNeighbourTriangles)
        {
            if (this.m_boundingBox.Intersects(ref sphere))
            {
                BoundingBox boundingBox = new BoundingBox();
                int num = 0;
                while (true)
                {
                    while (true)
                    {
                        if (num >= this.m_triangleIndices.Count)
                        {
                            if (this.m_childs != null)
                            {
                                for (int i = 0; i < this.m_childs.Count; i++)
                                {
                                    this.m_childs[i].GetTrianglesIntersectingSphere(model, ref sphere, referenceNormalVector, maxAngle, retTriangles, maxNeighbourTriangles);
                                }
                            }
                            return;
                        }
                        if (retTriangles.Count == maxNeighbourTriangles)
                        {
                            return;
                        }
                        int triangleIndex = this.m_triangleIndices[num];
                        model.GetTriangleBoundingBox(triangleIndex, ref boundingBox);
                        if (boundingBox.Intersects(ref sphere))
                        {
                            MyTriangle_Vertices vertices;
                            MyTriangle_Normals normals;
                            MyTriangleVertexIndices indices = model.Triangles[triangleIndex];
                            vertices.Vertex0 = model.GetVertex(indices.I0);
                            vertices.Vertex1 = model.GetVertex(indices.I2);
                            vertices.Vertex2 = model.GetVertex(indices.I1);
                            normals.Normal0 = model.GetVertexNormal(indices.I0);
                            normals.Normal1 = model.GetVertexNormal(indices.I2);
                            normals.Normal2 = model.GetVertexNormal(indices.I1);
                            Plane trianglePlane = new Plane(vertices.Vertex0, vertices.Vertex1, vertices.Vertex2);
                            if (MyUtils.GetSphereTriangleIntersection(ref sphere, ref trianglePlane, ref vertices) != null)
                            {
                                MyTriangle_Vertex_Normals normals2;
                                Vector3 normalVectorFromTriangle = MyUtils.GetNormalVectorFromTriangle(ref vertices);
                                if ((referenceNormalVector != null) && (maxAngle != null))
                                {
                                    float? nullable2 = maxAngle;
                                    if (!((MyUtils.GetAngleBetweenVectors(referenceNormalVector.Value, normalVectorFromTriangle) <= nullable2.GetValueOrDefault()) & (nullable2 != null)))
                                    {
                                        break;
                                    }
                                }
                                normals2.Vertices = vertices;
                                normals2.Normals = normals;
                                retTriangles.Add(normals2);
                            }
                        }
                        break;
                    }
                    num++;
                }
            }
        }

        public void GetTrianglesIntersectingSphere(MyModel model, ref BoundingSphereD sphere, Vector3? referenceNormalVector, float? maxAngle, List<MyTriangle_Vertex_Normal> retTriangles, int maxNeighbourTriangles)
        {
            BoundingSphere sphere2 = (BoundingSphere) sphere;
            if (this.m_boundingBox.Intersects(ref sphere))
            {
                BoundingBox boundingBox = new BoundingBox();
                int num = 0;
                while (true)
                {
                    while (true)
                    {
                        if (num >= this.m_triangleIndices.Count)
                        {
                            if (this.m_childs != null)
                            {
                                for (int i = 0; i < this.m_childs.Count; i++)
                                {
                                    this.m_childs[i].GetTrianglesIntersectingSphere(model, ref sphere, referenceNormalVector, maxAngle, retTriangles, maxNeighbourTriangles);
                                }
                            }
                            return;
                        }
                        if (retTriangles.Count == maxNeighbourTriangles)
                        {
                            return;
                        }
                        int triangleIndex = this.m_triangleIndices[num];
                        model.GetTriangleBoundingBox(triangleIndex, ref boundingBox);
                        if (boundingBox.Intersects(ref sphere))
                        {
                            MyTriangle_Vertices vertices;
                            MyTriangleVertexIndices indices = model.Triangles[triangleIndex];
                            vertices.Vertex0 = model.GetVertex(indices.I0);
                            vertices.Vertex1 = model.GetVertex(indices.I2);
                            vertices.Vertex2 = model.GetVertex(indices.I1);
                            Vector3 normalVectorFromTriangle = MyUtils.GetNormalVectorFromTriangle(ref vertices);
                            Plane trianglePlane = new Plane(vertices.Vertex0, vertices.Vertex1, vertices.Vertex2);
                            if (MyUtils.GetSphereTriangleIntersection(ref sphere2, ref trianglePlane, ref vertices) != null)
                            {
                                MyTriangle_Vertex_Normal normal;
                                Vector3 vectorB = MyUtils.GetNormalVectorFromTriangle(ref vertices);
                                if ((referenceNormalVector != null) && (maxAngle != null))
                                {
                                    float? nullable2 = maxAngle;
                                    if (!((MyUtils.GetAngleBetweenVectors(referenceNormalVector.Value, vectorB) <= nullable2.GetValueOrDefault()) & (nullable2 != null)))
                                    {
                                        break;
                                    }
                                }
                                normal.Vertexes = vertices;
                                normal.Normal = normalVectorFromTriangle;
                                retTriangles.Add(normal);
                            }
                        }
                        break;
                    }
                    num++;
                }
            }
        }

        public void OptimizeChilds()
        {
            this.m_boundingBox = this.m_realBoundingBox;
            for (int i = 0; i < this.m_childs.Count; i++)
            {
                if (this.m_childs[i] != null)
                {
                    this.m_childs[i].OptimizeChilds();
                }
            }
            while (this.m_childs.Remove(null))
            {
            }
            while ((this.m_childs != null) && (this.m_childs.Count == 1))
            {
                foreach (int num2 in this.m_childs[0].m_triangleIndices)
                {
                    this.m_triangleIndices.Add(num2);
                }
                this.m_childs = this.m_childs[0].m_childs;
            }
            if ((this.m_childs != null) && (this.m_childs.Count == 0))
            {
                this.m_childs = null;
            }
        }
    }
}


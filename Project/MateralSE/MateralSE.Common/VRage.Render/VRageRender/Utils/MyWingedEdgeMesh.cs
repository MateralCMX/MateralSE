namespace VRageRender.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Library;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyWingedEdgeMesh
    {
        public static bool BASIC_CONSISTENCY_CHECKS = false;
        public static bool ADVANCED_CONSISTENCY_CHECKS = false;
        public static int INVALID_INDEX = -1;
        private static HashSet<int> m_tmpFreeEdges = new HashSet<int>();
        private static HashSet<int> m_tmpFreeFaces = new HashSet<int>();
        private static HashSet<int> m_tmpFreeVertices = new HashSet<int>();
        private static HashSet<int> m_tmpVisitedIndices = new HashSet<int>();
        private static HashSet<int> m_tmpDebugDrawFreeIndices = new HashSet<int>();
        private static List<int> m_tmpIndexList = new List<int>();
        private List<EdgeTableEntry> m_edgeTable = new List<EdgeTableEntry>();
        private List<VertexTableEntry> m_vertexTable = new List<VertexTableEntry>();
        private List<FaceTableEntry> m_faceTable = new List<FaceTableEntry>();
        private int m_freeEdges = INVALID_INDEX;
        private int m_freeVertices = INVALID_INDEX;
        private int m_freeFaces = INVALID_INDEX;
        private static HashSet<int> m_debugDrawEdges = null;

        private int AllocateAndInsertFace(object userData, int incidentEdge)
        {
            FaceTableEntry item = new FaceTableEntry {
                IncidentEdge = incidentEdge,
                UserData = userData
            };
            if (this.m_freeFaces == INVALID_INDEX)
            {
                this.m_faceTable.Add(item);
                return this.m_faceTable.Count;
            }
            int freeFaces = this.m_freeFaces;
            this.m_freeFaces = this.m_faceTable[this.m_freeFaces].NextFreeEntry;
            this.m_faceTable[freeFaces] = item;
            return freeFaces;
        }

        private int AllocateAndInsertVertex(ref Vector3 position, int incidentEdge)
        {
            VertexTableEntry item = new VertexTableEntry {
                IncidentEdge = incidentEdge,
                VertexPosition = position
            };
            if (this.m_freeVertices == INVALID_INDEX)
            {
                this.m_vertexTable.Add(item);
                return this.m_vertexTable.Count;
            }
            int freeVertices = this.m_freeVertices;
            if ((freeVertices < 0) || (freeVertices >= this.m_vertexTable.Count))
            {
                this.m_freeVertices = -1;
                return this.AllocateAndInsertVertex(ref position, incidentEdge);
            }
            this.m_freeVertices = this.m_vertexTable[freeVertices].NextFreeEntry;
            this.m_vertexTable[freeVertices] = item;
            return freeVertices;
        }

        private int AllocateEdge()
        {
            EdgeTableEntry item = new EdgeTableEntry();
            item.Init();
            if (this.m_freeEdges == INVALID_INDEX)
            {
                this.m_edgeTable.Add(item);
                return this.m_edgeTable.Count;
            }
            int freeEdges = this.m_freeEdges;
            this.m_freeEdges = this.m_edgeTable[this.m_freeEdges].NextFreeEntry;
            this.m_edgeTable[freeEdges] = item;
            return freeEdges;
        }

        public int ApproximateMemoryFootprint()
        {
            int num = MyEnvironment.Is64BitProcess ? 0x20 : 20;
            int num2 = MyEnvironment.Is64BitProcess ? 0x58 : 0x38;
            int num3 = 0x10;
            int num4 = 0x20;
            int num5 = (MyEnvironment.Is64BitProcess ? 8 : 12) + num2;
            return (((((MyEnvironment.Is64BitProcess ? 0x34 : 0x20) + (3 * num)) + (this.m_edgeTable.Capacity * num4)) + (this.m_faceTable.Capacity * num5)) + (this.m_vertexTable.Capacity * num3));
        }

        [Conditional("DEBUG")]
        private void CheckEdgeIndexValid(int index)
        {
            if (BASIC_CONSISTENCY_CHECKS)
            {
                EdgeTableEntry entry;
                for (int i = this.m_freeEdges; i != INVALID_INDEX; i = entry.NextFreeEntry)
                {
                    entry = this.m_edgeTable[i];
                }
            }
        }

        [Conditional("DEBUG")]
        public void CheckEdgeIndexValidQuick(int index)
        {
            bool flag1 = BASIC_CONSISTENCY_CHECKS;
        }

        [Conditional("DEBUG")]
        private void CheckFaceIndexValid(int index)
        {
            if (BASIC_CONSISTENCY_CHECKS)
            {
                FaceTableEntry entry;
                for (int i = this.m_freeFaces; i != INVALID_INDEX; i = entry.NextFreeEntry)
                {
                    entry = this.m_faceTable[i];
                }
            }
        }

        [Conditional("DEBUG")]
        public void CheckFaceIndexValidQuick(int index)
        {
            bool flag1 = BASIC_CONSISTENCY_CHECKS;
        }

        [Conditional("DEBUG")]
        private void CheckFreeEntryConsistency()
        {
            if (BASIC_CONSISTENCY_CHECKS)
            {
                m_tmpVisitedIndices.Clear();
                int freeVertices = this.m_freeVertices;
                while (freeVertices != INVALID_INDEX)
                {
                    VertexTableEntry entry = this.m_vertexTable[freeVertices];
                    freeVertices = entry.NextFreeEntry;
                    m_tmpVisitedIndices.Add(freeVertices);
                }
                m_tmpVisitedIndices.Clear();
                freeVertices = this.m_freeEdges;
                while (freeVertices != INVALID_INDEX)
                {
                    EdgeTableEntry entry2 = this.m_edgeTable[freeVertices];
                    freeVertices = entry2.NextFreeEntry;
                    m_tmpVisitedIndices.Add(freeVertices);
                }
                m_tmpVisitedIndices.Clear();
                freeVertices = this.m_freeFaces;
                while (freeVertices != INVALID_INDEX)
                {
                    FaceTableEntry entry3 = this.m_faceTable[freeVertices];
                    freeVertices = entry3.NextFreeEntry;
                    m_tmpVisitedIndices.Add(freeVertices);
                }
                m_tmpVisitedIndices.Clear();
            }
        }

        [Conditional("DEBUG")]
        public void CheckMeshConsistency()
        {
            if (ADVANCED_CONSISTENCY_CHECKS)
            {
                for (int i = 0; i < this.m_edgeTable.Count; i++)
                {
                    if (!m_tmpFreeEdges.Contains(i))
                    {
                        EdgeTableEntry entry = this.m_edgeTable[i];
                        int num1 = INVALID_INDEX;
                        int leftFace = entry.LeftFace;
                        int num8 = INVALID_INDEX;
                        int rightFace = entry.RightFace;
                        int leftSucc = entry.LeftSucc;
                        int leftPred = entry.LeftPred;
                        int rightSucc = entry.RightSucc;
                        int rightPred = entry.RightPred;
                    }
                }
                for (int j = 0; j < this.m_vertexTable.Count; j++)
                {
                    if (!m_tmpFreeVertices.Contains(j))
                    {
                        int num3 = 0;
                        int incidentEdge = this.m_vertexTable[j].IncidentEdge;
                        EdgeTableEntry entry2 = this.m_edgeTable[incidentEdge];
                        if (entry2.VertexLeftFace(j) == INVALID_INDEX)
                        {
                            num3++;
                        }
                        for (int m = entry2.VertexSucc(j); m != incidentEdge; m = entry2.VertexSucc(j))
                        {
                            entry2 = this.m_edgeTable[m];
                            if (entry2.VertexLeftFace(j) == INVALID_INDEX)
                            {
                                num3++;
                            }
                        }
                    }
                }
                for (int k = 0; k < this.m_faceTable.Count; k++)
                {
                    if (!m_tmpFreeFaces.Contains(k))
                    {
                        FaceTableEntry local1 = this.m_faceTable[k];
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void CheckVertexIndexValid(int index)
        {
            if (BASIC_CONSISTENCY_CHECKS)
            {
                VertexTableEntry entry;
                for (int i = this.m_freeVertices; i != INVALID_INDEX; i = entry.NextFreeEntry)
                {
                    entry = this.m_vertexTable[i];
                }
            }
        }

        [Conditional("DEBUG")]
        public void CheckVertexIndexValidQuick(int index)
        {
            bool flag1 = BASIC_CONSISTENCY_CHECKS;
        }

        [Conditional("DEBUG")]
        private void CheckVertexLoopConsistency(int vertexIndex)
        {
        }

        public MyWingedEdgeMesh Copy()
        {
            MyWingedEdgeMesh mesh1 = new MyWingedEdgeMesh();
            mesh1.m_freeEdges = this.m_freeEdges;
            mesh1.m_freeFaces = this.m_freeFaces;
            mesh1.m_freeVertices = this.m_freeVertices;
            mesh1.m_edgeTable = this.m_edgeTable.ToList<EdgeTableEntry>();
            mesh1.m_vertexTable = this.m_vertexTable.ToList<VertexTableEntry>();
            mesh1.m_faceTable = this.m_faceTable.ToList<FaceTableEntry>();
            return mesh1;
        }

        public void CustomDebugDrawFaces(ref Matrix drawMatrix, MyWEMDebugDrawMode draw, Func<object, string> drawFunction)
        {
            if ((draw & MyWEMDebugDrawMode.FACES) != MyWEMDebugDrawMode.NONE)
            {
                m_tmpDebugDrawFreeIndices.Clear();
                int freeFaces = this.m_freeFaces;
                while (true)
                {
                    if (freeFaces == INVALID_INDEX)
                    {
                        for (int i = 0; i < this.m_faceTable.Count; i++)
                        {
                            if (!m_tmpDebugDrawFreeIndices.Contains(i))
                            {
                                Vector3 zero = Vector3.Zero;
                                int num3 = 0;
                                Face face = this.GetFace(i);
                                FaceVertexEnumerator vertexEnumerator = face.GetVertexEnumerator();
                                while (true)
                                {
                                    if (!vertexEnumerator.MoveNext())
                                    {
                                        MyRenderProxy.DebugDrawText3D(Vector3.Transform(zero / ((float) num3), (Matrix) drawMatrix), drawFunction(face.GetUserData<object>()), Color.CadetBlue, 0.6f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                                        break;
                                    }
                                    zero += vertexEnumerator.Current;
                                    num3++;
                                }
                            }
                        }
                        break;
                    }
                    m_tmpDebugDrawFreeIndices.Add(freeFaces);
                    FaceTableEntry entry = this.m_faceTable[freeFaces];
                    freeFaces = entry.NextFreeEntry;
                }
            }
        }

        private void DeallocateEdge(int edgeIndex)
        {
            EdgeTableEntry entry = new EdgeTableEntry {
                NextFreeEntry = this.m_freeEdges
            };
            this.m_edgeTable[edgeIndex] = entry;
            this.m_freeEdges = edgeIndex;
        }

        private void DeallocateFace(int faceIndex)
        {
            FaceTableEntry entry = new FaceTableEntry {
                NextFreeEntry = this.m_freeFaces
            };
            this.m_faceTable[faceIndex] = entry;
            this.m_freeFaces = faceIndex;
        }

        private void DeallocateVertex(int vertexIndex)
        {
            VertexTableEntry entry = new VertexTableEntry {
                NextFreeEntry = this.m_freeVertices
            };
            this.m_vertexTable[vertexIndex] = entry;
            this.m_freeVertices = vertexIndex;
        }

        public void DebugDraw(ref Matrix drawMatrix, MyWEMDebugDrawMode draw)
        {
            m_tmpDebugDrawFreeIndices.Clear();
            int freeEdges = this.m_freeEdges;
            while (freeEdges != INVALID_INDEX)
            {
                m_tmpDebugDrawFreeIndices.Add(freeEdges);
                EdgeTableEntry entry = this.m_edgeTable[freeEdges];
                freeEdges = entry.NextFreeEntry;
            }
            for (int i = 0; i < this.m_edgeTable.Count; i++)
            {
                if (!m_tmpDebugDrawFreeIndices.Contains(i) && ((m_debugDrawEdges == null) || m_debugDrawEdges.Contains(i)))
                {
                    EdgeTableEntry edgeEntry = this.GetEdgeEntry(i);
                    Vector3 pointFrom = Vector3.Transform(this.m_vertexTable[edgeEntry.Vertex1].VertexPosition, (Matrix) drawMatrix);
                    Vector3 pointTo = Vector3.Transform(this.m_vertexTable[edgeEntry.Vertex2].VertexPosition, (Matrix) drawMatrix);
                    Vector3 vector3 = (pointFrom + pointTo) * 0.5f;
                    EdgeTableEntry entry3 = this.GetEdgeEntry(edgeEntry.LeftSucc);
                    EdgeTableEntry entry4 = this.GetEdgeEntry(edgeEntry.RightPred);
                    EdgeTableEntry entry5 = this.GetEdgeEntry(edgeEntry.LeftPred);
                    EdgeTableEntry entry6 = this.GetEdgeEntry(edgeEntry.RightSucc);
                    Vector3 vector4 = Vector3.Transform(this.m_vertexTable[entry3.OtherVertex(edgeEntry.Vertex1)].VertexPosition, (Matrix) drawMatrix);
                    Vector3 vector5 = Vector3.Transform(this.m_vertexTable[entry4.OtherVertex(edgeEntry.Vertex1)].VertexPosition, (Matrix) drawMatrix);
                    Vector3 vector6 = Vector3.Transform(this.m_vertexTable[entry5.OtherVertex(edgeEntry.Vertex2)].VertexPosition, (Matrix) drawMatrix);
                    Vector3 vector7 = Vector3.Transform(this.m_vertexTable[entry6.OtherVertex(edgeEntry.Vertex2)].VertexPosition, (Matrix) drawMatrix);
                    if (((draw & MyWEMDebugDrawMode.LINES) != MyWEMDebugDrawMode.NONE) || ((draw & MyWEMDebugDrawMode.EDGES) != MyWEMDebugDrawMode.NONE))
                    {
                        bool flag = (edgeEntry.LeftFace == INVALID_INDEX) || (edgeEntry.RightFace == INVALID_INDEX);
                        Color colorFrom = ((draw & MyWEMDebugDrawMode.LINES) != MyWEMDebugDrawMode.NONE) ? (flag ? Color.Red : Color.DarkSlateBlue) : Color.Black;
                        MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, colorFrom, ((draw & MyWEMDebugDrawMode.LINES) != MyWEMDebugDrawMode.NONE) ? (flag ? Color.Red : Color.DarkSlateBlue) : Color.White, (draw & MyWEMDebugDrawMode.LINES_DEPTH) != MyWEMDebugDrawMode.NONE, false);
                    }
                    if ((draw & MyWEMDebugDrawMode.EDGES) != MyWEMDebugDrawMode.NONE)
                    {
                        if (edgeEntry.RightFace == INVALID_INDEX)
                        {
                            Vector3 vector8 = pointTo - pointFrom;
                            vector8.Normalize();
                            vector7 = pointTo + (pointTo - (vector6 - (vector8 * Vector3.Dot(vector6 - pointTo, vector8))));
                            vector5 = pointFrom + (pointFrom - (vector4 - (vector8 * Vector3.Dot(vector4 - pointFrom, vector8))));
                        }
                        if (edgeEntry.LeftFace == INVALID_INDEX)
                        {
                            Vector3 vector9 = pointFrom - pointTo;
                            vector9.Normalize();
                            vector6 = pointTo + (pointTo - (vector7 - (vector9 * Vector3.Dot(vector7 - pointTo, vector9))));
                            vector4 = pointFrom + (pointFrom - (vector5 - (vector9 * Vector3.Dot(vector5 - pointFrom, vector9))));
                        }
                        vector4 = (((pointFrom * 0.8f) + (vector4 * 0.2f)) * 0.5f) + (vector3 * 0.5f);
                        vector5 = (((pointFrom * 0.8f) + (vector5 * 0.2f)) * 0.5f) + (vector3 * 0.5f);
                        vector6 = (((pointTo * 0.8f) + (vector6 * 0.2f)) * 0.5f) + (vector3 * 0.5f);
                        vector7 = (((pointTo * 0.8f) + (vector7 * 0.2f)) * 0.5f) + (vector3 * 0.5f);
                        MyRenderProxy.DebugDrawLine3D(vector3, vector4, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawLine3D(vector3, vector5, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawLine3D(vector3, vector6, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawLine3D(vector3, vector7, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawLine3D(vector3, (vector7 + vector5) * 0.5f, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawLine3D(vector3, (vector6 + vector4) * 0.5f, Color.Black, Color.Gray, false, false);
                        MyRenderProxy.DebugDrawText3D(vector3, i.ToString(), Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        MyRenderProxy.DebugDrawText3D(vector4, edgeEntry.LeftSucc.ToString(), Color.LightYellow, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        MyRenderProxy.DebugDrawText3D(vector5, edgeEntry.RightPred.ToString(), Color.LightYellow, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        MyRenderProxy.DebugDrawText3D(vector6, edgeEntry.LeftPred.ToString(), Color.LightYellow, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        MyRenderProxy.DebugDrawText3D(vector7, edgeEntry.RightSucc.ToString(), Color.LightYellow, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        if (edgeEntry.RightFace != INVALID_INDEX)
                        {
                            MyRenderProxy.DebugDrawText3D((vector7 + vector5) * 0.5f, edgeEntry.RightFace.ToString(), Color.LightBlue, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        else
                        {
                            MyRenderProxy.DebugDrawText3D((vector7 + vector5) * 0.5f, edgeEntry.RightFace.ToString(), Color.Red, 0.8f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        if (edgeEntry.LeftFace != INVALID_INDEX)
                        {
                            MyRenderProxy.DebugDrawText3D((vector6 + vector4) * 0.5f, edgeEntry.LeftFace.ToString(), Color.LightBlue, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        else
                        {
                            MyRenderProxy.DebugDrawText3D((vector6 + vector4) * 0.5f, edgeEntry.LeftFace.ToString(), Color.Red, 0.8f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        MyRenderProxy.DebugDrawText3D((pointFrom * 0.05f) + (pointTo * 0.95f), edgeEntry.Vertex2.ToString(), Color.LightGreen, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        MyRenderProxy.DebugDrawText3D((pointFrom * 0.95f) + (pointTo * 0.05f), edgeEntry.Vertex1.ToString(), Color.LightGreen, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                    }
                }
            }
            if (((draw & MyWEMDebugDrawMode.VERTICES) != MyWEMDebugDrawMode.NONE) || ((draw & MyWEMDebugDrawMode.VERTICES_DETAILED) != MyWEMDebugDrawMode.NONE))
            {
                m_tmpDebugDrawFreeIndices.Clear();
                freeEdges = this.m_freeVertices;
                while (true)
                {
                    if (freeEdges == INVALID_INDEX)
                    {
                        for (int j = 0; j < this.m_vertexTable.Count; j++)
                        {
                            if (!m_tmpDebugDrawFreeIndices.Contains(j))
                            {
                                Vector3 worldCoord = Vector3.Transform(this.m_vertexTable[j].VertexPosition, (Matrix) drawMatrix);
                                if ((draw & MyWEMDebugDrawMode.VERTICES_DETAILED) != MyWEMDebugDrawMode.NONE)
                                {
                                    MyRenderProxy.DebugDrawText3D(worldCoord, this.m_vertexTable[j].ToString(), Color.LightGreen, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                                }
                                else
                                {
                                    MyRenderProxy.DebugDrawText3D(worldCoord, j.ToString(), Color.LightGreen, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                                }
                            }
                        }
                        break;
                    }
                    m_tmpDebugDrawFreeIndices.Add(freeEdges);
                    VertexTableEntry entry7 = this.m_vertexTable[freeEdges];
                    freeEdges = entry7.NextFreeEntry;
                }
            }
            m_tmpDebugDrawFreeIndices.Clear();
        }

        public static void DebugDrawEdgesAdd(int edgeIndex)
        {
            if (m_debugDrawEdges != null)
            {
                m_debugDrawEdges.Add(edgeIndex);
            }
        }

        public static void DebugDrawEdgesReset()
        {
            if (m_debugDrawEdges == null)
            {
                m_debugDrawEdges = new HashSet<int>();
            }
            else if (m_debugDrawEdges.Count == 0)
            {
                m_debugDrawEdges = null;
            }
            else
            {
                m_debugDrawEdges.Clear();
            }
        }

        public int ExtrudeTriangleFromEdge(ref Vector3 newVertex, int edge, object faceUserData, out int newEdgeS, out int newEdgeP)
        {
            int num3;
            int num4;
            EdgeTableEntry edgeEntry = this.GetEdgeEntry(edge);
            newEdgeP = this.AllocateEdge();
            newEdgeS = this.AllocateEdge();
            int index = edgeEntry.FacePred(INVALID_INDEX);
            int num2 = edgeEntry.FaceSucc(INVALID_INDEX);
            edgeEntry.GetFaceVertices(INVALID_INDEX, out num3, out num4);
            EdgeTableEntry entry = this.GetEdgeEntry(index);
            EdgeTableEntry entry3 = this.GetEdgeEntry(num2);
            EdgeTableEntry entry4 = new EdgeTableEntry();
            entry4.Init();
            EdgeTableEntry entry5 = new EdgeTableEntry();
            entry5.Init();
            int face = this.AllocateAndInsertFace(faceUserData, newEdgeP);
            int vertex = this.AllocateAndInsertVertex(ref newVertex, newEdgeP);
            entry4.AddFace(face);
            entry4.SetFacePredVertex(face, vertex);
            entry4.SetFacePred(face, newEdgeS);
            entry4.SetFaceSuccVertex(face, num3);
            entry4.SetFaceSucc(face, edge);
            entry4.SetFacePred(INVALID_INDEX, index);
            entry4.SetFaceSucc(INVALID_INDEX, newEdgeS);
            entry5.AddFace(face);
            entry5.SetFacePredVertex(face, num4);
            entry5.SetFacePred(face, edge);
            entry5.SetFaceSuccVertex(face, vertex);
            entry5.SetFaceSucc(face, newEdgeP);
            entry5.SetFacePred(INVALID_INDEX, newEdgeP);
            entry5.SetFaceSucc(INVALID_INDEX, num2);
            edgeEntry.AddFace(face);
            edgeEntry.SetFacePred(face, newEdgeP);
            edgeEntry.SetFaceSucc(face, newEdgeS);
            entry.SetFaceSucc(INVALID_INDEX, newEdgeP);
            entry3.SetFacePred(INVALID_INDEX, newEdgeS);
            this.SetEdgeEntry(newEdgeP, ref entry4);
            this.SetEdgeEntry(newEdgeS, ref entry5);
            this.SetEdgeEntry(index, ref entry);
            this.SetEdgeEntry(num2, ref entry3);
            this.SetEdgeEntry(edge, ref edgeEntry);
            return face;
        }

        public Edge GetEdge(int edgeIndex) => 
            new Edge(this, edgeIndex);

        private EdgeTableEntry GetEdgeEntry(int index) => 
            this.m_edgeTable[index];

        public EdgeEnumerator GetEdges(HashSet<int> preallocatedHelperHashset = null) => 
            new EdgeEnumerator(this, preallocatedHelperHashset);

        public Face GetFace(int faceIndex) => 
            new Face(this, faceIndex);

        private FaceTableEntry GetFaceEntry(int index) => 
            this.m_faceTable[index];

        public VertexEdgeEnumerator GetVertexEdges(int vertexIndex) => 
            new VertexEdgeEnumerator(this, vertexIndex);

        private VertexTableEntry GetVertexEntry(int index) => 
            this.m_vertexTable[index];

        public Vector3 GetVertexPosition(int vertexIndex) => 
            this.m_vertexTable[vertexIndex].VertexPosition;

        public bool IntersectEdge(ref Edge edge, ref Plane p, out Vector3 intersection)
        {
            intersection = new Vector3();
            Ray output = new Ray();
            edge.ToRay(this, ref output);
            float? nullable = output.Intersects(p);
            if (nullable == null)
            {
                return false;
            }
            float num = nullable.Value;
            if ((num < 0f) || (num > 1f))
            {
                return false;
            }
            intersection = output.Position + (num * output.Direction);
            return true;
        }

        public int MakeEdgeFace(int vert1, int vert2, int edge1, int edge2, object faceUserData, out int newEdge)
        {
            EdgeTableEntry entry6;
            newEdge = this.AllocateEdge();
            int face = this.AllocateAndInsertFace(faceUserData, newEdge);
            EdgeTableEntry edgeEntry = this.GetEdgeEntry(edge1);
            EdgeTableEntry entry = this.GetEdgeEntry(edge2);
            EdgeTableEntry entry3 = new EdgeTableEntry();
            entry3.Init();
            entry3.Vertex1 = vert1;
            entry3.Vertex2 = vert2;
            entry3.RightFace = face;
            edgeEntry.AddFace(face);
            entry.AddFace(face);
            int vertexIndex = entry.OtherVertex(vert2);
            for (int i = entry.VertexSucc(vertexIndex); i != edge1; i = entry6.VertexSucc(vertexIndex))
            {
                entry6 = this.GetEdgeEntry(i);
                entry6.AddFace(face);
                this.SetEdgeEntry(i, ref entry6);
                vertexIndex = entry6.OtherVertex(vertexIndex);
            }
            entry3.SetVertexSucc(vert2, edge2);
            int index = entry.SetVertexPred(vert2, newEdge);
            entry3.SetVertexPred(vert1, edge1);
            int num5 = edgeEntry.SetVertexSucc(vert1, newEdge);
            EdgeTableEntry entry4 = this.GetEdgeEntry(num5);
            EdgeTableEntry entry5 = new EdgeTableEntry();
            if (num5 != index)
            {
                entry5 = this.GetEdgeEntry(index);
            }
            entry4.SetVertexPred(vert1, newEdge);
            entry3.SetVertexSucc(vert1, num5);
            if (num5 != index)
            {
                entry5.SetVertexSucc(vert2, newEdge);
            }
            else
            {
                entry4.SetVertexSucc(vert2, newEdge);
            }
            entry3.SetVertexPred(vert2, index);
            this.SetEdgeEntry(num5, ref entry4);
            if (num5 != index)
            {
                this.SetEdgeEntry(index, ref entry5);
            }
            this.SetEdgeEntry(newEdge, ref entry3);
            this.SetEdgeEntry(edge1, ref edgeEntry);
            this.SetEdgeEntry(edge2, ref entry);
            return face;
        }

        public int MakeFace(object userData, int incidentEdge)
        {
            int face = this.AllocateAndInsertFace(userData, incidentEdge);
            int index = incidentEdge;
            while (true)
            {
                EdgeTableEntry edgeEntry = this.GetEdgeEntry(index);
                edgeEntry.AddFace(face);
                this.SetEdgeEntry(index, ref edgeEntry);
                index = edgeEntry.FaceSucc(face);
                if (index == incidentEdge)
                {
                    return face;
                }
            }
        }

        public int MakeNewPoly(object userData, List<Vector3> points, List<int> outEdges)
        {
            if ((outEdges.Count != 0) || (points.Count < 3))
            {
                return INVALID_INDEX;
            }
            m_tmpIndexList.Clear();
            int item = INVALID_INDEX;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 position = points[i];
                item = this.AllocateEdge();
                outEdges.Add(item);
                m_tmpIndexList.Add(this.AllocateAndInsertVertex(ref position, item));
            }
            int face = this.AllocateAndInsertFace(userData, item);
            int newPred = outEdges[points.Count - 1];
            item = outEdges[0];
            int vertex = m_tmpIndexList[0];
            for (int j = 0; j < points.Count; j++)
            {
                int num3;
                int num5;
                if (j != (points.Count - 1))
                {
                    num3 = outEdges[j + 1];
                    num5 = m_tmpIndexList[j + 1];
                }
                else
                {
                    num3 = outEdges[0];
                    num5 = m_tmpIndexList[0];
                }
                EdgeTableEntry entry = new EdgeTableEntry();
                entry.Init();
                entry.AddFace(face);
                entry.SetFacePred(face, newPred);
                entry.SetFaceSucc(face, num3);
                entry.SetFacePred(INVALID_INDEX, num3);
                entry.SetFaceSucc(INVALID_INDEX, newPred);
                entry.SetFacePredVertex(face, vertex);
                entry.SetFaceSuccVertex(face, num5);
                this.SetEdgeEntry(item, ref entry);
                newPred = item;
                item = num3;
                vertex = num5;
            }
            return face;
        }

        public int MakeNewTriangle(object userData, ref Vector3 A, ref Vector3 B, ref Vector3 C, out int edgeAB, out int edgeBC, out int edgeCA)
        {
            edgeAB = this.AllocateEdge();
            edgeBC = this.AllocateEdge();
            edgeCA = this.AllocateEdge();
            int vertex = this.AllocateAndInsertVertex(ref A, edgeAB);
            int num2 = this.AllocateAndInsertVertex(ref B, edgeBC);
            int num3 = this.AllocateAndInsertVertex(ref C, edgeCA);
            int face = this.AllocateAndInsertFace(userData, edgeAB);
            EdgeTableEntry entry = new EdgeTableEntry();
            entry.Init();
            EdgeTableEntry entry2 = new EdgeTableEntry();
            entry2.Init();
            EdgeTableEntry entry3 = new EdgeTableEntry();
            entry3.Init();
            entry.AddFace(face);
            entry2.AddFace(face);
            entry3.AddFace(face);
            entry.SetFaceSuccVertex(face, num2);
            entry2.SetFaceSuccVertex(face, num3);
            entry3.SetFaceSuccVertex(face, vertex);
            entry.SetFacePredVertex(face, vertex);
            entry2.SetFacePredVertex(face, num2);
            entry3.SetFacePredVertex(face, num3);
            entry.SetFaceSucc(face, edgeBC);
            entry2.SetFaceSucc(face, edgeCA);
            entry3.SetFaceSucc(face, edgeAB);
            entry.SetFacePred(face, edgeCA);
            entry2.SetFacePred(face, edgeAB);
            entry3.SetFacePred(face, edgeBC);
            entry.SetFaceSucc(INVALID_INDEX, edgeCA);
            entry2.SetFaceSucc(INVALID_INDEX, edgeAB);
            entry3.SetFaceSucc(INVALID_INDEX, edgeBC);
            entry.SetFacePred(INVALID_INDEX, edgeBC);
            entry2.SetFacePred(INVALID_INDEX, edgeCA);
            entry3.SetFacePred(INVALID_INDEX, edgeAB);
            this.SetEdgeEntry(edgeAB, ref entry);
            this.SetEdgeEntry(edgeBC, ref entry2);
            this.SetEdgeEntry(edgeCA, ref entry3);
            return face;
        }

        public void MergeAngle(int leftEdge, int rightEdge, int commonVert)
        {
            EdgeTableEntry edgeEntry = this.GetEdgeEntry(leftEdge);
            EdgeTableEntry entry = this.GetEdgeEntry(rightEdge);
            int index = edgeEntry.FaceSucc(INVALID_INDEX);
            int num2 = entry.FacePred(INVALID_INDEX);
            int oldVertex = edgeEntry.OtherVertex(commonVert);
            int vertex = entry.OtherVertex(commonVert);
            int faceIndex = edgeEntry.OtherFace(INVALID_INDEX);
            int num6 = edgeEntry.FaceSucc(faceIndex);
            int num7 = edgeEntry.FacePred(faceIndex);
            EdgeTableEntry entry3 = this.GetEdgeEntry(num6);
            EdgeTableEntry entry4 = this.GetEdgeEntry(index);
            EdgeTableEntry entry5 = this.GetEdgeEntry(num2);
            entry4.SetFacePredVertex(INVALID_INDEX, vertex);
            entry4.SetFacePred(INVALID_INDEX, num2);
            entry5.SetFaceSucc(INVALID_INDEX, index);
            if (num7 == index)
            {
                entry4.SetFaceSucc(faceIndex, rightEdge);
            }
            else
            {
                EdgeTableEntry entry8 = this.GetEdgeEntry(num7);
                entry8.SetFaceSucc(faceIndex, rightEdge);
                entry8.ChangeVertex(oldVertex, vertex);
                this.SetEdgeEntry(num7, ref entry8);
                for (int i = entry8.VertexPred(vertex); i != index; i = entry8.VertexPred(vertex))
                {
                    entry8 = this.GetEdgeEntry(i);
                    entry8.ChangeVertex(oldVertex, vertex);
                    this.SetEdgeEntry(i, ref entry8);
                }
            }
            entry.AddFace(faceIndex);
            entry.SetFacePred(faceIndex, num7);
            entry.SetFaceSucc(faceIndex, num6);
            entry3.SetFacePred(faceIndex, rightEdge);
            VertexTableEntry entry6 = this.m_vertexTable[commonVert];
            entry6.IncidentEdge = rightEdge;
            this.m_vertexTable[commonVert] = entry6;
            FaceTableEntry faceEntry = this.GetFaceEntry(faceIndex);
            faceEntry.IncidentEdge = rightEdge;
            this.SetFaceEntry(faceIndex, faceEntry);
            this.DeallocateEdge(leftEdge);
            this.DeallocateVertex(oldVertex);
            this.SetEdgeEntry(rightEdge, ref entry);
            this.SetEdgeEntry(index, ref entry4);
            this.SetEdgeEntry(num6, ref entry3);
            this.SetEdgeEntry(num2, ref entry5);
        }

        public void MergeEdges(int edge1, int edge2)
        {
            int num;
            int num2;
            int num3;
            int num4;
            EdgeTableEntry edgeEntry = this.GetEdgeEntry(edge1);
            EdgeTableEntry entry = this.GetEdgeEntry(edge2);
            edgeEntry.GetFaceVertices(INVALID_INDEX, out num, out num2);
            entry.GetFaceVertices(INVALID_INDEX, out num3, out num4);
            int faceIndex = edgeEntry.OtherFace(INVALID_INDEX);
            int index = edgeEntry.FaceSucc(INVALID_INDEX);
            int num7 = edgeEntry.FacePred(INVALID_INDEX);
            int num8 = entry.FaceSucc(INVALID_INDEX);
            int num9 = entry.FacePred(INVALID_INDEX);
            int newSucc = edgeEntry.FaceSucc(faceIndex);
            int newPred = edgeEntry.FacePred(faceIndex);
            EdgeTableEntry entry3 = this.GetEdgeEntry(index);
            EdgeTableEntry entry4 = new EdgeTableEntry();
            if (index != num7)
            {
                entry4 = this.GetEdgeEntry(num7);
            }
            EdgeTableEntry entry5 = this.GetEdgeEntry(num8);
            EdgeTableEntry entry6 = new EdgeTableEntry();
            if (num8 != num9)
            {
                entry6 = this.GetEdgeEntry(num9);
            }
            entry3.SetFacePred(INVALID_INDEX, num9);
            entry5.SetFacePred(INVALID_INDEX, num7);
            if (index != num7)
            {
                entry4.SetFaceSucc(INVALID_INDEX, num8);
            }
            else
            {
                entry3.SetFaceSucc(INVALID_INDEX, num8);
            }
            if (num8 != num9)
            {
                entry6.SetFaceSucc(INVALID_INDEX, index);
            }
            else
            {
                entry5.SetFaceSucc(INVALID_INDEX, index);
            }
            entry.AddFace(faceIndex);
            entry.SetFacePred(faceIndex, newPred);
            entry.SetFaceSucc(faceIndex, newSucc);
            entry3.SetFacePredVertex(INVALID_INDEX, num3);
            if (index != num7)
            {
                entry4.SetFaceSuccVertex(INVALID_INDEX, num4);
            }
            else
            {
                entry3.SetFaceSuccVertex(INVALID_INDEX, num4);
            }
            if (newPred == index)
            {
                entry3.SetFaceSucc(faceIndex, edge2);
            }
            else
            {
                EdgeTableEntry entry8 = this.GetEdgeEntry(newPred);
                entry8.SetFaceSucc(faceIndex, edge2);
                entry8.SetFaceSuccVertex(faceIndex, num3);
                this.SetEdgeEntry(newPred, ref entry8);
                for (newPred = entry8.VertexPred(num3); newPred != index; newPred = entry8.VertexPred(num3))
                {
                    entry8 = this.GetEdgeEntry(newPred);
                    entry8.ChangeVertex(num2, num3);
                    this.SetEdgeEntry(newPred, ref entry8);
                }
            }
            if (newSucc == num7)
            {
                if (index != num7)
                {
                    entry4.SetFacePred(faceIndex, edge2);
                }
                else
                {
                    entry3.SetFacePred(faceIndex, edge2);
                }
            }
            else
            {
                EdgeTableEntry entry9 = this.GetEdgeEntry(newSucc);
                entry9.SetFacePred(faceIndex, edge2);
                entry9.SetFacePredVertex(faceIndex, num4);
                this.SetEdgeEntry(newSucc, ref entry9);
                for (newSucc = entry9.VertexSucc(num4); newSucc != num7; newSucc = entry9.VertexSucc(num4))
                {
                    entry9 = this.GetEdgeEntry(newSucc);
                    entry9.ChangeVertex(num, num4);
                    this.SetEdgeEntry(newSucc, ref entry9);
                }
            }
            FaceTableEntry faceEntry = this.GetFaceEntry(faceIndex);
            faceEntry.IncidentEdge = edge2;
            this.SetFaceEntry(faceIndex, faceEntry);
            this.DeallocateVertex(num);
            this.DeallocateVertex(num2);
            this.DeallocateEdge(edge1);
            this.SetEdgeEntry(edge2, ref entry);
            this.SetEdgeEntry(index, ref entry3);
            if (index != num7)
            {
                this.SetEdgeEntry(num7, ref entry4);
            }
            this.SetEdgeEntry(num8, ref entry5);
            if (num8 != num9)
            {
                this.SetEdgeEntry(num9, ref entry6);
            }
        }

        [Conditional("DEBUG")]
        public void PrepareFreeEdgeHashset()
        {
            EdgeTableEntry entry;
            m_tmpFreeEdges.Clear();
            for (int i = this.m_freeEdges; i != INVALID_INDEX; i = entry.NextFreeEntry)
            {
                m_tmpFreeEdges.Add(i);
                entry = this.m_edgeTable[i];
            }
        }

        [Conditional("DEBUG")]
        public void PrepareFreeFaceHashset()
        {
            FaceTableEntry entry;
            m_tmpFreeFaces.Clear();
            for (int i = this.m_freeFaces; i != INVALID_INDEX; i = entry.NextFreeEntry)
            {
                m_tmpFreeFaces.Add(i);
                entry = this.m_faceTable[i];
            }
        }

        [Conditional("DEBUG")]
        public void PrepareFreeVertexHashset()
        {
            VertexTableEntry entry;
            m_tmpFreeVertices.Clear();
            for (int i = this.m_freeVertices; i != INVALID_INDEX; i = entry.NextFreeEntry)
            {
                m_tmpFreeVertices.Add(i);
                entry = this.m_vertexTable[i];
            }
        }

        public void RemoveFace(int faceIndex)
        {
            int num3;
            int num4;
            int incidentEdge = this.GetFaceEntry(faceIndex).IncidentEdge;
            int index = incidentEdge;
            bool flag = false;
            EdgeTableEntry edgeEntry = this.GetEdgeEntry(incidentEdge);
            EdgeTableEntry entry2 = this.GetEdgeEntry(index);
            edgeEntry.GetFaceVertices(faceIndex, out num3, out num4);
            while (true)
            {
                int num5 = edgeEntry.FaceSucc(faceIndex);
                num3 = num4;
                EdgeTableEntry entry = ((num5 == index) & flag) ? entry2 : this.GetEdgeEntry(num5);
                num4 = entry.OtherVertex(num4);
                if (edgeEntry.VertexLeftFace(num3) == INVALID_INDEX)
                {
                    if (incidentEdge == index)
                    {
                        flag = true;
                    }
                    this.DeallocateEdge(incidentEdge);
                    if (entry.VertexLeftFace(num4) != INVALID_INDEX)
                    {
                        int num8 = edgeEntry.FacePred(INVALID_INDEX);
                        EdgeTableEntry entry7 = this.GetEdgeEntry(num8);
                        entry7.SetFaceSucc(INVALID_INDEX, num5);
                        if (num5 != index)
                        {
                            entry.SetFacePred(faceIndex, num8);
                        }
                        else
                        {
                            entry.SetFacePred(INVALID_INDEX, num8);
                        }
                        this.SetEdgeEntry(num8, ref entry7);
                        this.SetEdgeEntry(num5, ref entry);
                        VertexTableEntry entry8 = this.m_vertexTable[num3];
                        entry8.IncidentEdge = num5;
                        this.m_vertexTable[num3] = entry8;
                    }
                    else if (entry.VertexSucc(num3) == incidentEdge)
                    {
                        this.DeallocateVertex(num3);
                    }
                    else
                    {
                        int num6 = entry.VertexSucc(num3);
                        int num7 = edgeEntry.VertexPred(num3);
                        EdgeTableEntry entry4 = this.GetEdgeEntry(num6);
                        EdgeTableEntry entry5 = this.GetEdgeEntry(num7);
                        entry4.SetVertexPred(num3, num7);
                        entry5.SetVertexSucc(num3, num6);
                        this.SetEdgeEntry(num7, ref entry5);
                        this.SetEdgeEntry(num6, ref entry4);
                        VertexTableEntry entry6 = this.m_vertexTable[num3];
                        entry6.IncidentEdge = num7;
                        this.m_vertexTable[num3] = entry6;
                    }
                }
                else if (entry.VertexLeftFace(num4) == INVALID_INDEX)
                {
                    int num9 = entry.FaceSucc(INVALID_INDEX);
                    EdgeTableEntry entry9 = this.GetEdgeEntry(num9);
                    entry9.SetFacePred(INVALID_INDEX, incidentEdge);
                    edgeEntry.SetFaceSucc(faceIndex, num9);
                    edgeEntry.ChangeFace(faceIndex, INVALID_INDEX);
                    this.SetEdgeEntry(num9, ref entry9);
                    this.SetEdgeEntry(incidentEdge, ref edgeEntry);
                    VertexTableEntry entry10 = this.m_vertexTable[num3];
                    entry10.IncidentEdge = incidentEdge;
                    this.m_vertexTable[num3] = entry10;
                }
                else
                {
                    int num10 = entry.VertexSucc(num3);
                    while (true)
                    {
                        if (num10 != incidentEdge)
                        {
                            EdgeTableEntry entry11 = this.GetEdgeEntry(num10);
                            if (entry11.VertexRightFace(num3) != INVALID_INDEX)
                            {
                                num10 = entry11.VertexSucc(num3);
                                continue;
                            }
                            int num11 = entry11.VertexSucc(num3);
                            VertexTableEntry entry12 = this.m_vertexTable[num3];
                            Vector3 vertexPosition = entry12.VertexPosition;
                            entry12.IncidentEdge = num5;
                            this.m_vertexTable[num3] = entry12;
                            int newVertex = this.AllocateAndInsertVertex(ref vertexPosition, num11);
                            EdgeTableEntry entry13 = this.GetEdgeEntry(num11);
                            entry13.SetVertexPred(num3, incidentEdge);
                            edgeEntry.SetVertexSucc(num3, num11);
                            edgeEntry.ChangeVertex(num3, newVertex);
                            while (true)
                            {
                                entry13.ChangeVertex(num3, newVertex);
                                this.SetEdgeEntry(num11, ref entry13);
                                num11 = entry13.VertexSucc(newVertex);
                                if (num11 == incidentEdge)
                                {
                                    entry11.SetVertexSucc(num3, num5);
                                    entry.SetVertexPred(num3, num10);
                                    this.SetEdgeEntry(num10, ref entry11);
                                    this.SetEdgeEntry(num5, ref entry);
                                    break;
                                }
                                entry13 = this.GetEdgeEntry(num11);
                            }
                        }
                        edgeEntry.ChangeFace(faceIndex, INVALID_INDEX);
                        this.SetEdgeEntry(incidentEdge, ref edgeEntry);
                        break;
                    }
                }
                incidentEdge = num5;
                edgeEntry = entry;
                if (incidentEdge == index)
                {
                    this.DeallocateFace(faceIndex);
                    return;
                }
            }
        }

        private void SetEdgeEntry(int index, ref EdgeTableEntry entry)
        {
            this.m_edgeTable[index] = entry;
        }

        private void SetFaceEntry(int index, FaceTableEntry entry)
        {
            this.m_faceTable[index] = entry;
        }

        public void SortFreeFaces()
        {
            if (this.m_freeFaces != INVALID_INDEX)
            {
                FaceTableEntry entry2;
                m_tmpIndexList.Clear();
                for (int i = this.m_freeFaces; i != INVALID_INDEX; i = entry2.NextFreeEntry)
                {
                    m_tmpIndexList.Add(i);
                    entry2 = this.m_faceTable[i];
                }
                m_tmpIndexList.Sort();
                this.m_freeFaces = m_tmpIndexList[0];
                for (int j = 0; j < (m_tmpIndexList.Count - 1); j++)
                {
                    FaceTableEntry entry3 = this.m_faceTable[m_tmpIndexList[j]];
                    entry3.NextFreeEntry = m_tmpIndexList[j + 1];
                    this.m_faceTable[m_tmpIndexList[j]] = entry3;
                }
                FaceTableEntry entry = this.m_faceTable[m_tmpIndexList[m_tmpIndexList.Count - 1]];
                entry.NextFreeEntry = INVALID_INDEX;
                this.m_faceTable[m_tmpIndexList[m_tmpIndexList.Count - 1]] = entry;
                m_tmpIndexList.Clear();
            }
        }

        public void Transform(Matrix transformation)
        {
            int num;
            VertexTableEntry entry;
            m_tmpFreeVertices.Clear();
            for (num = this.m_freeVertices; num != INVALID_INDEX; num = entry.NextFreeEntry)
            {
                m_tmpFreeVertices.Add(num);
                entry = this.m_vertexTable[num];
            }
            for (num = 0; num < this.m_vertexTable.Count; num++)
            {
                if (!m_tmpFreeVertices.Contains(num))
                {
                    VertexTableEntry entry2 = this.m_vertexTable[num];
                    Vector3.Transform(ref entry2.VertexPosition, ref transformation, out entry2.VertexPosition);
                    this.m_vertexTable[num] = entry2;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Edge
        {
            private MyWingedEdgeMesh.EdgeTableEntry m_entry;
            private int m_index;
            public int LeftFace =>
                this.m_entry.LeftFace;
            public int RightFace =>
                this.m_entry.RightFace;
            public int Vertex1 =>
                this.m_entry.Vertex1;
            public int Vertex2 =>
                this.m_entry.Vertex2;
            public int Index =>
                this.m_index;
            public Edge(MyWingedEdgeMesh mesh, int index)
            {
                this.m_entry = mesh.GetEdgeEntry(index);
                this.m_index = index;
            }

            public int TryGetSharedVertex(ref MyWingedEdgeMesh.Edge other) => 
                this.m_entry.TryGetSharedVertex(ref other.m_entry);

            public int GetFacePredVertex(int face) => 
                this.m_entry.GetFacePredVertex(face);

            public int GetFaceSuccVertex(int face) => 
                this.m_entry.GetFaceSuccVertex(face);

            public int OtherVertex(int vertex) => 
                this.m_entry.OtherVertex(vertex);

            public int OtherFace(int face) => 
                this.m_entry.OtherFace(face);

            public int GetPreviousFaceEdge(int faceIndex) => 
                this.m_entry.FacePred(faceIndex);

            public int GetNextFaceEdge(int faceIndex) => 
                this.m_entry.FaceSucc(faceIndex);

            public int GetNextVertexEdge(int vertexIndex) => 
                this.m_entry.VertexPred(vertexIndex);

            public int VertexLeftFace(int vertexIndex) => 
                this.m_entry.VertexLeftFace(vertexIndex);

            public void ToRay(MyWingedEdgeMesh mesh, ref Ray output)
            {
                Vector3 vertexPosition = mesh.GetVertexPosition(this.Vertex1);
                Vector3 vector2 = mesh.GetVertexPosition(this.Vertex2);
                output.Position = vertexPosition;
                output.Direction = vector2 - vertexPosition;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EdgeEnumerator
        {
            private int m_currentEdge;
            private HashSet<int> m_freeEdges;
            private MyWingedEdgeMesh m_mesh;
            public EdgeEnumerator(MyWingedEdgeMesh mesh, HashSet<int> preallocatedHelperHashSet = null)
            {
                MyWingedEdgeMesh.EdgeTableEntry entry;
                this.m_currentEdge = -1;
                this.m_freeEdges = preallocatedHelperHashSet ?? new HashSet<int>();
                this.m_mesh = mesh;
                this.m_freeEdges.Clear();
                for (int i = mesh.m_freeEdges; i != MyWingedEdgeMesh.INVALID_INDEX; i = entry.NextFreeEntry)
                {
                    this.m_freeEdges.Add(i);
                    entry = this.m_mesh.m_edgeTable[i];
                }
            }

            public int CurrentIndex =>
                this.m_currentEdge;
            public MyWingedEdgeMesh.Edge Current =>
                new MyWingedEdgeMesh.Edge(this.m_mesh, this.m_currentEdge);
            public bool MoveNext()
            {
                int count = this.m_mesh.m_edgeTable.Count;
                while (true)
                {
                    this.m_currentEdge++;
                    if (!this.m_freeEdges.Contains(this.m_currentEdge) || (this.m_currentEdge >= count))
                    {
                        return (this.m_currentEdge < count);
                    }
                }
            }

            public void Dispose()
            {
                this.m_freeEdges.Clear();
                this.m_freeEdges = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EdgeTableEntry
        {
            public int Vertex1;
            public int Vertex2;
            public int LeftFace;
            public int RightFace;
            public int LeftPred;
            public int LeftSucc;
            public int RightPred;
            public int RightSucc;
            public int NextFreeEntry
            {
                get => 
                    this.Vertex1;
                set => 
                    (this.Vertex1 = value);
            }
            public void Init()
            {
                this.Vertex1 = MyWingedEdgeMesh.INVALID_INDEX;
                this.Vertex2 = MyWingedEdgeMesh.INVALID_INDEX;
                this.LeftFace = MyWingedEdgeMesh.INVALID_INDEX;
                this.RightFace = MyWingedEdgeMesh.INVALID_INDEX;
                this.LeftPred = MyWingedEdgeMesh.INVALID_INDEX;
                this.LeftSucc = MyWingedEdgeMesh.INVALID_INDEX;
                this.RightPred = MyWingedEdgeMesh.INVALID_INDEX;
                this.RightSucc = MyWingedEdgeMesh.INVALID_INDEX;
            }

            public int OtherVertex(int vert) => 
                ((vert == this.Vertex1) ? this.Vertex2 : this.Vertex1);

            public void GetFaceVertices(int face, out int predVertex, out int succVertex)
            {
                if (face == this.LeftFace)
                {
                    predVertex = this.Vertex2;
                    succVertex = this.Vertex1;
                }
                else
                {
                    predVertex = this.Vertex1;
                    succVertex = this.Vertex2;
                }
            }

            public int GetFaceSuccVertex(int face) => 
                ((face != this.LeftFace) ? this.Vertex2 : this.Vertex1);

            public int GetFacePredVertex(int face) => 
                ((face != this.LeftFace) ? this.Vertex1 : this.Vertex2);

            public void SetFaceSuccVertex(int face, int vertex)
            {
                if (face == this.LeftFace)
                {
                    this.Vertex1 = vertex;
                }
                else
                {
                    this.Vertex2 = vertex;
                }
            }

            public void SetFacePredVertex(int face, int vertex)
            {
                if (face == this.LeftFace)
                {
                    this.Vertex2 = vertex;
                }
                else
                {
                    this.Vertex1 = vertex;
                }
            }

            public int TryGetSharedVertex(ref MyWingedEdgeMesh.EdgeTableEntry otherEdge) => 
                ((this.Vertex1 != otherEdge.Vertex1) ? ((this.Vertex1 != otherEdge.Vertex2) ? ((this.Vertex2 != otherEdge.Vertex1) ? ((this.Vertex2 != otherEdge.Vertex2) ? MyWingedEdgeMesh.INVALID_INDEX : this.Vertex2) : this.Vertex2) : this.Vertex1) : this.Vertex1);

            public void ChangeVertex(int oldVertex, int newVertex)
            {
                if (oldVertex == this.Vertex1)
                {
                    this.Vertex1 = newVertex;
                }
                else
                {
                    this.Vertex2 = newVertex;
                }
            }

            public int OtherFace(int face) => 
                ((face != this.LeftFace) ? this.LeftFace : this.RightFace);

            public int VertexLeftFace(int vertex) => 
                ((vertex == this.Vertex1) ? this.RightFace : this.LeftFace);

            public int VertexRightFace(int vertex) => 
                ((vertex == this.Vertex1) ? this.LeftFace : this.RightFace);

            public void AddFace(int face)
            {
                if (this.LeftFace == MyWingedEdgeMesh.INVALID_INDEX)
                {
                    this.LeftFace = face;
                }
                else
                {
                    this.RightFace = face;
                }
            }

            public void ChangeFace(int previousFace, int newFace)
            {
                if (previousFace == this.LeftFace)
                {
                    this.LeftFace = newFace;
                }
                else
                {
                    this.RightFace = newFace;
                }
            }

            public int FaceSucc(int faceIndex) => 
                ((faceIndex != this.LeftFace) ? this.RightSucc : this.LeftSucc);

            public void SetFaceSucc(int faceIndex, int newSucc)
            {
                if (faceIndex == this.LeftFace)
                {
                    this.LeftSucc = newSucc;
                }
                else
                {
                    this.RightSucc = newSucc;
                }
            }

            public int FacePred(int faceIndex) => 
                ((faceIndex != this.LeftFace) ? this.RightPred : this.LeftPred);

            public void SetFacePred(int faceIndex, int newPred)
            {
                if (faceIndex == this.LeftFace)
                {
                    this.LeftPred = newPred;
                }
                else
                {
                    this.RightPred = newPred;
                }
            }

            public int VertexSucc(int vertexIndex) => 
                ((vertexIndex == this.Vertex1) ? this.LeftSucc : this.RightSucc);

            public int SetVertexSucc(int vertexIndex, int newSucc)
            {
                int leftSucc = MyWingedEdgeMesh.INVALID_INDEX;
                if (vertexIndex == this.Vertex1)
                {
                    leftSucc = this.LeftSucc;
                    this.LeftSucc = newSucc;
                }
                else
                {
                    leftSucc = this.RightSucc;
                    this.RightSucc = newSucc;
                }
                return leftSucc;
            }

            public int VertexPred(int vertexIndex) => 
                ((vertexIndex == this.Vertex1) ? this.RightPred : this.LeftPred);

            public int SetVertexPred(int vertexIndex, int newPred)
            {
                int rightPred = MyWingedEdgeMesh.INVALID_INDEX;
                if (vertexIndex == this.Vertex1)
                {
                    rightPred = this.RightPred;
                    this.RightPred = newPred;
                }
                else
                {
                    rightPred = this.LeftPred;
                    this.LeftPred = newPred;
                }
                return rightPred;
            }

            public override string ToString()
            {
                object[] objArray1 = new object[0x10];
                objArray1[0] = "V: ";
                objArray1[1] = this.Vertex1;
                objArray1[2] = ", ";
                objArray1[3] = this.Vertex2;
                objArray1[4] = "; Left (Pred, Face, Succ): ";
                objArray1[5] = this.LeftPred;
                objArray1[6] = ", ";
                objArray1[7] = this.LeftFace;
                objArray1[8] = ", ";
                objArray1[9] = this.LeftSucc;
                objArray1[10] = "; Right (Pred, Face, Succ): ";
                objArray1[11] = this.RightPred;
                objArray1[12] = ", ";
                objArray1[13] = this.RightFace;
                objArray1[14] = ", ";
                objArray1[15] = this.RightSucc;
                return string.Concat(objArray1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Face
        {
            private int m_faceIndex;
            private MyWingedEdgeMesh m_mesh;
            public Face(MyWingedEdgeMesh mesh, int index)
            {
                this.m_mesh = mesh;
                this.m_faceIndex = index;
            }

            public MyWingedEdgeMesh.FaceEdgeEnumerator GetEnumerator() => 
                new MyWingedEdgeMesh.FaceEdgeEnumerator(this.m_mesh, this.m_faceIndex);

            public MyWingedEdgeMesh.FaceVertexEnumerator GetVertexEnumerator() => 
                new MyWingedEdgeMesh.FaceVertexEnumerator(this.m_mesh, this.m_faceIndex);

            public T GetUserData<T>() where T: class => 
                (this.m_mesh.GetFaceEntry(this.m_faceIndex).UserData as T);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FaceEdgeEnumerator
        {
            private MyWingedEdgeMesh m_mesh;
            private int m_faceIndex;
            private int m_currentEdge;
            private int m_startingEdge;
            public int Current =>
                this.m_currentEdge;
            public FaceEdgeEnumerator(MyWingedEdgeMesh mesh, int faceIndex)
            {
                this.m_mesh = mesh;
                this.m_faceIndex = faceIndex;
                this.m_currentEdge = MyWingedEdgeMesh.INVALID_INDEX;
                this.m_startingEdge = this.m_mesh.GetFaceEntry(faceIndex).IncidentEdge;
            }

            public bool MoveNext()
            {
                if (this.m_currentEdge == MyWingedEdgeMesh.INVALID_INDEX)
                {
                    this.m_currentEdge = this.m_startingEdge;
                    return true;
                }
                this.m_currentEdge = this.m_mesh.GetEdgeEntry(this.m_currentEdge).FaceSucc(this.m_faceIndex);
                return (this.m_currentEdge != this.m_startingEdge);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FaceTableEntry
        {
            public int IncidentEdge;
            public object UserData;
            public int NextFreeEntry
            {
                get => 
                    this.IncidentEdge;
                set => 
                    (this.IncidentEdge = value);
            }
            public override string ToString() => 
                ("-> " + this.IncidentEdge.ToString());
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FaceVertexEnumerator
        {
            private MyWingedEdgeMesh m_mesh;
            private int m_faceIndex;
            private int m_currentEdge;
            private int m_startingEdge;
            public Vector3 Current
            {
                get
                {
                    MyWingedEdgeMesh.EdgeTableEntry edgeEntry = this.m_mesh.GetEdgeEntry(this.m_currentEdge);
                    return ((this.m_faceIndex != edgeEntry.LeftFace) ? this.m_mesh.m_vertexTable[edgeEntry.Vertex1].VertexPosition : this.m_mesh.m_vertexTable[edgeEntry.Vertex2].VertexPosition);
                }
            }
            public int CurrentIndex
            {
                get
                {
                    MyWingedEdgeMesh.EdgeTableEntry edgeEntry = this.m_mesh.GetEdgeEntry(this.m_currentEdge);
                    return ((this.m_faceIndex != edgeEntry.LeftFace) ? edgeEntry.Vertex1 : edgeEntry.Vertex2);
                }
            }
            public FaceVertexEnumerator(MyWingedEdgeMesh mesh, int faceIndex)
            {
                this.m_mesh = mesh;
                this.m_faceIndex = faceIndex;
                this.m_currentEdge = MyWingedEdgeMesh.INVALID_INDEX;
                this.m_startingEdge = this.m_mesh.GetFaceEntry(faceIndex).IncidentEdge;
            }

            public bool MoveNext()
            {
                if (this.m_currentEdge == MyWingedEdgeMesh.INVALID_INDEX)
                {
                    this.m_currentEdge = this.m_startingEdge;
                    return true;
                }
                this.m_currentEdge = this.m_mesh.GetEdgeEntry(this.m_currentEdge).FaceSucc(this.m_faceIndex);
                return (this.m_currentEdge != this.m_startingEdge);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexEdgeEnumerator
        {
            private int m_vertexIndex;
            private int m_startingEdge;
            private int m_currentEdgeIndex;
            private MyWingedEdgeMesh m_mesh;
            private MyWingedEdgeMesh.Edge m_currentEdge;
            public int CurrentIndex =>
                this.m_currentEdgeIndex;
            public MyWingedEdgeMesh.Edge Current =>
                this.m_currentEdge;
            public VertexEdgeEnumerator(MyWingedEdgeMesh mesh, int index)
            {
                this.m_vertexIndex = index;
                MyWingedEdgeMesh.VertexTableEntry vertexEntry = mesh.GetVertexEntry(this.m_vertexIndex);
                this.m_startingEdge = vertexEntry.IncidentEdge;
                this.m_mesh = mesh;
                this.m_currentEdgeIndex = MyWingedEdgeMesh.INVALID_INDEX;
                this.m_currentEdge = new MyWingedEdgeMesh.Edge();
            }

            public bool MoveNext()
            {
                if (this.m_currentEdgeIndex == MyWingedEdgeMesh.INVALID_INDEX)
                {
                    this.m_currentEdgeIndex = this.m_startingEdge;
                    this.m_currentEdge = this.m_mesh.GetEdge(this.m_startingEdge);
                    return true;
                }
                int nextVertexEdge = this.m_currentEdge.GetNextVertexEdge(this.m_vertexIndex);
                if (nextVertexEdge == this.m_startingEdge)
                {
                    return false;
                }
                this.m_currentEdgeIndex = nextVertexEdge;
                this.m_currentEdge = this.m_mesh.GetEdge(this.m_currentEdgeIndex);
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexTableEntry
        {
            public int IncidentEdge;
            public Vector3 VertexPosition;
            public int NextFreeEntry
            {
                get => 
                    this.IncidentEdge;
                set => 
                    (this.IncidentEdge = value);
            }
            public override string ToString() => 
                (this.VertexPosition.ToString() + " -> " + this.IncidentEdge);
        }
    }
}


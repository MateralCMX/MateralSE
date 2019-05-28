namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Stats;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    internal class MyOndraSimulator3 : IMyIntegritySimulator
    {
        public static string[] Algs = new string[] { "Stat", "Dyn", "Adv", "AdvS" };
        public static int AlgIndex = 3;
        private static float m_blurAmount = 0.057f;
        private static float m_closestDistanceThreshold = 3f;
        private static float m_blurIterations = 7f;
        private static bool m_blurEnabled = false;
        private static bool m_blurStaticShareSupport = true;
        private bool m_needsRecalc;
        public static bool DrawText = true;
        private static Vector3I[] Offsets = new Vector3I[] { new Vector3I(0, 0, 1), new Vector3I(0, 1, 0), new Vector3I(1, 0, 0), new Vector3I(0, 0, -1), new Vector3I(0, -1, 0), new Vector3I(-1, 0, 0) };
        private static float SideRatio = 0.25f;
        private static float[] DirectionRatios = new float[] { SideRatio, 1f, SideRatio };
        private MyCubeGrid m_grid;
        private int BlockCount;
        private float TotalMax;
        private HashSet<Node> StaticBlocks = new HashSet<Node>();
        private HashSet<Node> DynamicBlocks = new HashSet<Node>();
        private Dictionary<Vector3I, Node> All = new Dictionary<Vector3I, Node>();
        private int m_frameCounter;
        private int m_lastFrameCollision;
        private int DYNAMIC_UPDATE_DELAY = 3;
        private Dictionary<VRage.Game.Entity.MyEntity, CollidingEntityInfo> m_collidingEntities = new Dictionary<VRage.Game.Entity.MyEntity, CollidingEntityInfo>();
        private Dictionary<Vector3I, float> DynamicWeights = new Dictionary<Vector3I, float>();
        private HashSet<MyCubeGrid> m_constrainedGrid = new HashSet<MyCubeGrid>();
        private static Vector3I m_selectedCube;
        private static MyCubeGrid m_selectedGrid;
        private static float SlidingOffset = 0f;

        public MyOndraSimulator3(MyCubeGrid grid)
        {
            this.m_grid = grid;
            m_selectedGrid = this.m_grid;
            SelectedCube = this.m_grid.GetBlocks().First<MySlimBlock>().Position;
        }

        public void Add(MySlimBlock block)
        {
        }

        private void AddConstrainedGrids(MyCubeGrid collidingGrid)
        {
            foreach (HkConstraint local1 in collidingGrid.Physics.Constraints)
            {
                MyCubeGrid entity = local1.RigidBodyA.GetBody().Entity as MyCubeGrid;
                MyCubeGrid item = local1.RigidBodyB.GetBody().Entity as MyCubeGrid;
                if (!this.m_constrainedGrid.Contains(entity))
                {
                    this.m_constrainedGrid.Add(entity);
                    this.AddConstrainedGrids(entity);
                }
                if (!this.m_constrainedGrid.Contains(item))
                {
                    this.m_constrainedGrid.Add(item);
                    this.AddConstrainedGrids(item);
                }
            }
        }

        private void AddNeighbours()
        {
            using (Stats.Timing.Measure("SI - AddNeighbours", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
            {
                foreach (KeyValuePair<Vector3I, Node> pair in this.All)
                {
                    foreach (Vector3I vectori in Offsets)
                    {
                        Node node;
                        if (this.All.TryGetValue(pair.Value.Pos + vectori, out node))
                        {
                            pair.Value.Neighbours.Add(node);
                        }
                    }
                }
            }
        }

        public void Close()
        {
        }

        public void DebugDraw()
        {
            SlidingOffset += 0.005f;
            if (MyPetaInputComponent.DEBUG_DRAW_PATHS)
            {
                Color aqua = Color.Aqua;
                this.DrawCube(this.m_grid.GridSize, SelectedCube, ref aqua, null);
                if (this.All.ContainsKey(SelectedCube))
                {
                    Node node = this.All[SelectedCube];
                    float offset = (-0.2f * node.Paths.Count) / 2f;
                    float num2 = 0.2f;
                    int index = 0;
                    float num4 = 0f;
                    foreach (KeyValuePair<Node, PathInfo> pair in node.Paths)
                    {
                        this.DebugDrawPath(pair.Value, offset, index);
                        offset += num2;
                        index++;
                        num4 += pair.Value.Ratio;
                        if (pair.Value.Parents.Count > 1)
                        {
                            foreach (PathInfo info in pair.Value.Parents)
                            {
                                this.DebugDrawPath(info, offset, index);
                            }
                        }
                    }
                    MyRenderProxy.DebugDrawText3D(Vector3D.Transform((Vector3) (node.Pos * this.m_grid.GridSize), this.m_grid.WorldMatrix) + (Vector3D.Up / 2.0), SelectedCube.ToString(), Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                }
            }
            if (MyPetaInputComponent.DEBUG_DRAW_TENSIONS)
            {
                float gridSize = this.m_grid.GridSize;
                if (1 != 0)
                {
                    foreach (KeyValuePair<Vector3I, Node> pair2 in this.All)
                    {
                        Color gray = Color.Gray;
                        float offset = 0f;
                        if (!pair2.Value.IsDynamicWeight)
                        {
                            if (!pair2.Value.IsStatic)
                            {
                                offset = pair2.Value.TotalSupportingWeight / pair2.Value.Mass;
                                gray = GetTension(offset, 10f);
                            }
                            this.DrawCube(gridSize, pair2.Key, ref gray, offset.ToString("0.00"));
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<Vector3I, Node> pair3 in this.All)
                    {
                        Color tension = GetTension(pair3.Value.Mass, 4f);
                        string text = pair3.Value.Mass.ToString("0.00");
                        this.DrawCube(gridSize, pair3.Key, ref tension, text);
                    }
                }
            }
        }

        private void DebugDrawPath(PathInfo pathInfo, float offset, int index)
        {
        }

        public void Draw()
        {
        }

        private void DrawCube(float size, Vector3I pos, ref Color color, string text)
        {
            Matrix matrix = (Matrix.CreateScale((float) (size * 1.02f)) * Matrix.CreateTranslation((Vector3) (pos * size))) * this.m_grid.WorldMatrix;
            MyRenderProxy.DebugDrawOBB(matrix, color.ToVector3(), 0.5f, true, true, true, false);
            if ((DrawText && ((text != null) && (text != "0.00"))) && (Vector3D.Distance(matrix.Translation, MySector.MainCamera.Position) < 30.0))
            {
                MyRenderProxy.DebugDrawText3D(matrix.Translation, text, Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            }
        }

        private static void DrawSlidingLine(Vector3D startPosition, Vector3D endPosition, Color color1, Color color2)
        {
            float num = 0.1f;
            float num2 = Vector3.Distance((Vector3) startPosition, (Vector3) endPosition);
            Vector3D vectord = Vector3D.Normalize(endPosition - startPosition);
            Vector3D vectord2 = vectord * num;
            Color colorFrom = color1;
            Vector3D min = Vector3D.Min(startPosition, endPosition);
            Vector3D max = Vector3D.Max(startPosition, endPosition);
            float num3 = (SlidingOffset - ((num * 2f) * ((int) (SlidingOffset / (num * 2f))))) - (2f * num);
            Vector3D vectord5 = startPosition + (num3 * vectord);
            float num4 = 0f;
            while (num4 < num2)
            {
                MyRenderProxy.DebugDrawLine3D(Vector3D.Clamp(vectord5, min, max), Vector3D.Clamp(vectord5 + vectord2, min, max), colorFrom, colorFrom, false, false);
                colorFrom = !(colorFrom == color1) ? color1 : color2;
                num4 += num;
                vectord5 += vectord2;
            }
        }

        private unsafe void FindAndCaculateFromAdvanced()
        {
            HashSet<Node> set = new HashSet<Node>();
            List<Node> list = new List<Node>();
            Queue<Node> queue = new Queue<Node>();
            Queue<Node> queue2 = new Queue<Node>();
            foreach (Node node in this.DynamicBlocks)
            {
                set.Clear();
                list.Clear();
                queue2.Clear();
                node.PathCount = 1;
                int num = 2;
                int num2 = 0x7fffffff;
                node.Distance = 0;
                node.Parents.Clear();
                set.Add(node);
                queue2.Enqueue(node);
                while (true)
                {
                    if (queue2.Count > 0)
                    {
                        Node item = queue2.Dequeue();
                        item.Ratio = 1f;
                        if (item.Distance <= num2)
                        {
                            foreach (Node node3 in item.Neighbours)
                            {
                                if (!set.Add(node3))
                                {
                                    if (node3.Distance != (item.Distance + 1))
                                    {
                                        continue;
                                    }
                                    node3.Parents.Add(item);
                                    continue;
                                }
                                node3.Parents.Clear();
                                node3.Parents.Add(item);
                                node3.Distance = item.Distance + 1;
                                if (!node3.IsStatic)
                                {
                                    queue2.Enqueue(node3);
                                    continue;
                                }
                                list.Add(node3);
                                if (num2 == 0x7fffffff)
                                {
                                    num2 = node3.Distance + num;
                                }
                            }
                            continue;
                        }
                    }
                    float num3 = 0f;
                    if (list.Count > 1)
                    {
                        foreach (Node node4 in list)
                        {
                            float num4 = 0f;
                            foreach (Node node5 in list)
                            {
                                num4 += MyUtils.GetAngleBetweenVectorsAndNormalise((Vector3) (node.Pos - node4.Pos), (Vector3) (node.Pos - node5.Pos));
                            }
                            node4.Ratio = num4;
                            num3 += num4;
                        }
                        foreach (Node local1 in list)
                        {
                            local1.Ratio /= num3;
                        }
                        break;
                    }
                    using (List<Node>.Enumerator enumerator2 = list.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.Ratio = 1f;
                        }
                    }
                    break;
                }
                foreach (Node node6 in list)
                {
                    set.Clear();
                    queue.Clear();
                    node6.PathCount = 1;
                    queue.Enqueue(node6);
                    while (queue.Count > 0)
                    {
                        Node node7 = queue.Dequeue();
                        float num5 = node7.Ratio / ((float) node7.Parents.Count);
                        foreach (Node node8 in node7.Parents)
                        {
                            int num6;
                            int num7;
                            Vector3I vectori = node8.Pos - node7.Pos;
                            if (((vectori.X + vectori.Y) + vectori.Z) > 0)
                            {
                                num6 = vectori.Y + (vectori.Z * 2);
                                num7 = num6 + 3;
                            }
                            else
                            {
                                num6 = ((-vectori.X * 3) - (vectori.Y * 4)) - (vectori.Z * 5);
                                num7 = num6 - 3;
                            }
                            float* singlePtr1 = (float*) ref node7.SupportingWeights[num6];
                            singlePtr1[0] -= node.Mass * num5;
                            float* singlePtr2 = (float*) ref node8.SupportingWeights[num7];
                            singlePtr2[0] += node.Mass * num5;
                            if (!set.Add(node8))
                            {
                                node8.Ratio += num5;
                            }
                            else
                            {
                                node8.Ratio = num5;
                                queue.Enqueue(node8);
                            }
                        }
                    }
                }
            }
        }

        private unsafe void FindAndCaculateFromAdvancedStatic()
        {
            HashSet<Node>.Enumerator enumerator;
            int closestDistanceThreshold = (int) ClosestDistanceThreshold;
            Queue<PathInfo> queue = new Queue<PathInfo>();
            using (enumerator = this.DynamicBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Distance = 0x7fffffff;
                }
            }
            enumerator = this.StaticBlocks.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Node current = enumerator.Current;
                    current.Distance = 0;
                    PathInfo info1 = new PathInfo();
                    info1.EndNode = current;
                    info1.StartNode = current;
                    info1.Distance = 0;
                    info1.DirectionRatio = 1f;
                    PathInfo item = info1;
                    queue.Enqueue(item);
                }
            }
            finally
            {
                enumerator.Dispose();
                goto TR_004D;
            }
        TR_0040:
            if (queue.Count <= 0)
            {
                foreach (Node node3 in this.DynamicBlocks)
                {
                    node3.PathCount = 1;
                    bool flag1 = node3.Pos == new Vector3I(-6, 6, 0);
                    float num2 = 0f;
                    if (node3.Paths.Count > 1)
                    {
                        foreach (KeyValuePair<Node, PathInfo> pair in node3.Paths)
                        {
                            Vector3 vectorA = Vector3.TransformNormal((Vector3) ((node3.Pos - pair.Value.StartNode.Pos) * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                            float num3 = 0f;
                            foreach (KeyValuePair<Node, PathInfo> pair2 in node3.Paths)
                            {
                                if (pair.Key != pair2.Key)
                                {
                                    Vector3 vectorB = Vector3.TransformNormal((Vector3) ((node3.Pos - pair2.Value.StartNode.Pos) * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                                    float angleBetweenVectorsAndNormalise = MyUtils.GetAngleBetweenVectorsAndNormalise(vectorA, vectorB);
                                    float amount = Math.Abs(Vector3.Normalize(vectorA).Dot(Vector3.Up));
                                    amount = MathHelper.Lerp(0.1f, 1f, amount);
                                    float num6 = MathHelper.Lerp(0.1f, 1f, Math.Abs(Vector3.Normalize(vectorB).Dot(Vector3.Up)));
                                    float num7 = angleBetweenVectorsAndNormalise;
                                    if (!MyPetaInputComponent.OLD_SI)
                                    {
                                        num7 = (angleBetweenVectorsAndNormalise * amount) * pair.Value.DirectionRatio;
                                    }
                                    num3 += num7;
                                }
                            }
                            pair.Value.Ratio = num3;
                            num2 += num3;
                        }
                        foreach (KeyValuePair<Node, PathInfo> pair3 in node3.Paths)
                        {
                            if (num2 <= 0f)
                            {
                                pair3.Value.Ratio = 1f;
                                continue;
                            }
                            PathInfo local1 = pair3.Value;
                            local1.Ratio /= num2;
                        }
                        continue;
                    }
                    foreach (KeyValuePair<Node, PathInfo> pair4 in node3.Paths)
                    {
                        pair4.Value.Ratio = 1f;
                    }
                }
                using (enumerator = this.StaticBlocks.GetEnumerator())
                {
                    Node current;
                    goto TR_0016;
                TR_0002:
                    if (current.OwnedPaths.Count <= 0)
                    {
                        goto TR_0016;
                    }
                TR_000E:
                    while (true)
                    {
                        PathInfo info4 = current.OwnedPaths.Pop();
                        Node endNode = info4.EndNode;
                        float num8 = endNode.TransferMass + (endNode.Mass * info4.Ratio);
                        float num9 = num8 / ((float) info4.Parents.Count);
                        foreach (PathInfo local2 in info4.Parents)
                        {
                            int num10;
                            int num11;
                            Vector3I vectori = local2.EndNode.Pos - endNode.Pos;
                            if (((vectori.X + vectori.Y) + vectori.Z) > 0)
                            {
                                num10 = vectori.Y + (vectori.Z * 2);
                                num11 = num10 + 3;
                            }
                            else
                            {
                                num10 = ((-vectori.X * 3) - (vectori.Y * 4)) - (vectori.Z * 5);
                                num11 = num10 - 3;
                            }
                            float* singlePtr1 = (float*) ref endNode.SupportingWeights[num10];
                            singlePtr1[0] -= num9;
                            PathInfo local3 = local2;
                            float* singlePtr2 = (float*) ref local3.EndNode.SupportingWeights[num11];
                            singlePtr2[0] += num9;
                            local3.EndNode.TransferMass += num9;
                        }
                        endNode.TransferMass -= num8;
                        break;
                    }
                    goto TR_0002;
                TR_0016:
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            Stack<PathInfo>.Enumerator enumerator5 = current.OwnedPaths.GetEnumerator();
                            try
                            {
                                while (enumerator5.MoveNext())
                                {
                                    enumerator5.Current.EndNode.TransferMass = 0f;
                                }
                            }
                            finally
                            {
                                enumerator5.Dispose();
                                break;
                            }
                            goto TR_0002;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    }
                    goto TR_000E;
                }
            }
        TR_004D:
            while (true)
            {
                PathInfo item = queue.Dequeue();
                foreach (Node node2 in item.EndNode.Neighbours)
                {
                    PathInfo info3;
                    if (node2.IsStatic)
                    {
                        continue;
                    }
                    if (node2.Paths.TryGetValue(item.StartNode, out info3))
                    {
                        if (info3.Distance != (item.Distance + 1))
                        {
                            continue;
                        }
                        info3.Parents.Add(item);
                        continue;
                    }
                    if ((item.Distance - closestDistanceThreshold) <= node2.Distance)
                    {
                        node2.Distance = Math.Min(node2.Distance, item.Distance);
                        info3 = new PathInfo {
                            Distance = item.Distance + 1,
                            StartNode = item.StartNode,
                            EndNode = node2
                        };
                        info3.Parents.Add(item);
                        info3.DirectionRatio = item.DirectionRatio * DirectionRatios[(node2.Pos - item.EndNode.Pos).AbsMaxComponent()];
                        node2.Paths.Add(item.StartNode, info3);
                        queue.Enqueue(info3);
                        item.StartNode.OwnedPaths.Push(info3);
                    }
                }
                break;
            }
            goto TR_0040;
        }

        private unsafe void FindAndCaculateFromDynamic()
        {
            HashSet<Node> closedList = new HashSet<Node>();
            List<Node> staticNodes = new List<Node>();
            Queue<Node> queue = new Queue<Node>();
            foreach (Node node in this.DynamicBlocks)
            {
                MyStatToken token;
                closedList.Clear();
                staticNodes.Clear();
                node.PathCount = 1;
                using (token = Stats.Timing.Measure("SI - Dynamic.FindDistances", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindDistances(node, closedList, staticNodes);
                }
                int count = staticNodes.Count;
                float num2 = node.Mass / ((float) count);
                using (token = Stats.Timing.Measure("SI - Dynamic.Calculate", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    foreach (Node node2 in staticNodes)
                    {
                        closedList.Clear();
                        queue.Clear();
                        node2.Ratio = 1f;
                        node.PathCount = 1;
                        queue.Enqueue(node2);
                        while (queue.Count > 0)
                        {
                            Node node3 = queue.Dequeue();
                            float num3 = node3.Ratio / ((float) node3.Parents.Count);
                            foreach (Node node4 in node3.Parents)
                            {
                                int num4;
                                int num5;
                                Vector3I vectori = node4.Pos - node3.Pos;
                                if (((vectori.X + vectori.Y) + vectori.Z) > 0)
                                {
                                    num4 = vectori.Y + (vectori.Z * 2);
                                    num5 = num4 + 3;
                                }
                                else
                                {
                                    num4 = ((-vectori.X * 3) - (vectori.Y * 4)) - (vectori.Z * 5);
                                    num5 = num4 - 3;
                                }
                                float* singlePtr1 = (float*) ref node3.SupportingWeights[num4];
                                singlePtr1[0] -= num2 * num3;
                                float* singlePtr2 = (float*) ref node4.SupportingWeights[num5];
                                singlePtr2[0] += num2 * num3;
                                if (!closedList.Add(node4))
                                {
                                    node4.Ratio += num3;
                                }
                                else
                                {
                                    node4.Ratio = num3;
                                    queue.Enqueue(node4);
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void FindAndCaculateFromStatic()
        {
            HashSet<Node> closedList = new HashSet<Node>();
            List<Node> staticNodes = new List<Node>();
            NodeDistanceComparer comparer = new NodeDistanceComparer();
            foreach (Node node in this.StaticBlocks)
            {
                MyStatToken token;
                closedList.Clear();
                staticNodes.Clear();
                using (token = Stats.Timing.Measure("SI - Static.FindDistances", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindDistances(node, closedList, staticNodes);
                }
                staticNodes.Clear();
                using (token = Stats.Timing.Measure("SI - Dynamic.Sort", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    foreach (Node node2 in closedList)
                    {
                        if (!node2.IsStatic)
                        {
                            node2.Ratio = 1f;
                            node2.TransferMass = node2.Mass;
                            staticNodes.Add(node2);
                        }
                    }
                    staticNodes.Sort(comparer);
                }
                using (token = Stats.Timing.Measure("SI - Dynamic.Calculate", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    foreach (Node node3 in staticNodes)
                    {
                        if (!node3.IsStatic)
                        {
                            node3.PathCount++;
                            foreach (Node local1 in node3.Parents)
                            {
                                int num;
                                int num2;
                                Vector3I vectori = local1.Pos - node3.Pos;
                                if (((vectori.X + vectori.Y) + vectori.Z) > 0)
                                {
                                    num = vectori.Y + (vectori.Z * 2);
                                    num2 = num + 3;
                                }
                                else
                                {
                                    num = ((-vectori.X * 3) - (vectori.Y * 4)) - (vectori.Z * 5);
                                    num2 = num - 3;
                                }
                                float* singlePtr1 = (float*) ref node3.SupportingWeights[num];
                                singlePtr1[0] -= node3.TransferMass / ((float) node3.Parents.Count);
                                Node local2 = local1;
                                float* singlePtr2 = (float*) ref local2.SupportingWeights[num2];
                                singlePtr2[0] += node3.TransferMass / ((float) node3.Parents.Count);
                                local2.TransferMass += node3.TransferMass / ((float) node3.Parents.Count);
                            }
                        }
                    }
                }
            }
        }

        private void FindDistances(Node from, HashSet<Node> closedList, List<Node> staticNodes)
        {
            from.Parents.Clear();
            from.Distance = 0;
            Queue<Node> queue = new Queue<Node>();
            closedList.Add(from);
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                Node item = queue.Dequeue();
                item.Ratio = 0f;
                foreach (Node node2 in item.Neighbours)
                {
                    if (!closedList.Add(node2))
                    {
                        if (node2.Distance != (item.Distance + 1))
                        {
                            continue;
                        }
                        node2.Parents.Add(item);
                        continue;
                    }
                    node2.Parents.Clear();
                    node2.Parents.Add(item);
                    node2.Distance = item.Distance + 1;
                    if (node2.IsStatic)
                    {
                        staticNodes.Add(node2);
                        continue;
                    }
                    queue.Enqueue(node2);
                }
            }
        }

        public void ForceRecalc()
        {
            this.m_needsRecalc = true;
        }

        public float GetSupportedWeight(Vector3I pos)
        {
            Node node;
            return (!this.All.TryGetValue(pos, out node) ? 0f : node.TotalSupportingWeight);
        }

        public float GetTension(Vector3I pos)
        {
            Node node;
            return (!this.All.TryGetValue(pos, out node) ? 0f : (node.TotalSupportingWeight / node.Mass));
        }

        private static Color GetTension(float offset, float max) => 
            ((offset >= (max / 2f)) ? new Color(1f, 1f - ((offset - (max / 2f)) / (max / 2f)), 0f) : new Color(offset / (max / 2f), 1f, 0f));

        public bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB) => 
            true;

        private void LoadBlocks()
        {
            if (this.m_grid.Physics != null)
            {
                this.All.Clear();
                this.DynamicBlocks.Clear();
                this.StaticBlocks.Clear();
                using (Stats.Timing.Measure("SI - Collect", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    foreach (MySlimBlock block in this.m_grid.GetBlocks())
                    {
                        if (this.m_grid.Physics.Shape.BlocksConnectedToWorld.Contains(block.Position))
                        {
                            Node node = new Node(block.Position, true);
                            this.StaticBlocks.Add(node);
                            this.All.Add(block.Position, node);
                            continue;
                        }
                        Node item = new Node(block.Position, false) {
                            Mass = MassToSI(this.m_grid.Physics.Shape.GetBlockMass(block.Position))
                        };
                        this.DynamicBlocks.Add(item);
                        this.All.Add(block.Position, item);
                    }
                    foreach (KeyValuePair<Vector3I, float> pair in this.DynamicWeights)
                    {
                        if (this.All.ContainsKey(pair.Key))
                        {
                            Node local1 = this.All[pair.Key];
                            local1.Mass += pair.Value;
                            continue;
                        }
                        Node item = new Node(pair.Key, false) {
                            Mass = this.DynamicWeights[pair.Key],
                            IsDynamicWeight = true
                        };
                        this.DynamicBlocks.Add(item);
                        this.All.Add(pair.Key, item);
                    }
                }
                this.m_grid.Physics.ContactPointCallback -= new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
                this.m_grid.Physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
            }
        }

        public static float MassFromSI(float mass) => 
            (mass * 30000f);

        public static float MassToSI(float mass) => 
            (mass / 30000f);

        private void Physics_ContactPointCallback(ref MyPhysics.MyContactPointEvent e)
        {
            if (this.m_lastFrameCollision != this.m_frameCounter)
            {
                this.DynamicWeights.Clear();
            }
            MyGridContactInfo info = new MyGridContactInfo(ref e.ContactPointEvent, this.m_grid);
            if (!info.CollidingEntity.Physics.IsStatic)
            {
                float num = info.CollidingEntity.Physics.LinearVelocity.Length();
                if (num >= 0.1f)
                {
                    Vector3I vectori = this.m_grid.WorldToGridInteger(info.ContactPosition + (Vector3.Up * 0.25f));
                    float num2 = Vector3.Dot(Vector3.Normalize(info.CollidingEntity.Physics.LinearVelocity), Vector3.Down);
                    float mass = 0f;
                    this.m_constrainedGrid.Clear();
                    MyCubeGrid collidingEntity = info.CollidingEntity as MyCubeGrid;
                    if (collidingEntity != null)
                    {
                        this.m_constrainedGrid.Add(collidingEntity);
                        this.AddConstrainedGrids(collidingEntity);
                        foreach (MyCubeGrid grid2 in this.m_constrainedGrid)
                        {
                            mass += grid2.Physics.Mass;
                        }
                    }
                    else
                    {
                        mass = info.CollidingEntity.Physics.Mass;
                    }
                    float num4 = ((info.CollidingEntity is MyCharacter) ? MassToSI(mass) : MassToSI(MyDestructionHelper.MassFromHavok(mass))) * MyPetaInputComponent.SI_DYNAMICS_MULTIPLIER;
                    float num5 = ((num4 * num) * num2) + num4;
                    if (num5 >= 0f)
                    {
                        MyCubeSize gridSizeEnum = this.m_grid.GridSizeEnum;
                        this.DynamicWeights[vectori] = num5;
                        this.m_needsRecalc = true;
                        this.m_lastFrameCollision = this.m_frameCounter;
                        if (this.m_collidingEntities.ContainsKey(info.CollidingEntity))
                        {
                            this.m_collidingEntities[info.CollidingEntity].FrameTime = this.m_frameCounter;
                        }
                        else
                        {
                            CollidingEntityInfo info1 = new CollidingEntityInfo();
                            info1.Position = vectori;
                            info1.FrameTime = this.m_frameCounter;
                            this.m_collidingEntities.Add(info.CollidingEntity, info1);
                            info.CollidingEntity.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
                        }
                    }
                }
            }
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
            if (this.m_collidingEntities.ContainsKey((VRage.Game.Entity.MyEntity) obj.Container.Entity) && ((this.m_frameCounter - this.m_collidingEntities[(VRage.Game.Entity.MyEntity) obj.Container.Entity].FrameTime) > 20))
            {
                obj.OnPositionChanged -= new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
                this.DynamicWeights.Remove(this.m_collidingEntities[(VRage.Game.Entity.MyEntity) obj.Container.Entity].Position);
                this.m_collidingEntities.Remove((VRage.Game.Entity.MyEntity) obj.Container.Entity);
                this.m_needsRecalc = true;
            }
        }

        private bool Refresh()
        {
            MyStatToken token;
            this.m_frameCounter++;
            if ((this.m_grid.GetBlocks().Count == this.BlockCount) && !this.m_needsRecalc)
            {
                return false;
            }
            this.m_needsRecalc = false;
            m_selectedGrid = this.m_grid;
            long timestamp = Stopwatch.GetTimestamp();
            this.BlockCount = this.m_grid.GetBlocks().Count;
            this.LoadBlocks();
            this.AddNeighbours();
            if (AlgIndex == 0)
            {
                using (token = Stats.Timing.Measure("SI TOTAL - FindAndCaculateFromStatic", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindAndCaculateFromStatic();
                    goto TR_0044;
                }
            }
            if (AlgIndex == 1)
            {
                using (token = Stats.Timing.Measure("SI TOTAL - FindAndCaculateFromDynamic", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindAndCaculateFromDynamic();
                    goto TR_0044;
                }
            }
            if (AlgIndex == 2)
            {
                using (token = Stats.Timing.Measure("SI TOTAL - FindAndCaculateAdvanced", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindAndCaculateFromAdvanced();
                    goto TR_0044;
                }
            }
            using (token = Stats.Timing.Measure("SI TOTAL - FindAndCaculateAdvancedStatic", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
            {
                this.FindAndCaculateFromAdvancedStatic();
            }
        TR_0044:
            using (token = Stats.Timing.Measure("SI - Sum", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
            {
                foreach (KeyValuePair<Vector3I, Node> pair1 in this.All)
                {
                }
                foreach (KeyValuePair<Vector3I, Node> pair in this.All)
                {
                    float num2 = 0f;
                    int index = 0;
                    while (true)
                    {
                        if (index >= 6)
                        {
                            if ((num2 != 0f) || (pair.Value.PathCount != 0))
                            {
                                Node local1 = pair.Value;
                                local1.TotalSupportingWeight += pair.Value.IsStatic ? 0f : ((num2 * 0.5f) / ((float) pair.Value.PathCount));
                            }
                            break;
                        }
                        num2 += Math.Abs(pair.Value.SupportingWeights[index]);
                        index++;
                    }
                }
                int num4 = 0;
                while (true)
                {
                    if (!BlurEnabled || (num4 >= BlurIterations))
                    {
                        foreach (KeyValuePair<Vector3I, Node> pair2 in this.All)
                        {
                            this.TotalMax = Math.Max(this.TotalMax, pair2.Value.TotalSupportingWeight);
                        }
                        float single1 = ((float) (Stopwatch.GetTimestamp() - timestamp)) / ((float) Stopwatch.Frequency);
                        break;
                    }
                    using (HashSet<Node>.Enumerator enumerator2 = this.DynamicBlocks.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.Ratio = 0f;
                        }
                    }
                    foreach (Node node in this.DynamicBlocks)
                    {
                        float totalSupportingWeight = node.TotalSupportingWeight;
                        int num6 = 1;
                        foreach (Node node2 in node.Neighbours)
                        {
                            if (BlurStaticShareSupport || !node2.IsStatic)
                            {
                                totalSupportingWeight += node2.TotalSupportingWeight;
                                num6++;
                            }
                        }
                        float num7 = totalSupportingWeight / ((float) num6);
                        node.Ratio += (num7 - node.TotalSupportingWeight) * BlurAmount;
                        foreach (Node node3 in node.Neighbours)
                        {
                            if (!node3.IsStatic)
                            {
                                node3.Ratio += (num7 - node3.TotalSupportingWeight) * BlurAmount;
                            }
                        }
                    }
                    foreach (Node node4 in this.DynamicBlocks)
                    {
                        node4.TotalSupportingWeight += node4.Ratio;
                    }
                    num4++;
                }
            }
            return true;
        }

        public void Remove(MySlimBlock block)
        {
        }

        private void RestoreDynamicMasses()
        {
            foreach (Node node in this.DynamicBlocks)
            {
                float num = MassToSI(this.m_grid.Physics.Shape.GetBlockMass(node.Pos));
                node.Mass = num;
            }
        }

        public bool Simulate(float deltaTime) => 
            this.Refresh();

        public static bool BlurEnabled
        {
            get => 
                m_blurEnabled;
            set => 
                (m_blurEnabled = value);
        }

        public static bool BlurStaticShareSupport
        {
            get => 
                m_blurStaticShareSupport;
            set => 
                (m_blurStaticShareSupport = value);
        }

        public static float BlurAmount
        {
            get => 
                m_blurAmount;
            set => 
                (m_blurAmount = value);
        }

        public static float BlurIterations
        {
            get => 
                m_blurIterations;
            set => 
                (m_blurIterations = value);
        }

        public static float ClosestDistanceThreshold
        {
            get => 
                m_closestDistanceThreshold;
            set => 
                (m_closestDistanceThreshold = value);
        }

        public static Vector3I SelectedCube
        {
            get => 
                m_selectedCube;
            set
            {
                if ((m_selectedGrid != null) && (m_selectedGrid.GetCubeBlock(value) != null))
                {
                    m_selectedCube = value;
                }
            }
        }

        private class CollidingEntityInfo
        {
            public Vector3I Position;
            public int FrameTime;
        }

        private class Node
        {
            public List<MyOndraSimulator3.Node> Parents = new List<MyOndraSimulator3.Node>();
            public int Distance;
            public float Ratio;
            public float TransferMass;
            public bool IsDynamicWeight;
            public Dictionary<MyOndraSimulator3.Node, MyOndraSimulator3.PathInfo> Paths = new Dictionary<MyOndraSimulator3.Node, MyOndraSimulator3.PathInfo>();
            public Stack<MyOndraSimulator3.PathInfo> OwnedPaths = new Stack<MyOndraSimulator3.PathInfo>();
            public float[] SupportingWeights = new float[6];
            public float TotalSupportingWeight;
            public int PathCount;
            public Vector3I Pos;
            public float Mass = 1f;
            public bool IsStatic;
            public List<MyOndraSimulator3.Node> Neighbours = new List<MyOndraSimulator3.Node>();

            public Node(Vector3I pos, bool isStatic)
            {
                this.Pos = pos;
                this.IsStatic = isStatic;
            }
        }

        private class NodeDistanceComparer : IComparer<MyOndraSimulator3.Node>
        {
            public int Compare(MyOndraSimulator3.Node x, MyOndraSimulator3.Node y) => 
                (y.Distance - x.Distance);
        }

        private class PathInfo
        {
            public MyOndraSimulator3.Node EndNode;
            public MyOndraSimulator3.Node StartNode;
            public int Distance;
            public float Ratio;
            public float DirectionRatio;
            public List<MyOndraSimulator3.PathInfo> Parents = new List<MyOndraSimulator3.PathInfo>();
        }
    }
}


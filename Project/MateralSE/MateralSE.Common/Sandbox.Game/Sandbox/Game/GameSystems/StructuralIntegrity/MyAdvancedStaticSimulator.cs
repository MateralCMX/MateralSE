namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using ParallelTasks;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game.Components;
    using VRage.Stats;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    internal class MyAdvancedStaticSimulator : IMyIntegritySimulator
    {
        public static bool Multithreaded = true;
        private static float m_closestDistanceThreshold = 3f;
        private bool m_needsRecalc;
        public static bool DrawText = true;
        private static Vector3I[] Offsets = new Vector3I[] { new Vector3I(0, 0, 1), new Vector3I(0, 1, 0), new Vector3I(1, 0, 0), new Vector3I(0, 0, -1), new Vector3I(0, -1, 0), new Vector3I(-1, 0, 0) };
        private static float SideRatio = 0.25f;
        private static float[] DirectionRatios = new float[] { SideRatio, 1f, SideRatio };
        private MyCubeGrid m_grid;
        private int DYNAMIC_UPDATE_DELAY = 3;
        private Dictionary<MyEntity, CollidingEntityInfo> m_collidingEntities = new Dictionary<MyEntity, CollidingEntityInfo>();
        private int m_frameCounter;
        private int m_lastFrameCollision;
        private GridSimulationData m_finishedData = new GridSimulationData();
        private GridSimulationData m_simulatedData = new GridSimulationData();
        private bool m_simulationDataPrepared;
        private bool m_simulationDataReady;
        private bool m_simulationInProgress;
        private static Vector3I m_selectedCube;
        private static MyCubeGrid m_selectedGrid;
        private static float SlidingOffset = 0f;

        public MyAdvancedStaticSimulator(MyCubeGrid grid)
        {
            this.m_grid = grid;
            m_selectedGrid = this.m_grid;
            if (this.m_grid.BlocksCount > 0)
            {
                SelectedCube = this.m_grid.GetBlocks().First<MySlimBlock>().Position;
            }
        }

        public void Add(MySlimBlock block)
        {
            m_selectedGrid = block.CubeGrid;
            m_selectedCube = block.Position;
        }

        private void AddConstrainedGrids(MyCubeGrid collidingGrid)
        {
        }

        private void AddNeighbours(GridSimulationData simData)
        {
            using (Stats.Timing.Measure("SI - AddNeighbours", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
            {
                foreach (KeyValuePair<Vector3I, Node> pair in simData.All)
                {
                    foreach (Vector3I vectori in Offsets)
                    {
                        Node node;
                        if (simData.All.TryGetValue(pair.Value.Pos + vectori, out node))
                        {
                            pair.Value.Neighbours.Add(node);
                        }
                    }
                }
            }
        }

        public void Close()
        {
            if (this.m_grid.Physics != null)
            {
                this.m_grid.Physics.ContactPointCallback -= new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
            }
        }

        public void DebugDraw()
        {
            if (this.m_simulationDataPrepared)
            {
                SlidingOffset += 0.005f;
                if (MyPetaInputComponent.DEBUG_DRAW_PATHS)
                {
                    Color aqua = Color.Aqua;
                    this.DrawCube(this.m_grid.GridSize, SelectedCube, ref aqua, null);
                    if (this.m_finishedData.All.ContainsKey(SelectedCube))
                    {
                        Node node = this.m_finishedData.All[SelectedCube];
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
                        foreach (KeyValuePair<Vector3I, Node> pair2 in this.m_finishedData.All)
                        {
                            Color gray = Color.Gray;
                            float offset = 0f;
                            if (!pair2.Value.IsDynamicWeight)
                            {
                                if (!pair2.Value.IsStatic)
                                {
                                    offset = pair2.Value.TotalSupportingWeight / (pair2.Value.Mass * pair2.Value.PhysicalMaterial.SupportMultiplier);
                                    gray = GetTension(offset, 10f);
                                }
                                this.DrawCube(gridSize, pair2.Key, ref gray, offset.ToString("0.00"));
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<Vector3I, Node> pair3 in this.m_finishedData.All)
                        {
                            Color tension = GetTension(pair3.Value.Mass, 4f);
                            string text = pair3.Value.Mass.ToString("0.00");
                            this.DrawCube(gridSize, pair3.Key, ref tension, text);
                        }
                    }
                }
            }
        }

        private void DebugDrawPath(PathInfo pathInfo, float offset, int index)
        {
            for (int i = 0; i < (pathInfo.PathNodes.Count - 1); i++)
            {
                Node node = pathInfo.PathNodes[i + 1];
                Node local1 = pathInfo.PathNodes[i];
                Vector3D vectord = Vector3D.Transform((Vector3) (local1.Pos * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                Vector3D vectord2 = Vector3D.Transform((Vector3) (node.Pos * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                Vector3 vector = new Vector3(offset);
                DrawSlidingLine(vectord2 + vector, vectord + vector, Color.White, Color.Red);
                if (local1.IsStatic)
                {
                    MyRenderProxy.DebugDrawSphere(vectord + vector, 0.5f, Color.Gray, 1f, false, false, true, false);
                }
                if (i == (pathInfo.PathNodes.Count - 2))
                {
                    object[] objArray1 = new object[] { index.ToString(), " (", pathInfo.Ratio, ")" };
                    MyRenderProxy.DebugDrawText3D(vectord2 + vector, string.Concat(objArray1), Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
                }
            }
        }

        public void Draw()
        {
            if (this.m_simulationDataPrepared)
            {
                foreach (KeyValuePair<Vector3I, Node> pair in this.m_finishedData.All)
                {
                    Color gray = Color.Gray;
                    if (!pair.Value.IsDynamicWeight)
                    {
                        if (!pair.Value.IsStatic)
                        {
                            float max = 10f;
                            gray = GetTension(pair.Value.TotalSupportingWeight / (pair.Value.Mass * pair.Value.PhysicalMaterial.SupportMultiplier), max);
                        }
                        this.DrawCube(this.m_grid.GridSize, pair.Key, ref gray, null);
                    }
                }
            }
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

        private unsafe void FindAndCaculateFromAdvancedStatic(GridSimulationData simData)
        {
            HashSet<Node>.Enumerator enumerator;
            int closestDistanceThreshold = (int) ClosestDistanceThreshold;
            simData.Queue.Clear();
            using (enumerator = simData.DynamicBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Distance = 0x7fffffff;
                }
            }
            enumerator = simData.StaticBlocks.GetEnumerator();
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
                    item.PathNodes.Add(current);
                    simData.Queue.Enqueue(item);
                }
            }
            finally
            {
                enumerator.Dispose();
                goto TR_008E;
            }
        TR_0081:
            if (simData.Queue.Count <= 0)
            {
                foreach (Node node3 in simData.DynamicBlocks)
                {
                    node3.PathCount = 1;
                    bool flag1 = node3.Pos == new Vector3I(-6, 6, 0);
                    float num5 = 0f;
                    if (node3.Paths.Count > 1)
                    {
                        foreach (KeyValuePair<Node, PathInfo> pair in node3.Paths)
                        {
                            Vector3 vectorA = Vector3.TransformNormal((Vector3) ((node3.Pos - pair.Value.StartNode.Pos) * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                            float num6 = 0f;
                            foreach (KeyValuePair<Node, PathInfo> pair2 in node3.Paths)
                            {
                                if (pair.Key != pair2.Key)
                                {
                                    Vector3 vectorB = Vector3.TransformNormal((Vector3) ((node3.Pos - pair2.Value.StartNode.Pos) * this.m_grid.GridSize), this.m_grid.WorldMatrix);
                                    float angleBetweenVectorsAndNormalise = MyUtils.GetAngleBetweenVectorsAndNormalise(vectorA, vectorB);
                                    float amount = Math.Abs(Vector3.Normalize(vectorA).Dot(Vector3.Up));
                                    amount = MathHelper.Lerp(0.1f, 1f, amount);
                                    float num9 = MathHelper.Lerp(0.1f, 1f, Math.Abs(Vector3.Normalize(vectorB).Dot(Vector3.Up)));
                                    float num10 = angleBetweenVectorsAndNormalise;
                                    if (!MyPetaInputComponent.OLD_SI)
                                    {
                                        num10 = (angleBetweenVectorsAndNormalise * amount) * pair.Value.DirectionRatio;
                                    }
                                    num6 += num10;
                                }
                            }
                            pair.Value.Ratio = num6;
                            num5 += num6;
                        }
                        foreach (KeyValuePair<Node, PathInfo> pair3 in node3.Paths)
                        {
                            if (num5 <= 0f)
                            {
                                pair3.Value.Ratio = 1f;
                                continue;
                            }
                            PathInfo local1 = pair3.Value;
                            local1.Ratio /= num5;
                            if (pair3.Value.Ratio < 1E-06f)
                            {
                                pair3.Value.Ratio = 0f;
                            }
                        }
                        continue;
                    }
                    foreach (KeyValuePair<Node, PathInfo> pair4 in node3.Paths)
                    {
                        pair4.Value.Ratio = 1f;
                    }
                }
                using (enumerator = simData.StaticBlocks.GetEnumerator())
                {
                    Node current;
                    goto TR_0055;
                TR_0034:
                    if (current.OwnedPaths.Count <= 0)
                    {
                        goto TR_0055;
                    }
                TR_004D:
                    while (true)
                    {
                        PathInfo info4 = current.OwnedPaths.Pop();
                        Node endNode = info4.EndNode;
                        float num11 = endNode.TransferMass + (endNode.Mass * info4.Ratio);
                        bool flag2 = endNode.Pos == new Vector3I(-1, 1, 0);
                        float num12 = num11 / ((float) info4.Parents.Count);
                        foreach (PathInfo info5 in info4.Parents)
                        {
                            int num13;
                            int num14;
                            Vector3I vectori = info5.EndNode.Pos - endNode.Pos;
                            if (((vectori.X + vectori.Y) + vectori.Z) > 0)
                            {
                                num13 = vectori.Y + (vectori.Z * 2);
                                num14 = num13 + 3;
                            }
                            else
                            {
                                num13 = ((-vectori.X * 3) - (vectori.Y * 4)) - (vectori.Z * 5);
                                num14 = num13 - 3;
                            }
                            endNode.OutgoingNodeswWithWeights[num13].Add(new Tuple<Node, float>(info5.EndNode, num12));
                            if (((num13 == 0) || ((num13 == 2) || (num13 == 3))) || (num13 == 5))
                            {
                                float* singlePtr1 = (float*) ref endNode.SupportingWeights[num13];
                                singlePtr1[0] -= endNode.PhysicalMaterial.HorisontalFragility * num12;
                            }
                            else
                            {
                                float* singlePtr2 = (float*) ref endNode.SupportingWeights[num13];
                                singlePtr2[0] -= num12;
                            }
                            if (((num14 == 0) || ((num14 == 2) || (num14 == 3))) || (num14 == 5))
                            {
                                float* singlePtr3 = (float*) ref info5.EndNode.SupportingWeights[num14];
                                singlePtr3[0] += info5.EndNode.PhysicalMaterial.HorisontalFragility * num12;
                            }
                            else
                            {
                                float* singlePtr4 = (float*) ref info5.EndNode.SupportingWeights[num14];
                                singlePtr4[0] += num12;
                            }
                            info5.EndNode.SupportingNodeswWithWeights[num14].Add(new Tuple<Node, float>(endNode, num12));
                            info5.EndNode.TransferMass += num12;
                        }
                        endNode.TransferMass -= num11;
                        break;
                    }
                    goto TR_0034;
                TR_0055:
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
                            goto TR_0034;
                        }
                        else
                        {
                            using (Stats.Timing.Measure("SI - Sum", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                            {
                                foreach (KeyValuePair<Vector3I, Node> pair1 in simData.All)
                                {
                                }
                                foreach (KeyValuePair<Vector3I, Node> pair5 in simData.All)
                                {
                                    bool flag3 = pair5.Key == new Vector3I(-1, 1, 0);
                                    float num15 = 0f;
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index >= 6)
                                        {
                                            if ((num15 != 0f) || (pair5.Value.PathCount != 0))
                                            {
                                                Node local2 = pair5.Value;
                                                local2.TotalSupportingWeight += pair5.Value.IsStatic ? 0f : ((num15 * 0.5f) / ((float) pair5.Value.PathCount));
                                            }
                                            break;
                                        }
                                        float num17 = pair5.Value.SupportingWeights[index];
                                        num15 += Math.Abs(num17);
                                        index++;
                                    }
                                }
                                List<Node> list = new List<Node>();
                                using (Dictionary<Vector3I, Node>.Enumerator enumerator7 = simData.All.GetEnumerator())
                                {
                                    while (enumerator7.MoveNext())
                                    {
                                        KeyValuePair<Vector3I, Node> node;
                                        list.Clear();
                                        float totalSupportingWeight = node.Value.TotalSupportingWeight;
                                        float mass = node.Value.Mass;
                                        foreach (Node node6 in node.Value.Neighbours)
                                        {
                                            Func<KeyValuePair<Node, PathInfo>, bool> <>9__0;
                                            Func<KeyValuePair<Node, PathInfo>, bool> predicate = <>9__0;
                                            if (<>9__0 == null)
                                            {
                                                Func<KeyValuePair<Node, PathInfo>, bool> local3 = <>9__0;
                                                predicate = <>9__0 = x => (x.Key.Pos.X == node.Key.X) && (x.Key.Pos.Z == node.Key.Z);
                                            }
                                            if ((node6.Pos.Y == node.Key.Y) & node6.Paths.Any<KeyValuePair<Node, PathInfo>>(predicate))
                                            {
                                                totalSupportingWeight += node6.TotalSupportingWeight;
                                                mass += node6.Mass;
                                                list.Add(node6);
                                            }
                                        }
                                        if (list.Count > 0)
                                        {
                                            float num20 = totalSupportingWeight / mass;
                                            foreach (Node node7 in list)
                                            {
                                                node7.TotalSupportingWeight = num20 * node7.Mass;
                                            }
                                            node.Value.TotalSupportingWeight = num20 * node.Value.Mass;
                                        }
                                    }
                                }
                                foreach (KeyValuePair<Vector3I, Node> pair6 in simData.All)
                                {
                                    simData.TotalMax = Math.Max(simData.TotalMax, pair6.Value.TotalSupportingWeight);
                                }
                            }
                            return;
                        }
                        break;
                    }
                    goto TR_004D;
                }
            }
        TR_008E:
            while (true)
            {
                PathInfo item = simData.Queue.Dequeue();
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
                            EndNode = node2,
                            PathNodes = item.PathNodes.ToList<Node>()
                        };
                        info3.PathNodes.Add(node2);
                        info3.Parents.Add(item);
                        float num3 = MathHelper.Clamp((float) (item.EndNode.Mass / (node2.Mass + item.EndNode.Mass)), (float) 0f, (float) 1f);
                        float num2 = (node2.PhysicalMaterial.HorisontalTransmissionMultiplier * item.EndNode.PhysicalMaterial.HorisontalTransmissionMultiplier) * (node2.Mass * num3);
                        float[] numArray = new float[] { num2, 1f, num2 };
                        int index = (node2.Pos - item.EndNode.Pos).AbsMaxComponent();
                        info3.DirectionRatio = (item.DirectionRatio * DirectionRatios[index]) * numArray[index];
                        node2.Paths.Add(item.StartNode, info3);
                        simData.Queue.Enqueue(info3);
                        item.StartNode.OwnedPaths.Push(info3);
                    }
                }
                break;
            }
            goto TR_0081;
        }

        public void ForceRecalc()
        {
            this.m_needsRecalc = true;
        }

        public float GetSupportedWeight(Vector3I pos)
        {
            Node node;
            return (this.m_simulationDataPrepared ? (!this.m_finishedData.All.TryGetValue(pos, out node) ? 0f : node.TotalSupportingWeight) : 0f);
        }

        public float GetTension(Vector3I pos)
        {
            Node node;
            return (this.m_simulationDataPrepared ? (!this.m_finishedData.All.TryGetValue(pos, out node) ? 0f : (node.TotalSupportingWeight / (node.Mass * node.PhysicalMaterial.SupportMultiplier))) : 0f);
        }

        private static Color GetTension(float offset, float max) => 
            ((offset >= (max / 2f)) ? new Color(1f, 1f - ((offset - (max / 2f)) / (max / 2f)), 0f) : new Color(offset / (max / 2f), 1f, 0f));

        public bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB) => 
            true;

        private void LoadBlocks(GridSimulationData simData)
        {
            simData.BlockCount = this.m_grid.GetBlocks().Count;
            simData.All.Clear();
            simData.DynamicBlocks.Clear();
            simData.StaticBlocks.Clear();
            using (Stats.Timing.Measure("SI - Collect", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
            {
                foreach (MySlimBlock block in this.m_grid.GetBlocks())
                {
                    if (!simData.All.ContainsKey(block.Position))
                    {
                        if (this.m_grid.Physics.Shape.BlocksConnectedToWorld.Contains(block.Position))
                        {
                            Node item = new Node(block.Position, true) {
                                PhysicalMaterial = block.BlockDefinition.PhysicalMaterial
                            };
                            simData.StaticBlocks.Add(item);
                            simData.All.Add(block.Position, item);
                            continue;
                        }
                        float num = MassToSI(this.m_grid.Physics.Shape.GetBlockMass(block.Position));
                        MyPhysicalMaterialDefinition physicalMaterial = block.BlockDefinition.PhysicalMaterial;
                        if (block.FatBlock is MyCompoundCubeBlock)
                        {
                            MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                            physicalMaterial = fatBlock.GetBlocks().First<MySlimBlock>().BlockDefinition.PhysicalMaterial;
                            bool flag = true;
                            using (List<MySlimBlock>.Enumerator enumerator2 = fatBlock.GetBlocks().GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    if (!enumerator2.Current.BlockDefinition.IsGeneratedBlock)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            bool flag2 = true;
                            foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                            {
                                if (!block3.BlockDefinition.IsGeneratedBlock)
                                {
                                    flag2 = false;
                                }
                                else if (block3.BlockDefinition.IsGeneratedBlock && (block3.BlockDefinition.PhysicalMaterial.Id.SubtypeName == "Stone"))
                                {
                                    flag2 = false;
                                }
                                else if ((block3.BlockDefinition.IsGeneratedBlock && (block3.BlockDefinition.PhysicalMaterial.Id.SubtypeName == "RoofTile")) & flag)
                                {
                                    flag2 = false;
                                    num *= 6f;
                                }
                                else if ((block3.BlockDefinition.IsGeneratedBlock && (block3.BlockDefinition.PhysicalMaterial.Id.SubtypeName == "RoofWood")) & flag)
                                {
                                    flag2 = false;
                                    num *= 3f;
                                }
                                else
                                {
                                    if (!((block3.BlockDefinition.IsGeneratedBlock && (block3.BlockDefinition.PhysicalMaterial.Id.SubtypeName == "RoofHay")) & flag))
                                    {
                                        continue;
                                    }
                                    flag2 = false;
                                    num *= 1.2f;
                                }
                                break;
                            }
                            if (flag2)
                            {
                                continue;
                            }
                        }
                        Vector3I min = block.Min;
                        float num2 = 1f / ((float) block.BlockDefinition.Size.Size);
                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref block.Min, ref block.Max);
                        while (iterator.IsValid())
                        {
                            Node item = new Node(min, false) {
                                Mass = num * num2,
                                PhysicalMaterial = physicalMaterial
                            };
                            simData.DynamicBlocks.Add(item);
                            simData.All.Add(min, item);
                            iterator.GetNext(out min);
                        }
                    }
                }
                foreach (KeyValuePair<Vector3I, float> pair in simData.DynamicWeights)
                {
                    if (simData.All.ContainsKey(pair.Key))
                    {
                        Node local1 = simData.All[pair.Key];
                        local1.Mass += pair.Value;
                        continue;
                    }
                    Node item = new Node(pair.Key, false) {
                        Mass = simData.DynamicWeights[pair.Key],
                        IsDynamicWeight = true
                    };
                    simData.DynamicBlocks.Add(item);
                    simData.All.Add(pair.Key, item);
                }
            }
            this.m_grid.Physics.ContactPointCallback -= new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
            this.m_grid.Physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
            this.AddNeighbours(simData);
        }

        public static float MassFromSI(float mass) => 
            (mass * 30000f);

        public static float MassToSI(float mass) => 
            (mass / 30000f);

        private void Physics_ContactPointCallback(ref MyPhysics.MyContactPointEvent e)
        {
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
        }

        public void Remove(MySlimBlock block)
        {
            m_selectedGrid = block.CubeGrid;
        }

        private void RestoreDynamicMasses()
        {
        }

        public bool Simulate(float deltaTime)
        {
            if (this.m_grid.Physics == null)
            {
                return false;
            }
            this.m_frameCounter++;
            if (this.m_simulationDataReady)
            {
                this.SwapSimulatedDatas();
                this.m_simulationDataReady = false;
                this.m_simulationDataPrepared = true;
                return true;
            }
            if (((this.m_grid.GetBlocks().Count == this.m_finishedData.BlockCount) && this.m_simulationDataPrepared) && !this.m_needsRecalc)
            {
                return false;
            }
            this.m_needsRecalc = true;
            if (this.m_simulationInProgress)
            {
                return false;
            }
            this.m_simulationInProgress = true;
            this.LoadBlocks(this.m_simulatedData);
            if (Multithreaded)
            {
                this.m_needsRecalc = false;
                Parallel.Start(delegate {
                    this.FindAndCaculateFromAdvancedStatic(this.m_simulatedData);
                }, delegate {
                    this.m_simulationInProgress = false;
                    this.m_simulationDataReady = true;
                });
            }
            else
            {
                this.m_needsRecalc = false;
                using (Stats.Timing.Measure("SI TOTAL - FindAndCaculateAdvancedStatic", MyStatTypeEnum.Avg | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.DontDisappearFlag | MyStatTypeEnum.Min, 200, 1, -1))
                {
                    this.FindAndCaculateFromAdvancedStatic(this.m_simulatedData);
                }
                this.SwapSimulatedDatas();
                this.m_simulationInProgress = false;
                this.m_simulationDataPrepared = true;
            }
            return true;
        }

        private void SwapSimulatedDatas()
        {
            GridSimulationData finishedData = this.m_finishedData;
            this.m_finishedData = this.m_simulatedData;
            this.m_simulatedData = finishedData;
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

        private class GridSimulationData
        {
            public int BlockCount;
            public float TotalMax;
            public HashSet<MyAdvancedStaticSimulator.Node> StaticBlocks = new HashSet<MyAdvancedStaticSimulator.Node>();
            public HashSet<MyAdvancedStaticSimulator.Node> DynamicBlocks = new HashSet<MyAdvancedStaticSimulator.Node>();
            public Dictionary<Vector3I, MyAdvancedStaticSimulator.Node> All = new Dictionary<Vector3I, MyAdvancedStaticSimulator.Node>();
            public Dictionary<Vector3I, float> DynamicWeights = new Dictionary<Vector3I, float>();
            public HashSet<MyCubeGrid> ConstrainedGrid = new HashSet<MyCubeGrid>();
            public Queue<MyAdvancedStaticSimulator.PathInfo> Queue = new Queue<MyAdvancedStaticSimulator.PathInfo>();
        }

        private class Node
        {
            public int Distance;
            public float Ratio;
            public float TransferMass;
            public bool IsDynamicWeight;
            public Dictionary<MyAdvancedStaticSimulator.Node, MyAdvancedStaticSimulator.PathInfo> Paths = new Dictionary<MyAdvancedStaticSimulator.Node, MyAdvancedStaticSimulator.PathInfo>();
            public Stack<MyAdvancedStaticSimulator.PathInfo> OwnedPaths = new Stack<MyAdvancedStaticSimulator.PathInfo>();
            public float[] SupportingWeights = new float[6];
            public List<Tuple<MyAdvancedStaticSimulator.Node, float>>[] SupportingNodeswWithWeights = new List<Tuple<MyAdvancedStaticSimulator.Node, float>>[6];
            public List<Tuple<MyAdvancedStaticSimulator.Node, float>>[] OutgoingNodeswWithWeights = new List<Tuple<MyAdvancedStaticSimulator.Node, float>>[6];
            public float TotalSupportingWeight;
            public int PathCount;
            public Vector3I Pos;
            public float Mass = 1f;
            public bool IsStatic;
            public List<MyAdvancedStaticSimulator.Node> Neighbours = new List<MyAdvancedStaticSimulator.Node>();
            public MyPhysicalMaterialDefinition PhysicalMaterial;

            public Node(Vector3I pos, bool isStatic)
            {
                this.Pos = pos;
                this.IsStatic = isStatic;
                for (int i = 0; i < 6; i++)
                {
                    this.SupportingNodeswWithWeights[i] = new List<Tuple<MyAdvancedStaticSimulator.Node, float>>();
                    this.OutgoingNodeswWithWeights[i] = new List<Tuple<MyAdvancedStaticSimulator.Node, float>>();
                }
            }
        }

        private class PathInfo
        {
            public MyAdvancedStaticSimulator.Node EndNode;
            public MyAdvancedStaticSimulator.Node StartNode;
            public int Distance;
            public float Ratio;
            public float DirectionRatio;
            public List<MyAdvancedStaticSimulator.PathInfo> Parents = new List<MyAdvancedStaticSimulator.PathInfo>();
            public List<MyAdvancedStaticSimulator.Node> PathNodes = new List<MyAdvancedStaticSimulator.Node>();
        }
    }
}


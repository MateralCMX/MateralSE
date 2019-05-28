namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyConveyorLine : IEnumerable<Vector3I>, IEnumerable, IMyPathEdge<IMyConveyorEndpoint>
    {
        private static ConcurrentDictionary<MyDefinitionId, BlockLinePositionInformation[]> m_blockLinePositions = new ConcurrentDictionary<MyDefinitionId, BlockLinePositionInformation[]>();
        private static readonly float CONVEYOR_PER_LINE_PENALTY = 1f;
        private const int FRAMES_PER_BIG_UPDATE = 0x40;
        private const float BIG_UPDATE_FRACTION = 0.015625f;
        private ConveyorLinePosition m_endpointPosition1;
        private ConveyorLinePosition m_endpointPosition2;
        private IMyConveyorEndpoint m_endpoint1;
        private IMyConveyorEndpoint m_endpoint2;
        private MyObjectBuilder_ConveyorLine.LineType m_type;
        private MyObjectBuilder_ConveyorLine.LineConductivity m_conductivity;
        private int m_length = 0;
        private MyCubeGrid m_cubeGrid;
        [ThreadStatic]
        private static bool m_invertedConductivity = false;
        private MySinglyLinkedList<MyConveyorPacket> m_queue1 = new MySinglyLinkedList<MyConveyorPacket>();
        private MySinglyLinkedList<MyConveyorPacket> m_queue2 = new MySinglyLinkedList<MyConveyorPacket>();
        private List<SectionInformation> m_sections = null;
        private static List<SectionInformation> m_tmpSections1 = new List<SectionInformation>();
        private static List<SectionInformation> m_tmpSections2 = new List<SectionInformation>();
        private bool m_stopped1 = false;
        private bool m_stopped2 = false;
        private float m_queuePosition = 0f;
        private bool m_isFunctional = false;
        private bool m_isWorking = false;
        private LinePositionEnumerator m_enumerator;

        public void BigUpdate()
        {
            this.StopQueuesIfNeeded();
            if (!this.m_stopped1)
            {
                foreach (MyConveyorPacket local1 in this.m_queue1)
                {
                    local1.LinePosition++;
                }
            }
            if (!this.m_stopped2)
            {
                foreach (MyConveyorPacket local2 in this.m_queue2)
                {
                    local2.LinePosition++;
                }
            }
            if (!this.m_isWorking)
            {
                this.m_stopped1 = true;
                this.m_stopped2 = true;
            }
            this.m_queuePosition = 0f;
            if (!this.m_stopped1 || !this.m_stopped2)
            {
                this.RecalculatePacketPositions();
            }
        }

        public bool CheckSectionConsistency()
        {
            if (this.m_sections != null)
            {
                Base6Directions.Direction? nullable = null;
                using (List<SectionInformation>.Enumerator enumerator = this.m_sections.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        SectionInformation current = enumerator.Current;
                        if ((nullable == null) || (((Base6Directions.Direction) nullable.Value) != current.Direction))
                        {
                            nullable = new Base6Directions.Direction?(current.Direction);
                            continue;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public void DebugDraw(MyCubeGrid grid)
        {
            Vector3 position = new Vector3(this.m_endpointPosition2.LocalGridPosition) * grid.GridSize;
            position = (Vector3) Vector3.Transform(position, grid.WorldMatrix);
            string text = ((((this.m_endpoint1 == null) ? "- " : "# ") + this.m_length.ToString() + " ") + this.m_type.ToString() + ((this.m_endpoint2 == null) ? " -" : " #")) + " " + this.m_conductivity.ToString();
            MyRenderProxy.DebugDrawText3D((Vector3.Transform(new Vector3(this.m_endpointPosition1.LocalGridPosition) * grid.GridSize, grid.WorldMatrix) + position) * 0.5f, text, Color.Blue, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            Vector3 local1 = (Vector3) text;
            string local2 = text;
            Color colorFrom = this.IsFunctional ? Color.Green : Color.Red;
            MyRenderProxy.DebugDrawLine3D((Vector3D) local2, position, colorFrom, colorFrom, false, false);
        }

        public void DebugDrawPackets()
        {
            MatrixD worldMatrix;
            foreach (MyConveyorPacket packet in this.m_queue1)
            {
                Color red = Color.Red;
                MyRenderProxy.DebugDrawSphere(packet.WorldMatrix.Translation, 0.2f, red.ToVector3(), 1f, false, false, true, false);
                worldMatrix = packet.WorldMatrix;
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, packet.LinePosition.ToString(), Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            foreach (MyConveyorPacket packet2 in this.m_queue2)
            {
                MyRenderProxy.DebugDrawSphere(packet2.WorldMatrix.Translation, 0.2f, Color.Red.ToVector3(), 1f, false, false, true, false);
                worldMatrix = packet2.WorldMatrix;
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, packet2.LinePosition.ToString(), Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        public void DisconnectEndpoint(IMyConveyorEndpoint endpoint)
        {
            if (ReferenceEquals(endpoint, this.m_endpoint1))
            {
                this.m_endpoint1 = null;
            }
            if (ReferenceEquals(endpoint, this.m_endpoint2))
            {
                this.m_endpoint2 = null;
            }
            this.UpdateIsFunctional();
        }

        public static BlockLinePositionInformation[] GetBlockLinePositions(MyCubeBlock block)
        {
            BlockLinePositionInformation[] informationArray;
            if (!m_blockLinePositions.TryGetValue(block.BlockDefinition.Id, out informationArray))
            {
                MyCubeBlockDefinition blockDefinition = block.BlockDefinition;
                float cubeSize = MyDefinitionManager.Static.GetCubeSize(blockDefinition.CubeSize);
                Vector3 vector = (new Vector3(blockDefinition.Size) * 0.5f) * cubeSize;
                MyModel modelOnlyDummies = MyModels.GetModelOnlyDummies(block.BlockDefinition.Model);
                int num2 = 0;
                foreach (KeyValuePair<string, MyModelDummy> pair in modelOnlyDummies.Dummies)
                {
                    char[] separator = new char[] { '_' };
                    string[] strArray = pair.Key.ToLower().Split(separator);
                    if ((strArray.Length >= 2) && ((strArray[0] == "detector") && strArray[1].StartsWith("conveyor")))
                    {
                        num2++;
                    }
                }
                informationArray = new BlockLinePositionInformation[num2];
                int index = 0;
                foreach (KeyValuePair<string, MyModelDummy> pair2 in modelOnlyDummies.Dummies)
                {
                    char[] separator = new char[] { '_' };
                    string[] strArray2 = pair2.Key.ToLower().Split(separator);
                    if ((strArray2.Length >= 2) && ((strArray2[0] == "detector") && strArray2[1].StartsWith("conveyor")))
                    {
                        int num1;
                        int num4;
                        informationArray[index].LineType = ((strArray2.Length <= 2) || (strArray2[2] != "small")) ? MyObjectBuilder_ConveyorLine.LineType.LARGE_LINE : MyObjectBuilder_ConveyorLine.LineType.SMALL_LINE;
                        informationArray[index].LineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FULL;
                        if ((strArray2.Length <= 2) || (strArray2[2] != "in"))
                        {
                            num1 = (strArray2.Length <= 3) ? 0 : ((int) (strArray2[3] == "in"));
                        }
                        else
                        {
                            num1 = 1;
                        }
                        if (num1 != 0)
                        {
                            informationArray[index].LineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD;
                        }
                        if ((strArray2.Length <= 2) || (strArray2[2] != "out"))
                        {
                            num4 = (strArray2.Length <= 3) ? 0 : ((int) (strArray2[3] == "out"));
                        }
                        else
                        {
                            num4 = 1;
                        }
                        if (num4 != 0)
                        {
                            informationArray[index].LineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD;
                        }
                        Matrix matrix = pair2.Value.Matrix;
                        ConveyorLinePosition position = new ConveyorLinePosition();
                        Vector3I vectori = Vector3I.Floor(((matrix.Translation + blockDefinition.ModelOffset) + vector) / cubeSize);
                        vectori = Vector3I.Max(Vector3I.Zero, vectori);
                        vectori = Vector3I.Min(blockDefinition.Size - Vector3I.One, vectori);
                        position.LocalGridPosition = vectori - blockDefinition.Center;
                        position.Direction = Base6Directions.GetDirection(Vector3.Normalize(Vector3.DominantAxisProjection(Vector3.Divide(((matrix.Translation + blockDefinition.ModelOffset) + vector) - ((new Vector3(vectori) + Vector3.Half) * cubeSize), cubeSize))));
                        informationArray[index].Position = position;
                        index++;
                    }
                }
                m_blockLinePositions.TryAdd(blockDefinition.Id, informationArray);
            }
            return informationArray;
        }

        public IMyConveyorEndpoint GetEndpoint(int index)
        {
            if (index == 0)
            {
                return this.m_endpoint1;
            }
            if (index != 1)
            {
                throw new IndexOutOfRangeException();
            }
            return this.m_endpoint2;
        }

        public ConveyorLinePosition GetEndpointPosition(int index)
        {
            if (index == 0)
            {
                return this.m_endpointPosition1;
            }
            if (index != 1)
            {
                throw new IndexOutOfRangeException();
            }
            return this.m_endpointPosition2;
        }

        public IEnumerator<Vector3I> GetEnumerator() => 
            new LinePositionEnumerator(this);

        private MyCubeGrid GetGrid()
        {
            if ((this.m_endpoint1 == null) || (this.m_endpoint2 == null))
            {
                return null;
            }
            return this.m_endpoint1.CubeBlock.CubeGrid;
        }

        public MyObjectBuilder_ConveyorLine GetObjectBuilder()
        {
            MyObjectBuilder_ConveyorLine line = new MyObjectBuilder_ConveyorLine();
            foreach (MyConveyorPacket packet in this.m_queue1)
            {
                MyObjectBuilder_ConveyorPacket item = new MyObjectBuilder_ConveyorPacket {
                    Item = packet.Item.GetObjectBuilder(),
                    LinePosition = packet.LinePosition
                };
                line.PacketsForward.Add(item);
            }
            foreach (MyConveyorPacket packet3 in this.m_queue2)
            {
                MyObjectBuilder_ConveyorPacket item = new MyObjectBuilder_ConveyorPacket {
                    Item = packet3.Item.GetObjectBuilder(),
                    LinePosition = packet3.LinePosition
                };
                line.PacketsBackward.Add(item);
            }
            line.StartPosition = this.m_endpointPosition1.LocalGridPosition;
            line.StartDirection = this.m_endpointPosition1.Direction;
            line.EndPosition = this.m_endpointPosition2.LocalGridPosition;
            line.EndDirection = this.m_endpointPosition2.Direction;
            if (this.m_sections != null)
            {
                line.Sections = new List<SerializableLineSectionInformation>(this.m_sections.Count);
                foreach (SectionInformation information in this.m_sections)
                {
                    SerializableLineSectionInformation item = new SerializableLineSectionInformation {
                        Direction = information.Direction,
                        Length = information.Length
                    };
                    line.Sections.Add(item);
                }
            }
            line.ConveyorLineType = this.m_type;
            line.ConveyorLineConductivity = this.m_conductivity;
            return line;
        }

        public IMyConveyorEndpoint GetOtherVertex(IMyConveyorEndpoint endpoint)
        {
            if (this.m_isWorking)
            {
                MyObjectBuilder_ConveyorLine.LineConductivity bACKWARD = this.m_conductivity;
                if (m_invertedConductivity)
                {
                    if (this.m_conductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)
                    {
                        bACKWARD = MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD;
                    }
                    else if (this.m_conductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)
                    {
                        bACKWARD = MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD;
                    }
                }
                if (ReferenceEquals(endpoint, this.m_endpoint1))
                {
                    if ((bACKWARD == MyObjectBuilder_ConveyorLine.LineConductivity.FULL) || (bACKWARD == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD))
                    {
                        return this.m_endpoint2;
                    }
                    return null;
                }
                if (ReferenceEquals(endpoint, this.m_endpoint2) && ((bACKWARD == MyObjectBuilder_ConveyorLine.LineConductivity.FULL) || (bACKWARD == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)))
                {
                    return this.m_endpoint1;
                }
            }
            return null;
        }

        public float GetWeight() => 
            (this.Length + CONVEYOR_PER_LINE_PENALTY);

        public void Init(MyObjectBuilder_ConveyorLine objectBuilder, MyCubeGrid cubeGrid)
        {
            this.m_cubeGrid = cubeGrid;
            foreach (MyObjectBuilder_ConveyorPacket packet in objectBuilder.PacketsForward)
            {
                MyConveyorPacket item = new MyConveyorPacket();
                item.Init(packet, this.m_cubeGrid);
                this.m_queue1.Append(item);
            }
            foreach (MyObjectBuilder_ConveyorPacket packet3 in objectBuilder.PacketsBackward)
            {
                MyConveyorPacket item = new MyConveyorPacket();
                item.Init(packet3, this.m_cubeGrid);
                this.m_queue2.Append(item);
            }
            this.m_endpointPosition1 = new ConveyorLinePosition((Vector3I) objectBuilder.StartPosition, objectBuilder.StartDirection);
            this.m_endpointPosition2 = new ConveyorLinePosition((Vector3I) objectBuilder.EndPosition, objectBuilder.EndDirection);
            this.m_length = 0;
            if ((objectBuilder.Sections != null) && (objectBuilder.Sections.Count != 0))
            {
                this.InitializeSectionList(objectBuilder.Sections.Count);
                foreach (SerializableLineSectionInformation information in objectBuilder.Sections)
                {
                    SectionInformation item = new SectionInformation {
                        Direction = information.Direction,
                        Length = information.Length
                    };
                    this.m_sections.Add(item);
                    this.m_length += item.Length;
                }
            }
            if (this.m_length == 0)
            {
                this.m_length = this.m_endpointPosition2.LocalGridPosition.RectangularDistance(this.m_endpointPosition1.LocalGridPosition);
            }
            this.m_type = objectBuilder.ConveyorLineType;
            if (this.m_type == MyObjectBuilder_ConveyorLine.LineType.DEFAULT_LINE)
            {
                if (cubeGrid.GridSizeEnum == MyCubeSize.Small)
                {
                    this.m_type = MyObjectBuilder_ConveyorLine.LineType.SMALL_LINE;
                }
                else if (cubeGrid.GridSizeEnum == MyCubeSize.Large)
                {
                    this.m_type = MyObjectBuilder_ConveyorLine.LineType.LARGE_LINE;
                }
            }
            this.m_conductivity = objectBuilder.ConveyorLineConductivity;
            this.StopQueuesIfNeeded();
            this.RecalculatePacketPositions();
        }

        public void Init(ConveyorLinePosition endpoint1, ConveyorLinePosition endpoint2, MyCubeGrid cubeGrid, MyObjectBuilder_ConveyorLine.LineType type, MyObjectBuilder_ConveyorLine.LineConductivity conductivity = 0, Vector3I? corner = new Vector3I?())
        {
            this.m_cubeGrid = cubeGrid;
            this.m_type = type;
            this.m_conductivity = conductivity;
            this.m_endpointPosition1 = endpoint1;
            this.m_endpointPosition2 = endpoint2;
            this.m_isFunctional = false;
            if (corner != null)
            {
                this.InitializeSectionList(2);
                Vector3I vectori = corner.Value - endpoint1.LocalGridPosition;
                int num = vectori.RectangularLength();
                Vector3I vectori2 = endpoint2.LocalGridPosition - corner.Value;
                int num2 = vectori2.RectangularLength();
                SectionInformation information3 = new SectionInformation {
                    Direction = Base6Directions.GetDirection((Vector3I) (vectori / num)),
                    Length = num
                };
                SectionInformation item = information3;
                information3 = new SectionInformation {
                    Direction = Base6Directions.GetDirection((Vector3I) (vectori2 / num2)),
                    Length = num2
                };
                SectionInformation information2 = information3;
                this.m_sections.Add(item);
                this.m_sections.Add(information2);
            }
            this.m_length = endpoint1.LocalGridPosition.RectangularDistance(endpoint2.LocalGridPosition);
        }

        private void InitAfterSplit(ConveyorLinePosition endpoint1, ConveyorLinePosition endpoint2, List<SectionInformation> sections, int newLength, MyCubeGrid cubeGrid, MyObjectBuilder_ConveyorLine.LineType lineType)
        {
            this.m_endpointPosition1 = endpoint1;
            this.m_endpointPosition2 = endpoint2;
            this.m_sections = sections;
            this.m_length = newLength;
            this.m_cubeGrid = cubeGrid;
            this.m_type = lineType;
        }

        public void InitEndpoints(IMyConveyorEndpoint endpoint1, IMyConveyorEndpoint endpoint2)
        {
            this.m_endpoint1 = endpoint1;
            this.m_endpoint2 = endpoint2;
            this.UpdateIsFunctional();
        }

        private void InitializeSectionList(int size = -1)
        {
            if (this.m_sections != null)
            {
                this.m_sections.Clear();
                if (size != -1)
                {
                    this.m_sections.Capacity = size;
                }
            }
            else if (size != -1)
            {
                this.m_sections = new List<SectionInformation>(size);
            }
            else
            {
                this.m_sections = new List<SectionInformation>();
            }
        }

        public void Merge(MyConveyorLine mergingLine, IMyConveyorSegmentBlock newlyAddedBlock = null)
        {
            ConveyorLinePosition connectingPosition = this.m_endpointPosition2.GetConnectingPosition();
            if (mergingLine.m_endpointPosition1.Equals(connectingPosition))
            {
                this.MergeInternal(mergingLine, newlyAddedBlock);
            }
            else if (mergingLine.m_endpointPosition2.Equals(connectingPosition))
            {
                mergingLine.Reverse();
                this.MergeInternal(mergingLine, newlyAddedBlock);
            }
            else
            {
                this.Reverse();
                connectingPosition = this.m_endpointPosition2.GetConnectingPosition();
                if (mergingLine.m_endpointPosition1.Equals(connectingPosition))
                {
                    this.MergeInternal(mergingLine, newlyAddedBlock);
                }
                else if (mergingLine.m_endpointPosition2.Equals(connectingPosition))
                {
                    mergingLine.Reverse();
                    this.MergeInternal(mergingLine, newlyAddedBlock);
                }
            }
            mergingLine.RecalculateConductivity();
        }

        public unsafe void MergeInternal(MyConveyorLine mergingLine, IMyConveyorSegmentBlock newlyAddedBlock = null)
        {
            this.m_endpointPosition2 = mergingLine.m_endpointPosition2;
            this.m_endpoint2 = mergingLine.m_endpoint2;
            if (mergingLine.m_sections == null)
            {
                if (this.m_sections != null)
                {
                    SectionInformation information3 = this.m_sections[this.m_sections.Count - 1];
                    int* numPtr3 = (int*) ref information3.Length;
                    numPtr3[0] += mergingLine.m_length - 1;
                    this.m_sections[this.m_sections.Count - 1] = information3;
                }
            }
            else if (this.m_sections == null)
            {
                this.InitializeSectionList(mergingLine.m_sections.Count);
                this.m_sections.AddList<SectionInformation>(mergingLine.m_sections);
                SectionInformation information = this.m_sections[0];
                int* numPtr1 = (int*) ref information.Length;
                numPtr1[0] += this.m_length - 1;
                this.m_sections[0] = information;
            }
            else
            {
                this.m_sections.Capacity = (this.m_sections.Count + mergingLine.m_sections.Count) - 1;
                SectionInformation information2 = this.m_sections[this.m_sections.Count - 1];
                int* numPtr2 = (int*) ref information2.Length;
                numPtr2[0] += mergingLine.m_sections[0].Length - 1;
                this.m_sections[this.m_sections.Count - 1] = information2;
                for (int i = 1; i < mergingLine.m_sections.Count; i++)
                {
                    this.m_sections.Add(mergingLine.m_sections[i]);
                }
            }
            this.m_length = (this.m_length + mergingLine.m_length) - 1;
            this.UpdateIsFunctional();
            if (newlyAddedBlock != null)
            {
                this.m_isFunctional &= newlyAddedBlock.ConveyorSegment.CubeBlock.IsFunctional;
                this.m_isWorking &= this.m_isFunctional;
            }
        }

        private static bool PositionIsInSection(Vector3I position, Vector3I sectionStart, SectionInformation section, out int sectionLength)
        {
            sectionLength = 0;
            Vector3I intVector = Base6Directions.GetIntVector(section.Direction);
            Vector3I vectori2 = position - sectionStart;
            switch (Base6Directions.GetAxis(section.Direction))
            {
                case Base6Directions.Axis.ForwardBackward:
                    sectionLength = intVector.Z * vectori2.Z;
                    break;

                case Base6Directions.Axis.LeftRight:
                    sectionLength = intVector.X * vectori2.X;
                    break;

                case Base6Directions.Axis.UpDown:
                    sectionLength = intVector.Y * vectori2.Y;
                    break;

                default:
                    break;
            }
            return ((sectionLength >= 0) && ((sectionLength < section.Length) && (vectori2.RectangularLength() == sectionLength)));
        }

        public void PrepareForDraw(MyCubeGrid grid)
        {
            if ((this.m_queue1.Count != 0) || (this.m_queue2.Count != 0))
            {
                MySinglyLinkedList<MyConveyorPacket>.Enumerator enumerator;
                if (!this.m_stopped1)
                {
                    using (enumerator = this.m_queue1.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.MoveRelative(0.015625f);
                        }
                    }
                }
                if (!this.m_stopped2)
                {
                    using (enumerator = this.m_queue2.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.MoveRelative(0.015625f);
                        }
                    }
                }
            }
        }

        public void RecalculateConductivity()
        {
            ConveyorLinePosition position;
            this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FULL;
            MyObjectBuilder_ConveyorLine.LineConductivity fULL = MyObjectBuilder_ConveyorLine.LineConductivity.FULL;
            MyObjectBuilder_ConveyorLine.LineConductivity lineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FULL;
            if ((this.m_endpoint1 != null) && (this.m_endpoint1 is MyMultilineConveyorEndpoint))
            {
                MyMultilineConveyorEndpoint endpoint = this.m_endpoint1 as MyMultilineConveyorEndpoint;
                foreach (BlockLinePositionInformation information in GetBlockLinePositions(endpoint.CubeBlock))
                {
                    position = endpoint.PositionToGridCoords(information.Position);
                    if (position.Equals(this.m_endpointPosition1))
                    {
                        fULL = information.LineConductivity;
                        break;
                    }
                }
            }
            if ((this.m_endpoint2 != null) && (this.m_endpoint2 is MyMultilineConveyorEndpoint))
            {
                MyMultilineConveyorEndpoint endpoint2 = this.m_endpoint2 as MyMultilineConveyorEndpoint;
                foreach (BlockLinePositionInformation information2 in GetBlockLinePositions(endpoint2.CubeBlock))
                {
                    position = endpoint2.PositionToGridCoords(information2.Position);
                    if (position.Equals(this.m_endpointPosition2))
                    {
                        lineConductivity = information2.LineConductivity;
                        if (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)
                        {
                            lineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD;
                        }
                        else if (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)
                        {
                            lineConductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD;
                        }
                        break;
                    }
                }
            }
            if (((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.NONE) || ((lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.NONE) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)))) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)))
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.NONE;
            }
            else if ((((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.FULL) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FULL))) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)))
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD;
            }
            else if ((((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.FULL) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FULL))) || ((fULL == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD) && (lineConductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)))
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD;
            }
            else
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FULL;
            }
        }

        private void RecalculatePacketPositions()
        {
            int sectionStartPosition = 0;
            Vector3I localGridPosition = this.m_endpointPosition1.LocalGridPosition;
            Base6Directions.Direction dir = this.m_endpointPosition1.Direction;
            int num2 = 0;
            int length = this.Length;
            if (this.m_sections != null)
            {
                num2 = this.m_sections.Count - 1;
                sectionStartPosition = this.Length - this.m_sections[num2].Length;
                localGridPosition = this.m_endpointPosition2.LocalGridPosition - (Base6Directions.GetIntVector(dir) * this.m_sections[num2].Length);
                dir = this.m_sections[num2].Direction;
                length = this.m_sections[num2].Length;
            }
            Base6Directions.Direction perpendicular = Base6Directions.GetPerpendicular(dir);
            MySinglyLinkedList<MyConveyorPacket>.Enumerator enumerator = this.m_queue1.GetEnumerator();
            bool flag = enumerator.MoveNext();
            while (true)
            {
                if (sectionStartPosition < 0)
                {
                    break;
                }
                while (true)
                {
                    if (flag && (enumerator.Current.LinePosition >= sectionStartPosition))
                    {
                        enumerator.Current.SetLocalPosition(localGridPosition, sectionStartPosition, this.m_cubeGrid.GridSize, dir, perpendicular);
                        enumerator.Current.SetSegmentLength(this.m_cubeGrid.GridSize);
                        flag = enumerator.MoveNext();
                        continue;
                    }
                    if ((this.m_sections != null) && flag)
                    {
                        num2--;
                        if (num2 >= 0)
                        {
                            length = this.m_sections[num2].Length;
                            sectionStartPosition -= length;
                            localGridPosition -= Base6Directions.GetIntVector(this.m_sections[num2].Direction) * length;
                            break;
                        }
                    }
                    break;
                }
            }
            sectionStartPosition = 0;
            localGridPosition = this.m_endpointPosition2.LocalGridPosition;
            dir = this.m_endpointPosition2.Direction;
            perpendicular = Base6Directions.GetFlippedDirection(perpendicular);
            num2 = 0;
            length = this.Length;
            if (this.m_sections != null)
            {
                length = this.m_sections[num2].Length;
                sectionStartPosition = this.Length - length;
                dir = Base6Directions.GetFlippedDirection(this.m_sections[num2].Direction);
                localGridPosition = this.m_endpointPosition1.LocalGridPosition - (Base6Directions.GetIntVector(dir) * length);
            }
            MySinglyLinkedList<MyConveyorPacket>.Enumerator enumerator2 = this.m_queue2.GetEnumerator();
            bool flag2 = enumerator2.MoveNext();
            while (true)
            {
                if (sectionStartPosition < 0)
                {
                    break;
                }
                while (true)
                {
                    if (flag2 && (enumerator2.Current.LinePosition >= sectionStartPosition))
                    {
                        enumerator2.Current.SetLocalPosition(localGridPosition, sectionStartPosition, this.m_cubeGrid.GridSize, dir, perpendicular);
                        enumerator2.Current.SetSegmentLength(this.m_cubeGrid.GridSize);
                        flag2 = enumerator2.MoveNext();
                        continue;
                    }
                    if ((this.m_sections != null) && flag2)
                    {
                        num2++;
                        if (num2 < this.m_sections.Count)
                        {
                            length = this.m_sections[num2].Length;
                            sectionStartPosition -= length;
                            localGridPosition -= Base6Directions.GetIntVector(Base6Directions.GetFlippedDirection(this.m_sections[num2].Direction)) * length;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        public MyConveyorLine RemovePortion(Vector3I startPosition, Vector3I endPosition)
        {
            List<SectionInformation> list;
            bool flag;
            if (this.IsCircular)
            {
                this.RotateCircularLine(startPosition);
            }
            if (!(startPosition != endPosition))
            {
                goto TR_001C;
            }
            else
            {
                flag = false;
                if (this.m_sections != null)
                {
                    Vector3I localGridPosition = this.m_endpointPosition1.LocalGridPosition;
                    foreach (SectionInformation information in this.m_sections)
                    {
                        int num3;
                        int num4;
                        bool flag2 = PositionIsInSection(startPosition, localGridPosition, information, out num3);
                        bool flag3 = PositionIsInSection(endPosition, localGridPosition, information, out num4);
                        if (flag2 & flag3)
                        {
                            if (num4 < num3)
                            {
                                flag = true;
                            }
                        }
                        else if (flag3)
                        {
                            flag = true;
                        }
                        else if (!flag2)
                        {
                            localGridPosition = (Vector3I) (localGridPosition + (Base6Directions.GetIntVector(information.Direction) * information.Length));
                            continue;
                        }
                        break;
                    }
                }
                else if (Vector3I.DistanceManhattan(this.m_endpointPosition1.LocalGridPosition, endPosition) < Vector3I.DistanceManhattan(this.m_endpointPosition1.LocalGridPosition, startPosition))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                Vector3I vectori1 = startPosition;
                startPosition = endPosition;
                endPosition = vectori1;
            }
        TR_001C:
            list = null;
            List<SectionInformation> list2 = null;
            ConveyorLinePosition position = new ConveyorLinePosition(startPosition, this.m_endpointPosition2.Direction);
            ConveyorLinePosition position2 = new ConveyorLinePosition(endPosition, this.m_endpointPosition1.Direction);
            ConveyorLinePosition position3 = new ConveyorLinePosition();
            int num = 0;
            int lengthLimit = 0;
            if (this.m_sections == null)
            {
                num = startPosition.RectangularDistance(this.m_endpointPosition1.LocalGridPosition);
                lengthLimit = endPosition.RectangularDistance(this.m_endpointPosition2.LocalGridPosition);
            }
            else
            {
                m_tmpSections1.Clear();
                m_tmpSections2.Clear();
                SplitSections(this.m_sections, this.Length, this.m_endpointPosition1.LocalGridPosition, startPosition, m_tmpSections1, m_tmpSections2, out position, out position2, out num);
                lengthLimit = this.Length - num;
                if (m_tmpSections1.Count > 1)
                {
                    list = new List<SectionInformation>();
                    list.AddList<SectionInformation>(m_tmpSections1);
                }
                if (!(startPosition != endPosition))
                {
                    if (m_tmpSections2.Count > 1)
                    {
                        new List<SectionInformation>().AddList<SectionInformation>(m_tmpSections2);
                    }
                }
                else
                {
                    int num5;
                    m_tmpSections1.Clear();
                    SplitSections(m_tmpSections2, lengthLimit, position2.LocalGridPosition, endPosition, null, m_tmpSections1, out position3, out position2, out num5);
                    lengthLimit -= num5;
                    if (m_tmpSections1.Count > 1)
                    {
                        list2 = new List<SectionInformation>();
                        list2.AddList<SectionInformation>(m_tmpSections1);
                    }
                }
                m_tmpSections1.Clear();
                m_tmpSections2.Clear();
            }
            MyConveyorLine line = null;
            if ((num <= 1) || (num < lengthLimit))
            {
                if ((num > 1) || ((num > 0) && (this.m_endpoint1 != null)))
                {
                    line = new MyConveyorLine();
                    line.InitAfterSplit(this.m_endpointPosition1, position, list, num, this.m_cubeGrid, this.m_type);
                    line.InitEndpoints(this.m_endpoint1, null);
                }
                this.InitAfterSplit(position2, this.m_endpointPosition2, list2, lengthLimit, this.m_cubeGrid, this.m_type);
                this.InitEndpoints(null, this.m_endpoint2);
            }
            else
            {
                if ((lengthLimit > 1) || ((lengthLimit > 0) && (this.m_endpoint2 != null)))
                {
                    line = new MyConveyorLine();
                    line.InitAfterSplit(position2, this.m_endpointPosition2, list2, lengthLimit, this.m_cubeGrid, this.m_type);
                    line.InitEndpoints(null, this.m_endpoint2);
                }
                this.InitAfterSplit(this.m_endpointPosition1, position, list, num, this.m_cubeGrid, this.m_type);
                this.InitEndpoints(this.m_endpoint1, null);
            }
            this.RecalculateConductivity();
            if (line != null)
            {
                line.RecalculateConductivity();
            }
            return line;
        }

        public unsafe void Reverse()
        {
            ConveyorLinePosition position = this.m_endpointPosition1;
            this.m_endpointPosition1 = this.m_endpointPosition2;
            this.m_endpointPosition2 = position;
            IMyConveyorEndpoint endpoint = this.m_endpoint1;
            this.m_endpoint1 = this.m_endpoint2;
            this.m_endpoint2 = endpoint;
            if (this.m_conductivity == MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD)
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD;
            }
            else if (this.m_conductivity == MyObjectBuilder_ConveyorLine.LineConductivity.BACKWARD)
            {
                this.m_conductivity = MyObjectBuilder_ConveyorLine.LineConductivity.FORWARD;
            }
            if (this.m_sections != null)
            {
                for (int i = 0; i < ((this.m_sections.Count + 1) / 2); i++)
                {
                    int num2 = (this.m_sections.Count - i) - 1;
                    SectionInformation information = this.m_sections[i];
                    SectionInformation* informationPtr1 = (SectionInformation*) ref information;
                    informationPtr1->Direction = Base6Directions.GetFlippedDirection(information.Direction);
                    SectionInformation information2 = this.m_sections[num2];
                    SectionInformation* informationPtr2 = (SectionInformation*) ref information2;
                    informationPtr2->Direction = Base6Directions.GetFlippedDirection(information2.Direction);
                    this.m_sections[i] = information2;
                    this.m_sections[num2] = information;
                }
            }
        }

        private void RotateCircularLine(Vector3I position)
        {
            List<SectionInformation> list = new List<SectionInformation>(this.m_sections.Count + 1);
            Vector3I localGridPosition = this.m_endpointPosition1.LocalGridPosition;
            int num = 0;
            while (true)
            {
                if (num < this.m_sections.Count)
                {
                    SectionInformation information = this.m_sections[num];
                    int num2 = 0;
                    Vector3I intVector = Base6Directions.GetIntVector(information.Direction);
                    Vector3I vectori3 = position - localGridPosition;
                    Base6Directions.Axis axis = Base6Directions.GetAxis(information.Direction);
                    switch (axis)
                    {
                        case Base6Directions.Axis.ForwardBackward:
                            num2 = intVector.Z * vectori3.Z;
                            break;

                        case Base6Directions.Axis.LeftRight:
                            num2 = intVector.X * vectori3.X;
                            break;

                        case Base6Directions.Axis.UpDown:
                            num2 = intVector.Y * vectori3.Y;
                            break;

                        default:
                            break;
                    }
                    if (((num2 <= 0) || (num2 > information.Length)) || (vectori3.RectangularLength() != num2))
                    {
                        localGridPosition = (Vector3I) (localGridPosition + (Base6Directions.GetIntVector(information.Direction) * information.Length));
                        num++;
                        continue;
                    }
                    SectionInformation item = new SectionInformation {
                        Direction = this.m_sections[num].Direction,
                        Length = (this.m_sections[num].Length - num2) + 1
                    };
                    list.Add(item);
                    int num3 = num + 1;
                    while (true)
                    {
                        if (num3 >= (this.m_sections.Count - 1))
                        {
                            SectionInformation information3 = new SectionInformation {
                                Direction = this.m_sections[0].Direction,
                                Length = (this.m_sections[0].Length + this.m_sections[this.m_sections.Count - 1].Length) - 1
                            };
                            list.Add(information3);
                            int num4 = 1;
                            while (true)
                            {
                                if (num4 >= num)
                                {
                                    SectionInformation information4 = new SectionInformation {
                                        Direction = this.m_sections[num].Direction,
                                        Length = num2
                                    };
                                    list.Add(information4);
                                    break;
                                }
                                list.Add(this.m_sections[num4]);
                                num4++;
                            }
                            break;
                        }
                        list.Add(this.m_sections[num3]);
                        num3++;
                    }
                }
                this.m_sections = list;
                this.m_endpointPosition2 = new ConveyorLinePosition(position, Base6Directions.GetFlippedDirection(this.m_sections[0].Direction));
                this.m_endpointPosition1 = this.m_endpointPosition2.GetConnectingPosition();
                return;
            }
        }

        public void SetEndpoint(int index, IMyConveyorEndpoint endpoint)
        {
            if (index == 0)
            {
                this.m_endpoint1 = endpoint;
            }
            else
            {
                if (index != 1)
                {
                    throw new IndexOutOfRangeException();
                }
                this.m_endpoint2 = endpoint;
            }
        }

        private static void SplitSections(List<SectionInformation> sections, int lengthLimit, Vector3I startPosition, Vector3I splittingPosition, List<SectionInformation> sections1, List<SectionInformation> sections2, out ConveyorLinePosition newPosition1, out ConveyorLinePosition newPosition2, out int line1Length)
        {
            bool flag = false;
            int num = 0;
            line1Length = 0;
            Vector3I sectionStart = startPosition;
            SectionInformation section = new SectionInformation();
            int sectionLength = 0;
            num = 0;
            while (true)
            {
                if (num < sections.Count)
                {
                    section = sections[num];
                    if (!PositionIsInSection(splittingPosition, sectionStart, section, out sectionLength))
                    {
                        line1Length += section.Length;
                        sectionStart = (Vector3I) (sectionStart + (Base6Directions.GetIntVector(section.Direction) * section.Length));
                        num++;
                        continue;
                    }
                    line1Length += sectionLength;
                    if (sectionLength == 0)
                    {
                        flag = true;
                    }
                }
                newPosition2 = new ConveyorLinePosition(splittingPosition, section.Direction);
                newPosition1 = !flag ? new ConveyorLinePosition(splittingPosition, Base6Directions.GetFlippedDirection(section.Direction)) : new ConveyorLinePosition(splittingPosition, Base6Directions.GetFlippedDirection(sections[num - 1].Direction));
                int num3 = flag ? num : (num + 1);
                int num4 = sections.Count - num;
                SectionInformation item = new SectionInformation();
                if (sections1 != null)
                {
                    int num5 = 0;
                    while (true)
                    {
                        if (num5 >= (num3 - 1))
                        {
                            if (flag)
                            {
                                sections1.Add(sections[num3 - 1]);
                            }
                            else
                            {
                                item.Direction = sections[num3 - 1].Direction;
                                item.Length = sectionLength;
                                sections1.Add(item);
                            }
                            break;
                        }
                        sections1.Add(sections[num5]);
                        num5++;
                    }
                }
                item.Direction = sections[num].Direction;
                item.Length = sections[num].Length - sectionLength;
                sections2.Add(item);
                for (int i = 1; i < num4; i++)
                {
                    sections2.Add(sections[num + i]);
                }
                return;
            }
        }

        public void StopQueuesIfNeeded()
        {
            if (this.m_queuePosition == 0f)
            {
                if ((!this.m_stopped1 && (this.m_queue1.Count != 0)) && (this.m_queue1.First().LinePosition >= (this.Length - 1)))
                {
                    this.m_stopped1 = true;
                }
                if ((!this.m_stopped2 && (this.m_queue2.Count != 0)) && (this.m_queue2.First().LinePosition >= (this.Length - 1)))
                {
                    this.m_stopped2 = true;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            new LinePositionEnumerator(this);

        public override string ToString() => 
            (this.m_endpointPosition1.LocalGridPosition.ToString() + " <-> " + this.m_endpointPosition2.LocalGridPosition.ToString());

        public void Update()
        {
            this.m_queuePosition += 0.015625f;
            if (this.m_queuePosition >= 1f)
            {
                this.BigUpdate();
            }
        }

        public void UpdateIsFunctional()
        {
            this.m_isFunctional = this.UpdateIsFunctionalInternal();
            this.UpdateIsWorking();
        }

        private bool UpdateIsFunctionalInternal()
        {
            if (((this.m_endpoint1 == null) || ((this.m_endpoint2 == null) || !this.m_endpoint1.CubeBlock.IsFunctional)) || !this.m_endpoint2.CubeBlock.IsFunctional)
            {
                return false;
            }
            MyCubeGrid cubeGrid = this.m_endpoint1.CubeBlock.CubeGrid;
            using (IEnumerator<Vector3I> enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Vector3I current = enumerator.Current;
                    MySlimBlock cubeBlock = cubeGrid.GetCubeBlock(current);
                    if ((cubeBlock != null) && ((cubeBlock.FatBlock != null) && !cubeBlock.FatBlock.IsFunctional))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public MyResourceStateEnum UpdateIsWorking()
        {
            MyResourceStateEnum noPower = MyResourceStateEnum.NoPower;
            if (!this.m_isFunctional)
            {
                this.m_isWorking = false;
                return noPower;
            }
            if (this.IsDisconnected)
            {
                this.m_isWorking = false;
                return noPower;
            }
            if (MySession.Static == null)
            {
                this.m_isWorking = false;
                return noPower;
            }
            MyCubeGrid grid = this.GetGrid();
            if (grid.GridSystems.ResourceDistributor != null)
            {
                noPower = grid.GridSystems.ResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true);
                bool flag = noPower != MyResourceStateEnum.NoPower;
                if (this.m_isWorking != flag)
                {
                    this.m_isWorking = flag;
                    grid.GridSystems.ConveyorSystem.FlagForRecomputation();
                }
            }
            return noPower;
        }

        public bool IsFunctional =>
            this.m_isFunctional;

        public bool IsWorking =>
            this.m_isWorking;

        public int Length =>
            this.m_length;

        public bool IsDegenerate =>
            ((this.Length == 1) && this.HasNullEndpoints);

        public bool IsCircular =>
            ((this.Length != 1) && this.m_endpointPosition1.GetConnectingPosition().Equals(this.m_endpointPosition2));

        public bool HasNullEndpoints =>
            ((this.m_endpoint1 == null) && ReferenceEquals(this.m_endpoint2, null));

        public bool IsDisconnected =>
            ((this.m_endpoint1 == null) || ReferenceEquals(this.m_endpoint2, null));

        public bool IsEmpty =>
            ((this.m_queue1.Count == 0) && (this.m_queue2.Count == 0));

        public MyObjectBuilder_ConveyorLine.LineType Type =>
            this.m_type;

        public MyObjectBuilder_ConveyorLine.LineConductivity Conductivity =>
            this.m_conductivity;

        [StructLayout(LayoutKind.Sequential)]
        public struct BlockLinePositionInformation
        {
            public ConveyorLinePosition Position;
            public VRage.Game.MyObjectBuilder_ConveyorLine.LineType LineType;
            public VRage.Game.MyObjectBuilder_ConveyorLine.LineConductivity LineConductivity;
        }

        public class InvertedConductivity : IDisposable
        {
            public InvertedConductivity()
            {
                MyConveyorLine.m_invertedConductivity = !MyConveyorLine.m_invertedConductivity;
            }

            public void Dispose()
            {
                MyConveyorLine.m_invertedConductivity = !MyConveyorLine.m_invertedConductivity;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LinePositionEnumerator : IEnumerator<Vector3I>, IDisposable, IEnumerator
        {
            private MyConveyorLine m_line;
            private Vector3I m_currentPosition;
            private Vector3I m_direction;
            private int m_index;
            private int m_sectionIndex;
            private int m_sectionLength;
            public LinePositionEnumerator(MyConveyorLine line)
            {
                this.m_line = line;
                this.m_currentPosition = line.m_endpointPosition1.LocalGridPosition;
                this.m_direction = line.m_endpointPosition1.VectorDirection;
                this.m_index = 0;
                this.m_sectionIndex = 0;
                this.m_sectionLength = this.m_line.m_length;
                this.UpdateSectionLength();
            }

            public Vector3I Current =>
                this.m_currentPosition;
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                this.m_index++;
                this.m_currentPosition = (Vector3I) (this.m_currentPosition + this.m_direction);
                if (this.m_index >= this.m_sectionLength)
                {
                    this.m_index = 0;
                    this.m_sectionIndex++;
                    if (this.m_line.m_sections == null)
                    {
                        goto TR_0000;
                    }
                    else if (this.m_sectionIndex < this.m_line.m_sections.Count)
                    {
                        this.m_direction = Base6Directions.GetIntVector(this.m_line.m_sections[this.m_sectionIndex].Direction);
                        this.UpdateSectionLength();
                    }
                    else
                    {
                        goto TR_0000;
                    }
                }
                return true;
            TR_0000:
                return false;
            }

            public void Reset()
            {
                this.m_currentPosition = this.m_line.m_endpointPosition1.LocalGridPosition;
                this.m_direction = this.m_line.m_endpointPosition1.VectorDirection;
                this.m_index = 0;
                this.m_sectionIndex = 0;
                this.m_sectionLength = this.m_line.m_length;
                this.UpdateSectionLength();
            }

            object IEnumerator.Current =>
                this.Current;
            private void UpdateSectionLength()
            {
                if ((this.m_line.m_sections != null) && (this.m_line.m_sections.Count != 0))
                {
                    this.m_sectionLength = this.m_line.m_sections[this.m_sectionIndex].Length;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SectionInformation
        {
            public VRageMath.Base6Directions.Direction Direction;
            public int Length;
            public void Reverse()
            {
                this.Direction = Base6Directions.GetFlippedDirection(this.Direction);
            }

            public override string ToString() => 
                (this.Length.ToString() + " -> " + this.Direction.ToString());
        }
    }
}


namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Algorithms;
    using VRage.Game.Models;
    using VRageMath;

    public class MyMultilineConveyorEndpoint : IMyConveyorEndpoint, IMyPathVertex<IMyConveyorEndpoint>, IEnumerable<IMyPathEdge<IMyConveyorEndpoint>>, IEnumerable
    {
        protected MyConveyorLine[] m_conveyorLines;
        protected static Dictionary<MyDefinitionId, ConveyorLinePosition[]> m_linePositions = new Dictionary<MyDefinitionId, ConveyorLinePosition[]>();
        private MyCubeBlock m_block;
        private MyPathfindingData m_pathfindingData;

        public MyMultilineConveyorEndpoint(MyCubeBlock myBlock)
        {
            this.m_block = myBlock;
            MyConveyorLine.BlockLinePositionInformation[] blockLinePositions = MyConveyorLine.GetBlockLinePositions(myBlock);
            this.m_conveyorLines = new MyConveyorLine[blockLinePositions.Length];
            MyGridConveyorSystem conveyorSystem = myBlock.CubeGrid.GridSystems.ConveyorSystem;
            int index = 0;
            foreach (MyConveyorLine.BlockLinePositionInformation information in blockLinePositions)
            {
                ConveyorLinePosition position = this.PositionToGridCoords(information.Position);
                MyConveyorLine deserializingLine = conveyorSystem.GetDeserializingLine(position);
                if (deserializingLine == null)
                {
                    deserializingLine = new MyConveyorLine();
                    Vector3I? corner = null;
                    deserializingLine.Init(position, position.GetConnectingPosition(), myBlock.CubeGrid, information.LineType, information.LineConductivity, corner);
                    deserializingLine.InitEndpoints(this, null);
                }
                else if (deserializingLine.GetEndpointPosition(0).Equals(position))
                {
                    deserializingLine.SetEndpoint(0, this);
                }
                else if (deserializingLine.GetEndpointPosition(1).Equals(position))
                {
                    deserializingLine.SetEndpoint(1, this);
                }
                this.m_conveyorLines[index] = deserializingLine;
                index++;
            }
            myBlock.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.UpdateLineFunctionality);
            myBlock.CubeGrid.GridSystems.ConveyorSystem.ResourceSink.IsPoweredChanged += new Action(this.UpdateLineFunctionality);
            this.m_pathfindingData = new MyPathfindingData(this);
        }

        public void DebugDraw()
        {
        }

        public MyConveyorLine GetConveyorLine(ConveyorLinePosition position)
        {
            ConveyorLinePosition[] linePositions = this.GetLinePositions();
            for (int i = 0; i < linePositions.Length; i++)
            {
                ConveyorLinePosition position2 = this.PositionToGridCoords(linePositions[i]);
                if (position2.Equals(position))
                {
                    return this.m_conveyorLines[i];
                }
            }
            return null;
        }

        public MyConveyorLine GetConveyorLine(int index)
        {
            if (index >= this.m_conveyorLines.Length)
            {
                throw new IndexOutOfRangeException();
            }
            return this.m_conveyorLines[index];
        }

        protected virtual IMyPathEdge<IMyConveyorEndpoint> GetEdge(int index) => 
            this.m_conveyorLines[index];

        public ConveyorLineEnumerator GetEnumeratorInternal() => 
            new ConveyorLineEnumerator(this);

        public int GetLineCount() => 
            this.m_conveyorLines.Length;

        protected ConveyorLinePosition[] GetLinePositions()
        {
            ConveyorLinePosition[] positionArray = null;
            Dictionary<MyDefinitionId, ConveyorLinePosition[]> linePositions = m_linePositions;
            lock (linePositions)
            {
                if (!m_linePositions.TryGetValue(this.CubeBlock.BlockDefinition.Id, out positionArray))
                {
                    positionArray = GetLinePositions(this.CubeBlock, "detector_conveyor");
                    m_linePositions.Add(this.CubeBlock.BlockDefinition.Id, positionArray);
                }
            }
            return positionArray;
        }

        public static ConveyorLinePosition[] GetLinePositions(MyCubeBlock cubeBlock, string dummyName) => 
            GetLinePositions(cubeBlock, MyModels.GetModelOnlyDummies(cubeBlock.BlockDefinition.Model).Dummies, dummyName);

        public static ConveyorLinePosition[] GetLinePositions(MyCubeBlock cubeBlock, IDictionary<string, MyModelDummy> dummies, string dummyName)
        {
            MyCubeBlockDefinition blockDefinition = cubeBlock.BlockDefinition;
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(blockDefinition.CubeSize);
            Vector3 vector = (new Vector3(blockDefinition.Size) * 0.5f) * cubeSize;
            int num2 = 0;
            foreach (KeyValuePair<string, MyModelDummy> pair in dummies)
            {
                if (pair.Key.ToLower().Contains(dummyName))
                {
                    num2++;
                }
            }
            ConveyorLinePosition[] positionArray = new ConveyorLinePosition[num2];
            int index = 0;
            foreach (KeyValuePair<string, MyModelDummy> pair2 in dummies)
            {
                if (pair2.Key.ToLower().Contains(dummyName))
                {
                    Matrix matrix = pair2.Value.Matrix;
                    ConveyorLinePosition position = new ConveyorLinePosition();
                    Vector3I vectori = Vector3I.Floor(((matrix.Translation + blockDefinition.ModelOffset) + vector) / cubeSize);
                    vectori = Vector3I.Max(Vector3I.Zero, vectori);
                    vectori = Vector3I.Min(blockDefinition.Size - Vector3I.One, vectori);
                    position.LocalGridPosition = vectori - blockDefinition.Center;
                    position.Direction = Base6Directions.GetDirection(Vector3.Normalize(Vector3.DominantAxisProjection(((matrix.Translation + blockDefinition.ModelOffset) + vector) - ((new Vector3(vectori) + Vector3.Half) * cubeSize))));
                    positionArray[index] = position;
                    index++;
                }
            }
            return positionArray;
        }

        protected virtual IMyPathVertex<IMyConveyorEndpoint> GetNeighbor(int index) => 
            this.m_conveyorLines[index].GetOtherVertex(this);

        protected virtual int GetNeighborCount() => 
            this.m_conveyorLines.Length;

        public ConveyorLinePosition GetPosition(int index)
        {
            ConveyorLinePosition[] linePositions = this.GetLinePositions();
            return this.PositionToGridCoords(linePositions[index]);
        }

        public ConveyorLinePosition PositionToGridCoords(ConveyorLinePosition position) => 
            PositionToGridCoords(position, this.CubeBlock);

        public static ConveyorLinePosition PositionToGridCoords(ConveyorLinePosition position, MyCubeBlock cubeBlock)
        {
            ConveyorLinePosition position2 = new ConveyorLinePosition();
            Matrix result = new Matrix();
            cubeBlock.Orientation.GetMatrix(out result);
            position2.LocalGridPosition = (Vector3I) (Vector3I.Round(Vector3.Transform(new Vector3(position.LocalGridPosition), result)) + cubeBlock.Position);
            position2.Direction = cubeBlock.Orientation.TransformDirection(position.Direction);
            return position2;
        }

        public void SetConveyorLine(ConveyorLinePosition position, MyConveyorLine newLine)
        {
            ConveyorLinePosition[] linePositions = this.GetLinePositions();
            for (int i = 0; i < linePositions.Length; i++)
            {
                ConveyorLinePosition position2 = this.PositionToGridCoords(linePositions[i]);
                if (position2.Equals(position))
                {
                    this.m_conveyorLines[i] = newLine;
                    return;
                }
            }
        }

        IEnumerator<IMyPathEdge<IMyConveyorEndpoint>> IEnumerable<IMyPathEdge<IMyConveyorEndpoint>>.GetEnumerator() => 
            this.GetEnumeratorInternal();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumeratorInternal();

        protected void UpdateLineFunctionality()
        {
            MySandboxGame.Static.Invoke(delegate {
                for (int i = 0; i < this.m_conveyorLines.Length; i++)
                {
                    this.m_conveyorLines[i].UpdateIsFunctional();
                }
            }, "MyMultilineConveyorEndpoint::UpdateLineFunctionality");
        }

        float IMyPathVertex<IMyConveyorEndpoint>.EstimateDistanceTo(IMyPathVertex<IMyConveyorEndpoint> other) => 
            Vector3.RectangularDistance((Vector3) (other as IMyConveyorEndpoint).CubeBlock.WorldMatrix.Translation, (Vector3) this.CubeBlock.WorldMatrix.Translation);

        IMyPathEdge<IMyConveyorEndpoint> IMyPathVertex<IMyConveyorEndpoint>.GetEdge(int index) => 
            this.GetEdge(index);

        IMyPathVertex<IMyConveyorEndpoint> IMyPathVertex<IMyConveyorEndpoint>.GetNeighbor(int index) => 
            this.GetNeighbor(index);

        int IMyPathVertex<IMyConveyorEndpoint>.GetNeighborCount() => 
            this.GetNeighborCount();

        public MyCubeBlock CubeBlock =>
            this.m_block;

        MyPathfindingData IMyPathVertex<IMyConveyorEndpoint>.PathfindingData =>
            this.m_pathfindingData;
    }
}


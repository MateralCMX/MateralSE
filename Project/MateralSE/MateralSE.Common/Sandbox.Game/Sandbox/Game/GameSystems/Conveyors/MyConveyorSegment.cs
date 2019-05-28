namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyConveyorSegment
    {
        public Base6Directions.Direction CalculateConnectingDirection(Vector3I connectingPosition)
        {
            Vector3 vec = Vector3.DominantAxisProjection(Vector3.Multiply((new Vector3((Vector3I) (this.CubeBlock.Max + this.CubeBlock.Min)) * 0.5f) - connectingPosition, new Vector3((Vector3I) ((this.CubeBlock.Max - this.CubeBlock.Min) + Vector3I.One)) * 0.5f));
            vec.Normalize();
            return Base6Directions.GetDirection(vec);
        }

        private Vector3I CalculateCornerPosition()
        {
            Vector3I vectori = this.ConnectingPosition2.LocalGridPosition - this.ConnectingPosition1.LocalGridPosition;
            switch (Base6Directions.GetAxis(this.ConnectingPosition1.Direction))
            {
                case Base6Directions.Axis.ForwardBackward:
                    return (Vector3I) (this.ConnectingPosition1.LocalGridPosition + new Vector3I(0, 0, vectori.Z));

                case Base6Directions.Axis.LeftRight:
                    return (Vector3I) (this.ConnectingPosition1.LocalGridPosition + new Vector3I(vectori.X, 0, 0));

                case Base6Directions.Axis.UpDown:
                    return (Vector3I) (this.ConnectingPosition1.LocalGridPosition + new Vector3I(0, vectori.Y, 0));
            }
            return Vector3I.Zero;
        }

        public bool CanConnectTo(ConveyorLinePosition connectingPosition, MyObjectBuilder_ConveyorLine.LineType type)
        {
            if (type != this.ConveyorLine.Type)
            {
                return false;
            }
            if (!connectingPosition.Equals(this.ConnectingPosition1.GetConnectingPosition()))
            {
                return connectingPosition.Equals(this.ConnectingPosition2.GetConnectingPosition());
            }
            return true;
        }

        private void CubeBlock_IsFunctionalChanged()
        {
            this.ConveyorLine.UpdateIsFunctional();
        }

        public void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CONVEYORS)
            {
                Vector3 pointFrom = (Vector3) Vector3.Transform((this.ConnectingPosition1.LocalGridPosition + (this.ConnectingPosition1.VectorDirection * 0.5f)) * this.CubeBlock.CubeGrid.GridSize, this.CubeBlock.CubeGrid.WorldMatrix);
                Vector3 pointTo = (Vector3) Vector3.Transform((this.ConnectingPosition2.LocalGridPosition + (this.ConnectingPosition2.VectorDirection * 0.5f)) * this.CubeBlock.CubeGrid.GridSize, this.CubeBlock.CubeGrid.WorldMatrix);
                Color colorFrom = this.ConveyorLine.IsFunctional ? Color.Orange : Color.DarkRed;
                colorFrom = this.ConveyorLine.IsWorking ? Color.GreenYellow : colorFrom;
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, colorFrom, colorFrom, false, false);
                if ((this.ConveyorLine != null) && MyDebugDrawSettings.DEBUG_DRAW_CONVEYORS_LINE_IDS)
                {
                    MyRenderProxy.DebugDrawText3D((pointFrom + pointTo) * 0.5f, this.ConveyorLine.GetHashCode().ToString(), colorFrom, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
        }

        public void Init(MyCubeBlock myBlock, ConveyorLinePosition a, ConveyorLinePosition b, MyObjectBuilder_ConveyorLine.LineType type, MyObjectBuilder_ConveyorLine.LineConductivity conductivity = 0)
        {
            this.CubeBlock = myBlock;
            this.ConnectingPosition1 = a;
            this.ConnectingPosition2 = b;
            Vector3I neighbourGridPosition = (myBlock as IMyConveyorSegmentBlock).ConveyorSegment.ConnectingPosition1.NeighbourGridPosition;
            this.ConveyorLine = myBlock.CubeGrid.GridSystems.ConveyorSystem.GetDeserializingLine(neighbourGridPosition);
            if (this.ConveyorLine == null)
            {
                this.ConveyorLine = new MyConveyorLine();
                if (this.IsCorner)
                {
                    this.ConveyorLine.Init(a, b, myBlock.CubeGrid, type, conductivity, new Vector3I?(this.CalculateCornerPosition()));
                }
                else
                {
                    Vector3I? corner = null;
                    this.ConveyorLine.Init(a, b, myBlock.CubeGrid, type, conductivity, corner);
                }
            }
            myBlock.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.CubeBlock_IsFunctionalChanged);
        }

        public void SetConveyorLine(MyConveyorLine newLine)
        {
            this.ConveyorLine = newLine;
        }

        public MyConveyorLine ConveyorLine { get; private set; }

        public ConveyorLinePosition ConnectingPosition1 { get; private set; }

        public ConveyorLinePosition ConnectingPosition2 { get; private set; }

        public MyCubeBlock CubeBlock { get; private set; }

        public bool IsCorner
        {
            get
            {
                Vector3I vectorDirection = this.ConnectingPosition1.VectorDirection;
                Vector3I vectori2 = this.ConnectingPosition2.VectorDirection;
                return (Vector3I.Dot(ref vectorDirection, ref vectori2) != -1);
            }
        }
    }
}


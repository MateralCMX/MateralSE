namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyEdgeInfo
    {
        public Vector4 LocalOrthoMatrix;
        private VRageMath.Color m_packedColor;
        public MyStringHash EdgeModel;
        public Base27Directions.Direction PackedNormal0;
        public Base27Directions.Direction PackedNormal1;

        public MyEdgeInfo()
        {
        }

        public MyEdgeInfo(ref Vector3 pos, ref Vector3I edgeDirection, ref Vector3 normal0, ref Vector3 normal1, ref VRageMath.Color color, MyStringHash edgeModel)
        {
            MyEdgeOrientationInfo info = MyCubeGridDefinitions.EdgeOrientations[edgeDirection];
            this.PackedNormal0 = Base27Directions.GetDirection((Vector3) normal0);
            this.PackedNormal1 = Base27Directions.GetDirection((Vector3) normal1);
            this.m_packedColor = color;
            this.EdgeType = info.EdgeType;
            this.LocalOrthoMatrix = Vector4.PackOrthoMatrix(pos, info.Orientation.Forward, info.Orientation.Up);
            this.EdgeModel = edgeModel;
        }

        public VRageMath.Color Color
        {
            get
            {
                VRageMath.Color packedColor = this.m_packedColor;
                packedColor.A = 0;
                return packedColor;
            }
            set
            {
                byte a = this.m_packedColor.A;
                this.m_packedColor = value;
                this.m_packedColor.A = a;
            }
        }

        public MyCubeEdgeType EdgeType
        {
            get => 
                ((MyCubeEdgeType) this.m_packedColor.A);
            set => 
                (this.m_packedColor.A = (byte) value);
        }
    }
}


namespace Sandbox.Definitions
{
    using System;
    using VRageMath;

    public class MyEdgeOrientationInfo
    {
        public readonly Matrix Orientation;
        public readonly MyCubeEdgeType EdgeType;

        public MyEdgeOrientationInfo(Matrix localMatrix, MyCubeEdgeType edgeType)
        {
            this.Orientation = localMatrix;
            this.EdgeType = edgeType;
        }
    }
}


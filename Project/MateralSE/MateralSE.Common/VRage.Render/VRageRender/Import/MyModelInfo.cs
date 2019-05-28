namespace VRageRender.Import
{
    using System;
    using VRageMath;

    public class MyModelInfo
    {
        public int TrianglesCount;
        public int VerticesCount;
        public Vector3 BoundingBoxSize;

        public MyModelInfo(int triCnt, int VertCnt, Vector3 BBsize)
        {
            this.TrianglesCount = triCnt;
            this.VerticesCount = VertCnt;
            this.BoundingBoxSize = BBsize;
        }
    }
}


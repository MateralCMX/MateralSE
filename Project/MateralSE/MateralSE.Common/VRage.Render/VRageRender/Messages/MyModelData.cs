namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyModelData
    {
        public List<MyRuntimeSectionInfo> Sections = new List<MyRuntimeSectionInfo>();
        public BoundingBox AABB;
        public List<int> Indices = new List<int>();
        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();
        public List<Vector2> TexCoords = new List<Vector2>();

        public void Clear()
        {
            this.AABB = BoundingBox.CreateInvalid();
            this.Indices.Clear();
            this.Positions.Clear();
            this.Normals.Clear();
            this.Tangents.Clear();
            this.TexCoords.Clear();
            this.Sections.Clear();
        }
    }
}


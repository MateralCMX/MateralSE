namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Reflection;
    using VRageMath;

    public class MyWrappedCubemap<FaceFormat> where FaceFormat: IMyWrappedCubemapFace
    {
        protected FaceFormat[] m_faces;
        protected int m_resolution;
        public string Name;

        public void PrepareSides()
        {
            int x = this.m_resolution - 1;
            this.Front.CopyRange(new Vector2I(0, -1), new Vector2I(x, -1), this.Top, new Vector2I(0, x), new Vector2I(x, x));
            this.Front.CopyRange(new Vector2I(0, this.m_resolution), new Vector2I(x, this.m_resolution), this.Bottom, new Vector2I(x, x), new Vector2I(0, x));
            this.Front.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Left, new Vector2I(x, 0), new Vector2I(x, x));
            this.Front.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Right, new Vector2I(0, 0), new Vector2I(0, x));
            this.Back.CopyRange(new Vector2I(x, -1), new Vector2I(0, -1), this.Top, new Vector2I(0, 0), new Vector2I(x, 0));
            this.Back.CopyRange(new Vector2I(x, this.m_resolution), new Vector2I(0, this.m_resolution), this.Bottom, new Vector2I(x, 0), new Vector2I(0, 0));
            this.Back.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Right, new Vector2I(x, 0), new Vector2I(x, x));
            this.Back.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Left, new Vector2I(0, 0), new Vector2I(0, x));
            this.Left.CopyRange(new Vector2I(x, -1), new Vector2I(0, -1), this.Top, new Vector2I(0, x), new Vector2I(0, 0));
            this.Left.CopyRange(new Vector2I(x, this.m_resolution), new Vector2I(0, this.m_resolution), this.Bottom, new Vector2I(x, x), new Vector2I(x, 0));
            this.Left.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Front, new Vector2I(0, 0), new Vector2I(0, x));
            this.Left.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Back, new Vector2I(x, 0), new Vector2I(x, x));
            this.Right.CopyRange(new Vector2I(x, -1), new Vector2I(0, -1), this.Top, new Vector2I(x, 0), new Vector2I(x, x));
            this.Right.CopyRange(new Vector2I(x, this.m_resolution), new Vector2I(0, this.m_resolution), this.Bottom, new Vector2I(0, 0), new Vector2I(0, x));
            this.Right.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Back, new Vector2I(0, 0), new Vector2I(0, x));
            this.Right.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Front, new Vector2I(x, 0), new Vector2I(x, x));
            this.Top.CopyRange(new Vector2I(0, this.m_resolution), new Vector2I(x, this.m_resolution), this.Front, new Vector2I(0, 0), new Vector2I(x, 0));
            this.Top.CopyRange(new Vector2I(0, -1), new Vector2I(x, -1), this.Back, new Vector2I(x, 0), new Vector2I(0, 0));
            this.Top.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Right, new Vector2I(x, 0), new Vector2I(0, 0));
            this.Top.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Left, new Vector2I(0, 0), new Vector2I(x, 0));
            this.Bottom.CopyRange(new Vector2I(0, this.m_resolution), new Vector2I(x, this.m_resolution), this.Front, new Vector2I(x, x), new Vector2I(0, x));
            this.Bottom.CopyRange(new Vector2I(0, -1), new Vector2I(x, -1), this.Back, new Vector2I(0, x), new Vector2I(x, x));
            this.Bottom.CopyRange(new Vector2I(-1, 0), new Vector2I(-1, x), this.Right, new Vector2I(x, x), new Vector2I(0, x));
            this.Bottom.CopyRange(new Vector2I(this.m_resolution, 0), new Vector2I(this.m_resolution, x), this.Left, new Vector2I(0, x), new Vector2I(x, x));
            for (int i = 0; i < 6; i++)
            {
                this.Faces[i].FinishFace($"{this.Name}_{MyCubemapHelpers.GetNameForFace(i)}");
            }
        }

        public FaceFormat Left =>
            this.m_faces[2];

        public FaceFormat Right =>
            this.m_faces[3];

        public FaceFormat Top =>
            this.m_faces[4];

        public FaceFormat Bottom =>
            this.m_faces[5];

        public FaceFormat Front =>
            this.m_faces[0];

        public FaceFormat Back =>
            this.m_faces[1];

        public int Resolution =>
            this.m_resolution;

        public FaceFormat this[int i] =>
            this.m_faces[i];

        public FaceFormat[] Faces =>
            this.m_faces;
    }
}


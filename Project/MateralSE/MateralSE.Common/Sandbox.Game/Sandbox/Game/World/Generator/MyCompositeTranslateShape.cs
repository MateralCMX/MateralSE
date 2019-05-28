namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Voxels;
    using VRageMath;

    public class MyCompositeTranslateShape : IMyCompositeShape
    {
        private Vector3 m_translation;
        private readonly IMyCompositeShape m_shape;

        public MyCompositeTranslateShape(IMyCompositeShape shape, Vector3 translation)
        {
            this.m_shape = shape;
            this.m_translation = -translation;
        }

        public void Close()
        {
            this.m_shape.Close();
        }

        public void ComputeContent(MyStorageData storage, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, int lodVoxelSize)
        {
            Vector3I vectori = ((Vector3I) this.m_translation) >> lodIndex;
            this.m_shape.ComputeContent(storage, lodIndex, (Vector3I) (lodVoxelRangeMin + vectori), (Vector3I) (lodVoxelRangeMax + vectori), lodVoxelSize);
        }

        public ContainmentType Contains(ref BoundingBox queryBox, ref BoundingSphere querySphere, int lodVoxelSize)
        {
            BoundingBox box = queryBox;
            box.Translate(this.m_translation);
            BoundingSphere sphere = querySphere.Translate(ref this.m_translation);
            return this.m_shape.Contains(ref box, ref sphere, lodVoxelSize);
        }

        public void DebugDraw(ref MatrixD worldMatrix, Color color)
        {
            MatrixD xd = MatrixD.CreateTranslation(-this.m_translation) * worldMatrix;
            this.m_shape.DebugDraw(ref xd, color);
        }

        public float SignedDistance(ref Vector3 localPos, int lodVoxelSize)
        {
            Vector3 vector = localPos + this.m_translation;
            return this.m_shape.SignedDistance(ref vector, lodVoxelSize);
        }
    }
}


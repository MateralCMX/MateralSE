namespace Sandbox.Engine.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyUtilRandomVector3ByDeviatingVector
    {
        private Matrix m_matrix;
        public MyUtilRandomVector3ByDeviatingVector(Vector3 originalVector)
        {
            this.m_matrix = Matrix.CreateFromDir(originalVector);
        }

        public Vector3 GetNext(float maxAngle)
        {
            float randomFloat = MyUtils.GetRandomFloat(-maxAngle, maxAngle);
            float angle = MyUtils.GetRandomFloat(0f, 6.283185f);
            return Vector3.TransformNormal(-new Vector3(MyMath.FastSin(randomFloat) * MyMath.FastCos(angle), MyMath.FastSin(randomFloat) * MyMath.FastSin(angle), MyMath.FastCos(randomFloat)), this.m_matrix);
        }

        public static Vector3 GetRandom(Vector3 originalVector, float maxAngle)
        {
            if (maxAngle == 0f)
            {
                return originalVector;
            }
            MyUtilRandomVector3ByDeviatingVector vector = new MyUtilRandomVector3ByDeviatingVector(originalVector);
            return vector.GetNext(maxAngle);
        }
    }
}


namespace BulletXNA.LinearMath
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct IndexedMatrix
    {
        private static IndexedMatrix _identity;
        public IndexedBasisMatrix _basis;
        public IndexedVector3 _origin;
        public static IndexedMatrix Identity =>
            _identity;
        public IndexedMatrix(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33, float m41, float m42, float m43)
        {
            this._basis = new IndexedBasisMatrix(m11, m12, m13, m21, m22, m23, m31, m32, m33);
            this._origin = new IndexedVector3(m41, m42, m43);
        }

        public IndexedMatrix(IndexedBasisMatrix basis, IndexedVector3 origin)
        {
            this._basis = basis;
            this._origin = origin;
        }

        public static IndexedVector3 operator *(IndexedMatrix matrix1, IndexedVector3 v) => 
            new IndexedVector3(matrix1._basis._Row0.Dot(ref v) + matrix1._origin.X, matrix1._basis._Row1.Dot(ref v) + matrix1._origin.Y, matrix1._basis._Row2.Dot(ref v) + matrix1._origin.Z);

        public static IndexedMatrix operator *(IndexedMatrix matrix1, IndexedMatrix matrix2)
        {
            IndexedMatrix matrix;
            matrix._basis = (IndexedBasisMatrix) (matrix1._basis * matrix2._basis);
            matrix._origin = matrix1 * matrix2._origin;
            return matrix;
        }

        public IndexedMatrix Inverse()
        {
            IndexedBasisMatrix basis = this._basis.Transpose();
            return new IndexedMatrix(basis, basis * -this._origin);
        }

        static IndexedMatrix()
        {
            _identity = new IndexedMatrix(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
        }
    }
}


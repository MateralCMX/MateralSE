namespace BulletXNA.LinearMath
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct IndexedBasisMatrix
    {
        public IndexedVector3 _Row0;
        public IndexedVector3 _Row1;
        public IndexedVector3 _Row2;
        public IndexedBasisMatrix(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            this._Row0 = new IndexedVector3(m11, m12, m13);
            this._Row1 = new IndexedVector3(m21, m22, m23);
            this._Row2 = new IndexedVector3(m31, m32, m33);
        }

        public IndexedVector3 GetRow(int i)
        {
            switch (i)
            {
                case 0:
                    return this._Row0;

                case 1:
                    return this._Row1;

                case 2:
                    return this._Row2;
            }
            return IndexedVector3.Zero;
        }

        public float this[int i, int j]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        switch (j)
                        {
                            case 0:
                                return this._Row0.X;

                            case 1:
                                return this._Row0.Y;

                            case 2:
                                return this._Row0.Z;

                            default:
                                break;
                        }
                        break;

                    case 1:
                        switch (j)
                        {
                            case 0:
                                return this._Row1.X;

                            case 1:
                                return this._Row1.Y;

                            case 2:
                                return this._Row1.Z;

                            default:
                                break;
                        }
                        break;

                    case 2:
                        switch (j)
                        {
                            case 0:
                                return this._Row2.X;

                            case 1:
                                return this._Row2.Y;

                            case 2:
                                return this._Row2.Z;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                return 0f;
            }
            set
            {
                switch (i)
                {
                    case 0:
                        switch (j)
                        {
                            case 0:
                                this._Row0.X = value;
                                return;

                            case 1:
                                this._Row0.Y = value;
                                return;

                            case 2:
                                this._Row0.Z = value;
                                return;
                        }
                        return;

                    case 1:
                        switch (j)
                        {
                            case 0:
                                this._Row1.X = value;
                                return;

                            case 1:
                                this._Row1.Y = value;
                                return;

                            case 2:
                                this._Row1.Z = value;
                                return;
                        }
                        return;

                    case 2:
                        switch (j)
                        {
                            case 0:
                                this._Row2.X = value;
                                return;

                            case 1:
                                this._Row2.Y = value;
                                return;

                            case 2:
                                this._Row2.Z = value;
                                return;
                        }
                        return;
                }
            }
        }
        public IndexedVector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this._Row0;

                    case 1:
                        return this._Row1;

                    case 2:
                        return this._Row2;
                }
                return IndexedVector3.Zero;
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this._Row0 = value;
                        return;

                    case 1:
                        this._Row1 = value;
                        return;

                    case 2:
                        this._Row2 = value;
                        return;
                }
            }
        }
        public static IndexedVector3 operator *(IndexedBasisMatrix m, IndexedVector3 v) => 
            new IndexedVector3(m._Row0.Dot(ref v), m._Row1.Dot(ref v), m._Row2.Dot(ref v));

        public static IndexedBasisMatrix operator *(IndexedBasisMatrix m1, IndexedBasisMatrix m2) => 
            new IndexedBasisMatrix(m2.TDotX(ref m1._Row0), m2.TDotY(ref m1._Row0), m2.TDotZ(ref m1._Row0), m2.TDotX(ref m1._Row1), m2.TDotY(ref m1._Row1), m2.TDotZ(ref m1._Row1), m2.TDotX(ref m1._Row2), m2.TDotY(ref m1._Row2), m2.TDotZ(ref m1._Row2));

        public float TDotX(ref IndexedVector3 v) => 
            (((this._Row0.X * v.X) + (this._Row1.X * v.Y)) + (this._Row2.X * v.Z));

        public float TDotY(ref IndexedVector3 v) => 
            (((this._Row0.Y * v.X) + (this._Row1.Y * v.Y)) + (this._Row2.Y * v.Z));

        public float TDotZ(ref IndexedVector3 v) => 
            (((this._Row0.Z * v.X) + (this._Row1.Z * v.Y)) + (this._Row2.Z * v.Z));

        public IndexedBasisMatrix Inverse()
        {
            IndexedVector3 v = new IndexedVector3(this.Cofac(1, 1, 2, 2), this.Cofac(1, 2, 2, 0), this.Cofac(1, 0, 2, 1));
            float num2 = 1f / this[0].Dot(v);
            return new IndexedBasisMatrix(v.X * num2, this.Cofac(0, 2, 2, 1) * num2, this.Cofac(0, 1, 1, 2) * num2, v.Y * num2, this.Cofac(0, 0, 2, 2) * num2, this.Cofac(0, 2, 1, 0) * num2, v.Z * num2, this.Cofac(0, 1, 2, 0) * num2, this.Cofac(0, 0, 1, 1) * num2);
        }

        public float Cofac(int r1, int c1, int r2, int c2) => 
            ((this[r1][c1] * this[r2][c2]) - (this[r1][c2] * this[r2][c1]));

        public IndexedBasisMatrix Transpose() => 
            new IndexedBasisMatrix(this._Row0.X, this._Row1.X, this._Row2.X, this._Row0.Y, this._Row1.Y, this._Row2.Y, this._Row0.Z, this._Row1.Z, this._Row2.Z);
    }
}


namespace BulletXNA.LinearMath
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct IndexedVector3
    {
        private static IndexedVector3 _zero;
        private static IndexedVector3 _one;
        private static IndexedVector3 _up;
        public float X;
        public float Y;
        public float Z;
        public IndexedVector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public IndexedVector3(float x)
        {
            this.X = x;
            this.Y = x;
            this.Z = x;
        }

        public IndexedVector3(IndexedVector3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        public static IndexedVector3 operator +(IndexedVector3 value1, IndexedVector3 value2)
        {
            IndexedVector3 vector;
            vector.X = value1.X + value2.X;
            vector.Y = value1.Y + value2.Y;
            vector.Z = value1.Z + value2.Z;
            return vector;
        }

        public static IndexedVector3 operator -(IndexedVector3 value1, IndexedVector3 value2)
        {
            IndexedVector3 vector;
            vector.X = value1.X - value2.X;
            vector.Y = value1.Y - value2.Y;
            vector.Z = value1.Z - value2.Z;
            return vector;
        }

        public static IndexedVector3 operator *(IndexedVector3 value, float scaleFactor)
        {
            IndexedVector3 vector;
            vector.X = value.X * scaleFactor;
            vector.Y = value.Y * scaleFactor;
            vector.Z = value.Z * scaleFactor;
            return vector;
        }

        public static IndexedVector3 operator *(float scaleFactor, IndexedVector3 value)
        {
            IndexedVector3 vector;
            vector.X = value.X * scaleFactor;
            vector.Y = value.Y * scaleFactor;
            vector.Z = value.Z * scaleFactor;
            return vector;
        }

        public static IndexedVector3 operator -(IndexedVector3 value)
        {
            IndexedVector3 vector;
            vector.X = -value.X;
            vector.Y = -value.Y;
            vector.Z = -value.Z;
            return vector;
        }

        public static IndexedVector3 operator *(IndexedVector3 value1, IndexedVector3 value2)
        {
            IndexedVector3 vector;
            vector.X = value1.X * value2.X;
            vector.Y = value1.Y * value2.Y;
            vector.Z = value1.Z * value2.Z;
            return vector;
        }

        public static IndexedVector3 operator /(IndexedVector3 value1, IndexedVector3 value2)
        {
            IndexedVector3 vector;
            vector.X = value1.X / value2.X;
            vector.Y = value1.Y / value2.Y;
            vector.Z = value1.Z / value2.Z;
            return vector;
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.X;

                    case 1:
                        return this.Y;

                    case 2:
                        return this.Z;
                }
                return 0f;
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.X = value;
                        return;

                    case 1:
                        this.Y = value;
                        return;

                    case 2:
                        this.Z = value;
                        return;
                }
            }
        }
        public bool Equals(IndexedVector3 other) => 
            ((this.X == other.X) && ((this.Y == other.Y) && (this.Z == other.Z)));

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is IndexedVector3)
            {
                flag = this.Equals((IndexedVector3) obj);
            }
            return flag;
        }

        public static IndexedVector3 Zero =>
            _zero;
        public float Dot(ref IndexedVector3 v) => 
            (((this.X * v.X) + (this.Y * v.Y)) + (this.Z * v.Z));

        public float Dot(IndexedVector3 v) => 
            (((this.X * v.X) + (this.Y * v.Y)) + (this.Z * v.Z));

        public override int GetHashCode() => 
            ((((this.X.GetHashCode() * 0x18d) ^ this.Y.GetHashCode()) * 0x18d) ^ this.Z.GetHashCode());

        static IndexedVector3()
        {
            _zero = new IndexedVector3();
            _one = new IndexedVector3(1f);
            _up = new IndexedVector3(0f, 1f, 0f);
        }
    }
}


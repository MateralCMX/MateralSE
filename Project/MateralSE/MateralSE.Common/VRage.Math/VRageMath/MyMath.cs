namespace VRageMath
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyMath
    {
        private const float Size = 10000f;
        private static int ANGLE_GRANULARITY = 0;
        private static float[] m_precomputedValues = null;
        private static Vector3[] m_corners = new Vector3[8];
        private static readonly float OneOverRoot3 = ((float) Math.Pow(3.0, -0.5));
        public static Vector3 Vector3One = Vector3.One;

        public static Vector3 Abs(ref Vector3 vector) => 
            new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));

        public static float AngleBetween(Vector3 a, Vector3 b)
        {
            float num2 = Vector3.Dot(a, b) / (a.Length() * b.Length());
            return ((Math.Abs((float) (1f - num2)) >= 0.001f) ? ((float) Math.Acos((double) num2)) : 0f);
        }

        public static Vector3 AngleTo(Vector3 From, Vector3 Location)
        {
            Vector3 zero = Vector3.Zero;
            Vector3 vector2 = Vector3.Normalize(Location - From);
            zero.X = (float) Math.Asin((double) vector2.Y);
            zero.Y = ArcTanAngle(-vector2.Z, -vector2.X);
            return zero;
        }

        public static float ArcTanAngle(float x, float y) => 
            ((x != 0f) ? ((x <= 0f) ? ((x >= 0f) ? 0f : ((y <= 0f) ? (((float) Math.Atan((double) (y / x))) - 3.141593f) : (((float) Math.Atan((double) (y / x))) + 3.141593f))) : ((float) Math.Atan((double) (y / x)))) : ((y != 1f) ? -1.570796f : 1.570796f));

        public static float Clamp(float val, float min, float max) => 
            ((val >= min) ? ((val <= max) ? val : max) : min);

        public static float CosineDistance(ref Vector3 a, ref Vector3 b)
        {
            float num;
            Vector3.Dot(ref a, ref b, out num);
            return (num / (a.Length() * b.Length()));
        }

        public static double CosineDistance(ref Vector3D a, ref Vector3D b)
        {
            double num;
            Vector3D.Dot(ref a, ref b, out num);
            return (num / (a.Length() * b.Length()));
        }

        public static BoundingBox CreateFromInsideRadius(float radius)
        {
            float num = OneOverRoot3 * radius;
            return new BoundingBox(-new Vector3(num), new Vector3(num));
        }

        public static float DistanceSquaredFromLineSegment(Vector3 v, Vector3 w, Vector3 p)
        {
            Vector3 vector = w - v;
            float num = vector.LengthSquared();
            if (num == 0f)
            {
                return Vector3.DistanceSquared(p, v);
            }
            float num2 = Vector3.Dot(p - v, vector);
            return ((num2 > 0f) ? ((num2 < num) ? Vector3.DistanceSquared(p, v + ((num2 / num) * vector)) : Vector3.DistanceSquared(p, w)) : Vector3.DistanceSquared(p, v));
        }

        public static float FastCos(float angle) => 
            FastSin(angle + 1.570796f);

        public static float FastSin(float angle)
        {
            int index = ((int) (angle * 10000f)) % ANGLE_GRANULARITY;
            if (index < 0)
            {
                index += ANGLE_GRANULARITY;
            }
            return m_precomputedValues[index];
        }

        public static float FastTanH(float x) => 
            ((x >= -3f) ? ((x <= 3f) ? ((x * (27f + (x * x))) / (27f + ((9f * x) * x))) : 1f) : -1f);

        public static Vector3 ForwardVectorProjection(Vector3 forwardVector, Vector3 projectedVector) => 
            ((Vector3.Dot(projectedVector, forwardVector) <= 0f) ? Vector3.Zero : forwardVector.Project((projectedVector + forwardVector)));

        public static void InitializeFastSin()
        {
            if (m_precomputedValues == null)
            {
                ANGLE_GRANULARITY = 0xf56e;
                m_precomputedValues = new float[ANGLE_GRANULARITY];
                for (int i = 0; i < ANGLE_GRANULARITY; i++)
                {
                    m_precomputedValues[i] = (float) Math.Sin((double) (((float) i) / 10000f));
                }
            }
        }

        public static Vector3 MaxComponents(ref Vector3 a, ref Vector3 b) => 
            new Vector3(MathHelper.Max(a.X, b.X), MathHelper.Max(a.Y, b.Y), MathHelper.Max(a.Z, b.Z));

        public static int Mod(int x, int m) => 
            (((x % m) + m) % m);

        public static long Mod(long x, int m) => 
            (((x % ((long) m)) + m) % ((long) m));

        public static float NormalizeAngle(float angle, float center = 0f) => 
            (angle - (6.283185f * ((float) Math.Floor((double) (((angle + 3.141593f) - center) / 6.283185f)))));

        public static Vector3 QuaternionToEuler(Quaternion Rotation)
        {
            Vector3 position = Vector3.Transform(Vector3.Up, Rotation);
            Vector3 from = new Vector3();
            Vector3 vector3 = AngleTo(from, Vector3.Transform(Vector3.Forward, Rotation));
            if (vector3.X == 1.570796f)
            {
                vector3.Y = ArcTanAngle(position.Z, position.X);
                vector3.Z = 0f;
            }
            else if (vector3.X == -1.570796f)
            {
                vector3.Y = ArcTanAngle(-position.Y, -position.X);
                vector3.Z = 0f;
            }
            else
            {
                position = Vector3.Transform(Vector3.Transform(position, Matrix.CreateRotationY(-vector3.Y)), Matrix.CreateRotationX(-vector3.X));
                vector3.Z = ArcTanAngle(position.Y, -position.X);
            }
            return vector3;
        }

        public static Vector3 VectorFromColor(byte red, byte green, byte blue) => 
            new Vector3(((float) red) / 255f, ((float) green) / 255f, ((float) blue) / 255f);

        public static Vector4 VectorFromColor(byte red, byte green, byte blue, byte alpha) => 
            new Vector4(((float) red) / 255f, ((float) green) / 255f, ((float) blue) / 255f, ((float) alpha) / 255f);
    }
}


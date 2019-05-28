namespace VRageMath
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Capsule
    {
        public Vector3 P0;
        public Vector3 P1;
        public float Radius;
        public Capsule(Vector3 p0, Vector3 p1, float radius)
        {
            this.P0 = p0;
            this.P1 = p1;
            this.Radius = radius;
        }

        public unsafe bool Intersect(Ray ray, ref Vector3 p1, ref Vector3 p2, ref Vector3 n1, ref Vector3 n2)
        {
            Vector3 v = this.P1 - this.P0;
            Vector3 vector2 = ray.Position - this.P0;
            float num = v.Dot(ray.Direction);
            float num2 = v.Dot(vector2);
            float num3 = ((Vector3*) ref v).Dot(v);
            float num4 = (num3 > 0f) ? (num / num3) : 0f;
            float num5 = (num3 > 0f) ? (num2 / num3) : 0f;
            Vector3 vector3 = ray.Direction - (v * num4);
            Vector3 vector4 = vector2 - (v * num5);
            float num6 = ((Vector3*) ref vector3).Dot(vector3);
            float num7 = 2f * vector3.Dot(vector4);
            Vector3* vectorPtr3 = (Vector3*) ref vector4;
            float num8 = vectorPtr3.Dot(vector4) - (this.Radius * this.Radius);
            if (num6 == 0f)
            {
                BoundingSphere sphere;
                BoundingSphere sphere2;
                float num14;
                float num15;
                float num16;
                float num17;
                sphere.Center = this.P0;
                sphere.Radius = this.Radius;
                sphere2.Center = this.P1;
                sphere2.Radius = this.Radius;
                if (!sphere.IntersectRaySphere(ray, out num14, out num15) || !sphere2.IntersectRaySphere(ray, out num16, out num17))
                {
                    return false;
                }
                if (num14 < num16)
                {
                    p1 = ray.Position + (ray.Direction * num14);
                    n1 = p1 - this.P0;
                    n1.Normalize();
                }
                else
                {
                    p1 = ray.Position + (ray.Direction * num16);
                    n1 = p1 - this.P1;
                    n1.Normalize();
                }
                if (num15 > num17)
                {
                    p2 = ray.Position + (ray.Direction * num15);
                    n2 = p2 - this.P0;
                    n2.Normalize();
                }
                else
                {
                    p2 = ray.Position + (ray.Direction * num17);
                    n2 = p2 - this.P1;
                    n2.Normalize();
                }
                return true;
            }
            float num9 = (num7 * num7) - ((4f * num6) * num8);
            if (num9 < 0f)
            {
                return false;
            }
            float num10 = (-num7 - ((float) Math.Sqrt((double) num9))) / (2f * num6);
            float num11 = (-num7 + ((float) Math.Sqrt((double) num9))) / (2f * num6);
            if (num10 > num11)
            {
                num10 = num11;
                num11 = num10;
            }
            float num12 = (num10 * num4) + num5;
            if (num12 < 0f)
            {
                BoundingSphere sphere3;
                float num18;
                float num19;
                sphere3.Center = this.P0;
                sphere3.Radius = this.Radius;
                if (!sphere3.IntersectRaySphere(ray, out num18, out num19))
                {
                    return false;
                }
                p1 = ray.Position + (ray.Direction * num18);
                n1 = p1 - this.P0;
                n1.Normalize();
            }
            else if (num12 <= 1f)
            {
                p1 = ray.Position + (ray.Direction * num10);
                Vector3 vector5 = this.P0 + (v * num12);
                n1 = p1 - vector5;
                n1.Normalize();
            }
            else
            {
                BoundingSphere sphere4;
                float num20;
                float num21;
                sphere4.Center = this.P1;
                sphere4.Radius = this.Radius;
                if (!sphere4.IntersectRaySphere(ray, out num20, out num21))
                {
                    return false;
                }
                p1 = ray.Position + (ray.Direction * num20);
                n1 = p1 - this.P1;
                n1.Normalize();
            }
            float num13 = (num11 * num4) + num5;
            if (num13 < 0f)
            {
                BoundingSphere sphere5;
                float num22;
                float num23;
                sphere5.Center = this.P0;
                sphere5.Radius = this.Radius;
                if (!sphere5.IntersectRaySphere(ray, out num22, out num23))
                {
                    return false;
                }
                p2 = ray.Position + (ray.Direction * num23);
                n2 = p2 - this.P0;
                n2.Normalize();
            }
            else if (num13 <= 1f)
            {
                p2 = ray.Position + (ray.Direction * num11);
                Vector3 vector6 = this.P0 + (v * num13);
                n2 = p2 - vector6;
                n2.Normalize();
            }
            else
            {
                BoundingSphere sphere6;
                float num24;
                float num25;
                sphere6.Center = this.P1;
                sphere6.Radius = this.Radius;
                if (!sphere6.IntersectRaySphere(ray, out num24, out num25))
                {
                    return false;
                }
                p2 = ray.Position + (ray.Direction * num25);
                n2 = p2 - this.P1;
                n2.Normalize();
            }
            return true;
        }

        public bool Intersect(Line line, ref Vector3 p1, ref Vector3 p2, ref Vector3 n1, ref Vector3 n2)
        {
            Ray ray = new Ray(line.From, line.Direction);
            if (!this.Intersect(ray, ref p1, ref p2, ref n1, ref n2))
            {
                return false;
            }
            Vector3 vector = p1 - line.From;
            Vector3 vector2 = p2 - line.From;
            float num = vector.Normalize();
            vector2.Normalize();
            return ((Vector3.Dot(line.Direction, vector) >= 0.9f) ? ((Vector3.Dot(line.Direction, vector2) >= 0.9f) ? (line.Length >= num) : false) : false);
        }
    }
}


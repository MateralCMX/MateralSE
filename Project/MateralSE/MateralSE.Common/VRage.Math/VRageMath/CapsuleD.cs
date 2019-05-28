namespace VRageMath
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CapsuleD
    {
        public Vector3D P0;
        public Vector3D P1;
        public float Radius;
        public CapsuleD(Vector3D p0, Vector3D p1, float radius)
        {
            this.P0 = p0;
            this.P1 = p1;
            this.Radius = radius;
        }

        public unsafe bool Intersect(RayD ray, ref Vector3D p1, ref Vector3D p2, ref Vector3 n1, ref Vector3 n2)
        {
            Vector3D v = this.P1 - this.P0;
            Vector3D vectord2 = ray.Position - this.P0;
            double num = v.Dot(ray.Direction);
            double num2 = v.Dot(vectord2);
            double num3 = ((Vector3D*) ref v).Dot(v);
            double num4 = (num3 > 0.0) ? (num / num3) : 0.0;
            double num5 = (num3 > 0.0) ? (num2 / num3) : 0.0;
            Vector3D vectord3 = ray.Direction - (v * num4);
            Vector3D vectord4 = vectord2 - (v * num5);
            double num6 = ((Vector3D*) ref vectord3).Dot(vectord3);
            double num7 = 2.0 * vectord3.Dot(vectord4);
            Vector3D* vectordPtr3 = (Vector3D*) ref vectord4;
            double num8 = vectordPtr3.Dot(vectord4) - (this.Radius * this.Radius);
            if (num6 == 0.0)
            {
                BoundingSphereD ed;
                BoundingSphereD ed2;
                double num14;
                double num15;
                double num16;
                double num17;
                ed.Center = this.P0;
                ed.Radius = this.Radius;
                ed2.Center = this.P1;
                ed2.Radius = this.Radius;
                if (!ed.IntersectRaySphere(ray, out num14, out num15) || !ed2.IntersectRaySphere(ray, out num16, out num17))
                {
                    return false;
                }
                if (num14 < num16)
                {
                    p1 = ray.Position + (ray.Direction * num14);
                    n1 = (Vector3) (p1 - this.P0);
                    n1.Normalize();
                }
                else
                {
                    p1 = ray.Position + (ray.Direction * num16);
                    n1 = (Vector3) (p1 - this.P1);
                    n1.Normalize();
                }
                if (num15 > num17)
                {
                    p2 = ray.Position + (ray.Direction * num15);
                    n2 = (Vector3) (p2 - this.P0);
                    n2.Normalize();
                }
                else
                {
                    p2 = ray.Position + (ray.Direction * num17);
                    n2 = (Vector3) (p2 - this.P1);
                    n2.Normalize();
                }
                return true;
            }
            double d = (num7 * num7) - ((4.0 * num6) * num8);
            if (d < 0.0)
            {
                return false;
            }
            double num10 = (-num7 - Math.Sqrt(d)) / (2.0 * num6);
            double num11 = (-num7 + Math.Sqrt(d)) / (2.0 * num6);
            if (num10 > num11)
            {
                num10 = num11;
                num11 = num10;
            }
            double num12 = (num10 * num4) + num5;
            if (num12 < 0.0)
            {
                BoundingSphereD ed3;
                double num18;
                double num19;
                ed3.Center = this.P0;
                ed3.Radius = this.Radius;
                if (!ed3.IntersectRaySphere(ray, out num18, out num19))
                {
                    return false;
                }
                p1 = ray.Position + (ray.Direction * num18);
                n1 = (Vector3) (p1 - this.P0);
                n1.Normalize();
            }
            else if (num12 <= 1.0)
            {
                p1 = ray.Position + (ray.Direction * num10);
                Vector3D vectord5 = this.P0 + (v * num12);
                n1 = (Vector3) (p1 - vectord5);
                n1.Normalize();
            }
            else
            {
                BoundingSphereD ed4;
                double num20;
                double num21;
                ed4.Center = this.P1;
                ed4.Radius = this.Radius;
                if (!ed4.IntersectRaySphere(ray, out num20, out num21))
                {
                    return false;
                }
                p1 = ray.Position + (ray.Direction * num20);
                n1 = (Vector3) (p1 - this.P1);
                n1.Normalize();
            }
            double num13 = (num11 * num4) + num5;
            if (num13 < 0.0)
            {
                BoundingSphereD ed5;
                double num22;
                double num23;
                ed5.Center = this.P0;
                ed5.Radius = this.Radius;
                if (!ed5.IntersectRaySphere(ray, out num22, out num23))
                {
                    return false;
                }
                p2 = ray.Position + (ray.Direction * num23);
                n2 = (Vector3) (p2 - this.P0);
                n2.Normalize();
            }
            else if (num13 <= 1.0)
            {
                p2 = ray.Position + (ray.Direction * num11);
                Vector3D vectord6 = this.P0 + (v * num13);
                n2 = (Vector3) (p2 - vectord6);
                n2.Normalize();
            }
            else
            {
                BoundingSphereD ed6;
                double num24;
                double num25;
                ed6.Center = this.P1;
                ed6.Radius = this.Radius;
                if (!ed6.IntersectRaySphere(ray, out num24, out num25))
                {
                    return false;
                }
                p2 = ray.Position + (ray.Direction * num25);
                n2 = (Vector3) (p2 - this.P1);
                n2.Normalize();
            }
            return true;
        }

        public bool Intersect(LineD line, ref Vector3D p1, ref Vector3D p2, ref Vector3 n1, ref Vector3 n2)
        {
            RayD ray = new RayD(line.From, line.Direction);
            if (!this.Intersect(ray, ref p1, ref p2, ref n1, ref n2))
            {
                return false;
            }
            Vector3D vectord = p1 - line.From;
            Vector3D vectord2 = p2 - line.From;
            double num = vectord.Normalize();
            vectord2.Normalize();
            return ((Vector3D.Dot(line.Direction, vectord) >= 0.9) ? ((Vector3D.Dot(line.Direction, vectord2) >= 0.9) ? (line.Length >= num) : false) : false);
        }
    }
}


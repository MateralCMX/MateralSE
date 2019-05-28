namespace VRageMath
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyOrientedBoundingBoxD : IEquatable<MyOrientedBoundingBox>
    {
        public const int CornerCount = 8;
        private const float RAY_EPSILON = 1E-20f;
        public static readonly int[] StartVertices;
        public static readonly int[] EndVertices;
        public static readonly int[] StartXVertices;
        public static readonly int[] EndXVertices;
        public static readonly int[] StartYVertices;
        public static readonly int[] EndYVertices;
        public static readonly int[] StartZVertices;
        public static readonly int[] EndZVertices;
        public static readonly Vector3[] XNeighbourVectorsBack;
        public static readonly Vector3[] XNeighbourVectorsForw;
        public static readonly Vector3[] YNeighbourVectorsBack;
        public static readonly Vector3[] YNeighbourVectorsForw;
        public static readonly Vector3[] ZNeighbourVectorsBack;
        public static readonly Vector3[] ZNeighbourVectorsForw;
        public Vector3D Center;
        public Vector3D HalfExtent;
        public Quaternion Orientation;
        [ThreadStatic]
        private static Vector3D[] m_cornersTmp;
        public static bool GetNormalBetweenEdges(int axis, int edge0, int edge1, out Vector3 normal)
        {
            Vector3[] xNeighbourVectorsForw = null;
            Vector3[] xNeighbourVectorsBack = null;
            normal = Vector3.Zero;
            switch (axis)
            {
                case 0:
                {
                    int[] startXVertices = StartXVertices;
                    int[] endXVertices = EndXVertices;
                    xNeighbourVectorsForw = XNeighbourVectorsForw;
                    xNeighbourVectorsBack = XNeighbourVectorsBack;
                    break;
                }
                case 1:
                {
                    int[] startYVertices = StartYVertices;
                    int[] endYVertices = EndYVertices;
                    xNeighbourVectorsForw = YNeighbourVectorsForw;
                    xNeighbourVectorsBack = YNeighbourVectorsBack;
                    break;
                }
                case 2:
                {
                    int[] startZVertices = StartZVertices;
                    int[] endZVertices = EndZVertices;
                    xNeighbourVectorsForw = ZNeighbourVectorsForw;
                    xNeighbourVectorsBack = ZNeighbourVectorsBack;
                    break;
                }
                default:
                    return false;
            }
            if (edge0 == -1)
            {
                edge0 = 3;
            }
            if (edge0 == 4)
            {
                edge0 = 0;
            }
            if (edge1 == -1)
            {
                edge1 = 3;
            }
            if (edge1 == 4)
            {
                edge1 = 0;
            }
            if ((edge0 == 3) && (edge1 == 0))
            {
                normal = xNeighbourVectorsForw[3];
                return true;
            }
            if ((edge0 == 0) && (edge1 == 3))
            {
                normal = xNeighbourVectorsBack[3];
                return true;
            }
            if ((edge0 + 1) == edge1)
            {
                normal = xNeighbourVectorsForw[edge0];
                return true;
            }
            if (edge0 != (edge1 + 1))
            {
                return false;
            }
            normal = xNeighbourVectorsBack[edge1];
            return true;
        }

        public unsafe MyOrientedBoundingBoxD(MatrixD matrix)
        {
            this.Center = matrix.Translation;
            Vector3D vectord = new Vector3D(matrix.Right.Length(), matrix.Up.Length(), matrix.Forward.Length());
            this.HalfExtent = vectord / 2.0;
            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
            xdPtr1.Right /= vectord.X;
            MatrixD* xdPtr2 = (MatrixD*) ref matrix;
            xdPtr2.Up /= vectord.Y;
            MatrixD* xdPtr3 = (MatrixD*) ref matrix;
            xdPtr3.Forward /= vectord.Z;
            Quaternion.CreateFromRotationMatrix(ref matrix, out this.Orientation);
        }

        public MyOrientedBoundingBoxD(Vector3D center, Vector3D halfExtents, Quaternion orientation)
        {
            this.Center = center;
            this.HalfExtent = halfExtents;
            this.Orientation = orientation;
        }

        public MyOrientedBoundingBoxD(BoundingBoxD box, MatrixD transform)
        {
            this.Center = (box.Min + box.Max) * 0.5;
            this.HalfExtent = (box.Max - box.Min) * 0.5;
            this.Center = Vector3D.Transform(this.Center, transform);
            this.Orientation = Quaternion.CreateFromRotationMatrix(transform);
        }

        public static MyOrientedBoundingBoxD CreateFromBoundingBox(BoundingBoxD box) => 
            new MyOrientedBoundingBoxD((box.Min + box.Max) * 0.5, (box.Max - box.Min) * 0.5, Quaternion.Identity);

        public MyOrientedBoundingBoxD Transform(Quaternion rotation, Vector3D translation) => 
            new MyOrientedBoundingBoxD(Vector3D.Transform(this.Center, rotation) + translation, this.HalfExtent, this.Orientation * rotation);

        public MyOrientedBoundingBoxD Transform(float scale, Quaternion rotation, Vector3D translation) => 
            new MyOrientedBoundingBoxD(Vector3D.Transform(this.Center * scale, rotation) + translation, this.HalfExtent * scale, this.Orientation * rotation);

        public void Transform(MatrixD matrix)
        {
            this.Center = Vector3D.Transform(this.Center, matrix);
            this.Orientation = Quaternion.CreateFromRotationMatrix(MatrixD.CreateFromQuaternion(this.Orientation) * matrix);
        }

        public bool Equals(MyOrientedBoundingBox other) => 
            ((this.Center == other.Center) && ((this.HalfExtent == other.HalfExtent) && (this.Orientation == other.Orientation)));

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is MyOrientedBoundingBox))
            {
                return false;
            }
            MyOrientedBoundingBox box = (MyOrientedBoundingBox) obj;
            return ((this.Center == box.Center) && ((this.HalfExtent == box.HalfExtent) && (this.Orientation == box.Orientation)));
        }

        public override int GetHashCode() => 
            ((this.Center.GetHashCode() ^ this.HalfExtent.GetHashCode()) ^ this.Orientation.GetHashCode());

        public static bool operator ==(MyOrientedBoundingBoxD a, MyOrientedBoundingBoxD b) => 
            a.Equals(b);

        public static bool operator !=(MyOrientedBoundingBoxD a, MyOrientedBoundingBoxD b) => 
            !a.Equals(b);

        public override string ToString()
        {
            string[] textArray1 = new string[] { "{Center:", this.Center.ToString(), " Extents:", this.HalfExtent.ToString(), " Orientation:", this.Orientation.ToString(), "}" };
            return string.Concat(textArray1);
        }

        public bool Intersects(ref BoundingBox box)
        {
            Vector3D vectord = (box.Max + box.Min) * 0.5f;
            Vector3D hA = (box.Max - box.Min) * 0.5f;
            MatrixD mB = MatrixD.CreateFromQuaternion(this.Orientation);
            mB.Translation = this.Center - vectord;
            return (ContainsRelativeBox(ref hA, ref this.HalfExtent, ref mB) != ContainmentType.Disjoint);
        }

        public bool Intersects(ref BoundingBoxD box)
        {
            Vector3D vectord = (box.Max + box.Min) * 0.5;
            Vector3D hA = (box.Max - box.Min) * 0.5;
            MatrixD mB = MatrixD.CreateFromQuaternion(this.Orientation);
            mB.Translation = this.Center - vectord;
            return (ContainsRelativeBox(ref hA, ref this.HalfExtent, ref mB) != ContainmentType.Disjoint);
        }

        public ContainmentType Contains(ref BoundingBox box)
        {
            BoundingBoxD xd = box;
            return this.Contains(ref xd);
        }

        public unsafe ContainmentType Contains(ref BoundingBoxD box)
        {
            Quaternion quaternion;
            Vector3D hB = (box.Max - box.Min) * 0.5;
            Quaternion.Conjugate(ref this.Orientation, out quaternion);
            MatrixD matrix = MatrixD.CreateFromQuaternion(quaternion);
            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
            xdPtr1.Translation = Vector3D.TransformNormal(((box.Max + box.Min) * 0.5) - this.Center, matrix);
            return ContainsRelativeBox(ref this.HalfExtent, ref hB, ref matrix);
        }

        public static ContainmentType Contains(ref BoundingBox boxA, ref MyOrientedBoundingBox oboxB)
        {
            Vector3 hA = (boxA.Max - boxA.Min) * 0.5f;
            Vector3 vector2 = (boxA.Max + boxA.Min) * 0.5f;
            Matrix mB = Matrix.CreateFromQuaternion(oboxB.Orientation);
            mB.Translation = oboxB.Center - vector2;
            return MyOrientedBoundingBox.ContainsRelativeBox(ref hA, ref oboxB.HalfExtent, ref mB);
        }

        public bool Intersects(ref MyOrientedBoundingBoxD other) => 
            (this.Contains(ref other) != ContainmentType.Disjoint);

        public ContainmentType Contains(ref MyOrientedBoundingBoxD other)
        {
            Quaternion quaternion;
            Quaternion quaternion2;
            Quaternion.Conjugate(ref this.Orientation, out quaternion);
            Quaternion.Multiply(ref quaternion, ref other.Orientation, out quaternion2);
            MatrixD mB = MatrixD.CreateFromQuaternion(quaternion2);
            mB.Translation = Vector3D.Transform(other.Center - this.Center, quaternion);
            return ContainsRelativeBox(ref this.HalfExtent, ref other.HalfExtent, ref mB);
        }

        public ContainmentType Contains(BoundingFrustumD frustum) => 
            this.ConvertToFrustum().Contains(frustum);

        public bool Intersects(BoundingFrustumD frustum) => 
            (this.Contains(frustum) != ContainmentType.Disjoint);

        public static ContainmentType Contains(BoundingFrustumD frustum, ref MyOrientedBoundingBoxD obox) => 
            frustum.Contains(obox.ConvertToFrustum());

        public ContainmentType Contains(ref BoundingSphereD sphere)
        {
            Quaternion rotation = Quaternion.Conjugate(this.Orientation);
            Vector3 vector1 = Vector3.Transform((Vector3) (sphere.Center - this.Center), rotation);
            double num = Math.Abs(vector1.X) - this.HalfExtent.X;
            double num2 = Math.Abs(vector1.Y) - this.HalfExtent.Y;
            double num3 = Math.Abs(vector1.Z) - this.HalfExtent.Z;
            double radius = sphere.Radius;
            if (((num <= -radius) && (num2 <= -radius)) && (num3 <= -radius))
            {
                return ContainmentType.Contains;
            }
            num = Math.Max(num, 0.0);
            num2 = Math.Max(num2, 0.0);
            num3 = Math.Max(num3, 0.0);
            return (((((num * num) + (num2 * num2)) + (num3 * num3)) < (radius * radius)) ? ContainmentType.Intersects : ContainmentType.Disjoint);
        }

        public bool Intersects(ref BoundingSphereD sphere)
        {
            Quaternion rotation = Quaternion.Conjugate(this.Orientation);
            Vector3 vector1 = Vector3.Transform((Vector3) (sphere.Center - this.Center), rotation);
            double num = Math.Max((double) (Math.Abs(vector1.X) - this.HalfExtent.X), (double) 0.0);
            double num2 = Math.Max((double) (Math.Abs(vector1.Y) - this.HalfExtent.Y), (double) 0.0);
            double num3 = Math.Max((double) (Math.Abs(vector1.Z) - this.HalfExtent.Z), (double) 0.0);
            double radius = sphere.Radius;
            return ((((num * num) + (num2 * num2)) + (num3 * num3)) < (radius * radius));
        }

        public static unsafe ContainmentType Contains(ref BoundingSphere sphere, ref MyOrientedBoundingBox box)
        {
            Quaternion rotation = Quaternion.Conjugate(box.Orientation);
            Vector3 vector = Vector3.Transform(sphere.Center - box.Center, rotation);
            Vector3* vectorPtr1 = (Vector3*) ref vector;
            vectorPtr1->X = Math.Abs(vector.X);
            Vector3* vectorPtr2 = (Vector3*) ref vector;
            vectorPtr2->Y = Math.Abs(vector.Y);
            Vector3* vectorPtr3 = (Vector3*) ref vector;
            vectorPtr3->Z = Math.Abs(vector.Z);
            float num = sphere.Radius * sphere.Radius;
            if ((vector + box.HalfExtent).LengthSquared() <= num)
            {
                return ContainmentType.Contains;
            }
            Vector3 vector2 = vector - box.HalfExtent;
            Vector3* vectorPtr4 = (Vector3*) ref vector2;
            vectorPtr4->X = Math.Max(vector2.X, 0f);
            Vector3* vectorPtr5 = (Vector3*) ref vector2;
            vectorPtr5->Y = Math.Max(vector2.Y, 0f);
            Vector3* vectorPtr6 = (Vector3*) ref vector2;
            vectorPtr6->Z = Math.Max(vector2.Z, 0f);
            return ((vector2.LengthSquared() < num) ? ContainmentType.Intersects : ContainmentType.Disjoint);
        }

        public bool Contains(ref Vector3 point)
        {
            Quaternion rotation = Quaternion.Conjugate(this.Orientation);
            Vector3 vector = (Vector3) Vector3D.Transform(point - this.Center, rotation);
            return ((Math.Abs(vector.X) <= this.HalfExtent.X) && ((Math.Abs(vector.Y) <= this.HalfExtent.Y) && (Math.Abs(vector.Z) <= this.HalfExtent.Z)));
        }

        public bool Contains(ref Vector3D point)
        {
            Quaternion rotation = Quaternion.Conjugate(this.Orientation);
            Vector3D vectord = Vector3D.Transform(point - this.Center, rotation);
            return ((Math.Abs(vectord.X) <= this.HalfExtent.X) && ((Math.Abs(vectord.Y) <= this.HalfExtent.Y) && (Math.Abs(vectord.Z) <= this.HalfExtent.Z)));
        }

        public double? Intersects(ref RayD ray)
        {
            MatrixD xd = Matrix.CreateFromQuaternion(this.Orientation);
            Vector3D vectord = this.Center - ray.Position;
            double minValue = double.MinValue;
            double maxValue = double.MaxValue;
            double num3 = Vector3D.Dot(xd.Right, vectord);
            double num4 = Vector3D.Dot(xd.Right, ray.Direction);
            if ((num4 >= -9.9999996826552254E-21) && (num4 <= 9.9999996826552254E-21))
            {
                if (((-num3 - this.HalfExtent.X) > 0.0) || ((-num3 + this.HalfExtent.X) < 0.0))
                {
                    return null;
                }
            }
            else
            {
                double num5 = (num3 - this.HalfExtent.X) / num4;
                double num6 = (num3 + this.HalfExtent.X) / num4;
                if (num5 > num6)
                {
                    num5 = num6;
                    num6 = num5;
                }
                if (num5 > minValue)
                {
                    minValue = num5;
                }
                if (num6 < maxValue)
                {
                    maxValue = num6;
                }
                if ((maxValue < 0.0) || (minValue > maxValue))
                {
                    return null;
                }
            }
            num3 = Vector3D.Dot(xd.Up, vectord);
            num4 = Vector3D.Dot(xd.Up, ray.Direction);
            if ((num4 >= -9.9999996826552254E-21) && (num4 <= 9.9999996826552254E-21))
            {
                if (((-num3 - this.HalfExtent.Y) > 0.0) || ((-num3 + this.HalfExtent.Y) < 0.0))
                {
                    return null;
                }
            }
            else
            {
                double num7 = (num3 - this.HalfExtent.Y) / num4;
                double num8 = (num3 + this.HalfExtent.Y) / num4;
                if (num7 > num8)
                {
                    num7 = num8;
                    num8 = num7;
                }
                if (num7 > minValue)
                {
                    minValue = num7;
                }
                if (num8 < maxValue)
                {
                    maxValue = num8;
                }
                if ((maxValue < 0.0) || (minValue > maxValue))
                {
                    return null;
                }
            }
            num3 = Vector3D.Dot(xd.Forward, vectord);
            num4 = Vector3D.Dot(xd.Forward, ray.Direction);
            if ((num4 >= -9.9999996826552254E-21) && (num4 <= 9.9999996826552254E-21))
            {
                if (((-num3 - this.HalfExtent.Z) > 0.0) || ((-num3 + this.HalfExtent.Z) < 0.0))
                {
                    return null;
                }
            }
            else
            {
                double num9 = (num3 - this.HalfExtent.Z) / num4;
                double num10 = (num3 + this.HalfExtent.Z) / num4;
                if (num9 > num10)
                {
                    num9 = num10;
                    num10 = num9;
                }
                if (num9 > minValue)
                {
                    minValue = num9;
                }
                if (num10 < maxValue)
                {
                    maxValue = num10;
                }
                if ((maxValue < 0.0) || (minValue > maxValue))
                {
                    return null;
                }
            }
            return new double?(minValue);
        }

        public double? Intersects(ref LineD line)
        {
            if (!this.Contains(ref line.From))
            {
                RayD yd2 = new RayD(line.From, line.Direction);
                double? nullable3 = this.Intersects(ref yd2);
                if (nullable3 != null)
                {
                    if (nullable3.Value < 0.0)
                    {
                        return null;
                    }
                    if (nullable3.Value <= line.Length)
                    {
                        return new double?(nullable3.Value);
                    }
                }
                return null;
            }
            RayD ray = new RayD(line.To, -line.Direction);
            double? nullable = this.Intersects(ref ray);
            if (nullable != null)
            {
                double num = line.Length - nullable.Value;
                if (num < 0.0)
                {
                    return null;
                }
                if (num <= line.Length)
                {
                    return new double?(num);
                }
            }
            return null;
        }

        public PlaneIntersectionType Intersects(ref PlaneD plane)
        {
            double num = plane.DotCoordinate(this.Center);
            Vector3D vectord = Vector3D.Transform(plane.Normal, Quaternion.Conjugate(this.Orientation));
            double num2 = (Math.Abs((double) (this.HalfExtent.X * vectord.X)) + Math.Abs((double) (this.HalfExtent.Y * vectord.Y))) + Math.Abs((double) (this.HalfExtent.Z * vectord.Z));
            return ((num <= num2) ? ((num >= -num2) ? PlaneIntersectionType.Intersecting : PlaneIntersectionType.Back) : PlaneIntersectionType.Front);
        }

        public void GetCorners(Vector3D[] corners, int startIndex)
        {
            MatrixD xd = MatrixD.CreateFromQuaternion(this.Orientation);
            Vector3D vectord = xd.Left * this.HalfExtent.X;
            Vector3D vectord2 = xd.Up * this.HalfExtent.Y;
            Vector3D vectord3 = xd.Backward * this.HalfExtent.Z;
            int index = startIndex;
            index++;
            corners[index] = ((this.Center - vectord) + vectord2) + vectord3;
            index++;
            corners[index] = ((this.Center + vectord) + vectord2) + vectord3;
            index++;
            corners[index] = ((this.Center + vectord) - vectord2) + vectord3;
            index++;
            corners[index] = ((this.Center - vectord) - vectord2) + vectord3;
            index++;
            corners[index] = ((this.Center - vectord) + vectord2) - vectord3;
            index++;
            corners[index] = ((this.Center + vectord) + vectord2) - vectord3;
            index++;
            corners[index] = ((this.Center + vectord) - vectord2) - vectord3;
            index++;
            corners[index] = ((this.Center - vectord) - vectord2) - vectord3;
        }

        public static ContainmentType ContainsRelativeBox(ref Vector3D hA, ref Vector3D hB, ref MatrixD mB)
        {
            Vector3D translation = mB.Translation;
            Vector3D vectord2 = new Vector3D(Math.Abs(translation.X), Math.Abs(translation.Y), Math.Abs(translation.Z));
            Vector3D right = mB.Right;
            Vector3D up = mB.Up;
            Vector3D backward = mB.Backward;
            Vector3D vectord6 = right * hB.X;
            Vector3D vectord7 = up * hB.Y;
            Vector3D vectord8 = backward * hB.Z;
            double num = (Math.Abs(vectord6.X) + Math.Abs(vectord7.X)) + Math.Abs(vectord8.X);
            double num2 = (Math.Abs(vectord6.Y) + Math.Abs(vectord7.Y)) + Math.Abs(vectord8.Y);
            double num3 = (Math.Abs(vectord6.Z) + Math.Abs(vectord7.Z)) + Math.Abs(vectord8.Z);
            if ((((vectord2.X + num) <= hA.X) && ((vectord2.Y + num2) <= hA.Y)) && ((vectord2.Z + num3) <= hA.Z))
            {
                return ContainmentType.Contains;
            }
            if (vectord2.X > (((hA.X + Math.Abs(vectord6.X)) + Math.Abs(vectord7.X)) + Math.Abs(vectord8.X)))
            {
                return ContainmentType.Disjoint;
            }
            if (vectord2.Y > (((hA.Y + Math.Abs(vectord6.Y)) + Math.Abs(vectord7.Y)) + Math.Abs(vectord8.Y)))
            {
                return ContainmentType.Disjoint;
            }
            if (vectord2.Z > (((hA.Z + Math.Abs(vectord6.Z)) + Math.Abs(vectord7.Z)) + Math.Abs(vectord8.Z)))
            {
                return ContainmentType.Disjoint;
            }
            if (Math.Abs(Vector3D.Dot(translation, right)) > (((Math.Abs((double) (hA.X * right.X)) + Math.Abs((double) (hA.Y * right.Y))) + Math.Abs((double) (hA.Z * right.Z))) + hB.X))
            {
                return ContainmentType.Disjoint;
            }
            if (Math.Abs(Vector3D.Dot(translation, up)) > (((Math.Abs((double) (hA.X * up.X)) + Math.Abs((double) (hA.Y * up.Y))) + Math.Abs((double) (hA.Z * up.Z))) + hB.Y))
            {
                return ContainmentType.Disjoint;
            }
            if (Math.Abs(Vector3D.Dot(translation, backward)) > (((Math.Abs((double) (hA.X * backward.X)) + Math.Abs((double) (hA.Y * backward.Y))) + Math.Abs((double) (hA.Z * backward.Z))) + hB.Z))
            {
                return ContainmentType.Disjoint;
            }
            Vector3D vectord9 = new Vector3D(0.0, -right.Z, right.Y);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Y * vectord9.Y)) + Math.Abs((double) (hA.Z * vectord9.Z))) + Math.Abs(Vector3D.Dot(vectord9, vectord7))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(0.0, -up.Z, up.Y);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Y * vectord9.Y)) + Math.Abs((double) (hA.Z * vectord9.Z))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(0.0, -backward.Z, backward.Y);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Y * vectord9.Y)) + Math.Abs((double) (hA.Z * vectord9.Z))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))) + Math.Abs(Vector3D.Dot(vectord9, vectord7))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(right.Z, 0.0, -right.X);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Z * vectord9.Z)) + Math.Abs((double) (hA.X * vectord9.X))) + Math.Abs(Vector3D.Dot(vectord9, vectord7))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(up.Z, 0.0, -up.X);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Z * vectord9.Z)) + Math.Abs((double) (hA.X * vectord9.X))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(backward.Z, 0.0, -backward.X);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.Z * vectord9.Z)) + Math.Abs((double) (hA.X * vectord9.X))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))) + Math.Abs(Vector3D.Dot(vectord9, vectord7))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(-right.Y, right.X, 0.0);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.X * vectord9.X)) + Math.Abs((double) (hA.Y * vectord9.Y))) + Math.Abs(Vector3D.Dot(vectord9, vectord7))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(-up.Y, up.X, 0.0);
            if (Math.Abs(Vector3D.Dot(translation, vectord9)) > (((Math.Abs((double) (hA.X * vectord9.X)) + Math.Abs((double) (hA.Y * vectord9.Y))) + Math.Abs(Vector3D.Dot(vectord9, vectord8))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))))
            {
                return ContainmentType.Disjoint;
            }
            vectord9 = new Vector3D(-backward.Y, backward.X, 0.0);
            return ((Math.Abs(Vector3D.Dot(translation, vectord9)) <= (((Math.Abs((double) (hA.X * vectord9.X)) + Math.Abs((double) (hA.Y * vectord9.Y))) + Math.Abs(Vector3D.Dot(vectord9, vectord6))) + Math.Abs(Vector3D.Dot(vectord9, vectord7)))) ? ContainmentType.Intersects : ContainmentType.Disjoint);
        }

        public unsafe BoundingFrustumD ConvertToFrustum()
        {
            Quaternion quaternion;
            MatrixD xd;
            Quaternion.Conjugate(ref this.Orientation, out quaternion);
            double num = 1.0 / this.HalfExtent.X;
            double num2 = 1.0 / this.HalfExtent.Y;
            double num3 = 0.5 / this.HalfExtent.Z;
            MatrixD.CreateFromQuaternion(ref quaternion, out xd);
            double* numPtr1 = (double*) ref xd.M11;
            numPtr1[0] *= num;
            double* numPtr2 = (double*) ref xd.M21;
            numPtr2[0] *= num;
            double* numPtr3 = (double*) ref xd.M31;
            numPtr3[0] *= num;
            double* numPtr4 = (double*) ref xd.M12;
            numPtr4[0] *= num2;
            double* numPtr5 = (double*) ref xd.M22;
            numPtr5[0] *= num2;
            double* numPtr6 = (double*) ref xd.M32;
            numPtr6[0] *= num2;
            double* numPtr7 = (double*) ref xd.M13;
            numPtr7[0] *= num3;
            double* numPtr8 = (double*) ref xd.M23;
            numPtr8[0] *= num3;
            double* numPtr9 = (double*) ref xd.M33;
            numPtr9[0] *= num3;
            MatrixD* xdPtr1 = (MatrixD*) ref xd;
            xdPtr1.Translation = (Vector3.UnitZ * 0.5f) + Vector3D.TransformNormal(-this.Center, xd);
            return new BoundingFrustumD(xd);
        }

        public BoundingBoxD GetAABB()
        {
            if (m_cornersTmp == null)
            {
                m_cornersTmp = new Vector3D[8];
            }
            this.GetCorners(m_cornersTmp, 0);
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            for (int i = 0; i < 8; i++)
            {
                xd.Include(m_cornersTmp[i]);
            }
            return xd;
        }

        public static MyOrientedBoundingBoxD Create(BoundingBoxD boundingBox, MatrixD matrix)
        {
            MyOrientedBoundingBoxD xd = new MyOrientedBoundingBoxD(boundingBox.Center, boundingBox.HalfExtents, Quaternion.Identity);
            xd.Transform(matrix);
            return xd;
        }

        static MyOrientedBoundingBoxD()
        {
            StartVertices = new int[] { 0, 1, 5, 4, 3, 2, 6, 7, 0, 1, 5, 4 };
            EndVertices = new int[] { 1, 5, 4, 0, 2, 6, 7, 3, 3, 2, 6, 7 };
            StartXVertices = new int[] { 0, 4, 7, 3 };
            EndXVertices = new int[] { 1, 5, 6, 2 };
            StartYVertices = new int[] { 0, 1, 5, 4 };
            EndYVertices = new int[] { 3, 2, 6, 7 };
            StartZVertices = new int[] { 0, 3, 2, 1 };
            EndZVertices = new int[] { 4, 7, 6, 5 };
            XNeighbourVectorsBack = new Vector3[] { new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, -1f), new Vector3(0f, -1f, 0f) };
            XNeighbourVectorsForw = new Vector3[] { new Vector3(0f, 0f, -1f), new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f) };
            YNeighbourVectorsBack = new Vector3[] { new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, -1f) };
            YNeighbourVectorsForw = new Vector3[] { new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, -1f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f) };
            ZNeighbourVectorsBack = new Vector3[] { new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, -1f, 0f), new Vector3(-1f, 0f, 0f) };
            ZNeighbourVectorsForw = new Vector3[] { new Vector3(0f, -1f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f) };
        }
    }
}


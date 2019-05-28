namespace Sandbox.Engine.Utils
{
    using BulletXNA;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;

    public class MyGridIntersection
    {
        public static unsafe void Calculate(List<Vector3I> result, float gridSize, Vector3D lineStart, Vector3D lineEnd, Vector3I min, Vector3I max)
        {
            Vector3D vectord = lineEnd - lineStart;
            Vector3D v = lineStart / ((double) gridSize);
            if (MyUtils.IsZero(vectord, 1E-05f))
            {
                if (IsPointInside((Vector3) v, min, max))
                {
                    result.Add(GetGridPoint(ref v, min, max));
                }
            }
            else
            {
                Vector3D end = lineEnd / ((double) gridSize);
                if (ClipLine(ref v, ref end, min, max))
                {
                    Vector3 vector = Sign((Vector3) vectord);
                    Vector3I vectori = SignInt((Vector3) vectord);
                    Vector3I vectori2 = GetGridPoint(ref v, min, max) * vectori;
                    Vector3I vectori3 = GetGridPoint(ref end, min, max) * vectori;
                    vectord *= vector;
                    v *= vector;
                    double num = 1.0 / vectord.X;
                    double num2 = num * (Math.Floor((double) (v.X + 1.0)) - v.X);
                    double num3 = 1.0 / vectord.Y;
                    double num4 = num3 * (Math.Floor((double) (v.Y + 1.0)) - v.Y);
                    double num5 = 1.0 / vectord.Z;
                    double num6 = num5 * (Math.Floor((double) (v.Z + 1.0)) - v.Z);
                    while (true)
                    {
                        int num7;
                        result.Add(vectori2 * vectori);
                        if (num2 < num6)
                        {
                            if (num2 < num4)
                            {
                                num2 += num;
                                int* numPtr1 = (int*) ref vectori2.X;
                                num7 = numPtr1[0] + 1;
                                numPtr1[0] = num7;
                                if (num7 > vectori3.X)
                                {
                                    return;
                                }
                                continue;
                            }
                            num4 += num3;
                            int* numPtr2 = (int*) ref vectori2.Y;
                            num7 = numPtr2[0] + 1;
                            numPtr2[0] = num7;
                            if (num7 > vectori3.Y)
                            {
                                return;
                            }
                            continue;
                        }
                        if (num6 < num4)
                        {
                            num6 += num5;
                            int* numPtr3 = (int*) ref vectori2.Z;
                            num7 = numPtr3[0] + 1;
                            numPtr3[0] = num7;
                            if (num7 > vectori3.Z)
                            {
                                return;
                            }
                            continue;
                        }
                        num4 += num3;
                        int* numPtr4 = (int*) ref vectori2.Y;
                        num7 = numPtr4[0] + 1;
                        numPtr4[0] = num7;
                        if (num7 > vectori3.Y)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public static void CalculateHavok(List<Vector3I> result, float gridSize, Vector3D lineStart, Vector3D lineEnd, Vector3I min, Vector3I max)
        {
            Vector3D v = Vector3D.Normalize(lineEnd - lineStart);
            Vector3D vectord = Vector3D.Normalize(Vector3D.CalculatePerpendicularVector(v)) * 0.059999998658895493;
            Vector3D vectord2 = Vector3D.Normalize(Vector3D.Cross(v, vectord)) * 0.06;
            Calculate(result, gridSize, lineStart + vectord, lineEnd + vectord, min, max);
            Calculate(result, gridSize, lineStart - vectord, lineEnd - vectord, min, max);
            Calculate(result, gridSize, lineStart + vectord2, lineEnd + vectord2, min, max);
            Calculate(result, gridSize, lineStart - vectord2, lineEnd - vectord2, min, max);
        }

        private static bool ClipLine(ref Vector3D start, ref Vector3D end, Vector3I min, Vector3I max)
        {
            Vector3D vectord = end - start;
            if (MyUtils.IsZero(vectord, 1E-05f))
            {
                return IsPointInside((Vector3) start, min, max);
            }
            double tE = 0.0;
            double tL = 1.0;
            if ((!IntersectionT(min.X - start.X, vectord.X, ref tE, ref tL) || (!IntersectionT((start.X - max.X) - 1.0, -vectord.X, ref tE, ref tL) || (!IntersectionT(min.Y - start.Y, vectord.Y, ref tE, ref tL) || (!IntersectionT((start.Y - max.Y) - 1.0, -vectord.Y, ref tE, ref tL) || !IntersectionT(min.Z - start.Z, vectord.Z, ref tE, ref tL))))) || !IntersectionT((start.Z - max.Z) - 1.0, -vectord.Z, ref tE, ref tL))
            {
                return false;
            }
            if (tL < 1.0)
            {
                end = start + (tL * vectord);
            }
            if (tE > 0.0)
            {
                start += tE * vectord;
            }
            return true;
        }

        private static Vector3I GetGridPoint(ref Vector3D v, Vector3I min, Vector3I max)
        {
            Vector3I vectori = new Vector3I();
            if (v.X < min.X)
            {
                v.X = vectori.X = min.X;
            }
            else if (v.X < (max.X + 1))
            {
                vectori.X = (int) Math.Floor(v.X);
            }
            else
            {
                v.X = MathUtil.NextAfter((float) (max.X + 1), float.NegativeInfinity);
                vectori.X = max.X;
            }
            if (v.Y < min.Y)
            {
                v.Y = vectori.Y = min.Y;
            }
            else if (v.Y < (max.Y + 1))
            {
                vectori.Y = (int) Math.Floor(v.Y);
            }
            else
            {
                v.Y = MathUtil.NextAfter((float) (max.Y + 1), float.NegativeInfinity);
                vectori.Y = max.Y;
            }
            if (v.Z < min.Z)
            {
                v.Z = vectori.Z = min.Z;
            }
            else if (v.Z < (max.Z + 1))
            {
                vectori.Z = (int) Math.Floor(v.Z);
            }
            else
            {
                v.Z = MathUtil.NextAfter((float) (max.Z + 1), float.NegativeInfinity);
                vectori.Z = max.Z;
            }
            return vectori;
        }

        private static bool IntersectionT(double n, double d, ref double tE, ref double tL)
        {
            if (MyUtils.IsZero(d, 1E-05f))
            {
                return !(n != 0.0);
            }
            double num = n / d;
            if (d > 0.0)
            {
                if (num > tL)
                {
                    return false;
                }
                if (num > tE)
                {
                    tE = num;
                }
            }
            else
            {
                if (num < tE)
                {
                    return false;
                }
                if (num < tL)
                {
                    tL = num;
                }
            }
            return true;
        }

        private static bool IsPointInside(Vector3 p, Vector3I min, Vector3I max) => 
            ((p.X >= min.X) && ((p.X < (max.X + 1)) && ((p.Y >= min.Y) && ((p.Y < (max.Y + 1)) && ((p.Z >= min.Z) && (p.Z < (max.Z + 1)))))));

        private static Vector3 Sign(Vector3 v) => 
            new Vector3((v.X >= 0f) ? ((float) 1) : ((float) (-1)), (v.Y >= 0f) ? ((float) 1) : ((float) (-1)), (v.Z >= 0f) ? ((float) 1) : ((float) (-1)));

        private static Vector3I SignInt(Vector3 v) => 
            new Vector3I((v.X >= 0f) ? 1 : -1, (v.Y >= 0f) ? 1 : -1, (v.Z >= 0f) ? 1 : -1);
    }
}


namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public static class MyCubemapHelpers
    {
        public const int NUM_MAPS = 6;
        public static readonly TexcoordCalculator[] TexcoordCalculators = new TexcoordCalculator[] { new TexcoordCalculator(MyCubemapHelpers.CalcFrontTexcoord), new TexcoordCalculator(MyCubemapHelpers.CalcBackTexcoord), new TexcoordCalculator(MyCubemapHelpers.CalcLeftTexcoord), new TexcoordCalculator(MyCubemapHelpers.CalcRightTexcoord), new TexcoordCalculator(MyCubemapHelpers.CalcUpTexcoord), new TexcoordCalculator(MyCubemapHelpers.CalcDownTexcoord) };
        public const float UshortRecip = 1.525902E-05f;
        public const float Ushort2Recip = 3.051804E-05f;
        public const float ByteRecip = 0.003921569f;

        public static void CalcBackTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Y / vector.Z;
            texCoord.X = localPos.X / vector.Z;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void CalcDownTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Z / vector.Y;
            texCoord.X = localPos.X / vector.Y;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void CalcFrontTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Y / vector.Z;
            texCoord.X = -localPos.X / vector.Z;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void CalcLeftTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Y / vector.X;
            texCoord.X = -localPos.Z / vector.X;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void CalcRightTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Y / vector.X;
            texCoord.X = localPos.Z / vector.X;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void CalculateSamplePosition(ref Vector3 localPos, out Vector3I samplePosition, ref Vector2 texCoord, int resolution)
        {
            Vector3 vector = Vector3.Abs(localPos);
            if (vector.X > vector.Y)
            {
                if (vector.X > vector.Z)
                {
                    localPos /= vector.X;
                    texCoord.Y = -localPos.Y;
                    if (localPos.X > 0f)
                    {
                        texCoord.X = -localPos.Z;
                        samplePosition.X = 2;
                    }
                    else
                    {
                        texCoord.X = localPos.Z;
                        samplePosition.X = 3;
                    }
                }
                else
                {
                    localPos /= vector.Z;
                    texCoord.Y = -localPos.Y;
                    if (localPos.Z > 0f)
                    {
                        texCoord.X = localPos.X;
                        samplePosition.X = 1;
                    }
                    else
                    {
                        texCoord.X = -localPos.X;
                        samplePosition.X = 0;
                    }
                }
            }
            else if (vector.Y > vector.Z)
            {
                localPos /= vector.Y;
                texCoord.Y = -localPos.Z;
                if (localPos.Y > 0f)
                {
                    texCoord.X = -localPos.X;
                    samplePosition.X = 4;
                }
                else
                {
                    texCoord.X = localPos.X;
                    samplePosition.X = 5;
                }
            }
            else
            {
                localPos /= vector.Z;
                texCoord.Y = -localPos.Y;
                if (localPos.Z > 0f)
                {
                    texCoord.X = localPos.X;
                    samplePosition.X = 1;
                }
                else
                {
                    texCoord.X = -localPos.X;
                    samplePosition.X = 0;
                }
            }
            texCoord = ((texCoord + 1f) * 0.5f) * resolution;
            samplePosition.Y = (int) Math.Round((double) texCoord.X);
            samplePosition.Z = (int) Math.Round((double) texCoord.Y);
        }

        public static void CalculateSampleTexcoord(ref Vector3 localPos, out int face, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            if (vector.X > vector.Y)
            {
                if (vector.X > vector.Z)
                {
                    float num = 1f / vector.X;
                    texCoord.Y = -localPos.Y * num;
                    if (localPos.X > 0f)
                    {
                        texCoord.X = -localPos.Z * num;
                        face = 2;
                    }
                    else
                    {
                        texCoord.X = localPos.Z * num;
                        face = 3;
                    }
                }
                else
                {
                    float num2 = 1f / vector.Z;
                    texCoord.Y = -localPos.Y * num2;
                    if (localPos.Z > 0f)
                    {
                        texCoord.X = localPos.X * num2;
                        face = 1;
                    }
                    else
                    {
                        texCoord.X = -localPos.X * num2;
                        face = 0;
                    }
                }
            }
            else if (vector.Y > vector.Z)
            {
                float num3 = 1f / vector.Y;
                texCoord.Y = -localPos.Z * num3;
                if (localPos.Y > 0f)
                {
                    texCoord.X = -localPos.X * num3;
                    face = 4;
                }
                else
                {
                    texCoord.X = localPos.X * num3;
                    face = 5;
                }
            }
            else
            {
                float num4 = 1f / vector.Z;
                texCoord.Y = -localPos.Y * num4;
                if (localPos.Z > 0f)
                {
                    texCoord.X = localPos.X * num4;
                    face = 1;
                }
                else
                {
                    texCoord.X = -localPos.X * num4;
                    face = 0;
                }
            }
            texCoord = (texCoord + 1f) * 0.5f;
            if (texCoord.X == 1f)
            {
                texCoord.X = 0.999999f;
            }
            if (texCoord.Y == 1f)
            {
                texCoord.Y = 0.999999f;
            }
        }

        public static void CalculateTexcoordForFace(ref Vector3 localPos, int face, out Vector2 texCoord)
        {
            float num;
            switch (face)
            {
                case 0:
                    num = 1f / Math.Abs(localPos.Z);
                    texCoord.X = -localPos.X * num;
                    texCoord.Y = -localPos.Y * num;
                    break;

                case 1:
                    num = 1f / Math.Abs(localPos.Z);
                    texCoord.X = localPos.X * num;
                    texCoord.Y = -localPos.Y * num;
                    break;

                case 2:
                    num = 1f / Math.Abs(localPos.X);
                    texCoord.X = -localPos.Z * num;
                    texCoord.Y = -localPos.Y * num;
                    break;

                case 3:
                    num = 1f / Math.Abs(localPos.X);
                    texCoord.X = localPos.Z * num;
                    texCoord.Y = -localPos.Y * num;
                    break;

                case 4:
                    num = 1f / Math.Abs(localPos.Y);
                    texCoord.X = -localPos.X * num;
                    texCoord.Y = -localPos.Z * num;
                    break;

                case 5:
                    num = 1f / Math.Abs(localPos.Y);
                    texCoord.X = localPos.X * num;
                    texCoord.Y = -localPos.Z * num;
                    break;

                default:
                    texCoord = Vector2.Zero;
                    break;
            }
            texCoord = (texCoord + 1f) * 0.5f;
            if (texCoord.X == 1f)
            {
                texCoord.X = 0.999999f;
            }
            if (texCoord.Y == 1f)
            {
                texCoord.Y = 0.999999f;
            }
        }

        public static void CalcUpTexcoord(ref Vector3 localPos, out Vector2 texCoord)
        {
            Vector3 vector = Vector3.Abs(localPos);
            texCoord.Y = -localPos.Z / vector.Y;
            texCoord.X = -localPos.X / vector.Y;
            texCoord = (texCoord + 1f) * 0.5f;
        }

        public static void GetCubeFace(ref Vector3 position, out int face)
        {
            Vector3 vector = Vector3.Abs(position);
            if (vector.X > vector.Y)
            {
                if (vector.X > vector.Z)
                {
                    if (position.X > 0f)
                    {
                        face = 2;
                    }
                    else
                    {
                        face = 3;
                    }
                }
                else if (position.Z > 0f)
                {
                    face = 1;
                }
                else
                {
                    face = 0;
                }
            }
            else if (vector.Y > vector.Z)
            {
                if (position.Y > 0f)
                {
                    face = 4;
                }
                else
                {
                    face = 5;
                }
            }
            else if (position.Z > 0f)
            {
                face = 1;
            }
            else
            {
                face = 0;
            }
        }

        public static void GetCubeFaceDirection(ref Vector3 position, out Vector3B face)
        {
            Vector3 vector = Vector3.Abs(position);
            if (vector.X > vector.Y)
            {
                if (vector.X > vector.Z)
                {
                    if (position.X > 0f)
                    {
                        face = Vector3B.Right;
                    }
                    else
                    {
                        face = Vector3B.Left;
                    }
                }
                else if (position.Z > 0f)
                {
                    face = Vector3B.Backward;
                }
                else
                {
                    face = Vector3B.Forward;
                }
            }
            else if (vector.Y > vector.Z)
            {
                if (position.Y > 0f)
                {
                    face = Vector3B.Up;
                }
                else
                {
                    face = Vector3B.Down;
                }
            }
            else if (position.Z > 0f)
            {
                face = Vector3B.Backward;
            }
            else
            {
                face = Vector3B.Forward;
            }
        }

        public static string GetNameForFace(int i)
        {
            switch (i)
            {
                case 0:
                    return "front";

                case 1:
                    return "back";

                case 2:
                    return "left";

                case 3:
                    return "right";

                case 4:
                    return "up";

                case 5:
                    return "down";
            }
            return "";
        }

        public static Vector2I GetStep(ref Vector2I start, ref Vector2I end) => 
            ((start.X <= end.X) ? ((start.X >= end.X) ? ((start.Y <= end.Y) ? ((start.Y >= end.Y) ? Vector2I.Zero : Vector2I.UnitY) : -Vector2I.UnitY) : Vector2I.UnitX) : -Vector2I.UnitX);

        internal enum Faces : byte
        {
            Left = 2,
            Right = 3,
            Up = 4,
            Down = 5,
            Back = 1,
            Front = 0
        }

        public delegate void TexcoordCalculator(ref Vector3 local, out Vector2 texcoord);
    }
}


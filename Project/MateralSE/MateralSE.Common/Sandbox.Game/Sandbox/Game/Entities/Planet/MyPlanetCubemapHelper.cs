namespace Sandbox.Game.Entities.Planet
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public static class MyPlanetCubemapHelper
    {
        public static uint[] AdjacentFaceTransforms = new uint[] { 
            0, 0, 0, 0x10, 10, 0x1a, 0, 0, 0x10, 0, 6, 0x16, 0x10, 0, 0, 0,
            3, 0x1f, 0, 0x10, 0, 0, 15, 0x13, 0x19, 5, 0x13, 15, 0, 0, 9, 0x15,
            0x1f, 3, 0, 0
        };

        public static int FindCubeFace(ref Vector3D localPos)
        {
            Vector3D vectord;
            Vector3D.Abs(ref localPos, out vectord);
            return ((vectord.X <= vectord.Y) ? ((vectord.Y <= vectord.Z) ? ((localPos.Z <= 0.0) ? 0 : 1) : ((localPos.Y <= 0.0) ? 5 : 4)) : ((vectord.X <= vectord.Z) ? ((localPos.Z <= 0.0) ? 0 : 1) : ((localPos.X <= 0.0) ? 2 : 3)));
        }

        public static void GetForwardUp(Base6Directions.Direction axis, out Vector3D forward, out Vector3D up)
        {
            forward = Base6Directions.Directions[(int) axis];
            up = Base6Directions.Directions[(int) Base6Directions.GetPerpendicular(axis)];
        }

        public static void ProjectForFace(ref Vector3D localPos, int face, out Vector2D normalCoord)
        {
            Vector3D vectord;
            Vector3D.Abs(ref localPos, out vectord);
            switch (((byte) face))
            {
                case 0:
                    localPos /= vectord.Z;
                    normalCoord.X = -localPos.X;
                    normalCoord.Y = localPos.Y;
                    return;

                case 1:
                    localPos /= vectord.Z;
                    normalCoord.X = localPos.X;
                    normalCoord.Y = localPos.Y;
                    return;

                case 2:
                    localPos /= vectord.X;
                    normalCoord.X = localPos.Z;
                    normalCoord.Y = localPos.Y;
                    return;

                case 3:
                    localPos /= vectord.X;
                    normalCoord.X = -localPos.Z;
                    normalCoord.Y = localPos.Y;
                    return;

                case 4:
                    localPos /= vectord.Y;
                    normalCoord.X = localPos.Z;
                    normalCoord.Y = localPos.X;
                    return;

                case 5:
                    localPos /= vectord.Y;
                    normalCoord.X = -localPos.Z;
                    normalCoord.Y = localPos.X;
                    return;
            }
            normalCoord = Vector2D.Zero;
        }

        public static void ProjectToCube(ref Vector3D localPos, out int direction, out Vector2D texcoords)
        {
            Vector3D vectord;
            Vector3D.Abs(ref localPos, out vectord);
            if (vectord.X > vectord.Y)
            {
                if (vectord.X > vectord.Z)
                {
                    localPos /= vectord.X;
                    texcoords.Y = localPos.Y;
                    if (localPos.X > 0.0)
                    {
                        texcoords.X = -localPos.Z;
                        direction = 3;
                    }
                    else
                    {
                        texcoords.X = localPos.Z;
                        direction = 2;
                    }
                }
                else
                {
                    localPos /= vectord.Z;
                    texcoords.Y = localPos.Y;
                    if (localPos.Z > 0.0)
                    {
                        texcoords.X = localPos.X;
                        direction = 1;
                    }
                    else
                    {
                        texcoords.X = -localPos.X;
                        direction = 0;
                    }
                }
            }
            else if (vectord.Y > vectord.Z)
            {
                localPos /= vectord.Y;
                texcoords.Y = localPos.X;
                if (localPos.Y > 0.0)
                {
                    texcoords.X = localPos.Z;
                    direction = 4;
                }
                else
                {
                    texcoords.X = -localPos.Z;
                    direction = 5;
                }
            }
            else
            {
                localPos /= vectord.Z;
                texcoords.Y = localPos.Y;
                if (localPos.Z > 0.0)
                {
                    texcoords.X = localPos.X;
                    direction = 1;
                }
                else
                {
                    texcoords.X = -localPos.X;
                    direction = 0;
                }
            }
        }

        public static unsafe void TranslateTexcoordsToFace(ref Vector2D texcoords, int originalFace, int myFace, out Vector2D newCoords)
        {
            Vector2D vectord = texcoords;
            if ((originalFace & -2) != (myFace & -2))
            {
                uint num = AdjacentFaceTransforms[(myFace * 6) + originalFace];
                double* numPtr = (double*) &vectord;
                if ((num & 1) != ((num >> 1) & 1))
                {
                    double num3 = numPtr[0];
                    numPtr[0] = numPtr[8];
                    numPtr[8] = num3;
                }
                uint num2 = (num >> 1) & 1;
                if (((num >> 2) & 1) != 0)
                {
                    numPtr[(int) (num2 * 8L)] = -numPtr[(int) (num2 * 8L)];
                }
                if (((num >> 3) & 1) != 0)
                {
                    numPtr[(int) ((1 ^ num2) * 8L)] = -numPtr[(int) ((1 ^ num2) * 8L)];
                }
                if (((num >> 4) & 1) != 0)
                {
                    double* numPtr1 = numPtr + (num2 * 8L);
                    numPtr1[0] -= 2.0;
                }
                else
                {
                    double* numPtr2 = numPtr + (num2 * 8L);
                    numPtr2[0] += 2.0;
                }
            }
            newCoords = vectord;
        }
    }
}


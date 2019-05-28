namespace Sandbox.Engine.Voxels
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels.Planet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyPlanetShapeProvider
    {
        private const int MAX_UNCULLED_HISTORY = 10;
        private static Matrix CR;
        private static Matrix CRT;
        private static Matrix BInv;
        private static Matrix BInvT;
        private static float Tau;
        public static MyDebugHitCounter PruningStats = new MyDebugHitCounter(0x186a0);
        public static MyDebugHitCounter CacheStats = new MyDebugHitCounter(0x186a0);
        public static MyDebugHitCounter CullStats = new MyDebugHitCounter(0x186a0);
        private readonly int m_mapResolutionMinusOne;
        private readonly float m_radius;
        private string m_dataFileName;
        private MyHeightCubemap m_heightmap;
        private readonly VrPlanetShape m_nativeShape;
        private SurfaceDetailSampler m_detail;
        private float m_detailSlopeRecip;
        private float m_detailFade;
        private readonly Vector3 m_translation;
        private readonly float m_maxHillHeight;
        private readonly float m_minHillHeight;
        private readonly float m_heightRatio;
        private readonly float m_heightRatioRecip;
        private float m_detailScale;
        private readonly float m_pixelSize;
        private float m_pixelSizeRecip;
        private readonly float m_pixelSizeRecip2;
        private double m_curvatureThresholdRecip;
        private float m_pixelSize4;
        private float m_voxelSize;
        private readonly float m_mapStepScale;
        private readonly float m_mapStepScaleSquare;
        private float m_mapHeightScale;
        [ThreadStatic]
        private static Cache m_cache;
        private const bool EnableBezierCull = true;
        [ThreadStatic]
        private static Matrix s_Cz;
        private const bool ForceBilinear = false;

        static MyPlanetShapeProvider()
        {
            SetTau(0.5f);
            BInvT = new Matrix(1f, 1f, 1f, 1f, 0f, 0.3333333f, 0.6666667f, 1f, 0f, 0f, 0.3333333f, 1f, 0f, 0f, 0f, 1f);
            BInv = Matrix.Transpose(BInvT);
        }

        public MyPlanetShapeProvider(Vector3 translation, float radius, MyPlanetGeneratorDefinition definition, MyHeightCubemap cubemap, MyPlanetTextureMapProvider texProvider)
        {
            this.m_radius = radius;
            this.m_translation = translation;
            this.m_maxHillHeight = definition.HillParams.Max * this.m_radius;
            this.m_minHillHeight = definition.HillParams.Min * this.m_radius;
            this.InnerRadius = radius + this.m_minHillHeight;
            this.OuterRadius = radius + this.m_maxHillHeight;
            this.m_heightmap = cubemap;
            this.m_mapResolutionMinusOne = this.m_heightmap.Resolution - 1;
            this.m_heightRatio = this.m_maxHillHeight - this.m_minHillHeight;
            this.m_heightRatioRecip = 1f / this.m_heightRatio;
            float faceSize = (float) ((radius * 3.1415926535897931) * 0.5);
            this.m_pixelSize = faceSize / ((float) this.m_heightmap.Resolution);
            this.m_pixelSizeRecip = 1f / this.m_pixelSize;
            this.m_pixelSizeRecip2 = 0.5f / this.m_pixelSize;
            double num2 = (Math.Sin(Math.Acos((double) ((radius - 1f) / radius))) * 2.0) * radius;
            this.m_curvatureThresholdRecip = 1.0 / num2;
            this.m_pixelSize4 = this.m_pixelSize * 4f;
            this.m_voxelSize = (float) (2.0 / (radius * 3.1415926535897931));
            this.m_mapStepScale = this.m_pixelSize / this.m_heightRatio;
            this.m_mapStepScaleSquare = this.m_mapStepScale * this.m_mapStepScale;
            if (definition.Detail != null)
            {
                this.m_detail.Init(texProvider, definition.Detail, faceSize);
            }
            VrPlanetShape.Mapset maps = this.m_heightmap.GetMapset();
            VrPlanetShape.DetailMapData detailMapData = new VrPlanetShape.DetailMapData();
            if (definition.Detail != null)
            {
                detailMapData = this.m_detail.GetDetailMapData();
            }
            this.m_nativeShape = new VrPlanetShape(translation, radius, definition.HillParams.Min, definition.HillParams.Max, maps, detailMapData, true);
            this.Closed = false;
        }

        public float AltitudeToRatio(float altitude) => 
            ((altitude - this.m_minHillHeight) * this.m_heightRatioRecip);

        private unsafe void CalculateCacheCell(MyHeightmapFace map, Cache.Cell* cell, bool compouteBounds = false)
        {
            ushort* numPtr2;
            ushort[] pinned numArray;
            int x = cell.Coord.X;
            int y = cell.Coord.Y;
            float* values = &s_Cz.M11;
            int linearOfft = (map.GetRowStart(y - 1) + x) - 1;
            if (((numArray = map.Data) == null) || (numArray.Length == 0))
            {
                numPtr2 = null;
            }
            else
            {
                numPtr2 = numArray;
            }
            map.Get4Row(linearOfft, values, numPtr2);
            linearOfft += map.RowStride;
            map.Get4Row(linearOfft, values + (4 * 4), numPtr2);
            linearOfft += map.RowStride;
            map.Get4Row(linearOfft, values + (8 * 4), numPtr2);
            linearOfft += map.RowStride;
            map.Get4Row(linearOfft, values + (12 * 4), numPtr2);
            Matrix.Multiply(ref CR, ref s_Cz, out cell.Gz);
            Matrix.Multiply(ref cell.Gz, ref CRT, out cell.Gz);
            if (!compouteBounds)
            {
                cell.Max = 1f;
                cell.Min = 0f;
            }
            else
            {
                Matrix matrix;
                float positiveInfinity = float.PositiveInfinity;
                float negativeInfinity = float.NegativeInfinity;
                Matrix.Multiply(ref BInv, ref cell.Gz, out matrix);
                Matrix* matrixPtr1 = (Matrix*) ref matrix;
                Matrix.Multiply(ref (Matrix) ref matrixPtr1, ref BInvT, out matrix);
                float* numPtr3 = &matrix.M11;
                int num6 = 0;
                while (true)
                {
                    if (num6 >= 0x10)
                    {
                        cell.Max = negativeInfinity;
                        cell.Min = positiveInfinity;
                        break;
                    }
                    if (negativeInfinity < numPtr3[num6 * 4])
                    {
                        negativeInfinity = numPtr3[num6 * 4];
                    }
                    if (positiveInfinity > numPtr3[num6 * 4])
                    {
                        positiveInfinity = numPtr3[num6 * 4];
                    }
                    num6++;
                }
            }
            numArray = null;
            fixed (float* numRef = null)
            {
                return;
            }
        }

        private unsafe void CalculateDistanceFieldInternal(Vector3 localPos, int faceHint, Vector3I min, Vector3I max, Vector3I writeOffsetLoc, MyStorageData target, float lodVoxelSize)
        {
            Vector3I vectori;
            this.PrepareCache();
            Vector3 vector = localPos;
            if (faceHint == -1)
            {
                vectori.Z = min.Z;
                while (vectori.Z <= max.Z)
                {
                    vectori.Y = min.Y;
                    while (true)
                    {
                        if (vectori.Y > max.Y)
                        {
                            float* singlePtr3 = (float*) ref localPos.Z;
                            singlePtr3[0] += lodVoxelSize;
                            localPos.Y = vector.Y;
                            int* numPtr3 = (int*) ref vectori.Z;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.X = min.X;
                        Vector3I p = (Vector3I) (vectori + writeOffsetLoc);
                        int linearIdx = target.ComputeLinear(ref p);
                        while (true)
                        {
                            if (vectori.X > max.X)
                            {
                                float* singlePtr2 = (float*) ref localPos.Y;
                                singlePtr2[0] += lodVoxelSize;
                                localPos.X = vector.X;
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            byte content = (byte) (((MathHelper.Clamp(-(this.SignedDistanceLocal(localPos, lodVoxelSize) / lodVoxelSize), -1f, 1f) * 0.5f) + 0.5f) * 255f);
                            target.Content(linearIdx, content);
                            linearIdx += target.StepLinear;
                            float* singlePtr1 = (float*) ref localPos.X;
                            singlePtr1[0] += lodVoxelSize;
                            int* numPtr1 = (int*) ref vectori.X;
                            numPtr1[0]++;
                        }
                    }
                }
            }
            else
            {
                vectori.Z = min.Z;
                while (vectori.Z <= max.Z)
                {
                    vectori.Y = min.Y;
                    while (true)
                    {
                        if (vectori.Y > max.Y)
                        {
                            float* singlePtr6 = (float*) ref localPos.Z;
                            singlePtr6[0] += lodVoxelSize;
                            localPos.Y = vector.Y;
                            int* numPtr6 = (int*) ref vectori.Z;
                            numPtr6[0]++;
                            break;
                        }
                        vectori.X = min.X;
                        Vector3I p = (Vector3I) (vectori + writeOffsetLoc);
                        int linearIdx = target.ComputeLinear(ref p);
                        while (true)
                        {
                            if (vectori.X > max.X)
                            {
                                float* singlePtr5 = (float*) ref localPos.Y;
                                singlePtr5[0] += lodVoxelSize;
                                localPos.X = vector.X;
                                int* numPtr5 = (int*) ref vectori.Y;
                                numPtr5[0]++;
                                break;
                            }
                            byte content = (byte) (((MathHelper.Clamp(-(this.SignedDistanceLocal(localPos, lodVoxelSize, faceHint) / lodVoxelSize), -1f, 1f) * 0.5f) + 0.5f) * 255f);
                            target.Content(linearIdx, content);
                            linearIdx += target.StepLinear;
                            float* singlePtr4 = (float*) ref localPos.X;
                            singlePtr4[0] += lodVoxelSize;
                            int* numPtr4 = (int*) ref vectori.X;
                            numPtr4[0]++;
                        }
                    }
                }
            }
        }

        internal Vector3 Center() => 
            this.m_translation;

        public void Close()
        {
            this.m_heightmap = null;
            this.Closed = true;
        }

        internal float DistanceToRatio(float distance) => 
            ((distance - this.InnerRadius) * this.m_heightRatioRecip);

        public unsafe void GetBounds(ref BoundingBox box)
        {
            Vector3* corners = (Vector3*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3))];
            box.GetCornersUnsafe(corners);
            this.GetBounds(corners, 8, out box.Min.Z, out box.Max.Z);
        }

        public unsafe void GetBounds(Vector3* localPoints, int pointCount, out float minHeight, out float maxHeight)
        {
            int face = -1;
            for (int i = 0; i < pointCount; i++)
            {
                int num3;
                MyCubemapHelpers.GetCubeFace(ref (Vector3) ref (localPoints + i), out num3);
                if (face == -1)
                {
                    face = num3;
                }
            }
            BoundingBox query = new BoundingBox(new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0f), new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0f));
            for (int j = 0; j < pointCount; j++)
            {
                Vector2 vector;
                MyCubemapHelpers.CalculateTexcoordForFace(ref (Vector3) ref (localPoints + j), face, out vector);
                float f = vector.X * vector.Y;
                if (!float.IsNaN(f) && !float.IsInfinity(f))
                {
                    if (vector.X < query.Min.X)
                    {
                        query.Min.X = vector.X;
                    }
                    if (vector.X > query.Max.X)
                    {
                        query.Max.X = vector.X;
                    }
                    if (vector.Y < query.Min.Y)
                    {
                        query.Min.Y = vector.Y;
                    }
                    if (vector.Y > query.Max.Y)
                    {
                        query.Max.Y = vector.Y;
                    }
                }
            }
            this.m_heightmap.Faces[face].GetBounds(ref query);
            minHeight = (query.Min.Z * this.m_heightRatio) + this.InnerRadius;
            maxHeight = (query.Max.Z * this.m_heightRatio) + this.InnerRadius;
        }

        public unsafe void GetBounds(Vector3D* localPoints, int pointCount, out float minHeight, out float maxHeight)
        {
            int face = -1;
            for (int i = 0; i < pointCount; i++)
            {
                int num3;
                Vector3 position = *((Vector3*) (localPoints + i));
                MyCubemapHelpers.GetCubeFace(ref position, out num3);
                if (face == -1)
                {
                    face = num3;
                }
            }
            BoundingBox query = new BoundingBox(new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0f), new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0f));
            for (int j = 0; j < pointCount; j++)
            {
                Vector2 vector2;
                Vector3 localPos = *((Vector3*) (localPoints + j));
                MyCubemapHelpers.CalculateTexcoordForFace(ref localPos, face, out vector2);
                if (vector2.X < query.Min.X)
                {
                    query.Min.X = vector2.X;
                }
                if (vector2.X > query.Max.X)
                {
                    query.Max.X = vector2.X;
                }
                if (vector2.Y < query.Min.Y)
                {
                    query.Min.Y = vector2.Y;
                }
                if (vector2.Y > query.Max.Y)
                {
                    query.Max.Y = vector2.Y;
                }
            }
            this.m_heightmap.Faces[face].GetBounds(ref query);
            minHeight = (query.Min.Z * this.m_heightRatio) + this.InnerRadius;
            maxHeight = (query.Max.Z * this.m_heightRatio) + this.InnerRadius;
        }

        public void GetBounds(ref BoundingBox2 texcoordRange, int face, out float min, out float max)
        {
            BoundingBox query = new BoundingBox(new Vector3(texcoordRange.Min, 0f), new Vector3(texcoordRange.Max, 0f));
            this.m_heightmap.Faces[face].GetBounds(ref query);
            min = query.Min.Z;
            max = query.Max.Z;
        }

        public double GetDistanceToSurfaceCacheless(Vector3 localPos)
        {
            int num2;
            Vector2 vector;
            Vector3 vector2;
            float f = localPos.Length();
            if (f.IsZero(0.0001f))
            {
                return 0.0;
            }
            if (!f.IsValid())
            {
                return 0.0;
            }
            MyCubemapHelpers.CalculateSampleTexcoord(ref localPos, out num2, out vector);
            return (double) (f - (this.GetValueForPositionCacheless(num2, ref vector, out vector2) + this.Radius));
        }

        public double GetDistanceToSurfaceWithCache(Vector3 localPos)
        {
            int num2;
            Vector2 vector;
            Vector3 vector2;
            float f = localPos.Length();
            if (f.IsZero(0.0001f))
            {
                return 0.0;
            }
            MyCubemapHelpers.CalculateSampleTexcoord(ref localPos, out num2, out vector);
            return (double) (f - (this.GetValueForPositionWithCache(num2, ref vector, out vector2) + this.Radius));
        }

        public float GetHeight(int face, ref Vector2 texcoord, out Vector3 localNormal) => 
            ((this.m_nativeShape.GetValue(ref texcoord, face, out localNormal) * this.m_heightRatio) + this.InnerRadius);

        public static float GetTau() => 
            Tau;

        public float GetValue(int face, ref Vector2 texcoord, out Vector3 localNormal) => 
            this.m_nativeShape.GetValue(ref texcoord, face, out localNormal);

        public unsafe float GetValueForPositionCacheless(int face, ref Vector2 texcoord, out Vector3 localNormal)
        {
            Cache.Cell cell;
            if (this.m_heightmap == null)
            {
                localNormal = Vector3.Zero;
                return 0f;
            }
            Vector2 vector = texcoord * this.m_mapResolutionMinusOne;
            if (((vector.X >= this.m_heightmap.Resolution) || ((vector.Y >= this.m_heightmap.Resolution) || (vector.X < 0f))) || (vector.Y < 0f))
            {
                localNormal = Vector3.Zero;
                return 0f;
            }
            Cache.Cell* cellPtr = &cell;
            cellPtr->Coord = new Vector3I((int) vector.X, (int) vector.Y, face);
            this.CalculateCacheCell(this.m_heightmap.Faces[face], cellPtr, false);
            return ((this.SampleHeightBicubic(vector.X - ((float) Math.Floor((double) vector.X)), vector.Y - ((float) Math.Floor((double) vector.Y)), ref cellPtr->Gz, out localNormal) * this.m_heightRatio) + this.m_minHillHeight);
        }

        internal unsafe float GetValueForPositionInternal(int face, ref Vector2 texcoord, float lodSize, float distance, out Vector3 Normal)
        {
            float num;
            Vector2 vector = texcoord * this.m_mapResolutionMinusOne;
            float s = vector.X - ((float) Math.Floor((double) vector.X));
            float t = vector.Y - ((float) Math.Floor((double) vector.Y));
            int x = (int) vector.X;
            int y = (int) vector.Y;
            if (lodSize >= this.m_pixelSize)
            {
                num = this.SampleHeightBilinear(this.m_heightmap.Faces[face], lodSize, s, t, x, y, out Normal);
            }
            else
            {
                Vector3I coord = new Vector3I(x, y, face);
                Cache.Cell* cell = &(m_cache.Cells[m_cache.CellCoord(ref coord)]);
                if (cell->Coord != coord)
                {
                    cell->Coord = coord;
                    this.CalculateCacheCell(this.m_heightmap.Faces[face], cell, true);
                }
                float num6 = (distance - this.InnerRadius) * this.m_heightRatioRecip;
                float num7 = lodSize * this.m_heightRatioRecip;
                if (num6 > (cell->Max + num7))
                {
                    Normal = Vector3.Backward;
                    return float.NegativeInfinity;
                }
                if (num6 < (cell->Min - num7))
                {
                    Normal = Vector3.Backward;
                    return float.PositiveInfinity;
                }
                num = this.SampleHeightBicubic(vector.X - ((float) Math.Floor((double) vector.X)), vector.Y - ((float) Math.Floor((double) vector.Y)), ref cell->Gz, out Normal);
            }
            return ((num * this.m_heightRatio) + this.m_minHillHeight);
        }

        public unsafe float GetValueForPositionWithCache(int face, ref Vector2 texcoord, out Vector3 localNormal)
        {
            Vector2 vector = texcoord * this.m_mapResolutionMinusOne;
            Vector3I coord = new Vector3I((int) vector.X, (int) vector.Y, face);
            Cache.Cell* cell = &(m_cache.Cells[m_cache.CellCoord(ref coord)]);
            if (cell->Coord != coord)
            {
                cell->Coord = coord;
                this.CalculateCacheCell(this.m_heightmap.Faces[face], cell, false);
            }
            return ((this.SampleHeightBicubic(vector.X - ((float) Math.Floor((double) vector.X)), vector.Y - ((float) Math.Floor((double) vector.Y)), ref cell->Gz, out localNormal) * this.m_heightRatio) + this.m_minHillHeight);
        }

        public ContainmentType IntersectBoundingBox(ref BoundingBox box, float lodLevel)
        {
            bool flag;
            ContainmentType type;
            int num;
            box.Inflate((float) 1f);
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, this.OuterRadius + lodLevel);
            sphere.Intersects(ref box, out flag);
            if (!flag)
            {
                return ContainmentType.Disjoint;
            }
            sphere.Radius = this.InnerRadius - lodLevel;
            sphere.Contains(ref box, out type);
            return ((type != ContainmentType.Contains) ? this.IntersectBoundingBoxInternal(ref box, lodLevel, out num) : ContainmentType.Contains);
        }

        private unsafe ContainmentType IntersectBoundingBoxCornerCase(uint faces, Vector3* vertices, float minHeight, float maxHeight)
        {
            BoundingBox query = new BoundingBox(new Vector3(float.PositiveInfinity, float.PositiveInfinity, minHeight), new Vector3(float.NegativeInfinity, float.NegativeInfinity, maxHeight));
            query.Min.Z = (((query.Min.Z - this.m_radius) - this.m_detailScale) - this.m_minHillHeight) * this.m_heightRatioRecip;
            query.Max.Z = ((query.Max.Z - this.m_radius) - this.m_minHillHeight) * this.m_heightRatioRecip;
            ContainmentType type = ~ContainmentType.Disjoint;
            ContainmentType type2 = type;
            for (int i = 0; i <= 5; i++)
            {
                if ((faces & (1 << (i & 0x1f))) != 0)
                {
                    query.Min.X = float.PositiveInfinity;
                    query.Min.Y = float.PositiveInfinity;
                    query.Max.X = float.NegativeInfinity;
                    query.Max.Y = float.NegativeInfinity;
                    int num2 = 0;
                    while (true)
                    {
                        Vector2 vector;
                        if (num2 >= 8)
                        {
                            ContainmentType type3 = this.m_heightmap.Faces[i].QueryHeight(ref query);
                            if (type3 != type2)
                            {
                                if (type2 != type)
                                {
                                    return ContainmentType.Intersects;
                                }
                                type2 = type3;
                            }
                            break;
                        }
                        MyCubemapHelpers.TexcoordCalculators[i](ref (Vector3) ref (vertices + num2), out vector);
                        if (vector.X < query.Min.X)
                        {
                            query.Min.X = vector.X;
                        }
                        if (vector.X > query.Max.X)
                        {
                            query.Max.X = vector.X;
                        }
                        if (vector.Y < query.Min.Y)
                        {
                            query.Min.Y = vector.Y;
                        }
                        if (vector.Y > query.Max.Y)
                        {
                            query.Max.Y = vector.Y;
                        }
                        num2++;
                    }
                }
            }
            return type2;
        }

        protected unsafe ContainmentType IntersectBoundingBoxInternal(ref BoundingBox box, float lodLevel, out int boxFace)
        {
            float num3;
            Vector3 vector2;
            int face = -1;
            uint faces = 0;
            bool flag = false;
            Vector3* corners = (Vector3*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3))];
            box.GetCornersUnsafe(corners);
            for (int i = 0; i < 8; i++)
            {
                int num6;
                MyCubemapHelpers.GetCubeFace(ref (Vector3) ref (corners + i), out num6);
                if (face == -1)
                {
                    face = num6;
                }
                else if (face != num6)
                {
                    flag = true;
                }
                faces |= (uint) (1 << (num6 & 0x1f));
            }
            if (Vector3.Zero.IsInsideInclusive(ref box.Min, ref box.Max))
            {
                num3 = 0f;
            }
            else
            {
                num3 = Vector3.Clamp(Vector3.Zero, box.Min, box.Max).Length();
            }
            Vector3 center = box.Center;
            vector2.X = (center.X >= 0f) ? box.Max.X : box.Min.X;
            Vector3 local1 = center;
            vector2.Y = (local1.Y >= 0f) ? box.Max.Y : box.Min.Y;
            vector2.Z = (local1.Z >= 0f) ? box.Max.Z : box.Min.Z;
            float maxHeight = vector2.Length();
            if (flag)
            {
                boxFace = -1;
                return this.IntersectBoundingBoxCornerCase(faces, corners, num3, maxHeight);
            }
            BoundingBox query = new BoundingBox(new Vector3(float.PositiveInfinity, float.PositiveInfinity, num3), new Vector3(float.NegativeInfinity, float.NegativeInfinity, maxHeight));
            for (int j = 0; j < 8; j++)
            {
                Vector2 vector3;
                MyCubemapHelpers.CalculateTexcoordForFace(ref (Vector3) ref (corners + j), face, out vector3);
                if (vector3.X < query.Min.X)
                {
                    query.Min.X = vector3.X;
                }
                if (vector3.X > query.Max.X)
                {
                    query.Max.X = vector3.X;
                }
                if (vector3.Y < query.Min.Y)
                {
                    query.Min.Y = vector3.Y;
                }
                if (vector3.Y > query.Max.Y)
                {
                    query.Max.Y = vector3.Y;
                }
            }
            query.Min.Z = (((query.Min.Z - this.m_radius) - this.m_detailScale) - this.m_minHillHeight) * this.m_heightRatioRecip;
            query.Max.Z = ((query.Max.Z - this.m_radius) - this.m_minHillHeight) * this.m_heightRatioRecip;
            boxFace = face;
            return this.m_heightmap.Faces[face].QueryHeight(ref query);
        }

        public unsafe bool IntersectLine(ref LineD ll, out double startOffset, out double endOffset)
        {
            BoundingBox boundingBox = (BoundingBox) ll.GetBoundingBox();
            int face = -1;
            uint faces = 0;
            bool flag = false;
            Vector3* corners = (Vector3*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3))];
            boundingBox.GetCornersUnsafe(corners);
            for (int i = 0; i < 8; i++)
            {
                int num4;
                MyCubemapHelpers.GetCubeFace(ref (Vector3) ref (corners + i), out num4);
                if (face == -1)
                {
                    face = num4;
                }
                else if (face != num4)
                {
                    flag = true;
                }
                faces |= (uint) (1 << (num4 & 0x1f));
            }
            return (!flag ? this.IntersectLineFace(ref ll, face, out startOffset, out endOffset) : this.IntersectLineCornerCase(ref ll, faces, out startOffset, out endOffset));
        }

        private bool IntersectLineCornerCase(ref LineD line, uint faces, out double startOffset, out double endOffset)
        {
            startOffset = 1.0;
            endOffset = 0.0;
            return true;
        }

        public bool IntersectLineFace(ref LineD ll, int face, out double startOffset, out double endOffset)
        {
            Vector2 vector;
            Vector2 vector2;
            Vector3 from = (Vector3) ll.From;
            Vector3 to = (Vector3) ll.To;
            MyCubemapHelpers.CalculateTexcoordForFace(ref from, face, out vector);
            MyCubemapHelpers.CalculateTexcoordForFace(ref to, face, out vector2);
            int num = (int) Math.Ceiling((double) ((vector2 - vector).Length() * this.m_heightmap.Resolution));
            double num2 = 1.0 / ((double) num);
            for (int i = 0; i < num; i++)
            {
                float num6;
                float num7;
                from = (Vector3) (ll.From + (((ll.Direction * ll.Length) * i) * num2));
                to = (Vector3) (ll.From + (((ll.Direction * ll.Length) * (i + 1)) * num2));
                float num4 = from.Length();
                float num5 = to.Length();
                MyCubemapHelpers.CalculateTexcoordForFace(ref from, face, out vector);
                MyCubemapHelpers.CalculateTexcoordForFace(ref to, face, out vector2);
                from.X = vector.X;
                from.Y = vector.Y;
                from.Z = ((num4 - this.m_radius) - this.m_minHillHeight) * this.m_heightRatioRecip;
                to.X = vector2.X;
                to.Y = vector2.Y;
                to.Z = ((num5 - this.m_radius) - this.m_minHillHeight) * this.m_heightRatioRecip;
                if (this.m_heightmap[face].QueryLine(ref from, ref to, out num6, out num7))
                {
                    startOffset = Math.Max((double) ((i + num6) * num2), (double) 0.0);
                    endOffset = 1.0;
                    return true;
                }
            }
            startOffset = 0.0;
            endOffset = 1.0;
            return false;
        }

        public void PrepareCache()
        {
            if (this.m_heightmap.Name != m_cache.Name)
            {
                m_cache.Name = this.m_heightmap.Name;
                m_cache.Clean();
            }
        }

        public bool ProjectToSurface(Vector3 localPos, out Vector3 surface)
        {
            int num2;
            Vector2 vector2;
            Vector3 vector3;
            float f = localPos.Length();
            if (f.IsZero(0.0001f))
            {
                surface = localPos;
                return false;
            }
            MyCubemapHelpers.CalculateSampleTexcoord(ref localPos, out num2, out vector2);
            float num3 = this.GetValueForPositionCacheless(num2, ref vector2, out vector3) + this.Radius;
            surface = (Vector3) (num3 * (localPos / f));
            return true;
        }

        internal void ReadContentRange(ref MyVoxelDataRequest req, bool detectOnly = false)
        {
            ContainmentType disjoint;
            if (this.Closed)
            {
                return;
            }
            float size = (1 << (req.Lod & 0x1f)) * 1f;
            Vector3I minInLod = req.MinInLod;
            Vector3I maxInLod = req.MaxInLod;
            Vector3 min = ((Vector3) (minInLod * size)) - this.m_translation;
            int boxFace = -1;
            MyVoxelRequestFlags flags = req.RequestFlags & ~(MyVoxelRequestFlags.ContentCheckedDeep | MyVoxelRequestFlags.ContentChecked | MyVoxelRequestFlags.FullContent | MyVoxelRequestFlags.EmptyData);
            if ((req.MinInLod - req.MaxInLod).Size > 8)
            {
                bool flag;
                BoundingBox box = new BoundingBox(min, min + ((maxInLod - minInLod) * size));
                box.Inflate(size);
                BoundingSphere sphere = new BoundingSphere(Vector3.Zero, this.OuterRadius + size);
                sphere.Intersects(ref box, out flag);
                if (flag)
                {
                    ContainmentType type2;
                    sphere.Radius = this.InnerRadius - size;
                    sphere.Contains(ref box, out type2);
                    if (type2 != ContainmentType.Contains)
                    {
                        disjoint = this.IntersectBoundingBoxInternal(ref box, size, out boxFace);
                        if (disjoint == ContainmentType.Intersects)
                        {
                            goto TR_0016;
                        }
                    }
                    else
                    {
                        disjoint = type2;
                    }
                    goto TR_0009;
                }
                else
                {
                    disjoint = ContainmentType.Disjoint;
                    goto TR_0009;
                }
            }
            goto TR_0016;
        TR_0009:
            if (disjoint == ContainmentType.Disjoint)
            {
                if (req.RequestFlags.HasFlags(MyVoxelRequestFlags.ContentChecked))
                {
                    flags |= MyVoxelRequestFlags.ContentCheckedDeep | MyVoxelRequestFlags.ContentChecked | MyVoxelRequestFlags.EmptyData;
                }
                else
                {
                    req.Target.BlockFillContent(req.Offset, ((Vector3I) (req.Offset + maxInLod)) - minInLod, 0);
                }
            }
            else if (disjoint == ContainmentType.Contains)
            {
                if (req.RequestFlags.HasFlags(MyVoxelRequestFlags.ContentChecked))
                {
                    flags |= MyVoxelRequestFlags.ContentCheckedDeep | MyVoxelRequestFlags.ContentChecked | MyVoxelRequestFlags.FullContent;
                }
                else
                {
                    req.Target.BlockFillContent(req.Offset, ((Vector3I) (req.Offset + maxInLod)) - minInLod, 0xff);
                }
            }
            req.Flags = flags;
            return;
        TR_0016:
            if (!detectOnly)
            {
                if (this.m_nativeShape == null)
                {
                    this.CalculateDistanceFieldInternal(min, boxFace, minInLod, maxInLod, req.Offset - minInLod, req.Target, size);
                }
                else
                {
                    int offset = req.Target.ComputeLinear(ref req.Offset);
                    Vector3I step = req.Target.Step;
                    this.m_nativeShape.ReadContentRange(req.Target[MyStorageDataTypeEnum.Content], offset, ref step, ref req.MinInLod, ref req.MaxInLod, size, boxFace);
                }
            }
            else if (this.m_nativeShape != null)
            {
                int num3 = this.m_nativeShape.CheckContentRange(ref req.MinInLod, ref req.MaxInLod, size, boxFace);
                if (num3 == 0)
                {
                    flags |= MyVoxelRequestFlags.EmptyData;
                }
                else if (num3 == 0xff)
                {
                    flags |= MyVoxelRequestFlags.FullContent;
                }
            }
            req.Flags = flags;
        }

        private float SampleHeightBicubic(float s, float t, ref Matrix Gz, out Vector3 Normal)
        {
            float num = s * s;
            float num2 = num * s;
            float num3 = t * t;
            float num4 = num3 * t;
            float num5 = ((Gz.M12 + (Gz.M22 * t)) + (Gz.M32 * num3)) + (Gz.M42 * num4);
            float num6 = ((Gz.M13 + (Gz.M23 * t)) + (Gz.M33 * num3)) + (Gz.M43 * num4);
            float num7 = ((Gz.M14 + (Gz.M24 * t)) + (Gz.M34 * num3)) + (Gz.M44 * num4);
            float x = (num5 + ((2f * s) * num6)) + ((3f * num) * num7);
            Normal = new Vector3(x, ((((Gz.M21 + (Gz.M22 * s)) + (Gz.M23 * num)) + (Gz.M24 * num2)) + ((2f * t) * (((Gz.M31 + (Gz.M32 * s)) + (Gz.M33 * num)) + (Gz.M34 * num2)))) + ((3f * num3) * (((Gz.M41 + (Gz.M42 * s)) + (Gz.M43 * num)) + (Gz.M44 * num2))), this.m_mapStepScale);
            Normal.Normalize();
            return ((((((Gz.M11 + (Gz.M21 * t)) + (Gz.M31 * num3)) + (Gz.M41 * num4)) + (s * num5)) + (num * num6)) + (num2 * num7));
        }

        private float SampleHeightBilinear(MyHeightmapFace map, float lodSize, float s, float t, int sx, int sy, out Vector3 Normal)
        {
            float num = lodSize * this.m_pixelSizeRecip2;
            float num2 = 1f - s;
            float num3 = 1f - t;
            int x = Math.Min(sx + ((int) Math.Ceiling((double) num)), this.m_heightmap.Resolution);
            int y = Math.Min(sy + ((int) Math.Ceiling((double) num)), this.m_heightmap.Resolution);
            float valuef = map.GetValuef(sx, sy);
            float num7 = map.GetValuef(x, sy);
            float num8 = map.GetValuef(sx, y);
            float num9 = map.GetValuef(x, y);
            float num10 = ((num7 - valuef) * num3) + ((num9 - num8) * t);
            float num11 = ((num8 - valuef) * num2) + ((num9 - num7) * s);
            Normal = new Vector3(this.m_mapStepScale * num10, this.m_mapStepScale * num11, this.m_mapStepScaleSquare);
            Normal.Normalize();
            return (((((valuef * num2) * num3) + ((num7 * s) * num3)) + ((num8 * num2) * t)) + ((num9 * s) * t));
        }

        public static void SetTau(float tau)
        {
            Tau = tau;
            CRT = new Matrix(0f, -Tau, 2f * Tau, -Tau, 1f, 0f, Tau - 3f, 2f - Tau, 0f, Tau, 3f - (2f * Tau), Tau - 2f, 0f, 0f, -Tau, Tau);
            CR = Matrix.Transpose(CRT);
        }

        internal float SignedDistanceLocal(Vector3 position, float lodVoxelSize)
        {
            int num2;
            Vector2 vector;
            Vector3 vector2;
            float distance = position.Length();
            if ((distance <= 0.1) || (distance < (this.InnerRadius - lodVoxelSize)))
            {
                return -lodVoxelSize;
            }
            if (distance > (this.OuterRadius + lodVoxelSize))
            {
                return float.PositiveInfinity;
            }
            MyCubemapHelpers.CalculateSampleTexcoord(ref position, out num2, out vector);
            float num3 = this.GetValueForPositionInternal(num2, ref vector, lodVoxelSize, distance, out vector2);
            if (this.m_detail.Matches(vector2.Z))
            {
                float dtx = vector.X * this.m_detail.Factor;
                float num5 = vector.Y * this.m_detail.Factor;
                dtx -= (float) Math.Floor((double) dtx);
                num3 += this.m_detail.GetValue(dtx, num5 - ((float) Math.Floor((double) num5)), vector2.Z);
            }
            return (((distance - this.m_radius) - num3) * vector2.Z);
        }

        internal float SignedDistanceLocal(Vector3 position, float lodVoxelSize, int face)
        {
            Vector2 vector;
            Vector3 vector2;
            float distance = position.Length();
            if ((distance <= 0.1) || (distance < (this.InnerRadius - lodVoxelSize)))
            {
                return -lodVoxelSize;
            }
            if (distance > (this.OuterRadius + lodVoxelSize))
            {
                return float.PositiveInfinity;
            }
            MyCubemapHelpers.CalculateTexcoordForFace(ref position, face, out vector);
            float num2 = this.GetValueForPositionInternal(face, ref vector, lodVoxelSize, distance, out vector2);
            if (this.m_detail.Matches(vector2.Z))
            {
                float dtx = vector.X * this.m_detail.Factor;
                float num4 = vector.Y * this.m_detail.Factor;
                dtx -= (float) Math.Floor((double) dtx);
                num2 += this.m_detail.GetValue(dtx, num4 - ((float) Math.Floor((double) num4)), vector2.Z);
            }
            return (((distance - this.m_radius) - num2) * vector2.Z);
        }

        internal float SignedDistanceLocalCacheless(Vector3 position)
        {
            int num2;
            Vector2 vector;
            Vector3 vector2;
            float num = position.Length();
            if ((num <= 0.1) || (num < (this.InnerRadius - 1f)))
            {
                return -1f;
            }
            if (num > (this.OuterRadius + 1f))
            {
                return float.PositiveInfinity;
            }
            MyCubemapHelpers.CalculateSampleTexcoord(ref position, out num2, out vector);
            float num3 = this.GetValueForPositionCacheless(num2, ref vector, out vector2);
            if (this.m_detail.Matches(vector2.Z))
            {
                float dtx = vector.X * this.m_detail.Factor;
                float num5 = vector.Y * this.m_detail.Factor;
                dtx -= (float) Math.Floor((double) dtx);
                num3 += this.m_detail.GetValue(dtx, num5 - ((float) Math.Floor((double) num5)), vector2.Z);
            }
            return (((num - this.m_radius) - num3) * vector2.Z);
        }

        public float SignedDistanceWithSample(float lodVoxelSize, float distance, float value) => 
            ((distance - this.m_radius) - value);

        public float OuterRadius { get; private set; }

        public float InnerRadius { get; private set; }

        public bool Closed { get; private set; }

        public float MinHillHeight =>
            this.m_minHillHeight;

        public float MaxHillHeight =>
            this.m_maxHillHeight;

        public float Radius =>
            this.m_radius;

        public MyHeightCubemap Heightmap =>
            this.m_heightmap;

        public float HeightRatio =>
            this.m_heightRatio;

        [StructLayout(LayoutKind.Sequential)]
        private struct Cache
        {
            private const int CacheBits = 4;
            private const int CacheMask = 15;
            private const int CacheSize = 0x100;
            public Cell[] Cells;
            public string Name;
            public int CellCoord(ref Vector3I coord) => 
                (((coord.Y & 15) << 4) | (coord.X & 15));

            internal void Clean()
            {
                if (this.Cells == null)
                {
                    this.Cells = new Cell[0x100];
                }
                for (int i = 0; i < 0x100; i++)
                {
                    this.Cells[i].Coord = new Vector3I(-1);
                }
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct Cell
            {
                public Matrix Gz;
                public float Min;
                public float Max;
                public Vector3I Coord;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SurfaceDetailSampler : IDisposable
        {
            private MyHeightDetailTexture m_detail;
            public float Factor;
            public float Size;
            public float Scale;
            private float m_min;
            private float m_max;
            private float m_in;
            private float m_out;
            private float m_inRecip;
            private float m_outRecip;
            private float m_mid;
            private GCHandle m_handle;
            public void Init(MyPlanetTextureMapProvider texProvider, MyPlanetSurfaceDetail def, float faceSize)
            {
                this.m_detail = texProvider.GetDetailMap(def.Texture);
                this.Size = def.Size;
                this.Factor = faceSize / this.Size;
                this.m_min = (float) Math.Cos((double) MathHelper.ToRadians(def.Slope.Max));
                this.m_max = (float) Math.Cos((double) MathHelper.ToRadians(def.Slope.Min));
                this.m_in = (float) Math.Cos((double) MathHelper.ToRadians((float) (def.Slope.Max - def.Transition)));
                this.m_out = (float) Math.Cos((double) MathHelper.ToRadians((float) (def.Slope.Min + def.Transition)));
                this.m_inRecip = 1f / (this.m_in - this.m_min);
                this.m_outRecip = 1f / (this.m_max - this.m_out);
                this.m_mid = (float) Math.Cos((double) MathHelper.ToRadians((float) ((def.Slope.Max + def.Slope.Min) / 2f)));
                this.Scale = def.Scale;
                this.m_handle = GCHandle.Alloc(this.m_detail.Data, GCHandleType.Pinned);
            }

            public void Dispose()
            {
                if (this.m_handle.IsAllocated)
                {
                    this.m_handle.Free();
                }
            }

            internal bool Matches(float angle) => 
                ((angle <= this.m_max) && (angle >= this.m_min));

            internal float GetValue(float dtx, float dty, float angle)
            {
                if (this.m_detail == null)
                {
                    return 0f;
                }
                float num = 1f;
                num = (angle <= this.m_mid) ? Math.Max(Math.Min((float) ((angle - this.m_in) * this.m_inRecip), (float) 1f), 0f) : Math.Min(Math.Max((float) (1f - ((angle - this.m_out) * this.m_outRecip)), (float) 0f), 1f);
                return ((this.m_detail.GetValue(dtx, dty) * num) * this.Scale);
            }

            internal unsafe VrPlanetShape.DetailMapData GetDetailMapData() => 
                new VrPlanetShape.DetailMapData { 
                    Data = (byte*) this.m_handle.AddrOfPinnedObject().ToPointer(),
                    Factor = this.Factor,
                    Size = this.Size,
                    Resolution = (int) this.m_detail.Resolution,
                    Scale = this.Scale,
                    m_min = this.m_min,
                    m_max = this.m_max,
                    m_in = this.m_in,
                    m_out = this.m_out,
                    m_inRecip = this.m_inRecip,
                    m_outRecip = this.m_outRecip,
                    m_mid = this.m_mid
                };
        }
    }
}


namespace Sandbox.Engine.Voxels
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels.Planet;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    [MyStorageDataProvider(0x273a)]
    public class MyPlanetStorageProvider : IMyStorageDataProvider
    {
        private static readonly int STORAGE_VERSION = 1;
        private PlanetData m_data;

        public void Close()
        {
            this.Material.Close();
            this.Shape.Close();
            this.Closed = true;
        }

        public unsafe void ComputeCombinedMaterialAndSurface(Vector3 position, bool useCache, out MySurfaceParams props)
        {
            if (this.Closed)
            {
                props = new MySurfaceParams();
            }
            else
            {
                MyPlanetMaterialProvider.MaterialSampleParams @params;
                int num2;
                Vector2 vector2;
                position -= this.Shape.Center();
                float num = position.Length();
                @params.Gravity = position / num;
                props.Latitude = @params.Gravity.Y;
                Vector2 vector = new Vector2(-@params.Gravity.X, -@params.Gravity.Z);
                vector.Normalize();
                props.Longitude = vector.Y;
                if (-@params.Gravity.X > 0f)
                {
                    props.Longitude = 2f - props.Longitude;
                }
                MyCubemapHelpers.CalculateSampleTexcoord(ref position, out num2, out vector2);
                float altitude = useCache ? this.Shape.GetValueForPositionWithCache(num2, ref vector2, out props.Normal) : this.Shape.GetValueForPositionCacheless(num2, ref vector2, out props.Normal);
                @params.SampledHeight = altitude;
                @params.SurfaceDepth = 0f;
                @params.Texcoord = vector2;
                @params.LodSize = 1f;
                @params.Latitude = props.Latitude;
                @params.Longitude = props.Longitude;
                @params.Face = num2;
                @params.Normal = props.Normal;
                props.Position = (@params.Gravity * (this.Radius + altitude)) + this.Shape.Center();
                MyPlanetMaterialProvider.MaterialSampleParams* paramsPtr1 = (MyPlanetMaterialProvider.MaterialSampleParams*) ref @params;
                props.Gravity = paramsPtr1->Gravity = -@params.Gravity;
                @params.DistanceToCenter = props.Position.Length();
                MyPlanetMaterialProvider.PlanetMaterial layeredMaterialForPosition = this.Material.GetLayeredMaterialForPosition(ref @params, out props.Biome);
                props.Material = (layeredMaterialForPosition.FirstOrDefault != null) ? layeredMaterialForPosition.FirstOrDefault.Index : 0;
                props.Normal = @params.Normal;
                props.HeightRatio = this.Shape.AltitudeToRatio(altitude);
            }
        }

        public void ComputeCombinedMaterialAndSurfaceExtended(Vector3 position, out SurfacePropertiesExtended props)
        {
            if (this.Closed)
            {
                props = new SurfacePropertiesExtended();
            }
            else
            {
                this.Material.GetMaterialForPositionDebug(ref position, out props);
            }
        }

        public void DebugDraw(ref MatrixD worldMatrix)
        {
        }

        public MyVoxelMaterialDefinition GetMaterialAtPosition(ref Vector3D localPosition)
        {
            if (this.Closed)
            {
                return null;
            }
            Vector3 pos = (Vector3) localPosition;
            return this.Material.GetMaterialForPosition(ref pos, 1f);
        }

        private void Init(long seed)
        {
            float radius = (float) this.m_data.Radius;
            float num3 = radius + (radius * this.Generator.HillParams.Max);
            this.StorageSize = MyVoxelCoordSystems.FindBestOctreeSize(2f * num3);
            float num4 = this.StorageSize.X * 0.5f;
            MyPlanetTextureMapProvider texProvider = new MyPlanetTextureMapProvider();
            texProvider.Init(seed, this.Generator, this.Generator.MapProvider);
            this.Shape = new MyPlanetShapeProvider(new Vector3(num4), radius, this.Generator, texProvider.GetHeightmap(), texProvider);
            this.Material = new MyPlanetMaterialProvider(this.Generator, this.Shape, texProvider.GetMaps(this.Generator.PlanetMaps.ToSet()));
        }

        public void Init(long seed, MyPlanetGeneratorDefinition generator, double radius)
        {
            double num1 = Math.Max(radius, 1.0);
            radius = num1;
            this.Generator = generator;
            PlanetData data = new PlanetData {
                Radius = radius,
                Seed = seed,
                Version = STORAGE_VERSION
            };
            this.m_data = data;
            this.Init(seed);
            this.Closed = false;
        }

        public ContainmentType Intersect(BoundingBoxI box, int lod)
        {
            if (this.Closed)
            {
                return ContainmentType.Disjoint;
            }
            BoundingBox box2 = new BoundingBox(box);
            box2.Translate(-this.Shape.Center());
            return this.Shape.IntersectBoundingBox(ref box2, 1f);
        }

        public unsafe bool Intersect(ref LineD line, out double startOffset, out double endOffset)
        {
            LineD ll = line;
            Vector3 vector = this.Shape.Center();
            Vector3D* vectordPtr1 = (Vector3D*) ref ll.To;
            vectordPtr1[0] -= vector;
            Vector3D* vectordPtr2 = (Vector3D*) ref ll.From;
            vectordPtr2[0] -= vector;
            if (!this.Shape.IntersectLine(ref ll, out startOffset, out endOffset))
            {
                return false;
            }
            Vector3D* vectordPtr3 = (Vector3D*) ref ll.From;
            vectordPtr3[0] += vector;
            Vector3D* vectordPtr4 = (Vector3D*) ref ll.To;
            vectordPtr4[0] += vector;
            line = ll;
            return true;
        }

        private static uint PackColorShift(Vector3 hsv)
        {
            int num2 = MathHelper.Clamp((int) hsv.Y, -100, 100);
            int num3 = MathHelper.Clamp((int) hsv.Z, -100, 100);
            return (uint) ((((0xffff & (((int) hsv.X) % 360)) << 0x10) | ((0xff & num2) << 8)) | (0xff & num3));
        }

        public unsafe void PostProcess(VrVoxelMesh mesh, MyStorageDataTypeFlags dataTypes)
        {
            if (dataTypes.Requests(MyStorageDataTypeEnum.Material))
            {
                VrVoxelVertex* vertices = mesh.Vertices;
                int vertexCount = mesh.VertexCount;
                Vector3 start = (Vector3) mesh.Start;
                float scale = mesh.Scale;
                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3 position = (start + vertices[i].Position) * scale;
                    Vector3 vector3 = this.Material.GetColorShift(position, vertices[i].Material, 1024f);
                    if (vector3 != Vector3.Zero)
                    {
                        vertices[i].Color.PackedValue = PackColorShift(vector3 * new Vector3(360f, 100f, 100f));
                    }
                }
            }
        }

        public void ReadFrom(int storageVersion, Stream stream, int size, ref bool isOldFormat)
        {
            this.m_data.Version = stream.ReadInt64();
            this.m_data.Seed = stream.ReadInt64();
            this.m_data.Radius = stream.ReadDouble();
            string str = stream.ReadString(null);
            if (this.m_data.Version != STORAGE_VERSION)
            {
                isOldFormat = true;
            }
            MyPlanetGeneratorDefinition definition = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(str));
            if (definition == null)
            {
                throw new Exception($"Cannot load planet generator definition for subtype '{str}'.");
            }
            this.Generator = definition;
            this.Init(this.m_data.Seed);
        }

        public unsafe void ReadRange(ref MyVoxelDataRequest req, bool detectOnly = false)
        {
            if (!this.Closed)
            {
                if (req.RequestedData.Requests(MyStorageDataTypeEnum.Content))
                {
                    this.Shape.ReadContentRange(ref req, detectOnly);
                    MyVoxelRequestFlags* flagsPtr1 = (MyVoxelRequestFlags*) ref req.RequestFlags;
                    *((int*) flagsPtr1) |= 2;
                }
                if (!req.Flags.HasFlags(MyVoxelRequestFlags.EmptyData))
                {
                    if (req.RequestedData.Requests(MyStorageDataTypeEnum.Material))
                    {
                        this.Material.ReadMaterialRange(ref req, detectOnly);
                    }
                }
                else if (!detectOnly && req.RequestedData.Requests(MyStorageDataTypeEnum.Material))
                {
                    req.Target.BlockFill(MyStorageDataTypeEnum.Material, req.MinInLod, req.MaxInLod, 0xff);
                }
            }
        }

        public void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataType, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod)
        {
            if (!this.Closed)
            {
                MyVoxelDataRequest req = new MyVoxelDataRequest {
                    Target = target,
                    Offset = writeOffset,
                    RequestedData = dataType,
                    Lod = lodIndex,
                    MinInLod = minInLod,
                    MaxInLod = maxInLod
                };
                this.ReadRange(ref req, false);
            }
        }

        public void ReindexMaterials(Dictionary<byte, byte> oldToNewIndexMap)
        {
        }

        public void WriteTo(Stream stream)
        {
            stream.WriteNoAlloc(this.m_data.Version);
            stream.WriteNoAlloc(this.m_data.Seed);
            stream.WriteNoAlloc(this.m_data.Radius);
            stream.WriteNoAlloc(this.Generator.Id.SubtypeName, null);
        }

        public MyPlanetGeneratorDefinition Generator { get; private set; }

        public int SerializedSize
        {
            get
            {
                int byteCount = Encoding.UTF8.GetByteCount(this.Generator.Id.SubtypeName);
                byteCount += (MathHelper.Log2Floor(byteCount) + 6) / 7;
                return (sizeof(PlanetData) + byteCount);
            }
        }

        public bool Closed { get; private set; }

        public MyPlanetShapeProvider Shape { get; private set; }

        public MyPlanetMaterialProvider Material { get; private set; }

        public Vector3I StorageSize { get; private set; }

        public float Radius =>
            ((float) this.m_data.Radius);

        [StructLayout(LayoutKind.Sequential)]
        private struct PlanetData
        {
            public long Version;
            public long Seed;
            public double Radius;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SurfacePropertiesExtended
        {
            public Vector3 Position;
            public Vector3 Gravity;
            public MyVoxelMaterialDefinition Material;
            public float Slope;
            public float HeightRatio;
            public float Depth;
            public float GroundHeight;
            public float Latitude;
            public float Longitude;
            public float Altitude;
            public int Face;
            public Vector2 Texcoord;
            public byte BiomeValue;
            public byte MaterialValue;
            public byte OreValue;
            public MyPlanetMaterialProvider.PlanetMaterial EffectiveRule;
            public MyPlanetMaterialProvider.PlanetBiome Biome;
            public MyPlanetMaterialProvider.PlanetOre Ore;
            public MaterialOrigin Origin;
            public enum MaterialOrigin
            {
                Rule,
                Ore,
                Map,
                Default
            }
        }
    }
}


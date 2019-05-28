namespace Sandbox.Engine.Voxels
{
    using Sandbox.Definitions;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Noise;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyPlanetMaterialProvider
    {
        private int m_mapResolutionMinusOne;
        private MyPlanetShapeProvider m_planetShape;
        private Dictionary<byte, PlanetMaterial> m_materials;
        private Dictionary<byte, PlanetBiome> m_biomes;
        private Dictionary<byte, List<PlanetOre>> m_ores;
        private PlanetMaterial m_defaultMaterial;
        private PlanetMaterial m_subsurfaceMaterial;
        private MyCubemap m_materialMap;
        private MyCubemap m_biomeMap;
        private MyCubemap m_oreMap;
        private MyTileTexture<byte> m_blendingTileset;
        private MyPlanetGeneratorDefinition m_generator;
        private float m_invHeightRange;
        private float m_heightmapScale;
        private float m_biomePixelSize;
        private bool m_hasRules;
        private int m_hashCode;
        [ThreadStatic]
        private static List<PlanetMaterialRule>[] m_rangeBiomes;
        [ThreadStatic]
        private static bool m_rangeClean;
        [ThreadStatic]
        private static WeakReference<MyPlanetMaterialProvider> m_chachedProviderRef;
        private MyPerlin m_perlin = new MyPerlin(MyNoiseQuality.Standard, 6, 0x1e240, 5.0, 2.0, 0.5);
        private Vector3 m_targetShift = new Vector3(0f, -0.9f, -0.08f);
        [ThreadStatic]
        private static MapBlendCache m_materialBC;

        public MyPlanetMaterialProvider(MyPlanetGeneratorDefinition generatorDef, MyPlanetShapeProvider planetShape, MyCubemap[] maps)
        {
            this.m_materials = new Dictionary<byte, PlanetMaterial>(generatorDef.SurfaceMaterialTable.Length);
            for (int i = 0; i < generatorDef.SurfaceMaterialTable.Length; i++)
            {
                byte num2 = generatorDef.SurfaceMaterialTable[i].Value;
                this.m_materials[num2] = new PlanetMaterial(generatorDef.SurfaceMaterialTable[i], generatorDef.MinimumSurfaceLayerDepth);
            }
            this.m_defaultMaterial = new PlanetMaterial(generatorDef.DefaultSurfaceMaterial, generatorDef.MinimumSurfaceLayerDepth);
            this.m_subsurfaceMaterial = (generatorDef.DefaultSubSurfaceMaterial == null) ? this.m_defaultMaterial : new PlanetMaterial(generatorDef.DefaultSubSurfaceMaterial, generatorDef.MinimumSurfaceLayerDepth);
            this.m_planetShape = planetShape;
            this.Maps = maps;
            this.m_materialMap = maps[0];
            this.m_biomeMap = maps[1];
            this.m_oreMap = maps[2];
            if (this.m_materialMap != null)
            {
                this.m_mapResolutionMinusOne = this.m_materialMap.Resolution - 1;
            }
            this.m_generator = generatorDef;
            this.m_invHeightRange = 1f / (this.m_planetShape.MaxHillHeight - this.m_planetShape.MinHillHeight);
            this.m_biomePixelSize = ((float) ((planetShape.MaxHillHeight + planetShape.Radius) * 3.1415926535897931)) / ((this.m_mapResolutionMinusOne + 1) * 2f);
            this.m_hashCode = generatorDef.FolderName.GetHashCode();
            if ((this.m_generator.MaterialGroups != null) && (this.m_generator.MaterialGroups.Length != 0))
            {
                this.m_biomes = new Dictionary<byte, PlanetBiome>();
                foreach (MyPlanetMaterialGroup group in this.m_generator.MaterialGroups)
                {
                    this.m_biomes[group.Value] = new PlanetBiome(group, this.m_generator.MinimumSurfaceLayerDepth);
                }
            }
            if (MyHeightMapLoadingSystem.Static != null)
            {
                this.m_blendingTileset = MyHeightMapLoadingSystem.Static.GetTerrainBlendTexture(this.m_generator.MaterialBlending);
            }
            this.m_ores = new Dictionary<byte, List<PlanetOre>>();
            foreach (MyPlanetOreMapping mapping in this.m_generator.OreMappings)
            {
                MyVoxelMaterialDefinition material = GetMaterial(mapping.Type);
                if (material != null)
                {
                    PlanetOre item = new PlanetOre {
                        Depth = mapping.Depth,
                        Start = mapping.Start,
                        Value = mapping.Value,
                        Material = material,
                        ColorInfluence = mapping.ColorInfluence
                    };
                    if (mapping.ColorShift != null)
                    {
                        item.TargetColor = new Vector3?(mapping.ColorShift.Value.ColorToHSV());
                    }
                    if (!this.m_ores.ContainsKey(mapping.Value))
                    {
                        List<PlanetOre> list1 = new List<PlanetOre>();
                        list1.Add(item);
                        List<PlanetOre> list = list1;
                        this.m_ores.Add(mapping.Value, list);
                    }
                    this.m_ores[mapping.Value].Add(item);
                }
            }
            this.Closed = false;
        }

        private void CleanRules()
        {
            if (m_rangeBiomes == null)
            {
                m_rangeBiomes = new List<PlanetMaterialRule>[0x100];
            }
            foreach (PlanetBiome biome in this.m_biomes.Values)
            {
                m_rangeBiomes[biome.Value] = biome.Rules;
            }
            m_rangeClean = true;
            CachedProvider = this;
        }

        public void Close()
        {
            this.m_blendingTileset = null;
            this.m_subsurfaceMaterial = null;
            this.m_generator = null;
            this.m_biomeMap = null;
            this.m_biomes = null;
            this.m_materials = null;
            this.m_planetShape = null;
            this.m_ores = null;
            this.m_materialMap = null;
            this.m_oreMap = null;
            this.m_biomeMap = null;
            this.Maps = null;
            this.Closed = true;
        }

        private unsafe byte ComputeMapBlend(Vector2 coords, int face, ref MapBlendCache cache, MyCubemapData<byte> map)
        {
            byte num;
            coords = (coords * map.Resolution) - 0.5f;
            Vector2I vectori = new Vector2I(coords);
            if (((cache.HashCode != this.m_hashCode) || (cache.Face != face)) || (cache.Cell != vectori))
            {
                cache.HashCode = this.m_hashCode;
                cache.Cell = vectori;
                cache.Face = (byte) face;
                if (this.m_materialMap != null)
                {
                    byte num2;
                    byte num3;
                    byte num4;
                    byte num5;
                    map.GetValue(vectori.X, vectori.Y, out num2);
                    map.GetValue(vectori.X + 1, vectori.Y, out num3);
                    map.GetValue(vectori.X, vectori.Y + 1, out num4);
                    map.GetValue(vectori.X + 1, vectori.Y + 1, out num5);
                    byte* v = stackalloc byte[4];
                    v[0] = num2;
                    v[1] = num3;
                    v[2] = num4;
                    v[3] = num5;
                    if (((num2 != num3) || (num4 != num5)) || (num4 != num2))
                    {
                        Sort4(v);
                        ComputeTilePattern(num2, num3, num4, num5, v, &cache.Data.FixedElementField);
                        numRef = null;
                    }
                    else
                    {
                        IntPtr ptr1 = (IntPtr) &cache.Data.FixedElementField;
                        ptr1[0] = (IntPtr) ((ushort) ((num2 << 8) | 15));
                        ptr1[2] = IntPtr.Zero;
                        ptr1[(int) (((IntPtr) 2) * 2)] = IntPtr.Zero;
                        ptr1[(int) (((IntPtr) 3) * 2)] = IntPtr.Zero;
                        numRef = null;
                    }
                }
            }
            ushort* values = &cache.Data.FixedElementField;
            coords -= Vector2.Floor(coords);
            if (coords.X == 1f)
            {
                coords.X = 0.99999f;
            }
            if (coords.Y == 1f)
            {
                coords.Y = 0.99999f;
            }
            this.SampleTile(values, ref coords, out num);
            fixed (ushort* numRef = null)
            {
                return num;
            }
        }

        private static unsafe void ComputeTilePattern(byte tl, byte tr, byte bl, byte br, byte* ss, ushort* values)
        {
            int index = 0;
            for (int i = 0; i < 4; i++)
            {
                if ((i <= 0) || (ss[i] != ss[i - 1]))
                {
                    index++;
                    values[index] = (ushort) (((((ss[i] << 8) | ((ss[i] == tl) ? 8 : 0)) | ((ss[i] == tr) ? 4 : 0)) | ((ss[i] == bl) ? 2 : 0)) | ((ss[i] == br) ? 1 : 0));
                }
            }
            while (index < 4)
            {
                values[index] = 0;
                index++;
            }
        }

        public Vector3 GetColorShift(Vector3 position, byte material, float maxDepth = 1f)
        {
            if (maxDepth >= 1f)
            {
                int num;
                Vector2 vector2;
                List<PlanetOre> list;
                MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(material);
                if ((voxelMaterialDefinition == null) || (voxelMaterialDefinition.ColorKey == null))
                {
                    return Vector3.Zero;
                }
                MyCubemapHelpers.CalculateSampleTexcoord(ref position - this.m_planetShape.Center(), out num, out vector2);
                if (this.m_oreMap == null)
                {
                    return Vector3.Zero;
                }
                byte key = this.m_oreMap.Faces[num].GetValue(vector2.X, vector2.Y);
                if (!this.m_ores.TryGetValue(key, out list))
                {
                    return Vector3.Zero;
                }
                float num3 = (float) MathHelper.Saturate(this.m_perlin.GetValue((double) (vector2.X * 1000f), (double) (vector2.Y * 1000f), 0.0));
                using (List<PlanetOre>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        PlanetOre current = enumerator.Current;
                        float colorInfluence = current.ColorInfluence;
                        colorInfluence = 256f;
                        if ((colorInfluence >= 1f) && ((colorInfluence >= current.Start) && ((current.Start <= maxDepth) && (current.TargetColor != null))))
                        {
                            Vector3 targetShift = current.TargetColor.Value;
                            Color violet = Color.Violet;
                            if (targetShift == Vector3.Backward)
                            {
                                targetShift = new Vector3(0f, 1f, -0.08f);
                            }
                            else
                            {
                                targetShift = this.m_targetShift;
                            }
                            return (Vector3) ((num3 * targetShift) * (1f - (current.Start / colorInfluence)));
                        }
                    }
                }
            }
            return Vector3.Zero;
        }

        public PlanetMaterial GetLayeredMaterialForPosition(ref MaterialSampleParams ps, out byte biomeValue)
        {
            if (ps.DistanceToCenter < 0.01)
            {
                biomeValue = 0xff;
                return this.m_defaultMaterial;
            }
            byte key = 0;
            PlanetMaterial defaultMaterial = null;
            byte num2 = 0;
            if (this.m_biomeMap != null)
            {
                num2 = this.m_biomeMap.Faces[ps.Face].GetValue(ps.Texcoord.X, ps.Texcoord.Y);
            }
            if (this.m_biomePixelSize < ps.LodSize)
            {
                if (this.m_materialMap != null)
                {
                    key = this.m_materialMap.Faces[ps.Face].GetValue(ps.Texcoord.X, ps.Texcoord.Y);
                }
            }
            else if (this.m_materialMap != null)
            {
                key = this.ComputeMapBlend(ps.Texcoord, ps.Face, ref m_materialBC, this.m_materialMap.Faces[ps.Face]);
            }
            this.m_materials.TryGetValue(key, out defaultMaterial);
            if ((defaultMaterial == null) && (this.m_biomes != null))
            {
                List<PlanetMaterialRule> list = m_rangeBiomes[key];
                if ((list != null) && (list.Count != 0))
                {
                    float height = (ps.SampledHeight - this.m_planetShape.MinHillHeight) * this.m_invHeightRange;
                    foreach (PlanetMaterialRule rule in list)
                    {
                        if (rule.Check(height, ps.Latitude, ps.Longitude, ps.Normal.Z))
                        {
                            defaultMaterial = rule;
                            break;
                        }
                    }
                }
            }
            if (defaultMaterial == null)
            {
                defaultMaterial = this.m_defaultMaterial;
            }
            biomeValue = num2;
            return defaultMaterial;
        }

        private static MyVoxelMaterialDefinition GetMaterial(string name)
        {
            MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(name);
            if (voxelMaterialDefinition == null)
            {
                MyLog.Default.WriteLine("Could not load voxel material " + name);
            }
            return voxelMaterialDefinition;
        }

        public MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize)
        {
            byte num;
            return this.GetMaterialForPosition(ref pos, lodSize, out num, false);
        }

        public MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize, out byte biomeValue, bool preciseOrePositions)
        {
            MaterialSampleParams @params;
            biomeValue = 0;
            this.GetPositionParams(ref pos, lodSize, out @params, false);
            MyVoxelMaterialDefinition material = null;
            float num = !preciseOrePositions ? ((@params.SurfaceDepth / Math.Max((float) (lodSize * 0.5f), (float) 1f)) + 0.5f) : (@params.SurfaceDepth + 0.5f);
            if (this.m_oreMap != null)
            {
                List<PlanetOre> list;
                byte key = this.m_oreMap.Faces[@params.Face].GetValue(@params.Texcoord.X, @params.Texcoord.Y);
                if (this.m_ores.TryGetValue(key, out list))
                {
                    using (List<PlanetOre>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            PlanetOre current = enumerator.Current;
                            if ((current.Start <= -num) && ((current.Start + current.Depth) >= -num))
                            {
                                return current.Material;
                            }
                        }
                    }
                }
            }
            PlanetMaterial layeredMaterialForPosition = this.GetLayeredMaterialForPosition(ref @params, out biomeValue);
            float num2 = @params.SurfaceDepth / lodSize;
            if (!layeredMaterialForPosition.HasLayers)
            {
                if (num2 >= -layeredMaterialForPosition.Depth)
                {
                    material = layeredMaterialForPosition.Material;
                }
            }
            else
            {
                VoxelMaterial[] layers = layeredMaterialForPosition.Layers;
                for (int i = 0; i < layers.Length; i++)
                {
                    if (num2 >= -layers[i].Depth)
                    {
                        material = layeredMaterialForPosition.Layers[i].Material;
                        break;
                    }
                }
            }
            if (material == null)
            {
                material = this.m_subsurfaceMaterial.FirstOrDefault;
            }
            return material;
        }

        public void GetMaterialForPositionDebug(ref Vector3 pos, out MyPlanetStorageProvider.SurfacePropertiesExtended props)
        {
            MaterialSampleParams @params;
            this.GetPositionParams(ref pos, 1f, out @params, true);
            props.Position = pos;
            props.Gravity = -@params.Gravity;
            props.Material = this.m_defaultMaterial.FirstOrDefault;
            props.Slope = @params.Normal.Z;
            props.HeightRatio = this.m_planetShape.AltitudeToRatio(@params.SampledHeight);
            props.Depth = @params.SurfaceDepth;
            props.Latitude = @params.Latitude;
            props.Longitude = @params.Longitude;
            props.Altitude = @params.DistanceToCenter - this.m_planetShape.Radius;
            props.GroundHeight = @params.SampledHeight + this.m_planetShape.Radius;
            props.Face = @params.Face;
            props.Texcoord = @params.Texcoord;
            props.BiomeValue = 0;
            props.MaterialValue = 0;
            props.OreValue = 0;
            props.EffectiveRule = null;
            props.Biome = null;
            props.Ore = new PlanetOre();
            props.Origin = MyPlanetStorageProvider.SurfacePropertiesExtended.MaterialOrigin.Default;
            PlanetMaterial defaultMaterial = null;
            if (this.m_oreMap != null)
            {
                List<PlanetOre> list;
                props.OreValue = this.m_oreMap.Faces[@params.Face].GetValue(@params.Texcoord.X, @params.Texcoord.Y);
                if (this.m_ores.TryGetValue(props.OreValue, out list))
                {
                    foreach (PlanetOre ore in list)
                    {
                        props.Ore = ore;
                        if ((ore.Start <= -@params.SurfaceDepth) && ((ore.Start + ore.Depth) >= -@params.SurfaceDepth))
                        {
                            props.Material = ore.Material;
                            props.Origin = MyPlanetStorageProvider.SurfacePropertiesExtended.MaterialOrigin.Ore;
                            break;
                        }
                    }
                }
            }
            if (@params.DistanceToCenter >= 0.01)
            {
                byte key = 0;
                if (this.m_biomePixelSize < @params.LodSize)
                {
                    if (this.m_materialMap != null)
                    {
                        key = this.m_materialMap.Faces[@params.Face].GetValue(@params.Texcoord.X, @params.Texcoord.Y);
                    }
                }
                else if (this.m_materialMap != null)
                {
                    key = this.ComputeMapBlend(@params.Texcoord, @params.Face, ref m_materialBC, this.m_materialMap.Faces[@params.Face]);
                }
                this.m_materials.TryGetValue(key, out defaultMaterial);
                props.Origin = MyPlanetStorageProvider.SurfacePropertiesExtended.MaterialOrigin.Map;
                props.MaterialValue = key;
                if ((defaultMaterial == null) && (this.m_biomes != null))
                {
                    PlanetBiome biome;
                    this.m_biomes.TryGetValue(key, out biome);
                    props.Biome = biome;
                    if ((biome != null) && biome.IsValid)
                    {
                        foreach (PlanetMaterialRule rule in biome.Rules)
                        {
                            if (rule.Check(props.HeightRatio, @params.Latitude, @params.Longitude, @params.Normal.Z))
                            {
                                defaultMaterial = rule;
                                props.Origin = MyPlanetStorageProvider.SurfacePropertiesExtended.MaterialOrigin.Rule;
                                break;
                            }
                        }
                    }
                }
                if (defaultMaterial == null)
                {
                    defaultMaterial = this.m_defaultMaterial;
                    props.Origin = MyPlanetStorageProvider.SurfacePropertiesExtended.MaterialOrigin.Default;
                }
                byte num2 = 0;
                if (this.m_biomeMap != null)
                {
                    num2 = this.m_biomeMap.Faces[@params.Face].GetValue(@params.Texcoord.X, @params.Texcoord.Y);
                }
                props.BiomeValue = num2;
                float num3 = @params.SurfaceDepth + 0.5f;
                if (!defaultMaterial.HasLayers)
                {
                    if (num3 >= -defaultMaterial.Depth)
                    {
                        props.Material = defaultMaterial.Material;
                    }
                }
                else
                {
                    VoxelMaterial[] layers = defaultMaterial.Layers;
                    for (int i = 0; i < layers.Length; i++)
                    {
                        if (num3 >= -layers[i].Depth)
                        {
                            props.Material = defaultMaterial.Layers[i].Material;
                            break;
                        }
                    }
                }
                props.EffectiveRule = defaultMaterial;
            }
        }

        public void GetPositionParams(ref Vector3 pos, float lodSize, out MaterialSampleParams ps, bool skipCache = false)
        {
            Vector3 localPos = pos - this.m_planetShape.Center();
            ps.DistanceToCenter = localPos.Length();
            ps.LodSize = lodSize;
            if (ps.DistanceToCenter < 0.01f)
            {
                ps.SurfaceDepth = 0f;
                ps.Gravity = Vector3.Down;
                ps.Latitude = 0f;
                ps.Longitude = 0f;
                ps.Texcoord = Vector2.One / 2f;
                ps.Face = 0;
                ps.Normal = Vector3.Backward;
                ps.SampledHeight = 0f;
            }
            else
            {
                ps.Gravity = localPos / ps.DistanceToCenter;
                MyCubemapHelpers.CalculateSampleTexcoord(ref localPos, out ps.Face, out ps.Texcoord);
                ps.SampledHeight = !skipCache ? this.m_planetShape.GetValueForPositionWithCache(ps.Face, ref ps.Texcoord, out ps.Normal) : this.m_planetShape.GetValueForPositionCacheless(ps.Face, ref ps.Texcoord, out ps.Normal);
                ps.SurfaceDepth = this.m_planetShape.SignedDistanceWithSample(lodSize, ps.DistanceToCenter, ps.SampledHeight) * ps.Normal.Z;
                ps.Latitude = ps.Gravity.Y;
                Vector2 vector2 = new Vector2(-ps.Gravity.X, -ps.Gravity.Z);
                vector2.Normalize();
                ps.Longitude = vector2.Y;
                if (-ps.Gravity.X > 0f)
                {
                    ps.Longitude = 2f - ps.Longitude;
                }
            }
        }

        private unsafe void GetRuleBounds(ref BoundingBox request, out BoundingBox ruleBounds)
        {
            float y;
            Vector3 vector2;
            Vector3* corners = (Vector3*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3))];
            ruleBounds.Min = new Vector3(float.PositiveInfinity);
            ruleBounds.Max = new Vector3(float.NegativeInfinity);
            request.GetCornersUnsafe(corners);
            if (Vector3.Zero.IsInsideInclusive(ref request.Min, ref request.Max))
            {
                ruleBounds.Min.X = 0f;
            }
            else
            {
                ruleBounds.Min.X = this.m_planetShape.DistanceToRatio(Vector3.Clamp(Vector3.Zero, request.Min, request.Max).Length());
            }
            Vector3 center = request.Center;
            vector2.X = (center.X >= 0f) ? request.Max.X : request.Min.X;
            Vector3 local1 = center;
            vector2.Y = (local1.Y >= 0f) ? request.Max.Y : request.Min.Y;
            vector2.Z = (local1.Z >= 0f) ? request.Max.Z : request.Min.Z;
            ruleBounds.Max.X = this.m_planetShape.DistanceToRatio(vector2.Length());
            if (((request.Min.X < 0f) && ((request.Min.Z < 0f) && (request.Max.X > 0f))) && (request.Max.Z > 0f))
            {
                ruleBounds.Min.Z = -1f;
                ruleBounds.Max.Z = 3f;
                for (int i = 0; i < 8; i++)
                {
                    float num4 = (corners + i).Length();
                    y = corners[i].Y / num4;
                    if (ruleBounds.Min.Y > y)
                    {
                        ruleBounds.Min.Y = y;
                    }
                    if (ruleBounds.Max.Y < y)
                    {
                        ruleBounds.Max.Y = y;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    float num6 = (corners + i).Length();
                    Vector3* vectorPtr1 = corners + i;
                    vectorPtr1[0] /= num6;
                    y = corners[i].Y;
                    Vector2 vector3 = new Vector2(-corners[i].X, -corners[i].Z);
                    vector3.Normalize();
                    float num2 = vector3.Y;
                    if (vector3.X > 0f)
                    {
                        num2 = 2f - num2;
                    }
                    if (ruleBounds.Min.Y > y)
                    {
                        ruleBounds.Min.Y = y;
                    }
                    if (ruleBounds.Max.Y < y)
                    {
                        ruleBounds.Max.Y = y;
                    }
                    if (ruleBounds.Min.Z > num2)
                    {
                        ruleBounds.Min.Z = num2;
                    }
                    if (ruleBounds.Max.Z < num2)
                    {
                        ruleBounds.Max.Z = num2;
                    }
                }
            }
        }

        public void PrepareRulesForBox(ref BoundingBox request)
        {
            if (this.m_biomes != null)
            {
                if (request.Extents.Sum > 50f)
                {
                    this.PrepareRulesForBoxInternal(ref request);
                }
                else
                {
                    this.CleanRules();
                }
            }
        }

        private void PrepareRulesForBoxInternal(ref BoundingBox request)
        {
            BoundingBox box;
            if (m_rangeBiomes == null)
            {
                m_rangeBiomes = new List<PlanetMaterialRule>[0x100];
            }
            request.Translate(-this.m_planetShape.Center());
            request.Inflate((float) (request.Extents.Length() * 0.1f));
            this.GetRuleBounds(ref request, out box);
            bool flag = !ReferenceEquals(CachedProvider, this);
            foreach (PlanetBiome biome in this.m_biomes.Values)
            {
                if ((ReferenceEquals(m_rangeBiomes[biome.Value], biome.Rules) || ReferenceEquals(m_rangeBiomes[biome.Value], null)) | flag)
                {
                    m_rangeBiomes[biome.Value] = new List<PlanetMaterialRule>();
                }
                biome.MateriaTree.OverlapAllBoundingBox<PlanetMaterialRule>(ref box, m_rangeBiomes[biome.Value], 0, true);
                m_rangeBiomes[biome.Value].Sort();
            }
            m_rangeClean = false;
            CachedProvider = this;
        }

        public unsafe void ReadMaterialRange(ref MyVoxelDataRequest req, bool detectOnly = false)
        {
            Vector3I vectori4;
            req.Flags = req.RequestFlags & MyVoxelRequestFlags.RequestFlags;
            Vector3I minInLod = req.MinInLod;
            Vector3I maxInLod = req.MaxInLod;
            float lodSize = 1 << (req.Lod & 0x1f);
            bool flag = req.RequestFlags.HasFlags(MyVoxelRequestFlags.SurfaceMaterial);
            bool flag2 = req.RequestFlags.HasFlags(MyVoxelRequestFlags.ConsiderContent);
            bool preciseOrePositions = req.RequestFlags.HasFlags(MyVoxelRequestFlags.PreciseOrePositions);
            this.m_planetShape.PrepareCache();
            if (this.m_biomes != null)
            {
                if (req.SizeLinear > 0x7d)
                {
                    BoundingBox request = new BoundingBox((Vector3) (minInLod * lodSize), (Vector3) (maxInLod * lodSize));
                    this.PrepareRulesForBoxInternal(ref request);
                }
                else if (!m_rangeClean || !ReferenceEquals(CachedProvider, this))
                {
                    this.CleanRules();
                }
            }
            Vector3 vector = (minInLod + 0.5f) * lodSize;
            Vector3 pos = vector;
            Vector3I vectori3 = (Vector3I) (-minInLod + req.Offset);
            if (detectOnly)
            {
                vectori4.Z = minInLod.Z;
                while (vectori4.Z <= maxInLod.Z)
                {
                    vectori4.Y = minInLod.Y;
                    while (true)
                    {
                        if (vectori4.Y > maxInLod.Y)
                        {
                            float* singlePtr3 = (float*) ref pos.Z;
                            singlePtr3[0] += lodSize;
                            pos.Y = vector.Y;
                            int* numPtr3 = (int*) ref vectori4.Z;
                            numPtr3[0]++;
                            break;
                        }
                        vectori4.X = minInLod.X;
                        while (true)
                        {
                            byte num2;
                            if (vectori4.X > maxInLod.X)
                            {
                                float* singlePtr2 = (float*) ref pos.Y;
                                singlePtr2[0] += lodSize;
                                pos.X = vector.X;
                                int* numPtr2 = (int*) ref vectori4.Y;
                                numPtr2[0]++;
                                break;
                            }
                            MyVoxelMaterialDefinition definition = this.GetMaterialForPosition(ref pos, lodSize, out num2, preciseOrePositions);
                            if ((definition != null) && (definition.Index != 0xff))
                            {
                                return;
                            }
                            float* singlePtr1 = (float*) ref pos.X;
                            singlePtr1[0] += lodSize;
                            int* numPtr1 = (int*) ref vectori4.X;
                            numPtr1[0]++;
                        }
                    }
                }
                MyVoxelRequestFlags* flagsPtr1 = (MyVoxelRequestFlags*) ref req.Flags;
                *((int*) flagsPtr1) |= 8;
            }
            else
            {
                bool flag4 = true;
                MyStorageData target = req.Target;
                vectori4.Z = minInLod.Z;
                while (true)
                {
                    while (true)
                    {
                        if (vectori4.Z <= maxInLod.Z)
                        {
                            vectori4.Y = minInLod.Y;
                            break;
                        }
                        if (flag4)
                        {
                            MyVoxelRequestFlags* flagsPtr2 = (MyVoxelRequestFlags*) ref req.Flags;
                            *((int*) flagsPtr2) |= 8;
                        }
                        return;
                    }
                    while (true)
                    {
                        if (vectori4.Y > maxInLod.Y)
                        {
                            float* singlePtr6 = (float*) ref pos.Z;
                            singlePtr6[0] += lodSize;
                            pos.Y = vector.Y;
                            int* numPtr6 = (int*) ref vectori4.Z;
                            numPtr6[0]++;
                            break;
                        }
                        vectori4.X = minInLod.X;
                        Vector3I p = (Vector3I) (vectori4 + vectori3);
                        int linearIdx = target.ComputeLinear(ref p);
                        while (true)
                        {
                            if (vectori4.X <= maxInLod.X)
                            {
                                byte num4;
                                if ((!flag || (target.Material(linearIdx) == 0)) && (!flag2 || (target.Content(linearIdx) != 0)))
                                {
                                    byte num5;
                                    MyVoxelMaterialDefinition definition2 = this.GetMaterialForPosition(ref pos, lodSize, out num5, preciseOrePositions);
                                    num4 = (definition2 != null) ? definition2.Index : ((byte) 0xff);
                                }
                                else
                                {
                                    num4 = 0xff;
                                }
                                target.Material(linearIdx, num4);
                                flag4 &= num4 == 0xff;
                                linearIdx += target.StepLinear;
                                float* singlePtr4 = (float*) ref pos.X;
                                singlePtr4[0] += lodSize;
                                int* numPtr4 = (int*) ref vectori4.X;
                                numPtr4[0]++;
                                continue;
                            }
                            else
                            {
                                float* singlePtr5 = (float*) ref pos.Y;
                                singlePtr5[0] += lodSize;
                                pos.X = vector.X;
                                int* numPtr5 = (int*) ref vectori4.Y;
                                numPtr5[0]++;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private unsafe void SampleTile(ushort* values, ref Vector2 coords, out byte computed)
        {
            byte num = 0;
            int index = 0;
            while (true)
            {
                if (index < 4)
                {
                    byte num3 = (byte) (values[index] >> 8);
                    if (values[index] != 0)
                    {
                        byte num5;
                        int corners = values[index] & 15;
                        this.m_blendingTileset.GetValue(corners, coords, out num5);
                        num = num3;
                        if (num5 != 0)
                        {
                            index++;
                            continue;
                        }
                    }
                }
                computed = num;
                return;
            }
        }

        private static unsafe void Sort4(byte* v)
        {
            byte num;
            if (v[0] > v[1])
            {
                num = v[1];
                v[1] = v[0];
                v[0] = num;
            }
            if (v[2] > v[3])
            {
                num = v[2];
                v[2] = v[3];
                v[3] = num;
            }
            if (v[0] > v[3])
            {
                num = v[3];
                v[3] = v[0];
                v[3] = num;
            }
            if (v[1] > v[2])
            {
                num = v[1];
                v[1] = v[2];
                v[2] = num;
            }
            if (v[0] > v[1])
            {
                num = v[1];
                v[1] = v[0];
                v[0] = num;
            }
            if (v[2] > v[3])
            {
                num = v[2];
                v[2] = v[3];
                v[3] = num;
            }
        }

        public MyCubemap[] Maps { get; private set; }

        private static MyPlanetMaterialProvider CachedProvider
        {
            get
            {
                MyPlanetMaterialProvider provider;
                if ((m_chachedProviderRef == null) || !m_chachedProviderRef.TryGetTarget(out provider))
                {
                    return null;
                }
                return provider;
            }
            set
            {
                if ((value != null) || (m_chachedProviderRef != null))
                {
                    if (m_chachedProviderRef != null)
                    {
                        m_chachedProviderRef.SetTarget(value);
                    }
                    else
                    {
                        m_chachedProviderRef = new WeakReference<MyPlanetMaterialProvider>(value);
                    }
                }
            }
        }

        public bool Closed { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MapBlendCache
        {
            public Vector2I Cell;
            [FixedBuffer(typeof(ushort), 4)]
            public <Data>e__FixedBuffer Data;
            public byte Face;
            public int HashCode;
            [StructLayout(LayoutKind.Sequential, Size=8), CompilerGenerated, UnsafeValueType]
            public struct <Data>e__FixedBuffer
            {
                public ushort FixedElementField;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialSampleParams
        {
            public Vector3 Gravity;
            public Vector3 Normal;
            public float DistanceToCenter;
            public float SampledHeight;
            public int Face;
            public Vector2 Texcoord;
            public float SurfaceDepth;
            public float LodSize;
            public float Latitude;
            public float Longitude;
        }

        public class PlanetBiome
        {
            public MyDynamicAABBTree MateriaTree;
            public byte Value;
            public string Name;
            public List<MyPlanetMaterialProvider.PlanetMaterialRule> Rules;

            public PlanetBiome(MyPlanetMaterialGroup group, float minimumSurfaceLayerDepth)
            {
                this.Value = group.Value;
                this.Name = group.Name;
                this.Rules = new List<MyPlanetMaterialProvider.PlanetMaterialRule>(group.MaterialRules.Length);
                for (int i = 0; i < group.MaterialRules.Length; i++)
                {
                    this.Rules.Add(new MyPlanetMaterialProvider.PlanetMaterialRule(group.MaterialRules[i], i, minimumSurfaceLayerDepth));
                }
                this.MateriaTree = new MyDynamicAABBTree(Vector3.Zero, 1f);
                foreach (MyPlanetMaterialProvider.PlanetMaterialRule rule in this.Rules)
                {
                    BoundingBox aabb = new BoundingBox(new Vector3(rule.Height.Min, rule.Latitude.Min, rule.Longitude.Min), new Vector3(rule.Height.Max, rule.Latitude.Max, rule.Longitude.Max));
                    this.MateriaTree.AddProxy(ref aabb, rule, 0, true);
                    if (rule.Latitude.Mirror)
                    {
                        float num2 = -aabb.Max.Y;
                        aabb.Max.Y = -aabb.Min.Y;
                        aabb.Min.Y = num2;
                        this.MateriaTree.AddProxy(ref aabb, rule, 0, true);
                    }
                }
            }

            public bool IsValid =>
                (this.Rules.Count > 0);
        }

        public class PlanetMaterial : MyPlanetMaterialProvider.VoxelMaterial
        {
            public MyPlanetMaterialProvider.VoxelMaterial[] Layers;

            public PlanetMaterial(MyPlanetMaterialDefinition def, float minimumSurfaceLayerDepth)
            {
                base.Depth = def.MaxDepth;
                if (def.Material != null)
                {
                    base.Material = MyPlanetMaterialProvider.GetMaterial(def.Material);
                }
                base.Value = def.Value;
                if (def.HasLayers)
                {
                    int length = def.Layers.Length;
                    if ((def.Layers[0].Depth < minimumSurfaceLayerDepth) && (MyPlanetMaterialProvider.GetMaterial(def.Layers[0].Material).RenderParams.Foliage != null))
                    {
                        length++;
                    }
                    this.Layers = new MyPlanetMaterialProvider.VoxelMaterial[length];
                    int index = 0;
                    for (int i = 0; index < def.Layers.Length; i++)
                    {
                        MyPlanetMaterialProvider.VoxelMaterial material1 = new MyPlanetMaterialProvider.VoxelMaterial();
                        material1.Material = MyPlanetMaterialProvider.GetMaterial(def.Layers[index].Material);
                        material1.Depth = def.Layers[index].Depth;
                        this.Layers[i] = material1;
                        if ((i == 0) && (def.Layers[index].Depth < minimumSurfaceLayerDepth))
                        {
                            if ((minimumSurfaceLayerDepth <= 1f) || (this.Layers[i].Material.RenderParams.Foliage == null))
                            {
                                this.Layers[i].Depth = minimumSurfaceLayerDepth;
                            }
                            else
                            {
                                MyVoxelMaterialDefinition definition = string.IsNullOrEmpty(this.Layers[i].Material.BareVariant) ? this.Layers[i].Material : MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.Layers[i].Material.BareVariant);
                                this.Layers[i].Depth = 1f;
                                i++;
                                MyPlanetMaterialProvider.VoxelMaterial material2 = new MyPlanetMaterialProvider.VoxelMaterial();
                                material2.Material = definition;
                                material2.Depth = minimumSurfaceLayerDepth - 1f;
                                this.Layers[i] = material2;
                            }
                        }
                        index++;
                    }
                }
            }

            private string FormatLayers(int padding)
            {
                StringBuilder builder = new StringBuilder();
                string str = new string(' ', padding);
                builder.Append('[');
                if (this.Layers.Length != 0)
                {
                    builder.Append('\n');
                    for (int i = 0; i < this.Layers.Length; i++)
                    {
                        builder.Append(str);
                        builder.Append("\t\t");
                        builder.Append(this.Layers[i]);
                        builder.Append('\n');
                    }
                }
                builder.Append(str);
                builder.Append(']');
                return builder.ToString();
            }

            public override string ToString() => 
                this.ToString(0);

            public string ToString(int padding) => 
                (!this.HasLayers ? ("SimpleMaterial" + base.ToString()) : $"LayeredMaterial({this.FormatLayers(padding)})");

            public bool HasLayers =>
                ((this.Layers != null) && (this.Layers.Length != 0));

            public MyVoxelMaterialDefinition FirstOrDefault =>
                (this.HasLayers ? this.Layers[0].Material : base.Material);
        }

        public class PlanetMaterialRule : MyPlanetMaterialProvider.PlanetMaterial, IComparable<MyPlanetMaterialProvider.PlanetMaterialRule>
        {
            public SerializableRange Height;
            public SymmetricSerializableRange Latitude;
            public SerializableRange Longitude;
            public SerializableRange Slope;
            public int Index;

            public PlanetMaterialRule(MyPlanetMaterialPlacementRule def, int index, float minimumSurfaceLayerDepth) : base(def, minimumSurfaceLayerDepth)
            {
                this.Height = def.Height;
                this.Latitude = def.Latitude;
                this.Longitude = def.Longitude;
                this.Slope = def.Slope;
                this.Index = index;
            }

            public bool Check(float height, float latitude, float longitude, float slope) => 
                (this.Height.ValueBetween(height) && (this.Latitude.ValueBetween(latitude) && (this.Longitude.ValueBetween(longitude) && this.Slope.ValueBetween(slope))));

            public int CompareTo(MyPlanetMaterialProvider.PlanetMaterialRule other) => 
                (!ReferenceEquals(this, other) ? ((other != null) ? this.Index.CompareTo(other.Index) : 1) : 0);

            public override string ToString() => 
                $"MaterialRule(
	Height: {this.Height.ToString()};
	Slope: {this.Slope.ToStringAcos()};
	Latitude: {this.Latitude.ToStringAsin()};
	Longitude: {this.Longitude.ToStringLongitude()};
	Materials: {base.ToString(4)})";

            public override bool IsRule =>
                true;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PlanetOre
        {
            public byte Value;
            public float Depth;
            public float Start;
            public float ColorInfluence;
            public MyVoxelMaterialDefinition Material;
            public Vector3? TargetColor;
            public override string ToString()
            {
                if (this.Material == null)
                {
                    return "";
                }
                return $"{this.Material.Id.SubtypeName}({this.Start}:{this.Depth}; {this.Value})";
            }
        }

        public class VoxelMaterial
        {
            public MyVoxelMaterialDefinition Material;
            public float Depth;
            public byte Value;

            public override string ToString() => 
                ((this.Material == null) ? "null" : $"({this.Material.Id.SubtypeName}:{this.Depth})");

            public virtual bool IsRule =>
                false;
        }
    }
}


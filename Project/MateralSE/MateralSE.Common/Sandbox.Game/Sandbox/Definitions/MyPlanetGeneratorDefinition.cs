namespace Sandbox.Definitions
{
    using Sandbox.Game.WorldEnvironment.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders.Definitions.Components;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_PlanetGeneratorDefinition), typeof(MyPlanetGeneratorDefinition.Postprocessor))]
    public class MyPlanetGeneratorDefinition : MyDefinitionBase
    {
        public MyDefinitionId? EnvironmentId;
        public MyWorldEnvironmentDefinition EnvironmentDefinition;
        private MyObjectBuilder_PlanetGeneratorDefinition m_pgob;
        public bool HasAtmosphere;
        public List<MyCloudLayerSettings> CloudLayers;
        public MyPlanetMaps PlanetMaps;
        public SerializableRange HillParams;
        public SerializableRange MaterialsMaxDepth;
        public SerializableRange MaterialsMinDepth;
        public MyPlanetOreMapping[] OreMappings = new MyPlanetOreMapping[0];
        public float GravityFalloffPower = 7f;
        public MyObjectBuilder_PlanetMapProvider MapProvider;
        public MyAtmosphereColorShift HostileAtmosphereColorShift = new MyAtmosphereColorShift();
        public MyPlanetMaterialDefinition[] SurfaceMaterialTable = new MyPlanetMaterialDefinition[0];
        public MyPlanetDistortionDefinition[] DistortionTable = new MyPlanetDistortionDefinition[0];
        public MyPlanetMaterialDefinition DefaultSurfaceMaterial;
        public MyPlanetMaterialDefinition DefaultSubSurfaceMaterial;
        public MyPlanetEnvironmentalSoundRule[] SoundRules;
        public List<MyMusicCategory> MusicCategories;
        public MyPlanetMaterialGroup[] MaterialGroups = new MyPlanetMaterialGroup[0];
        public Dictionary<int, Dictionary<string, List<MyPlanetEnvironmentMapping>>> MaterialEnvironmentMappings = new Dictionary<int, Dictionary<string, List<MyPlanetEnvironmentMapping>>>();
        public float SurfaceGravity = 1f;
        public float AtmosphereHeight;
        public float SectorDensity = 0.0017f;
        public MyPlanetAtmosphere Atmosphere = new MyPlanetAtmosphere();
        public MyAtmosphereSettings? AtmosphereSettings;
        public MyPlanetMaterialBlendSettings MaterialBlending;
        public string FolderName;
        public MyPlanetSurfaceDetail Detail;
        public MyPlanetAnimalSpawnInfo AnimalSpawnInfo;
        public MyPlanetAnimalSpawnInfo NightAnimalSpawnInfo;
        public Type EnvironmentSectorType;
        public MyObjectBuilder_VoxelMesherComponentDefinition MesherPostprocessing;
        public float MinimumSurfaceLayerDepth;

        public MyPlanetGeneratorDefinition()
        {
            MyPlanetMaterialBlendSettings settings = new MyPlanetMaterialBlendSettings {
                Texture = "Data/PlanetDataFiles/Extra/material_blend_grass",
                CellSize = 0x40
            };
            this.MaterialBlending = settings;
        }

        private void InheritFrom(string generator)
        {
            MyPlanetGeneratorDefinition definition = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(generator));
            if (definition == null)
            {
                MyDefinitionManager.Static.LoadingSet.m_planetGeneratorDefinitions.TryGetValue(new MyDefinitionId(typeof(MyObjectBuilder_PlanetGeneratorDefinition), generator), out definition);
            }
            if (definition == null)
            {
                MyLog.Default.WriteLine($"Could not find planet generator definition for '{generator}'.");
            }
            else
            {
                this.PlanetMaps = definition.PlanetMaps;
                this.HasAtmosphere = definition.HasAtmosphere;
                this.Atmosphere = definition.Atmosphere;
                this.CloudLayers = definition.CloudLayers;
                this.SoundRules = definition.SoundRules;
                this.MusicCategories = definition.MusicCategories;
                this.HillParams = definition.HillParams;
                this.MaterialsMaxDepth = definition.MaterialsMaxDepth;
                this.MaterialsMinDepth = definition.MaterialsMinDepth;
                this.GravityFalloffPower = definition.GravityFalloffPower;
                this.HostileAtmosphereColorShift = definition.HostileAtmosphereColorShift;
                this.SurfaceMaterialTable = definition.SurfaceMaterialTable;
                this.DistortionTable = definition.DistortionTable;
                this.DefaultSurfaceMaterial = definition.DefaultSurfaceMaterial;
                this.DefaultSubSurfaceMaterial = definition.DefaultSubSurfaceMaterial;
                this.MaterialGroups = definition.MaterialGroups;
                this.MaterialEnvironmentMappings = definition.MaterialEnvironmentMappings;
                this.SurfaceGravity = definition.SurfaceGravity;
                this.AtmosphereSettings = definition.AtmosphereSettings;
                this.FolderName = definition.FolderName;
                this.MaterialBlending = definition.MaterialBlending;
                this.OreMappings = definition.OreMappings;
                this.AnimalSpawnInfo = definition.AnimalSpawnInfo;
                this.NightAnimalSpawnInfo = definition.NightAnimalSpawnInfo;
                this.Detail = definition.Detail;
                this.SectorDensity = definition.SectorDensity;
                this.EnvironmentSectorType = definition.EnvironmentSectorType;
                this.MesherPostprocessing = definition.MesherPostprocessing;
                this.MinimumSurfaceLayerDepth = definition.MinimumSurfaceLayerDepth;
            }
        }

        protected override unsafe void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PlanetGeneratorDefinition definition = builder as MyObjectBuilder_PlanetGeneratorDefinition;
            if ((definition.InheritFrom != null) && (definition.InheritFrom.Length > 0))
            {
                this.InheritFrom(definition.InheritFrom);
            }
            if (definition.Environment != null)
            {
                this.EnvironmentId = new MyDefinitionId?(definition.Environment.Value);
            }
            else
            {
                this.m_pgob = definition;
            }
            if (definition.PlanetMaps != null)
            {
                this.PlanetMaps = definition.PlanetMaps.Value;
            }
            if (definition.HasAtmosphere != null)
            {
                this.HasAtmosphere = definition.HasAtmosphere.Value;
            }
            if (definition.CloudLayers != null)
            {
                this.CloudLayers = definition.CloudLayers;
            }
            if (definition.SoundRules != null)
            {
                this.SoundRules = new MyPlanetEnvironmentalSoundRule[definition.SoundRules.Length];
                for (int i = 0; i < definition.SoundRules.Length; i++)
                {
                    MyPlanetEnvironmentalSoundRule rule = new MyPlanetEnvironmentalSoundRule {
                        Latitude = definition.SoundRules[i].Latitude,
                        Height = definition.SoundRules[i].Height,
                        SunAngleFromZenith = definition.SoundRules[i].SunAngleFromZenith,
                        EnvironmentSound = MyStringHash.GetOrCompute(definition.SoundRules[i].EnvironmentSound)
                    };
                    rule.Latitude.ConvertToSine();
                    rule.SunAngleFromZenith.ConvertToCosine();
                    this.SoundRules[i] = rule;
                }
            }
            if (definition.MusicCategories != null)
            {
                this.MusicCategories = definition.MusicCategories;
            }
            if (definition.HillParams != null)
            {
                this.HillParams = definition.HillParams.Value;
            }
            if (definition.Atmosphere != null)
            {
                this.Atmosphere = definition.Atmosphere;
            }
            if (definition.GravityFalloffPower != null)
            {
                this.GravityFalloffPower = definition.GravityFalloffPower.Value;
            }
            if (definition.HostileAtmosphereColorShift != null)
            {
                this.HostileAtmosphereColorShift = definition.HostileAtmosphereColorShift;
            }
            if (definition.MaterialsMaxDepth != null)
            {
                this.MaterialsMaxDepth = definition.MaterialsMaxDepth.Value;
            }
            if (definition.MaterialsMinDepth != null)
            {
                this.MaterialsMinDepth = definition.MaterialsMinDepth.Value;
            }
            if ((definition.CustomMaterialTable != null) && (definition.CustomMaterialTable.Length != 0))
            {
                this.SurfaceMaterialTable = new MyPlanetMaterialDefinition[definition.CustomMaterialTable.Length];
                for (int i = 0; i < this.SurfaceMaterialTable.Length; i++)
                {
                    this.SurfaceMaterialTable[i] = definition.CustomMaterialTable[i].Clone() as MyPlanetMaterialDefinition;
                    if ((this.SurfaceMaterialTable[i].Material == null) && !this.SurfaceMaterialTable[i].HasLayers)
                    {
                        MyLog.Default.WriteLine("Custom material does not contain any material ids.");
                    }
                    else if (this.SurfaceMaterialTable[i].HasLayers)
                    {
                        float depth = this.SurfaceMaterialTable[i].Layers[0].Depth;
                        for (int j = 1; j < this.SurfaceMaterialTable[i].Layers.Length; j++)
                        {
                            float* singlePtr1 = (float*) ref this.SurfaceMaterialTable[i].Layers[j].Depth;
                            singlePtr1[0] += depth;
                            depth = this.SurfaceMaterialTable[i].Layers[j].Depth;
                        }
                    }
                }
            }
            if ((definition.DistortionTable != null) && (definition.DistortionTable.Length != 0))
            {
                this.DistortionTable = definition.DistortionTable;
            }
            if (definition.DefaultSurfaceMaterial != null)
            {
                this.DefaultSurfaceMaterial = definition.DefaultSurfaceMaterial;
            }
            if (definition.DefaultSubSurfaceMaterial != null)
            {
                this.DefaultSubSurfaceMaterial = definition.DefaultSubSurfaceMaterial;
            }
            if (definition.SurfaceGravity != null)
            {
                this.SurfaceGravity = definition.SurfaceGravity.Value;
            }
            if (definition.AtmosphereSettings != null)
            {
                this.AtmosphereSettings = definition.AtmosphereSettings;
            }
            this.FolderName = (definition.FolderName != null) ? definition.FolderName : definition.Id.SubtypeName;
            if ((definition.ComplexMaterials != null) && (definition.ComplexMaterials.Length != 0))
            {
                this.MaterialGroups = new MyPlanetMaterialGroup[definition.ComplexMaterials.Length];
                int index = 0;
                while (index < definition.ComplexMaterials.Length)
                {
                    this.MaterialGroups[index] = definition.ComplexMaterials[index].Clone() as MyPlanetMaterialGroup;
                    MyPlanetMaterialGroup group = this.MaterialGroups[index];
                    MyPlanetMaterialPlacementRule[] materialRules = group.MaterialRules;
                    List<int> indices = new List<int>();
                    int num6 = 0;
                    while (true)
                    {
                        if (num6 >= materialRules.Length)
                        {
                            if (indices.Count > 0)
                            {
                                materialRules = materialRules.RemoveIndices<MyPlanetMaterialPlacementRule>(indices);
                            }
                            group.MaterialRules = materialRules;
                            index++;
                            break;
                        }
                        if ((materialRules[num6].Material == null) && ((materialRules[num6].Layers == null) || (materialRules[num6].Layers.Length == 0)))
                        {
                            MyLog.Default.WriteLine("Material rule does not contain any material ids.");
                            indices.Add(num6);
                        }
                        else
                        {
                            if ((materialRules[num6].Layers != null) && (materialRules[num6].Layers.Length != 0))
                            {
                                float depth = materialRules[num6].Layers[0].Depth;
                                for (int i = 1; i < materialRules[num6].Layers.Length; i++)
                                {
                                    float* singlePtr2 = (float*) ref materialRules[num6].Layers[i].Depth;
                                    singlePtr2[0] += depth;
                                    depth = materialRules[num6].Layers[i].Depth;
                                }
                            }
                            materialRules[num6].Slope.ConvertToCosine();
                            materialRules[num6].Latitude.ConvertToSine();
                            materialRules[num6].Longitude.ConvertToCosineLongitude();
                        }
                        num6++;
                    }
                }
            }
            if (definition.OreMappings != null)
            {
                this.OreMappings = definition.OreMappings;
            }
            if (definition.MaterialBlending != null)
            {
                this.MaterialBlending = definition.MaterialBlending.Value;
            }
            if (definition.SurfaceDetail != null)
            {
                this.Detail = definition.SurfaceDetail;
            }
            if (definition.AnimalSpawnInfo != null)
            {
                this.AnimalSpawnInfo = definition.AnimalSpawnInfo;
            }
            if (definition.NightAnimalSpawnInfo != null)
            {
                this.NightAnimalSpawnInfo = definition.NightAnimalSpawnInfo;
            }
            if (definition.SectorDensity != null)
            {
                this.SectorDensity = definition.SectorDensity.Value;
            }
            MyObjectBuilder_PlanetMapProvider mapProvider = definition.MapProvider;
            if (definition.MapProvider == null)
            {
                MyObjectBuilder_PlanetMapProvider local1 = definition.MapProvider;
                MyObjectBuilder_PlanetTextureMapProvider provider1 = new MyObjectBuilder_PlanetTextureMapProvider();
                provider1.Path = this.FolderName;
                mapProvider = provider1;
            }
            this.MapProvider = mapProvider;
            this.MesherPostprocessing = definition.MesherPostprocessing;
            if (this.MesherPostprocessing == null)
            {
                MyLog.Default.Warning("PERFORMANCE WARNING: Postprocessing voxel triangle decimation steps not defined for " + this, Array.Empty<object>());
            }
            this.MinimumSurfaceLayerDepth = definition.MinimumSurfaceLayerDepth;
        }

        public override string ToString()
        {
            string str = base.ToString();
            foreach (FieldInfo info in typeof(MyPlanetGeneratorDefinition).GetFields())
            {
                if (info.IsPublic)
                {
                    object obj2 = info.GetValue(this);
                    object[] objArray1 = new object[] { str, "\n   ", info.Name, " = ", obj2 ?? "<null>" };
                    str = string.Concat(objArray1);
                }
            }
            foreach (PropertyInfo info2 in typeof(MyPlanetGeneratorDefinition).GetProperties())
            {
                object obj3 = info2.GetValue(this, null);
                object[] objArray2 = new object[] { str, "\n   ", info2.Name, " = ", obj3 ?? "<null>" };
                str = string.Concat(objArray2);
            }
            return str;
        }

        internal class Postprocessor : MyDefinitionPostprocessor
        {
            public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
            {
            }

            public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
            {
                List<int> indices = new List<int>();
                foreach (MyDefinitionBase base2 in definitions.Values)
                {
                    MyPlanetGeneratorDefinition definition = (MyPlanetGeneratorDefinition) base2;
                    if (definition.EnvironmentId != null)
                    {
                        definition.EnvironmentDefinition = MyDefinitionManager.Static.GetDefinition<MyWorldEnvironmentDefinition>(definition.EnvironmentId.Value);
                    }
                    else
                    {
                        definition.EnvironmentDefinition = MyProceduralEnvironmentDefinition.FromLegacyPlanet(definition.m_pgob, base2.Context);
                        set.AddOrRelaceDefinition(definition.EnvironmentDefinition);
                        definition.m_pgob = null;
                    }
                    if (definition.EnvironmentDefinition != null)
                    {
                        definition.EnvironmentSectorType = definition.EnvironmentDefinition.SectorType;
                        using (Dictionary<int, Dictionary<string, List<MyPlanetEnvironmentMapping>>>.ValueCollection.Enumerator enumerator2 = definition.MaterialEnvironmentMappings.Values.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                using (Dictionary<string, List<MyPlanetEnvironmentMapping>>.ValueCollection.Enumerator enumerator3 = enumerator2.Current.Values.GetEnumerator())
                                {
                                    while (enumerator3.MoveNext())
                                    {
                                        foreach (MyPlanetEnvironmentMapping mapping in enumerator3.Current)
                                        {
                                            int index = 0;
                                            while (true)
                                            {
                                                MyEnvironmentItemsDefinition definition2;
                                                if (index >= mapping.Items.Length)
                                                {
                                                    if (indices.Count > 0)
                                                    {
                                                        mapping.Items = mapping.Items.RemoveIndices<MyMaterialEnvironmentItem>(indices);
                                                        mapping.ComputeDistribution();
                                                        indices.Clear();
                                                    }
                                                    break;
                                                }
                                                if (mapping.Items[index].IsEnvironemntItem && !MyDefinitionManager.Static.TryGetDefinition<MyEnvironmentItemsDefinition>(mapping.Items[index].Definition, out definition2))
                                                {
                                                    MyLog.Default.WriteLine($"Could not find environment item definition for {mapping.Items[index].Definition}.");
                                                    indices.Add(index);
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override int Priority =>
                0x3e8;
        }
    }
}


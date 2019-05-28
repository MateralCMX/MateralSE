namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Definitions;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ProceduralWorldEnvironment), typeof(MyProceduralEnvironmentDefinitionPostprocessor))]
    public class MyProceduralEnvironmentDefinition : MyWorldEnvironmentDefinition
    {
        private static readonly int[] ArrayOfZero = new int[1];
        private MyObjectBuilder_ProceduralWorldEnvironment m_ob;
        public Dictionary<string, MyItemTypeDefinition> ItemTypes = new Dictionary<string, MyItemTypeDefinition>();
        public Dictionary<MyBiomeMaterial, List<MyEnvironmentItemMapping>> MaterialEnvironmentMappings;
        public MyProceduralScanningMethod ScanningMethod;

        public static MyWorldEnvironmentDefinition FromLegacyPlanet(MyObjectBuilder_PlanetGeneratorDefinition pgdef, MyModContext context)
        {
            PlanetEnvironmentItemMapping[] environmentItems;
            int num;
            MyProceduralEnvironmentMapping mapping2;
            MyPlanetEnvironmentItemDef[] items;
            int num2;
            MyObjectBuilder_ProceduralWorldEnvironment builder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ProceduralWorldEnvironment>(pgdef.Id.SubtypeId);
            builder.Id = new SerializableDefinitionId(builder.TypeId, builder.SubtypeName);
            SerializableDefinitionId id = new SerializableDefinitionId(typeof(MyObjectBuilder_ProceduralEnvironmentModuleDefinition), "Static");
            SerializableDefinitionId id2 = new SerializableDefinitionId(typeof(MyObjectBuilder_ProceduralEnvironmentModuleDefinition), "Memory");
            SerializableDefinitionId id3 = new SerializableDefinitionId(typeof(MyObjectBuilder_EnvironmentModuleProxyDefinition), "Breakable");
            SerializableDefinitionId id4 = new SerializableDefinitionId(typeof(MyObjectBuilder_EnvironmentModuleProxyDefinition), "VoxelMap");
            SerializableDefinitionId id5 = new SerializableDefinitionId(typeof(MyObjectBuilder_EnvironmentModuleProxyDefinition), "BotSpawner");
            SerializableDefinitionId id1 = new SerializableDefinitionId(typeof(MyObjectBuilder_EnvironmentModuleProxyDefinition), "EnvironmentalParticles");
            MyEnvironmentItemTypeDefinition definition = new MyEnvironmentItemTypeDefinition {
                LodFrom = -1,
                LodTo = 1,
                Name = "Tree",
                Provider = new SerializableDefinitionId?(id)
            };
            definition.Proxies = new SerializableDefinitionId[] { id3 };
            MyEnvironmentItemTypeDefinition[] definitionArray1 = new MyEnvironmentItemTypeDefinition[4];
            definitionArray1[0] = definition;
            definition = new MyEnvironmentItemTypeDefinition {
                LodFrom = 0,
                LodTo = -1,
                Name = "Bush",
                Provider = new SerializableDefinitionId?(id)
            };
            definition.Proxies = new SerializableDefinitionId[] { id3 };
            definitionArray1[1] = definition;
            definition = new MyEnvironmentItemTypeDefinition {
                LodFrom = 0,
                LodTo = -1,
                Name = "VoxelMap",
                Provider = new SerializableDefinitionId?(id2)
            };
            definition.Proxies = new SerializableDefinitionId[] { id4 };
            definitionArray1[2] = definition;
            definition = new MyEnvironmentItemTypeDefinition {
                LodFrom = 0,
                LodTo = -1,
                Name = "Bot",
                Provider = null
            };
            definition.Proxies = new SerializableDefinitionId[] { id5 };
            definitionArray1[3] = definition;
            builder.ItemTypes = definitionArray1;
            builder.ScanningMethod = MyProceduralScanningMethod.Random;
            builder.ItemsPerSqMeter = 0.0034;
            builder.MaxSyncLod = 0;
            builder.SectorSize = 200.0;
            List<MyProceduralEnvironmentMapping> list = new List<MyProceduralEnvironmentMapping>();
            List<MyEnvironmentItemInfo> list2 = new List<MyEnvironmentItemInfo>();
            MyPlanetSurfaceRule rule = new MyPlanetSurfaceRule();
            if (pgdef.EnvironmentItems == null)
            {
                goto TR_0000;
            }
            else
            {
                environmentItems = pgdef.EnvironmentItems;
                num = 0;
            }
            goto TR_0015;
        TR_0000:
            list.Capacity = list.Count;
            builder.EnvironmentMappings = list.GetInternalArray<MyProceduralEnvironmentMapping>();
            MyProceduralEnvironmentDefinition definition1 = new MyProceduralEnvironmentDefinition();
            definition1.Context = context;
            definition1.Init(builder);
            return definition1;
        TR_0002:
            num2++;
        TR_0012:
            while (true)
            {
                if (num2 >= items.Length)
                {
                    mapping2.Items = list2.ToArray();
                    list.Add(mapping2);
                    num++;
                    break;
                }
                MyPlanetEnvironmentItemDef def = items[num2];
                MyEnvironmentItemInfo info1 = new MyEnvironmentItemInfo();
                info1.Density = def.Density;
                info1.Subtype = MyStringHash.GetOrCompute(def.SubtypeId);
                MyEnvironmentItemInfo item = info1;
                string typeId = def.TypeId;
                if (typeId == "MyObjectBuilder_DestroyableItems")
                {
                    item.Type = "Bush";
                    item.Density *= 0.5f;
                }
                else if (typeId == "MyObjectBuilder_Trees")
                {
                    item.Type = "Tree";
                }
                else if (typeId == "MyObjectBuilder_VoxelMapStorageDefinition")
                {
                    item.Type = "VoxelMap";
                    item.Density *= 0.5f;
                    if (def.SubtypeId == null)
                    {
                        MyStringHash orCompute = MyStringHash.GetOrCompute($"G({def.GroupId})M({def.ModifierId})");
                        if (MyDefinitionManager.Static.GetDefinition<MyVoxelMapCollectionDefinition>(orCompute) == null)
                        {
                            MyObjectBuilder_VoxelMapCollectionDefinition definition3 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_VoxelMapCollectionDefinition>(orCompute.ToString());
                            definition3.Id = new SerializableDefinitionId(definition3.TypeId, definition3.SubtypeName);
                            MyObjectBuilder_VoxelMapCollectionDefinition.VoxelMapStorage storage = new MyObjectBuilder_VoxelMapCollectionDefinition.VoxelMapStorage {
                                Storage = def.GroupId
                            };
                            definition3.StorageDefs = new MyObjectBuilder_VoxelMapCollectionDefinition.VoxelMapStorage[] { storage };
                            definition3.Modifier = def.ModifierId;
                            MyVoxelMapCollectionDefinition definition2 = new MyVoxelMapCollectionDefinition();
                            definition2.Init(definition3, context);
                            MyDefinitionManager.Static.Definitions.AddDefinition(definition2);
                        }
                        item.Subtype = orCompute;
                    }
                }
                else
                {
                    object[] args = new object[] { pgdef.SubtypeName, def.SubtypeId };
                    MyLog.Default.Error("Planet Generator {0}: Invalid Item Type: {1}", args);
                    goto TR_0002;
                }
                MyStringHash subtype = item.Subtype;
                list2.Add(item);
                goto TR_0002;
            }
        TR_0015:
            while (true)
            {
                if (num < environmentItems.Length)
                {
                    PlanetEnvironmentItemMapping mapping = environmentItems[num];
                    mapping2 = new MyProceduralEnvironmentMapping {
                        Biomes = mapping.Biomes,
                        Materials = mapping.Materials
                    };
                    MyPlanetSurfaceRule rule2 = mapping.Rule ?? rule;
                    mapping2.Height = rule2.Height;
                    mapping2.Latitude = rule2.Latitude;
                    mapping2.Longitude = rule2.Longitude;
                    mapping2.Slope = rule2.Slope;
                    list2.Clear();
                    items = mapping.Items;
                    num2 = 0;
                }
                else
                {
                    goto TR_0000;
                }
                break;
            }
            goto TR_0012;
        }

        public void GetItemDefinition(ushort definitionIndex, out MyRuntimeEnvironmentItemInfo def)
        {
            if (definitionIndex >= base.Items.Length)
            {
                def = null;
            }
            else
            {
                def = base.Items[definitionIndex];
            }
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ProceduralWorldEnvironment environment = (MyObjectBuilder_ProceduralWorldEnvironment) builder;
            this.m_ob = environment;
            this.ScanningMethod = environment.ScanningMethod;
        }

        public void Prepare()
        {
            int num;
            if (this.m_ob.ItemTypes != null)
            {
                MyEnvironmentItemTypeDefinition[] itemTypes = this.m_ob.ItemTypes;
                num = 0;
                while (num < itemTypes.Length)
                {
                    MyEnvironmentItemTypeDefinition def = itemTypes[num];
                    try
                    {
                        MyItemTypeDefinition definition2 = new MyItemTypeDefinition(def);
                        this.ItemTypes.Add(def.Name, definition2);
                    }
                    catch (ArgumentException)
                    {
                        object[] args = new object[] { def.Name };
                        MyLog.Default.Error("Duplicate environment item definition for item {0}.", args);
                    }
                    catch (Exception exception)
                    {
                        object[] args = new object[] { def.Name, exception.Message };
                        MyLog.Default.Error("Error preparing environment item definition for item {0}:\n {1}", args);
                    }
                    num++;
                }
            }
            this.MaterialEnvironmentMappings = new Dictionary<MyBiomeMaterial, List<MyEnvironmentItemMapping>>(MyBiomeMaterial.Comparer);
            List<MyRuntimeEnvironmentItemInfo> list = new List<MyRuntimeEnvironmentItemInfo>();
            MyProceduralEnvironmentMapping[] environmentMappings = this.m_ob.EnvironmentMappings;
            if ((environmentMappings != null) && (environmentMappings.Length != 0))
            {
                this.MaterialEnvironmentMappings = new Dictionary<MyBiomeMaterial, List<MyEnvironmentItemMapping>>(MyBiomeMaterial.Comparer);
                for (int i = 0; i < environmentMappings.Length; i++)
                {
                    MyProceduralEnvironmentMapping mapping = environmentMappings[i];
                    MyEnvironmentRule rule1 = new MyEnvironmentRule();
                    rule1.Height = mapping.Height;
                    rule1.Slope = mapping.Slope;
                    rule1.Latitude = mapping.Latitude;
                    rule1.Longitude = mapping.Longitude;
                    MyEnvironmentRule rule = rule1;
                    if (mapping.Materials == null)
                    {
                        object[] args = new object[] { base.Id };
                        MyLog.Default.Warning("Mapping in definition {0} does not define any materials, it will not be applied.", args);
                    }
                    else
                    {
                        if (mapping.Biomes == null)
                        {
                            mapping.Biomes = ArrayOfZero;
                        }
                        bool flag = false;
                        MyRuntimeEnvironmentItemInfo[] map = new MyRuntimeEnvironmentItemInfo[mapping.Items.Length];
                        int index = 0;
                        while (true)
                        {
                            if (index >= mapping.Items.Length)
                            {
                                if (flag)
                                {
                                    MyEnvironmentItemMapping item = new MyEnvironmentItemMapping(map, rule, this);
                                    int[] biomes = mapping.Biomes;
                                    num = 0;
                                    while (num < biomes.Length)
                                    {
                                        int num4 = biomes[num];
                                        string[] materials = mapping.Materials;
                                        int num5 = 0;
                                        while (true)
                                        {
                                            if (num5 >= materials.Length)
                                            {
                                                num++;
                                                break;
                                            }
                                            string name = materials[num5];
                                            if (MyDefinitionManager.Static.GetVoxelMaterialDefinition(name) != null)
                                            {
                                                List<MyEnvironmentItemMapping> list2;
                                                MyBiomeMaterial key = new MyBiomeMaterial((byte) num4, MyDefinitionManager.Static.GetVoxelMaterialDefinition(name).Index);
                                                if (!this.MaterialEnvironmentMappings.TryGetValue(key, out list2))
                                                {
                                                    list2 = new List<MyEnvironmentItemMapping>();
                                                    this.MaterialEnvironmentMappings[key] = list2;
                                                }
                                                list2.Add(item);
                                            }
                                            num5++;
                                        }
                                    }
                                }
                                break;
                            }
                            if (!this.ItemTypes.ContainsKey(mapping.Items[index].Type))
                            {
                                object[] args = new object[] { mapping.Items[index].Type };
                                MyLog.Default.Error("No definition for item type {0}", args);
                            }
                            else
                            {
                                map[index] = new MyRuntimeEnvironmentItemInfo(this, mapping.Items[index], list.Count);
                                list.Add(map[index]);
                                flag = true;
                            }
                            index++;
                        }
                    }
                }
            }
            base.Items = list.GetInternalArray<MyRuntimeEnvironmentItemInfo>();
            this.m_ob = null;
        }

        public override Type SectorType =>
            typeof(MyEnvironmentSector);
    }
}


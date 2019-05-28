namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Common;
    using VRage.Game.ModAPI;
    using VRage.Game.Voxels;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender.Messages;

    public class MyWorldGenerator
    {
        private static List<MyCubeGrid> m_tmpSpawnedGridList = new List<MyCubeGrid>();
        [CompilerGenerated]
        private static ActionRef<Args> OnAfterGenerate;

        public static  event ActionRef<Args> OnAfterGenerate
        {
            [CompilerGenerated] add
            {
                ActionRef<Args> onAfterGenerate = OnAfterGenerate;
                while (true)
                {
                    ActionRef<Args> a = onAfterGenerate;
                    ActionRef<Args> ref4 = (ActionRef<Args>) Delegate.Combine(a, value);
                    onAfterGenerate = Interlocked.CompareExchange<ActionRef<Args>>(ref OnAfterGenerate, ref4, a);
                    if (ReferenceEquals(onAfterGenerate, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ActionRef<Args> onAfterGenerate = OnAfterGenerate;
                while (true)
                {
                    ActionRef<Args> source = onAfterGenerate;
                    ActionRef<Args> ref4 = (ActionRef<Args>) Delegate.Remove(source, value);
                    onAfterGenerate = Interlocked.CompareExchange<ActionRef<Args>>(ref OnAfterGenerate, ref4, source);
                    if (ReferenceEquals(onAfterGenerate, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyWorldGenerator()
        {
            if (MyFakes.TEST_PREFABS_FOR_INCONSISTENCIES)
            {
                string[] files = Directory.GetFiles(Path.Combine(MyFileSystem.ContentPath, "Data", "Prefabs"));
                int index = 0;
                while (true)
                {
                    List<MyObjectBuilder_CubeBlock>.Enumerator enumerator;
                    if (index >= files.Length)
                    {
                        files = Directory.GetDirectories(Path.Combine(MyFileSystem.ContentPath, "Worlds"));
                        index = 0;
                        while (index < files.Length)
                        {
                            string[] strArray2 = Directory.GetFiles(files[index]);
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= strArray2.Length)
                                {
                                    index++;
                                    break;
                                }
                                string str2 = strArray2[num2];
                                if (Path.GetExtension(str2) == ".sbs")
                                {
                                    MyObjectBuilder_Sector objectBuilder = null;
                                    MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Sector>(Path.Combine(MyFileSystem.ContentPath, str2), out objectBuilder);
                                    foreach (MyObjectBuilder_EntityBase base2 in objectBuilder.SectorObjects)
                                    {
                                        if (base2.TypeId == typeof(MyObjectBuilder_CubeGrid))
                                        {
                                            using (enumerator = ((MyObjectBuilder_CubeGrid) base2).CubeBlocks.GetEnumerator())
                                            {
                                                while (enumerator.MoveNext() && (enumerator.Current.IntegrityPercent != 0f))
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                                num2++;
                            }
                        }
                        break;
                    }
                    string path = files[index];
                    if (Path.GetExtension(path) == ".sbc")
                    {
                        MyObjectBuilder_CubeGrid objectBuilder = null;
                        MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_CubeGrid>(Path.Combine(MyFileSystem.ContentPath, path), out objectBuilder);
                        if (objectBuilder != null)
                        {
                            using (enumerator = objectBuilder.CubeBlocks.GetEnumerator())
                            {
                                while (enumerator.MoveNext() && (enumerator.Current.IntegrityPercent != 0f))
                                {
                                }
                            }
                        }
                    }
                    index++;
                }
            }
        }

        public static MyVoxelMap AddAsteroidPrefab(string prefabName, MatrixD worldMatrix, string name)
        {
            MyStorageBase storage = LoadRandomizedVoxelMapPrefab(GetVoxelPrefabPath(prefabName));
            return AddVoxelMap(name, storage, worldMatrix, 0L, false, true);
        }

        public static MyVoxelMap AddAsteroidPrefab(string prefabName, Vector3D position, string name)
        {
            MyStorageBase storage = LoadRandomizedVoxelMapPrefab(GetVoxelPrefabPath(prefabName));
            return AddVoxelMap(name, storage, position, 0L);
        }

        public static MyVoxelMap AddAsteroidPrefabCentered(string prefabName, Vector3D position, string name)
        {
            MyStorageBase storage = LoadRandomizedVoxelMapPrefab(GetVoxelPrefabPath(prefabName));
            Vector3 vector = (Vector3) (storage.Size * 0.5f);
            return AddVoxelMap(name, storage, position - vector, 0L);
        }

        public static MyVoxelMap AddAsteroidPrefabCentered(string prefabName, Vector3D position, MatrixD rotation, string name)
        {
            MyStorageBase storage = LoadRandomizedVoxelMapPrefab(GetVoxelPrefabPath(prefabName));
            Vector3 vector = (Vector3) (storage.Size * 0.5f);
            rotation.Translation = position - vector;
            return AddVoxelMap(name, storage, rotation, 0L, false, true);
        }

        public static void AddEntity(MyObjectBuilder_EntityBase entityBuilder)
        {
            MyEntities.CreateFromObjectBuilderAndAdd(entityBuilder, false);
        }

        private static void AddObjectsPrefab(string prefabName)
        {
            using (List<MyObjectBuilder_EntityBase>.Enumerator enumerator = LoadObjectsPrefab(prefabName).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyEntities.CreateFromObjectBuilderAndAdd(enumerator.Current, false);
                }
            }
        }

        public static MyPlanet AddPlanet(string storageName, string planetName, string definitionName, Vector3D positionMinCorner, int seed, float size, bool fadeIn, long entityId = 0L, bool addGPS = false, bool userCreated = false)
        {
            MyPlanetGeneratorDefinition generatorDef = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(definitionName));
            return CreatePlanet(storageName, planetName, ref positionMinCorner, seed, size, entityId, ref generatorDef, addGPS, userCreated, fadeIn);
        }

        public static unsafe void AddPlanetPrefab(string planetName, string definitionName, Vector3D position, bool addGPS, bool fadeIn)
        {
            foreach (MyPlanetPrefabDefinition definition in MyDefinitionManager.Static.GetPlanetsPrefabsDefinitions())
            {
                if (definition.Id.SubtypeName == planetName)
                {
                    MyPlanetInitArguments arguments;
                    MyPlanetInitArguments* argumentsPtr1;
                    MyPlanetGeneratorDefinition definition2 = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(definitionName));
                    MyObjectBuilder_Planet planetBuilder = definition.PlanetBuilder;
                    MyPlanet planet1 = new MyPlanet();
                    planet1.EntityId = planetBuilder.EntityId;
                    arguments.StorageName = planetBuilder.StorageName;
                    arguments.Seed = planetBuilder.Seed;
                    arguments.Storage = MyStorageBase.LoadFromFile(MyFileSystem.ContentPath + @"\VoxelMaps\" + planetBuilder.StorageName + ".vx2", null, true);
                    arguments.PositionMinCorner = position;
                    arguments.Radius = planetBuilder.Radius;
                    arguments.AtmosphereRadius = planetBuilder.AtmosphereRadius;
                    arguments.MaxRadius = planetBuilder.MaximumHillRadius;
                    arguments.MinRadius = planetBuilder.MinimumSurfaceRadius;
                    arguments.HasAtmosphere = definition2.HasAtmosphere;
                    arguments.AtmosphereWavelengths = planetBuilder.AtmosphereWavelengths;
                    arguments.GravityFalloff = definition2.GravityFalloffPower;
                    arguments.MarkAreaEmpty = true;
                    MyAtmosphereSettings? atmosphereSettings = planetBuilder.AtmosphereSettings;
                    argumentsPtr1->AtmosphereSettings = (atmosphereSettings != null) ? atmosphereSettings.GetValueOrDefault() : MyAtmosphereSettings.Defaults();
                    argumentsPtr1 = (MyPlanetInitArguments*) ref arguments;
                    arguments.SurfaceGravity = definition2.SurfaceGravity;
                    arguments.AddGps = addGPS;
                    arguments.SpherizeWithDistance = true;
                    arguments.Generator = definition2;
                    arguments.UserCreated = false;
                    arguments.InitializeComponents = true;
                    arguments.FadeIn = fadeIn;
                    MyPlanet entity = planet1;
                    entity.Init(arguments);
                    MyEntities.Add(entity, true);
                    MyEntities.RaiseEntityCreated(entity);
                }
            }
        }

        public static MyVoxelMap AddVoxelMap(string storageName, MyStorageBase storage, Vector3D positionMinCorner, long entityId = 0L)
        {
            MyVoxelMap entity = new MyVoxelMap();
            if (entityId != 0)
            {
                entity.EntityId = entityId;
            }
            entity.Init(storageName, storage, positionMinCorner);
            MyEntities.RaiseEntityCreated(entity);
            MyEntities.Add(entity, true);
            return entity;
        }

        public static MyVoxelMap AddVoxelMap(string storageName, MyStorageBase storage, MatrixD worldMatrix, long entityId = 0L, bool lazyPhysics = false, bool useVoxelOffset = true)
        {
            if ((entityId == 0) || !MyEntityIdentifier.ExistsById(entityId))
            {
                MyVoxelMap entity = new MyVoxelMap();
                if (entityId != 0)
                {
                    entity.EntityId = entityId;
                }
                entity.DelayRigidBodyCreation = lazyPhysics;
                entity.Init(storageName, storage, worldMatrix, useVoxelOffset);
                MyEntities.Add(entity, true);
                MyEntities.RaiseEntityCreated(entity);
                return entity;
            }
            MyVoxelMap entityById = MyEntityIdentifier.GetEntityById(entityId, false) as MyVoxelMap;
            if ((entityById != null) && (entityById.StorageName == storageName))
            {
                MyLog.Default.WriteLine($"CRITICAL-VOXEL MAP!!! ---- VoxelMap already loaded. This must not happen ({storageName})", LoggingOptions.VOXEL_MAPS);
            }
            else
            {
                IMyEntity entity = MyEntityIdentifier.GetEntityById(entityId, false);
                if (entity == null)
                {
                    MyLog.Default.WriteLine($"CRITICAL-VOXEL MAP!!! ---- VoxelMap entity collision. Entity (null) with id {entityId} is already registered in place of VoxelMap{storageName}.", LoggingOptions.VOXEL_MAPS);
                }
                else
                {
                    MyLog.Default.WriteLine($"CRITICAL-VOXEL MAP!!! ---- VoxelMap entity collision. Entity with id {entityId} is already registered in place of VoxelMap{storageName}. ( entity ({entity.DisplayName}) ({entity.GetType()}) ({entity.ToString()}) )", LoggingOptions.VOXEL_MAPS);
                }
            }
            return null;
        }

        public static void CallOnAfterGenerate(ref Args args)
        {
            if (OnAfterGenerate != null)
            {
                OnAfterGenerate(ref args);
            }
        }

        private static unsafe MyPlanet CreatePlanet(string storageName, string planetName, ref Vector3D positionMinCorner, int seed, float size, long entityId, ref MyPlanetGeneratorDefinition generatorDef, bool addGPS, bool userCreated = false, bool fadeIn = false)
        {
            if (!MyFakes.ENABLE_PLANETS)
            {
                return null;
            }
            MyRandom instance = MyRandom.Instance;
            using (MyRandom.Instance.PushSeed(seed))
            {
                MyPlanetInitArguments arguments;
                MyPlanetInitArguments* argumentsPtr1;
                float single2;
                MyPlanetStorageProvider dataProvider = new MyPlanetStorageProvider();
                dataProvider.Init((long) seed, generatorDef, (double) (size / 2f));
                VRage.Game.Voxels.IMyStorage storage = new MyOctreeStorage(dataProvider, dataProvider.StorageSize);
                float num2 = dataProvider.Radius * generatorDef.HillParams.Max;
                float radius = dataProvider.Radius;
                float num3 = radius + num2;
                float num4 = radius + (dataProvider.Radius * generatorDef.HillParams.Min);
                if ((generatorDef.AtmosphereSettings == null) || (generatorDef.AtmosphereSettings.Value.Scale <= 1f))
                {
                    single2 = 1.75f;
                }
                else
                {
                    single2 = 1f + generatorDef.AtmosphereSettings.Value.Scale;
                }
                float num7 = instance.NextFloat(generatorDef.HostileAtmosphereColorShift.G.Min, generatorDef.HostileAtmosphereColorShift.G.Max);
                float num8 = instance.NextFloat(generatorDef.HostileAtmosphereColorShift.B.Min, generatorDef.HostileAtmosphereColorShift.B.Max);
                Vector3 vector = new Vector3(0.65f + instance.NextFloat(generatorDef.HostileAtmosphereColorShift.R.Min, generatorDef.HostileAtmosphereColorShift.R.Max), 0.57f + num7, 0.475f + num8);
                Vector3* vectorPtr1 = (Vector3*) ref vector;
                vectorPtr1->X = MathHelper.Clamp(vector.X, 0.1f, 1f);
                Vector3* vectorPtr2 = (Vector3*) ref vector;
                vectorPtr2->Y = MathHelper.Clamp(vector.Y, 0.1f, 1f);
                Vector3* vectorPtr3 = (Vector3*) ref vector;
                vectorPtr3->Z = MathHelper.Clamp(vector.Z, 0.1f, 1f);
                MyPlanet planet1 = new MyPlanet();
                planet1.EntityId = entityId;
                arguments.StorageName = storageName;
                arguments.Seed = seed;
                arguments.Storage = storage;
                arguments.PositionMinCorner = positionMinCorner;
                arguments.Radius = dataProvider.Radius;
                arguments.AtmosphereRadius = single2 * dataProvider.Radius;
                arguments.MaxRadius = num3;
                arguments.MinRadius = num4;
                arguments.HasAtmosphere = generatorDef.HasAtmosphere;
                arguments.AtmosphereWavelengths = vector;
                arguments.GravityFalloff = generatorDef.GravityFalloffPower;
                arguments.MarkAreaEmpty = true;
                MyAtmosphereSettings? atmosphereSettings = generatorDef.AtmosphereSettings;
                argumentsPtr1->AtmosphereSettings = (atmosphereSettings != null) ? atmosphereSettings.GetValueOrDefault() : MyAtmosphereSettings.Defaults();
                argumentsPtr1 = (MyPlanetInitArguments*) ref arguments;
                arguments.SurfaceGravity = generatorDef.SurfaceGravity;
                arguments.AddGps = addGPS;
                arguments.SpherizeWithDistance = true;
                arguments.Generator = generatorDef;
                arguments.UserCreated = userCreated;
                arguments.InitializeComponents = true;
                arguments.FadeIn = fadeIn;
                MyPlanet entity = planet1;
                entity.Init(arguments);
                entity.AsteroidName = planetName;
                MyEntities.RaiseEntityCreated(entity);
                MyEntities.Add(entity, true);
                return entity;
            }
        }

        public static void FillInventoryWithDefaults(MyObjectBuilder_Inventory inventory, MyScenarioDefinition scenario)
        {
            if (inventory.Items == null)
            {
                inventory.Items = new List<MyObjectBuilder_InventoryItem>();
            }
            else
            {
                inventory.Items.Clear();
            }
            if ((scenario != null) && MySession.Static.Settings.SpawnWithTools)
            {
                int num2;
                MyScenarioDefinition.StartingItem[] itemArray4;
                MyStringId itemName;
                MyStringId[] idArray = !MySession.Static.CreativeMode ? scenario.SurvivalModeWeapons : scenario.CreativeModeWeapons;
                uint num = 0;
                if (idArray != null)
                {
                    MyStringId[] idArray2 = idArray;
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= idArray2.Length)
                        {
                            inventory.nextItemId = num;
                            break;
                        }
                        MyStringId id = idArray2[num2];
                        MyObjectBuilder_InventoryItem item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
                        item.Amount = 1;
                        item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalGunObject>(id.ToString());
                        num++;
                        item.ItemId = num;
                        inventory.Items.Add(item);
                        num2++;
                    }
                }
                MyScenarioDefinition.StartingItem[] itemArray = !MySession.Static.CreativeMode ? scenario.SurvivalModeComponents : scenario.CreativeModeComponents;
                if (itemArray != null)
                {
                    itemArray4 = itemArray;
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= itemArray4.Length)
                        {
                            inventory.nextItemId = num;
                            break;
                        }
                        MyScenarioDefinition.StartingItem item2 = itemArray4[num2];
                        MyObjectBuilder_InventoryItem item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
                        item.Amount = item2.amount;
                        itemName = item2.itemName;
                        item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(itemName.ToString());
                        num++;
                        item.ItemId = num;
                        inventory.Items.Add(item);
                        num2++;
                    }
                }
                MyScenarioDefinition.StartingPhysicalItem[] itemArray2 = !MySession.Static.CreativeMode ? scenario.SurvivalModePhysicalItems : scenario.CreativeModePhysicalItems;
                if (itemArray2 != null)
                {
                    MyScenarioDefinition.StartingPhysicalItem[] itemArray5 = itemArray2;
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= itemArray5.Length)
                        {
                            inventory.nextItemId = num;
                            break;
                        }
                        MyScenarioDefinition.StartingPhysicalItem item4 = itemArray5[num2];
                        MyObjectBuilder_InventoryItem item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
                        item.Amount = item4.amount;
                        if (item4.itemType.ToString().Equals("Ore"))
                        {
                            itemName = item4.itemName;
                            item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(itemName.ToString());
                        }
                        else if (item4.itemType.ToString().Equals("Ingot"))
                        {
                            item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ingot>(item4.itemName.ToString());
                        }
                        else if (item4.itemType.ToString().Equals("OxygenBottle"))
                        {
                            item.Amount = 1;
                            item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_OxygenContainerObject>(item4.itemName.ToString());
                            (item.PhysicalContent as MyObjectBuilder_GasContainerObject).GasLevel = (float) item4.amount;
                        }
                        else if (item4.itemType.ToString().Equals("GasBottle"))
                        {
                            item.Amount = 1;
                            item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GasContainerObject>(item4.itemName.ToString());
                            (item.PhysicalContent as MyObjectBuilder_GasContainerObject).GasLevel = (float) item4.amount;
                        }
                        num++;
                        item.ItemId = num;
                        inventory.Items.Add(item);
                        num2++;
                    }
                }
                itemArray = !MySession.Static.CreativeMode ? scenario.SurvivalModeAmmoItems : scenario.CreativeModeAmmoItems;
                if (itemArray != null)
                {
                    itemArray4 = itemArray;
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= itemArray4.Length)
                        {
                            inventory.nextItemId = num;
                            break;
                        }
                        MyScenarioDefinition.StartingItem item6 = itemArray4[num2];
                        MyObjectBuilder_InventoryItem item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
                        item.Amount = item6.amount;
                        item.PhysicalContent = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_AmmoMagazine>(item6.itemName.ToString());
                        num++;
                        item.ItemId = num;
                        inventory.Items.Add(item);
                        num2++;
                    }
                }
                MyObjectBuilder_InventoryItem[] itemArray3 = MySession.Static.CreativeMode ? scenario.CreativeInventoryItems : scenario.SurvivalInventoryItems;
                if (itemArray3 != null)
                {
                    MyObjectBuilder_InventoryItem[] itemArray6 = itemArray3;
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= itemArray6.Length)
                        {
                            inventory.nextItemId = num;
                            break;
                        }
                        MyObjectBuilder_InventoryItem item = itemArray6[num2].Clone() as MyObjectBuilder_InventoryItem;
                        num++;
                        item.ItemId = num;
                        inventory.Items.Add(item);
                        num2++;
                    }
                }
            }
        }

        public static void GenerateWorld(Args args)
        {
            MySandboxGame.Log.WriteLine("MyWorldGenerator.GenerateWorld - START");
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                RunGeneratorOperations(ref args);
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    SetupPlayer(ref args);
                }
                CallOnAfterGenerate(ref args);
            }
            MySandboxGame.Log.WriteLine("MyWorldGenerator.GenerateWorld - END");
        }

        private static string GetObjectsPrefabPath(string prefabName) => 
            Path.Combine("Data", "Prefabs", prefabName + ".sbs");

        public static string GetPrefabTypeName(MyObjectBuilder_EntityBase entity)
        {
            if (entity is MyObjectBuilder_VoxelMap)
            {
                return "Asteroid";
            }
            if (!(entity is MyObjectBuilder_CubeGrid))
            {
                return (!(entity is MyObjectBuilder_Character) ? "Unknown" : "Character");
            }
            MyObjectBuilder_CubeGrid grid = (MyObjectBuilder_CubeGrid) entity;
            return (!grid.IsStatic ? ((grid.GridSizeEnum != MyCubeSize.Large) ? "SmallShip" : "LargeShip") : "Station");
        }

        public static string GetVoxelPrefabPath(string prefabName)
        {
            MyVoxelMapStorageDefinition definition;
            return (!MyDefinitionManager.Static.TryGetVoxelMapStorageDefinition(prefabName, out definition) ? Path.Combine(MyFileSystem.ContentPath, "VoxelMaps", prefabName + ".vx2") : (!definition.Context.IsBaseGame ? definition.StorageFile : Path.Combine(MyFileSystem.ContentPath, definition.StorageFile)));
        }

        public static void InitInventoryWithDefaults(MyInventory inventory)
        {
            MyObjectBuilder_Inventory inventory2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Inventory>();
            FillInventoryWithDefaults(inventory2, MySession.Static.Scenario);
            inventory.Init(inventory2);
        }

        private static List<MyObjectBuilder_EntityBase> LoadObjectsPrefab(string file)
        {
            MyObjectBuilder_Sector sector;
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Sector>(Path.Combine(MyFileSystem.ContentPath, GetObjectsPrefabPath(file)), out sector);
            using (List<MyObjectBuilder_EntityBase>.Enumerator enumerator = sector.SectorObjects.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.EntityId = 0L;
                }
            }
            return sector.SectorObjects;
        }

        public static MyStorageBase LoadRandomizedVoxelMapPrefab(string prefabFilePath)
        {
            MyStorageBase base2 = MyStorageBase.LoadFromFile(prefabFilePath, null, true);
            int? generator = null;
            base2.DataProvider = MyCompositeShapeProvider.CreateAsteroidShape(MyUtils.GetRandomInt(0x7ffffffe) + 1, base2.Size.AbsMax() * 1f, 0, generator);
            base2.Reset(MyStorageDataTypeFlags.Material);
            return base2;
        }

        private static void RunGeneratorOperations(ref Args args)
        {
            MyWorldGeneratorOperationBase[] worldGeneratorOperations = args.Scenario.WorldGeneratorOperations;
            if ((worldGeneratorOperations != null) && (worldGeneratorOperations.Length != 0))
            {
                MyWorldGeneratorOperationBase[] baseArray2 = worldGeneratorOperations;
                for (int i = 0; i < baseArray2.Length; i++)
                {
                    baseArray2[i].Apply();
                }
            }
        }

        public static void SetProceduralSettings(int? asteroidAmount, MyObjectBuilder_SessionSettings sessionSettings)
        {
            sessionSettings.ProceduralSeed = MyRandom.Instance.Next();
            switch (asteroidAmount.Value)
            {
                case -4:
                    sessionSettings.ProceduralDensity = 0f;
                    return;

                case -3:
                    sessionSettings.ProceduralDensity = 0.5f;
                    return;

                case -2:
                    sessionSettings.ProceduralDensity = 0.35f;
                    return;

                case -1:
                    sessionSettings.ProceduralDensity = 0.25f;
                    return;
            }
            throw new InvalidBranchException();
        }

        private static void SetupBase(string basePrefabName, Vector3 offset, string voxelFilename, string beaconName = null, long ownerId = 0L)
        {
            Vector3 initialLinearVelocity = new Vector3();
            initialLinearVelocity = new Vector3();
            MyPrefabManager.Static.SpawnPrefab(basePrefabName, new Vector3(-3f, 11f, 15f) + offset, Vector3.Forward, Vector3.Up, initialLinearVelocity, initialLinearVelocity, beaconName, null, SpawningOptions.None, 0L, false, null);
            MyPrefabManager.Static.AddShipPrefab("SmallShip_SingleBlock", new Matrix?(Matrix.CreateTranslation(new Vector3(-5.208184f, -0.4429844f, -8.315228f) + offset)), ownerId, false);
            if (voxelFilename != null)
            {
                MyStorageBase storage = LoadRandomizedVoxelMapPrefab(GetVoxelPrefabPath("VerticalIsland_128x128x128"));
                AddVoxelMap(voxelFilename, storage, new Vector3(-20f, -110f, -60f) + offset, 0L);
            }
        }

        private static void SetupPlayer(ref Args args)
        {
            Vector3? colorMask = null;
            MyIdentity identity = Sync.Players.CreateNewIdentity(Sync.Clients.LocalClient.DisplayName, null, colorMask, false);
            MyPlayer player = Sync.Players.CreateNewPlayer(identity, Sync.Clients.LocalClient, Sync.MyName, true);
            MyWorldGeneratorStartingStateBase[] possiblePlayerStarts = args.Scenario.PossiblePlayerStarts;
            if ((possiblePlayerStarts == null) || (possiblePlayerStarts.Length == 0))
            {
                Sync.Players.RespawnComponent.SetupCharacterDefault(player, args);
            }
            else
            {
                Sync.Players.RespawnComponent.SetupCharacterFromStarts(player, possiblePlayerStarts, args);
            }
            MyObjectBuilder_Toolbar defaultToolbar = args.Scenario.DefaultToolbar;
            if (defaultToolbar != null)
            {
                MyToolbar toolbar = new MyToolbar(MyToolbarType.Character, 9, 9);
                toolbar.Init(defaultToolbar, player.Character, true);
                MySession.Static.Toolbars.RemovePlayerToolbar(player.Id);
                MySession.Static.Toolbars.AddPlayerToolbar(player.Id, toolbar);
                MyToolbarComponent.InitToolbar(MyToolbarType.Character, defaultToolbar);
                MyToolbarComponent.InitCharacterToolbar(defaultToolbar);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Args
        {
            public MyScenarioDefinition Scenario;
            public int AsteroidAmount;
        }

        [StartingStateType(typeof(MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform))]
        public class MyTransformState : MyWorldGeneratorStartingStateBase
        {
            public MyPositionAndOrientation? Transform;
            public bool JetpackEnabled;
            public bool DampenersEnabled;

            public override MyObjectBuilder_WorldGeneratorPlayerStartingState GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform;
                objectBuilder.Transform = this.Transform;
                objectBuilder.JetpackEnabled = this.JetpackEnabled;
                objectBuilder.DampenersEnabled = this.DampenersEnabled;
                return objectBuilder;
            }

            public override Vector3D? GetStartingLocation()
            {
                if ((this.Transform != null) && MyPerGameSettings.CharacterStartsOnVoxel)
                {
                    return new Vector3D?(base.FixPositionToVoxel((Vector3D) this.Transform.Value.Position));
                }
                return null;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorPlayerStartingState builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform transform = builder as MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform;
                this.Transform = transform.Transform;
                this.JetpackEnabled = transform.JetpackEnabled;
                this.DampenersEnabled = transform.DampenersEnabled;
            }

            public override unsafe void SetupCharacter(MyWorldGenerator.Args generatorArgs)
            {
                if (MySession.Static.LocalHumanPlayer != null)
                {
                    MyObjectBuilder_Character objectBuilder = MyCharacter.Random();
                    if ((this.Transform == null) || !MyPerGameSettings.CharacterStartsOnVoxel)
                    {
                        objectBuilder.PositionAndOrientation = this.Transform;
                    }
                    else
                    {
                        MyPositionAndOrientation orientation = this.Transform.Value;
                        MyPositionAndOrientation* orientationPtr1 = (MyPositionAndOrientation*) ref orientation;
                        orientationPtr1->Position = base.FixPositionToVoxel((Vector3D) orientation.Position);
                        objectBuilder.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                    }
                    objectBuilder.JetpackEnabled = this.JetpackEnabled;
                    objectBuilder.DampenersEnabled = this.DampenersEnabled;
                    if (objectBuilder.Inventory == null)
                    {
                        objectBuilder.Inventory = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Inventory>();
                    }
                    MyWorldGenerator.FillInventoryWithDefaults(objectBuilder.Inventory, generatorArgs.Scenario);
                    MyCharacter entity = new MyCharacter {
                        Name = "Player"
                    };
                    entity.Init(objectBuilder);
                    MyEntities.RaiseEntityCreated(entity);
                    MyEntities.Add(entity, true);
                    this.CreateAndSetPlayerFaction();
                    MySession.Static.LocalHumanPlayer.SpawnIntoCharacter(entity);
                }
            }
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab))]
        public class OperationAddAsteroidPrefab : MyWorldGeneratorOperationBase
        {
            public string Name;
            public string PrefabName;
            public Vector3 Position;

            public override void Apply()
            {
                MyWorldGenerator.AddAsteroidPrefab(this.PrefabName, this.Position, this.Name);
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab;
                objectBuilder.Name = this.Name;
                objectBuilder.PrefabFile = this.PrefabName;
                objectBuilder.Position = this.Position;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab prefab = builder as MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab;
                this.Name = prefab.Name;
                this.PrefabName = prefab.PrefabFile;
                this.Position = (Vector3) prefab.Position;
            }
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab))]
        public class OperationAddObjectsPrefab : MyWorldGeneratorOperationBase
        {
            public string PrefabFile;

            public override void Apply()
            {
                MyWorldGenerator.AddObjectsPrefab(this.PrefabFile);
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab;
                objectBuilder.PrefabFile = this.PrefabFile;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab prefab = builder as MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab;
                this.PrefabFile = prefab.PrefabFile;
            }
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab))]
        public class OperationAddPlanetPrefab : MyWorldGeneratorOperationBase
        {
            public string PrefabName;
            public string DefinitionName;
            public Vector3D Position;
            public bool AddGPS;

            public override void Apply()
            {
                MyWorldGenerator.AddPlanetPrefab(this.PrefabName, this.DefinitionName, this.Position, this.AddGPS, true);
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab;
                objectBuilder.DefinitionName = this.DefinitionName;
                objectBuilder.PrefabName = this.PrefabName;
                objectBuilder.Position = this.Position;
                objectBuilder.AddGPS = this.AddGPS;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab prefab = builder as MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab;
                this.DefinitionName = prefab.DefinitionName;
                this.PrefabName = prefab.PrefabName;
                this.Position = (Vector3D) prefab.Position;
                this.AddGPS = prefab.AddGPS;
            }
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab))]
        public class OperationAddShipPrefab : MyWorldGeneratorOperationBase
        {
            public string PrefabFile;
            public bool UseFirstGridOrigin;
            public MyPositionAndOrientation Transform = MyPositionAndOrientation.Default;
            public float RandomRadius;

            public override void Apply()
            {
                MyFaction faction = null;
                if (base.FactionTag != null)
                {
                    faction = MySession.Static.Factions.TryGetOrCreateFactionByTag(base.FactionTag);
                }
                long ownerId = (faction != null) ? faction.FounderId : 0L;
                if (this.RandomRadius == 0f)
                {
                    MyPrefabManager.Static.AddShipPrefab(this.PrefabFile, new Matrix?((Matrix) this.Transform.GetMatrix()), ownerId, this.UseFirstGridOrigin);
                }
                else
                {
                    MyPrefabManager.Static.AddShipPrefabRandomPosition(this.PrefabFile, (Vector3D) this.Transform.Position, this.RandomRadius, ownerId, false);
                }
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab;
                objectBuilder.PrefabFile = this.PrefabFile;
                objectBuilder.Transform = this.Transform;
                objectBuilder.RandomRadius = this.RandomRadius;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab prefab = builder as MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab;
                this.PrefabFile = prefab.PrefabFile;
                this.UseFirstGridOrigin = prefab.UseFirstGridOrigin;
                this.Transform = prefab.Transform;
                this.RandomRadius = prefab.RandomRadius;
            }
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_CreatePlanet))]
        public class OperationCreatePlanet : MyWorldGeneratorOperationBase
        {
            public string DefinitionName;
            public bool AddGPS;
            public Vector3D PositionMinCorner;
            public Vector3D PositionCenter;
            public float Diameter;

            public override void Apply()
            {
                MyPlanetGeneratorDefinition generatorDef = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(this.DefinitionName));
                if (generatorDef == null)
                {
                    string msg = $"Definition for planet {this.DefinitionName} could not be found. Skipping.";
                    MyLog.Default.WriteLine(msg);
                }
                else
                {
                    Vector3D positionMinCorner = this.PositionMinCorner;
                    if (this.PositionCenter.IsValid())
                    {
                        positionMinCorner = this.PositionCenter - (MyVoxelCoordSystems.FindBestOctreeSize(this.Diameter * (1f + generatorDef.HillParams.Max)) / 2.0);
                    }
                    int seed = MyRandom.Instance.Next();
                    object[] objArray1 = new object[] { this.DefinitionName, "-", seed, "d", this.Diameter };
                    MyWorldGenerator.CreatePlanet(string.Concat(objArray1), generatorDef.FolderName, ref positionMinCorner, seed, this.Diameter, MyRandom.Instance.NextLong(), ref generatorDef, this.AddGPS, false, false);
                }
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_CreatePlanet objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_CreatePlanet;
                objectBuilder.DefinitionName = this.DefinitionName;
                objectBuilder.DefinitionName = this.DefinitionName;
                objectBuilder.AddGPS = this.AddGPS;
                objectBuilder.Diameter = this.Diameter;
                objectBuilder.PositionMinCorner = this.PositionMinCorner;
                objectBuilder.PositionCenter = this.PositionCenter;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_CreatePlanet planet = builder as MyObjectBuilder_WorldGeneratorOperation_CreatePlanet;
                this.DefinitionName = planet.DefinitionName;
                this.DefinitionName = planet.DefinitionName;
                this.AddGPS = planet.AddGPS;
                this.Diameter = planet.Diameter;
                this.PositionMinCorner = (Vector3D) planet.PositionMinCorner;
                this.PositionCenter = (Vector3D) planet.PositionCenter;
            }
        }

        public static class OperationFactory
        {
            private static MyObjectFactory<MyWorldGenerator.OperationTypeAttribute, MyWorldGeneratorOperationBase> m_objectFactory = new MyObjectFactory<MyWorldGenerator.OperationTypeAttribute, MyWorldGeneratorOperationBase>();

            static OperationFactory()
            {
                m_objectFactory.RegisterFromCreatedObjectAssembly();
                m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
                m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
                m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
            }

            public static MyWorldGeneratorOperationBase CreateInstance(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                MyWorldGeneratorOperationBase local1 = m_objectFactory.CreateInstance(builder.TypeId);
                local1.Init(builder);
                return local1;
            }

            public static MyObjectBuilder_WorldGeneratorOperation CreateObjectBuilder(MyWorldGeneratorOperationBase instance) => 
                m_objectFactory.CreateObjectBuilder<MyObjectBuilder_WorldGeneratorOperation>(instance);
        }

        [OperationType(typeof(MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab))]
        public class OperationSetupBasePrefab : MyWorldGeneratorOperationBase
        {
            public string PrefabFile;
            public Vector3 Offset;
            public string AsteroidName;
            public string BeaconName;

            public override void Apply()
            {
                MyFaction faction = null;
                if (base.FactionTag != null)
                {
                    faction = MySession.Static.Factions.TryGetOrCreateFactionByTag(base.FactionTag);
                }
                long ownerId = (faction != null) ? faction.FounderId : 0L;
                MyWorldGenerator.SetupBase(this.PrefabFile, this.Offset, this.AsteroidName, this.BeaconName, ownerId);
            }

            public override MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
            {
                MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab;
                objectBuilder.PrefabFile = this.PrefabFile;
                objectBuilder.Offset = this.Offset;
                objectBuilder.AsteroidName = this.AsteroidName;
                objectBuilder.BeaconName = this.BeaconName;
                return objectBuilder;
            }

            public override void Init(MyObjectBuilder_WorldGeneratorOperation builder)
            {
                base.Init(builder);
                MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab prefab = builder as MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab;
                this.PrefabFile = prefab.PrefabFile;
                this.Offset = (Vector3) prefab.Offset;
                this.AsteroidName = prefab.AsteroidName;
                this.BeaconName = prefab.BeaconName;
            }
        }

        public class OperationTypeAttribute : MyFactoryTagAttribute
        {
            public OperationTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
            {
            }
        }

        public static class StartingStateFactory
        {
            private static MyObjectFactory<MyWorldGenerator.StartingStateTypeAttribute, MyWorldGeneratorStartingStateBase> m_objectFactory = new MyObjectFactory<MyWorldGenerator.StartingStateTypeAttribute, MyWorldGeneratorStartingStateBase>();

            static StartingStateFactory()
            {
                m_objectFactory.RegisterFromCreatedObjectAssembly();
                m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
                m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
                m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
            }

            public static MyWorldGeneratorStartingStateBase CreateInstance(MyObjectBuilder_WorldGeneratorPlayerStartingState builder)
            {
                MyWorldGeneratorStartingStateBase base2 = m_objectFactory.CreateInstance(builder.TypeId);
                if (base2 != null)
                {
                    base2.Init(builder);
                }
                return base2;
            }

            public static MyObjectBuilder_WorldGeneratorPlayerStartingState CreateObjectBuilder(MyWorldGeneratorStartingStateBase instance) => 
                m_objectFactory.CreateObjectBuilder<MyObjectBuilder_WorldGeneratorPlayerStartingState>(instance);
        }

        public class StartingStateTypeAttribute : MyFactoryTagAttribute
        {
            public StartingStateTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
            {
            }
        }
    }
}


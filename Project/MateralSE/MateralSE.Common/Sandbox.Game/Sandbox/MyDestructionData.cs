namespace Sandbox
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Fractures;
    using VRageRender.Messages;
    using VRageRender.Models;
    using VRageRender.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyDestructionData : MySessionComponentBase
    {
        private static List<HkdShapeInstanceInfo> m_tmpChildrenList = new List<HkdShapeInstanceInfo>();
        private static MyPhysicsMesh m_tmpMesh = new MyPhysicsMesh();
        private HkDestructionStorage Storage;
        private static Dictionary<string, MyPhysicalMaterialDefinition> m_physicalMaterials;

        private bool CheckVolumeMassRec(HkdBreakableShape bShape, float minVolume, float minMass)
        {
            if (!bShape.Name.Contains("Fake"))
            {
                if (bShape.Volume <= minVolume)
                {
                    return false;
                }
                HkMassProperties massProperties = new HkMassProperties();
                bShape.BuildMassProperties(ref massProperties);
                if (massProperties.Mass <= minMass)
                {
                    return false;
                }
                if (((massProperties.InertiaTensor.M11 == 0f) || (massProperties.InertiaTensor.M22 == 0f)) || (massProperties.InertiaTensor.M33 == 0f))
                {
                    return false;
                }
                for (int i = 0; i < bShape.GetChildrenCount(); i++)
                {
                    if (!this.CheckVolumeMassRec(bShape.GetChildShape(i), minVolume, minMass))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void CreateBreakableShapeFromCollisionShapes(MyModel model, Vector3 defaultSize, MyPhysicalModelDefinition modelDef)
        {
            HkShape shape;
            if ((model.HavokCollisionShapes == null) || (model.HavokCollisionShapes.Length == 0))
            {
                shape = (HkShape) new HkBoxShape(defaultSize * 0.5f, MyPerGameSettings.PhysicsConvexRadius);
            }
            else if (model.HavokCollisionShapes.Length > 1)
            {
                shape = (HkShape) HkListShape.Create<HkShape>(model.HavokCollisionShapes, model.HavokCollisionShapes.Length, HkReferencePolicy.None);
            }
            else
            {
                shape = model.HavokCollisionShapes[0];
                shape.AddReference();
            }
            HkdBreakableShape shape2 = new HkdBreakableShape(shape) {
                Name = model.AssetName
            };
            shape2.SetMass(modelDef.Mass);
            model.HavokBreakableShapes = new HkdBreakableShape[] { shape2 };
            shape.RemoveReference();
        }

        private HkReferenceObject CreateGeometryFromSplitPlane(string splitPlane)
        {
            MyModel modelOnlyData = MyModels.GetModelOnlyData(splitPlane);
            if (modelOnlyData == null)
            {
                return null;
            }
            IPhysicsMesh graphicsData = this.CreatePhysicsMesh(modelOnlyData);
            return this.Storage.CreateGeometry(graphicsData, Path.GetFileNameWithoutExtension(splitPlane));
        }

        private IPhysicsMesh CreatePhysicsMesh(MyModel model)
        {
            IPhysicsMesh mesh = new MyPhysicsMesh();
            mesh.SetAABB(model.BoundingBox.Min, model.BoundingBox.Max);
            for (int i = 0; i < model.GetVerticesCount(); i++)
            {
                Vector3 vertex = model.GetVertex(i);
                Vector3 vertexNormal = model.GetVertexNormal(i);
                Vector3 vertexTangent = model.GetVertexTangent(i);
                if (model.TexCoords == null)
                {
                    model.LoadTexCoordData();
                }
                mesh.AddVertex(vertex, vertexNormal, vertexTangent, model.TexCoords[i].ToVector2());
            }
            for (int j = 0; j < model.Indices16.Length; j++)
            {
                mesh.AddIndex(model.Indices16[j]);
            }
            for (int k = 0; k < model.GetMeshList().Count; k++)
            {
                VRageRender.Models.MyMesh mesh2 = model.GetMeshList()[k];
                mesh.AddSectionData(mesh2.IndexStart, mesh2.TriCount, mesh2.Material.Name);
            }
            return mesh;
        }

        private void CreatePieceData(MyModel model, HkdBreakableShape breakableShape)
        {
            MyRenderMessageAddRuntimeModel message = MyRenderProxy.PrepareAddRuntimeModel();
            m_tmpMesh.Data = message.ModelData;
            Static.Storage.GetDataFromShape(breakableShape, m_tmpMesh);
            if (message.ModelData.Sections.Count > 0)
            {
                if (MyFakes.USE_HAVOK_MODELS)
                {
                    message.ReplacedModel = model.AssetName;
                }
                MyRenderProxy.AddRuntimeModel(breakableShape.ShapeName, message);
            }
            using (m_tmpChildrenList.GetClearToken<HkdShapeInstanceInfo>())
            {
                breakableShape.GetChildren(m_tmpChildrenList);
                LoadChildrenShapes(m_tmpChildrenList);
            }
        }

        private void DisableRefCountRec(HkdBreakableShape bShape)
        {
            bShape.DisableRefCount();
            List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
            bShape.GetChildren(list);
            foreach (HkdShapeInstanceInfo info in list)
            {
                this.DisableRefCountRec(info.Shape);
            }
        }

        private void FractureBreakableShape(HkdBreakableShape bShape, MyModelFractures modelFractures, string modPath)
        {
            HkdFracture fracture = null;
            HkReferenceObject data = null;
            if (modelFractures.Fractures[0] is RandomSplitFractureSettings)
            {
                RandomSplitFractureSettings settings = (RandomSplitFractureSettings) modelFractures.Fractures[0];
                HkdRandomSplitFracture fracture1 = new HkdRandomSplitFracture();
                fracture1.NumObjectsOnLevel1 = settings.NumObjectsOnLevel1;
                fracture1.NumObjectsOnLevel2 = settings.NumObjectsOnLevel2;
                fracture1.RandomRange = settings.RandomRange;
                fracture1.RandomSeed1 = settings.RandomSeed1;
                fracture1.RandomSeed2 = settings.RandomSeed2;
                fracture1.SplitGeometryScale = Vector4.One;
                fracture = fracture1;
                if (!string.IsNullOrEmpty(settings.SplitPlane))
                {
                    string splitPlane = settings.SplitPlane;
                    if (!string.IsNullOrEmpty(modPath))
                    {
                        splitPlane = Path.Combine(modPath, settings.SplitPlane);
                    }
                    data = this.CreateGeometryFromSplitPlane(splitPlane);
                    if (data != null)
                    {
                        ((HkdRandomSplitFracture) fracture).SetGeometry(data);
                        MyRenderProxy.PreloadMaterials(splitPlane);
                    }
                }
            }
            if (modelFractures.Fractures[0] is VoronoiFractureSettings)
            {
                VoronoiFractureSettings settings2 = (VoronoiFractureSettings) modelFractures.Fractures[0];
                HkdVoronoiFracture fracture2 = new HkdVoronoiFracture();
                fracture2.Seed = settings2.Seed;
                fracture2.NumSitesToGenerate = settings2.NumSitesToGenerate;
                fracture2.NumIterations = settings2.NumIterations;
                fracture = fracture2;
                if (!string.IsNullOrEmpty(settings2.SplitPlane))
                {
                    string splitPlane = settings2.SplitPlane;
                    if (!string.IsNullOrEmpty(modPath))
                    {
                        splitPlane = Path.Combine(modPath, settings2.SplitPlane);
                    }
                    data = this.CreateGeometryFromSplitPlane(splitPlane);
                    MyModels.GetModel(splitPlane);
                    if (data != null)
                    {
                        ((HkdVoronoiFracture) fracture).SetGeometry(data);
                        MyRenderProxy.PreloadMaterials(splitPlane);
                    }
                }
            }
            if (modelFractures.Fractures[0] is WoodFractureSettings)
            {
                WoodFractureSettings settings1 = (WoodFractureSettings) modelFractures.Fractures[0];
                fracture = new HkdWoodFracture();
            }
            if (fracture != null)
            {
                this.Storage.FractureShape(bShape, fracture);
                fracture.Dispose();
            }
            if (data != null)
            {
                data.Dispose();
            }
        }

        public float GetBlockMass(string model, MyCubeBlockDefinition def)
        {
            HkdBreakableShape breakableShape = this.BlockShapePool.GetBreakableShape(model, def);
            this.BlockShapePool.EnqueShape(model, def.Id, breakableShape);
            return breakableShape.GetMass();
        }

        public static MyPhysicalMaterialDefinition GetPhysicalMaterial(MyPhysicalModelDefinition modelDef, string physicalMaterial)
        {
            if (m_physicalMaterials == null)
            {
                m_physicalMaterials = new Dictionary<string, MyPhysicalMaterialDefinition>();
                foreach (MyPhysicalMaterialDefinition definition in MyDefinitionManager.Static.GetPhysicalMaterialDefinitions())
                {
                    m_physicalMaterials.Add(definition.Id.SubtypeName, definition);
                }
                MyPhysicalMaterialDefinition definition1 = new MyPhysicalMaterialDefinition();
                definition1.Density = 1920f;
                definition1.HorisontalTransmissionMultiplier = 1f;
                definition1.HorisontalFragility = 2f;
                definition1.CollisionMultiplier = 1.4f;
                definition1.SupportMultiplier = 1.5f;
                m_physicalMaterials["Default"] = definition1;
            }
            if (!string.IsNullOrEmpty(physicalMaterial))
            {
                if (m_physicalMaterials.ContainsKey(physicalMaterial))
                {
                    return m_physicalMaterials[physicalMaterial];
                }
                string msg = "ERROR: Physical material " + physicalMaterial + " does not exist!";
                MyLog.Default.WriteLine(msg);
            }
            if (modelDef.Id.SubtypeName.Contains("Stone") && m_physicalMaterials.ContainsKey("Stone"))
            {
                return m_physicalMaterials["Stone"];
            }
            if ((!modelDef.Id.SubtypeName.Contains("Wood") || !m_physicalMaterials.ContainsKey("Wood")) && (!modelDef.Id.SubtypeName.Contains("Timber") || !m_physicalMaterials.ContainsKey("Timber")))
            {
                return m_physicalMaterials["Default"];
            }
            return m_physicalMaterials["Wood"];
        }

        private static void LoadChildrenShapes(List<HkdShapeInstanceInfo> children)
        {
            foreach (HkdShapeInstanceInfo info in children)
            {
                if (info.IsValid())
                {
                    MyRenderMessageAddRuntimeModel message = MyRenderProxy.PrepareAddRuntimeModel();
                    m_tmpMesh.Data = message.ModelData;
                    Static.Storage.GetDataFromShapeInstance(info, m_tmpMesh);
                    m_tmpMesh.Transform(info.GetTransform());
                    if (message.ModelData.Sections.Count > 0)
                    {
                        MyRenderProxy.AddRuntimeModel(info.ShapeName, message);
                    }
                    List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                    info.GetChildren(list);
                    LoadChildrenShapes(list);
                }
            }
        }

        public override void LoadData()
        {
            if (!HkBaseSystem.DestructionEnabled)
            {
                MyLog.Default.WriteLine("Havok Destruction is not availiable in this build.");
                throw new InvalidOperationException("Havok Destruction is not availiable in this build.");
            }
            if (Static != null)
            {
                MyLog.Default.WriteLine("Destruction data was not freed. Unloading now...");
                this.UnloadData();
            }
            Static = this;
            this.BlockShapePool = new MyBlockShapePool();
            this.TemporaryWorld = new HkWorld(true, 50000f, MyPhysics.RestingVelocity, MyFakes.ENABLE_HAVOK_MULTITHREADING, 4);
            this.TemporaryWorld.MarkForWrite();
            this.TemporaryWorld.DestructionWorld = new HkdWorld(this.TemporaryWorld);
            this.TemporaryWorld.UnmarkForWrite();
            this.Storage = new HkDestructionStorage(this.TemporaryWorld.DestructionWorld);
            foreach (string str in MyDefinitionManager.Static.GetDefinitionPairNames())
            {
                MyCubeBlockDefinition.BuildProgressModel[] buildProgressModels;
                int num;
                MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(str);
                if (definitionGroup.Large != null)
                {
                    MyModel model = MyModels.GetModel(definitionGroup.Large.Model);
                    if (model == null)
                    {
                        continue;
                    }
                    if (!MyFakes.LAZY_LOAD_DESTRUCTION || ((model != null) && (model.HavokBreakableShapes != null)))
                    {
                        this.LoadModelDestruction(definitionGroup.Large.Model, definitionGroup.Large, (Vector3) (definitionGroup.Large.Size * MyDefinitionManager.Static.GetCubeSize(definitionGroup.Large.CubeSize)), true, false);
                    }
                    buildProgressModels = definitionGroup.Large.BuildProgressModels;
                    num = 0;
                    while (true)
                    {
                        if (num >= buildProgressModels.Length)
                        {
                            if ((MyFakes.CHANGE_BLOCK_CONVEX_RADIUS && (model != null)) && (model.HavokBreakableShapes != null))
                            {
                                HkShape shape = model.HavokBreakableShapes[0].GetShape();
                                if ((shape.ShapeType != HkShapeType.Sphere) && (shape.ShapeType != HkShapeType.Capsule))
                                {
                                    this.SetConvexRadius(model.HavokBreakableShapes[0], 0.05f);
                                }
                            }
                            break;
                        }
                        MyCubeBlockDefinition.BuildProgressModel model2 = buildProgressModels[num];
                        model = MyModels.GetModel(model2.File);
                        if ((model != null) && (!MyFakes.LAZY_LOAD_DESTRUCTION || ((model != null) && (model.HavokBreakableShapes != null))))
                        {
                            this.LoadModelDestruction(model2.File, definitionGroup.Large, (Vector3) (definitionGroup.Large.Size * MyDefinitionManager.Static.GetCubeSize(definitionGroup.Large.CubeSize)), true, false);
                        }
                        num++;
                    }
                }
                if (definitionGroup.Small != null)
                {
                    MyModel model = MyModels.GetModel(definitionGroup.Small.Model);
                    if (model != null)
                    {
                        if (!MyFakes.LAZY_LOAD_DESTRUCTION || ((model != null) && (model.HavokBreakableShapes != null)))
                        {
                            this.LoadModelDestruction(definitionGroup.Small.Model, definitionGroup.Small, (Vector3) (definitionGroup.Small.Size * MyDefinitionManager.Static.GetCubeSize(definitionGroup.Small.CubeSize)), true, false);
                        }
                        buildProgressModels = definitionGroup.Small.BuildProgressModels;
                        num = 0;
                        while (true)
                        {
                            if (num >= buildProgressModels.Length)
                            {
                                if ((MyFakes.CHANGE_BLOCK_CONVEX_RADIUS && (model != null)) && (model.HavokBreakableShapes != null))
                                {
                                    HkShape shape = model.HavokBreakableShapes[0].GetShape();
                                    if ((shape.ShapeType != HkShapeType.Sphere) && (shape.ShapeType != HkShapeType.Capsule))
                                    {
                                        this.SetConvexRadius(model.HavokBreakableShapes[0], 0.05f);
                                    }
                                }
                                break;
                            }
                            MyCubeBlockDefinition.BuildProgressModel model4 = buildProgressModels[num];
                            model = MyModels.GetModel(model4.File);
                            if ((model != null) && (!MyFakes.LAZY_LOAD_DESTRUCTION || ((model != null) && (model.HavokBreakableShapes != null))))
                            {
                                this.LoadModelDestruction(model4.File, definitionGroup.Small, (Vector3) (definitionGroup.Large.Size * MyDefinitionManager.Static.GetCubeSize(definitionGroup.Large.CubeSize)), true, false);
                            }
                            num++;
                        }
                    }
                }
            }
            if (!MyFakes.LAZY_LOAD_DESTRUCTION)
            {
                this.BlockShapePool.Preallocate();
            }
            foreach (MyPhysicalModelDefinition definition in MyDefinitionManager.Static.GetAllDefinitions<MyPhysicalModelDefinition>())
            {
                this.LoadModelDestruction(definition.Model, definition, Vector3.One, false, true);
            }
        }

        public void LoadModelDestruction(string modelName, MyPhysicalModelDefinition modelDef, Vector3 defaultSize, bool destructionRequired = true, bool useShapeVolume = false)
        {
            MyModel modelOnlyData = MyModels.GetModelOnlyData(modelName);
            if (modelOnlyData.HavokBreakableShapes == null)
            {
                bool flag = false;
                MyCubeBlockDefinition definition = modelDef as MyCubeBlockDefinition;
                if (definition != null)
                {
                    flag = !definition.CreateFracturedPieces;
                }
                MyPhysicalMaterialDefinition physicalMaterial = modelDef.PhysicalMaterial;
                string shapeName = modelName;
                if (modelOnlyData != null)
                {
                    bool flag2 = false;
                    modelOnlyData.LoadUV = true;
                    bool flag3 = false;
                    bool flag4 = false;
                    bool flag5 = false;
                    if (modelOnlyData.ModelFractures == null)
                    {
                        if ((modelOnlyData.HavokDestructionData != null) && !flag2)
                        {
                            try
                            {
                                if (modelOnlyData.HavokBreakableShapes == null)
                                {
                                    modelOnlyData.HavokBreakableShapes = this.Storage.LoadDestructionDataFromBuffer(modelOnlyData.HavokDestructionData);
                                    flag3 = true;
                                    flag4 = true;
                                    flag5 = true;
                                }
                            }
                            catch
                            {
                                modelOnlyData.HavokBreakableShapes = null;
                            }
                        }
                    }
                    else if ((modelOnlyData.HavokCollisionShapes != null) && (modelOnlyData.HavokCollisionShapes.Length != 0))
                    {
                        this.CreateBreakableShapeFromCollisionShapes(modelOnlyData, defaultSize, modelDef);
                        IPhysicsMesh graphicsData = this.CreatePhysicsMesh(modelOnlyData);
                        this.Storage.RegisterShapeWithGraphics(graphicsData, modelOnlyData.HavokBreakableShapes[0], shapeName);
                        string modPath = null;
                        if (Path.IsPathRooted(modelOnlyData.AssetName))
                        {
                            modPath = modelOnlyData.AssetName.Remove(modelOnlyData.AssetName.LastIndexOf("Models"));
                        }
                        this.FractureBreakableShape(modelOnlyData.HavokBreakableShapes[0], modelOnlyData.ModelFractures, modPath);
                        flag4 = true;
                        flag5 = true;
                        flag3 = true;
                    }
                    modelOnlyData.HavokDestructionData = null;
                    modelOnlyData.HavokData = null;
                    if ((modelOnlyData.HavokBreakableShapes == null) & destructionRequired)
                    {
                        MyLog.Default.WriteLine(modelOnlyData.AssetName + " does not have destruction data");
                        this.CreateBreakableShapeFromCollisionShapes(modelOnlyData, defaultSize, modelDef);
                        flag4 = true;
                        flag5 = true;
                    }
                    if (modelOnlyData.HavokBreakableShapes == null)
                    {
                        MyLog.Default.WriteLine($"Model {modelOnlyData.AssetName} - Unable to load havok destruction data", LoggingOptions.LOADING_MODELS);
                    }
                    else
                    {
                        HkdBreakableShape shape = modelOnlyData.HavokBreakableShapes[0];
                        if (flag)
                        {
                            shape.SetFlagRecursively(HkdBreakableShape.Flags.DONT_CREATE_FRACTURE_PIECE);
                        }
                        if (flag5)
                        {
                            shape.AddReference();
                            this.Storage.RegisterShape(shape, shapeName);
                        }
                        MyRenderProxy.PreloadMaterials(modelOnlyData.AssetName);
                        if (flag3)
                        {
                            this.CreatePieceData(modelOnlyData, shape);
                        }
                        if (flag4)
                        {
                            float volume = shape.CalculateGeometryVolume();
                            if (!(volume != 0f) | useShapeVolume)
                            {
                                volume = shape.Volume;
                            }
                            shape.SetMassRecursively(MyDestructionHelper.MassToHavok(volume * physicalMaterial.Density));
                        }
                        if (modelDef.Mass > 0f)
                        {
                            shape.SetMassRecursively(MyDestructionHelper.MassToHavok(modelDef.Mass));
                        }
                        this.DisableRefCountRec(shape);
                        if ((MyFakes.CHANGE_BLOCK_CONVEX_RADIUS && (modelOnlyData != null)) && (modelOnlyData.HavokBreakableShapes != null))
                        {
                            HkShape shape2 = modelOnlyData.HavokBreakableShapes[0].GetShape();
                            if ((shape2.ShapeType != HkShapeType.Sphere) && (shape2.ShapeType != HkShapeType.Capsule))
                            {
                                this.SetConvexRadius(modelOnlyData.HavokBreakableShapes[0], 0.05f);
                            }
                        }
                        if (MyFakes.LAZY_LOAD_DESTRUCTION)
                        {
                            this.BlockShapePool.AllocateForDefinition(shapeName, modelDef, 50);
                        }
                    }
                }
            }
        }

        private void SetConvexRadius(HkdBreakableShape bShape, float radius)
        {
            HkShape shape = bShape.GetShape();
            if (shape.IsConvex)
            {
                HkConvexShape shape2 = (HkConvexShape) shape;
                if (shape2.ConvexRadius > radius)
                {
                    shape2.ConvexRadius = radius;
                }
            }
            else if (shape.IsContainer())
            {
                HkShapeContainerIterator container = shape.GetContainer();
                while (container.IsValid)
                {
                    HkShape currentValue = container.CurrentValue;
                    if (currentValue.IsConvex)
                    {
                        HkConvexShape shape4 = (HkConvexShape) container.CurrentValue;
                        if (shape4.ConvexRadius > radius)
                        {
                            shape4.ConvexRadius = radius;
                        }
                    }
                    container.Next();
                }
            }
        }

        protected override void UnloadData()
        {
            this.TemporaryWorld.MarkForWrite();
            this.Storage.Dispose();
            this.Storage = null;
            this.TemporaryWorld.DestructionWorld.Dispose();
            this.TemporaryWorld.Dispose();
            this.TemporaryWorld = null;
            this.BlockShapePool.Free();
            this.BlockShapePool = null;
            Static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.BlockShapePool.RefillPools();
        }

        public static MyDestructionData Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }

        public HkWorld TemporaryWorld { get; private set; }

        public MyBlockShapePool BlockShapePool { get; private set; }

        public override bool IsRequiredByGame =>
            MyPerGameSettings.Destruction;
    }
}


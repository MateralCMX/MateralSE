namespace Sandbox.Definitions
{
    using Sandbox.Game;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Generics;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyDefinitionType(typeof(MyObjectBuilder_CubeBlockDefinition), (Type) null)]
    public class MyCubeBlockDefinition : MyPhysicalModelDefinition
    {
        public MyCubeSize CubeSize;
        public MyBlockTopology BlockTopology = MyBlockTopology.TriangleMesh;
        public Vector3I Size;
        public Vector3 ModelOffset;
        public bool UseModelIntersection;
        public MyCubeDefinition CubeDefinition;
        public bool SilenceableByShipSoundSystem;
        public Component[] Components;
        public ushort CriticalGroup;
        public float CriticalIntegrityRatio;
        public float OwnershipIntegrityRatio;
        public float MaxIntegrityRatio;
        public float MaxIntegrity;
        public int? DamageEffectID;
        public string DamageEffectName = string.Empty;
        public string DestroyEffect = "";
        public Vector3? DestroyEffectOffset;
        public MySoundPair DestroySound = MySoundPair.Empty;
        public Sandbox.Definitions.CubeBlockEffectBase[] Effects;
        public MountPoint[] MountPoints;
        public Dictionary<Vector3I, Dictionary<Vector3I, bool>> IsCubePressurized;
        public MyBlockNavigationDefinition NavigationDefinition;
        public VRageMath.Color Color;
        public List<MyCubeBlockDefinition> Variants = new List<MyCubeBlockDefinition>();
        public MyCubeBlockDefinition UniqueVersion;
        public MyPhysicsOption PhysicsOption;
        public MyStringId? DisplayNameVariant;
        public string BlockPairName;
        public bool UsesDeformation;
        public float DeformationRatio;
        public float IntegrityPointsPerSec;
        public string EdgeType;
        public List<VRage.Game.BoneInfo> Skeleton;
        public Dictionary<Vector3I, Vector3> Bones;
        public bool? IsAirTight;
        public bool IsStandAlone = true;
        public bool HasPhysics = true;
        public bool UseNeighbourOxygenRooms;
        public MyStringId BuildType;
        public string BuildMaterial;
        public MyDefinitionId[] GeneratedBlockDefinitions;
        public MyStringId GeneratedBlockType;
        public float BuildProgressToPlaceGeneratedBlocks;
        public bool CreateFracturedPieces;
        public MyStringHash EmissiveColorPreset = MyStringHash.NullOrEmpty;
        public string[] CompoundTemplates;
        public bool CompoundEnabled;
        public string MultiBlock;
        public Dictionary<string, MyDefinitionId> SubBlockDefinitions;
        public MyDefinitionId[] BlockStages;
        public BuildProgressModel[] BuildProgressModels;
        private Vector3I m_center;
        private MySymmetryAxisEnum m_symmetryX;
        private MySymmetryAxisEnum m_symmetryY;
        private MySymmetryAxisEnum m_symmetryZ;
        private StringBuilder m_displayNameTextCache;
        public float DisassembleRatio;
        public MyAutorotateMode AutorotateMode;
        private string m_mirroringBlock;
        public MySoundPair PrimarySound;
        public MySoundPair ActionSound;
        public MySoundPair DamagedSound;
        public int PCU;
        public static readonly int PCU_CONSTRUCTION_STAGE_COST = 1;
        public bool PlaceDecals;
        public Dictionary<string, MyObjectBuilder_ComponentBase> EntityComponents;
        public VoxelPlacementOverride? VoxelPlacement;
        public float GeneralDamageMultiplier;
        private static Matrix[] m_mountPointTransforms = new Matrix[] { (Matrix.CreateFromDir(Vector3.Right, Vector3.Up) * Matrix.CreateScale(1f, 1f, -1f)), (Matrix.CreateFromDir(Vector3.Up, Vector3.Forward) * Matrix.CreateScale(-1f, 1f, 1f)), (Matrix.CreateFromDir(Vector3.Forward, Vector3.Up) * Matrix.CreateScale(-1f, 1f, 1f)), (Matrix.CreateFromDir(Vector3.Left, Vector3.Up) * Matrix.CreateScale(1f, 1f, -1f)), (Matrix.CreateFromDir(Vector3.Down, Vector3.Backward) * Matrix.CreateScale(-1f, 1f, 1f)), (Matrix.CreateFromDir(Vector3.Backward, Vector3.Up) * Matrix.CreateScale(-1f, 1f, 1f)) };
        private static Vector3[] m_mountPointWallOffsets = new Vector3[] { new Vector3(1f, 0f, 1f), new Vector3(0f, 1f, 1f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f) };
        private static int[] m_mountPointWallIndices = new int[] { 2, 5, 3, 0, 1, 4 };
        private const float OFFSET_CONST = 0.001f;
        private const float THICKNESS_HALF = 0.0004f;
        private static List<int> m_tmpIndices = new List<int>();
        private static List<MyObjectBuilder_CubeBlockDefinition.MountPoint> m_tmpMounts = new List<MyObjectBuilder_CubeBlockDefinition.MountPoint>();
        private static readonly MyDynamicObjectPool<List<string>> m_stringPool = new MyDynamicObjectPool<List<string>>(10);
        private static readonly HashSet<MyCubeBlockDefinition> m_preloadedDefinitions = new HashSet<MyCubeBlockDefinition>();

        [Conditional("DEBUG")]
        private void CheckBuildProgressModels()
        {
            if (this.BuildProgressModels != null)
            {
                foreach (BuildProgressModel model in this.BuildProgressModels)
                {
                    if (model != null)
                    {
                        string file = model.File;
                        if (!Path.IsPathRooted(file))
                        {
                            Path.Combine(MyFileSystem.ContentPath, file);
                        }
                    }
                }
            }
        }

        public static void ClearPreloadedConstructionModels()
        {
            m_preloadedDefinitions.Clear();
        }

        public bool ContainsComputer() => 
            (this.Components.Count<Component>(x => ((x.Definition.Id.TypeId == typeof(MyObjectBuilder_Component)) && (x.Definition.Id.SubtypeName == "Computer"))) > 0);

        public float FinalModelThreshold()
        {
            if ((this.BuildProgressModels == null) || (this.BuildProgressModels.Length == 0))
            {
                return 0f;
            }
            return this.BuildProgressModels[this.BuildProgressModels.Length - 1].BuildRatioUpperBound;
        }

        public MountPoint[] GetBuildProgressModelMountPoints(float currentIntegrityRatio)
        {
            if (((this.BuildProgressModels == null) || (this.BuildProgressModels.Length == 0)) || (currentIntegrityRatio >= this.BuildProgressModels[this.BuildProgressModels.Length - 1].BuildRatioUpperBound))
            {
                return this.MountPoints;
            }
            int index = 0;
            while (true)
            {
                if (index < (this.BuildProgressModels.Length - 1))
                {
                    BuildProgressModel model = this.BuildProgressModels[index];
                    if (currentIntegrityRatio > model.BuildRatioUpperBound)
                    {
                        index++;
                        continue;
                    }
                }
                return (this.BuildProgressModels[index].MountPoints ?? this.MountPoints);
            }
        }

        public MyCubeBlockDefinition GetGeneratedBlockDefinition(MyStringId additionalModelType)
        {
            if (this.GeneratedBlockDefinitions != null)
            {
                foreach (MyDefinitionId id in this.GeneratedBlockDefinitions)
                {
                    MyCubeBlockDefinition definition;
                    MyDefinitionManager.Static.TryGetCubeBlockDefinition(id, out definition);
                    if (((definition != null) && definition.IsGeneratedBlock) && (definition.GeneratedBlockType == additionalModelType))
                    {
                        return definition;
                    }
                }
            }
            return null;
        }

        public static int GetMountPointWallIndex(VRageMath.Base6Directions.Direction direction) => 
            m_mountPointWallIndices[(int) direction];

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            int num;
            MyObjectBuilder_CubeBlockDefinition objectBuilder = (MyObjectBuilder_CubeBlockDefinition) base.GetObjectBuilder();
            objectBuilder.Size = this.Size;
            objectBuilder.Model = base.Model;
            objectBuilder.UseModelIntersection = this.UseModelIntersection;
            objectBuilder.CubeSize = this.CubeSize;
            objectBuilder.SilenceableByShipSoundSystem = this.SilenceableByShipSoundSystem;
            objectBuilder.ModelOffset = this.ModelOffset;
            objectBuilder.BlockTopology = this.BlockTopology;
            objectBuilder.PhysicsOption = this.PhysicsOption;
            objectBuilder.BlockPairName = this.BlockPairName;
            objectBuilder.Center = new SerializableVector3I?(this.m_center);
            objectBuilder.MirroringX = this.m_symmetryX;
            objectBuilder.MirroringY = this.m_symmetryY;
            objectBuilder.MirroringZ = this.m_symmetryZ;
            objectBuilder.UsesDeformation = this.UsesDeformation;
            objectBuilder.DeformationRatio = this.DeformationRatio;
            objectBuilder.EdgeType = this.EdgeType;
            objectBuilder.AutorotateMode = this.AutorotateMode;
            objectBuilder.MirroringBlock = this.m_mirroringBlock;
            objectBuilder.MultiBlock = this.MultiBlock;
            objectBuilder.GuiVisible = this.GuiVisible;
            objectBuilder.Rotation = this.Rotation;
            objectBuilder.Direction = this.Direction;
            objectBuilder.Mirrored = this.Mirrored;
            objectBuilder.BuildType = this.BuildType.ToString();
            objectBuilder.BuildMaterial = this.BuildMaterial;
            objectBuilder.GeneratedBlockType = this.GeneratedBlockType.ToString();
            objectBuilder.DamageEffectName = this.DamageEffectName;
            objectBuilder.DestroyEffect = (this.DestroyEffect.Length > 0) ? this.DestroyEffect : "";
            objectBuilder.DestroyEffectOffset = this.DestroyEffectOffset;
            objectBuilder.Icons = base.Icons;
            objectBuilder.VoxelPlacement = this.VoxelPlacement;
            objectBuilder.GeneralDamageMultiplier = this.GeneralDamageMultiplier;
            if (base.PhysicalMaterial != null)
            {
                objectBuilder.PhysicalMaterial = base.PhysicalMaterial.Id.SubtypeName;
            }
            objectBuilder.CompoundEnabled = this.CompoundEnabled;
            objectBuilder.PCU = this.PCU;
            objectBuilder.PlaceDecals = this.PlaceDecals;
            if (this.Components != null)
            {
                List<MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent> list2 = new List<MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent>();
                Component[] components = this.Components;
                num = 0;
                while (true)
                {
                    if (num >= components.Length)
                    {
                        objectBuilder.Components = list2.ToArray();
                        break;
                    }
                    Component component = components[num];
                    MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent item = new MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent {
                        Count = (ushort) component.Count,
                        Type = component.Definition.Id.TypeId,
                        Subtype = component.Definition.Id.SubtypeName
                    };
                    list2.Add(item);
                    num++;
                }
            }
            MyObjectBuilder_CubeBlockDefinition.CriticalPart part1 = new MyObjectBuilder_CubeBlockDefinition.CriticalPart();
            part1.Index = 0;
            part1.Subtype = objectBuilder.Components[0].Subtype;
            part1.Type = objectBuilder.Components[0].Type;
            objectBuilder.CriticalComponent = part1;
            List<MyObjectBuilder_CubeBlockDefinition.MountPoint> list = null;
            if (this.MountPoints != null)
            {
                list = new List<MyObjectBuilder_CubeBlockDefinition.MountPoint>();
                MountPoint[] mountPoints = this.MountPoints;
                num = 0;
                while (true)
                {
                    if (num >= mountPoints.Length)
                    {
                        objectBuilder.MountPoints = list.ToArray();
                        break;
                    }
                    MyObjectBuilder_CubeBlockDefinition.MountPoint item = mountPoints[num].GetObjectBuilder(this.Size);
                    list.Add(item);
                    num++;
                }
            }
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CubeBlockDefinition def = builder as MyObjectBuilder_CubeBlockDefinition;
            this.Size = (Vector3I) def.Size;
            base.Model = def.Model;
            this.UseModelIntersection = def.UseModelIntersection;
            this.CubeSize = def.CubeSize;
            this.ModelOffset = (Vector3) def.ModelOffset;
            this.BlockTopology = def.BlockTopology;
            this.PhysicsOption = def.PhysicsOption;
            this.BlockPairName = def.BlockPairName;
            SerializableVector3I? center = def.Center;
            this.m_center = (center != null) ? ((Vector3I) center.GetValueOrDefault()) : ((Vector3I) ((this.Size - 1) / 2));
            this.m_symmetryX = def.MirroringX;
            this.m_symmetryY = def.MirroringY;
            this.m_symmetryZ = def.MirroringZ;
            this.UsesDeformation = def.UsesDeformation;
            this.DeformationRatio = def.DeformationRatio;
            this.SilenceableByShipSoundSystem = def.SilenceableByShipSoundSystem;
            this.EdgeType = def.EdgeType;
            this.AutorotateMode = def.AutorotateMode;
            this.m_mirroringBlock = def.MirroringBlock;
            this.MultiBlock = def.MultiBlock;
            this.GuiVisible = def.GuiVisible;
            this.Rotation = def.Rotation;
            this.Direction = def.Direction;
            this.Mirrored = def.Mirrored;
            this.RandomRotation = def.RandomRotation;
            this.BuildType = MyStringId.GetOrCompute(def.BuildType?.ToLower());
            this.BuildMaterial = def.BuildMaterial?.ToLower();
            this.BuildProgressToPlaceGeneratedBlocks = def.BuildProgressToPlaceGeneratedBlocks;
            this.GeneratedBlockType = MyStringId.GetOrCompute(def.GeneratedBlockType?.ToLower());
            this.CompoundEnabled = def.CompoundEnabled;
            this.CreateFracturedPieces = def.CreateFracturedPieces;
            this.EmissiveColorPreset = (def.EmissiveColorPreset != null) ? MyStringHash.GetOrCompute(def.EmissiveColorPreset) : MyStringHash.NullOrEmpty;
            this.VoxelPlacement = def.VoxelPlacement;
            this.GeneralDamageMultiplier = def.GeneralDamageMultiplier;
            if (def.PhysicalMaterial != null)
            {
                base.PhysicalMaterial = MyDefinitionManager.Static.GetPhysicalMaterialDefinition(def.PhysicalMaterial);
            }
            if (def.Effects != null)
            {
                this.Effects = new Sandbox.Definitions.CubeBlockEffectBase[def.Effects.Length];
                for (int i = 0; i < def.Effects.Length; i++)
                {
                    this.Effects[i] = new Sandbox.Definitions.CubeBlockEffectBase(def.Effects[i].Name, def.Effects[i].ParameterMin, def.Effects[i].ParameterMax);
                    if ((def.Effects[i].ParticleEffects == null) || (def.Effects[i].ParticleEffects.Length == 0))
                    {
                        this.Effects[i].ParticleEffects = null;
                    }
                    else
                    {
                        this.Effects[i].ParticleEffects = new Sandbox.Definitions.CubeBlockEffect[def.Effects[i].ParticleEffects.Length];
                        for (int j = 0; j < def.Effects[i].ParticleEffects.Length; j++)
                        {
                            this.Effects[i].ParticleEffects[j] = new Sandbox.Definitions.CubeBlockEffect(def.Effects[i].ParticleEffects[j]);
                        }
                    }
                }
            }
            if (def.DamageEffectId != 0)
            {
                this.DamageEffectID = new int?(def.DamageEffectId);
            }
            if (!string.IsNullOrEmpty(def.DamageEffectName))
            {
                this.DamageEffectName = def.DamageEffectName;
            }
            if ((def.DestroyEffect != null) && (def.DestroyEffect.Length > 0))
            {
                this.DestroyEffect = def.DestroyEffect;
            }
            if (def.DestroyEffectOffset != null)
            {
                this.DestroyEffectOffset = new Vector3?(def.DestroyEffectOffset.Value);
            }
            this.InitEntityComponents(def.EntityComponents);
            this.CompoundTemplates = def.CompoundTemplates;
            if (def.SubBlockDefinitions != null)
            {
                this.SubBlockDefinitions = new Dictionary<string, MyDefinitionId>();
                foreach (MyObjectBuilder_CubeBlockDefinition.MySubBlockDefinition definition3 in def.SubBlockDefinitions)
                {
                    MyDefinitionId id;
                    if (!this.SubBlockDefinitions.TryGetValue(definition3.SubBlock, out id))
                    {
                        id = definition3.Id;
                        this.SubBlockDefinitions.Add(definition3.SubBlock, id);
                    }
                }
            }
            if (def.BlockVariants != null)
            {
                this.BlockStages = new MyDefinitionId[def.BlockVariants.Length];
                for (int i = 0; i < def.BlockVariants.Length; i++)
                {
                    this.BlockStages[i] = def.BlockVariants[i];
                }
            }
            MyObjectBuilder_CubeBlockDefinition.PatternDefinition cubeDefinition = def.CubeDefinition;
            if (cubeDefinition != null)
            {
                MyCubeDefinition definition4 = new MyCubeDefinition {
                    CubeTopology = cubeDefinition.CubeTopology,
                    ShowEdges = cubeDefinition.ShowEdges
                };
                MyObjectBuilder_CubeBlockDefinition.Side[] sides = cubeDefinition.Sides;
                definition4.Model = new string[sides.Length];
                definition4.PatternSize = new Vector2I[sides.Length];
                definition4.ScaleTile = new Vector2I[sides.Length];
                int index = 0;
                while (true)
                {
                    if (index >= sides.Length)
                    {
                        this.CubeDefinition = definition4;
                        break;
                    }
                    MyObjectBuilder_CubeBlockDefinition.Side side = sides[index];
                    definition4.Model[index] = side.Model;
                    definition4.PatternSize[index] = (Vector2I) side.PatternSize;
                    definition4.ScaleTile[index] = new Vector2I(side.ScaleTileU, side.ScaleTileV);
                    index++;
                }
            }
            MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent[] components = def.Components;
            float num = 0f;
            float num2 = 0f;
            float num3 = 0f;
            this.MaxIntegrityRatio = 1f;
            if ((components == null) || (components.Length == 0))
            {
                if (def.MaxIntegrity != 0)
                {
                    this.MaxIntegrity = def.MaxIntegrity;
                }
            }
            else
            {
                this.Components = new Component[components.Length];
                float num9 = 0f;
                int num10 = 0;
                int index = 0;
                while (true)
                {
                    if (index >= components.Length)
                    {
                        this.MaxIntegrity = num9;
                        this.IntegrityPointsPerSec = this.MaxIntegrity / def.BuildTimeSeconds;
                        this.DisassembleRatio = def.DisassembleRatio;
                        if (def.MaxIntegrity != 0)
                        {
                            this.MaxIntegrityRatio = ((float) def.MaxIntegrity) / this.MaxIntegrity;
                            this.DeformationRatio /= this.MaxIntegrityRatio;
                        }
                        if (!MyPerGameSettings.Destruction)
                        {
                            base.Mass = num;
                        }
                        break;
                    }
                    MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent component = components[index];
                    MyComponentDefinition componentDefinition = MyDefinitionManager.Static.GetComponentDefinition(new MyDefinitionId(component.Type, component.Subtype));
                    MyPhysicalItemDefinition definition = null;
                    if (!component.DeconstructId.IsNull() && !MyDefinitionManager.Static.TryGetPhysicalItemDefinition(component.DeconstructId, out definition))
                    {
                        definition = componentDefinition;
                    }
                    if (definition == null)
                    {
                        definition = componentDefinition;
                    }
                    Component component1 = new Component();
                    component1.Definition = componentDefinition;
                    component1.Count = component.Count;
                    component1.DeconstructItem = definition;
                    Component component2 = component1;
                    if (((component.Type == typeof(MyObjectBuilder_Component)) && (component.Subtype == "Computer")) && (num3 == 0f))
                    {
                        num3 = num9 + component2.Definition.MaxIntegrity;
                    }
                    num9 += component2.Count * component2.Definition.MaxIntegrity;
                    if ((component.Type == def.CriticalComponent.Type) && (component.Subtype == def.CriticalComponent.Subtype))
                    {
                        if (num10 == def.CriticalComponent.Index)
                        {
                            this.CriticalGroup = (ushort) index;
                            num2 = num9 - component2.Definition.MaxIntegrity;
                        }
                        num10++;
                    }
                    num += component2.Count * component2.Definition.Mass;
                    this.Components[index] = component2;
                    index++;
                }
            }
            this.CriticalIntegrityRatio = num2 / this.MaxIntegrity;
            this.OwnershipIntegrityRatio = num3 / this.MaxIntegrity;
            if (def.BuildProgressModels != null)
            {
                def.BuildProgressModels.Sort((a, b) => a.BuildPercentUpperBound.CompareTo(b.BuildPercentUpperBound));
                this.BuildProgressModels = new BuildProgressModel[def.BuildProgressModels.Count];
                for (int i = 0; i < this.BuildProgressModels.Length; i++)
                {
                    MyObjectBuilder_CubeBlockDefinition.BuildProgressModel model = def.BuildProgressModels[i];
                    if (!string.IsNullOrEmpty(model.File))
                    {
                        BuildProgressModel model1 = new BuildProgressModel();
                        BuildProgressModel model2 = new BuildProgressModel();
                        model2.BuildRatioUpperBound = (this.CriticalIntegrityRatio > 0f) ? (model.BuildPercentUpperBound * this.CriticalIntegrityRatio) : model.BuildPercentUpperBound;
                        BuildProgressModel local2 = model2;
                        local2.File = model.File;
                        local2.RandomOrientation = model.RandomOrientation;
                        this.BuildProgressModels[i] = local2;
                    }
                }
            }
            if (def.GeneratedBlocks != null)
            {
                this.GeneratedBlockDefinitions = new MyDefinitionId[def.GeneratedBlocks.Length];
                for (int i = 0; i < def.GeneratedBlocks.Length; i++)
                {
                    this.GeneratedBlockDefinitions[i] = def.GeneratedBlocks[i];
                }
            }
            this.Skeleton = def.Skeleton;
            if (this.Skeleton != null)
            {
                this.Bones = new Dictionary<Vector3I, Vector3>(def.Skeleton.Count);
                foreach (VRage.Game.BoneInfo info in this.Skeleton)
                {
                    this.Bones[(Vector3I) info.BonePosition] = Vector3UByte.Denormalize((Vector3UByte) info.BoneOffset, MyDefinitionManager.Static.GetCubeSize(def.CubeSize));
                }
            }
            this.IsAirTight = def.IsAirTight;
            this.IsStandAlone = def.IsStandAlone;
            this.HasPhysics = def.HasPhysics;
            this.UseNeighbourOxygenRooms = def.UseNeighbourOxygenRooms;
            this.InitMountPoints(def);
            this.InitPressurization();
            this.InitNavigationInfo(def, def.NavigationDefinition);
            this.PrimarySound = new MySoundPair(def.PrimarySound, true);
            this.ActionSound = new MySoundPair(def.ActionSound, true);
            if ((def.DamagedSound != null) && (def.DamagedSound.Length > 0))
            {
                this.DamagedSound = new MySoundPair(def.DamagedSound, true);
            }
            if ((def.DestroySound != null) && (def.DestroySound.Length > 0))
            {
                this.DestroySound = new MySoundPair(def.DestroySound, true);
            }
            this.PCU = def.PCU;
            this.PlaceDecals = def.PlaceDecals;
        }

        private void InitEntityComponents(MyObjectBuilder_CubeBlockDefinition.EntityComponentDefinition[] entityComponentDefinitions)
        {
            if (entityComponentDefinitions != null)
            {
                this.EntityComponents = new Dictionary<string, MyObjectBuilder_ComponentBase>(entityComponentDefinitions.Length);
                for (int i = 0; i < entityComponentDefinitions.Length; i++)
                {
                    MyObjectBuilder_CubeBlockDefinition.EntityComponentDefinition definition = entityComponentDefinitions[i];
                    MyObjectBuilderType type = MyObjectBuilderType.Parse(definition.BuilderType);
                    if (!type.IsNull)
                    {
                        MyObjectBuilder_ComponentBase base2 = MyObjectBuilderSerializer.CreateNewObject(type) as MyObjectBuilder_ComponentBase;
                        if (base2 != null)
                        {
                            this.EntityComponents.Add(definition.ComponentType, base2);
                        }
                    }
                }
            }
        }

        private void InitMountPoints(MyObjectBuilder_CubeBlockDefinition def)
        {
            if (this.MountPoints == null)
            {
                Vector3 vector1 = (this.Size - 1) / 2;
                if ((!base.Context.IsBaseGame && (def.MountPoints != null)) && (def.MountPoints.Length == 0))
                {
                    def.MountPoints = null;
                    string message = "Obsolete default definition of mount points in " + def.Id;
                    MyDefinitionErrors.Add(base.Context, message, TErrorSeverity.Warning, true);
                }
                if (def.MountPoints != null)
                {
                    this.SetMountPoints(ref this.MountPoints, def.MountPoints, m_tmpMounts);
                    if (def.BuildProgressModels != null)
                    {
                        for (int i = 0; i < def.BuildProgressModels.Count; i++)
                        {
                            BuildProgressModel model = this.BuildProgressModels[i];
                            if (model != null)
                            {
                                MyObjectBuilder_CubeBlockDefinition.BuildProgressModel model2 = def.BuildProgressModels[i];
                                if (model2.MountPoints != null)
                                {
                                    MyObjectBuilder_CubeBlockDefinition.MountPoint[] mountPoints = model2.MountPoints;
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index >= mountPoints.Length)
                                        {
                                            m_tmpIndices.Clear();
                                            model.MountPoints = new MountPoint[m_tmpMounts.Count];
                                            this.SetMountPoints(ref model.MountPoints, m_tmpMounts.ToArray(), null);
                                            break;
                                        }
                                        MyObjectBuilder_CubeBlockDefinition.MountPoint item = mountPoints[index];
                                        int sideId = (int) item.Side;
                                        if (!m_tmpIndices.Contains(sideId))
                                        {
                                            m_tmpMounts.RemoveAll(mount => mount.Side == sideId);
                                            m_tmpIndices.Add(sideId);
                                        }
                                        m_tmpMounts.Add(item);
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                    m_tmpMounts.Clear();
                }
                else
                {
                    Vector3I vectori;
                    Vector3I vectori2;
                    Vector3I vectori3;
                    Vector3I vectori4;
                    Vector3I vectori5;
                    Vector3I vectori6;
                    Vector3 vector3;
                    Vector3 vector4;
                    Vector3 vector5;
                    Vector3 vector6;
                    Vector3 vector9;
                    Vector3 vector10;
                    Vector3 vector11;
                    Vector3 vector12;
                    Vector3 vector15;
                    Vector3 vector16;
                    Vector3 vector17;
                    Vector3 vector18;
                    List<MountPoint> list = new List<MountPoint>(6);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[0], out vectori);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[1], out vectori3);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[2], out vectori5);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[3], out vectori2);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[4], out vectori4);
                    Vector3I.TransformNormal(ref Vector3I.Forward, ref m_mountPointTransforms[5], out vectori6);
                    Vector3 position = new Vector3(0.001f, 0.001f, 0.0004f);
                    Vector3 vector2 = new Vector3(this.Size.Z - 0.001f, this.Size.Y - 0.001f, -0.0004f);
                    TransformMountPointPosition(ref position, 0, this.Size, out vector3);
                    TransformMountPointPosition(ref vector2, 0, this.Size, out vector5);
                    TransformMountPointPosition(ref position, 3, this.Size, out vector4);
                    TransformMountPointPosition(ref vector2, 3, this.Size, out vector6);
                    MountPoint item = new MountPoint {
                        Start = vector3,
                        End = vector5,
                        Normal = vectori,
                        Enabled = true
                    };
                    list.Add(item);
                    item = new MountPoint {
                        Start = vector4,
                        End = vector6,
                        Normal = vectori2,
                        Enabled = true
                    };
                    list.Add(item);
                    Vector3 vector7 = new Vector3(0.001f, 0.001f, 0.0004f);
                    Vector3 vector8 = new Vector3(this.Size.X - 0.001f, this.Size.Z - 0.001f, -0.0004f);
                    TransformMountPointPosition(ref vector7, 1, this.Size, out vector9);
                    TransformMountPointPosition(ref vector8, 1, this.Size, out vector11);
                    TransformMountPointPosition(ref vector7, 4, this.Size, out vector10);
                    TransformMountPointPosition(ref vector8, 4, this.Size, out vector12);
                    item = new MountPoint {
                        Start = vector9,
                        End = vector11,
                        Normal = vectori3,
                        Enabled = true
                    };
                    list.Add(item);
                    item = new MountPoint {
                        Start = vector10,
                        End = vector12,
                        Normal = vectori4,
                        Enabled = true
                    };
                    list.Add(item);
                    Vector3 vector13 = new Vector3(0.001f, 0.001f, 0.0004f);
                    Vector3 vector14 = new Vector3(this.Size.X - 0.001f, this.Size.Y - 0.001f, -0.0004f);
                    TransformMountPointPosition(ref vector13, 2, this.Size, out vector15);
                    TransformMountPointPosition(ref vector14, 2, this.Size, out vector17);
                    TransformMountPointPosition(ref vector13, 5, this.Size, out vector16);
                    TransformMountPointPosition(ref vector14, 5, this.Size, out vector18);
                    item = new MountPoint {
                        Start = vector15,
                        End = vector17,
                        Normal = vectori5,
                        Enabled = true
                    };
                    list.Add(item);
                    item = new MountPoint {
                        Start = vector16,
                        End = vector18,
                        Normal = vectori6,
                        Enabled = true
                    };
                    list.Add(item);
                    this.MountPoints = list.ToArray();
                }
            }
        }

        public void InitNavigationInfo(MyObjectBuilder_CubeBlockDefinition blockDef, string infoSubtypeId)
        {
            if (MyPerGameSettings.EnableAi)
            {
                if (infoSubtypeId == "Default")
                {
                    MyDefinitionManager.Static.SetDefaultNavDef(this);
                }
                else
                {
                    MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_BlockNavigationDefinition), infoSubtypeId);
                    MyDefinitionManager.Static.TryGetDefinition<MyBlockNavigationDefinition>(defId, out this.NavigationDefinition);
                }
                if ((this.NavigationDefinition != null) && (this.NavigationDefinition.Mesh != null))
                {
                    this.NavigationDefinition.Mesh.MakeStatic();
                }
            }
        }

        public void InitPressurization()
        {
            this.IsCubePressurized = new Dictionary<Vector3I, Dictionary<Vector3I, bool>>();
            int x = 0;
            while (x < this.Size.X)
            {
                int y = 0;
                while (true)
                {
                    if (y >= this.Size.Y)
                    {
                        x++;
                        break;
                    }
                    int z = 0;
                    while (true)
                    {
                        if (z >= this.Size.Z)
                        {
                            y++;
                            break;
                        }
                        Vector3 position = new Vector3((float) x, (float) y, (float) z);
                        Vector3 vector2 = new Vector3((float) x, (float) y, (float) z) + Vector3.One;
                        Vector3I vectori = new Vector3I(x, y, z);
                        this.IsCubePressurized[vectori] = new Dictionary<Vector3I, bool>();
                        Vector3I[] intDirections = Base6Directions.IntDirections;
                        int index = 0;
                        while (true)
                        {
                            if (index >= intDirections.Length)
                            {
                                z++;
                                break;
                            }
                            Vector3I vec = intDirections[index];
                            this.IsCubePressurized[vectori][vec] = false;
                            if ((((vec.X != 1) || (x == (this.Size.X - 1))) && (((vec.X != -1) || (x == 0)) && (((vec.Y != 1) || (y == (this.Size.Y - 1))) && (((vec.Y != -1) || (y == 0)) && ((vec.Z != 1) || (z == (this.Size.Z - 1))))))) && ((vec.Z != -1) || (z == 0)))
                            {
                                foreach (MountPoint point in this.MountPoints)
                                {
                                    if (vec == point.Normal)
                                    {
                                        Vector3 vector5;
                                        Vector3 vector6;
                                        Vector3 vector7;
                                        Vector3 vector8;
                                        int mountPointWallIndex = GetMountPointWallIndex(Base6Directions.GetDirection(ref vec));
                                        Vector3I size = this.Size;
                                        Vector3 start = point.Start;
                                        Vector3 end = point.End;
                                        UntransformMountPointPosition(ref start, mountPointWallIndex, size, out vector5);
                                        UntransformMountPointPosition(ref end, mountPointWallIndex, size, out vector6);
                                        UntransformMountPointPosition(ref position, mountPointWallIndex, size, out vector8);
                                        UntransformMountPointPosition(ref vector2, mountPointWallIndex, size, out vector7);
                                        Vector3 vector9 = new Vector3(Math.Max(vector8.X, vector7.X), Math.Max(vector8.Y, vector7.Y), Math.Max(vector8.Z, vector7.Z));
                                        Vector3 vector10 = new Vector3(Math.Min(vector8.X, vector7.X), Math.Min(vector8.Y, vector7.Y), Math.Min(vector8.Z, vector7.Z));
                                        if ((((vector5.X - 0.05) <= vector10.X) && (((vector6.X + 0.05) > vector9.X) && ((vector5.Y - 0.05) <= vector10.Y))) && ((vector6.Y + 0.05) > vector9.Y))
                                        {
                                            this.IsCubePressurized[vectori][vec] = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            index++;
                        }
                    }
                }
            }
        }

        public bool ModelChangeIsNeeded(float percentageA, float percentageB)
        {
            if (percentageA >= percentageB)
            {
                return false;
            }
            if (percentageA == 0f)
            {
                return true;
            }
            if (this.BuildProgressModels == null)
            {
                return false;
            }
            int index = 0;
            while ((index < this.BuildProgressModels.Length) && (percentageA > this.BuildProgressModels[index].BuildRatioUpperBound))
            {
                index++;
            }
            return ((index < this.BuildProgressModels.Length) ? (percentageB >= this.BuildProgressModels[index].BuildRatioUpperBound) : false);
        }

        public Vector3 MountPointLocalNormalToBlockLocal(Vector3 normal, VRageMath.Base6Directions.Direction mountPointDirection)
        {
            Vector3 result = new Vector3();
            int index = m_mountPointWallIndices[(int) mountPointDirection];
            Vector3.TransformNormal(ref normal, ref m_mountPointTransforms[index], out result);
            return result;
        }

        public Vector3 MountPointLocalToBlockLocal(Vector3 coord, VRageMath.Base6Directions.Direction mountPointDirection)
        {
            Vector3 result = new Vector3();
            int wallIndex = m_mountPointWallIndices[(int) mountPointDirection];
            TransformMountPointPosition(ref coord, wallIndex, this.Size, out result);
            return (result - this.Center);
        }

        public static BlockSideEnum NormalToBlockSide(Vector3I normal)
        {
            for (int i = 0; i < m_mountPointTransforms.Length; i++)
            {
                Vector3I vectori = new Vector3I(m_mountPointTransforms[i].Forward);
                if (normal == vectori)
                {
                    return (BlockSideEnum) i;
                }
            }
            return BlockSideEnum.Right;
        }

        public static void PreloadConstructionModels(MyCubeBlockDefinition block)
        {
            if ((block != null) && !m_preloadedDefinitions.Contains(block))
            {
                List<string> models = m_stringPool.Allocate();
                models.Clear();
                for (int i = 0; i < block.BuildProgressModels.Length; i++)
                {
                    BuildProgressModel model = block.BuildProgressModels[i];
                    if ((model != null) && !string.IsNullOrEmpty(model.File))
                    {
                        models.Add(model.File);
                    }
                }
                MyRenderProxy.PreloadModels(models, true);
                m_stringPool.Deallocate(models);
                m_preloadedDefinitions.Add(block);
            }
        }

        public bool RatioEnoughForDamageEffect(float ratio) => 
            (ratio < this.CriticalIntegrityRatio);

        public bool RatioEnoughForOwnership(float ratio) => 
            (ratio >= this.OwnershipIntegrityRatio);

        private unsafe void SetMountPoints(ref MountPoint[] mountPoints, MyObjectBuilder_CubeBlockDefinition.MountPoint[] mpBuilders, List<MyObjectBuilder_CubeBlockDefinition.MountPoint> addedMounts)
        {
            if (mountPoints == null)
            {
                mountPoints = new MountPoint[mpBuilders.Length];
            }
            for (int i = 0; i < mountPoints.Length; i++)
            {
                MyObjectBuilder_CubeBlockDefinition.MountPoint item = mpBuilders[i];
                if (addedMounts != null)
                {
                    addedMounts.Add(item);
                }
                Vector3 result = new Vector3(Vector2.Min((Vector2) item.Start, (Vector2) item.End) + 0.001f, 0.0004f);
                Vector3 vector2 = new Vector3(Vector2.Max((Vector2) item.Start, (Vector2) item.End) - 0.001f, -0.0004f);
                int side = (int) item.Side;
                Vector3I forward = Vector3I.Forward;
                Vector3* vectorPtr1 = (Vector3*) ref result;
                TransformMountPointPosition(ref (Vector3) ref vectorPtr1, side, this.Size, out result);
                Vector3* vectorPtr2 = (Vector3*) ref vector2;
                TransformMountPointPosition(ref (Vector3) ref vectorPtr2, side, this.Size, out vector2);
                Vector3I* vectoriPtr1 = (Vector3I*) ref forward;
                Vector3I.TransformNormal(ref (Vector3I) ref vectoriPtr1, ref m_mountPointTransforms[side], out forward);
                mountPoints[i].Start = result;
                mountPoints[i].End = vector2;
                mountPoints[i].Normal = forward;
                mountPoints[i].ExclusionMask = item.ExclusionMask;
                mountPoints[i].PropertiesMask = item.PropertiesMask;
                mountPoints[i].Enabled = item.Enabled;
                mountPoints[i].Default = item.Default;
            }
        }

        internal static void TransformMountPointPosition(ref Vector3 position, int wallIndex, Vector3I cubeSize, out Vector3 result)
        {
            Vector3.Transform(ref position, ref m_mountPointTransforms[wallIndex], out result);
            result += m_mountPointWallOffsets[wallIndex] * cubeSize;
        }

        internal static void UntransformMountPointPosition(ref Vector3 position, int wallIndex, Vector3I cubeSize, out Vector3 result)
        {
            Vector3 vector = position - (m_mountPointWallOffsets[wallIndex] * cubeSize);
            Matrix matrix = Matrix.Invert(m_mountPointTransforms[wallIndex]);
            Vector3.Transform(ref vector, ref matrix, out result);
        }

        public MyBlockDirection Direction { get; private set; }

        public MyBlockRotation Rotation { get; private set; }

        public bool IsGeneratedBlock =>
            (this.GeneratedBlockType != MyStringId.NullOrEmpty);

        public Vector3I Center =>
            this.m_center;

        public MySymmetryAxisEnum SymmetryX =>
            this.m_symmetryX;

        public MySymmetryAxisEnum SymmetryY =>
            this.m_symmetryY;

        public MySymmetryAxisEnum SymmetryZ =>
            this.m_symmetryZ;

        public string MirroringBlock =>
            this.m_mirroringBlock;

        public override string DisplayNameText
        {
            get
            {
                if (this.DisplayNameVariant == null)
                {
                    return base.DisplayNameText;
                }
                if (this.m_displayNameTextCache == null)
                {
                    this.m_displayNameTextCache = new StringBuilder();
                }
                this.m_displayNameTextCache.Clear();
                return this.m_displayNameTextCache.Append(base.DisplayNameText).Append(' ').Append(MyTexts.GetString(this.DisplayNameVariant.Value)).ToString();
            }
        }

        public bool GuiVisible { get; private set; }

        public bool Mirrored { get; private set; }

        public bool RandomRotation { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCubeBlockDefinition.<>c <>9 = new MyCubeBlockDefinition.<>c();
            public static Comparison<MyObjectBuilder_CubeBlockDefinition.BuildProgressModel> <>9__105_0;
            public static Func<MyCubeBlockDefinition.Component, bool> <>9__131_0;

            internal bool <ContainsComputer>b__131_0(MyCubeBlockDefinition.Component x) => 
                ((x.Definition.Id.TypeId == typeof(MyObjectBuilder_Component)) && (x.Definition.Id.SubtypeName == "Computer"));

            internal int <Init>b__105_0(MyObjectBuilder_CubeBlockDefinition.BuildProgressModel a, MyObjectBuilder_CubeBlockDefinition.BuildProgressModel b) => 
                a.BuildPercentUpperBound.CompareTo(b.BuildPercentUpperBound);
        }

        public class BuildProgressModel
        {
            public float BuildRatioUpperBound;
            public string File;
            public bool RandomOrientation;
            public MyCubeBlockDefinition.MountPoint[] MountPoints;
            public bool Visible;
        }

        public class Component
        {
            public MyComponentDefinition Definition;
            public int Count;
            public MyPhysicalItemDefinition DeconstructItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MountPoint
        {
            public Vector3I Normal;
            public Vector3 Start;
            public Vector3 End;
            public byte ExclusionMask;
            public byte PropertiesMask;
            public bool Enabled;
            public bool Default;
            public MyObjectBuilder_CubeBlockDefinition.MountPoint GetObjectBuilder(Vector3I cubeSize)
            {
                Vector3 vector;
                Vector3 vector2;
                MyObjectBuilder_CubeBlockDefinition.MountPoint point = new MyObjectBuilder_CubeBlockDefinition.MountPoint {
                    Side = MyCubeBlockDefinition.NormalToBlockSide(this.Normal)
                };
                MyCubeBlockDefinition.UntransformMountPointPosition(ref this.Start, (int) point.Side, cubeSize, out vector);
                MyCubeBlockDefinition.UntransformMountPointPosition(ref this.End, (int) point.Side, cubeSize, out vector2);
                point.Start = new SerializableVector2(vector.X, vector.Y);
                point.End = new SerializableVector2(vector2.X, vector2.Y);
                point.ExclusionMask = this.ExclusionMask;
                point.PropertiesMask = this.PropertiesMask;
                point.Enabled = this.Enabled;
                point.Default = this.Default;
                return point;
            }
        }
    }
}


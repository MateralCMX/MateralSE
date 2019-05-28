namespace VRage.Game
{
    using Medieval.ObjectBuilders.Definitions;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelMaterialDefinition), (Type) null)]
    public class MyVoxelMaterialDefinition : MyDefinitionBase
    {
        private static byte m_indexCounter;
        public string MaterialTypeName;
        public string MinedOre;
        public float MinedOreRatio;
        public bool CanBeHarvested;
        public bool IsRare;
        public int MinVersion;
        public int MaxVersion;
        public bool SpawnsInAsteroids;
        public bool SpawnsFromMeteorites;
        public string VoxelHandPreview;
        public float Friction;
        public float Restitution;
        public string LandingEffect;
        public int AsteroidGeneratorSpawnProbabilityMultiplier;
        public string BareVariant;
        private MyStringId m_materialTypeNameIdCache;
        private MyStringHash m_materialTypeNameHashCache;
        public MyStringHash DamagedMaterial;
        public MyRenderVoxelMaterialData RenderParams;
        public Vector3? ColorKey;

        public void AssignIndex()
        {
            m_indexCounter = (byte) (m_indexCounter + 1);
            this.Index = m_indexCounter;
            this.RenderParams.Index = this.Index;
        }

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder() => 
            null;

        protected override unsafe void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_Dx11VoxelMaterialDefinition definition = ob as MyObjectBuilder_Dx11VoxelMaterialDefinition;
            this.MaterialTypeName = definition.MaterialTypeName;
            this.MinedOre = definition.MinedOre;
            this.MinedOreRatio = definition.MinedOreRatio;
            this.CanBeHarvested = definition.CanBeHarvested;
            this.IsRare = definition.IsRare;
            this.SpawnsInAsteroids = definition.SpawnsInAsteroids;
            this.SpawnsFromMeteorites = definition.SpawnsFromMeteorites;
            this.VoxelHandPreview = definition.VoxelHandPreview;
            this.MinVersion = definition.MinVersion;
            this.MaxVersion = definition.MaxVersion;
            this.DamagedMaterial = MyStringHash.GetOrCompute(definition.DamagedMaterial);
            this.Friction = definition.Friction;
            this.Restitution = definition.Restitution;
            this.LandingEffect = definition.LandingEffect;
            this.BareVariant = definition.BareVariant;
            this.AsteroidGeneratorSpawnProbabilityMultiplier = definition.AsteroidGeneratorSpawnProbabilityMultiplier;
            if (definition.ColorKey != null)
            {
                this.ColorKey = new Vector3?(definition.ColorKey.Value.ColorToHSV());
            }
            this.RenderParams.Index = this.Index;
            this.RenderParams.TextureSets = new MyRenderVoxelMaterialData.TextureSet[3];
            this.RenderParams.TextureSets[0].ColorMetalXZnY = definition.ColorMetalXZnY;
            this.RenderParams.TextureSets[0].ColorMetalY = definition.ColorMetalY;
            this.RenderParams.TextureSets[0].NormalGlossXZnY = definition.NormalGlossXZnY;
            this.RenderParams.TextureSets[0].NormalGlossY = definition.NormalGlossY;
            this.RenderParams.TextureSets[0].ExtXZnY = definition.ExtXZnY;
            this.RenderParams.TextureSets[0].ExtY = definition.ExtY;
            this.RenderParams.TextureSets[0].Check();
            this.RenderParams.TextureSets[1].ColorMetalXZnY = definition.ColorMetalXZnYFar1 ?? this.RenderParams.TextureSets[0].ColorMetalXZnY;
            this.RenderParams.TextureSets[1].ColorMetalY = definition.ColorMetalYFar1 ?? this.RenderParams.TextureSets[1].ColorMetalXZnY;
            this.RenderParams.TextureSets[1].NormalGlossXZnY = definition.NormalGlossXZnYFar1 ?? this.RenderParams.TextureSets[0].NormalGlossXZnY;
            this.RenderParams.TextureSets[1].NormalGlossY = definition.NormalGlossYFar1 ?? this.RenderParams.TextureSets[1].NormalGlossXZnY;
            this.RenderParams.TextureSets[1].ExtXZnY = definition.ExtXZnYFar1 ?? this.RenderParams.TextureSets[0].ExtXZnY;
            this.RenderParams.TextureSets[1].ExtY = definition.ExtYFar1 ?? this.RenderParams.TextureSets[1].ExtXZnY;
            this.RenderParams.TextureSets[2].ColorMetalXZnY = definition.ColorMetalXZnYFar2 ?? this.RenderParams.TextureSets[1].ColorMetalXZnY;
            this.RenderParams.TextureSets[2].ColorMetalY = definition.ColorMetalYFar2 ?? this.RenderParams.TextureSets[2].ColorMetalXZnY;
            this.RenderParams.TextureSets[2].NormalGlossXZnY = definition.NormalGlossXZnYFar2 ?? this.RenderParams.TextureSets[1].NormalGlossXZnY;
            this.RenderParams.TextureSets[2].NormalGlossY = definition.NormalGlossYFar2 ?? this.RenderParams.TextureSets[2].NormalGlossXZnY;
            this.RenderParams.TextureSets[2].ExtXZnY = definition.ExtXZnYFar2 ?? this.RenderParams.TextureSets[1].ExtXZnY;
            this.RenderParams.TextureSets[2].ExtY = definition.ExtYFar2 ?? this.RenderParams.TextureSets[2].ExtXZnY;
            this.RenderParams.StandardTilingSetup.InitialScale = definition.InitialScale;
            this.RenderParams.StandardTilingSetup.ScaleMultiplier = definition.ScaleMultiplier;
            this.RenderParams.StandardTilingSetup.InitialDistance = definition.InitialDistance;
            this.RenderParams.StandardTilingSetup.DistanceMultiplier = definition.DistanceMultiplier;
            this.RenderParams.StandardTilingSetup.TilingScale = definition.TilingScale;
            this.RenderParams.StandardTilingSetup.Far1Distance = definition.Far1Distance;
            this.RenderParams.StandardTilingSetup.Far2Distance = definition.Far2Distance;
            this.RenderParams.StandardTilingSetup.Far3Distance = definition.Far3Distance;
            this.RenderParams.StandardTilingSetup.Far1Scale = definition.Far1Scale;
            this.RenderParams.StandardTilingSetup.Far2Scale = definition.Far2Scale;
            this.RenderParams.StandardTilingSetup.Far3Scale = definition.Far3Scale;
            this.RenderParams.StandardTilingSetup.ExtensionDetailScale = definition.ExtDetailScale;
            if (definition.SimpleTilingSetup == null)
            {
                this.RenderParams.SimpleTilingSetup = this.RenderParams.StandardTilingSetup;
            }
            else
            {
                Medieval.ObjectBuilders.Definitions.TilingSetup simpleTilingSetup = definition.SimpleTilingSetup;
                this.RenderParams.SimpleTilingSetup.InitialScale = simpleTilingSetup.InitialScale;
                this.RenderParams.SimpleTilingSetup.ScaleMultiplier = simpleTilingSetup.ScaleMultiplier;
                this.RenderParams.SimpleTilingSetup.InitialDistance = simpleTilingSetup.InitialDistance;
                this.RenderParams.SimpleTilingSetup.DistanceMultiplier = simpleTilingSetup.DistanceMultiplier;
                this.RenderParams.SimpleTilingSetup.TilingScale = simpleTilingSetup.TilingScale;
                this.RenderParams.SimpleTilingSetup.Far1Distance = simpleTilingSetup.Far1Distance;
                this.RenderParams.SimpleTilingSetup.Far2Distance = simpleTilingSetup.Far2Distance;
                this.RenderParams.SimpleTilingSetup.Far3Distance = simpleTilingSetup.Far3Distance;
                this.RenderParams.SimpleTilingSetup.Far1Scale = simpleTilingSetup.Far1Scale;
                this.RenderParams.SimpleTilingSetup.Far2Scale = simpleTilingSetup.Far2Scale;
                this.RenderParams.SimpleTilingSetup.Far3Scale = simpleTilingSetup.Far3Scale;
                this.RenderParams.SimpleTilingSetup.ExtensionDetailScale = simpleTilingSetup.ExtDetailScale;
            }
            this.RenderParams.Far3Color = definition.Far3Color;
            MyRenderFoliageData data = new MyRenderFoliageData();
            if (definition.FoliageColorTextureArray != null)
            {
                int length;
                data.Type = (MyFoliageType) definition.FoliageType;
                data.Density = definition.FoliageDensity;
                string[] foliageColorTextureArray = definition.FoliageColorTextureArray;
                string[] foliageNormalTextureArray = definition.FoliageNormalTextureArray;
                if (foliageNormalTextureArray == null)
                {
                    length = foliageColorTextureArray.Length;
                }
                else
                {
                    if (foliageColorTextureArray.Length != foliageNormalTextureArray.Length)
                    {
                        MyLog.Default.Warning("Legacy foliage format has different size normal and color arrays, only the minimum length will be used.", Array.Empty<object>());
                    }
                    length = Math.Min(foliageColorTextureArray.Length, foliageNormalTextureArray.Length);
                }
                length = Math.Min(length, 0x10);
                data.Entries = new MyRenderFoliageData.FoliageEntry[length];
                for (int i = 0; i < length; i++)
                {
                    MyRenderFoliageData.FoliageEntry* entryPtr1;
                    MyRenderFoliageData.FoliageEntry entry = new MyRenderFoliageData.FoliageEntry {
                        ColorAlphaTexture = foliageColorTextureArray[i]
                    };
                    entryPtr1->NormalGlossTexture = (foliageNormalTextureArray != null) ? foliageNormalTextureArray[i] : null;
                    entryPtr1 = (MyRenderFoliageData.FoliageEntry*) ref entry;
                    entry.Probability = 1f;
                    entry.Size = definition.FoliageScale;
                    entry.SizeVariation = definition.FoliageRandomRescaleMult;
                    data.Entries[i] = entry;
                }
            }
            if (data.Density > 0f)
            {
                this.RenderParams.Foliage = new MyRenderFoliageData?(data);
            }
        }

        public static void ResetIndexing()
        {
            m_indexCounter = 0;
        }

        public void UpdateVoxelMaterial()
        {
            MyRenderVoxelMaterialData[] materials = new MyRenderVoxelMaterialData[] { this.RenderParams };
            MyRenderProxy.UpdateRenderVoxelMaterials(materials);
        }

        public MyStringId MaterialTypeNameId
        {
            get
            {
                MyStringId id = new MyStringId();
                if (this.m_materialTypeNameIdCache == id)
                {
                    this.m_materialTypeNameIdCache = MyStringId.GetOrCompute(this.MaterialTypeName);
                }
                return this.m_materialTypeNameIdCache;
            }
        }

        public MyStringHash MaterialTypeNameHash
        {
            get
            {
                MyStringHash hash = new MyStringHash();
                if (this.m_materialTypeNameHashCache == hash)
                {
                    this.m_materialTypeNameHashCache = MyStringHash.GetOrCompute(this.MaterialTypeName);
                }
                return this.m_materialTypeNameHashCache;
            }
        }

        public byte Index { get; set; }

        public bool HasDamageMaterial =>
            (this.DamagedMaterial != MyStringHash.NullOrEmpty);

        public string Icon
        {
            get
            {
                if ((base.Icons == null) || (base.Icons.Length == 0))
                {
                    return this.RenderParams.TextureSets[0].ColorMetalXZnY;
                }
                return base.Icons[0];
            }
        }
    }
}


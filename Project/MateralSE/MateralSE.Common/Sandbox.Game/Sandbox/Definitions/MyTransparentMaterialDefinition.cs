namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;
    using VRageRender;

    [MyDefinitionType(typeof(MyObjectBuilder_TransparentMaterialDefinition), (Type) null)]
    public class MyTransparentMaterialDefinition : MyDefinitionBase
    {
        public string Texture;
        public string GlossTexture;
        public MyTransparentMaterialTextureType TextureType;
        public bool CanBeAffectedByLights;
        public bool AlphaMistingEnable;
        public bool UseAtlas;
        public float AlphaMistingStart;
        public float AlphaMistingEnd;
        public float SoftParticleDistanceScale;
        public float AlphaSaturation;
        public float Reflectivity;
        public float Fresnel;
        public bool IsFlareOccluder;
        public Vector4 Color = Vector4.One;
        public Vector4 ColorAdd = Vector4.Zero;
        public Vector4 ShadowMultiplier = Vector4.Zero;
        public Vector4 LightMultiplier = (Vector4.One * 0.1f);
        public bool AlphaCutout;
        public Vector2I TargetSize;
        public float ReflectionShadow = 0.1f;
        public float Gloss = 0.4f;
        public float GlossTextureAdd = 0.55f;
        public float SpecularColorFactor = 20f;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_TransparentMaterialDefinition definition = builder as MyObjectBuilder_TransparentMaterialDefinition;
            this.Texture = definition.Texture;
            this.GlossTexture = definition.GlossTexture;
            if (this.Texture == null)
            {
                this.Texture = string.Empty;
            }
            this.TextureType = definition.TextureType;
            this.CanBeAffectedByLights = definition.CanBeAffectedByOtherLights;
            this.AlphaMistingEnable = definition.AlphaMistingEnable;
            this.UseAtlas = definition.UseAtlas;
            this.AlphaMistingStart = definition.AlphaMistingStart;
            this.AlphaMistingEnd = definition.AlphaMistingEnd;
            this.SoftParticleDistanceScale = definition.SoftParticleDistanceScale;
            this.AlphaSaturation = definition.AlphaSaturation;
            this.Reflectivity = definition.Reflectivity;
            this.Fresnel = definition.Fresnel;
            this.IsFlareOccluder = definition.IsFlareOccluder;
            this.Color = definition.Color;
            this.ColorAdd = definition.ColorAdd;
            this.ShadowMultiplier = definition.ShadowMultiplier;
            this.LightMultiplier = definition.LightMultiplier;
            this.AlphaCutout = definition.AlphaCutout;
            this.TargetSize = definition.TargetSize;
            this.ReflectionShadow = definition.ReflectionShadow;
            this.Gloss = definition.Gloss;
            this.GlossTextureAdd = definition.GlossTextureAdd;
            this.SpecularColorFactor = definition.SpecularColorFactor;
        }
    }
}


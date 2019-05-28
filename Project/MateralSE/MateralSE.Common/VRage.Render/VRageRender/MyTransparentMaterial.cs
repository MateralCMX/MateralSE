namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyTransparentMaterial
    {
        public readonly MyStringId Id;
        public readonly MyTransparentMaterialTextureType TextureType;
        public readonly string Texture;
        public readonly string GlossTexture;
        public readonly bool CanBeAffectedByOtherLights;
        public readonly bool AlphaMistingEnable;
        public readonly bool UseAtlas;
        public readonly float AlphaMistingStart;
        public readonly float AlphaMistingEnd;
        public readonly float SoftParticleDistanceScale;
        public readonly float AlphaSaturation;
        public readonly bool AlphaCutout;
        public Vector2 UVOffset;
        public Vector2 UVSize;
        public readonly Vector2I TargetSize;
        public Vector4 Color;
        public Vector4 ColorAdd;
        public Vector4 ShadowMultiplier;
        public Vector4 LightMultiplier;
        public readonly float Reflectivity;
        public readonly float Fresnel;
        public readonly float ReflectionShadow;
        public readonly float Gloss;
        public readonly float GlossTextureAdd;
        public readonly float SpecularColorFactor;
        public readonly bool IsFlareOccluder;

        public MyTransparentMaterial(MyStringId id, MyTransparentMaterialTextureType textureType, string texture, string glossTexture, float softParticleDistanceScale, bool canBeAffectedByOtherLights, bool alphaMistingEnable, Vector4 color, Vector4 colorAdd, Vector4 shadowMultiplier, Vector4 lightMultiplier, bool isFlareOccluder, bool useAtlas = false, float alphaMistingStart = 1f, float alphaMistingEnd = 4f, float alphaSaturation = 1f, float reflectivity = 0f, bool alphaCutout = false, Vector2I? targetSize = new Vector2I?(), float fresnel = 1f, float reflectionShadow = 0.1f, float gloss = 0.4f, float glossTextureAdd = 0.55f, float specularColorFactor = 20f)
        {
            this.Id = id;
            this.TextureType = textureType;
            this.Texture = texture;
            this.GlossTexture = glossTexture;
            this.SoftParticleDistanceScale = softParticleDistanceScale;
            this.CanBeAffectedByOtherLights = canBeAffectedByOtherLights;
            this.AlphaMistingEnable = alphaMistingEnable;
            this.UseAtlas = useAtlas;
            this.AlphaMistingStart = alphaMistingStart;
            this.AlphaMistingEnd = alphaMistingEnd;
            this.AlphaSaturation = alphaSaturation;
            this.AlphaCutout = alphaCutout;
            this.Color = color.ToLinearRGB().PremultiplyColor();
            this.ColorAdd = colorAdd.ToLinearRGB();
            this.ShadowMultiplier = shadowMultiplier;
            this.LightMultiplier = lightMultiplier;
            this.IsFlareOccluder = isFlareOccluder;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;
            this.ReflectionShadow = reflectionShadow;
            this.Gloss = gloss;
            this.GlossTextureAdd = glossTextureAdd;
            this.SpecularColorFactor = specularColorFactor;
            Vector2I? nullable = targetSize;
            this.TargetSize = (nullable != null) ? nullable.GetValueOrDefault() : new Vector2I(-1, -1);
            this.UVOffset = new Vector2(0f, 0f);
            this.UVSize = new Vector2(1f, 1f);
        }
    }
}


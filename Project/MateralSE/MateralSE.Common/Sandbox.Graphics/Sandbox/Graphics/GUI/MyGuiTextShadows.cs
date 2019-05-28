namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public static class MyGuiTextShadows
    {
        public const string TEXT_SHADOW_DEFAULT = "Default";
        private static Dictionary<string, ShadowTextureSet> m_textureSets = new Dictionary<string, ShadowTextureSet>();

        public static void AddTextureSet(string name, IEnumerable<ShadowTexture> textures)
        {
            ShadowTextureSet set = new ShadowTextureSet();
            set.AddTextures(textures);
            m_textureSets[name] = set;
        }

        private static unsafe Vector2 AdjustPosition(Vector2 position, ref Vector2 textSize, ref Vector2 shadowSize, MyGuiDrawAlignEnum alignment)
        {
            if (alignment == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                float num = shadowSize.X - textSize.X;
                float num2 = shadowSize.Y - textSize.Y;
                float* singlePtr1 = (float*) ref position.X;
                singlePtr1[0] -= num / 2f;
                float* singlePtr2 = (float*) ref position.Y;
                singlePtr2[0] -= num2 / 2f;
            }
            else if ((alignment != MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) && (alignment == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP))
            {
                float num3 = shadowSize.X - textSize.X;
                float num4 = shadowSize.Y - textSize.Y;
                float* singlePtr3 = (float*) ref position.X;
                singlePtr3[0] += num3 / 2f;
                float* singlePtr4 = (float*) ref position.Y;
                singlePtr4[0] -= num4 / 2f;
            }
            return position;
        }

        public static void ClearShadowTextures()
        {
            m_textureSets.Clear();
        }

        public static void DrawShadow(ref Vector2 position, ref Vector2 textSize, string textureSet = null, float fogAlphaMultiplier = 1f, MyGuiDrawAlignEnum alignment = 4)
        {
            ShadowTexture texture;
            if (textureSet == null)
            {
                textureSet = "Default";
            }
            Vector2 shadowSize = GetShadowSize(ref textSize, textureSet, out texture);
            Vector2 normalizedCoord = AdjustPosition(position, ref textSize, ref shadowSize, alignment);
            Color color = new Color(0, 0, 0, (byte) ((255f * texture.DefaultAlpha) * fogAlphaMultiplier));
            MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, shadowSize, color, alignment, false, true);
        }

        public static Vector2 GetShadowSize(ref Vector2 size, string textureSet = null)
        {
            ShadowTexture texture;
            if (textureSet == null)
            {
                textureSet = "Default";
            }
            return GetShadowSize(ref size, textureSet, out texture);
        }

        private static unsafe Vector2 GetShadowSize(ref Vector2 size, string textureSet, out ShadowTexture texture)
        {
            ShadowTextureSet set;
            Vector2 vector = size;
            if (!m_textureSets.TryGetValue(textureSet, out set) && !m_textureSets.TryGetValue("Default", out set))
            {
                throw new Exception("Missing Default shadow texture. Check ShadowTextureSets.sbc");
            }
            texture = set.GetOptimalTexture(size.X);
            float* singlePtr1 = (float*) ref vector.X;
            singlePtr1[0] *= texture.GrowFactorWidth;
            float* singlePtr2 = (float*) ref vector.Y;
            singlePtr2[0] *= texture.GrowFactorHeight;
            return vector;
        }
    }
}


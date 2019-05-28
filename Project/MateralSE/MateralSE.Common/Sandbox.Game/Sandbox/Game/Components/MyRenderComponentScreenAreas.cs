namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Collections;
    using VRage.Game.Definitions;
    using VRage.Game.Entity;
    using VRage.Game.GUI.TextPanel;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyRenderComponentScreenAreas : MyRenderComponentCubeBlock
    {
        private MyEntity m_entity;
        private List<PanelScreenArea> m_screenAreas = new List<PanelScreenArea>();

        public MyRenderComponentScreenAreas(MyEntity entity)
        {
            this.m_entity = entity;
        }

        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            this.UpdateRenderAreas();
        }

        public void AddScreenArea(uint[] renderObjectIDs, string materialName)
        {
            PanelScreenArea item = new PanelScreenArea();
            item.RenderObjectIDs = renderObjectIDs.ToArray<uint>();
            item.Material = materialName;
            this.m_screenAreas.Add(item);
        }

        internal static Vector2 CalcAspectFactor(Vector2I textureSize, Vector2 aspectRatio)
        {
            Vector2 vector = (textureSize.X > textureSize.Y) ? new Vector2(1f, (float) (textureSize.X / textureSize.Y)) : new Vector2((float) (textureSize.Y / textureSize.X), 1f);
            return (aspectRatio * vector);
        }

        internal static Vector2 CalcShift(Vector2I textureSize, Vector2 aspectFactor) => 
            ((Vector2) ((textureSize * (aspectFactor - Vector2.One)) / 2f));

        public void ChangeTexture(int area, string path)
        {
            if (area < this.m_screenAreas.Count)
            {
                Color? nullable;
                float? nullable2;
                if (string.IsNullOrEmpty(path))
                {
                    for (int i = 0; i < this.m_screenAreas[area].RenderObjectIDs.Length; i++)
                    {
                        if (this.m_screenAreas[area].RenderObjectIDs[i] != uint.MaxValue)
                        {
                            MyRenderProxy.ChangeMaterialTexture(this.m_screenAreas[area].RenderObjectIDs[i], this.m_screenAreas[area].Material, null, null, null, null);
                            nullable = null;
                            nullable2 = null;
                            MyRenderProxy.UpdateModelProperties(this.m_screenAreas[area].RenderObjectIDs[i], this.m_screenAreas[area].Material, 0, RenderFlags.Visible, nullable, nullable2);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < this.m_screenAreas[area].RenderObjectIDs.Length; i++)
                    {
                        if (this.m_screenAreas[area].RenderObjectIDs[i] != uint.MaxValue)
                        {
                            MyRenderProxy.ChangeMaterialTexture(this.m_screenAreas[area].RenderObjectIDs[i], this.m_screenAreas[area].Material, path, null, null, null);
                            nullable = null;
                            nullable2 = null;
                            MyRenderProxy.UpdateModelProperties(this.m_screenAreas[area].RenderObjectIDs[i], this.m_screenAreas[area].Material, RenderFlags.Visible, 0, nullable, nullable2);
                        }
                    }
                }
            }
        }

        public void CreateTexture(int area, Vector2I textureSize)
        {
            MyRenderProxy.CreateGeneratedTexture(this.GenerateOffscreenTextureName(this.m_entity.EntityId, area), textureSize.X, textureSize.Y, MyGeneratedTextureType.RGBA, 1);
        }

        public string GenerateOffscreenTextureName(long entityId, int area) => 
            $"LCDOffscreenTexture_{entityId}_{this.m_screenAreas[area].Material}";

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
        }

        public override void ReleaseRenderObjectID(int index)
        {
            base.ReleaseRenderObjectID(index);
            for (int i = 0; i < this.m_screenAreas.Count; i++)
            {
                this.m_screenAreas[i].RenderObjectIDs[index] = uint.MaxValue;
            }
        }

        public void ReleaseTexture(int area)
        {
            MyRenderProxy.UnloadTexture(this.GenerateOffscreenTextureName(this.m_entity.EntityId, area));
        }

        public void RenderSpritesToTexture(int area, ListReader<MySprite> sprites, Vector2I textureSize, Vector2 aspectRatio, Color backgroundColor, byte backgroundAlpha)
        {
            string targetTexture = this.GenerateOffscreenTextureName(this.m_entity.EntityId, area);
            Vector2 aspectFactor = CalcAspectFactor(textureSize, aspectRatio);
            Vector2 vector2 = CalcShift(textureSize, aspectFactor);
            for (int i = 0; i < sprites.Count; i++)
            {
                MySprite sprite = sprites[i];
                Vector2? size = sprite.Size;
                Vector2 vector3 = (size != null) ? size.GetValueOrDefault() : ((Vector2) textureSize);
                size = sprite.Position;
                Vector2 screenCoord = (size != null) ? size.GetValueOrDefault() : ((Vector2) (textureSize / 2));
                Color? nullable2 = sprite.Color;
                Color colorMask = (nullable2 != null) ? nullable2.GetValueOrDefault() : Color.White;
                screenCoord += vector2;
                SpriteType type = sprite.Type;
                if (type != SpriteType.TEXTURE)
                {
                    if (type == SpriteType.TEXT)
                    {
                        switch (sprite.Alignment)
                        {
                            case TextAlignment.RIGHT:
                                screenCoord -= new Vector2(vector3.X, 0f);
                                break;

                            case TextAlignment.CENTER:
                                screenCoord -= new Vector2(vector3.X * 0.5f, 0f);
                                break;

                            default:
                                break;
                        }
                        MyFontDefinition definition = MyDefinitionManager.Static.GetDefinition<MyFontDefinition>(MyStringHash.GetOrCompute(sprite.FontId));
                        int textureWidthinPx = (int) Math.Round((double) vector3.X);
                        MyRenderProxy.DrawStringAligned((definition != null) ? ((int) definition.Id.SubtypeId) : ((int) MyStringHash.GetOrCompute("Debug")), screenCoord, colorMask, sprite.Data ?? string.Empty, sprite.RotationOrScale, float.PositiveInfinity, targetTexture, textureWidthinPx, (MyRenderTextAlignmentEnum) sprite.Alignment);
                    }
                }
                else
                {
                    MyLCDTextureDefinition definition = MyDefinitionManager.Static.GetDefinition<MyLCDTextureDefinition>(MyStringHash.GetOrCompute(sprite.Data));
                    if (definition != null)
                    {
                        switch (sprite.Alignment)
                        {
                            case TextAlignment.LEFT:
                                screenCoord += new Vector2(vector3.X * 0.5f, 0f);
                                break;

                            case TextAlignment.RIGHT:
                                screenCoord -= new Vector2(vector3.X * 0.5f, 0f);
                                break;

                            default:
                                break;
                        }
                        Vector2 rightVector = new Vector2(1f, 0f);
                        if (Math.Abs(sprite.RotationOrScale) > 1E-05f)
                        {
                            rightVector = new Vector2((float) Math.Cos((double) sprite.RotationOrScale), (float) Math.Sin((double) sprite.RotationOrScale));
                        }
                        MyRenderProxy.DrawSpriteAtlas(definition.SpritePath ?? definition.TexturePath, screenCoord, Vector2.Zero, Vector2.One, rightVector, Vector2.One, colorMask, vector3 / 2f, targetTexture);
                    }
                }
            }
            backgroundColor.A = backgroundAlpha;
            uint[] renderObjectIDs = this.m_screenAreas[area].RenderObjectIDs;
            int index = 0;
            while ((index < renderObjectIDs.Length) && (renderObjectIDs[index] == uint.MaxValue))
            {
                index++;
            }
            if (index < renderObjectIDs.Length)
            {
                MyRenderProxy.RenderOffscreenTexture(targetTexture, new Vector2?(aspectFactor), new Color?(backgroundColor));
            }
        }

        public void UpdateModelProperties()
        {
            int num = 0;
            while (num < this.m_screenAreas.Count)
            {
                int index = 0;
                while (true)
                {
                    if (index >= this.m_screenAreas[num].RenderObjectIDs.Length)
                    {
                        num++;
                        break;
                    }
                    if (this.m_screenAreas[num].RenderObjectIDs[index] != uint.MaxValue)
                    {
                        Color? diffuseColor = null;
                        float? emissivity = null;
                        MyRenderProxy.UpdateModelProperties(this.m_screenAreas[num].RenderObjectIDs[index], this.m_screenAreas[num].Material, 0, 0, diffuseColor, emissivity);
                    }
                    index++;
                }
            }
        }

        protected void UpdateRenderAreas()
        {
            int index = 0;
            while (index < base.RenderObjectIDs.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_screenAreas.Count)
                    {
                        index++;
                        break;
                    }
                    this.m_screenAreas[num2].RenderObjectIDs[index] = base.RenderObjectIDs[index];
                    num2++;
                }
            }
        }

        private class PanelScreenArea
        {
            public uint[] RenderObjectIDs;
            public string Material;
        }
    }
}


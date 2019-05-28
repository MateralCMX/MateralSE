namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions.GUI;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Gui;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyHudCrosshair
    {
        private Vector2 m_rightVector;
        private Vector2 m_position;
        private List<SpriteInfo> m_sprites = new List<SpriteInfo>();
        private int m_lastGameplayTimeInMs = 0;
        protected MyObjectBuilder_CrosshairStyle m_style;
        protected MyStatControls m_statControls;
        private static MyStringId m_defaultSpriteId = MyStringId.GetOrCompute("Default");

        public MyHudCrosshair()
        {
            this.ResetToDefault(true);
        }

        public unsafe void AddTemporarySprite(MyHudTexturesEnum spriteEnum, MyStringId spriteId, int timeout = 0x7d0, int fadeTime = 0x3e8, Color? color = new Color?(), float size = 0.02f)
        {
            SpriteInfo* infoPtr1;
            SpriteInfo item = new SpriteInfo();
            infoPtr1->Color = (color != null) ? color.Value : MyHudConstants.HUD_COLOR_LIGHT;
            infoPtr1 = (SpriteInfo*) ref item;
            item.FadeoutTime = fadeTime;
            item.HalfSize = Vector2.One * size;
            item.SpriteId = spriteId;
            item.SpriteEnum = spriteEnum;
            item.TimeRemaining = timeout;
            item.Visible = true;
            for (int i = 0; i < this.m_sprites.Count; i++)
            {
                if (this.m_sprites[i].SpriteId == spriteId)
                {
                    this.m_sprites[i] = item;
                    return;
                }
            }
            this.m_sprites.Add(item);
        }

        public void ChangeDefaultSprite(MyHudTexturesEnum newSprite, float size = 0f)
        {
            for (int i = 0; i < this.m_sprites.Count; i++)
            {
                SpriteInfo info = this.m_sprites[i];
                if (info.SpriteId == m_defaultSpriteId)
                {
                    if (size != 0f)
                    {
                        info.HalfSize = Vector2.One * size;
                    }
                    info.SpriteEnum = newSprite;
                    this.m_sprites[i] = info;
                }
            }
        }

        public void ChangePosition(Vector2 newPosition)
        {
            this.m_position = newPosition;
        }

        private void CreateDefaultSprite()
        {
            SpriteInfo item = new SpriteInfo {
                Color = MyHudConstants.HUD_COLOR_LIGHT,
                FadeoutTime = 0,
                HalfSize = Vector2.One * 0.02f,
                SpriteId = m_defaultSpriteId,
                SpriteEnum = MyHudTexturesEnum.crosshair,
                TimeRemaining = 0,
                Visible = true
            };
            bool flag = false;
            int num = 0;
            while (true)
            {
                if (num < this.m_sprites.Count)
                {
                    if (!(this.m_sprites[num].SpriteId == m_defaultSpriteId))
                    {
                        num++;
                        continue;
                    }
                    this.m_sprites[num] = item;
                    flag = true;
                }
                if (!flag)
                {
                    this.m_sprites.Add(item);
                }
                return;
            }
        }

        public unsafe void Draw(string atlas, MyAtlasTextureCoordinate[] atlasCoords)
        {
            float x = ((float) MyGuiManager.GetSafeFullscreenRectangle().Width) / MyGuiManager.GetHudSize().X;
            float y = ((float) MyGuiManager.GetSafeFullscreenRectangle().Height) / MyGuiManager.GetHudSize().Y;
            Vector2 position = this.m_position;
            if (MyVideoSettingsManager.IsTripleHead())
            {
                float* singlePtr1 = (float*) ref position.X;
                singlePtr1[0]++;
            }
            foreach (SpriteInfo info in this.m_sprites)
            {
                if (!info.Visible)
                {
                    continue;
                }
                int spriteEnum = (int) info.SpriteEnum;
                if (spriteEnum < atlasCoords.Length)
                {
                    MyAtlasTextureCoordinate coordinate = atlasCoords[spriteEnum];
                    Color color = info.Color;
                    if (info.TimeRemaining < info.FadeoutTime)
                    {
                        Color* colorPtr1 = (Color*) ref color;
                        colorPtr1.A = (byte) ((color.A * info.TimeRemaining) / info.FadeoutTime);
                    }
                    MyRenderProxy.DrawSpriteAtlas(atlas, position, coordinate.Offset, coordinate.Size, this.m_rightVector, new Vector2(x, y), color, info.HalfSize, null);
                }
            }
            if ((this.m_statControls != null) && (this.m_style != null))
            {
                Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
                Vector2 size = new Vector2((float) fullscreenRectangle.Width, (float) fullscreenRectangle.Height);
                Vector2 coordScreen = this.m_style.Position * size;
                this.m_statControls.Position = (((this.Position - ScreenCenter) / MyGuiManager.GetHudSize()) * size) + MyUtils.AlignCoord(coordScreen, size, this.m_style.OriginAlign);
                this.m_statControls.Draw(1f, 1f);
            }
        }

        public static bool GetProjectedTarget(Vector3D from, Vector3D to, ref Vector2 target)
        {
            Vector3D zero = Vector3D.Zero;
            return (GetTarget(from, to, ref zero) && GetProjectedVector(zero, ref target));
        }

        public static unsafe bool GetProjectedVector(Vector3D worldPosition, ref Vector2 target)
        {
            Vector3D vectord1 = Vector3D.Transform(worldPosition, MySector.MainCamera.ViewMatrix);
            Vector4 vector = Vector4.Transform((Vector3) vectord1, (Matrix) MySector.MainCamera.ProjectionMatrix);
            if (vectord1.Z > 0.0)
            {
                return false;
            }
            if (vector.W == 0f)
            {
                return false;
            }
            target = new Vector2(((vector.X / vector.W) / 2f) + 0.5f, ((-vector.Y / vector.W) / 2f) + 0.5f);
            if (MyVideoSettingsManager.IsTripleHead())
            {
                target.X = (target.X - 0.3333333f) / 0.3333333f;
            }
            float* singlePtr1 = (float*) ref target.Y;
            singlePtr1[0] *= MyGuiManager.GetHudSize().Y;
            return true;
        }

        public static bool GetTarget(Vector3D from, Vector3D to, ref Vector3D target)
        {
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(from, to, 15);
            if (nullable != null)
            {
                target = nullable.Value.Position;
            }
            return (nullable != null);
        }

        public void HideDefaultSprite()
        {
            for (int i = 0; i < this.m_sprites.Count; i++)
            {
                SpriteInfo info = this.m_sprites[i];
                if (info.SpriteId == m_defaultSpriteId)
                {
                    info.Visible = false;
                    this.m_sprites[i] = info;
                }
            }
        }

        private void InitStatControls()
        {
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            Vector2 size = new Vector2((float) fullscreenRectangle.Width, (float) fullscreenRectangle.Height);
            float uiScale = MyGuiManager.GetSafeScreenScale() * MyHud.HudElementsScaleMultiplier;
            this.m_statControls = new MyStatControls(this.m_style, uiScale);
            Vector2 coordScreen = this.m_style.Position * size;
            this.m_statControls.Position = (((this.Position - ScreenCenter) / MyGuiManager.GetHudSize()) * size) + MyUtils.AlignCoord(coordScreen, size, this.m_style.OriginAlign);
        }

        public void Recenter()
        {
            this.m_position = ScreenCenter;
        }

        public void RecreateControls(bool constructor)
        {
            MyHudDefinition hudDefinition = MyHud.HudDefinition;
            this.m_style = hudDefinition.Crosshair;
            if (this.m_style != null)
            {
                this.InitStatControls();
            }
        }

        public void ResetToDefault(bool clear = true)
        {
            this.SetDefaults(clear);
        }

        private void SetDefaults(bool clear)
        {
            if (clear)
            {
                this.m_sprites.Clear();
            }
            this.CreateDefaultSprite();
            this.m_rightVector = Vector2.UnitX;
        }

        public unsafe void Update()
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.m_lastGameplayTimeInMs == 0)
            {
                this.m_lastGameplayTimeInMs = totalGamePlayTimeInMilliseconds;
            }
            else
            {
                int num2 = totalGamePlayTimeInMilliseconds - this.m_lastGameplayTimeInMs;
                this.m_lastGameplayTimeInMs = totalGamePlayTimeInMilliseconds;
                for (int i = 0; i < this.m_sprites.Count; i++)
                {
                    SpriteInfo info = this.m_sprites[i];
                    if (info.SpriteId != m_defaultSpriteId)
                    {
                        int* numPtr1 = (int*) ref info.TimeRemaining;
                        numPtr1[0] -= num2;
                        if (info.TimeRemaining > 0)
                        {
                            this.m_sprites[i] = info;
                        }
                        else
                        {
                            this.m_sprites.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        public Vector2 Position =>
            this.m_position;

        public static Vector2 ScreenCenter =>
            new Vector2(0.5f, MyGuiManager.GetHudSizeHalf().Y);

        public bool Visible { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct SpriteInfo
        {
            public MyHudTexturesEnum SpriteEnum;
            public VRageMath.Color Color;
            public Vector2 HalfSize;
            public MyStringId SpriteId;
            public int FadeoutTime;
            public int TimeRemaining;
            public bool Visible;
        }
    }
}


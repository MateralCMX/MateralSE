namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public abstract class MyTSSCommon : MyTextSurfaceScriptBase
    {
        protected string m_fontId;
        protected float m_fontScale;

        protected MyTSSCommon(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_fontId = "Monospace";
            this.m_fontScale = 1f;
        }

        protected unsafe MySpriteDrawFrame AddBackground(MySpriteDrawFrame frame, Color? color = new Color?())
        {
            Vector2? nullable3;
            MySprite* spritePtr1;
            Vector2? nullable1;
            Vector2? nullable4;
            MySprite sprite = MyTextSurfaceHelper.DEFAULT_BACKGROUND;
            Color? nullable = color;
            spritePtr1->Color = new Color?((nullable != null) ? nullable.GetValueOrDefault() : Color.White);
            spritePtr1 = (MySprite*) ref sprite;
            Vector2?* nullablePtr1 = (Vector2?*) ref sprite.Position;
            Vector2? nullable2 = nullablePtr1[0];
            Vector2 vector = base.m_surface.TextureSize / 2f;
            if (nullable2 != null)
            {
                nullable1 = new Vector2?(nullable2.GetValueOrDefault() + vector);
            }
            else
            {
                nullable3 = null;
                nullable1 = nullable3;
            }
            nullablePtr1[0] = nullable1;
            frame.Add(sprite);
            Vector2?* nullablePtr2 = (Vector2?*) ref sprite.Position;
            nullable2 = nullablePtr2[0];
            vector = MyTextSurfaceHelper.BACKGROUND_SHIFT;
            if (nullable2 != null)
            {
                nullable4 = new Vector2?(nullable2.GetValueOrDefault() + vector);
            }
            else
            {
                nullable3 = null;
                nullable4 = nullable3;
            }
            nullablePtr2[0] = nullable4;
            frame.Add(sprite);
            return frame;
        }

        protected MySpriteDrawFrame AddBrackets(MySpriteDrawFrame frame, Vector2 size, float scale)
        {
            Vector2? position = null;
            position = null;
            MySprite sprite = new MySprite(SpriteType.TEXTURE, "DecorativeBracketLeft", position, position, new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, 0f) {
                Position = new Vector2(size.X * scale, base.m_halfSize.Y),
                Size = new Vector2?(size * scale)
            };
            frame.Add(sprite);
            position = null;
            position = null;
            sprite = new MySprite(SpriteType.TEXTURE, "DecorativeBracketRight", position, position, new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, 0f) {
                Position = new Vector2(base.m_size.X - (size.X * scale), base.m_halfSize.Y),
                Size = new Vector2?(size * scale)
            };
            frame.Add(sprite);
            return frame;
        }

        protected MySpriteDrawFrame AddLine(MySpriteDrawFrame frame, Vector2 startPos, Vector2 endPos, Color color, float thicknessPx)
        {
            Vector2 vector = endPos - startPos;
            Vector2 size = new Vector2(thicknessPx, vector.Length());
            MySprite sprite = MySprite.CreateSprite("SquareTapered", startPos + (vector * 0.5f), size);
            sprite.Color = new Color?(color);
            sprite.RotationOrScale = -((float) Math.Acos((double) Vector2.Dot(Vector2.Normalize(vector), Vector2.UnitX)));
            frame.Add(sprite);
            return frame;
        }

        protected unsafe MySpriteDrawFrame AddProgressBar(MySpriteDrawFrame frame, Vector2 position, Vector2 size, float ratio, Color barBgColor, Color barFgColor, string barBgSprite = null, string barFgSprite = null)
        {
            MySprite sprite3;
            Vector2? nullable = null;
            nullable = null;
            MySprite* spritePtr1 = (MySprite*) new MySprite(SpriteType.TEXTURE, barBgSprite ?? "SquareSimple", nullable, nullable, new Color?(barBgColor), null, TextAlignment.CENTER, 0f);
            spritePtr1 = (MySprite*) ref sprite3;
            sprite3.Alignment = TextAlignment.LEFT;
            sprite3.Position = new Vector2?(position - new Vector2(size.X * 0.5f, 0f));
            sprite3.Size = new Vector2?(size);
            MySprite sprite = sprite3;
            frame.Add(sprite);
            nullable = null;
            nullable = null;
            MySprite* spritePtr2 = (MySprite*) new MySprite(SpriteType.TEXTURE, barFgSprite ?? "SquareSimple", nullable, nullable, new Color?(barFgColor), null, TextAlignment.CENTER, 0f);
            spritePtr2 = (MySprite*) ref sprite3;
            sprite3.Alignment = TextAlignment.LEFT;
            sprite3.Position = new Vector2?(position - new Vector2(size.X * 0.5f, 0f));
            sprite3.Size = new Vector2(size.X * ratio, size.Y);
            MySprite sprite2 = sprite3;
            frame.Add(sprite2);
            return frame;
        }

        protected MySpriteDrawFrame AddTextBox(MySpriteDrawFrame frame, Vector2 position, Vector2 size, string text, string font, float scale, Color bgColor, Color textColor, string bgSprite = null, float textOffset = 0f)
        {
            Vector2 vector = position + new Vector2(size.X * 0.5f, 0f);
            if (!string.IsNullOrEmpty(bgSprite))
            {
                MySprite sprite2 = MySprite.CreateSprite(bgSprite, vector, size);
                sprite2.Color = new Color?(bgColor);
                sprite2.Alignment = TextAlignment.RIGHT;
                frame.Add(sprite2);
            }
            MySprite sprite = MySprite.CreateText(text, font, textColor, scale, TextAlignment.RIGHT);
            sprite.Position = new Vector2?(vector + new Vector2(-textOffset, -size.Y * 0.5f));
            sprite.Size = new Vector2?(size);
            frame.Add(sprite);
            return frame;
        }

        public override void Run()
        {
            base.m_backgroundColor = base.m_surface.ScriptBackgroundColor;
            base.m_foregroundColor = base.m_surface.ScriptForegroundColor;
        }
    }
}


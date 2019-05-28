namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics;
    using Sandbox.ModAPI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI;
    using VRageMath;

    [MyTextSurfaceScript("TSS_Gravity", "DisplayName_TSS_Gravity")]
    public class MyTSSGravity : MyTSSCommon
    {
        public static float ASPECT_RATIO = 3f;
        public static float DECORATION_RATIO = 0.25f;
        public static float TEXT_RATIO = 0.25f;
        private Vector2 m_innerSize;
        private Vector2 m_decorationSize;
        private float m_firstLine;
        private float m_secondLine;
        private StringBuilder m_sb;

        public MyTSSGravity(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_sb = new StringBuilder();
            this.m_innerSize = new Vector2(ASPECT_RATIO, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_decorationSize = new Vector2(0.012f * this.m_innerSize.X, DECORATION_RATIO * this.m_innerSize.Y);
            this.m_sb.Clear();
            this.m_sb.Append(MyTexts.Get(MySpaceTexts.AGravity));
            this.m_sb.Append(": 00.00g");
            Vector2 vector = MyGuiManager.MeasureStringRaw("Monospace", this.m_sb, 1f);
            float num = (TEXT_RATIO * this.m_innerSize.Y) / vector.Y;
            base.m_fontScale = Math.Min((this.m_innerSize.X * 0.72f) / vector.X, num);
            this.m_firstLine = base.m_halfSize.Y - (this.m_decorationSize.Y * 0.55f);
            this.m_secondLine = base.m_halfSize.Y + (this.m_decorationSize.Y * 0.55f);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
                if (base.m_block != null)
                {
                    Vector3D position = base.m_block.GetPosition();
                    float num = MyGravityProviderSystem.CalculateArtificialGravityInPoint(position, MyGravityProviderSystem.CalculateArtificialGravityStrengthMultiplier(MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(position))).Length() / 9.81f;
                    this.m_sb.Clear();
                    this.m_sb.Append(MyTexts.Get(MySpaceTexts.AGravity));
                    this.m_sb.Append($": {num:F2}g");
                    Vector2 vector2 = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, base.m_fontScale);
                    MySprite sprite = new MySprite {
                        Position = new Vector2(base.m_halfSize.X, this.m_firstLine - (vector2.Y * 0.5f)),
                        Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                        Type = SpriteType.TEXT,
                        FontId = base.m_fontId,
                        Alignment = TextAlignment.CENTER,
                        Color = new Color?(base.m_foregroundColor),
                        RotationOrScale = base.m_fontScale,
                        Data = this.m_sb.ToString()
                    };
                    frame.Add(sprite);
                    num = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position).Length() / 9.81f;
                    this.m_sb.Clear();
                    this.m_sb.Append(MyTexts.Get(MySpaceTexts.PGravity));
                    this.m_sb.Append($": {num:F2}g");
                    vector2 = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, base.m_fontScale);
                    sprite = new MySprite {
                        Position = new Vector2(base.m_halfSize.X, this.m_secondLine - (vector2.Y * 0.5f)),
                        Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                        Type = SpriteType.TEXT,
                        FontId = base.m_fontId,
                        Alignment = TextAlignment.CENTER,
                        Color = new Color?(base.m_foregroundColor),
                        RotationOrScale = base.m_fontScale,
                        Data = this.m_sb.ToString()
                    };
                    frame.Add(sprite);
                    float scale = (this.m_innerSize.Y / 256f) * 0.9f;
                    base.AddBrackets(frame, new Vector2(64f, 256f), scale);
                }
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update10;
    }
}


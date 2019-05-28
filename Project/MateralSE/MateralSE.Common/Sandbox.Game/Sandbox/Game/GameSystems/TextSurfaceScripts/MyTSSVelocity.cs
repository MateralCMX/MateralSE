namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Graphics;
    using Sandbox.ModAPI;
    using System;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI;
    using VRageMath;

    [MyTextSurfaceScript("TSS_Velocity", "DisplayName_TSS_Velocity")]
    public class MyTSSVelocity : MyTSSCommon
    {
        public static float ASPECT_RATIO = 3f;
        public static float DECORATION_RATIO = 0.25f;
        public static float TEXT_RATIO = 0.25f;
        private Vector2 m_innerSize;
        private Vector2 m_decorationSize;
        private float m_firstLine;
        private float m_secondLine;
        private StringBuilder m_sb;
        private MyCubeGrid m_grid;

        public MyTSSVelocity(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_sb = new StringBuilder();
            this.m_innerSize = new Vector2(ASPECT_RATIO, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_decorationSize = new Vector2(0.012f * this.m_innerSize.X, DECORATION_RATIO * this.m_innerSize.Y);
            this.m_sb.Clear();
            this.m_sb.Append("M");
            Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, 1f);
            base.m_fontScale = (TEXT_RATIO * this.m_innerSize.Y) / vector.Y;
            this.m_firstLine = base.m_halfSize.Y - (this.m_decorationSize.Y * 0.55f);
            this.m_secondLine = base.m_halfSize.Y + (this.m_decorationSize.Y * 0.55f);
            if (base.m_block != null)
            {
                this.m_grid = base.m_block.CubeGrid as MyCubeGrid;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.m_grid = null;
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
                if ((this.m_grid != null) && (this.m_grid.Physics != null))
                {
                    Color barBgColor = new Color(base.m_foregroundColor, 0.1f);
                    float num = this.m_grid.Physics.LinearVelocity.Length();
                    float ratio = num / Math.Max(MyGridPhysics.ShipMaxLinearVelocity(), 1f);
                    string str = $"{num:F2} m/s";
                    Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, new StringBuilder(str), base.m_fontScale);
                    MySprite sprite = new MySprite {
                        Position = new Vector2(base.m_halfSize.X, this.m_firstLine - (vector.Y * 0.5f)),
                        Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                        Type = SpriteType.TEXT,
                        FontId = base.m_fontId,
                        Alignment = TextAlignment.CENTER,
                        Color = new Color?(base.m_foregroundColor),
                        RotationOrScale = base.m_fontScale,
                        Data = str
                    };
                    frame.Add(sprite);
                    this.m_sb.Clear();
                    this.m_sb.Append("[");
                    Vector2 vector2 = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, 1f);
                    float scale = this.m_decorationSize.Y / vector2.Y;
                    float x = this.m_innerSize.X * 0.6f;
                    base.AddProgressBar(frame, new Vector2(base.m_halfSize.X, this.m_secondLine), new Vector2(x, MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, scale).Y * 0.4f), ratio, barBgColor, base.m_foregroundColor, null, null);
                    float num6 = (this.m_innerSize.Y / 256f) * 0.9f;
                    base.AddBrackets(frame, new Vector2(64f, 256f), num6);
                }
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update10;
    }
}


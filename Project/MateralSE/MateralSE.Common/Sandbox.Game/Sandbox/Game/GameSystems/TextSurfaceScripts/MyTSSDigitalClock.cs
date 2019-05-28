namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.Graphics;
    using Sandbox.ModAPI;
    using System;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI;
    using VRageMath;

    [MyTextSurfaceScript("TSS_ClockDigital", "DisplayName_TSS_ClockDigital")]
    public class MyTSSDigitalClock : MyTSSCommon
    {
        public static float ASPECT_RATIO = 2.5f;
        public static float DECORATION_RATIO = 0.25f;
        public static float TEXT_RATIO = 0.25f;
        private Vector2 m_innerSize;
        private Vector2 m_decorationSize;
        private StringBuilder m_sb;

        public MyTSSDigitalClock(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_sb = new StringBuilder();
            this.m_innerSize = new Vector2(ASPECT_RATIO, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_decorationSize = new Vector2(0.012f * this.m_innerSize.X, DECORATION_RATIO * this.m_innerSize.Y);
            this.m_sb.Clear();
            this.m_sb.Append("M");
            Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, this.m_sb, 1f);
            base.m_fontScale = (TEXT_RATIO * this.m_innerSize.Y) / vector.Y;
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
                string str = DateTime.Now.ToLocalTime().ToString("HH:mm:ss");
                Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, new StringBuilder(str), base.m_fontScale);
                MySprite sprite = new MySprite {
                    Position = new Vector2(base.m_halfSize.X, base.m_halfSize.Y - (vector.Y * 0.5f)),
                    Size = new Vector2(this.m_innerSize.X, this.m_innerSize.Y),
                    Type = SpriteType.TEXT,
                    FontId = base.m_fontId,
                    Alignment = TextAlignment.CENTER,
                    Color = new Color?(base.m_foregroundColor),
                    RotationOrScale = base.m_fontScale,
                    Data = str
                };
                frame.Add(sprite);
                float scale = (this.m_innerSize.Y / 256f) * 0.9f;
                base.AddBrackets(frame, new Vector2(64f, 256f), scale);
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update10;
    }
}


namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    [MyTextSurfaceScript("TSS_ClockAnalog", "DisplayName_TSS_ClockAnalog")]
    public class MyTSSAnalogClock : MyTSSCommon
    {
        public static float ASPECT_RATIO = 1.85f;
        public static float DECORATION_RATIO = 0.25f;
        public static readonly float INDICATOR_WIDTH = 0.012f;
        private static Vector2 HOURS_SIZE = new Vector2(0.32f, INDICATOR_WIDTH);
        private static Vector2 MINUTES_SIZE = new Vector2(0.42f, INDICATOR_WIDTH);
        private static Vector2 INDICATORS_SIZE = new Vector2(0.06f, INDICATOR_WIDTH);
        private Vector2 m_innerSize;
        private Vector2 m_clockSize;
        private Vector2 m_decorationSize;
        private Vector2 m_sizeModifier;
        private StringBuilder m_sb;

        public MyTSSAnalogClock(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.m_sb = new StringBuilder();
            this.m_innerSize = new Vector2(ASPECT_RATIO, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_clockSize = (this.m_innerSize.X > this.m_innerSize.Y) ? new Vector2(this.m_innerSize.Y) : new Vector2(this.m_innerSize.X);
            this.m_decorationSize = new Vector2(INDICATOR_WIDTH * this.m_innerSize.X, DECORATION_RATIO * this.m_innerSize.Y);
            this.m_sizeModifier = new Vector2(1f, 512f / this.m_clockSize.X);
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
                float rotation = 0f;
                float y = 0f;
                Vector2 zero = Vector2.Zero;
                Vector2 vector2 = new Vector2(INDICATORS_SIZE.X * 0.5f, 0f);
                Color foregroundColor = base.m_foregroundColor;
                int num4 = 0;
                while (true)
                {
                    Color? nullable;
                    if (num4 >= 12)
                    {
                        DateTime time = DateTime.Now.ToLocalTime();
                        rotation = MathHelper.ToRadians((float) ((30 * time.Hour) - 90));
                        zero = ((new Vector2((float) Math.Cos((double) rotation), (float) Math.Sin((double) rotation)) * this.m_clockSize) * 0.3f) * HOURS_SIZE.X;
                        nullable = new Color?(foregroundColor);
                        frame.Add(new MySprite(SpriteType.TEXTURE, "SquareTapered", new Vector2?(base.m_halfSize + zero), new Vector2?((HOURS_SIZE * this.m_clockSize) * this.m_sizeModifier), nullable, null, TextAlignment.CENTER, rotation));
                        rotation = MathHelper.ToRadians((float) ((6 * time.Minute) - 90));
                        zero = ((new Vector2((float) Math.Cos((double) rotation), (float) Math.Sin((double) rotation)) * this.m_clockSize) * 0.3f) * MINUTES_SIZE.X;
                        nullable = new Color?(foregroundColor);
                        frame.Add(new MySprite(SpriteType.TEXTURE, "SquareTapered", new Vector2?(base.m_halfSize + zero), new Vector2?((MINUTES_SIZE * this.m_clockSize) * this.m_sizeModifier), nullable, null, TextAlignment.CENTER, rotation));
                        float scale = (this.m_clockSize.Y / 256f) * 0.9f;
                        base.AddBrackets(frame, new Vector2(64f, 256f), scale);
                        break;
                    }
                    rotation = MathHelper.ToRadians((float) (30 * num4));
                    y = (float) Math.Sin((double) rotation);
                    zero = ((new Vector2((float) Math.Cos((double) rotation), y) * this.m_clockSize) * 0.4f) - vector2;
                    nullable = new Color?(foregroundColor);
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareTapered", new Vector2?(base.m_halfSize + zero), new Vector2?((INDICATORS_SIZE * this.m_clockSize) * this.m_sizeModifier), nullable, null, TextAlignment.CENTER, rotation));
                    num4++;
                }
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update1000;
    }
}

